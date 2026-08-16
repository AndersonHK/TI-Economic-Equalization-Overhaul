"""Generate the complete ship-hull appearance, volume, slot, and image report."""

import argparse
import csv
import hashlib
import importlib.util
import json
import math
import os
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont


SCRIPT_DIR = Path(__file__).resolve().parent
REPOSITORY_ROOT = SCRIPT_DIR.parents[1]
DEFAULT_GAME_INSTALL = Path(
    os.environ.get(
        "TI_GAME_INSTALL_DIR",
        r"D:\Games\SteamLibrary\steamapps\common\Terra Invicta",
    )
)
DEFAULT_OUTPUT_ROOT = REPOSITORY_ROOT / "docs" / "ship-balance-research"
REPORT_NAME = "hull-utility-slot-volume-report.md"
CSV_NAME = "hull-variant-volume-and-slots.csv"
JSON_NAME = "hull-variant-volume-and-slots.json"
IMAGE_DIRECTORY = "hull-variants"
THUMBNAIL_WIDTH = 440
THUMBNAIL_HEIGHT = 220
RENDER_SCALE = 2
MACHINERY_TOKENS = ("engine", "thruster", "reactor")


def load_measurement_module():
    spec = importlib.util.spec_from_file_location(
        "measure_ship_prefabs", SCRIPT_DIR / "measure_ship_prefabs.py"
    )
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def parse_args():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--game-install-dir",
        type=Path,
        default=DEFAULT_GAME_INSTALL,
        help="Terra Invicta installation root",
    )
    parser.add_argument(
        "--output-root",
        type=Path,
        default=DEFAULT_OUTPUT_ROOT,
        help="Directory that receives the Markdown, tables, and image directory",
    )
    return parser.parse_args()


def read_json(path):
    with path.open("r", encoding="utf-8-sig") as handle:
        return json.load(handle)


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        while True:
            block = handle.read(1024 * 1024)
            if not block:
                break
            digest.update(block)
    return digest.hexdigest().upper()


def merge_template_overrides(templates, overrides):
    override_by_name = {entry["dataName"]: entry for entry in overrides}
    merged = []
    for template in templates:
        result = dict(template)
        result.update(override_by_name.get(template["dataName"], {}))
        merged.append(result)
    unknown = sorted(set(override_by_name) - {entry["dataName"] for entry in templates})
    if unknown:
        raise RuntimeError(f"Unknown hull overrides: {', '.join(unknown)}")
    return merged


def prefab_location(measure, template_name, appearance_index, model_resource):
    if template_name in measure.PREFABS:
        ship_path = template_name.lower()
        if appearance_index == 0:
            return "base", measure.PREFABS[template_name]
        if appearance_index == 1:
            return (
                "base",
                "assets/artresources/ships/earth_alt/"
                f"{ship_path}/{ship_path}_1.prefab",
            )
        if appearance_index == 2:
            return (
                "dlc",
                "assets/artresources/ships/earth_prm/"
                f"{ship_path}/{ship_path}_2.prefab",
            )
        if appearance_index == 3:
            return (
                "dlc",
                "assets/artresources/ships/dlca/"
                f"{ship_path}/{ship_path}_3.prefab",
            )
    if template_name in measure.ALIEN_PREFABS:
        return "base", measure.ALIEN_PREFABS[template_name]
    if template_name == "STOFighter":
        return "base", "assets/artresources/earthmapmodels/jets/stofighter.prefab"
    raise RuntimeError(
        f"No prefab path rule for {template_name} appearance {appearance_index} "
        f"({model_resource})"
    )


def is_main_hull_container_path(measure, ship, path):
    if measure.is_hull_path(path):
        return True
    parts = path.lower().split("/")
    if ship == "STOFighter":
        return any("hull container" in part for part in parts[1:])
    if ship == "SalamanderGunship":
        return len(parts) > 1 and parts[1] == "salamander_gunship"
    return False


