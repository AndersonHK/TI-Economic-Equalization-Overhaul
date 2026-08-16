import json
import os
import sys

import numpy as np

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
REPOSITORY_ROOT = os.path.abspath(os.path.join(SCRIPT_DIR, "..", ".."))
LOCAL_UNITYPY = os.path.join(REPOSITORY_ROOT, ".tmp", "unitypy")
if os.path.isdir(LOCAL_UNITYPY):
    sys.path.insert(0, LOCAL_UNITYPY)

try:
    import UnityPy
except ImportError as exception:
    raise RuntimeError(
        "UnityPy is required. Install it in the active Python environment or "
        "place an unpacked runtime at .tmp/unitypy."
    ) from exception


BUNDLE = (
    r"D:\Games\SteamLibrary\steamapps\common\Terra Invicta"
    r"\TerraInvicta_Data\StreamingAssets\AssetBundles\ships"
)
DLC_BUNDLE = (
    r"D:\Games\SteamLibrary\steamapps\common\Terra Invicta"
    r"\DLC_Content\DarkSkies\AssetBundles\ships_prm"
)
PREFABS = {
    "Battlecruiser": "assets/artresources/ships/earth/battlecruiser/battlecruiser.prefab",
    "Battleship": "assets/artresources/ships/earth/battleship/battleship.prefab",
    "Corvette": "assets/artresources/ships/earth/corvette/corvette.prefab",
    "Cruiser": "assets/artresources/ships/earth/cruiser/cruiser.prefab",
    "Dreadnought": "assets/artresources/ships/earth/dreadnought/dreadnought.prefab",
    "Destroyer": "assets/artresources/ships/earth/destroyer/destroyer.prefab",
    "Escort": "assets/artresources/ships/earth/escort/escort.prefab",
    "Frigate": "assets/artresources/ships/earth/frigate/frigate.prefab",
    "Gunship": "assets/artresources/ships/earth/gunship/gunship.prefab",
    "Lancer": "assets/artresources/ships/earth/lancer/lancer.prefab",
    "Monitor": "assets/artresources/ships/earth/monitor/monitor.prefab",
    "Titan": "assets/artresources/ships/earth/titan/titan.prefab",
}
ALIEN_PREFABS = {
    "AlienAssaultCarrier": "assets/artresources/ships/alien/assault_carrier/alienassaultcarrier.prefab",
    "AlienBattlecruiser": "assets/artresources/ships/alien/battlecruiser/alienbattlecruiser.prefab",
    "AlienBattleship": "assets/artresources/ships/alien/battleship/alienbattleship.prefab",
    "AlienCorvette": "assets/artresources/ships/alien/corvette/aliencorvette.prefab",
    "AlienCruiser": "assets/artresources/ships/alien/cruiser/aliencruiser.prefab",
    "AlienDestroyer": "assets/artresources/ships/alien/destroyer/aliendestroyer.prefab",
    "AlienDreadnought": "assets/artresources/ships/alien/dreadnought/aliendreadnought.prefab",
    "AlienEscort": "assets/artresources/ships/alien/escort/alienescort.prefab",
    "AlienFrigate": "assets/artresources/ships/alien/frigate/alienfrigate.prefab",
    "AlienGunship": "assets/artresources/ships/alien/gunship/aliengunship.prefab",
    "AlienLancer": "assets/artresources/ships/alien/lancer/alienlancer.prefab",
    "AlienMonitor": "assets/artresources/ships/alien/monitor/alienmonitor.prefab",
    "AlienMothership": "assets/artresources/ships/alien/mothership/alienmothership.prefab",
    "AlienTitan": "assets/artresources/ships/alien/titan/alientitan.prefab",
    "SalamanderGunship": "assets/artresources/ships/alien/salamander_gunship/salamandergunship.prefab",
}
RAYCAST_LAYER = 17
MESH_COMPONENT_CACHE = {}
ALT_DRIVE_STEMS = {
    "Battlecruiser": "earth_bc_alt",
    "Battleship": "earth_bs_alt",
    "Corvette": "earth_co_alt",
    "Cruiser": "earth_lc_alt",
    "Destroyer": "earth_des_alt",
    "Dreadnought": "earth_dr_alt",
    "Escort": "earth_es_alt",
    "Frigate": "earth_fr_alt",
    "Gunship": "earth_gs_alt",
    "Lancer": "earth_lan_alt",
    "Monitor": "earth_mo_alt",
    "Titan": "earth_ti_alt",
}
ALIEN_DRIVE_RESOURCE_PARTS = {
    "AlienAssaultCarrier": ("assault_carrier", "thrusters_assault_carrier", "alienassaultcarrier"),
    "AlienBattlecruiser": ("battlecruiser", "thrusters_battlecruiser", "alienbattlecruiser"),
    "AlienBattleship": ("battleship", "thrusters_battleship", "alienbattleship"),
    "AlienCorvette": ("corvette", "thrusters_corvette", "aliencorvette"),
    "AlienCruiser": ("cruiser", "thrusters", "aliencruiser"),
    "AlienDestroyer": ("destroyer", "thrusters_aliendestroyer", "aliendestroyer"),
    "AlienDreadnought": ("dreadnought", "thrusters_dreadnought", "aliendreadnought"),
    "AlienEscort": ("escort", "thrusters_escort", "alienescort"),
    "AlienFrigate": ("frigate", "thrusters_frigate", "alienfrigate"),
    "AlienGunship": ("gunship", "thrusters_gunship", "aliengunship"),
    "AlienLancer": ("lancer", "thrusters", "alienlancer"),
    "AlienMonitor": ("monitor", "thrusters", "alienmonitor"),
    "AlienMothership": ("mothership", "thrusters", "alienmothership"),
    "AlienTitan": ("titan", "thrusters", "alientitan"),
}


