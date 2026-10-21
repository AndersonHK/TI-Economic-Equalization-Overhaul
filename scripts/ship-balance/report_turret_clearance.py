"""Generate Markdown tables from the turret audit evidence (standard library only)."""
import argparse
from collections import defaultdict
from functools import lru_cache
import itertools
import json
import math
from pathlib import Path
import re

CAPITALS = {'Titan', 'AlienTitan', 'AlienMothership'}


def num(value, places=2):
    return '—' if value is None else f'{value:,.{places}f}'


def pct(new, old):
    return '—' if new is None or not old else f'{100 * (new / old - 1):+.1f}%'


def table(headers, rows):
    return '\n'.join(['| ' + ' | '.join(headers) + ' |',
                      '| ' + ' | '.join(['---'] * len(headers)) + ' |'] +
                     ['| ' + ' | '.join(str(x).replace('|', '/') for x in row) + ' |' for row in rows]) + '\n'


def fit(v, size, absolute=5, relative=0, scope='native', context='combat'):
    return next(f for f in v['fits'] if f['size'] == size and f['padding_m'] == absolute
                and f['padding_ratio'] == relative and f['scope'] == scope and f['context'] == context)


def max_size(variants, absolute=5, relative=0, scope='native', context='combat', expanded=False):
    if not variants[0]['ui_hardpoints']:
        return 'no nodes'
    if variants[0]['ui_hardpoints'] == 1 and not expanded:
        return '4*'
    ok = [size for size in (1, 2, 4) if all(fit(v, size, absolute, relative, scope, context)
          ['all_pass' if expanded else 'ordinary_pass'] for v in variants)]
    return str(max(ok)) if ok else 'none certified'


def family(w):
    n = w['name']
    if w['kind'] == 'TIGunTemplate':
        return 'Naval cannon' if '-inchCannon' in n else n
    n = re.sub(r'^(Alien)?\d+cm', lambda m: m[1] or '', n)
    n = re.sub(r'^(Light|Heavy)', '', n)
    n = n.replace('AlienLightMag', 'AlienMag').replace('AlienHeavyMag', 'AlienMag')
    return n


def best_packed(v, weapons, baseline):
    """Exact maximum raw output with stock adjacency and mixed family sizes."""
    cells = sorted({tuple(c) for p in v['legal_placements']['OneHull'] for c in p['cells']})
    bit = {c: 1 << i for i, c in enumerate(cells)}
    options = []
    for w in weapons:
        mount = 'OneHull' if w['mount'] == 'HalfHull' else w['mount']
        for p in v['legal_placements'][mount]:
            mask = sum(bit[tuple(c)] for c in p['cells'])
            options.append((mask, w[baseline]['raw_MJ_s'], w['name']))
    @lru_cache(None)
    def solve(remaining):
        if not remaining:
            return 0., ()
        first = remaining & -remaining
        best = solve(remaining ^ first)
        for mask, dps, name in options:
            if mask & first and (mask & remaining) == mask:
                value, names = solve(remaining ^ mask)
                if value + dps > best[0]:
                    best = value + dps, names + (name,)
        return best
    return solve((1 << len(cells)) - 1)