def machinery_reason(measure, path):
    leaf = path.rsplit("/", 1)[-1].lower()
    if measure.is_drive_path(path):
        return "drive subtree"
    if measure.is_reactor_bay_mesh_path(path):
        return "radiator/reactor-bay mesh"
    for token in MACHINERY_TOKENS:
        if token in leaf:
            return f"{token}-named mesh"
    return None


def select_main_hull_meshes(measure, ship, records):
    included = []
    excluded = []
    for record in records["meshes"]:
        reason = machinery_reason(measure, record["path"])
        in_hull = is_main_hull_container_path(measure, ship, record["path"])
        if in_hull and reason is None:
            included.append(record)
        elif reason is not None:
            excluded.append({"path": record["path"], "reason": reason})
    if not included:
        raise RuntimeError(f"{ship}: no main-hull meshes selected")
    return included, excluded


def combined_bounds(measure, records):
    bounds = None
    for record in records:
        bounds = measure.include_bounds(bounds, record["points"])
    return bounds


def parse_obj_geometry(mesh, cache_key, cache):
    cached = cache.get(cache_key)
    if cached is not None:
        return cached
    vertices = []
    triangles = []
    for line in mesh.export().splitlines():
        if line.startswith("v "):
            vertices.append(tuple(float(value) for value in line.split()[1:4]))
        elif line.startswith("f "):
            face = [int(token.split("/", 1)[0]) - 1 for token in line.split()[1:]]
            for index in range(1, len(face) - 1):
                triangles.append((face[0], face[index], face[index + 1]))
    result = (
        np.asarray(vertices, dtype=float),
        np.asarray(triangles, dtype=np.int32),
    )
    cache[cache_key] = result
    return result


def transformed_geometry(measure, record, root_offset, cache):
    vertices, triangles = parse_obj_geometry(
        record["mesh"], record["mesh_cache_key"], cache
    )
    if not len(vertices) or not len(triangles):
        return None
    matrix = record["matrix"].copy()
    matrix[:3, 3] -= root_offset
    return measure.transform_points(matrix, vertices), triangles


def project_triangles(geometries, horizontal_axis, vertical_axis, depth_axis):
    projected = []
    for vertices, triangles in geometries:
        for triangle in triangles:
            points = vertices[triangle]
            edge_a = points[1] - points[0]
            edge_b = points[2] - points[0]
            normal = np.cross(edge_a, edge_b)
            magnitude = np.linalg.norm(normal)
            if magnitude <= 1e-9:
                continue
            light = abs(float(normal[depth_axis] / magnitude))
            projected.append(
                (
                    float(points[:, depth_axis].mean()),
                    points[:, [horizontal_axis, vertical_axis]],
                    light,
                )
            )
    projected.sort(key=lambda item: item[0])
    return projected


def draw_projection(draw, projected, bounds_2d, box, base_color):
    low, high = bounds_2d
    span = np.maximum(high - low, 1e-9)
    left, top, right, bottom = box
    scale = min((right - left) / span[0], (bottom - top) / span[1])
    center = (low + high) / 2
    screen_center = np.array([(left + right) / 2, (top + bottom) / 2])
    for _, points, light in projected:
        screen = np.empty_like(points)
        screen[:, 0] = screen_center[0] + (points[:, 0] - center[0]) * scale
        screen[:, 1] = screen_center[1] - (points[:, 1] - center[1]) * scale
        intensity = 0.55 + 0.45 * light
        color = tuple(min(255, round(channel * intensity)) for channel in base_color)
        draw.polygon([tuple(point) for point in screen], fill=color)


