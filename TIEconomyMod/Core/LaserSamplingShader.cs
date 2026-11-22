using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            public bool nativeVertexUnchanged=false;
            public ApertureManifest aperture=null;
        }

        private sealed class ApertureManifest
        {
            public string bundleSha256=null;
            public Variant[] shaders=null;
        }

        private sealed class Variant
        {
            public string nativeShader=null, shaderName=null, assetName=null;
            public float carrierRatio=0;
        }

        private const string CommonShader = "TerraInvicta/Beam Weapon Line";
        private const string FlowShader = "TerraInvicta/Laser Beam No Middle";
        private static AssetBundle particleBundle, apertureBundle;
        private static Shader particleShader;
        private static Dictionary<string, (Shader Shader, float Ratio)> apertureShaders;
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
            if (apertureShaders != null) return;
            string config = Path.Combine(modPath, "Config");
            var manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(config, "laser-ribbon-manifest.eeo")));
            if (manifest == null || manifest.schema != 3 ||
                manifest.graphicsApi != "Direct3D11" || manifest.shaderName != "EEO/Beam Weapon Line" ||
                manifest.assetName != "assets/eeo/beam-ribbon.shader" || !manifest.nativeVertexUnchanged ||
                manifest.aperture?.shaders == null || manifest.aperture.shaders.Length != 2)
                throw new InvalidDataException("Unsupported laser sampling manifest.");
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11)
                throw new NotSupportedException("Laser sampling shader requires the audited Direct3D11 backend.");
            Verify(nativeAssembly, manifest.nativeAssemblySha256);
            Verify(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(nativeAssembly), "..", "StreamingAssets", "AssetBundles", "spacecombat")), manifest.nativeBundleSha256);
            try
            {
                particleBundle = LoadBundle(config, "laser-ribbon.eeo", manifest.bundleSha256);
                apertureBundle = LoadBundle(config, "laser-aperture.eeo", manifest.aperture.bundleSha256);
                particleShader = LoadShader(particleBundle, manifest.assetName, manifest.shaderName);
                var calibrated = new Dictionary<string, (Shader, float)>(StringComparer.Ordinal);
                foreach (Variant variant in manifest.aperture.shaders)
                {
                    if (variant == null || !(variant.carrierRatio > 0f && variant.carrierRatio <= (float)LaserSamplingMath.LegacyCarrierRatio) ||
                        (variant.nativeShader != CommonShader && variant.nativeShader != FlowShader))
                        throw new InvalidDataException("Invalid optical beam calibration variant.");
                    calibrated.Add(variant.nativeShader,
                        (LoadShader(apertureBundle, variant.assetName, variant.shaderName), variant.carrierRatio));
                }
                apertureShaders = calibrated;
            }
            catch
            {
                if (particleBundle != null) particleBundle.Unload(true);
                if (apertureBundle != null) apertureBundle.Unload(true);
                particleBundle = apertureBundle = null;
                particleShader = null;
                throw;
            }
            Main.Log("Loaded aperture-calibrated core and flow shaders with native vertices and particle fallback.");
        }

        private static AssetBundle LoadBundle(string directory, string file, string expectedHash)
        {
            string path = Path.Combine(directory, file);
            Verify(path, expectedHash);
            AssetBundle asset = AssetBundle.LoadFromFile(path);
            if (asset == null)
                throw new InvalidDataException("Laser sampling bundle did not load: " + file);
            return asset;
        }

        private static Shader LoadShader(AssetBundle asset, string path, string name)
        {
            Shader shader = asset.LoadAsset<Shader>(path);
            if (shader == null || shader.name != name || !shader.isSupported)
                throw new InvalidDataException("Laser sampling shader is missing or unsupported: " + name);
            return shader;
        }

        public static bool TryResolve(Shader native, bool particle, out Shader shader, out float ratio)
        {
            shader = null;
            ratio = (float)LaserSamplingMath.LegacyCarrierRatio;
            if (native == null || apertureShaders == null)
                return false;
            if (particle)
            {
                shader = native.name == CommonShader ? particleShader : null;
                return shader != null;
            }
            if (!apertureShaders.TryGetValue(native.name, out var variant))
                return false;
            shader = variant.Shader;
            ratio = variant.Ratio;
            return true;
        }
    }
}
