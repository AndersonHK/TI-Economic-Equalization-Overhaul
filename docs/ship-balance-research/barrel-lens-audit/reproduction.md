# Reproduction and verification recipe

2026-10-21. These are archived research recipes, not installed mod tooling.
Temporary execution belongs in `.tmp/barrel-audit/`; persistent evidence belongs
in this directory. No game files are written, no build/deploy is needed for
these measurements, and the pre-existing 0.10.4 runtime work was preserved.

## Environment and order

Run from the repository root. Extraction uses Python 3.12 with NumPy and the
existing `.tmp/unitypy` package. Other analysis uses NumPy and Pillow.
Blender 2.79 renders the original triangles with analysis materials, reflecting
Unity Y/Z into Blender Z/Y and reversing winding. Run `extract.py`, `render.py`,
`inspect_meshes.py`, `sections.py`, `report.py`, then `verify.py` below.
Extract each fenced block into its stated temporary filename. Existing shared
helpers are in `scripts/ship-balance/measure_turret_clearance.py` and
`measure_ship_prefabs.py`; they are not modified by this research.

```powershell
& 'C:\Users\imper\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' .tmp/barrel-audit/extract.py
& 'C:\Program Files\Blender Foundation\Blender\blender.exe' --background --factory-startup -t 8 --python .tmp/barrel-audit/render.py
python .tmp/barrel-audit/inspect_meshes.py
python .tmp/barrel-audit/sections.py
python .tmp/barrel-audit/report.py
python .tmp/barrel-audit/verify.py
```

Before report/verification, freshly decompile the four types below from the
installed `TerraInvicta_Data/Managed/Assembly-CSharp.dll` into the temporary
directory with `.tmp/ilspycmd/ilspycmd.exe -t TYPE ASSEMBLY > FILE`.

| Type | File | Checked behavior |
|---|---|---|
| `TILaserWeaponTemplate` | `TILaserWeaponTemplate.cs` | Radius doubling, diffraction/jitter, spot area, armor exponent, interception, chipping |
| `TIShipWeaponTemplate` | `TIShipWeaponTemplate.cs` | MJ/20 and direct/chip split |
| `PavonisInteractive.TerraInvicta.Ship.Damage` | `Damage.cs` | Independent 0.8–1.2 damage randomization |
| `PavonisInteractive.TerraInvicta.TISpaceShipState` | `TISpaceShipState.cs` | Intact laser armor multiplier, resistance and final subtraction |

Source hashes identify this installed binary and the exact decoded evidence.
The pre-existing `ShipModelController.SetWeapon` inspection establishes native
resource scaling versus placeholder scaling; it is described in the preceding
turret-clearance reports. Full native decompilations remain temporary.

**Graser limitation:** the templates author 0.1 nm, but the inspected native
`wavelength_nm` field is an integer. Loaded conversion/rounding was not observed.
Sensitivity tables use authored 0.1 nm explicitly; if loaded as zero, the
already negligible diameter benefit becomes exactly zero. No material finding
depends on that distinction.

## Measurement audit

All 41 front and side renders were visually inspected. Cross-section loops
are welded at 0.00001 m and checked for closure. A separate concentric-square
tube fixture verifies topology, diameter, inscribed/circumscribed convention,
translation and scale. Repeating actual front sections at 0.1 and 1 mm inset
checks sensitivity. Human curved optical caps use their material submesh instead
of the tip section. Closed nose-rail faces are excluded from bore inference.
The numerical results are in `verification.json`; 44 laser scenarios also
match the unsimplified native optical expression. Runtime preservation compares
all selected source/config/package files against a pre-analysis SHA-256 manifest.

Gunpowder exposed-length shoulder Z values were manually selected from the
receiver-front vertex rings, not guessed from image pixels: art IDs 2/3/0/1 use
2.08138 / 2.07038 / 2.06338 / 2.08838 m. Subtract these from the forwardmost
vertex to reproduce the exposed assembly lengths; do not call them full bores.

## Archived scripts

### `.tmp/barrel-audit/extract.py`