def render_thumbnail(
    measure, records, root_offset, bounds, image_path, alien, geometry_cache
):
    geometries = []
    for record in records:
        geometry = transformed_geometry(measure, record, root_offset, geometry_cache)
        if geometry is not None:
            geometries.append(geometry)
    if not geometries:
        raise RuntimeError(f"No renderable triangle geometry for {image_path.name}")

    width = THUMBNAIL_WIDTH * RENDER_SCALE
    height = THUMBNAIL_HEIGHT * RENDER_SCALE
    image = Image.new("RGB", (width, height), (10, 16, 25))
    draw = ImageDraw.Draw(image)
    pad = 16 * RENDER_SCALE
    gap = 12 * RENDER_SCALE
    view_height = (height - 2 * pad - gap) / 2
    base_color = (223, 105, 138) if alien else (91, 188, 208)

    low, high = bounds
    side_bounds = np.array([[low[2], low[0]], [high[2], high[0]]])
    top_bounds = np.array([[low[2], low[1]], [high[2], high[1]]])
    side = project_triangles(geometries, 2, 0, 1)
    top_view = project_triangles(geometries, 2, 1, 0)
    draw_projection(
        draw,
        side,
        side_bounds,
        (pad, pad, width - pad, pad + view_height),
        base_color,
    )
    draw_projection(
        draw,
        top_view,
        top_bounds,
        (pad, pad + view_height + gap, width - pad, height - pad),
        base_color,
    )
    draw.line(
        (pad, height / 2, width - pad, height / 2),
        fill=(42, 61, 79),
        width=RENDER_SCALE,
    )
    image = image.resize(
        (THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT), Image.Resampling.LANCZOS
    )
    image_path.parent.mkdir(parents=True, exist_ok=True)
    image.save(image_path, optimize=True)


def make_contact_sheet(rows, image_root, output_path, columns, title):
    label_height = 30
    cell_width = THUMBNAIL_WIDTH
    cell_height = THUMBNAIL_HEIGHT + label_height
    row_count = math.ceil(len(rows) / columns)
    title_height = 46
    sheet = Image.new(
        "RGB",
        (columns * cell_width, title_height + row_count * cell_height),
        (7, 12, 20),
    )
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default()
    draw.text((14, 15), title, fill=(225, 233, 240), font=font)
    for index, row in enumerate(rows):
        column = index % columns
        sheet_row = index // columns
        x = column * cell_width
        y = title_height + sheet_row * cell_height
        with Image.open(image_root / row["image_file"]) as thumbnail:
            sheet.paste(thumbnail, (x, y))
        label = f"{row['data_name']} / appearance {row['appearance_index']}"
        draw.text((x + 8, y + THUMBNAIL_HEIGHT + 8), label, fill=(205, 216, 226), font=font)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output_path, optimize=True)


def format_number(value, digits=0):
    if digits == 0:
        return f"{value:,.0f}"
    return f"{value:,.{digits}f}"


def separation_note(row):
    reasons = {entry["reason"] for entry in row["excluded_machinery"]}
    if "drive subtree" not in reasons and row["data_name"] == "STOFighter":
        return "No separable drive/reactor mesh; the single jet mesh is inseparable."
    if not reasons:
        return "No separately named drive/reactor/radiator mesh was present."
    return "; ".join(sorted(reasons))


