# Reproduce the canonical-size projections

2026-10-21. Research recipes only; no runtime or installed asset writes.
The original audit and later aperture evidence remain the inputs. The original
35-resource geometry needed for exact preview reconstruction is retained in
`source-hull-geometry.npz`, so regeneration does not require the temporary
UnityPy extraction environment. The separate barrel/lens NPZ is also retained
in its original evidence directory. All files are local to this repository.

Use Python with NumPy and Pillow. Extract the following code blocks into the
listed `.tmp/canonical-turrets/` files and run from the repository root:

```powershell
python .tmp/canonical-turrets/project.py
python .tmp/canonical-turrets/reports.py
python .tmp/canonical-turrets/legal_layouts.py
python .tmp/canonical-turrets/plot.py
python .tmp/canonical-turrets/validate.py
```

The existing `scripts/ship-balance/report_turret_clearance.py` supplies original
fit/packing helpers and is hash-verified. No shipping tooling was modified.
`project.py` writes only the new projection directory and a temporary runtime
preservation manifest. Original research JSON and reports are never overwritten.
The main plan, methodology and report-set README are authored commentary rather
than generated outputs. Archiving the 25 mm plan was a one-time documentation
move with relative links updated; it is not rerun by these recipes.

## `.tmp/canonical-turrets/project.py`

