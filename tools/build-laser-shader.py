"""Compile the EEO native-style D3D11 beam variant; never starts Unity or a GPU.

Requires UnityPy 1.25.4/lz4 and Windows d3dcompiler_47. Example:
python tools/build-laser-shader.py --game-dir GAME --unitypy-dir DEPS
The installed source bundle is read-only. Output contains Shader + AssetBundle only.
"""
import argparse
import ctypes
import hashlib
import json
from pathlib import Path
import struct
import sys

ROOT = Path(__file__).resolve().parents[1]
SOURCE_HASH = "27F37070CBD1FC29275CB5FC38D3073DE24C10802AC441B5FAD39A54F573B90F"
ASSEMBLY_HASH = "64D88E996C63B4254675D608E859AAC523B43488625937D8E2E9B1E04A3E8B3B"
ASSET_NAME = "assets/eeo/beam-ribbon.shader"
SHADER_NAME = "EEO/Beam Weapon Line"
APERTURE_BUNDLE = "laser-aperture.eeo"
NATIVE_CORE = "TerraInvicta/Beam Weapon Line"
APERTURE_VARIANTS = (
    (NATIVE_CORE, "core", 0.46, {"LENS_CALIBRATION": 1}),
    ("TerraInvicta/Laser Beam No Middle", "flow", 0.35, {"LENS_CALIBRATION": 1, "FLOW_ONLY": 1}),
)


def digest(data):
    return hashlib.sha256(data).hexdigest().upper()


def blob_bytes(blob):
    table = ctypes.cast(blob, ctypes.POINTER(ctypes.POINTER(ctypes.c_void_p))).contents
    pointer = ctypes.WINFUNCTYPE(ctypes.c_void_p, ctypes.c_void_p)(table[3])
    size = ctypes.WINFUNCTYPE(ctypes.c_size_t, ctypes.c_void_p)(table[4])
    release = ctypes.WINFUNCTYPE(ctypes.c_ulong, ctypes.c_void_p)(table[2])
    data = ctypes.string_at(pointer(blob), size(blob))
    release(blob)
    return data


def compile_program(source, entry, target, defines=None):
    compiler = ctypes.WinDLL("d3dcompiler_47.dll")
    compile_shader = compiler.D3DCompile
    compile_shader.argtypes = [ctypes.c_void_p, ctypes.c_size_t, ctypes.c_char_p,
                              ctypes.c_void_p, ctypes.c_void_p, ctypes.c_char_p,
                              ctypes.c_char_p, ctypes.c_uint, ctypes.c_uint,
                              ctypes.POINTER(ctypes.c_void_p), ctypes.POINTER(ctypes.c_void_p)]
    compile_shader.restype = ctypes.c_long
    header = "".join(f"#define {key} {value}\n" for key, value in (defines or {}).items())
    text = header.encode("ascii") + source
    buffer = ctypes.create_string_buffer(text)
    output, errors = ctypes.c_void_p(), ctypes.c_void_p()
    # Strict source checking and optimization; no graphics device is created.
    result = compile_shader(buffer, len(text), b"BeamRibbon.hlsl", None, None,
                            entry.encode(), target.encode(), (1 << 11) | (1 << 15), 0,
                            ctypes.byref(output), ctypes.byref(errors))
    messages = blob_bytes(errors).decode("utf-8", "replace") if errors else ""
    if result < 0:
        raise RuntimeError(messages)
    if messages.strip():
        raise RuntimeError("Shader compiler warning: " + messages)
    return blob_bytes(output)


def pack_shader(shader_object, shader_name, fragment):
    import lz4.block
    from UnityPy.export.ShaderConverter import ShaderProgram
    from UnityPy.streams import EndianBinaryReader

    tree = shader_object.read_typetree()
    form = tree["m_ParsedForm"]
    assert len(form["m_SubShaders"]) == 1 and len(form["m_SubShaders"][0]["m_Passes"]) == 1
    native_blob = lz4.block.decompress(bytes(tree["compressedBlob"]),
                                       uncompressed_size=tree["decompressedLengths"][0][0])
    native_programs = ShaderProgram(EndianBinaryReader(native_blob, endian="<"), (2020, 3, 49, 1))
    programs = [native_programs.m_SubPrograms[0].m_ProgramCode[38:], fragment]
    vertex_offset, vertex_size, _ = struct.unpack_from("<3i", native_blob, 4)
    # Do not recompile, reconstruct or change the native vertex program/wrapper.
    segments = [native_blob[vertex_offset:vertex_offset + vertex_size]]
    old = native_programs.m_SubPrograms[1]
    wrapped = old.m_ProgramCode[:38] + programs[1]
    header = struct.pack("<9i", old.m_Version, int(old.m_ProgramType), 0, 1, 0, 0, 0, 0, len(wrapped))
    pixel_offset, pixel_size, _ = struct.unpack_from("<3i", native_blob, 16)
    old_code_end = (pixel_offset + 36 + len(old.m_ProgramCode) + 3) & ~3
    native_pixel_tail = native_blob[old_code_end:pixel_offset + pixel_size]
    segments.append(header + wrapped + b"\0" * (-len(wrapped) % 4) + native_pixel_tail)
    offset = 28
    blob = struct.pack("<i", 2)
    for segment in segments:
        blob += struct.pack("<3i", offset, len(segment), 0)
        offset += len(segment)
    blob += b"".join(segments)
    compressed = lz4.block.compress(blob, store_size=False)
    tree.update(compressedBlob=list(compressed), offsets=[[0]],
                compressedLengths=[[len(compressed)]], decompressedLengths=[[len(blob)]])
    form["m_Name"] = shader_name
    # Native properties, channels, constant buffers and all bindings stay intact.
    shader_object.save_typetree(tree)
    return programs