def xyz(value):
    return np.array([value.x, value.y, value.z], dtype=float)


def local_matrix(transform):
    q = transform.m_LocalRotation
    quaternion = np.array([q.x, q.y, q.z, q.w], dtype=float)
    quaternion /= np.linalg.norm(quaternion)
    x, y, z, w = quaternion
    rotation = np.array(
        [
            [1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)],
            [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)],
            [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)],
        ]
    )
    matrix = np.eye(4)
    matrix[:3, :3] = rotation @ np.diag(xyz(transform.m_LocalScale))
    matrix[:3, 3] = xyz(transform.m_LocalPosition)
    return matrix


def corners(center, extent):
    result = []
    for x in (-1, 1):
        for y in (-1, 1):
            for z in (-1, 1):
                result.append(center + extent * np.array([x, y, z]))
    return np.array(result)


def transform_points(matrix, points):
    homogeneous = np.column_stack([points, np.ones(len(points))])
    return (matrix @ homogeneous.T).T[:, :3]


def include_bounds(bounds, points):
    low = points.min(axis=0)
    high = points.max(axis=0)
    if bounds is None:
        return [low, high]
    return [np.minimum(bounds[0], low), np.maximum(bounds[1], high)]


def bound_record(bounds):
    if bounds is None:
        return None
    low, high = bounds
    return {
        "minimum_xyz_m": np.round(low, 3).tolist(),
        "maximum_xyz_m": np.round(high, 3).tolist(),
        "size_xyz_m": np.round(high - low, 3).tolist(),
    }


def component_ptrs(game_object):
    return [getattr(entry, "component", entry) for entry in game_object.m_Component]


def walk(transform_ptr, parent_matrix, parent_active, path, records):
    transform = transform_ptr.read()
    game_object = transform.m_GameObject.read()
    matrix = parent_matrix @ local_matrix(transform)
    active = parent_active and bool(game_object.m_IsActive)
    current_path = path + [game_object.m_Name]

    if active:
        components = component_ptrs(game_object)
        mesh_filter = next((p for p in components if p.type.name == "MeshFilter"), None)
        if mesh_filter:
            mesh_ptr = mesh_filter.read().m_Mesh
            if mesh_ptr.path_id:
                mesh = mesh_ptr.read()
                aabb = mesh.m_LocalAABB
                points = transform_points(
                    matrix, corners(xyz(aabb.m_Center), xyz(aabb.m_Extent))
                )
                records["meshes"].append(
                    {
                        "path": "/".join(current_path),
                        "layer": game_object.m_Layer,
                        "mesh_name": mesh.m_Name,
                        "mesh": mesh,
                        "mesh_cache_key": (
                            mesh.assets_file.name,
                            mesh.object_reader.path_id,
                        ),
                        "matrix": matrix,
                        "points": points,
                    }
                )

        for pointer in components:
            kind = pointer.type.name
            if kind not in {
                "BoxCollider",
                "CapsuleCollider",
                "SphereCollider",
            }:
                continue
            collider = pointer.read()
            if not collider.m_Enabled:
                continue
            if kind == "BoxCollider":
                center = xyz(collider.m_Center)
                extent = xyz(collider.m_Size) / 2
            elif kind == "SphereCollider":
                center = xyz(collider.m_Center)
                extent = np.full(3, collider.m_Radius)
            else:
                center = xyz(collider.m_Center)
                extent = np.full(3, collider.m_Radius)
                extent[collider.m_Direction] = collider.m_Height / 2
            points = transform_points(matrix, corners(center, extent))
            records["colliders"].append(
                {
                    "path": "/".join(current_path),
                    "layer": game_object.m_Layer,
                    "kind": kind,
                    "points": points,
                }
            )

    for child in transform.m_Children:
        walk(child, matrix, active, current_path, records)