def build_markdown(rows, metadata):
    human_rows = [row for row in rows if not row["alien"]]
    alien_rows = [row for row in rows if row["alien"]]
    standard_humans = [row for row in human_rows if row["data_name"] != "STOFighter"]
    human_volumes = [row["main_hull_elliptical_envelope_m3"] for row in human_rows]
    alien_volumes = [row["main_hull_elliptical_envelope_m3"] for row in alien_rows]
    lines = [
        "# Ship-hull graphical variants, measured volume, and utility slots",
        "",
        "Status: generated asset-measurement report. No gameplay values were changed.",
        "",
        "## Result",
        "",
        f"The installed catalog contains **{metadata['template_count']} hull templates** "
        f"and **{metadata['appearance_count']} graphical appearances**: "
        f"{metadata['human_template_count']} human templates with "
        f"{len(human_rows)} appearances, and {metadata['alien_template_count']} alien "
        f"templates with {len(alien_rows)} appearances. Every listed model resource "
        "was resolved and rendered.",
        "",
        "![Human hull appearance contact sheet](hull-variants/human-contact-sheet.png)",
        "",
        "![Alien hull appearance contact sheet](hull-variants/alien-contact-sheet.png)",
        "",
        "The art-derived values vary much more than the current utility counts. Across "
        f"all human appearances, the measured main-hull envelope spans "
        f"**{format_number(min(human_volumes))}–{format_number(max(human_volumes))} m³**; "
        f"the alien range is **{format_number(min(alien_volumes))}–"
        f"{format_number(max(alien_volumes))} m³**. These are exterior comparison "
        "envelopes, not usable interior volumes.",
        "",
        "## What the volume means",
        "",
        "For the combined active main-hull mesh bounds `X`, `Y`, and longitudinal "
        "length `L`, the report uses:",
        "",
        "`Vmain-envelope = pi / 4 * X * Y * L`",
        "",
        "The selection starts with the prefab's hull container and excludes the "
        "`Drive...` subtree, named radiator/reactor-bay meshes, and leaf meshes named "
        "as engines, thrusters, or reactors. The thumbnail is rendered from exactly "
        "the same included mesh set. This is the most reproducible volume available "
        "from the art without claiming that open or intersecting meshes form a "
        "watertight solid or that the exterior envelope is habitable space.",
        "",
        "`STOFighter` is the one explicit separation exception: its jet hull is a "
        "single mesh with no independent drive or reactor component. Its row is kept "
        "for complete catalog coverage and labelled accordingly.",
        "",
        "For comparison, `templateStoredVolume_m3` in the evidence is the serialized "
        "JSON value, while `runtimeCylinder_m3` applies the compiled-game formula "
        "`pi * (width_m / 2)^2 * length_m` after the mod's partial hull overrides. "
        "Neither is used to calculate the measured art envelope.",
        "",
        "## Slot definition",
        "",
        "The table reports nose hardpoints, hull hardpoints, and utility slots "
        "separately. `Total` is `nose + hull + utility`. Drive, power-plant, radiator, "
        "and armor positions are excluded, matching the established ship-balance "
        "analysis.",
        "",
        "## Standard human hulls and appearances",
        "",
        "| Graphic | Hull | App. | Main hull X × Y × L | Main hull envelope | Nose | Hull | Utility | Total |",
        "|---|---|---:|---:|---:|---:|---:|---:|---:|",
    ]
    lines.extend(markdown_rows(standard_humans))
    special_humans = [row for row in human_rows if row["data_name"] == "STOFighter"]
    if special_humans:
        lines.extend(
            [
                "",
                "## Special human hull",
                "",
                "| Graphic | Hull | App. | Main hull X × Y × L | Main hull envelope | Nose | Hull | Utility | Total |",
                "|---|---|---:|---:|---:|---:|---:|---:|---:|",
            ]
        )
        lines.extend(markdown_rows(special_humans))
    lines.extend(
        [
            "",
            "## Alien hulls",
            "",
            "| Graphic | Hull | App. | Main hull X × Y × L | Main hull envelope | Nose | Hull | Utility | Total |",
            "|---|---|---:|---:|---:|---:|---:|---:|---:|",
        ]
    )
    lines.extend(markdown_rows(alien_rows))
    lines.extend(
        [
            "",
            "## Hull-level utility-slot planning view",
            "",
            "Graphical appearances share template slot counts but can have different "
            "art envelopes. The range below prevents one appearance from being "
            "mistaken for the whole hull class.",
            "",
            "| Hull | Utility | Weapon + utility | Appearance count | Main-hull envelope range | Envelope per utility slot |",
            "|---|---:|---:|---:|---:|---:|",
        ]
    )
    for group in group_by_hull(rows):
        volumes = [row["main_hull_elliptical_envelope_m3"] for row in group]
        utility = group[0]["utility_slots"]
        per_utility = (
            f"{format_number(min(volumes) / utility)}–{format_number(max(volumes) / utility)} m³"
            if utility
            else "n/a"
        )
        lines.append(
            f"| {group[0]['data_name']} | {utility} | {group[0]['counted_slots']} | "
            f"{len(group)} | {format_number(min(volumes))}–{format_number(max(volumes))} m³ | "
            f"{per_utility} |"
        )
    lines.extend(
        [
            "",
            "## Interpretation for adding utility slots",
            "",
            "The measurements support using hull volume as a constraint or audit "
            "signal, but not a direct one-slot-equals-N-cubic-metres rule. Current "
            "utility slots are categorical permissions; larger hulls also devote "
            "more art volume to structure, weapons, tanks, armor clearance, damage "
            "tolerance, and heat-management machinery. Appearance spreads further "
            "show that a hull template can retain one slot layout while its art "
            "envelope changes materially.",
            "",
            "A later utility-slot change should therefore choose hull-level counts "
            "first, then use the smallest measured appearance envelope as the "
            "conservative art check. The present report supplies that evidence but "
            "does not recommend or implement new counts yet.",
            "",
            "## Reproducibility",
            "",
            f"- Installed hull template SHA-256: `{metadata['source_sha256']['hull_templates']}`",
            f"- Base `ships` bundle SHA-256: `{metadata['source_sha256']['ships_bundle']}`",
            f"- Dark Skies `ships_prm` bundle SHA-256: `{metadata['source_sha256']['ships_prm_bundle']}`",
            f"- Mod hull override SHA-256: `{metadata['source_sha256']['mod_hull_overrides']}`",
            "- Generator: [`generate_hull_variant_report.py`](../../scripts/ship-balance/generate_hull_variant_report.py)",
            "- Shared prefab traversal: [`measure_ship_prefabs.py`](../../scripts/ship-balance/measure_ship_prefabs.py)",
            "- Machine-readable rows: [`hull-variant-volume-and-slots.csv`](tables/hull-variant-volume-and-slots.csv)",
            "- Full mesh-path evidence: [`hull-variant-volume-and-slots.json`](tables/hull-variant-volume-and-slots.json)",
            "",
            "Run from the repository root with Python 3.12, NumPy, Pillow, UnityPy, "
            "and a local Terra Invicta installation:",
            "",
            "```powershell",
            "python scripts/ship-balance/generate_hull_variant_report.py `",
            "  --game-install-dir 'D:\\Games\\SteamLibrary\\steamapps\\common\\Terra Invicta'",
            "```",
            "",
            "The generator omits timestamps, sorts machine-readable keys, and uses "
            "fixed rendering settings so identical inputs produce identical files.",
        ]
    )
    return "\n".join(lines) + "\n"


