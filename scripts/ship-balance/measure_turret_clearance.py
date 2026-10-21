"""Measure installed turret meshes, native mount nodes and hypothetical clearance.

Read-only with respect to game assets and gameplay configuration. Reports are
written only below --output-root. Run export_turret_mount_maps.ps1 first.
"""
import argparse
import hashlib
import itertools
import json
import math
import os
from pathlib import Path
import sys

import numpy as np

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / '.tmp' / 'unitypy'))
import UnityPy
from UnityPy.helpers.MeshHelper import MeshHandler
from measure_ship_prefabs import component_ptrs, local_matrix, transform_points, xyz
from generate_hull_variant_report import prefab_location
import measure_ship_prefabs as hull_reader

DEFAULT_GAME = Path(os.environ.get('TI_GAME_INSTALL_DIR', r'D:\Games\SteamLibrary\steamapps\common\Terra Invicta'))
OUTPUT = ROOT / 'docs/ship-balance-research/turret-clearance'
TYPES = ['TIGunTemplate', 'TIMagneticGunTemplate', 'TILaserWeaponTemplate',
         'TIParticleWeaponTemplate', 'TIPlasmaWeaponTemplate', 'TIMissileTemplate']
SIZES = {'HalfHull': 1, 'OneHull': 1, 'TwoHullHoriz': 2, 'TwoHullVert': 2, 'FourHull': 4}
OFFSETS = {'OneHull': [(0, 0)], 'HalfHull': [(0, 0)],
           'TwoHullHoriz': [(0, 0), (1, 0)], 'TwoHullVert': [(0, 0), (0, 2)],
           'FourHull': [(0, 0), (1, 0), (0, 2), (1, 2)]}
CAPITALS = {'Titan', 'AlienTitan', 'AlienMothership'}
PADDINGS = [(0, 0), (2, 0), (5, 0), (10, 0), (0, .25), (0, .5), (5, .25)]


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def sha(path):
    with open(path, 'rb') as handle:
        return hashlib.file_digest(handle, 'sha256').hexdigest().upper()


def merge(base, overrides):
    by_name = {x['dataName']: x for x in overrides}
    assert not set(by_name) - {x['dataName'] for x in base}, 'Unknown override'
    return [dict(x, **{k: v for k, v in by_name.get(x['dataName'], {}).items()}) for x in base]


def transform(go):
    return next(p for p in component_ptrs(go) if p.type.name == 'Transform')


def resolve(owner, pointer):
    assert pointer['m_FileID'] == 0, 'Unexpected external serialized reference'
    return owner.assetsfile.objects[pointer['m_PathID']]


def mesh_vertices(go):
    ptr = next(p for p in component_ptrs(go) if p.type.name == 'MeshFilter')
    mesh = ptr.read().m_Mesh.read()
    handler = MeshHandler(mesh)
    handler.process()
    vertices = np.asarray(handler.m_Vertices, dtype=float)
    assert vertices.ndim == 2 and vertices.shape[1] == 3 and len(vertices)
    # MeshHandler positions are Unity coordinates, not the X-reflected OBJ export.
    aabb = mesh.m_LocalAABB
    assert np.all(vertices.min(0) >= xyz(aabb.m_Center) - xyz(aabb.m_Extent) - .02)
    assert np.all(vertices.max(0) <= xyz(aabb.m_Center) + xyz(aabb.m_Extent) + .02)
    return vertices, mesh.m_Name


def rotation_envelope(base, moving, offset, scale):
    """Bounding sphere under arbitrary yaw AND elevation, even nonuniform scale.

    ||S(p + Rv)|| <= ||Sp|| + sigma_max(S)||v||; triangle inequality
    gives a conservative bound without sampling gaps. Base rotation preserves norm.
    """
    base_r = np.linalg.norm(base * scale, axis=1).max()
    moving_r = np.linalg.norm(offset * scale) + max(abs(scale)) * np.linalg.norm(moving, axis=1).max()
    return float(max(base_r, moving_r))