def is_hull_path(path):
    parts = path.split("/")
    return len(parts) > 1 and any(
        "hull_" in part.lower() for part in parts[1:]
    )


def is_drive_path(path):
    parts = path.split("/")
    return len(parts) > 1 and parts[1].lower().startswith("drive")


def measure_hull(records):
    mesh_bounds = None
    collider_bounds = None
    collider_entries = []

    for record in records["meshes"]:
        path = record["path"]
        if is_hull_path(path):
            mesh_bounds = include_bounds(mesh_bounds, record["points"])

    for record in records["colliders"]:
        path = record["path"]
        if record["layer"] != RAYCAST_LAYER:
            continue
        if is_hull_path(path):
            collider_bounds = include_bounds(collider_bounds, record["points"])
            collider_entries.append(
                {
                    "path": path,
                    "kind": record["kind"],
                    **bound_record(
                        [record["points"].min(axis=0), record["points"].max(axis=0)]
                    ),
                }
            )

    if mesh_bounds is None:
        raise RuntimeError("No hull meshes found")

    return {
        "visual_bounds": mesh_bounds,
        "collider_bounds": collider_bounds,
        "output": {
            "hull_visual_mesh_envelope": bound_record(mesh_bounds),
            "hull_raycast_collider_envelope": bound_record(collider_bounds),
            "hull_raycast_colliders": collider_entries,
        },
    }


def measure_drive(records):
    mesh_bounds = None
    collider_bounds = None

    for record in records["meshes"]:
        if is_drive_path(record["path"]):
            mesh_bounds = include_bounds(mesh_bounds, record["points"])

    for record in records["colliders"]:
        if record["layer"] == RAYCAST_LAYER and is_drive_path(record["path"]):
            collider_bounds = include_bounds(
                collider_bounds, record["points"]
            )

    if mesh_bounds is None:
        raise RuntimeError("No drive meshes found")

    return {
        "visual_bounds": mesh_bounds,
        "collider_bounds": collider_bounds,
        "output": {
            "default_drive_visual_mesh_envelope": bound_record(mesh_bounds),
            "default_drive_raycast_collider_envelope": bound_record(
                collider_bounds
            ),
        },
    }


def is_reactor_bay_mesh_path(path):
    leaf = path.rsplit("/", 1)[-1].lower()
    return (
        "radiator" in leaf
        or leaf.endswith("_rads")
        or "_rad_" in leaf
    )


def measure_reactor_bay_prefab(env, asset_path, appearance_index):
    root = env.container[asset_path].read()
    root_transform = next(
        pointer
        for pointer in component_ptrs(root)
        if pointer.type.name == "Transform"
    )
    records = {"meshes": [], "colliders": []}
    walk(root_transform, np.eye(4), True, [], records)
    root_offset = xyz(root_transform.read().m_LocalPosition)

    bounds = None
    mesh_paths = []
    for record in records["meshes"]:
        record["points"] -= root_offset
        if not is_reactor_bay_mesh_path(record["path"]):
            continue
        bounds = include_bounds(bounds, record["points"])
        mesh_paths.append(record["path"])

    if bounds is None:
        raise RuntimeError(
            f"No reactor/radiator machinery mesh found in {asset_path}"
        )

    size = bounds[1] - bounds[0]
    diameter = min(float(size[0]), float(size[1]))
    inscribed_volume = np.pi / 4 * diameter * diameter * float(size[2])
    elliptical_volume = (
        np.pi / 4 * float(size[0]) * float(size[1]) * float(size[2])
    )
    return {
        "appearance_index": appearance_index,
        "asset_path": asset_path,
        "mesh_paths": mesh_paths,
        "bounds": bound_record(bounds),
        "inscribed_circular_cylinder_m3": round(
            float(inscribed_volume), 6
        ),
        "elliptical_cylinder_m3": round(float(elliptical_volume), 6),
    }


