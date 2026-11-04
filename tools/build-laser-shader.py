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
ASSEMBLY_HASH = "4A4B9AAE4154E444E9727204205D2D42AE8ED9E1C5F92CDC1280074A259D8350"
ASSET_NAME = "assets/eeo/beam-ribbon.shader"
SHADER_NAME = "EEO/Beam Weapon Line"


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


def compile_program(source, entry, target, vertex=False):
    compiler = ctypes.WinDLL("d3dcompiler_47.dll")
    compile_shader = compiler.D3DCompile
    compile_shader.argtypes = [ctypes.c_void_p, ctypes.c_size_t, ctypes.c_char_p,
                              ctypes.c_void_p, ctypes.c_void_p, ctypes.c_char_p,
                              ctypes.c_char_p, ctypes.c_uint, ctypes.c_uint,
                              ctypes.POINTER(ctypes.c_void_p), ctypes.POINTER(ctypes.c_void_p)]
    compile_shader.restype = ctypes.c_long
    text = (b"#define VERTEX_STAGE 1\n" if vertex else b"") + source
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


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-dir", required=True, type=Path)
    parser.add_argument("--unitypy-dir", type=Path)
    args = parser.parse_args()
    if args.unitypy_dir:
        sys.path.insert(0, str(args.unitypy_dir.resolve()))
    import UnityPy
    import lz4.block
    from UnityPy.export.ShaderConverter import ShaderProgram
    from UnityPy.streams import EndianBinaryReader

    source_bundle = args.game_dir / "TerraInvicta_Data/StreamingAssets/AssetBundles/spacecombat"
    assembly = args.game_dir / "TerraInvicta_Data/Managed/Assembly-CSharp.dll"
    assert digest(source_bundle.read_bytes()) == SOURCE_HASH, "Unsupported native bundle"
    assert digest(assembly.read_bytes()) == ASSEMBLY_HASH, "Unsupported native assembly"
    env = UnityPy.load(str(source_bundle))
    shader_object = next(o for o in env.objects if o.type.name == "Shader" and
                         o.read().m_ParsedForm.m_Name == "TerraInvicta/Beam Weapon Line")
    assets = shader_object.assets_file
    assert assets.unity_version == "2020.3.49f1"
    asset_bundle = next(o for o in assets.objects.values() if o.type.name == "AssetBundle")
    tree = shader_object.read_typetree()
    form = tree["m_ParsedForm"]
    assert len(form["m_SubShaders"]) == 1 and len(form["m_SubShaders"][0]["m_Passes"]) == 1
    native_blob = lz4.block.decompress(bytes(tree["compressedBlob"]),
                                       uncompressed_size=tree["decompressedLengths"][0][0])
    native_programs = ShaderProgram(EndianBinaryReader(native_blob, endian="<"), (2020, 3, 49, 1))
    hlsl = (ROOT / "TIEconomyMod/Shaders/BeamRibbon.hlsl").read_bytes()
    programs = [native_programs.m_SubPrograms[0].m_ProgramCode[38:], compile_program(hlsl, "Frag", "ps_4_0")]
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
    form["m_Name"] = SHADER_NAME
    # Native properties, channels, constant buffers and all bindings stay intact.
    shader_object.save_typetree(tree)
    ab = asset_bundle.read_typetree()
    reference = dict(m_FileID=0, m_PathID=shader_object.path_id)
    info = dict(preloadIndex=0, preloadSize=1, asset=reference)
    ab.update(m_Name="eeo-laser-ribbon", m_AssetBundleName="eeo-laser-ribbon",
              m_PreloadTable=[reference], m_Container=[(ASSET_NAME, info)], m_MainAsset=info,
              m_Dependencies=[])
    asset_bundle.save_typetree(ab)
    assets.objects = {o.path_id: o for o in [asset_bundle, shader_object]}
    bundle = next(iter(env.files.values()))
    # Distinct CAB identity avoids collision with the already loaded native bundle.
    assets.name = "CAB-eeo-laser-ribbon-0114"
    bundle.files = {assets.name: assets}
    data = bundle.save(packer="lz4")
    output = ROOT / "TIEconomyMod/ModFiles/Config/laser-ribbon.eeo"
    output.write_bytes(data)
    check = UnityPy.load(data)
    assert sorted(o.type.name for o in check.objects) == ["AssetBundle", "Shader"]
    assert next(o.read().m_ParsedForm.m_Name for o in check.objects if o.type.name == "Shader") == SHADER_NAME
    manifest = dict(schema=1, shaderName=SHADER_NAME, assetName=ASSET_NAME,
        unityVersion="2020.3.49f1", graphicsApi="Direct3D11", coreRibbonRatio=0.46,
        nativeAssemblySha256=ASSEMBLY_HASH, nativeBundleSha256=SOURCE_HASH,
        nativeVertexUnchanged=True, vertexSha256=digest(programs[0]),
        hlslSha256=digest(hlsl), bundleSha256=digest(data), bundleBytes=len(data),
        programs=[dict(stage=stage, sha256=digest(code), bytes=len(code))
                  for stage, code in zip(["vertex", "fragment"], programs)])
    (output.parent / "laser-ribbon-manifest.eeo").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