```python
"""Offline planning projection; never writes gameplay or original audit inputs."""
import json,math,hashlib,itertools,copy,sys,re,shutil
from pathlib import Path
from collections import defaultdict
from functools import lru_cache
import numpy as np
R=Path.cwd();B=R/'docs/ship-balance-research';O=B/'canonical-turret-projection';O.mkdir(exist_ok=True)
T=R/'.tmp/canonical-turrets';sys.path.insert(0,str(R/'scripts/ship-balance'))
import report_turret_clearance as report
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
OLD=read(B/'turret-clearance/measurements.json');INS=read(B/'turret-clearance/weapon-mesh-inspection-2026-10-21.json');DIM=read(B/'barrel-lens-audit/dimensions.json');INV=read(B/'barrel-lens-audit/inventory.json');FIX=read(B/'turret-clearance/mothership-layout-2026-10-21.json')
PRE=O/'source-hull-geometry.npz'
if not PRE.exists():shutil.copyfile(R/'.tmp/mothership-fix/weapon-scenes.npz',PRE)
DATA=np.load(PRE);NEW=np.load(B/'barrel-lens-audit/measured-geometry.npz')
G={g['resource']:g for g in OLD['geometries']};I={i['resource']:i for i in INS};DM={d['resource']:d for d in DIM};RES={r['resource']:r for r in INV['resources']}
WEAPONS=OLD['weapons'];W={w['name']:w for w in WEAPONS};FULL={w['dataName']:w for w in INV['weapons']}
sources={str(p.relative_to(R)):sha(p) for p in [B/'turret-clearance/measurements.json',B/'turret-clearance/class-clearance.md',B/'turret-clearance/weapon-inventory.md',B/'turret-clearance/node-inventory.md',B/'turret-clearance/dps-analysis.md',B/'turret-clearance/weapon-mesh-inspection-2026-10-21.json',B/'turret-clearance/mothership-layout-2026-10-21.json',B/'barrel-lens-audit/inventory.json',B/'barrel-lens-audit/dimensions.json',B/'barrel-lens-audit/measured-geometry.npz',R/'TIEconomyMod/Core/CapitalMountPolicy.cs',R/'TIEconomyMod/Core/MothershipMountGeometry.cs',R/'TIEconomyMod/Patches/MothershipMountPatches.cs']}
sources[str(PRE.relative_to(R))]=sha(PRE)
before={str(p.relative_to(R)):sha(p) for p in (R/'TIEconomyMod').rglob('*') if p.is_file() and p.suffix in ['.cs','.json','.dll','.csproj'] and not any(n in p.parts for n in ['bin','obj','.vs'])}
if not (T/'runtime-before.json').exists():(T/'runtime-before.json').write_text(json.dumps(before,indent=2))
(O/'input-hashes.json').write_text(json.dumps(sources,indent=2)+'\n')
def rotation(q):
 x,y,z,w=np.array(q)/np.linalg.norm(q)
 return np.array([[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],[2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],[2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]])
raw={}
for resource,g in G.items():
 i=I[resource];base=DATA['v'+i['meshes'][0]['key']];upper=DATA['v'+i['meshes'][1]['key']];S=np.array(g['root_scale']);C=np.array(g['child_scale']);p=np.array(g['child_position']);rot=rotation(i['source_child_rotation_xyzw'])
 b=base/S;v=((upper/S-p)@rot)/C
 calc=max(np.linalg.norm(b*S,axis=1).max(),np.linalg.norm(S*p)+max(abs(S))*np.linalg.norm(v*C,axis=1).max());assert abs(calc-g['swept_radius_m'])<1e-6,(resource,calc,g['swept_radius_m'])
 raw[resource]=(b,v,p,S,C,base,upper)
def proxy(w):return (4*w['ammoMass_kg']/(math.pi*19300*10))**(1/3)
def rule(w,scenario='canonical'):
 resource=w['modelResource'];d=DM.get(resource);kind=w['kind'];n=w['dataName']
 if kind=='TIGunTemplate':target=w['projectileDiameter_mm']/1000;measured=d['opening_inscribed_m'];return target/measured,'declared caliber / inscribed muzzle',target,measured
 if kind=='TILaserWeaponTemplate':
  # Numerical cm names match the native radius field; user selected simulation physics.
  target=w['mirrorRadius_cm']/50
  if scenario=='named_diameter' and 'PointDefense' not in n:target/=2
  return target/d['aperture_m'],'native optical diameter' if scenario!='named_diameter' else 'rejected name-as-diameter sensitivity',target,d['aperture_m']
 if kind=='TIMagneticGunTemplate':
  if 'opening_inscribed_m' not in d:return 1.,'closed rail face: bore unresolved',proxy(w),None
  target=proxy(w);measured=d['opening_inscribed_m'];s=target/measured
  if scenario=='mag_relative':
   alien='alien_' in resource;family='coil' if 'coilgun' in resource else 'rail';mount='battery' if 'battery' in resource else 'cannon';key='shipweapons/'+('alien_' if alien else '')+'small_'+family+'gun_'+mount
   anchor=DM.get(key)
   ws=[x for x in INV['weapons'] if x.get('modelResource')==key]
   if anchor and 'opening_inscribed_m' in anchor:s*=anchor['opening_inscribed_m']/min(proxy(x) for x in ws)
  return s,'derived magnetic proxy' if scenario!='mag_relative' else 'small-art anchored magnetic mass ratios',target,measured
 return 1.,'no canonical aperture; retained',None,None
def scale(w,scenario):
 if scenario=='baseline':return 1.
 return rule(dict(w['template'],kind=w['kind']),scenario)[0]
@lru_cache(None)
def radius_parts(resource,S,C):
 b,v,p,_,_,_,_=raw[resource];S=np.array(S);C=np.array(C)
 return float(np.linalg.norm(b*S,axis=1).max()),float(np.linalg.norm(p*S)),float(max(abs(S))*np.linalg.norm(v*C,axis=1).max())
def radius(w,scenario,node=None,preview=False):
 if (scenario=='compact_optics' or scenario.startswith('retier_')) and w['kind']=='TILaserWeaponTemplate' and w['alien']:
  base=raw[w['resource']][5];D=w['template']['mirrorRadius_cm']/50;hr=.6*D;h=max(0,float(base[:,1].max()))+hr
  return max(float(np.linalg.norm(base,axis=1).max()),h+hr)
 g=G[w['resource']];S=g['root_scale'] if not preview else node['placeholder_base_scale'];C=g['child_scale'] if not preview else node['placeholder_weapon_scale'];b,p,v=radius_parts(w['resource'],tuple(S),tuple(C));s=scale(w,scenario)
 return max(b,p+s*v) if scenario=='head_only' else s*max(b,p+v)
def size(w,scenario):
 if scenario in ['retier_rail','retier_pair'] and w['name'].startswith('HeavyRailgunBattery'):return 2
 if scenario=='retier_coil' and w['name'].startswith('HeavyCoilgunBattery'):return 2
 if scenario in ['retier_naval','retier_pair'] and w['name']=='6-inchCannon':return 2
 return w['size']
SCENARIOS=['baseline','canonical','head_only','compact_optics','named_diameter','mag_relative','retier_rail','retier_coil','retier_naval','retier_pair']
metrics=[]
for w in WEAPONS:
 g=G[w['resource']];i=I[w['resource']];b,v,p,S,C,base,upper=raw[w['resource']]
 for scenario in SCENARIOS:
  s=scale(w,scenario);newbase=base if scenario=='head_only' else base*s;newupper=(upper-p*S)*s+p*S if scenario=='head_only' else upper*s;points=np.concatenate([newbase,newupper]);dims=np.ptp(points,axis=0)
  # Uniform models preserve the earlier sampled-and-bounded horizontal radius exactly.
  horizontal=i['pitch_yaw_diameter_bound_m']*s if scenario!='head_only' else None
  if (scenario=='compact_optics' or scenario.startswith('retier_')) and w['kind']=='TILaserWeaponTemplate' and w['alien']:
   D=w['template']['mirrorRadius_cm']/50;hr=.6*D;h=max(0,float(base[:,1].max()))+hr;newbase=base
   points=np.concatenate([base,np.array(list(itertools.product([-hr,hr],[h-hr,h+hr],[-hr,hr])))]);dims=np.ptp(points,axis=0)
   horizontal=max(2*float(np.linalg.norm(base[:,[0,2]],axis=1).max()),2*hr)
  yaw=horizontal if (scenario=='compact_optics' or scenario.startswith('retier_')) and w['kind']=='TILaserWeaponTemplate' and w['alien'] else 2*float(np.linalg.norm(points[:,[0,2]],axis=1).max())
  metrics.append({'name':w['name'],'scenario':scenario,'size':size(w,scenario),'alien':w['alien'],'resource':w['resource'],'factor':s,'dimensions_m':dims.tolist(),'base_dimensions_m':np.ptp(newbase,axis=0).tolist(),'sphere_diameter_m':2*radius(w,scenario),'horizontal_diameter_m':horizontal,'yaw_pose_diameter_m':yaw,'family':report.family(w)})
(O/'weapon-projections.json').write_text(json.dumps(metrics,indent=2)+'\n')
# All ship-art inventory, including nose variants and templates that share an asset.
targets=[]
for w in INV['weapons']:
 if not w.get('modelResource'):continue
 d=DM[w['modelResource']];r=RES[w['modelResource']];s,method,target,measured=rule(w);points=np.concatenate([NEW['v'+m['key']] for m in r['meshes']]);dims=np.ptp(points,axis=0)
 targets.append({'name':w['dataName'],'kind':w['kind'],'resource':w['modelResource'],'mount':w['mount'],'factor':s,'reference':method,'target_m':target,'measured_m':measured,'before_dimensions_m':dims.tolist(),'canonical_uniform_dimensions_m':(dims*s).tolist()})
(O/'all-weapon-targets.json').write_text(json.dumps(targets,indent=2)+'\n')
def subset(ordinary,indices,pairs,absolute,relative):
 bad={tuple(sorted((p['a'],p['b']))) for p in pairs if p['gap_m']+1e-8<absolute or p['gap_ratio']+1e-8<relative}
 if any(a in ordinary and b in ordinary for a,b in bad):return None
 candidates=[i for i in indices if i not in ordinary and not any(tuple(sorted((i,j))) in bad for j in ordinary)];best=[]
 for n in range(len(candidates),-1,-1):
  for c in itertools.combinations(candidates,n):
   if all(tuple(sorted((a,b))) not in bad for a,b in itertools.combinations(c,2)):return sorted(set(ordinary)|set(c))
def project_variant(v,scenario,layout):
 v=copy.deepcopy(v);nodes=v['nodes']
 if layout=='current' and v['hull']=='AlienMothership':
  fix={r['index']:r for r in FIX['rows']}
  for n in nodes:n['position_m']=fix[n['index']][n['side']]
 fits=[]
 for context,scope,siz in itertools.product(['combat','designer'],['native','all'],[1,2,4]):
  eligible=[w for w in WEAPONS if size(w,scenario)==siz and (scope=='all' or w['alien']==v['alien'])];radii={};limiting={}
  for n in nodes:
   preview=context=='designer' or v['simple'];vals=[(radius(w,scenario,n,preview)*n['parent_max_scale'],w['name']) for w in eligible];k=(n['side'],n['index']);radii[k],limiting[k]=max(vals)
  pairs=[]
  for a,b in itertools.combinations(nodes,2):
   if a['index']==b['index']:continue
   ka=(a['side'],a['index']);kb=(b['side'],b['index']);dist=float(np.linalg.norm(np.array(a['position_m'])-b['position_m']));diam=radii[ka]+radii[kb];gap=dist-diam
   pairs.append({'a':a['index'],'b':b['index'],'sides':a['side']+'/'+b['side'],'distance_m':dist,'gap_m':gap,'gap_ratio':gap/diam,'limiting_a':limiting[ka],'limiting_b':limiting[kb]})
  opairs=[p for p in pairs if p['a'] in v['ordinary_indices'] and p['b'] in v['ordinary_indices']];indices=sorted({n['index'] for n in nodes})
  for absolute,relative in OLD['paddings']:
   passed=lambda ps:all(p['gap_m']+1e-8>=absolute and p['gap_ratio']+1e-8>=relative for p in ps)
   fits.append({'context':context,'scope':scope,'size':siz,'padding_m':absolute,'padding_ratio':relative,'ordinary_pass':bool(v['ordinary_indices']) and passed(opairs),'all_pass':bool(indices) and passed(pairs),'expanded_indices':subset(v['ordinary_indices'],indices,pairs,absolute,relative),'ordinary_min_gap_m':min((p['gap_m'] for p in opairs),default=None),'all_min_gap_m':min((p['gap_m'] for p in pairs),default=None),'limiting_pair':min(opairs,key=lambda p:p['gap_m']) if opairs else None})
 v['fits']=fits;return v
projections={};max_error=0;checked=0
for scenario in SCENARIOS:
 out=[]
 for vi,v in enumerate(OLD['variants']):
  new=project_variant(v,scenario,'native');out.append(new)
  if scenario=='baseline':
   for oldfit,newfit in zip(v['fits'],new['fits']):
    assert all(oldfit[k]==newfit[k] for k in ['ordinary_pass','all_pass','expanded_indices']), (v['hull'],v['appearance'],oldfit,newfit)
    if oldfit['ordinary_min_gap_m'] is not None:max_error=max(max_error,abs(oldfit['ordinary_min_gap_m']-newfit['ordinary_min_gap_m']))
    checked+=1
 projections[scenario]=out
 (O/(scenario+'-measurements.json')).write_text(json.dumps({'scenario':scenario,'layout':'original native asset nodes','paddings':OLD['paddings'],'variants':out},separators=(',',':'))+'\n')
 print('Completed',scenario,flush=True)
current={s:project_variant(next(v for v in OLD['variants'] if v['hull']=='AlienMothership'),s,'current') for s in SCENARIOS}
(O/'current-mothership.json').write_text(json.dumps(current,indent=2)+'\n')
(O/'projection-checks.json').write_text(json.dumps({'baseline_fit_records_reproduced':checked,'baseline_max_gap_error_m':max_error,'source_resources':35,'hull_templates':147,'classes':28,'appearances':64,'all_ship_targets':len(targets),'scenarios':SCENARIOS},indent=2)+'\n')
assert max_error<1e-6

```