def measure_reactor_bay_variants(base_env, dlc_env, ship):
    ship_path = ship.lower()
    variants = {
        0: (
            base_env,
            PREFABS[ship],
        ),
        1: (
            base_env,
            "assets/artresources/ships/earth_alt/"
            f"{ship_path}/{ship_path}_1.prefab",
        ),
        2: (
            dlc_env,
            "assets/artresources/ships/earth_prm/"
            f"{ship_path}/{ship_path}_2.prefab",
        ),
        3: (
            dlc_env,
            "assets/artresources/ships/dlca/"
            f"{ship_path}/{ship_path}_3.prefab",
        ),
    }
    output = {}
    for appearance_index, (env, asset_path) in variants.items():
        if env is None or asset_path not in env.container:
            output[str(appearance_index)] = {
                "appearance_index": appearance_index,
                "asset_path": asset_path,
                "status": "unavailable",
            }
            continue
        output[str(appearance_index)] = measure_reactor_bay_prefab(
            env, asset_path, appearance_index
        )
    return output


def walk_drive_resource(transform_ptr, parent_matrix, path, records):
    transform = transform_ptr.read()
    game_object = transform.m_GameObject.read()
    matrix = parent_matrix @ local_matrix(transform)
    current_path = path + [game_object.m_Name]

    components = component_ptrs(game_object)
    mesh_filter = next((p for p in components if p.type.name == "MeshFilter"), None)
    if mesh_filter:
        mesh_ptr = mesh_filter.read().m_Mesh
        if mesh_ptr.path_id:
            mesh = mesh_ptr.read()
            aabb = mesh.m_LocalAABB
            points = transform_points(
                matrix, corners(xyz(aabb.m_Center), xyz(aabb.m_Extent))
            )
            records["meshes"].append(
                {
                    "path": "/".join(current_path),
                    "mesh_name": mesh.m_Name,
                    "mesh": mesh,
                    "mesh_cache_key": (
                        mesh.assets_file.name,
                        mesh.object_reader.path_id,
                    ),
                    "matrix": matrix,
                    "points": points,
                }
            )

    name = game_object.m_Name
    if (
        ("ThrusterPoint" in name or "Thruster" in name or "thruster" in name)
        and "Thruster_Alien" not in name
    ):
        records["thruster_points"].append(
            {
                "path": "/".join(current_path),
                "position_xyz_m": matrix[:3, 3],
            }
        )

    for child in transform.m_Children:
        walk_drive_resource(child, matrix, current_path, records)


def mesh_connected_components(mesh, cache_key):
    cached = MESH_COMPONENT_CACHE.get(cache_key)
    if cached is not None:
        return cached

    vertices = []
    faces = []
    for line in mesh.export().splitlines():
        if line.startswith("v "):
            vertices.append(
                tuple(float(value) for value in line.split()[1:4])
            )
        elif line.startswith("f "):
            faces.append(
                [int(token.split("/", 1)[0]) - 1 for token in line.split()[1:]]
            )

    parent = list(range(len(vertices)))
    component_size = [1] * len(vertices)

    def find(value):
        while parent[value] != value:
            parent[value] = parent[parent[value]]
            value = parent[value]
        return value

    def union(left, right):
        left = find(left)
        right = find(right)
        if left == right:
            return
        if component_size[left] < component_size[right]:
            left, right = right, left
        parent[right] = left
        component_size[left] += component_size[right]

    for face in faces:
        for index in face[1:]:
            union(face[0], index)

    grouped = {}
    for index, vertex in enumerate(vertices):
        grouped.setdefault(find(index), []).append(vertex)

    components = []
    for component in grouped.values():
        if len(component) < 20:
            continue
        values = np.array(component, dtype=float)
        components.append(
            {
                "vertex_count": len(component),
                "local_bounds": [values.min(axis=0), values.max(axis=0)],
            }
        )
    MESH_COMPONENT_CACHE[cache_key] = components
    return components