def write_reports(d, output):
    output = Path(output)
    variants = d['variants']
    classes = defaultdict(list)
    for v in variants:
        classes[v['hull']].append(v)
    geometries = {g['resource']: g for g in d['geometries']}
    resources = defaultdict(list)
    for w in d['weapons']:
        resources[w['resource']].append(w)
    def save(name, sections):
        (output / name).write_text('\n\n'.join(sections).rstrip() + '\n', encoding='utf-8')

    overview = ['# Turret clearance and firepower audit',
        'Research snapshot: 2026-10-20 (user local date). Gameplay changes proposed here are not implemented. '
        'Measurements cover **28 hull classes, all 64 installed appearances, 147 hull-weapon templates, and 35 distinct weapon resources**. '
        'The geometry is decoded from installed assets; firing arcs and rendered combat behavior have not been manually tested.',
        '## Principal findings',
        '- A class-wide large-turret rule does **not** follow hull size monotonically. At a conservative **5 m edge gap**, '
        'all four human Destroyer, Cruiser, Battlecruiser and Lancer appearances certify size 4 at every ordinary node. '
        'Escort, Frigate, Monitor, Battleship, Dreadnought and Titan do not certify that rule across every appearance. '
        'A failed bound is not proof of mesh collision.\n'
        '- The designer and combat use different scale paths. `ShipModelController.SetWeapon` replaces base/weapon scales '
        'only when there is a ship state and the hull is not simple. This makes preview-based judgments insufficient; '
        'see the paired tables below and [methodology](methodology.md).\n'
        '- **AlienDestroyer has 3 paired controller indices for 2 UI hull cells**. Its middle merge mount is a credible '
        'extra-gun candidate. AlienDreadnought has 12 for 8, and AlienMothership 20 for 16; their closely clustered '
        'central mounts prevent treating all extras as free capacity. No human appearance has extra hull controller indices.\n'
        '- **Mothership ventral indices 1 and 18 are distinct controllers at exactly the same position**. The central '
        'ordinary left/right mounts are 8 m apart; merge nodes bisect those gaps to 4 m. The asset does not support '
        'an unconditional all-node large-turret clearance claim.\n'
        '- Raw firepower gains depend strongly on weapon family. The DPS report keeps complete stock, current-mod, '
        'size-2-per-node, and size-4-per-node baselines separate. Existing Titan/AlienTitan/Mothership size-4-per-node '
        'behavior is included in the current-mod baseline, not counted again as a new gain.',
        '## Reports and evidence',
        '- [Methodology, reproduction, formulas and limitations](methodology.md)\n'
        '- [Largest weapons and complete resource/template inventory](weapon-inventory.md)\n'
        '- [Every class and appearance: padding thresholds and clearance deltas](class-clearance.md)\n'
        '- [Extra/merge nodes and complete coordinate catalog](node-inventory.md), with [pairwise distance matrices](node-distance-matrices.md)\n'
        '- [DPS families, every class, capacity and mass deltas](dps-analysis.md)\n'
        '- [Machine-readable measurements](measurements.json) and [native controller maps](mount-maps.json)\n'
        '- [Validation and deployment record](validation.md)',
        '## Class-wide ordinary-node maximum size',
        'Native faction weapon inventory; every installed appearance must pass. `none certified` means no tested '
        'size certifies this conservative margin, not that stock weapons cannot work. `4*` denotes '
        'one logical mount: there is no neighboring hull turret, so this is a vacuous pair-spacing result, not a hull-fit recommendation. '
        'Opposite-face copies of the same logical weapon are not a second capacity slot.']
    overview.append(table(['Class', 'Tier', 'Art variants', 'UI nodes', 'Combat ≥2 m', 'Combat ≥5 m', 'Combat ≥10 m', 'Preview ≥5 m'],
        [(name, vs[0]['tier'], len(vs), vs[0]['ui_hardpoints'], max_size(vs, 2), max_size(vs), max_size(vs, 10),
          max_size(vs, context='designer')) for name, vs in classes.items()]))
    save('README.md', overview)

    methodology = ['# Measurement method and reproduction',
        '## Sources and scope',
        'Installed base and Dark Skies ship bundles, `shipweapons`, installed templates, repository partial overrides, '
        'and native integer controller mappings from the installed `Assembly-CSharp.dll`. Source hashes are below. '
        'The exporter invokes only `SlotToWeaponMountIndex` on uninitialized native controller instances; unavailable '
        'invalid-slot paths are recorded as null. Every used ordinary slot must map to a non-null, unique, in-range index. '
        'This avoids guessing from GameObject names or assuming serialized slots are sequential hull mounts. '
        'Native faction weapons are classified using the native `TIShipPartTemplate.isAlien` project rule '
        '(`Project_AlienMasterProject` / `Project_AlienAdvancedMasterProject`), including alien missiles with non-Alien names.',
        '## Geometry and scale',
        'Only the root base mesh and first-child weapon mesh are measured because those are the meshes copied by '
        '`ShipModelController.SetWeapon`. MeshHandler vertices retain Unity coordinates and are checked against '
        'the stored mesh bounds. Decorative VFX, fire-point transforms and arbitrary prefab scene placement are excluded. '
        'All hierarchy transforms are composed for hull nodes; scene-root translation is removed. Units follow the '
        'game art-space metre convention used by the existing hull survey, not each hull template’s separately declared dimensions.',
        'Combat on non-simple hulls replaces base scale with the weapon resource root scale and weapon scale with '
        'the resource child scale. Designer preview retains each hull node’s authored placeholder scales. Simple hulls '
        'retain placeholder scales in both paths. Child position always comes from the resource. The renderer initializes '
        'a base-orientation parent while preserving world placement; the extracted authored parent scale is retained. '
        'The installed nodes have unit parent scale. Exact placeholder scales and resource scales are retained in JSON.',
        '## Conservative swept envelope',
        'For root scale matrix S, weapon-child offset p, and child-scaled vertex v, arbitrary rotations obey '
        '`||S(p + Rv)|| ≤ ||Sp|| + σmax(S) ||v||`. The bound radius is the maximum of that expression and '
        'the farthest scaled base vertex, multiplied by any inherited parent maximum scale. The sphere includes '
        'all yaw/elevation orientations, not just the forward pose. This is deliberately conservative, especially '
        'for nonuniform alien scales and offset pivots. The inventory also gives exact authored-pose dimensions '
        'and an authored-pose yaw-only radius; these are not interchangeable with a full-elevation clearance guarantee.',
        'For each pair of different logical indices, including cross-face pairs, `gap = nodeDistance − radiusA − radiusB`. '
        'The proportional margin is `gap / (radiusA + radiusB)`; for equal turrets this is gap divided by the swept diameter. '
        'Absolute and proportional requirements combine with AND. We test 0, 2, 5 and 10 m; 25% and 50%; and 5 m plus 25%. '
        'Opposite-face copies of the same logical index are excluded from the neighbor test. No pair exists for single-node '
        'hulls, which are marked explicitly. A class passes only if every appearance and relevant node pair passes. '
        'The maximum size is searched independently: the biggest size-1 art can be larger than size-2 art.',
        '## Extra nodes and fitting',
        'Ordinary indices come from `OneHull` mapping of every declared HullHardPoint. All other controller indices '
        'are extra candidates, irrespective of their names. Legal multi-cell placements are derived from the native '
        'grid offsets: horizontal `(0,0),(1,0)`, vertical `(0,0),(0,2)`, four-cell rectangle. All-controller results '
        'require every candidate at once. The extra-subset search is exact: retain all ordinary mounts, then maximize '
        'compatible extras. An infeasible ordinary layout is reported as unavailable rather than silently deleting slots. '
        'New node use would require slot arrays, controller mapping, UI fitting, AI, save/refit migration, ammo/damage '
        'index handling and manual combat tests; extra graphics alone do not implement those behaviors.',
        '## What the geometry does not prove',
        'No occupied hull-mesh intersection test, radiator deployment sweep, nose-weapon clearance, fixed-launcher '
        'specialized envelope, occlusion/fire-arc validation, animation capture or in-game visual test is claimed. '
        'Sphere overlap only means this sufficient clearance test cannot certify the arrangement. It is not a '
        'mesh-intersection result. Zero-distance distinct mount origins are retained as a direct asset anomaly. '
        'A global inventory test additionally covers cross-faction art, but does not grant cross-faction weapon access.',
        '## Reproduce',
        'Use Python 3.12 with NumPy, UnityPy and Pillow (the shared hull catalog module imports Pillow). '
        'The existing `.tmp/unitypy` dependency directory is recognized. No network, game asset write or gameplay '
        'configuration write is performed by either research script.',
        '```powershell\npowershell -NoProfile -ExecutionPolicy Bypass -File scripts/ship-balance/export_turret_mount_maps.ps1\n'
        'python scripts/ship-balance/measure_turret_clearance.py\n'
        '# Regenerate Markdown from saved evidence using standard-library Python:\n'
        'python scripts/ship-balance/report_turret_clearance.py\n'
        'python scripts/ship-balance/test_turret_clearance.py\n```',
        'Both extraction scripts accept an installation path and output destination (`--game-install-dir` / '
        '`-GameInstallDir`; `--output-root` / `-OutputPath`). All retained artifacts belong under `docs/`. '
        'A stale native assembly hash is rejected before measurement. To inspect a given variant, locate its '
        '`hull` and `appearance` record in `measurements.json`; it contains all coordinates, native mapping, '
        'padding decisions and limiting pairs. Markdown nodes and variant tables expose the same inventory. '
        'For a single saved variant use `python scripts/ship-balance/report_turret_clearance.py --hull Titan --appearance 1`. '
        'Add `--geometry` to the test command for randomized envelope containment and exhaustive extra-subset checks; '
        'those checks require the extraction dependencies.',
        '## Source SHA-256 inventory',
        table(['Source', 'SHA-256'], [(s['path'], s['sha256']) for s in d['sources']])]
    save('methodology.md', methodology)

    weapon_doc = ['# Complete hull-weapon art inventory',
        'All 147 installed hull-weapon records are represented, including point defense, static launchers and '
        'HalfHull pods (classified with size 1 for this requested 1/2/4 survey). Technology variants frequently '
        'share the same mesh; counting them as separate art would distort the largest-size search. '
        'Dimensions and radii below use combat resource scale on a normal hull. Preview dimensions depend on the '
        'node’s placeholder scale. The mathematical bound and coordinate rules are in [methodology](methodology.md).',
        '## Largest by conservative full-rotation diameter',
        'This is the largest clearance envelope, not necessarily the widest forward-facing model. '
        'The complete dimensional-winner table follows so “largest” is not reduced to one ambiguous metric.']
    winners = []
    dimensional = []
    for label, selection in [('Human', False), ('Alien', True), ('All', None)]:
        for size in (1, 2, 4):
            ws = [w for w in d['weapons'] if w['size'] == size and (selection is None or w['alien'] == selection)]
            top = max(ws, key=lambda w: geometries[w['resource']]['swept_radius_m'])
            g = geometries[top['resource']]
            winners.append((label, size, top['name'], top['resource'].split('/')[-1],
                            ' × '.join(num(x, 3) for x in g['posed_dimensions_m']),
                            num(2*g['yaw_only_radius_m'], 3), num(2*g['swept_radius_m'], 3)))
            for metric, fn in [('width X', lambda g:g['posed_dimensions_m'][0]),
                               ('height Y', lambda g:g['posed_dimensions_m'][1]),
                               ('length Z', lambda g:g['posed_dimensions_m'][2]),
                               ('AABB envelope m³', lambda g:math.prod(g['posed_dimensions_m']))]:
                winner = max(ws, key=lambda w: fn(geometries[w['resource']]))
                dimensional.append((label,size,metric,winner['resource'].split('/')[-1],num(fn(geometries[winner['resource']]),3)))
    weapon_doc += [table(['Inventory','Size','Example weapon','Art','X × Y × Z m','Yaw diameter m','Swept diameter bound m'], winners),
                   table(['Inventory','Size','Largest metric','Art','Value'], dimensional),
                   '## Every distinct weapon resource',
                   table(['Resource','Sizes','Templates','X m','Y m','Z m','Yaw diameter m','Swept diameter m','Root scale'],
                         [(g['resource'].split('/')[-1], ','.join(map(str, sorted({w['size'] for w in resources[g['resource']]}))),
                           len(resources[g['resource']]), *[num(x,3) for x in g['posed_dimensions_m']],
                           num(2*g['yaw_only_radius_m'],3),num(2*g['swept_radius_m'],3),
                           ', '.join(num(x,5) for x in g['root_scale'])) for g in d['geometries']]),
                   '## Every template and art mapping',
                   'Raw MJ/s means emitted energy or muzzle kinetic energy before target effects. Missiles have no '
                   'single comparable raw DPS here and are marked “target dependent”; their geometry is fully included.',
                   table(['Template','Faction','Size / mount','Resource','Raw MJ/s','Mass t','Range km'],
                         [(w['name'],'Alien' if w['alien'] else 'Human',f"{w['size']} / {w['mount']}",
                           w['resource'].split('/')[-1], num(w['current']['raw_MJ_s']) if w['current'] else 'target dependent',
                           num(w['template']['baseWeaponMass_tons']),num(w['template']['targetingRange_km'],0)) for w in d['weapons']])]
    save('weapon-inventory.md',weapon_doc)

    clear = ['# Class and appearance clearance tables',
             'All gaps are between conservative full-rotation envelopes; values are metres. '
             '“None certified” is an inconclusive conservative test, not proven clipping. '
             '`4*` is a single-node pair-spacing result only. Each class row takes the worst appearance, '
             'and each appearance takes the worst pair. Native inventory means human weapons on human hulls '
             'and alien weapons on alien hulls. See [geometry definitions](methodology.md).']
    for context, scope in [('combat','native'), ('designer','native'), ('combat','all')]:
        clear += [f'## {context.title()} / {scope} weapon inventory: class-wide maximum size',
                  table(['Class','0 m','2 m','5 m','10 m','25% diameter','50% diameter','5 m AND 25%'],
                        [(name,*[max_size(vs,a,r,scope,context) for a,r in d['paddings']]) for name,vs in classes.items()])]
    clear += ['## Every appearance: ordinary nodes, combat native inventory',
              table(['Class/art','Nodes','Nearest centers m','Size 1 gap','Size 2 gap','Size 4 gap','Size 4 Δgap vs 1','Max at 5 m','Limiting pair'],
                    [(f"{v['hull']}/{v['appearance']}",v['ui_hardpoints'],num(v['ordinary_min_distance_m']),
                      *[num(fit(v,s)['ordinary_min_gap_m']) for s in (1,2,4)],
                      num(fit(v,4)['ordinary_min_gap_m']-fit(v,1)['ordinary_min_gap_m']) if v['ui_hardpoints']>1 else '—',
                      max_size([v]),v['ordinary_nearest_pair'] or 'no distinct logical pair') for v in variants]),
              '## Every appearance: preview versus combat at size 4',
              table(['Class/art','Preview gap m','Combat gap m','Combat − preview m','Preview maximum at 5 m','Combat maximum at 5 m'],
                    [(f"{v['hull']}/{v['appearance']}",num(fit(v,4,context='designer')['ordinary_min_gap_m']),
                      num(fit(v,4)['ordinary_min_gap_m']),
                      num(fit(v,4)['ordinary_min_gap_m']-fit(v,4,context='designer')['ordinary_min_gap_m']) if v['ui_hardpoints']>1 else '—',
                      max_size([v],context='designer'),max_size([v])) for v in variants]),
              '## Resource-specific class clearance',
              'A worst-of-size decision can hide smaller weapons that fit. This table gives the minimum combat '
              'gap over every appearance for each native resource on each class, independent of current UI legality. '
              'Single-node classes and simple-hull special cases are left to their detailed records.']
    for alien in (False,True):
        names = [name for name,vs in classes.items() if vs[0]['alien']==alien and vs[0]['ui_hardpoints']>1 and not vs[0]['simple']]
        eligible = [g for g in d['geometries'] if any(w['alien']==alien for w in resources[g['resource']])]
        clear.append(table(['Art (gap m)']+names,
              [(g['resource'].split('/')[-1], *[num(min(v['ordinary_min_distance_m']-2*g['swept_radius_m'] for v in classes[name]))
                for name in names]) for g in eligible]))
    save('class-clearance.md',clear)

    nodes_doc = ['# Hull controller, extra-node and coordinate inventory',
        'One logical hull weapon may be represented by two visible turrets. Dorsal plus ventral counts must '
        'not be added when estimating weapon capacity. Indices, not object names, select native controllers. '
        '“Extra” below means a controller index outside the set used by the UI’s ordinary OneHull placements.',
        '## All appearances: declared versus authored nodes',
        table(['Class/art','UI cells','Dorsal','Ventral','Distinct indices','Extra indices','All-node nearest m','Ordinary nearest m'],
              [(f"{v['hull']}/{v['appearance']}",v['ui_hardpoints'],
                sum(n['side']=='dorsal' for n in v['nodes']),sum(n['side']=='ventral' for n in v['nodes']),
                len({n['index'] for n in v['nodes']}),
                ','.join(map(str, sorted({n['index'] for n in v['nodes']} - set(v['ordinary_indices'])))) or 'none',
                num(v['all_min_distance_m']), num(v['ordinary_min_distance_m'])) for v in variants]),
        '## Extra-node candidates and padding consequences',
        'AlienDestroyer has a middle alternative mount between two ordinary mounts. AlienDreadnought and '
        'AlienMothership have central merge nodes within much tighter left/right groups. A class with one alien '
        'appearance is consistent across its installed inventory, but this does not imply unseen variants. '
        'No surveyed human class has additional hull-controller indices beyond its UI node count.']
    for name,vs in classes.items():
        if not any(len({n['index'] for n in v['nodes']})>v['ui_hardpoints'] for v in vs):
            continue
        extra=sorted({n['index'] for n in vs[0]['nodes']}-set(vs[0]['ordinary_indices']))
        reachable=sorted(set(extra)&set(vs[0]['multi_indices']))
        nodes_doc += [f'### {name}',
            f'Extra authored indices: **{extra}**. Of these, legal native multi-cell placements select '
            f'**{reachable}**; the remainder are authored alternatives not reached by the audited legal '
            'multi-cell grid mappings. All remain hypothetical candidates for a new explicit mapping.',
            table(['Context','Size','Padding m','Padding ratio','Ordinary pass','All nodes pass','Max count preserving ordinary','Additional nodes','Selected indices'],
                  [(context,s,a,f'{r:.0%}',str(f['ordinary_pass']),str(f['all_pass']),
                    len(f['expanded_indices']) if f['expanded_indices'] is not None else 'infeasible ordinary set',
                    len(f['expanded_indices'])-vs[0]['ui_hardpoints'] if f['expanded_indices'] is not None else '—',
                    ','.join(map(str,f['expanded_indices'])) if f['expanded_indices'] is not None else '—')
                   for context in ('combat','designer') for s in (1,2,4) for a,r in d['paddings']
                   for f in [fit(vs[0],s,a,r,context=context)]])]
    anomalies=[]
    for v in variants:
        for a,b in itertools.combinations(v['nodes'],2):
            if a['index']!=b['index'] and math.dist(a['position_m'],b['position_m'])<1e-5:
                anomalies.append((f"{v['hull']}/{v['appearance']}",a['side'],a['index'],b['side'],b['index'],
                                  ', '.join(num(x,3) for x in a['position_m']),a['controller_path_id'],b['controller_path_id']))
    nodes_doc += ['## Exact duplicate positions',
                 table(['Class/art','Side A','Index A','Side B','Index B','Position m','Controller A','Controller B'],anomalies),
                 'The duplicate Mothership objects have different controller IDs. This is not just one object '
                 'listed twice. No correction or repositioning was made. Even if this pair were corrected, '
                 'the 8 m ordinary central spacing and 4 m merge spacing would still need separate evaluation.',
                 '## Complete node coordinates',
                 'Every physical controller is included, including inactive alternative mounts. Coordinates are '
                 'relative to prefab scene translation; paths and controller IDs allow direct source inspection.']
    for name,vs in classes.items():
        nodes_doc += [f'### {name}',
                     table(['Art','Side','Index','Ordinary?','X m','Y m','Z m','Object'],
                           [(v['appearance'],n['side'],n['index'],'yes' if n['ordinary'] else 'extra',
                             *[num(x,3) for x in n['position_m']],n['path'].split('/')[-1]) for v in vs for n in v['nodes']])]
    save('node-inventory.md',nodes_doc)

    distances = ['# Complete same-face hull-node distance matrices',
                 'Distances are centre-to-centre metres, including all alternative/merge controllers. '
                 'An asterisk marks an extra index absent from the ordinary UI mapping. '
                 'The upper triangle is sufficient because distances are symmetric. These are physical '
                 'node distances, before subtracting weapon envelopes. Cross-face pairs are also checked '
                 'by the clearance engine; their minimum is listed beneath each appearance.']
    for v in variants:
        distances.append(f"## {v['hull']} / appearance {v['appearance']}")
        for side in ('dorsal','ventral'):
            ns=[n for n in v['nodes'] if n['side']==side]
            if not ns:
                distances.append(f'{side.title()}: no hull nodes.')
                continue
            labels=[str(n['index'])+('' if n['ordinary'] else '*') for n in ns]
            distances += [f'### {side.title()}',table(['Index']+labels,
                         [(labels[i],*[num(math.dist(a['position_m'],b['position_m'])) if j>i else '—'
                           for j,b in enumerate(ns)]) for i,a in enumerate(ns)])]
        cross=[(math.dist(a['position_m'],b['position_m']),a['index'],b['index'])
               for a in v['nodes'] for b in v['nodes']
               if a['side']=='dorsal' and b['side']=='ventral' and a['index']!=b['index']]
        if cross:
            c=min(cross)
            distances.append(f'Closest cross-face distinct-index pair: dorsal {c[1]} / ventral {c[2]}: **{num(c[0],3)} m**.')
    save('node-distance-matrices.md',distances)

    families = defaultdict(list)
    for w in d['weapons']:
        if w['current'] is not None:
            families[(w['alien'],family(w))].append(w)
    dps = ['# Hull-weapon DPS and capacity deltas',
        'All outputs are **raw sustained MJ/s**: muzzle kinetic energy for guns/magnetics/plasma or emitted '
        'energy for lasers/particles. Divide by 20 for nominal damage-points/s before armor and other target effects. '
        'This is a transparent capacity comparison, not expected delivered damage or the game’s weighted `EstimateDPS`. '
        'Nose weapons are unchanged and excluded; whole-ship percentage changes would be smaller when nose output is included.',
        '## Calculation and baseline definitions',
        '`cycle = cooldown + (salvoShots − 1) × intraSalvoCooldown`; `rawDPS = shotEnergy × salvoShots / cycle`. '
        'Kinetic shot energy is `0.5 × warheadMass_kg × muzzleVelocity_kps²` MJ. This uses warhead mass, not loaded '
        'ammo mass. Native `averageCooldown_s` and `TIGunTypeWeaponTemplate.BaseDamageAtRange_MJ` supply these rules. '
        'Laser/particle shotPower is emitted energy; no range loss, hit chance, penetration, chipping, PD interception, '
        'relative projectile velocity, officers, ammo depletion or cooling/power downtime is simulated.',
        '**Stock** uses installed vanilla weapon statistics and stock grid footprints. **Current** uses repository '
        'merged weapon statistics, the same adjacency packing, and the already implemented FourHull-per-node policy '
        'for Titan, AlienTitan and AlienMothership. Packing is an exact maximum over mixed sizes within one weapon '
        'family and tier, allowing small weapons to fill remaining cells. Current baseline is a hypothetical homogeneous '
        'family design, not a claim that the AI selects it. Families without size 4 have zero eligible current '
        'capital loadout. **2/node** and **4/node** each put that size on all ordinary logical nodes; '
        'these are hypothetical fixed-size loadouts, not an “up to size” optimization. Deltas compare against Current. '
        'A geometry failure does not remove a DPS row: feasibility is reported separately.',
        '## Same-family size scaling',
        table(['Family','Faction','D1','D2','D4','D2 vs D1','D4 vs D2','D4 vs D1','D2 / 2D1','D4 / 4D1'],
              [(name,'Alien' if alien else 'Human',*[num(by.get(s,{}).get('current',{}).get('raw_MJ_s')) for s in (1,2,4)],
                pct(by.get(2,{}).get('current',{}).get('raw_MJ_s'),by.get(1,{}).get('current',{}).get('raw_MJ_s')),
                pct(by.get(4,{}).get('current',{}).get('raw_MJ_s'),by.get(2,{}).get('current',{}).get('raw_MJ_s')),
                pct(by.get(4,{}).get('current',{}).get('raw_MJ_s'),by.get(1,{}).get('current',{}).get('raw_MJ_s')),
                num(by[2]['current']['raw_MJ_s']/(2*by[1]['current']['raw_MJ_s'])) if 1 in by and 2 in by else '—',
                num(by[4]['current']['raw_MJ_s']/(4*by[1]['current']['raw_MJ_s'])) if 1 in by and 4 in by else '—')
               for (alien,name),ws in families.items() for by in [{w['size']:w for w in ws}]])]
    dps += ['## Slot-packing and nominal capacity',
            'Counts are weapons, not capacity units. A size-4 weapon uses four ordinary cells under stock rules '
            'only when those cells form the legal rectangle. A 3-cell hull therefore need not fit any size 4. '
            'The capital override retains the logical node count but permits size 4 at each node.',
            table(['Class','Tier','Nodes H','Stock 1H','Stock 2H horizontal','Stock 2H vertical','Stock 4H','Current 4H','Proposed size-2 units','Proposed size-4 units'],
                  [(name,vs[0]['tier'],vs[0]['ui_hardpoints'],vs[0]['packing']['OneHull'],vs[0]['packing']['TwoHullHoriz'],
                    vs[0]['packing']['TwoHullVert'],vs[0]['packing']['FourHull'],
                    vs[0]['ui_hardpoints'] if name in CAPITALS else vs[0]['packing']['FourHull'],
                    vs[0]['ui_hardpoints']*2,vs[0]['ui_hardpoints']*4) for name,vs in classes.items()]),
            '## Tier-2 example: Mk3 coilgun batteries',
            'For the tier-2 examples Destroyer and Cruiser, medium Mk3 coil output is 139.21875 MJ/s per weapon '
            'and heavy is 350.659375 MJ/s. Both classes pass the native size-4 5 m conservative gap test in every appearance. '
            'The per-class tables below quantify output against the best current mixed-footprint family loadout; '
            'this differs from a comparison against all-small guns.']
    class_dps=[]
    for name,vs in classes.items():
        v=vs[0]
        rows=[]
        for (alien,fname),ws in families.items():
            if alien != v['alien']:
                continue
            by={w['size']:w for w in ws}
            stock, stock_names=best_packed(v,ws,'vanilla')
            current, current_names=best_packed(v,ws,'current')
            if name in CAPITALS:
                current=v['ui_hardpoints']*by[4]['current']['raw_MJ_s'] if 4 in by else 0.
                current_names=(by[4]['name'],)*v['ui_hardpoints'] if 4 in by else ()
            two=v['ui_hardpoints']*by[2]['current']['raw_MJ_s'] if 2 in by else None
            four=v['ui_hardpoints']*by[4]['current']['raw_MJ_s'] if 4 in by else None
            mass_current=sum(next(w['current']['mass_t'] for w in ws if w['name']==n) for n in current_names)
            mass2=v['ui_hardpoints']*by[2]['current']['mass_t'] if 2 in by else None
            mass4=v['ui_hardpoints']*by[4]['current']['mass_t'] if 4 in by else None
            rows.append((fname,num(stock),num(current),pct(current,stock),num(two),pct(two,current),num(four),pct(four,current)))
            class_dps.append({'hull':name,'family':fname,'stock_raw_MJ_s':stock,'current_raw_MJ_s':current,
                              'size2_raw_MJ_s':two,'size4_raw_MJ_s':four,'current_weapons':list(current_names),
                              'stock_weapons':list(stock_names),'current_mass_t':mass_current,'size2_mass_t':mass2,'size4_mass_t':mass4})
        dps += [f'### {name}: {v["ui_hardpoints"]} ordinary nodes, tier {v["tier"]}',
                f'All-appearance conservative maximum at 5 m: **{max_size(vs)}**. '
                + ('Already FourHull-only per node; size-2 rows would require reversing that restriction.' if name in CAPITALS else 'Stock grid footprints remain the current fitting rule.'),
                table(['Family','Stock MJ/s','Current MJ/s','Current vs stock','2/node MJ/s','2/node Δ','4/node MJ/s','4/node Δ'],rows)]
    dps += ['## Mount expansion: AlienDestroyer',
            'The middle alternative node changes logical capacity from 2 to 3 (+50%). Its minimum all-node spacing '
            'is 21 m, versus 44 m for the two ordinary nodes. The full-rotation 5 m test passes for all three native '
            'size categories. The table multiplies the same weapon by three; it does not double dorsal/ventral graphics.',
            table(['Family','Size','Two nodes MJ/s','Three nodes MJ/s','Gain MJ/s','Gain','Three-node mass t'],
                  [(name,s,num(2*w['current']['raw_MJ_s']),num(3*w['current']['raw_MJ_s']),num(w['current']['raw_MJ_s']),'+50.0%',num(3*w['current']['mass_t']))
                   for (alien,name),ws in families.items() if alien for w in ws for s in [w['size']]]),
            'AlienDreadnought 8→12 and Mothership 16→20 are arithmetic +50% and +25% ceilings at fixed weapon size, '
            'respectively. They are **not clearance-certified capacity gains**: the ordinary sets already fail the '
            'conservative tests, and central merge nodes tighten spacing further. See [extra-node scenarios](node-inventory.md).',
            '## Weapon mass changes for representative advanced families',
            'Weapon-only dry mass excludes ammo, reactor, radiator, battery and crew-support changes. It is a '
            'lower-scope accounting comparison, not a complete ship mass or delta-v calculation.',
            table(['Class','Family','Current t','2/node t','2/node Δt','4/node t','4/node Δt'],
                  [(r['hull'],r['family'],num(r['current_mass_t']),num(r['size2_mass_t']),
                    num(r['size2_mass_t']-r['current_mass_t']) if r['size2_mass_t'] is not None else '—',num(r['size4_mass_t']),
                    num(r['size4_mass_t']-r['current_mass_t']) if r['size4_mass_t'] is not None else '—') for r in class_dps
                   if r['family'] in ('CoilgunBatteryMk3','UVPhaserBattery','Gen3AlienMagBattery','AlienVioletLaserBattery')]),
            '## Combat interpretation',
            'Larger laser apertures can change armor penetration without a proportionate raw-MJ/s gain; particles '
            'have range/dispersion effects; magnetic shot velocity, packet damage, ammo endurance and PD survival '
            'matter independently. Plasma and early rail sizes can have worse footprint-normalized raw output. '
            'Point-defense-only families are inventoried but cannot be treated as anti-ship batteries. Missiles '
            'are inventoried geometrically, but no nonexistent size-2/4 family is synthesized. Their delivered damage '
            'depends on warhead and terminal velocity, so no misleading cross-family missile DPS ranking is provided. '
            'Nose output, fitting legality, targeting behavior, simultaneous engagement arcs, reactor demand, cooling, '
            'ammo and total ship economics remain required checks for any later balance implementation.']
    save('dps-analysis.md',dps)
    (output/'dps-scenarios.json').write_text(json.dumps(class_dps,indent=2,allow_nan=False)+'\n',encoding='utf-8')


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-root',type=Path,default=Path(__file__).resolve().parents[2]/'docs/ship-balance-research/turret-clearance')
    parser.add_argument('--hull',help='Inspect one hull in the saved evidence instead of rewriting reports')
    parser.add_argument('--appearance',type=int,help='Optional appearance index for --hull')
    args=parser.parse_args()
    evidence=json.loads((args.output_root/'measurements.json').read_text(encoding='utf-8'))
    if args.hull:
        selected=[v for v in evidence['variants'] if v['hull'].lower()==args.hull.lower()
                  and (args.appearance is None or v['appearance']==args.appearance)]
        if not selected:
            parser.error('No matching hull/appearance in the evidence')
        print(json.dumps(selected,indent=2))
    else:
        if args.appearance is not None:
            parser.error('--appearance requires --hull')
        write_reports(evidence,args.output_root)
