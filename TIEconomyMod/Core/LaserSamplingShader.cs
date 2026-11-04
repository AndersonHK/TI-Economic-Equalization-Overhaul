using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace TIEconomyMod.Core
{
    public static class LaserSamplingShader
    {
        private sealed class Manifest
        {
            public int schema=0;
            public string shaderName=null, assetName=null, graphicsApi=null, nativeAssemblySha256=null,
                nativeBundleSha256=null, bundleSha256=null;
            public double coreRibbonRatio=0;
            public bool nativeVertexUnchanged=false;
        }
        private static AssetBundle bundle;
        public static Shader Shader { get; private set; }
        private static void Verify(string path, string expected)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                if (!String.Equals(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", ""), expected,
                    StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Laser sampling source changed: " + Path.GetFileName(path));
        }
        public static void Load(string modPath, string nativeAssembly)
        {
            if (Shader != null) return;
            string config = Path.Combine(modPath, "Config");
            var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(config, "laser-ribbon-manifest.eeo")));
            if (manifest == null || manifest.schema != 1 || manifest.coreRibbonRatio != LaserSamplingMath.CoreRibbonRatio ||
                manifest.graphicsApi != "Direct3D11" || manifest.shaderName != "EEO/Beam Weapon Line" ||
                manifest.assetName != "assets/eeo/beam-ribbon.shader" || !manifest.nativeVertexUnchanged)
                throw new InvalidDataException("Unsupported laser sampling manifest.");
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11)
                throw new NotSupportedException("Laser sampling shader requires the audited Direct3D11 backend.");
            Verify(nativeAssembly, manifest.nativeAssemblySha256);
            Verify(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(nativeAssembly), "..", "StreamingAssets", "AssetBundles", "spacecombat")), manifest.nativeBundleSha256);
            string asset = Path.Combine(config, "laser-ribbon.eeo");
            Verify(asset, manifest.bundleSha256);
            bundle = AssetBundle.LoadFromFile(asset);
            if (bundle == null) throw new InvalidDataException("Laser sampling bundle did not load.");
            Shader shader = bundle.LoadAsset<Shader>(manifest.assetName);
            if (shader == null || shader.name != manifest.shaderName || !shader.isSupported)
            {
                bundle.Unload(true); bundle = null;
                throw new InvalidDataException("Laser sampling shader is missing or unsupported.");
            }
            Shader = shader;
            Main.Log("Loaded laser sampling shader with unchanged native vertices and resolved flow profile; fixed 46% ribbon calibration.");
        }
    }
}