def measure_individual_drive_nozzles(records):
    thruster_points = np.array(
        [record["position_xyz_m"] for record in records["thruster_points"]],
        dtype=float,
    )
    if len(thruster_points) < 2:
        return None

    neighbor_distances = []
    for index, point in enumerate(thruster_points):
        other_points = np.delete(thruster_points[:, :2], index, axis=0)
        neighbor_distances.append(
            np.linalg.norm(other_points - point[:2], axis=1).min()
        )
    matching_radius = float(np.median(neighbor_distances) * 0.4)

    components = []
    for mesh_record in records["meshes"]:
        raw_components = mesh_connected_components(
            mesh_record["mesh"], mesh_record["mesh_cache_key"]
        )
        for raw_component in raw_components:
            low, high = raw_component["local_bounds"]
            points = transform_points(
                mesh_record["matrix"], corners((low + high) / 2, (high - low) / 2)
            )
            transformed_low = points.min(axis=0)
            transformed_high = points.max(axis=0)
            size = transformed_high - transformed_low
            components.append(
                {
                    "vertex_count": raw_component["vertex_count"],
                    "center": (transformed_low + transformed_high) / 2,
                    "size": size,
                    "transverse_area": float(size[0] * size[1]),
                }
            )

    selected = []
    used = set()
    for thruster_point in thruster_points:
        candidates = []
        for index, component in enumerate(components):
            transverse_min = min(component["size"][0], component["size"][1])
            transverse_max = max(component["size"][0], component["size"][1])
            if (
                index in used
                or component["size"][2] <= 0.05
                or transverse_min <= 0.05
                or transverse_max / transverse_min > 1.5
                or component["size"][2] > 2.5 * transverse_max
            ):
                continue
            distance = np.linalg.norm(
                component["center"][:2] - thruster_point[:2]
            )
            if distance <= matching_radius:
                candidates.append((component["transverse_area"], index, component))
        if not candidates:
            return {
                "method": "largest connected mesh component centered on each ThrusterPoint",
                "status": "unresolved",
                "reason": (
                    "No circular disconnected bell component matched ThrusterPoint "
                    + np.round(thruster_point, 3).tolist().__repr__()
                ),
            }
        _, index, component = max(candidates)
        used.add(index)
        selected.append(component)

    sizes = np.array([component["size"] for component in selected])
    areas = np.array(
        [component["transverse_area"] for component in selected]
    )
    return {
        "method": "largest connected mesh component centered on each ThrusterPoint",
        "status": "measured",
        "count": len(selected),
        "mean_size_xyz_m": np.round(sizes.mean(axis=0), 3).tolist(),
        "minimum_size_xyz_m": np.round(sizes.min(axis=0), 3).tolist(),
        "maximum_size_xyz_m": np.round(sizes.max(axis=0), 3).tolist(),
        "mean_transverse_bounding_area_m2": round(float(areas.mean()), 3),
        "minimum_transverse_bounding_area_m2": round(float(areas.min()), 3),
        "maximum_transverse_bounding_area_m2": round(float(areas.max()), 3),
    }


def measure_drive_resource(env, asset_path, measure_individual_nozzles=False):
    root = env.container[asset_path].read()
    root_transform_ptr = next(
        pointer
        for pointer in component_ptrs(root)
        if pointer.type.name == "Transform"
    )
    root_transform = root_transform_ptr.read()
    root_parent_matrix = np.eye(4)
    root_parent_matrix[:3, 3] = -xyz(root_transform.m_LocalPosition)
    records = {"meshes": [], "thruster_points": []}
    walk_drive_resource(
        root_transform_ptr, root_parent_matrix, [], records
    )

    mesh_bounds = None
    for record in records["meshes"]:
        mesh_bounds = include_bounds(mesh_bounds, record["points"])
    if mesh_bounds is None:
        raise RuntimeError(f"No meshes found in drive resource {asset_path}")

    size = mesh_bounds[1] - mesh_bounds[0]
    transverse_area = size[0] * size[1]
    result = {
        "asset_path": asset_path,
        "mesh_envelope": bound_record(mesh_bounds),
        "transverse_bounding_area_m2": round(float(transverse_area), 3),
        "equivalent_transverse_diameter_m": round(
            float(np.sqrt(transverse_area)), 3
        ),
        "thruster_point_count": len(records["thruster_points"]),
        "thruster_points_xyz_m": [
            np.round(record["position_xyz_m"], 3).tolist()
            for record in records["thruster_points"]
        ],
        "mesh_names": [record["mesh_name"] for record in records["meshes"]],
    }
    if measure_individual_nozzles:
        result["individual_nozzle_measurement"] = (
            measure_individual_drive_nozzles(records)
        )
    return result