```python
import sys,json,hashlib
from pathlib import Path
import numpy as np
R=Path.cwd();sys.path.insert(0,str(R/'scripts/ship-balance'))
import measure_turret_clearance as m
from UnityPy.helpers.MeshHelper import MeshHandler
T=R/'.tmp/barrel-audit';O=R/'docs/ship-balance-research/barrel-lens-audit';O.mkdir(exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
before={str(p.relative_to(R)):sha(p) for p in (R/'TIEconomyMod').rglob('*') if p.is_file() and (p.suffix in ['.cs','.json','.dll','.csproj']) and not any(a in p.parts for a in ['obj','bin','.vs'])}
(T/'runtime-before.json').write_text(json.dumps(before,indent=2))
templates=m.DEFAULT_GAME/'TerraInvicta_Data/StreamingAssets/Templates';bundle=m.DEFAULT_GAME/'TerraInvicta_Data/StreamingAssets/AssetBundles/shipweapons'
weapons=[];sources={str(bundle):sha(bundle)}
for kind in ['TIGunTemplate','TIMagneticGunTemplate','TILaserWeaponTemplate']:
 p=templates/(kind+'.json');sources[str(p)]=sha(p);rows=m.read(p)
 overrides=list((R/'TIEconomyMod/ModFiles').rglob(kind+'.json'))
 for p in overrides:
  rows=m.merge(rows,m.read(p));sources[str(p.relative_to(R))]=sha(p)
 for row in rows:weapons.append(dict(row,kind=kind))
env=m.UnityPy.load(str(bundle));paths={Path(p).stem.lower():(p,v) for p,v in env.container.items()}
arrays={};resources=[]
for k,resource in enumerate(sorted({w['modelResource'] for w in weapons if w.get('modelResource')})):
 if resource.split('/')[-1].lower() not in paths:print('MISSING',resource);continue
 path,ptr=paths[resource.split('/')[-1].lower()];go=ptr.read();root=m.transform(go).read();child=root.m_Children[0].read()
 scale=m.xyz(root.m_LocalScale);cs=m.xyz(child.m_LocalScale)
 row={'index':k,'resource':resource,'path':path,'root_scale':scale.tolist(),'child_scale':cs.tolist(),'child_position_m':(m.xyz(child.m_LocalPosition)*scale).tolist(),'meshes':[]}
 for j,obj in enumerate([go,child.m_GameObject.read()]):
  mesh=next(p for p in m.component_ptrs(obj) if p.type.name=='MeshFilter').read().m_Mesh.read();h=MeshHandler(mesh);h.process()
  v=np.array(h.m_Vertices);v=v*scale if j==0 else m.transform_points(m.local_matrix(child),v)*scale
  key='%d_%d'%(k,j);arrays['v'+key]=v;tris=[]
  subs=[]
  for si,ts in enumerate(h.get_triangles()):
   ts=np.asarray(ts,dtype=np.int32)
   if len(ts):arrays['s'+key+'_'+str(si)]=ts;tris.extend(ts.tolist());subs.append({'submesh':si,'triangles':len(ts),'bounds':np.ptp(v[np.unique(ts)],axis=0).tolist()})
  arrays['t'+key]=np.array(tris,dtype=np.int32)
  materials=[]
  for q in m.component_ptrs(obj):
   if q.type.name=='MeshRenderer':
    materials=[p.read().m_Name for p in q.read().m_Materials]
  row['meshes'].append({'key':key,'name':mesh.m_Name,'dimensions_m':np.ptp(v,axis=0).tolist(),'min_m':v.min(0).tolist(),'max_m':v.max(0).tolist(),'materials':materials,'submeshes':subs})
 resources.append(row);print(k,resource,row['meshes'][1]['name'],np.round(row['meshes'][1]['dimensions_m'],3),materials,flush=True)
np.savez_compressed(T/'scenes.npz',**arrays)
(O/'inventory.json').write_text(json.dumps({'sources_sha256':sources,'resources':resources,'weapons':weapons},indent=2)+'\n')

```

### `.tmp/barrel-audit/render.py`

