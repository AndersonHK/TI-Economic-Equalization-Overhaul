"""Numerical and evidence checks for the offline turret study.

Uses only the standard library unless --geometry is passed, which tests the
actual extractor's arbitrary-rotation bound and exact subset search with NumPy.
"""
import hashlib
import itertools
import json
import math
from pathlib import Path
import sys
import unittest

from report_turret_clearance import best_packed, fit, family

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/ship-balance-research/turret-clearance'
GEOMETRY='--geometry' in sys.argv
if GEOMETRY:
    sys.argv.remove('--geometry')


class EvidenceChecks(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.d=json.loads((OUT/'measurements.json').read_text(encoding='utf-8'))
        cls.scenarios=json.loads((OUT/'dps-scenarios.json').read_text(encoding='utf-8'))

    def test_source_hashes(self):
        for source in self.d['sources']:
            h=hashlib.sha256()
            with open(source['path'],'rb') as f:
                for block in iter(lambda:f.read(1024*1024),b''):
                    h.update(block)
            self.assertEqual(h.hexdigest().upper(),source['sha256'],source['path'])

    def test_installed_inventory_coverage(self):
        source=next(s['path'] for s in self.d['sources'] if 'StreamingAssets' in s['path'] and s['path'].endswith('TIShipHullTemplate.json'))
        hulls=json.loads(Path(source).read_text(encoding='utf-8-sig'))
        expected={(h['dataName'],i) for h in hulls for i in range(len(h['modelResource']))}
        actual={(v['hull'],v['appearance']) for v in self.d['variants']}
        self.assertEqual(expected,actual)
        self.assertEqual(len(actual),len(self.d['variants']))
        expected_weapons=[]
        for source in self.d['sources']:
            if 'StreamingAssets' in source['path'] and Path(source['path']).stem in {
                'TIGunTemplate','TIMagneticGunTemplate','TILaserWeaponTemplate','TIParticleWeaponTemplate','TIPlasmaWeaponTemplate','TIMissileTemplate'}:
                expected_weapons.extend(w['dataName'] for w in json.loads(Path(source['path']).read_text(encoding='utf-8-sig'))
                                        if w.get('mount') in {'HalfHull','OneHull','TwoHullHoriz','TwoHullVert','FourHull'})
        self.assertEqual(set(expected_weapons),{w['name'] for w in self.d['weapons']})
        self.assertEqual(len(expected_weapons),len(self.d['weapons']))

    def test_native_maps_positions_and_scope(self):
        maps=json.loads((OUT/'mount-maps.json').read_text(encoding='utf-8'))['controllers']
        for v in self.d['variants']:
            ordinary={maps[v['controller']]['OneHull'][i] for i in v['slot_indices']}
            self.assertEqual(ordinary,set(v['ordinary_indices']))
            self.assertEqual(len(ordinary),v['ui_hardpoints'])
            self.assertEqual(len(v['fits']),84)
            self.assertTrue(all(abs(n['parent_max_scale']-1)<1e-5 for n in v['nodes']))
            ps=[math.dist(a['position_m'],b['position_m']) for a,b in itertools.combinations(v['nodes'],2)
                if a['ordinary'] and b['ordinary'] and a['index']!=b['index']]
            if ps:
                self.assertAlmostEqual(min(ps),v['ordinary_min_distance_m'],places=10)
            else:
                self.assertIsNone(v['ordinary_min_distance_m'])
        # Actual asset anomaly must survive serialization, not be deduplicated away.
        mothership=next(v for v in self.d['variants'] if v['hull']=='AlienMothership')
        ns={n['index']:n for n in mothership['nodes'] if n['side']=='ventral'}
        self.assertNotEqual(ns[1]['controller_path_id'],ns[18]['controller_path_id'])
        self.assertEqual(ns[1]['position_m'],ns[18]['position_m'])

    def test_stock_hull_grid_same_as_current(self):
        paths=[Path(s['path']) for s in self.d['sources'] if s['path'].endswith('TIShipHullTemplate.json')]
        stock=json.loads(paths[0].read_text(encoding='utf-8-sig'))
        overrides={h['dataName']:h for h in json.loads(paths[1].read_text(encoding='utf-8-sig'))}
        for h in stock:
            cur=dict(h,**overrides.get(h['dataName'],{}))
            extract=lambda x:[(i,s['x'],s['y']) for i,s in enumerate(x['shipModuleSlots']) if s['moduleSlotType']=='HullHardPoint']
            self.assertEqual(extract(h),extract(cur))

    def test_kinetic_energy_and_salvo_cadence(self):
        for w in self.d['weapons']:
            if w['current'] is None:
                continue
            t=w['template']; c=w['current']; n=t.get('salvo_shots',1)
            cycle=t['cooldown_s']+(n-1)*t.get('intraSalvoCooldown_s',0)
            if w['kind'] in {'TIGunTemplate','TIMagneticGunTemplate','TIPlasmaWeaponTemplate'}:
                joules=.5*t['warheadMass_kg']*(1000*t['muzzleVelocity_kps'])**2
                energy=joules/1000000
            else:
                energy=t['shotPower_MJ']
            self.assertAlmostEqual(c['raw_MJ_s'],energy*n/cycle,places=8)
        w=next(w for w in self.d['weapons'] if w['name']=='HeavyCoilgunBatteryMk3')
        self.assertAlmostEqual(w['current']['raw_MJ_s'],350.659375)

    def test_padding_monotonicity_and_extra_counts(self):
        for v in self.d['variants']:
            for context,scope,size in itertools.product(('combat','designer'),('native','all'),(1,2,4)):
                fs=[fit(v,size,a,0,scope,context) for a in (0,2,5,10)]
                self.assertEqual([f['ordinary_pass'] for f in fs],sorted([f['ordinary_pass'] for f in fs],reverse=True))
                for f in fs:
                    if f['expanded_indices'] is not None:
                        self.assertTrue(set(v['ordinary_indices'])<=set(f['expanded_indices']))
                        self.assertTrue(set(f['expanded_indices'])<={n['index'] for n in v['nodes']})
        v=next(v for v in self.d['variants'] if v['hull']=='AlienDestroyer')
        self.assertEqual(fit(v,4)['expanded_indices'],[0,1,2])

    def test_packing_with_known_adjacency_and_mixed_sizes(self):
        v={'legal_placements':{'OneHull':[{'cells':[(0,0)]},{'cells':[(1,0)]},{'cells':[(2,0)]}],
                              'TwoHullHoriz':[{'cells':[(0,0),(1,0)]},{'cells':[(1,0),(2,0)]}]}}
        ws=[{'mount':'OneHull','name':'small','current':{'raw_MJ_s':3}},
            {'mount':'TwoHullHoriz','name':'medium','current':{'raw_MJ_s':10}}]
        value,names=best_packed(v,ws,'current')
        self.assertEqual(value,13)
        self.assertEqual(sorted(names),['medium','small'])

    def test_capital_current_baseline_not_double_counted(self):
        for r in self.scenarios:
            if r['hull'] in {'Titan','AlienTitan','AlienMothership'}:
                self.assertEqual(r['current_raw_MJ_s'],r['size4_raw_MJ_s'] or 0)
        cruiser=next(r for r in self.scenarios if r['hull']=='Cruiser' and r['family']=='CoilgunBatteryMk3')
        self.assertAlmostEqual(cruiser['current_raw_MJ_s'],204.22916666666669)
        self.assertAlmostEqual(cruiser['size2_raw_MJ_s'],417.65625)

    def test_report_links_and_tables(self):
        import re
        for p in OUT.glob('*.md'):
            text=p.read_text(encoding='utf-8')
            for link in re.findall(r'\]\(([^)]+)\)',text):
                if not link.startswith(('http','#')):
                    self.assertTrue((p.parent/link.split('#')[0]).exists(),(p.name,link))
            width=None
            for line in text.splitlines():
                if line.startswith('|'):
                    cells=line.count('|')
                    if width is None: width=cells
                    self.assertEqual(width,cells,p.name)
                else: width=None


@unittest.skipUnless(GEOMETRY,'Pass --geometry with NumPy/UnityPy available')
class GeometryChecks(unittest.TestCase):
    def test_bound_contains_rotated_nonuniform_meshes(self):
        import numpy as np
        from measure_turret_clearance import rotation_envelope
        rng=np.random.default_rng(20261020)
        for _ in range(80):
            base=rng.normal(size=(20,3)); moving=rng.normal(size=(30,3))
            offset=rng.normal(size=3); scale=rng.uniform(.05,3,size=3)
            bound=rotation_envelope(base,moving,offset,scale)
            for _ in range(30):
                q,_=np.linalg.qr(rng.normal(size=(3,3)))
                posed=(offset+moving@q.T)*scale
                self.assertLessEqual(float(np.linalg.norm(posed,axis=1).max()),bound+1e-10)

    def test_extra_subset_exact_against_exhaustive_search(self):
        import random
        from measure_turret_clearance import extra_subset
        rng=random.Random(42)
        for _ in range(60):
            n=8; ordinary=[0,1]
            edges={pair for pair in itertools.combinations(range(n),2) if rng.random()<.25}
            pairs=[{'a':a,'b':b,'gap_m':0,'gap_ratio':0} for a,b in edges]
            got=extra_subset(ordinary,list(range(n)),pairs,1,0)
            valid=[]
            for mask in range(1<<n):
                chosen={i for i in range(n) if mask&(1<<i)}
                if set(ordinary)<=chosen and not any(a in chosen and b in chosen for a,b in edges):valid.append(chosen)
            self.assertEqual(len(got) if got is not None else None,max(map(len,valid),default=None))


if __name__=='__main__':
    unittest.main(verbosity=2)