def markdown_rows(rows):
    lines = []
    for row in rows:
        size = row["main_hull_bounds"]["size_xyz_m"]
        image = f"<img src=\"hull-variants/{row['image_file']}\" width=\"220\">"
        lines.append(
            f"| {image} | {row['data_name']} | {row['appearance_index']} | "
            f"{format_number(size[0], 1)} × {format_number(size[1], 1)} × "
            f"{format_number(size[2], 1)} m | "
            f"{format_number(row['main_hull_elliptical_envelope_m3'])} m³ | "
            f"{row['nose_hardpoints']} | {row['hull_hardpoints']} | "
            f"{row['utility_slots']} | {row['counted_slots']} |"
        )
    return lines


def group_by_hull(rows):
    groups = []
    current = []
    current_name = None
    for row in rows:
        if row["data_name"] != current_name:
            if current:
                groups.append(current)
            current = []
            current_name = row["data_name"]
        current.append(row)
    if current:
        groups.append(current)
    return groups


def write_csv(rows, path):
    fields = [
        "dataName",
        "alien",
        "appearanceIndex",
        "modelResource",
        "prefabAssetPath",
        "imageFile",
        "mainHullX_m",
        "mainHullY_m",
        "mainHullLength_m",
        "mainHullEllipticalEnvelope_m3",
        "templateStoredVolume_m3",
        "runtimeCylinder_m3",
        "noseHardpoints",
        "hullHardpoints",
        "utilitySlots",
        "weaponSlots",
        "countedSlots",
        "includedMeshCount",
        "excludedMachineryMeshCount",
        "separationNote",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields, lineterminator="\n")
        writer.writeheader()
        for row in rows:
            size = row["main_hull_bounds"]["size_xyz_m"]
            writer.writerow(
                {
                    "dataName": row["data_name"],
                    "alien": str(row["alien"]).lower(),
                    "appearanceIndex": row["appearance_index"],
                    "modelResource": row["model_resource"],
                    "prefabAssetPath": row["prefab_asset_path"],
                    "imageFile": row["image_file"],
                    "mainHullX_m": f"{size[0]:.6f}",
                    "mainHullY_m": f"{size[1]:.6f}",
                    "mainHullLength_m": f"{size[2]:.6f}",
                    "mainHullEllipticalEnvelope_m3": f"{row['main_hull_elliptical_envelope_m3']:.6f}",
                    "templateStoredVolume_m3": row["template_stored_volume_m3"],
                    "runtimeCylinder_m3": f"{row['runtime_cylinder_m3']:.6f}",
                    "noseHardpoints": row["nose_hardpoints"],
                    "hullHardpoints": row["hull_hardpoints"],
                    "utilitySlots": row["utility_slots"],
                    "weaponSlots": row["weapon_slots"],
                    "countedSlots": row["counted_slots"],
                    "includedMeshCount": len(row["included_mesh_paths"]),
                    "excludedMachineryMeshCount": len(row["excluded_machinery"]),
                    "separationNote": separation_note(row),
                }
            )