```python
import bpy,json,math
from pathlib import Path
import numpy as np
from mathutils import Vector
R=Path.cwd();T=R/'.tmp/barrel-audit';O=T/'renders';O.mkdir(exist_ok=True)
rows=json.loads((R/'docs/ship-balance-research/barrel-lens-audit/inventory.json').read_text())['resources'];data=np.load(str(T/'scenes.npz'))
for w in rows:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 scene=bpy.context.scene;scene.render.engine='BLENDER_RENDER';scene.render.resolution_x=800;scene.render.resolution_y=500;scene.render.resolution_percentage=100
 scene.render.image_settings.file_format='PNG';scene.world.horizon_color=(.035,.045,.06);scene.world.ambient_color=(0,0,0)
 scene.world.light_settings.use_ambient_occlusion=True;scene.world.light_settings.ao_factor=.7;scene.world.light_settings.samples=5
 scene.world.light_settings.use_environment_light=True;scene.world.light_settings.environment_energy=.2
 entry=w['meshes'][1];v=data['v'+entry['key']][:,[0,2,1]];tris=data['t'+entry['key']][:,::-1]
 mesh=bpy.data.meshes.new(entry['name']);mesh.from_pydata(v.tolist(),[],tris.tolist());mesh.update()
 ob=bpy.data.objects.new(entry['name'],mesh);scene.objects.link(ob)
 mat=bpy.data.materials.new('Body');mat.diffuse_color=(.37,.62,.78);mat.specular_intensity=.15;ob.data.materials.append(mat)
 if len(entry['submeshes'])>1:
  mat=bpy.data.materials.new('Optical cap');mat.diffuse_color=(.9,.52,.17);mat.specular_intensity=.15;ob.data.materials.append(mat)
  count=entry['submeshes'][0]['triangles']
  for poly in mesh.polygons:
   if poly.index>=count:poly.material_index=1
 for i,(direction,energy) in enumerate([((1,1,2),.9),((-1,-1,-1),.4)]):
  lamp=bpy.data.lamps.new('Key'+str(i),'SUN');lamp.energy=energy;lamp.use_shadow=False
  ob=bpy.data.objects.new(lamp.name,lamp);scene.objects.link(ob);ob.rotation_euler=Vector(direction).to_track_quat('Z','Y').to_euler()
 camera=bpy.data.cameras.new('Camera');cam=bpy.data.objects.new('Camera',camera);scene.objects.link(cam);scene.camera=cam;camera.type='ORTHO';camera.clip_end=1000
 center=(v.min(0)+v.max(0))/2
 for view,direction in [('front',(0,1,0)),('side',(1,0,0))]:
  # common side scale; front closeups independently framed and explicitly labeled
  camera.ortho_scale=8 if view=='side' else max(np.ptp(v,axis=0)[0],np.ptp(v,axis=0)[2]*1.6)*1.15
  cam.location=Vector(center)+Vector(direction)*60;cam.rotation_euler=(Vector(center)-cam.location).to_track_quat('-Z','Y').to_euler()
  scene.render.filepath=str(O/('%02d-%s.png'%(w['index'],view)));bpy.ops.render.render(write_still=True)
 print('DONE',w['index'],flush=True)

```

### `.tmp/barrel-audit/inspect_meshes.py`

```python
import json
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont
R=Path.cwd();T=R/'.tmp/barrel-audit';O=R/'docs/ship-balance-research/barrel-lens-audit';a=json.loads((O/'inventory.json').read_text());data=np.load(T/'scenes.npz')
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',19)
for group,indices in [('powder',[2,3,0,1]),('human-magnetic',[39,32,26,35,28,22,40,33,27,21,36,29,23,19]),('human-laser',[34,37,30,24,38,31,25,20]),('alien',[14,17,12,8,18,13,9,5,15,10,6,16,11,7,4])]:
 for page,start in enumerate(range(0,len(indices),5)):
  subset=indices[start:start+5];sheet=Image.new('RGB',(1300,365*len(subset)),(14,20,28));draw=ImageDraw.Draw(sheet)
  for r,i in enumerate(subset):
   w=next(x for x in a['resources'] if x['index']==i)
   draw.text((12,365*r+5),str(i)+' '+w['resource'].split('/')[-1],font=font,fill='white')
   for c,view in enumerate(['front','side']):
    im=Image.open(T/'renders'/('%02d-%s.png'%(i,view)));im.thumbnail((640,310));sheet.paste(im,(c*650,365*r+30))
   draw.text((12,365*r+340),'Front: individually framed | Side: all at 8 m image width',font=font,fill='#aabacc')
  sheet.save(O/(group+'-'+str(page+1)+'.png'))
for w in a['resources']:
 v=data['v'+w['meshes'][1]['key']];z=v[:,2];mx=z.max()
 zs,counts=np.unique(np.round(z,5),return_counts=True);order=np.argsort(zs)[::-1][:12]
 print(w['index'],w['resource'].split('/')[-1],'front rings',[(float(zs[i]),int(counts[i]),np.round(np.ptp(v[np.abs(z-zs[i])<.000006],axis=0)[:2],4).tolist()) for i in order])
 if len(w['meshes'][1]['submeshes'])>1:print('CAP',w['meshes'][1]['submeshes'][1])

```

### `.tmp/barrel-audit/sections.py`

