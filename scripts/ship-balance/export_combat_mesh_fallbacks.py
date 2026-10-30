"""Export guarded CPU data for native meshes that Unity marks unreadable.

Run with UnityPy installed. Game bundles are read-only; output is a private
binary .eeo file, deliberately outside native recursive JSON discovery.
"""
import argparse
import gc
import hashlib
import json
from pathlib import Path
import struct
import sys
import io
import zipfile

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / '.tmp/battle-scale-camera/deps'))
from UnityPy import load
from UnityPy.helpers.MeshHelper import MeshHandler

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--game-install-dir', type=Path, default=Path(r'D:\Games\SteamLibrary\steamapps\common\Terra Invicta'))
    parser.add_argument('--output', type=Path, default=ROOT/'TIEconomyMod/ModFiles/Config/combat-unreadable-meshes.eeo')
    parser.add_argument('--manifest', type=Path, default=ROOT/'docs/ship-balance-research/battle-scale/mesh-fallback-sources-2026-10-29.json')
    args = parser.parse_args()
    base = args.game_install_dir
    bundles = [base/'TerraInvicta_Data/StreamingAssets/AssetBundles'/x for x in ['ships','habmodules','shipweapons']]
    bundles.append(base/'DLC_Content/DarkSkies/AssetBundles/ships_prm')
    sources = {str(p.relative_to(base)):sha(p) for p in bundles}
    sources['TerraInvicta_Data/Managed/Assembly-CSharp.dll'] = sha(base/'TerraInvicta_Data/Managed/Assembly-CSharp.dll')
    records = []
    for bundle in bundles:
        env = load(str(bundle))
        for obj in env.objects:
            if obj.type.name != 'Mesh': continue
            mesh = obj.read()
            if mesh.m_IsReadable: continue
            handler = MeshHandler(mesh)
            handler.process()
            assert not handler.m_BoneWeights, 'Unreadable skinned mesh needs a separate binding export'
            blend = getattr(mesh, 'm_Shapes', None)
            assert blend is None or not blend.shapes, 'Unreadable blend-shape mesh needs a separate export'
            arrays = [handler.m_Vertices, handler.m_Normals, handler.m_Tangents, handler.m_Colors]
            arrays += [getattr(handler, 'm_UV'+str(i)) for i in range(8)]
            arrays = [x or [] for x in arrays]
            # Preserve each channel's authored component count and submesh order.
            triangles = handler.get_triangles()
            count = handler.m_VertexCount
            assert len(arrays[0]) == count
            assert all(not a or len(a) == count for a in arrays)
            assert all(0 <= i < count for sub in triangles for tri in sub for i in tri)
            records.append((mesh.m_Name, count, arrays, [[i for tri in sub for i in tri] for sub in triangles], 'dlc' if bundle.name=='ships_prm' else 'base'))
        print(bundle.name, 'unreadable records so far', len(records), flush=True)
        del env
        gc.collect()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    header = io.BytesIO()
    stream = header
    def integer(value): stream.write(struct.pack('<i', value))
    def string(value):
        data = value.encode('utf-8'); integer(len(data)); stream.write(data)
    string('EEO-COMBAT-MESH-3')
    string(sources['TerraInvicta_Data/Managed/Assembly-CSharp.dll'])
    integer(len(records))
    unique = set()
    payload_paths = {kind:args.output.with_name('combat-mesh-data-'+kind+'.eeo') for kind in ['base','dlc']}
    with zipfile.ZipFile(payload_paths['base'],'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as base_zip, \
         zipfile.ZipFile(payload_paths['dlc'],'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as dlc_zip, \
         zipfile.ZipFile(args.output,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as archive:
        for name, count, arrays, submeshes, kind in records:
            stream = io.BytesIO()
            integer(count)
            for array in arrays:
                width = len(array[0]) if array else 0
                integer(width)
                if width: stream.write(struct.pack('<'+'f'*(len(array)*width), *(v for row in array for v in row)))
            integer(len(submeshes))
            for sub in submeshes:
                integer(len(sub)); stream.write(struct.pack('<'+'i'*len(sub), *sub))
            payload = stream.getvalue()
            digest = hashlib.sha256(payload).hexdigest().upper()
            key=(kind,digest)
            if key not in unique:
                (dlc_zip if kind=='dlc' else base_zip).writestr(digest,payload); unique.add(key)
            stream = header
            string(name); integer(count); string(digest); string(payload_paths[kind].name)
        archive.writestr('header',header.getvalue())
    manifest = {'schema':3,'sources':sources,'file':str(args.output.relative_to(ROOT)),'unique_payloads':len(unique),
                'payload_files':{str(p.relative_to(ROOT)):sha(p) for p in payload_paths.values()},
                'sha256':sha(args.output),'meshes':[{'name':r[0],'vertices':r[1],'submeshes':len(r[3])} for r in records]}
    args.manifest.parent.mkdir(parents=True, exist_ok=True)
    args.manifest.write_text(json.dumps(manifest,indent=2)+'\n')
    print('Wrote',len(records),'guarded meshes;',args.output.stat().st_size,'bytes')

if __name__ == '__main__': main()