def pack_bundle(env, shaders, cab):
    assets = shaders[0][1].assets_file
    assert assets.unity_version == "2020.3.49f1"
    assert all(o.assets_file == assets for _, o in shaders)
    asset_bundle = next(o for o in assets.objects.values() if o.type.name == "AssetBundle")
    ab = asset_bundle.read_typetree()
    refs = [dict(m_FileID=0, m_PathID=shader.path_id) for _, shader in shaders]
    info = [(path, dict(preloadIndex=i, preloadSize=1, asset=refs[i]))
            for i, (path, _) in enumerate(shaders)]
    ab.update(m_Name=cab, m_AssetBundleName=cab,
              m_PreloadTable=refs, m_Container=info, m_MainAsset=info[0][1],
              m_Dependencies=[])
    asset_bundle.save_typetree(ab)
    assets.objects = {o.path_id: o for o in [asset_bundle] + [o for _, o in shaders]}
    bundle = next(iter(env.files.values()))
    # Distinct CAB identity avoids collision with the already loaded native bundle.
    assets.name = cab
    bundle.files = {assets.name: assets}
    return bundle.save(packer="lz4")


def find_shaders(env):
    return {obj.read().m_ParsedForm.m_Name: obj for obj in env.objects if obj.type.name == "Shader"}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-dir", required=True, type=Path)
    parser.add_argument("--unitypy-dir", type=Path)
    args = parser.parse_args()
    if args.unitypy_dir:
        sys.path.insert(0, str(args.unitypy_dir.resolve()))
    import UnityPy

    data = args.game_dir / "TerraInvicta_Data"
    source_bundle = data / "StreamingAssets/AssetBundles/spacecombat"
    assert digest(source_bundle.read_bytes()) == SOURCE_HASH, "Unsupported native bundle"
    assert digest((data / "Managed/Assembly-CSharp.dll").read_bytes()) == ASSEMBLY_HASH, "Unsupported native assembly"
    hlsl = (ROOT / "TIEconomyMod/Shaders/BeamRibbon.hlsl").read_bytes()
    config = ROOT / "TIEconomyMod/ModFiles/Config"
    # Keep the original program for the particle beam fallback.
    env = UnityPy.load(str(source_bundle))
    particle = find_shaders(env)[NATIVE_CORE]
    programs = pack_shader(particle, SHADER_NAME, compile_program(hlsl, "Frag", "ps_4_0"))
    legacy = pack_bundle(env, [(ASSET_NAME, particle)], "CAB-eeo-laser-ribbon-0114")

    env = UnityPy.load(str(source_bundle))
    native = find_shaders(env)
    variants, entries = [], []
    for name, short, ratio, defines in APERTURE_VARIANTS:
        shader_name, path = "EEO/Calibrated Beam " + short, f"assets/eeo/lens-{short}.shader"
        variant = native[name]
        vertex, pixel = pack_shader(variant, shader_name, compile_program(hlsl, "Frag", "ps_4_0", defines))
        variants.append((path, variant))
        entries.append(dict(nativeShader=name, shaderName=shader_name, assetName=path,
                            carrierRatio=ratio, vertexSha256=digest(vertex), fragmentSha256=digest(pixel)))
    aperture = pack_bundle(env, variants, "CAB-eeo-laser-aperture")
    check = UnityPy.load(aperture)
    assert find_shaders(check).keys() == {entry["shaderName"] for entry in entries}
    manifest = dict(schema=3, shaderName=SHADER_NAME, assetName=ASSET_NAME,
        unityVersion="2020.3.49f1", graphicsApi="Direct3D11",
        nativeAssemblySha256=ASSEMBLY_HASH, nativeBundleSha256=SOURCE_HASH,
        nativeVertexUnchanged=True, vertexSha256=digest(programs[0]),
        hlslSha256=digest(hlsl), bundleSha256=digest(legacy), bundleBytes=len(legacy),
        programs=[dict(stage=stage, sha256=digest(code), bytes=len(code))
                  for stage, code in zip(["vertex", "fragment"], programs)],
        aperture=dict(bundleSha256=digest(aperture), shaders=entries))
    (config/"laser-ribbon.eeo").write_bytes(legacy)
    (config/APERTURE_BUNDLE).write_bytes(aperture)
    (config/"laser-ribbon-manifest.eeo").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