def measure_drive_resources(env, ship):
    ship_path = ship.lower()
    output = {"default": {}, "alternate": {}}
    nozzle_counts = {
        "delaval": (1, 6),
        "magnetic": (1, 6),
        "pulse": (1,),
    }
    for nozzle, counts in nozzle_counts.items():
        output["default"][nozzle] = {}
        output["alternate"][nozzle] = {}
        for count in counts:
            default_asset_path = (
                "assets/artresources/ships/earth/"
                f"{ship_path}/prefabs/{nozzle}/"
                f"earth_{ship_path}_{nozzle}x{count}.prefab"
            )
            output["default"][nozzle][f"x{count}"] = measure_drive_resource(
                env,
                default_asset_path,
                measure_individual_nozzles=(count == 6),
            )

            alt_prefab_root = "prefabs/thrusters" if ship == "Titan" else "prefabs"
            alternate_asset_path = (
                "assets/artresources/ships/earth_alt/"
                f"{ship_path}/{alt_prefab_root}/{nozzle}/"
                f"{ALT_DRIVE_STEMS[ship]}_{nozzle}x{count}.prefab"
            )
            output["alternate"][nozzle][f"x{count}"] = measure_drive_resource(
                env,
                alternate_asset_path,
                measure_individual_nozzles=(count == 6),
            )
    return output


def measure_alien_drive_resources(env, ship):
    ship_path, prefab_root, resource_stem = ALIEN_DRIVE_RESOURCE_PARTS[ship]
    output = {"appearance_index": 0, "resource_family": "alien", "resources": {}}
    for count in (1, 6):
        asset_path = (
            "assets/artresources/ships/alien/"
            f"{ship_path}/prefabs/{prefab_root}/"
            f"thruster_{resource_stem}x{count}.prefab"
        )
        output["resources"][f"x{count}"] = measure_drive_resource(
            env,
            asset_path,
            measure_individual_nozzles=(count == 6),
        )
    return output


def summarize(records, ship):
    hull = measure_hull(records)
    drive = measure_drive(records)
    mesh_bounds = hull["visual_bounds"]
    collider_bounds = hull["collider_bounds"]
    drive_mesh_bounds = drive["visual_bounds"]
    drive_collider_bounds = drive["collider_bounds"]

    result = {"coordinate_note": "x/y are transverse axes; z is longitudinal"}
    result.update(hull["output"])
    result.update(drive["output"])
    combined_mesh = [
        np.minimum(mesh_bounds[0], drive_mesh_bounds[0]),
        np.maximum(mesh_bounds[1], drive_mesh_bounds[1]),
    ]
    combined_collider = None
    if collider_bounds is not None and drive_collider_bounds is not None:
        combined_collider = [
            np.minimum(collider_bounds[0], drive_collider_bounds[0]),
            np.maximum(collider_bounds[1], drive_collider_bounds[1]),
        ]
    result["hull_plus_default_drive_visual_envelope"] = bound_record(combined_mesh)
    result["hull_plus_default_drive_raycast_envelope"] = bound_record(
        combined_collider
    )
    return result


def main():
    env = UnityPy.load(BUNDLE)
    dlc_env = UnityPy.load(DLC_BUNDLE) if os.path.isfile(DLC_BUNDLE) else None
    output = {}
    for ship, asset_path in {**PREFABS, **ALIEN_PREFABS}.items():
        if ship not in PREFABS and ship not in ALIEN_DRIVE_RESOURCE_PARTS:
            output[ship] = {
                "drive_resource_measurements": {
                    "appearance_index": 0,
                    "status": "unavailable",
                    "reason": "No standalone drive resource is present in the installed ships bundle.",
                }
            }
            continue
        root_ptr = env.container[asset_path]
        root = root_ptr.read()
        root_transform = next(
            pointer
            for pointer in component_ptrs(root)
            if pointer.type.name == "Transform"
        )
        records = {"meshes": [], "colliders": []}
        # Ignore the prefab root's stored scene position; measurements are local.
        walk(root_transform, np.eye(4), True, [], records)
        root_offset = xyz(root_transform.read().m_LocalPosition)
        for category in records.values():
            for record in category:
                record["points"] -= root_offset
        try:
            output[ship] = summarize(records, ship)
        except RuntimeError as exception:
            raise RuntimeError(f"{ship}: {exception}") from exception
        if ship in PREFABS:
            output[ship]["reactor_bay_variant_measurements"] = (
                measure_reactor_bay_variants(env, dlc_env, ship)
            )
            output[ship]["drive_resource_measurements"] = (
                measure_drive_resources(env, ship)
            )
        elif ship in ALIEN_DRIVE_RESOURCE_PARTS:
            output[ship]["drive_resource_measurements"] = (
                measure_alien_drive_resources(env, ship)
            )
    print(json.dumps(output, indent=2))


if __name__ == "__main__":
    main()