def weapon_geometry(ptr):
    go = ptr.read()
    tr = transform(go).read()
    child = tr.m_Children[0].read()
    base, base_name = mesh_vertices(go)
    moving, moving_name = mesh_vertices(child.m_GameObject.read())
    scale, child_scale, offset = xyz(tr.m_LocalScale), xyz(child.m_LocalScale), xyz(child.m_LocalPosition)
    moving_scaled = moving * child_scale
    child_matrix = local_matrix(child)
    posed = np.concatenate([base * scale, transform_points(child_matrix, moving) * scale])
    radius = rotation_envelope(base, moving_scaled, offset, scale)
    yaw = float(np.linalg.norm(posed[:, [0, 2]], axis=1).max())
    return {
        'base_mesh': base_name, 'moving_mesh': moving_name,
        'base_vertices': len(base), 'moving_vertices': len(moving),
        'root_scale': scale.tolist(), 'child_scale': child_scale.tolist(), 'child_position': offset.tolist(),
        'posed_min_m': posed.min(0).tolist(), 'posed_max_m': posed.max(0).tolist(),
        'posed_dimensions_m': np.ptp(posed, axis=0).tolist(),
        'yaw_only_radius_m': yaw, 'swept_radius_m': radius,
        '_base': base, '_moving': moving, '_offset': offset,
    }


def hierarchy(ptr, parent=None, path=''):
    tr = ptr.read()
    go = tr.m_GameObject.read()
    matrix = (np.eye(4) if parent is None else parent) @ local_matrix(tr)
    current = path + '/' + go.m_Name
    yield go.object_reader.path_id, (matrix, tr, current)
    for child in tr.m_Children:
        yield from hierarchy(child, matrix, current)


def hull_geometry(env, path, template, maps):
    go = env.container[path].read()
    root_tr = transform(go)
    # Remove scene placement only. Retain root scale and orientation.
    objects = dict(hierarchy(root_tr))
    origin = objects[go.object_reader.path_id][0][:3, 3].copy()
    controllers = []
    for ptr in component_ptrs(go):
        if ptr.type.name == 'MonoBehaviour':
            tree = ptr.read_typetree()
            if 'dorsalHullWeaponControllers' in tree:
                controllers.append((ptr, tree))
    assert len(controllers) == 1, (path, len(controllers))
    owner, tree = controllers[0]
    script = owner.read().m_Script.read()
    cls = script.m_Namespace + '.' + script.m_ClassName
    mapping = maps[cls]
    slots = [(i, s) for i, s in enumerate(template['shipModuleSlots']) if s['moduleSlotType'] == 'HullHardPoint']
    ordinary = {mapping['OneHull'][i] for i, s in slots}
    assert None not in ordinary and len(ordinary) == len(slots), (template['dataName'], ordinary, slots)
    nodes = []
    for side in ('dorsal', 'ventral'):
        refs = tree[side + 'HullWeaponControllers']
        if refs:
            assert not ordinary - set(range(len(refs))), (path, side, ordinary, len(refs))
        for index, ref in enumerate(refs):
            node_owner = resolve(owner, ref)
            node_tree = node_owner.read_typetree()
            bid, wid = node_tree['baseObject']['m_PathID'], node_tree['weaponObject']['m_PathID']
            matrix, tr, node_path = objects[bid]
            _, wtr, weapon_path = objects[wid]
            assert wtr.m_Father.path_id == tr.object_reader.path_id, (node_path, 'unexpected weapon hierarchy')
            # Combat replaces base scale. Strip the placeholder base scale only.
            parent_rotation = matrix[:3, :3] @ np.diag(1 / xyz(tr.m_LocalScale))
            parent_scale = float(np.linalg.svd(parent_rotation, compute_uv=False).max())
            nodes.append({'side': side, 'index': index, 'ordinary': index in ordinary,
                          'path': node_path, 'position_m': (matrix[:3, 3] - origin).tolist(),
                          'parent_max_scale': parent_scale, 'placeholder_base_scale': xyz(tr.m_LocalScale).tolist(),
                          'placeholder_weapon_scale': xyz(wtr.m_LocalScale).tolist(),
                          'controller_path_id': node_owner.path_id})
    used_multi = set()
    coords = {(s['x'], s['y']) for _, s in slots}
    legal = {}
    for mount, offsets in OFFSETS.items():
        if mount == 'HalfHull':
            continue
        placements = []
        for i, slot in slots:
            cells = [(slot['x'] + dx, slot['y'] + dy) for dx, dy in offsets]
            if all(c in coords for c in cells):
                idx = mapping[mount][i]
                assert idx is not None
                placements.append({'slot': i, 'controller': idx, 'cells': cells})
                if mount != 'OneHull':
                    used_multi.add(idx)
        legal[mount] = placements
    return {'controller': cls, 'nodes': nodes, 'ordinary_indices': sorted(ordinary),
            'multi_indices': sorted(used_multi), 'legal_placements': legal,
            'slot_indices': [i for i, s in slots]}