## `.tmp/canonical-turrets/reports.py`

```python
import json,math,sys,copy,itertools,re
from pathlib import Path
from collections import defaultdict
from functools import lru_cache
import numpy as np
R=Path.cwd();B=R/'docs/ship-balance-research';O=B/'canonical-turret-projection';T=R/'.tmp/canonical-turrets';sys.path.insert(0,str(R/'scripts/ship-balance'));import report_turret_clearance as oldr
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
OLD=read(B/'turret-clearance/measurements.json');M=read(O/'weapon-projections.json');TARGETS=read(O/'all-weapon-targets.json');CHECKS=read(O/'projection-checks.json');SC=CHECKS['scenarios'];P={s:read(O/(s+'-measurements.json'))['variants'] for s in SC};FIX=read(O/'current-mothership.json');W={w['name']:w for w in OLD['weapons']};MM={(x['scenario'],x['name']):x for x in M};CL=list(dict.fromkeys(v['hull'] for v in OLD['variants']));CAP=oldr.CAPITALS
def table(h,r):return oldr.table(h,r)+'\n'
def n(x,k=3):return '—' if x is None else f'{x:.{k}f}'
def delta(a,b):return '—' if a is None or b is None else f'{a-b:+.3f}'
def percent(a,b):return '—' if not b else f'{(a/b-1)*100:+.1f}%'
def save(name,parts):(O/name).write_text('\n\n'.join(parts).rstrip()+'\n',encoding='utf-8')
def cl(s,c):return [v for v in P[s] if v['hull']==c]
def f(v,size,pad=(5,0),context='combat',scope='native'):return oldr.fit(v,size,*pad,scope,context)
def mx(vs,pad=(5,0),context='combat',scope='native',allnodes=False):return oldr.max_size(vs,*pad,scope,context,allnodes).replace('none certified','NC').replace('no nodes','—')
PADLABEL=['0 m','2 m','5 m','10 m','25%','50%','5 m + 25%']
heads=['# Projected class and appearance clearance','Planning snapshot, 2026-10-21. Read the [plan](../canonical-turret-scaling-plan-2026-10-21.md) and [methodology](methodology.md). This is a separate projected copy of the [original report](../turret-clearance/class-clearance.md); original coordinates, padding rules and scope are retained. Each cell reads **original → proportional canonical → compact alien optics**. `NC` is not certified, not proven clipping; `4*` has one logical mount and no neighbor test. These all-ordinary-node tables are hypothetical same-size-per-node capacity tests, not current legal fitting. Mothership native geometry remains in these tables; the corrected layout is below.']
for context,scope in itertools.product(['combat','designer'],['native','all']):
 heads+=['## '+context+' / '+scope+' inventory: all appearances must pass',table(['Class']+PADLABEL,[[c]+[' → '.join(mx(cl(s,c),pad,context,scope) for s in ['baseline','canonical','compact_optics']) for pad in OLD['paddings']] for c in CL])]
heads+=['## Every appearance: native faction numeric edge-gap deltas','The size-specific minimum is taken across all distinct logical index pairs, including opposite faces. No pair is shown as a dash. Columns remain separate because the worst weapon can change; negative bounds do not prove a mesh collision.']
for context in ['combat','designer']:
 rows=[]
 for i,v in enumerate(P['baseline']):
  for size in [1,2,4]:
   vals=[f(P[s][i],size,context=context)['ordinary_min_gap_m'] for s in ['baseline','canonical','compact_optics']];lim=f(P['canonical'][i],size,context=context)['limiting_pair'];rows.append([v['hull'],v['appearance'],size,*[n(x) for x in vals],delta(vals[1],vals[0]),delta(vals[2],vals[0]),lim['limiting_a'] if lim else '—'])
 heads+=['### '+context,table(['Class','Art','Size','Original gap m','Proportional gap m','Compact gap m','Proportional delta','Compact delta','Proportional limiting weapon'],rows)]
heads+=['## Other scenario sensitivity: combat/native, 5 m','Head-only keeps the current base and pivot, scaling only the moving assembly. It is a footprint sensitivity, not a proven mechanical fit. Name-as-diameter was rejected by the user. Magnetic-relative retains the light-art baseline rather than matching the absolute collision proxy. Retier cases inherit compact alien optics: rail/coil demote that heavy family 4 → 2; naval promotes the 6-inch 1 → 2; pair combines heavy rail and 6-inch. Every changed footprint is TwoHullHoriz.',table(['Class']+SC,[[c]+[mx(cl(s,c)) for s in SC] for c in CL])]
heads+=['## Corrected Mothership: current 0.10.4 nodes','These use the deployed 20-pair coordinates with 16 active ordinary mounts. No extra UI capacity is added. All other classes are unchanged between native and current geometry.',table(['Scenario','Size','Active-node gap m','All-20 gap m']+PADLABEL,[[s,size,n(f(v,size)['ordinary_min_gap_m']),n(f(v,size)['all_min_gap_m'])]+['pass' if f(v,size,pad)['ordinary_pass'] else 'NC' for pad in OLD['paddings']] for s,v in FIX.items() for size in [1,2,4]])]
save('class-clearance.md',heads)

heads=['# Projected weapon dimensions and size-tier distribution','Planning only. [Original weapon inventory](../turret-clearance/weapon-inventory.md), [canonical plan](../canonical-turret-scaling-plan-2026-10-21.md). The 3D sphere is the original conservative clearance metric; horizontal motion diameter is the later sampled-and-bounded footprint metric. These are not solid model widths. Proportional scaling is around the original mount origin; all optical/gameplay stats remain unchanged.','## Largest hull weapon at each size']
rows=[]
for s in ['baseline','canonical','compact_optics','head_only']:
 for alien in [False,True]:
  for size in [1,2,4]:
   x=max((x for x in M if x['scenario']==s and x['alien']==alien and x['size']==size),key=lambda x:x['sphere_diameter_m']);rows.append([s,'Alien' if alien else 'Human',size,x['name'],n(x['sphere_diameter_m']),n(x['horizontal_diameter_m']),' × '.join(n(v) for v in x['dimensions_m'])])
heads.append(table(['Scenario','Faction','Size','Limiting example','3D sphere D m','Horizontal D m','Visible W × H × L m'],rows))
heads+=['## Every original hull template, projected separately','Shared resources can require different factors for calibers or generations. Identical wavelength/mark art is still listed so no weapon silently inherits another template’s scale. Compact differs from proportional only for alien lasers.',table(['Weapon','Size','Scale','Original W/H/L m','Proportional W/H/L m','Sphere old m','Sphere new m','Sphere delta','Horizontal old m','Horizontal new m','Compact sphere m'],[[w['name'],w['size'],n(MM['canonical',w['name']]['factor'],4),' / '.join(n(v) for v in MM['baseline',w['name']]['dimensions_m']),' / '.join(n(v) for v in MM['canonical',w['name']]['dimensions_m']),n(MM['baseline',w['name']]['sphere_diameter_m']),n(MM['canonical',w['name']]['sphere_diameter_m']),delta(MM['canonical',w['name']]['sphere_diameter_m'],MM['baseline',w['name']]['sphere_diameter_m']),n(MM['baseline',w['name']]['horizontal_diameter_m']),n(MM['canonical',w['name']]['horizontal_diameter_m']),n(MM['compact_optics',w['name']]['sphere_diameter_m'])] for w in OLD['weapons']])]
heads+=['## Complete gun/magnetic/laser target ledger, including noses','Gun and magnetic targets use polygon inscribed diameter, allowing a centered circular projectile rather than only matching corner-to-corner width. Magnetic diameters are EEO collision proxies, not authored calibers. No sabot clearance is invented. Closed rail faces stay unresolved at scale 1; plasma, particles and missiles retain original art and are covered in the hull table above. Lens targets are native 2 × radius, including PD.',table(['Template','Mount','Reference','Measured m','Target m','Scale','Proportional W × H × L m'],[[t['name'],t['mount'],t['reference'],n(t['measured_m'],4),n(t['target_m'],4),n(t['factor'],4),' × '.join(n(x) for x in t['canonical_uniform_dimensions_m'])] for t in TARGETS])]
save('weapon-inventory.md',heads)

def unique_rows(s,alien=None):
 rows=[x for x in M if x['scenario']==s and (alien is None or x['alien']==alien)];seen={}
 for x in rows:seen[(x['resource'],x['size'],round(x['factor'],8))]=x
 return list(seen.values())
stats=[];inversions=[]
for s in SC:
 if s=='head_only':continue
 for alien in [False,True]:
  uu=unique_rows(s,alien)
  for size in [1,2,4]:
   xs=[x['horizontal_diameter_m'] for x in uu if x['size']==size];q=np.percentile(xs,[0,25,50,75,100]);stats.append({'scenario':s,'alien':alien,'size':size,'n':len(xs),'min':q[0],'q25':q[1],'median':q[2],'q75':q[3],'max':q[4],'max_min':q[4]/q[0],'cv':float(np.std(xs)/np.mean(xs))})
  for a,b in [(1,2),(2,4),(1,4)]:
   pairs=list(itertools.product([x for x in uu if x['size']==a],[x for x in uu if x['size']==b]));bad=sum(x['horizontal_diameter_m']>y['horizontal_diameter_m'] for x,y in pairs);inversions.append({'scenario':s,'alien':alien,'small':a,'large':b,'pairs':len(pairs),'inversions':bad,'fraction':bad/len(pairs)})
save('size-tier-coherence.md',['# Does canonical geometry tighten gameplay size tiers?','Each distinct `(resource, scale, size)` receives one vote: identical laser wavelengths and weapon marks do not inflate counts, while genuinely different caliber/Gen3 scales remain separate. Main metric is horizontal full-pitch/yaw diameter, not the 3D sphere. Static launchers retain their original conservative comparison bounds. Median and IQR describe the bulk; max/min and CV expose outliers. No universal diameter ∝ slot-count law is asserted.','## Within-tier spread',table(['Scenario','Faction','Size','Archetypes','Min m','Q25 m','Median m','Q75 m','Max m','Max/min','CV'],[[x['scenario'],'Alien' if x['alien'] else 'Human',x['size'],x['n'],*[n(x[k]) for k in ['min','q25','median','q75','max','max_min','cv']]] for x in stats]),'## Cross-tier inversions','An inversion means a lower-slot-cost art has a larger horizontal bound than a higher-slot-cost art; lower fractions indicate a more visually ordered ladder. They are descriptive comparisons, not balance scores.',table(['Scenario','Faction','Size pair','Inversions','Pairs','Fraction'],[[x['scenario'],'Alien' if x['alien'] else 'Human',f"{x['small']} / {x['large']}",x['inversions'],x['pairs'],f"{100*x['fraction']:.1f}%"] for x in inversions])])
(O/'tier-statistics.json').write_text(json.dumps({'spread':stats,'inversions':inversions},indent=2)+'\n')
paired=[];paired_spread=[]
for alien in [False,True]:
 names=[x['name'] for x in unique_rows('canonical',alien)]
 for s in [q for q in SC if q!='head_only']:
  uu=[MM[s,name] for name in names]
  for size in [1,2,4]:
   xs=[x['horizontal_diameter_m'] for x in uu if x['size']==size];paired_spread.append({'scenario':s,'alien':alien,'size':size,'n':len(xs),'median':float(np.median(xs)),'min':min(xs),'max':max(xs),'cv':float(np.std(xs)/np.mean(xs))})
  for a,b in [(1,2),(2,4),(1,4)]:
   pairs=list(itertools.product([x for x in uu if x['size']==a],[x for x in uu if x['size']==b]));bad=sum(x['horizontal_diameter_m']>y['horizontal_diameter_m'] for x,y in pairs);paired.append({'scenario':s,'alien':alien,'small':a,'large':b,'pairs':len(pairs),'inversions':bad,'fraction':bad/len(pairs)})
p=O/'size-tier-coherence.md';p.write_text(p.read_text(encoding='utf-8')+'\n## Paired-cohort comparison\n\nFor direct before/after inference, hold the set of canonical `(resource, factor, original size)` representatives fixed across scenarios. Thus the original shared 30/40 mm asset is counted once for each of its two future scales in the baseline as well. These paired rows avoid attributing a change in cohort composition to better art.\n\n'+table(['Scenario','Faction','Size','N','Min m','Median m','Max m','CV'],[[x['scenario'],'Alien' if x['alien'] else 'Human',x['size'],x['n'],n(x['min']),n(x['median']),n(x['max']),n(x['cv'])] for x in paired_spread])+table(['Scenario','Faction','Size pair','Inversions','Pairs','Fraction'],[[x['scenario'],'Alien' if x['alien'] else 'Human',f"{x['small']} / {x['large']}",x['inversions'],x['pairs'],f"{100*x['fraction']:.1f}%"] for x in paired]),encoding='utf-8')
(O/'paired-tier-statistics.json').write_text(json.dumps({'spread':paired_spread,'inversions':paired},indent=2)+'\n')

heads=['# Projected extra/merge-node capacity','This is a copy of the original node-inventory question with weapon envelopes changed. Coordinates and authored counts are unchanged. A larger compatible subset does not create UI slots or authorize guns. `NC` means the ordinary set itself cannot be certified. Different sizes may have different subsets. Mothership current geometry is reported separately.','## Native geometry: worst appearance, native faction weapons']
for pad in [(0,0),(5,0),(10,0),(5,.25)]:
 rows=[]
 for c in CL:
  vs=cl('baseline',c);authored=len({n['index'] for n in vs[0]['nodes']})
  for size in [1,2,4]:
   vals=[]
   for s in ['baseline','canonical','compact_optics']:
    subs=[f(v,size,pad)['expanded_indices'] for v in cl(s,c)];vals.append('NC' if any(x is None for x in subs) else min(map(len,subs)))
   rows.append([c,size,vs[0]['ui_hardpoints'],authored,*vals])
 heads+=['### Padding '+str(pad[0])+' m AND '+str(pad[1]*100)+'%',table(['Class','Size','UI cells','Authored pairs','Original compatible total','Proportional compatible total','Compact compatible total'],rows)]
heads+=['## Current Mothership, all sizes and padding thresholds',table(['Scenario','Size']+PADLABEL,[[s,size]+[('NC' if f(v,size,pad)['expanded_indices'] is None else len(f(v,size,pad)['expanded_indices'])) for pad in OLD['paddings']] for s,v in FIX.items() for size in [1,2,4]]),'Only AlienDestroyer (3 authored vs 2 UI), AlienDreadnought (12 vs 8), and AlienMothership (20 vs 16) have more authored hull pairs than declared UI cells. These identities are unchanged by art scaling. Mothership remains limited to sixteen active guns: the prior UI-capacity analysis requires 42 icons for four extra guns versus native capacity 40. No extra-weapon mass, crew or DPS is included here. Original duplicate/clustered nodes remain defects in the native snapshot even when a smaller art happens to improve clearance.']
save('node-inventory.md',heads)

# Exact maximum raw family loadout after re-tier, preserving actual native grid orientation.
rows=[];dpsdata=[]
for c in CL:
 v=cl('baseline',c)[0]
 for fam in ['RailgunBatteryMk1','RailgunBatteryMk2','RailgunBatteryMk3','CoilgunBatteryMk1','CoilgunBatteryMk2','CoilgunBatteryMk3','Naval cannon']:
  ws=[w for w in OLD['weapons'] if oldr.family(w)==fam and w['alien']==v['alien']]
  if not ws:continue
  after=copy.deepcopy(ws)
  for w in after:
   if w['name'].startswith('Heavy') or w['name']=='6-inchCannon':w['size']=2;w['mount']='TwoHullHoriz'
  def solve(ws):
   if c in CAP:
    eligible=[w for w in ws if w['size']==4]
    if not eligible:return 0.,()
    w=max(eligible,key=lambda w:w['current']['raw_MJ_s']);return w['current']['raw_MJ_s']*v['ui_hardpoints'],tuple([w['name']]*v['ui_hardpoints'])
   return oldr.best_packed(v,ws,'current')
  before,bnames=solve(ws);new,anames=solve(after);mass=lambda names:sum(W[n]['current']['mass_t'] for n in names)
  row={'class':c,'family':fam,'before_raw_MJ_s':before,'after_raw_MJ_s':new,'before_weapons':list(bnames),'after_weapons':list(anames),'before_mass_t':mass(bnames),'after_mass_t':mass(anames)};dpsdata.append(row);rows.append([c,fam,n(before),n(new),percent(new,before),len(bnames),len(anames),n(mass(bnames),0),n(mass(anames),0)])
heads=['# DPS and fitting fallout of canonical art and optional tier moves','[Original DPS report](../turret-clearance/dps-analysis.md). Every art-only scenario preserves all 147 hull weapon templates, footprints, shot energies, cadences, mass, optics and ranges: **raw hull DPS delta = 0 for every class and every original family scenario**. More empty space is not permission to add guns. The previously proposed 25 mm does not exist. Nose DPS also remains unchanged.','## Optional heavy rail / heavy coil 4 → 2; 6-inch 1 → 2','Each family is tested separately. All Mk I/II/III heavy rail or coil variants change to `TwoHullHoriz`; in the Naval cannon rows only the 6-inch changes to `TwoHullHoriz`. Statistics stay fixed. Exact native grid packing chooses the highest sustained raw MJ/s mixed-size loadout within that family. Divide by 20 for damage points/s. Range/per-hit penetration and target maneuver are not modeled. These are slot-capacity outcomes, not endorsed balance settings. Capitals continue their current FourHull-only rule, exposing the incompatibility instead of silently allowing the demoted weapon. The combined rail-plus-naval option has these same separate family results.',table(['Class','Family','Original MJ/s','Retier MJ/s','Delta','Old guns','New guns','Old mass t','New mass t'],rows),'## Capital policy blocker','Titan, AlienTitan and Mothership currently admit only `FourHull` weapons and treat each as one cell. Demoting a human heavy rail/coil family removes its only eligible capital entry; human Titan homogeneous family capacity becomes zero in the table. Alien capitals are unaffected by these human retiers. Keeping the old template as a capital-only legacy entry or introducing an explicit new heavy-role eligibility policy would require separate source, save-migration, UI and AI work. A mount enum edit alone is not safe.','## Other balance consequences','Size changes can affect native internal-size-derived HP and training behavior as well as packing. The inspected training initialization uses 270/internalSize, so 4 → 2 can double that rate unless explicitly decoupled. Existing designs, stock designs, upgrade choices, refits, ammo records and native controller merge mappings must be reviewed. Mass/crew/power are not automatically reduced with visual volume. No stat compensation is applied in these tables.']
save('dps-analysis.md',heads);(O/'retier-dps.json').write_text(json.dumps(dpsdata,indent=2)+'\n')

print('Reports generated. Key coherence:')
for x in stats:
 if x['scenario'] in ['baseline','canonical','compact_optics']:print(x['scenario'],x['alien'],x['size'],*[round(x[k],3) for k in ['median','max_min','cv']])
print('Inversions:')
for x in inversions:
 if x['scenario'] in ['baseline','canonical','compact_optics','retier_rail','retier_coil']:print(x)

```