def validate_rows(measure, templates, rows, image_root):
    expected = sum(len(template["modelResource"]) for template in templates)
    if len(rows) != expected:
        raise RuntimeError(f"Generated {len(rows)} rows; expected {expected}")
    expected_pairs = {
        (template["dataName"], index)
        for template in templates
        for index in range(len(template["modelResource"]))
    }
    actual_pairs = {(row["data_name"], row["appearance_index"]) for row in rows}
    if actual_pairs != expected_pairs:
        raise RuntimeError("Hull/appearance coverage does not match the template catalog")
    for row in rows:
        if row["counted_slots"] != (
            row["nose_hardpoints"]
            + row["hull_hardpoints"]
            + row["utility_slots"]
        ):
            raise RuntimeError(f"Slot total mismatch for {row['data_name']}")
        if row["main_hull_elliptical_envelope_m3"] <= 0:
            raise RuntimeError(f"Non-positive volume for {row['data_name']}")
        if not (image_root / row["image_file"]).is_file():
            raise RuntimeError(f"Missing image for {row['data_name']}")
        for included in row["included_mesh_paths"]:
            if machinery_reason(measure, included) is not None:
                raise RuntimeError(f"Machinery path included for {row['data_name']}: {included}")


def main():
    args = parse_args()
    game_root = args.game_install_dir.resolve()
    output_root = args.output_root.resolve()
    template_path = (
        game_root
        / "TerraInvicta_Data"
        / "StreamingAssets"
        / "Templates"
        / "TIShipHullTemplate.json"
    )
    ships_bundle = (
        game_root
        / "TerraInvicta_Data"
        / "StreamingAssets"
        / "AssetBundles"
        / "ships"
    )
    dlc_bundle = game_root / "DLC_Content" / "DarkSkies" / "AssetBundles" / "ships_prm"
    override_path = REPOSITORY_ROOT / "TIEconomyMod" / "ModFiles" / "TIShipHullTemplate.json"
    for required in (template_path, ships_bundle, dlc_bundle, override_path):
        if not required.is_file():
            raise FileNotFoundError(required)

    measure = load_measurement_module()
    templates = merge_template_overrides(
        read_json(template_path), read_json(override_path)
    )
    base_env = measure.UnityPy.load(str(ships_bundle))
    dlc_env = measure.UnityPy.load(str(dlc_bundle))
    environments = {"base": base_env, "dlc": dlc_env}
    image_root = output_root / IMAGE_DIRECTORY
    geometry_cache = {}
    rows = []

    for template in templates:
        data_name = template["dataName"]
        model_resources = template["modelResource"]
        for appearance_index, model_resource in enumerate(model_resources):
            environment_name, prefab_path = prefab_location(
                measure, data_name, appearance_index, model_resource
            )
            environment = environments[environment_name]
            if prefab_path not in environment.container:
                raise RuntimeError(f"Missing prefab {prefab_path} for {data_name}")
            root = environment.container[prefab_path].read()
            root_transform = next(
                pointer
                for pointer in measure.component_ptrs(root)
                if pointer.type.name == "Transform"
            )
            records = {"meshes": [], "colliders": []}
            measure.walk(root_transform, np.eye(4), True, [], records)
            root_offset = measure.xyz(root_transform.read().m_LocalPosition)
            for category in records.values():
                for record in category:
                    record["points"] -= root_offset
            included, excluded = select_main_hull_meshes(measure, data_name, records)
            bounds = combined_bounds(measure, included)
            size = bounds[1] - bounds[0]
            envelope = math.pi / 4 * float(size[0]) * float(size[1]) * float(size[2])
            image_name = f"{data_name.lower()}-appearance-{appearance_index}.png"
            render_thumbnail(
                measure,
                included,
                root_offset,
                bounds,
                image_root / image_name,
                bool(template["alien"]),
                geometry_cache,
            )
            nose = int(template["noseHardpoints"])
            hull = int(template["hullHardpoints"])
            utility = int(template["internalModules"])
            runtime_volume = (
                math.pi
                * (float(template["width_m"]) / 2) ** 2
                * float(template["length_m"])
            )
            rows.append(
                {
                    "data_name": data_name,
                    "alien": bool(template["alien"]),
                    "appearance_index": appearance_index,
                    "model_resource": model_resource,
                    "prefab_asset_path": prefab_path,
                    "image_file": image_name,
                    "main_hull_bounds": measure.bound_record(bounds),
                    "main_hull_elliptical_envelope_m3": round(envelope, 6),
                    "template_stored_volume_m3": template["volume"],
                    "runtime_cylinder_m3": round(runtime_volume, 6),
                    "nose_hardpoints": nose,
                    "hull_hardpoints": hull,
                    "utility_slots": utility,
                    "weapon_slots": nose + hull,
                    "counted_slots": nose + hull + utility,
                    "included_mesh_paths": [record["path"] for record in included],
                    "excluded_machinery": excluded,
                }
            )

    validate_rows(measure, templates, rows, image_root)
    human_rows = [row for row in rows if not row["alien"]]
    alien_rows = [row for row in rows if row["alien"]]
    make_contact_sheet(
        human_rows,
        image_root,
        image_root / "human-contact-sheet.png",
        4,
        "Human ship-hull graphical appearances — side and top orthographic views",
    )
    make_contact_sheet(
        alien_rows,
        image_root,
        image_root / "alien-contact-sheet.png",
        3,
        "Alien ship-hull graphical appearances — side and top orthographic views",
    )

    metadata = {
        "method": "pi / 4 * main hull mesh AABB X * Y * longitudinal L",
        "template_count": len(templates),
        "appearance_count": len(rows),
        "human_template_count": len([item for item in templates if not item["alien"]]),
        "alien_template_count": len([item for item in templates if item["alien"]]),
        "source_sha256": {
            "hull_templates": sha256(template_path),
            "ships_bundle": sha256(ships_bundle),
            "ships_prm_bundle": sha256(dlc_bundle),
            "mod_hull_overrides": sha256(override_path),
        },
    }
    table_root = output_root / "tables"
    write_csv(rows, table_root / CSV_NAME)
    evidence = {"metadata": metadata, "rows": rows}
    with (table_root / JSON_NAME).open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(evidence, handle, indent=2, sort_keys=True)
        handle.write("\n")
    with (output_root / REPORT_NAME).open("w", encoding="utf-8", newline="\n") as handle:
        handle.write(build_markdown(rows, metadata))
    print(
        f"Generated {len(rows)} appearance rows for {len(templates)} hull templates "
        f"under {output_root}"
    )


if __name__ == "__main__":
    main()