```python
import json,math
from pathlib import Path
import numpy as np
R=Path.cwd();T=R/'.tmp/barrel-audit';O=R/'docs/ship-balance-research/barrel-lens-audit';a=json.loads((O/'inventory.json').read_text());data=np.load(T/'scenes.npz')
def section(v,t,z):
 pts=v[t];segments=[]
 for p in pts[(pts[:,:,2].min(1)<z)&(pts[:,:,2].max(1)>z)]:
  hits=[]
  for x,y in [(p[0],p[1]),(p[1],p[2]),(p[2],p[0])]:
   if (x[2]-z)*(y[2]-z)<0:hits.append(x[:2]+(y[:2]-x[:2])*(z-x[2])/(y[2]-x[2]))
  if len(hits)==2:segments.append(hits)
 graph={};xy={}
 for seg in segments:
  keys=[tuple(np.round(p,5)) for p in seg]
  if keys[0]==keys[1]:continue
  for i in range(2):graph.setdefault(keys[i],set()).add(keys[1-i]);xy[keys[i]]=seg[i]
 loops=[];visited=set()
 for k in graph:
  if k in visited:continue
  todo=[k];comp=[]
  while todo:
   q=todo.pop()
   if q in visited:continue
   visited.add(q);comp.append(q);todo.extend(graph[q]-visited)
  ps=np.array([xy[q] for q in comp]);center=(ps.min(0)+ps.max(0))/2;dims=np.ptp(ps,axis=0)
  edges=[]
  for q in comp:
   for n in graph[q]:
    x=xy[q]-center;y=xy[n]-center;d=y-x;u=np.clip(-np.dot(x,d)/np.dot(d,d),0,1);edges.append(float(np.linalg.norm(x+u*d)))
  loops.append({'width_m':float(dims[0]),'height_m':float(dims[1]),'center_xy_m':center.tolist(),'nodes':len(comp),'closed':all(len(graph[q])==2 for q in comp),'inscribed_diameter_m':2*min(edges),'circumscribed_diameter_m':2*float(np.linalg.norm(ps-center,axis=1).max())})
 return sorted(loops,key=lambda x:x['center_xy_m'])
results=[]
for w in a['resources']:
 key=w['meshes'][1]['key'];v=data['v'+key];t=data['t'+key];mx=v[:,2].max();mn=v[:,2].min();row={'index':w['index'],'resource':w['resource'],'sections':[]}
 for inset in [.0001,.001,.01,.05,.1,.2,.3,.5,1,1.5,2]:
  if mx-inset<mn:continue
  row['sections'].append({'inset_m':inset,'z_m':float(mx-inset),'loops':section(v,t,mx-inset)})
 if len(w['meshes'][1]['submeshes'])>1:
  cap=v[np.unique(data['s'+key+'_1'])];row['cap_dimensions_m']=np.ptp(cap,axis=0).tolist()
 results.append(row)
 print(w['index'],w['resource'].split('/')[-1],[(round(l['width_m'],5),round(l['height_m'],5),l['closed']) for l in row['sections'][0]['loops']], 'cap',row.get('cap_dimensions_m'))
(O/'sections.json').write_text(json.dumps(results,indent=2)+'\n')

```

### `.tmp/barrel-audit/report.py`