def min_distance(nodes):
    pairs = [(float(np.linalg.norm(np.array(a['position_m']) - b['position_m'])), a, b)
             for a, b in itertools.combinations(nodes, 2) if a['index'] != b['index']]
    if not pairs:
        return None, None
    dist, a, b = min(pairs, key=lambda x: x[0])
    return dist, f"{a['side']}:{a['index']} / {b['side']}:{b['index']}"


def pair_clearance(nodes, radii):
    result = []
    for a, b in itertools.combinations(nodes, 2):
        if a['index'] == b['index']:
            continue  # Opposite-face representations of the same logical mount.
        ra, rb = radii[(a['side'], a['index'])], radii[(b['side'], b['index'])]
        distance = float(np.linalg.norm(np.array(a['position_m']) - b['position_m']))
        diameter = ra + rb
        result.append({'a': a['index'], 'b': b['index'], 'sides': a['side'] + '/' + b['side'],
                       'distance_m': distance, 'gap_m': distance - diameter,
                       'gap_ratio': (distance - diameter) / diameter})
    return result


def extra_subset(ordinary, all_indices, pairs, absolute, relative):
    bad = {tuple(sorted((p['a'], p['b']))) for p in pairs
           if p['gap_m'] + 1e-8 < absolute or p['gap_ratio'] + 1e-8 < relative}
    if any(a in ordinary and b in ordinary for a, b in bad):
        return None
    candidates = [x for x in sorted(set(all_indices) - set(ordinary))
                  if not any(tuple(sorted((x, y))) in bad for y in ordinary)]
    # Small authored controller sets: exact maximum additional-node search.
    best = []
    def visit(chosen, remaining):
        nonlocal best
        if len(chosen) + len(remaining) <= len(best):
            return
        if not remaining:
            best = chosen
            return
        first, rest = remaining[0], remaining[1:]
        visit(chosen + [first], [x for x in rest if tuple(sorted((first, x))) not in bad])
        visit(chosen, rest)
    visit([], candidates)
    return sorted(set(ordinary) | set(best))


def pack_count(placements):
    masks = [set(map(tuple, p['cells'])) for p in placements]
    best = 0
    def visit(index, used, count):
        nonlocal best
        if count + len(masks) - index <= best:
            return
        if index == len(masks):
            best = max(best, count)
            return
        if not (used & masks[index]):
            visit(index + 1, used | masks[index], count + 1)
        visit(index + 1, used, count)
    visit(0, set(), 0)
    return best


def raw_output(w, kind):
    shots = w.get('salvo_shots', 1)
    cycle = w['cooldown_s'] + (shots - 1) * w.get('intraSalvoCooldown_s', 0)
    if kind in ('TILaserWeaponTemplate', 'TIParticleWeaponTemplate'):
        energy = w['shotPower_MJ']
    elif kind in ('TIGunTemplate', 'TIMagneticGunTemplate', 'TIPlasmaWeaponTemplate'):
        energy = .5 * w['warheadMass_kg'] * w['muzzleVelocity_kps'] ** 2
    else:
        # Missile impact includes target-relative velocity and warhead effects;
        # do not invent a raw comparable kinetic DPS from launch speed.
        return None
    return {'shot_MJ': energy, 'cycle_s': cycle, 'shots': shots,
            'raw_MJ_s': energy * shots / cycle, 'raw_damage_points_s': energy * shots / cycle / 20,
            'mass_t': w['baseWeaponMass_tons'], 'range_km': w['targetingRange_km']}