## `.tmp/canonical-turrets/legal_layouts.py`

```python
"""Recompute current-legal homogeneous maximum packings, not all-node hypotheticals."""
import json,itertools,sys
from pathlib import Path
from functools import lru_cache
import numpy as np
R=Path.cwd();B=R/'docs/ship-balance-research';O=B/'canonical-turret-projection';sys.path.insert(0,str(R/'scripts/ship-balance'));import report_turret_clearance as r
read=lambda p:json.loads(p.read_text());old=read(B/'turret-clearance/measurements.json');metrics=read(O/'weapon-projections.json');mm={(x['scenario'],x['name']):x for x in metrics};fix=read(B/'turret-clearance/mothership-layout-2026-10-21.json');cap=r.CAPITALS
def layouts(v,mount):
 if v['hull'] in cap:return [tuple(v['ordinary_indices'])] if mount=='FourHull' else []
 ps=v['legal_placements'][mount];best=-1;out=[]
 def visit(j,used,chosen):
  nonlocal best,out
  if len(chosen)+len(ps)-j<best:return
  if j==len(ps):
   if len(chosen)>best:best=len(chosen);out=[]
   if len(chosen)==best:out.append(tuple(chosen))
   return
  p=ps[j];cells=set(map(tuple,p['cells']))
  if not used&cells:visit(j+1,used|cells,chosen+[p['controller']])
  visit(j+1,used,chosen)
 visit(0,set(),[])
 return sorted(set(out))
rows=[]
for v in old['variants']:
 nodes=v['nodes']
 if v['hull']=='AlienMothership':
  fm={q['index']:q for q in fix['rows']};nodes=[dict(n,position_m=fm[n['index']][n['side']]) for n in nodes]
 for size,mounts in [(1,['OneHull']),(2,['TwoHullHoriz','TwoHullVert']),(4,['FourHull'])]:
  ws=[w for w in old['weapons'] if w['size']==size and w['alien']==v['alien']];eligible=[]
  for w in ws:
   mount='OneHull' if w['mount']=='HalfHull' else w['mount'];ls=layouts(v,mount)
   if ls and len(ls[0])>0:eligible.append((w,ls))
  row={'hull':v['hull'],'appearance':v['appearance'],'size':size,'eligible_templates':len(eligible),'min_guns':min((len(ls[0]) for w,ls in eligible),default=0),'max_guns':max((len(ls[0]) for w,ls in eligible),default=0)}
  for s in ['baseline','canonical','compact_optics']:
   gaps=[];lim=None;bestlayouts=[]
   for w,ls in eligible:
    values=[]
    for indices in ls:
     subset=[n for n in nodes if n['index'] in indices];gs=[]
     for a,b in itertools.combinations(subset,2):
      if a['index']==b['index']:continue
      # Normal hull resource radii. Simple hulls have one mount, no pair here.
      rad=mm[s,w['name']]['sphere_diameter_m']/2;gap=float(np.linalg.norm(np.array(a['position_m'])-b['position_m']))-rad*(a['parent_max_scale']+b['parent_max_scale']);gs.append(gap)
     if gs:values.append(min(gs))
    if values:
     worst=min(values);bestlayouts.append(max(values));gaps.append(worst)
     if lim is None or worst<lim[0]:lim=(worst,w['name'])
   row[s]={'worst_all_max_packings_gap_m':min(gaps) if gaps else None,'worst_weapon_best_max_packing_gap_m':min(bestlayouts) if bestlayouts else None,'limiting_weapon':lim[1] if lim else None}
  rows.append(row)
(O/'legal-layouts.json').write_text(json.dumps(rows,indent=2)+'\n')
def n(v):return '—' if v is None else f'{v:.3f}'
parts=['# Current-legal maximum-packed loadout check','This separate check prevents confusing the original all-node hypothesis with actual fitting. It enumerates every maximum-cardinality homogeneous loadout of one weapon type on the native grid, using the correct merge controller for each footprint. FourHull-only capitals use one heavy gun per ordinary cell. Mothership uses corrected current positions. No new slots or mount types are granted.','`Worst` is the least edge gap across all allowed maximum-packed layouts and native-faction weapons of that size. `Best` first chooses each weapon’s best maximum packing, then reports the worst weapon. One-gun designs have no pair and are shown as a dash; they are not certified against hull geometry. Counts can differ by horizontal/vertical two-cell orientation. All bounds are full 3D spheres. Negative bounds are inconclusive, not clipping proof.']
parts.append(r.table(['Hull','Art','Size','Eligible templates','Guns min–max','Original worst m','Canonical worst m','Compact worst m','Compact best m','Compact limiting weapon'],[[x['hull'],x['appearance'],x['size'],x['eligible_templates'],f"{x['min_guns']}–{x['max_guns']}",n(x['baseline']['worst_all_max_packings_gap_m']),n(x['canonical']['worst_all_max_packings_gap_m']),n(x['compact_optics']['worst_all_max_packings_gap_m']),n(x['compact_optics']['worst_weapon_best_max_packing_gap_m']),x['compact_optics']['limiting_weapon'] or '—'] for x in rows]))
(O/'legal-layouts.md').write_text('\n\n'.join(parts)+'\n',encoding='utf-8')
print('Legal rows',len(rows))
for x in rows:
 if x['hull'] in ['Monitor','Titan','AlienMothership']:print(x['hull'],x['appearance'],x['size'],x['min_guns'],x['max_guns'],[(s,x[s]['worst_all_max_packings_gap_m']) for s in ['baseline','canonical','compact_optics']])

```