```python
import json,math,hashlib,shutil
from pathlib import Path
import numpy as np
R=Path.cwd();T=R/'.tmp/barrel-audit';O=R/'docs/ship-balance-research/barrel-lens-audit';A=json.loads((O/'inventory.json').read_text());S=json.loads((O/'sections.json').read_text());D=np.load(T/'scenes.npz');byi={r['index']:r for r in A['resources']};secs={r['index']:r for r in S};weapons=A['weapons']
def table(headers,rows):return '| '+' | '.join(headers)+' |\n|'+'|'.join(['---']*len(headers))+'|\n'+''.join('| '+' | '.join(map(str,r))+' |\n' for r in rows)+'\n'
def fmt(x,n=3):return f'{x:.{n}f}'
def projectile(w):return w['projectileDiameter_mm'] if w['kind']=='TIGunTemplate' else 1000*(4*w['ammoMass_kg']/(math.pi*19300*10))**(1/3)
derived=[]
for i,r in byi.items():
 loops=secs[i]['sections'][0]['loops'];row={'index':i,'resource':r['resource'],'moving_dimensions_m':r['meshes'][1]['dimensions_m']}
 if 'cap_dimensions_m' in secs[i]:row.update(aperture_m=secs[i]['cap_dimensions_m'][0],openings=1,method='optical cap submesh projected diameter')
 elif i in [21,33,40]:
  v=D['v'+r['meshes'][1]['key']];zs=sorted(set(np.round(v[:,2],5)),reverse=True);p=v[np.abs(v[:,2]-zs[1])<.000006]
  row.update(openings=None,method='closed accelerator front face; no separate bore',face_dimensions_m=np.ptp(p,axis=0)[:2].tolist())
 else:
  inner=sorted(loops,key=lambda l:l['width_m']*l['height_m'])[:len(loops)//2];outer=sorted(loops,key=lambda l:l['width_m']*l['height_m'])[len(loops)//2:]
  row.update(openings=len(inner),opening_width_m=float(np.mean([l['width_m'] for l in inner])),opening_height_m=float(np.mean([l['height_m'] for l in inner])),opening_inscribed_m=float(np.mean([l['inscribed_diameter_m'] for l in inner])),opening_circumscribed_m=float(np.mean([l['circumscribed_diameter_m'] for l in inner])),outer_width_m=float(np.mean([l['width_m'] for l in outer])),method='inner muzzle-rim section 0.1 mm behind tip')
  assert all(l['closed'] for l in loops)
  if 'laser' in r['resource']:row['aperture_m']=row['opening_width_m']
 derived.append(row)
M={r['index']:r for r in derived};MR={r['resource']:r for r in derived}
out=[]
out.append('# Complete barrel and aperture numerical inventory\n\nMeasured 2026-10-21; read with the [audit](../barrel-and-lens-dimensional-audit-2026-10-21.md). All dimensions use nominal resource scales, in metres unless labelled mm. Bore means visible polygon opening, not a modeled rifled tube. All template data are merged installed + repository overrides.\n')
out.append('## Gunpowder: every weapon\n')
rows=[]
for w in weapons:
 if w['kind']!='TIGunTemplate':continue
 m=MR[w['modelResource']];nom=projectile(w);b=m['opening_width_m']*1000;ins=m['opening_inscribed_m']*1000
 rows.append([w['dataName'],w['mount'],str(m['index']),fmt(nom,1),fmt(b,1),fmt(ins,1),f'{(b/nom-1)*100:+.1f}%',fmt(nom/b),fmt(m['moving_dimensions_m'][2])])
out.append(table(['Weapon','Mount','Art ID','Caliber mm','Opening X mm','Inscribed mm','X delta','Bore-only target scale','Moving assembly L m'],rows))
out.append('## Magnetic art: all 21 resources\n\nBore X/Y are per opening; the count is graphical, not an extra DPS multiplier. Blank bores are unresolved closed faces. Complete round mass is used for the EEO projectile proxy, not divided by barrel count.\n')
rows=[]
for i in [39,32,26,35,28,22,40,33,27,21,36,29,23,19,15,10,6,16,11,7,4]:
 m=M[i];ws=[w for w in weapons if w.get('modelResource')==m['resource']];projs=sorted({projectile(w) for w in ws});b=m.get('opening_width_m');h=m.get('opening_height_m');face=m.get('face_dimensions_m')
 rows.append([i,m['resource'].split('/')[-1],m['openings'] or 'closed face',fmt(b*1000,1) if b else 'â€”',fmt(h*1000,1) if h else 'â€”',fmt(m['outer_width_m']*1000,1) if b else 'face '+ ' Ã— '.join(fmt(x*1000,1) for x in face),fmt(m['moving_dimensions_m'][2]),'â€“'.join(fmt(x,1) for x in [min(projs),max(projs)])])
out.append(table(['ID','Art','Openings','Bore X mm','Bore Y mm','Outer lip X mm / face','Assembly L m','Projectile proxy range mm'],rows))
out.append('## Laser art: all 16 resources\n\nSimulation diameter = 2 Ã— stored radius. Alien multi-aperture area-equivalent diameter is sqrt(N) Ã— d, an area comparison only; it is not a justified coherent-array diffraction diameter.\n')
rows=[]
for i in [34,37,30,24,38,31,25,20,14,17,12,8,18,13,9,5]:
 m=M[i];ws=[w for w in weapons if w.get('modelResource')==m['resource']];radii=sorted({w['mirrorRadius_cm'] for w in ws});assert len(radii)==1
 sim=radii[0]/50;ap=m['aperture_m'];n=m['openings'];m['simulation_diameter_m']=sim
 rows.append([i,m['resource'].split('/')[-1],n,radii[0],fmt(sim),fmt(ap),fmt(ap*math.sqrt(n)),f'{100*(ap/sim-1):+.1f}%',fmt(sim/ap)])
out.append(table(['ID','Art','Lenses','Radius cm','Sim D m','Per-lens D m','Area-equivalent D m','Per-lens delta','Art factor to sim D'],rows))
out.append('## Family progression deltas\n\nEach row compares adjacent sizes. Projectile ratios use ordinary Mk1 / base alien rounds, excluding siege and Gen3. Muzzle ratio uses per-opening X width; a change in opening count is separately shown.\n')
rows=[]
families=[('Hull rail',[39,32,26],[14,30,48]),('Hull coil',[35,28,22],[13,25,50]),('Nose coil',[36,29,23,19],[31,63,94,125]),('Alien hull mag',[15,10,6],[19,38,75]),('Alien nose mag',[16,11,7,4],[50,100,150,200])]
for family,ids,masses in families:
 for a,b,ma,mb in zip(ids,ids[1:],masses,masses[1:]):
  x,y=M[a],M[b];expected=(mb/ma)**(1/3);actual=y['opening_width_m']/x['opening_width_m'];rows.append([family,f'{a} â†’ {b}',f"{x['openings']} â†’ {y['openings']}",fmt(mb/ma),fmt(expected),fmt(actual),f'{100*(actual/expected-1):+.1f}%',fmt(y['moving_dimensions_m'][2]/x['moving_dimensions_m'][2])])
out.append(table(['Family','IDs','Openings','Round mass ratio','Proxy diameter ratio','Bore X ratio','Ratio error','Assembly L ratio'],rows))
rows=[]
for family,ids in [('Human hull laser',[37,30,24]),('Human nose laser',[38,31,25,20]),('Alien hull laser',[17,12,8]),('Alien nose laser',[18,13,9,5])]:
 for a,b in zip(ids,ids[1:]):
  x,y=M[a],M[b];ex=y['simulation_diameter_m']/x['simulation_diameter_m'];ac=y['aperture_m']/x['aperture_m'];rows.append([family,f'{a} â†’ {b}',fmt(ex),fmt(ac),f'{100*(ac/ex-1):+.1f}%'])
out.append(table(['Family','IDs','Simulation D ratio','Art aperture ratio','Ratio error'],rows))
out.append('## All 166 ship weapon templates\n\nIDs link to measured resources above. Magnetic proxy is a mod collision convention, not a declared caliber. Laser entries list simulation aperture diameter; complete source values, wavelengths, jitter and ranges are in [inventory.json](inventory.json).\n')
rows=[]
for w in weapons:
 if not w.get('modelResource'):continue
 m=MR[w['modelResource']];laser=w['kind']=='TILaserWeaponTemplate'
 rows.append([w['dataName'],w['mount'],m['index'],fmt(w['mirrorRadius_cm']/50) if laser else fmt(projectile(w),1), 'optic m' if laser else ('bore mm' if w['kind']=='TIGunTemplate' else 'proxy mm'), 'â€”' if laser else w['ammoMass_kg']])
out.append(table(['Template','Mount','Art ID','Nominal / derived diameter','Meaning','Complete round kg'],rows))
out.append('## All 37 records without ship art\n\nThese are region and base defenses; their simulation optics are inventoried, but no ship-resource geometry is assigned. They are not silently counted as zero-size weapons.\n')
out.append(table(['Template','Mount','Simulation D m'],[[w['dataName'],w['mount'],fmt(w['mirrorRadius_cm']/50)] for w in weapons if not w.get('modelResource')]))
(O/'numerical-inventory.md').write_text('\n'.join(out),encoding='utf-8')
(O/'dimensions.json').write_text(json.dumps(derived,indent=2)+'\n')
# Aperture sensitivity; lower E means less armor absorption.
names=['60cmIRPhaserBattery','60cmGreenPhaserBattery','60cmUVPhaserBattery','120cmGreenPhaserBattery','360cmGreenPhaserBattery','240cmGreenPhaserCannon','480cmGreenPhaserCannon','720cmGreenPhaserCannon','960cmGreenPhaserCannon','Alien384cmXaserBattery','Alien1024cmGraserCannon']
sensitivity=[]
for name in names:
 w=next(w for w in weapons if w['dataName']==name);d=w['mirrorRadius_cm']/50;dif=(1.22*w['wavelength_nm']*1e-9*w['beam_quality']/d)**2;jit=(2*w['jitter_Rad'])**2
 for scale in [1,1.25,1.5,2]:
  spot2=(500000**2)*(dif/scale**2+jit);E=math.pi*spot2/4/.005;c=min(.25,E/1000);P=w['shotPower_MJ']/20
  sensitivity.append({'name':name,'diameter_scale':scale,'simulation_diameter_m':d*scale,'E_500km':E,'spot_diameter_m':math.sqrt(spot2),'E_ratio':(dif/scale**2+jit)/(dif+jit),'armor_threshold_nominal':(P*(1-c)/E)**(2/3),'nominal_DPS_vs_10_armor':max(0,P*(1-c)-10**1.5*E)/w['cooldown_s'],'range_km':w['targetingRange_km'],'raw_DPS':P/w['cooldown_s'],'jitter_fraction':jit/(dif+jit)})
(O/'laser-sensitivity.json').write_text(json.dumps(sensitivity,indent=2)+'\n')
out=['# Larger laser aperture sensitivity tables\n\nGraser rows evaluate authored 0.1 nm. The native wavelength field is an integer; loaded rounding was not inspected. If it becomes zero, the already negligible aperture-growth benefit becomes exactly zero.\n\nRead with the [deferred proposal](../larger-laser-aperture-proposal-2026-10-21.md). At 500 km, normal incidence, intact armor, full intercepted shot, no bonuses or laser-resistant material; nominal values before random damage. Targeting range remains unchanged.\n']
rows=[]
for name in names:
 ss=[s for s in sensitivity if s['name']==name];base=ss[0]
 rows.append([name,base['range_km'],fmt(base['raw_DPS']),fmt(base['E_500km']),f"{base['jitter_fraction']*100:.2f}%",*[f"{100*(s['E_ratio']-1):+.2f}%" for s in ss[1:]]])
out.append(table(['Weapon','Range km','Raw damage pts/s','E at 500 km','Jitter share of E','E delta +25% D','E delta +50% D','E delta +100% D'],rows))
out.append('## Absolute nominal penetration examples\n\nArmor threshold is the continuous A for zero nominal penetrating damage; values above it stop this nominal intact hit. Thresholds are not guaranteed penetration limits because damage is randomized. DPS below is penetration through intact 10 armor, not total chip/internal damage or simulated battle results.\n')
out.append(table(['Weapon','D scale','D m','Spot m','Nominal armor threshold','Nominal DPS vs A=10'],[[s['name'],s['diameter_scale'],fmt(s['simulation_diameter_m']),fmt(s['spot_diameter_m'],4),fmt(s['armor_threshold_nominal'],2),fmt(s['nominal_DPS_vs_10_armor'])] for s in sensitivity]))
(O/'laser-sensitivity.md').write_text('\n'.join(out),encoding='utf-8')
shutil.copyfile(T/'scenes.npz',O/'measured-geometry.npz')
sources=dict(A['sources_sha256'])
for p in [R/'TIEconomyMod/Core/ProjectileCollisionMath.cs',R/'TIEconomyMod/Core/ProjectileGeometryRegistry.cs',Path(r'D:/Games/SteamLibrary/steamapps/common/Terra Invicta/TerraInvicta_Data/Managed/Assembly-CSharp.dll'),T/'TILaserWeaponTemplate.cs']:
 sources[str(p)]=hashlib.sha256(p.read_bytes()).hexdigest()
(O/'source-hashes.json').write_text(json.dumps(sources,indent=2)+'\n')
print('Resources',len(derived),'ship templates',len(rows),'all templates',len(weapons))
print((O/'laser-sensitivity.md').read_text()[:5000])

```