def measure(game, output):
    template_root = game / 'TerraInvicta_Data/StreamingAssets/Templates'
    bundle_root = game / 'TerraInvicta_Data/StreamingAssets/AssetBundles'
    map_path = output / 'mount-maps.json'
    maps_doc = read(map_path)
    assembly = game / 'TerraInvicta_Data/Managed/Assembly-CSharp.dll'
    assert maps_doc['assembly_sha256'] == sha(assembly), 'Stale controller map; re-export'
    source_paths = [assembly, map_path, Path(__file__), Path(__file__).with_name('export_turret_mount_maps.ps1'),
                    Path(__file__).with_name('report_turret_clearance.py'),
                    ROOT / 'TIEconomyMod/Core/CapitalMountPolicy.cs', ROOT / 'TIEconomyMod/Patches/CapitalMountPatches.cs']
    def templates(kind):
        base_path = template_root / (kind + '.json')
        override_path = ROOT / 'TIEconomyMod/ModFiles' / (kind + '.json')
        source_paths.append(base_path)
        base = read(base_path)
        if override_path.exists():
            source_paths.append(override_path)
            return base, merge(base, read(override_path))
        return base, base
    vanilla_hulls, hulls = templates('TIShipHullTemplate')
    base_hulls = {h['dataName']: h for h in vanilla_hulls}
    bundle_paths = {'base': bundle_root / 'ships', 'dlc': game / 'DLC_Content/DarkSkies/AssetBundles/ships_prm',
                    'weapons': bundle_root / 'shipweapons'}
    source_paths.extend(bundle_paths.values())
    envs = {key: UnityPy.load(str(path)) for key, path in bundle_paths.items()}
    weapon_paths = {}
    for path, ptr in envs['weapons'].container.items():
        key = Path(path).stem.lower()
        assert key not in weapon_paths, ('Ambiguous resource', key)
        weapon_paths[key] = (path, ptr)
    weapons, geometries = [], {}
    for kind in TYPES:
        vanilla, current = templates(kind)
        old = {w['dataName']: w for w in vanilla}
        for w in current:
            if w.get('mount') not in SIZES:
                continue
            resource = w['modelResource']
            path, ptr = weapon_paths[resource.split('/')[-1].lower()]
            if resource not in geometries:
                geometries[resource] = dict(weapon_geometry(ptr), resource=resource, asset_path=path)
            weapons.append({'name': w['dataName'], 'kind': kind, 'size': SIZES[w['mount']], 'mount': w['mount'],
                            'alien': w.get('requiredProjectName') in ('Project_AlienMasterProject', 'Project_AlienAdvancedMasterProject'),
                            'attack': w.get('attackMode', False), 'resource': resource,
                            'current': raw_output(w, kind), 'vanilla': raw_output(old[w['dataName']], kind),
                            'template': w})
    variants = []
    for h in hulls:
        for appearance, resource in enumerate(h['modelResource']):
            key, path = prefab_location(hull_reader, h['dataName'], appearance, resource)
            geo = hull_geometry(envs[key], path, h, maps_doc['controllers'])
            ordinary = [n for n in geo['nodes'] if n['ordinary']]
            dist, pair = min_distance(ordinary)
            all_dist, all_pair = min_distance(geo['nodes'])
            row = dict(geo, hull=h['dataName'], appearance=appearance, model_resource=resource,
                       asset_path=path, alien=h.get('alien', False), tier=h.get('consTier'),
                       simple=h.get('simpleHull', False), ui_hardpoints=h['hullHardpoints'],
                       stock_hardpoints=base_hulls[h['dataName']]['hullHardpoints'],
                       ordinary_min_distance_m=dist, ordinary_nearest_pair=pair,
                       all_min_distance_m=all_dist, all_nearest_pair=all_pair,
                       packing={m: pack_count(p) for m, p in geo['legal_placements'].items()})
            assert len(geo['ordinary_indices']) == h['hullHardpoints']
            # Native-sized complete hull meshes follow resource scales in combat.
            # Simple hulls retain placeholder scale; also retain designer values.
            row['fits'] = []
            for context, scope in itertools.product(('combat', 'designer'), ('native', 'all')):
                for size in (1, 2, 4):
                    eligible = [w for w in weapons if w['size'] == size and
                                (scope == 'all' or w['alien'] == row['alien'])]
                    assert eligible, (scope, size, row['hull'])
                    radii, limiting = {}, {}
                    for node in geo['nodes']:
                        values = []
                        for w in eligible:
                            g = geometries[w['resource']]
                            if row['simple'] or context == 'designer':
                                r = rotation_envelope(g['_base'], g['_moving'] * node['placeholder_weapon_scale'],
                                                      g['_offset'], np.asarray(node['placeholder_base_scale']))
                            else:
                                r = g['swept_radius_m']
                            values.append((r * node['parent_max_scale'], w['resource']))
                        node_key = (node['side'], node['index'])
                        radii[node_key], limiting[node_key] = max(values)
                    pairs = pair_clearance(geo['nodes'], radii)
                    opairs = [p for p in pairs if p['a'] in geo['ordinary_indices'] and p['b'] in geo['ordinary_indices']]
                    for abs_pad, rel_pad in PADDINGS:
                        full = lambda ps: all(p['gap_m'] + 1e-8 >= abs_pad and p['gap_ratio'] + 1e-8 >= rel_pad for p in ps)
                        indices = sorted({n['index'] for n in geo['nodes']})
                        subset = extra_subset(geo['ordinary_indices'], indices, pairs, abs_pad, rel_pad)
                        row['fits'].append({'context': context, 'scope': scope, 'size': size, 'padding_m': abs_pad, 'padding_ratio': rel_pad,
                                            'ordinary_pass': bool(ordinary) and full(opairs),
                                            'all_pass': bool(indices) and full(pairs), 'expanded_indices': subset,
                                            'ordinary_min_gap_m': min((p['gap_m'] for p in opairs), default=None),
                                            'ordinary_min_gap_ratio': min((p['gap_ratio'] for p in opairs), default=None),
                                            'all_min_gap_m': min((p['gap_m'] for p in pairs), default=None),
                                            'limiting_pair': min(opairs, key=lambda p: p['gap_m']) if opairs else None})
            variants.append(row)
            print(f"Measured {row['hull']} appearance {appearance}: {len(ordinary)} ordinary physical nodes", flush=True)
    expected = sum(len(h['modelResource']) for h in hulls)
    assert len(variants) == expected
    public_geometry = [{k: v for k, v in g.items() if not k.startswith('_')} for g in geometries.values()]
    result = {'schema': 1, 'sources': [{'path': str(p), 'sha256': sha(p)} for p in dict.fromkeys(source_paths)],
              'coverage': {'hulls': len(hulls), 'variants': expected, 'weapons': len(weapons), 'weapon_resources': len(geometries)},
              'paddings': PADDINGS, 'geometries': public_geometry, 'weapons': weapons, 'variants': variants}
    output.mkdir(parents=True, exist_ok=True)
    (output / 'measurements.json').write_text(json.dumps(result, indent=2, allow_nan=False) + '\n', encoding='utf-8')
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--game-install-dir', type=Path, default=DEFAULT_GAME)
    parser.add_argument('--output-root', type=Path, default=OUTPUT)
    parser.add_argument('--reports-only', action='store_true')
    args = parser.parse_args()
    result = read(args.output_root / 'measurements.json') if args.reports_only else measure(args.game_install_dir, args.output_root)
    from report_turret_clearance import write_reports
    write_reports(result, args.output_root)
    print(json.dumps(result['coverage']))


if __name__ == '__main__':
    main()