## `.tmp/canonical-turrets/plot.py`

```python
import json,math
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
R=Path.cwd();O=R/'docs/ship-balance-research/canonical-turret-projection';rows=json.loads((O/'weapon-projections.json').read_text());lookup={(r['scenario'],r['name']):r for r in rows};cohort={}
for r in rows:
 if r['scenario']=='canonical':cohort[(r['resource'],r['size'],round(r['factor'],8))]=r['name']
im=Image.new('RGB',(1600,1020),'#101923');d=ImageDraw.Draw(im);font=lambda size:ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',size);title=font(30);body=font(21);small=font(17)
d.text((40,25),'Weapon visual size versus gameplay slot cost',font=title,fill='white');d.text((40,70),'Horizontal motion diameter • fixed cohort • logarithmic metre scale • planned art, not implemented',font=body,fill='#b3c4d5')
colors={'baseline':'#b8c3ce','canonical':'#ffb05e','compact_optics':'#5fdbd0'};labels={'baseline':'Original','canonical':'Proportional canonical','compact_optics':'Canonical + compact alien optics'}
for j,(s,c) in enumerate(colors.items()):
 x=40+j*490;d.ellipse((x,112,x+12,124),fill=c);d.text((x+23,101),labels[s],font=body,fill=c)
left=250;right=1540;top=175;bottom=900
def xx(v):return left+(math.log2(v)+1)/8*(right-left)
for tick in [.5,1,2,4,8,16,32,64,128]:
 x=xx(tick);d.line((x,top-20,x,bottom),fill='#2c3c4b',width=1);d.text((x-13,bottom+8),str(tick),font=small,fill='#b3c4d5')
for group,(alien,size) in enumerate([(False,1),(False,2),(False,4),(True,1),(True,2),(True,4)]):
 y=top+group*120;d.text((40,y+26),('Alien' if alien else 'Human')+' / size '+str(size),font=body,fill='white')
 names=[name for name in cohort.values() if lookup['canonical',name]['alien']==alien and lookup['canonical',name]['size']==size]
 for k,s in enumerate(colors):
  values=sorted((lookup[s,name]['horizontal_diameter_m'],name) for name in names)
  cy=y+k*27
  for i,(value,name) in enumerate(values):
   x=xx(value);jit=(i%3-1)*4;d.ellipse((x-4,cy+jit-4,x+4,cy+jit+4),fill=colors[s])
  med=sorted(v for v,_ in values);m=med[len(med)//2] if len(med)%2 else (med[len(med)//2-1]+med[len(med)//2])/2;x=xx(m);d.line((x,cy-10,x,cy+10),fill=colors[s],width=3)
 d.line((35,y+95,1540,y+95),fill='#26333f')
d.text((40,960),'Dots = distinct projected art configurations; bars = medians. Compact optics uses a new alien design envelope.',font=small,fill='#b3c4d5');im.save(O/'size-tier-distribution.png')


```