### `.tmp/barrel-audit/verify.py`

```python
import json,math,hashlib,runpy,contextlib,io,re
from pathlib import Path
import numpy as np
R=Path.cwd();T=R/'.tmp/barrel-audit';O=R/'docs/ship-balance-research/barrel-lens-audit'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
a=json.loads((O/'inventory.json').read_text());dims=json.loads((O/'dimensions.json').read_text());secs=json.loads((O/'sections.json').read_text());data=np.load(O/'measured-geometry.npz')
assert len(a['weapons'])==203 and sum(bool(w.get('modelResource')) for w in a['weapons'])==166 and len(dims)==41
assert {w['modelResource'] for w in a['weapons'] if w.get('modelResource')}=={r['resource'] for r in dims}
assert len({w['dataName'] for w in a['weapons']})==203
for r in a['resources']:
 for m in r['meshes']:
  v=data['v'+m['key']];t=data['t'+m['key']];assert np.isfinite(v).all() and t.min()>=0 and t.max()<len(v)
  assert np.allclose(np.ptp(v,axis=0),m['dimensions_m'],atol=1e-10)
with contextlib.redirect_stdout(io.StringIO()):section=runpy.run_path(str(T/'sections.py'))['section']
# Independent fixture: two concentric square tubes, side walls only.
vs=[];ts=[]
for size in [1,2]:
 start=len(vs)
 for z in [-1,1]:vs.extend([(x*size,y*size,z) for x,y in [(-1,-1),(1,-1),(1,1),(-1,1)]])
 for j in range(4):
  k=(j+1)%4;ts.extend([(start+j,start+k,start+4+k),(start+j,start+4+k,start+4+j)])
loops=section(np.array(vs,float),np.array(ts),.12345);assert len(loops)==2 and all(x['closed'] for x in loops)
assert np.allclose(sorted(x['width_m'] for x in loops),[2,4]);assert np.allclose(sorted(x['inscribed_diameter_m'] for x in loops),[2,4]);assert np.allclose(sorted(x['circumscribed_diameter_m'] for x in loops),np.sqrt(2)*np.array([2,4]))
# Translation/scale should preserve counts and predict measured dimensions.
loops2=section(np.array(vs,float)*3+np.array([2,5,7]),np.array(ts),7+.12345*3)
assert np.allclose(sorted(x['width_m'] for x in loops2),[6,12])
max_width_change=0
for s in secs:
 if s['index'] in [21,33,40] or 'cap_dimensions_m' in s:continue
 p,q=s['sections'][0]['loops'],s['sections'][1]['loops'];assert len(p)==len(q)
 n=len(p)//2;p=sorted(x['width_m'] for x in p)[:n];q=sorted(x['width_m'] for x in q)[:n]
 max_width_change=max(max_width_change,max(abs(x-y) for x,y in zip(p,q)))
assert max_width_change<.0001
coildelta=float(np.max(np.abs((data['v35_1']-data['v35_1'].mean(0))-(data['v28_1']-data['v28_1'].mean(0)))))
assert coildelta<.000002
ss=json.loads((O/'laser-sensitivity.json').read_text());assert len(ss)==44
for row in ss:
 w=next(w for w in a['weapons'] if w['dataName']==row['name']);d=w['mirrorRadius_cm']/50*row['diameter_scale']
 # Native expression before algebraic simplification, evaluated independently.
 native=500*1000*math.sqrt((1.22*w['wavelength_nm']/1e9*w['beam_quality'])**2+(2*w['jitter_Rad']*d)**2)/d
 assert math.isclose(native,row['spot_diameter_m'],rel_tol=1e-12)
 assert math.isclose(row['E_500km'],math.pi*native**2/4/.005,rel_tol=1e-12)
 assert 0<row['E_ratio']<=1 and row['nominal_DPS_vs_10_armor']<=row['raw_DPS']
before=json.loads((T/'runtime-before.json').read_text());changed=[p for p,h in before.items() if not (R/p).exists() or sha(R/p)!=h];assert not changed,changed
sources=json.loads((O/'source-hashes.json').read_text())
for p,h in sources.items():assert sha(Path(p))==h,p
for name in ['Damage.cs','TISpaceShipState.cs','TIShipWeaponTemplate.cs']:
 sources[str(T/name)]=sha(T/name)
for name in ['TIGunTemplate.en','TIMagneticGunTemplate.en','TILaserWeaponTemplate.en']:
 p=Path(r'D:/Games/SteamLibrary/steamapps/common/Terra Invicta/TerraInvicta_Data/StreamingAssets/Localization/en')/name;sources[str(p)]=sha(p)
for name in ['measure_ship_prefabs.py','measure_turret_clearance.py','generate_hull_variant_report.py']:
 p=R/'scripts/ship-balance'/name;sources[str(p)]=sha(p)
(O/'source-hashes.json').write_text(json.dumps(sources,indent=2)+'\n')
result={'status':'passed','ship_templates':166,'non_ship_laser_templates':37,'resources':41,'source_hashes_checked':len(sources),'runtime_files_unchanged':len(before),'geometry_index_and_bounds_checks':82,'section_fixture':'concentric square tubes plus translated/scaled repeat','max_inner_opening_change_0_1_to_1_mm_inset_m':max_width_change,'small_medium_coil_translation_aligned_max_vertex_delta_m':coildelta,'laser_formula_scenarios_checked':44,'manual_mesh_views_inspected':82,'live_game_test':False,'build_deploy_commit_this_turn':False,'graser_note':'Tables evaluate authored wavelength 0.1 nm; native field is int, loaded rounding was not inspected. At 0 nm aperture benefit would be exactly zero; conclusion unchanged.'}
(O/'verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,indent=2))

```