## `.tmp/canonical-turrets/validate.py`

```python
import json,hashlib,math,re,itertools
from pathlib import Path
import numpy as np
R=Path.cwd();B=R/'docs/ship-balance-research';O=B/'canonical-turret-projection';T=R/'.tmp/canonical-turrets'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
old=read(B/'turret-clearance/measurements.json');m=read(O/'weapon-projections.json');mm={(x['scenario'],x['name']):x for x in m};targets=read(O/'all-weapon-targets.json');checks=read(O/'projection-checks.json')
before=read(T/'runtime-before.json');assert all((R/p).is_file() and sha(R/p)==h for p,h in before.items())
inputs=read(O/'input-hashes.json');assert all(sha(R/p)==h for p,h in inputs.items())
native=[]
for s in old['sources']:
 p=Path(s['path']);actual=sha(p).upper();assert actual==s['sha256'].upper(),str(p);native.append(str(p))
assert len(targets)==166 and len(m)==147*10
assert checks['baseline_fit_records_reproduced']==5376 and checks['baseline_max_gap_error_m']==0
calibrated=0
for t in targets:
 if t['measured_m'] is not None and t['target_m'] is not None:
  assert math.isclose(t['factor']*t['measured_m'],t['target_m'],rel_tol=1e-10);calibrated+=1
for w in old['weapons']:
 b=mm['baseline',w['name']];c=mm['canonical',w['name']];h=mm['head_only',w['name']];s=c['factor']
 assert np.allclose(np.array(b['dimensions_m'])*s,c['dimensions_m'])
 assert math.isclose(b['sphere_diameter_m']*s,c['sphere_diameter_m'],rel_tol=1e-12)
 assert math.isclose(b['horizontal_diameter_m']*s,c['horizontal_diameter_m'],rel_tol=1e-12)
 assert (h['sphere_diameter_m']<=c['sphere_diameter_m']+1e-9) if s>=1 else (h['sphere_diameter_m']>=c['sphere_diameter_m']-1e-9)
# Independent randomized containment checks using archived raw mesh geometry.
ins={i['resource']:i for i in read(B/'turret-clearance/weapon-mesh-inspection-2026-10-21.json')};data=np.load(O/'source-hull-geometry.npz');rng=np.random.default_rng(20261021);rotations=0
def matrix(q):
 x,y,z,w=np.array(q)/np.linalg.norm(q)
 return np.array([[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],[2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],[2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]])
for g in old['geometries']:
 info=ins[g['resource']];upper=data['v'+info['meshes'][1]['key']];S=np.array(g['root_scale']);C=np.array(g['child_scale']);p=np.array(g['child_position']);v=((upper/S-p)@matrix(info['source_child_rotation_xyzw']))/C
 for _ in range(24):
  Q=matrix(rng.normal(size=4));points=(p+(v*C)@Q.T)*S;assert np.linalg.norm(points,axis=1).max()<=g['swept_radius_m']+1e-7;rotations+=1
# Compact concept: clear aperture fits housing and proposed extrema obey sphere.
compact=0
for w in old['weapons']:
 if w['kind']!='TILaserWeaponTemplate' or not w['alien']:continue
 D=w['template']['mirrorRadius_cm']/50;r=.6*D;assert r>=D/2
 entry=mm['compact_optics',w['name']];base=data['v'+ins[w['resource']]['meshes'][0]['key']];h=max(0,base[:,1].max())+r
 points=rng.normal(size=(300,3));points=points/np.linalg.norm(points,axis=1)[:,None]*r+np.array([0,h,0]);assert np.linalg.norm(points,axis=1).max()<=entry['sphere_diameter_m']/2+1e-8
 assert np.linalg.norm(points[:,[0,2]],axis=1).max()<=entry['horizontal_diameter_m']/2+1e-8;compact+=1
# All projected fit flags independently checked from saved minimum gaps/limiting pair ratios.
fitcount=0
for s in checks['scenarios']:
 d=read(O/(s+'-measurements.json'));assert len(d['variants'])==64
 for v in d['variants']:
  for f in v['fits']:
   if not v['ui_hardpoints']:assert not f['ordinary_pass']
   elif f['ordinary_min_gap_m'] is None:assert f['ordinary_pass']
   elif f['padding_ratio']==0:assert f['ordinary_pass']==(f['ordinary_min_gap_m']+1e-8>=f['padding_m'])
   if f['expanded_indices'] is not None:assert set(v['ordinary_indices']).issubset(f['expanded_indices'])
   fitcount+=1
assert fitcount==53760
# Native current capital rule must expose retier incompatibility; early naval losses are explicit.
dps=read(O/'retier-dps.json');assert all(x['after_raw_MJ_s']==0 for x in dps if x['class']=='Titan')
assert next(x for x in dps if x['class']=='Escort' and x['family']=='Naval cannon')['after_raw_MJ_s']==0
assert next(x for x in dps if x['class']=='Monitor' and x['family']=='RailgunBatteryMk3')['after_raw_MJ_s']>100
# Check no move of original native geometry is hidden in projected data.
for s in checks['scenarios']:
 for a,b in zip(old['variants'],read(O/(s+'-measurements.json'))['variants']):assert a['nodes']==b['nodes']
result={'status':'passed','baseline_fit_records_exact':5376,'projected_fit_records_checked':fitcount,'source_files_verified':len(inputs),'native_original_source_hashes_verified':len(native),'runtime_files_unchanged':len(before),'ship_target_records':len(targets),'calibrated_target_equations':calibrated,'random_rotations_tested_all_vertices':rotations,'compact_alien_laser_template_envelopes_checked':compact,'original_node_coordinates_preserved_all_scenarios':True,'live_game_test':False,'build_deploy_commit':False}
(O/'verification.json').write_text(json.dumps(result,indent=2)+'\n')

```
