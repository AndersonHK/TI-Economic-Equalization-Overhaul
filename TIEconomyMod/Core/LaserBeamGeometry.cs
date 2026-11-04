using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace TIEconomyMod.Core
{
    public sealed class LaserOpticsProfile
    {
        [JsonProperty("resource", Required = Required.Always)] public string Resource;
        [JsonProperty("meshName", Required = Required.Always)] public string MeshName;
        [JsonProperty("xRatio", Required = Required.Always)] public float XRatio;
        [JsonProperty("yRatio", Required = Required.Always)] public float YRatio;
        public float WorldDiameter(Vector3 localSize, Vector3 worldScale)
        {
            return Math.Min(Math.Abs(localSize.x * worldScale.x) * XRatio,
                Math.Abs(localSize.y * worldScale.y) * YRatio);
        }
        public bool Matches(Mesh mesh)
        {
            return mesh != null && (mesh.name == MeshName || mesh.name.StartsWith(MeshName + " [EEO", StringComparison.Ordinal) || mesh.name == MeshName + " Instance");
        }
    }
    public sealed class LaserOpticsDocument
    {
        [JsonProperty("schema", Required = Required.Always)] public int Schema;
        [JsonProperty("assemblySha256", Required = Required.Always)] public string AssemblySha256;
        [JsonProperty("weaponBundleSha256", Required = Required.Always)] public string WeaponBundleSha256;
        [JsonProperty("profiles", Required = Required.Always)] public LaserOpticsProfile[] Profiles;
    }
    public sealed class LaserOpticsCatalog
    {
        private readonly Dictionary<string, LaserOpticsProfile> profiles = new Dictionary<string, LaserOpticsProfile>(StringComparer.OrdinalIgnoreCase);
        public int Count { get { return profiles.Count; } }
        public bool TryGet(string resource, out LaserOpticsProfile profile)
        { profile = null; return resource != null && profiles.TryGetValue(resource, out profile); }
        public static LaserOpticsCatalog Load(string path, string assembly, string bundle)
        {
            var doc = JsonConvert.DeserializeObject<LaserOpticsDocument>(File.ReadAllText(path),
                new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            if (doc == null || doc.Schema != 1 || doc.Profiles == null || doc.Profiles.Length != 16)
                throw new InvalidDataException("Laser optics requires schema 1 and sixteen measured resources.");
            CheckHash(assembly, doc.AssemblySha256); CheckHash(bundle, doc.WeaponBundleSha256);
            var catalog = new LaserOpticsCatalog();
            foreach (var p in doc.Profiles)
            {
                if (p == null || String.IsNullOrEmpty(p.Resource) || String.IsNullOrEmpty(p.MeshName) ||
                    !ValidRatio(p.XRatio) || !ValidRatio(p.YRatio) || catalog.profiles.ContainsKey(p.Resource))
                    throw new InvalidDataException("Invalid or duplicate laser aperture profile.");
                catalog.profiles.Add(p.Resource, p);
            }
            return catalog;
        }
        private static bool ValidRatio(float x) { return !float.IsNaN(x) && !float.IsInfinity(x) && x > 0f && x <= 1f; }
        private static void CheckHash(string path, string expected)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                if (!String.Equals(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", ""), expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Laser optics source changed: " + Path.GetFileName(path));
        }
    }

    // Native OnEnable derives UV scale from the whole effect transform. Keep
    // vanilla longitudinal spacing when the ship scale changes; V stays native.
    public sealed class LaserTexturePeriodState
    {
        private bool captured;
        private Vector2 original;
        public Vector2 Configure(Vector2 nativeScale,float modelRatio)
        {
            if(float.IsNaN(modelRatio)||float.IsInfinity(modelRatio)||modelRatio<=0f)
                throw new ArgumentOutOfRangeException(nameof(modelRatio));
            if(!captured){original=nativeScale;captured=true;}
            return new Vector2(original.x*modelRatio,original.y);
        }
        public Vector2 Restore(Vector2 current)
        {if(!captured)return current;captured=false;return original;}
        public void Reset(){captured=false;}
    }
    public sealed class LaserBeamGeometry : MonoBehaviour
    {
        private ShipWeaponVisController emitter;
        private LineRenderer line;
        private Material beamMaterial;
        private Shader originalShader;
        private bool sampling;
        private float ribbonWidth;
        private float originalMultiplier;
        private bool captured,active;
        private readonly LaserTexturePeriodState texturePeriod=new LaserTexturePeriodState();
        private static readonly HashSet<string> Reports=new HashSet<string>();
        public void Bind(ShipWeaponVisController weapon)
        {
            emitter=weapon;
            if(line==null)line=GetComponentInChildren<LineRenderer>(true);
            if(line==null)return;
            if(!captured){originalMultiplier=line.widthMultiplier;captured=true;}
            active=true;Configure();
        }
        public bool Apply()
        {
            if(!active||line==null||emitter==null||
                (!(emitter.weaponTemplate is TILaserWeaponTemplate) && !(emitter.weaponTemplate is TIParticleWeaponTemplate)))return false;
            if (Patches.BattleScaleRuntime.Enabled&&TIGlobalValuesState.isSpaceCombatEnabled) return true;
            Restore(); return false;
        }
        private void Configure()
        {
            bool particle=emitter.weaponTemplate is TIParticleWeaponTemplate;
            float diameter=0f;bool measured=false;LaserOpticsProfile profile;
            if (particle)
            {
                // Same authored base term as native OnWillRenderObject, captured
                // at activation. Exclude its independent camera-distance floor.
                Vector3 scale=transform.lossyScale;
                float inherited=Math.Max(Math.Abs(scale.x),Math.Max(Math.Abs(scale.y),Math.Abs(scale.z)));
                ribbonWidth=(float)LaserSamplingMath.ParticleRibbonWidth(originalMultiplier,Math.Max(inherited,1e-12f));
            }
            else
            {
                MeshFilter mesh=emitter.weaponObject==null?null:emitter.weaponObject.GetComponent<MeshFilter>();
                if(Main.laserBeamOptics!=null&&Main.laserBeamOptics.TryGet(emitter.weaponTemplate.modelResource,out profile)&&
                    mesh!=null&&profile.Matches(mesh.sharedMesh))
                {diameter=profile.WorldDiameter(mesh.sharedMesh.bounds.size,mesh.transform.lossyScale);measured=true;}
                if(diameter<=0f)
                {
                    var laser=(TILaserWeaponTemplate)emitter.weaponTemplate;
                    diameter=Math.Max(1e-9f,laser.mirrorRadius_cm*.02f*Patches.BattleScaleRuntime.ModelScale);
                }
                // Fixed approved calibration only. Keep the authored shader profile.
                ribbonWidth=(float)LaserSamplingMath.RibbonWidth(diameter);
            }
            line.widthMultiplier=ribbonWidth;
            // Native BeamEffectController already instances this material in Awake.
            Material material=line.sharedMaterial;
            if(material!=beamMaterial)
            {
                if(beamMaterial!=null&&beamMaterial.HasProperty("_MainTex"))
                    beamMaterial.mainTextureScale=texturePeriod.Restore(beamMaterial.mainTextureScale);
                else texturePeriod.Reset();
                if (sampling && beamMaterial != null) beamMaterial.shader=originalShader;
                sampling=false;
                beamMaterial=material;
            }
            if(beamMaterial!=null&&beamMaterial.HasProperty("_MainTex"))
                beamMaterial.mainTextureScale=texturePeriod.Configure(beamMaterial.mainTextureScale,Patches.BattleScaleRuntime.ModelRatio);
            string key=emitter.weaponTemplate.dataName+":"+Patches.BattleScaleRuntime.Cinematic;
            bool report=!Reports.Contains(key);
            bool hasTint=report && beamMaterial!=null && beamMaterial.HasProperty("_Color");
            Color beforeTint=hasTint ? beamMaterial.GetColor("_Color") : default(Color);
            string beforeShader=report && beamMaterial!=null && beamMaterial.shader!=null ? beamMaterial.shader.name : "none";
            Vector2 beforeUV=report && beamMaterial!=null && beamMaterial.HasProperty("_MainTex") ? beamMaterial.mainTextureScale : default(Vector2);
            Texture beforeTexture=report && beamMaterial!=null && beamMaterial.HasProperty("_MainTex") ? beamMaterial.mainTexture : null;
            if (beamMaterial != null && LaserSamplingShader.Shader != null &&
                (sampling || (beamMaterial.shader != null && beamMaterial.shader.name == "TerraInvicta/Beam Weapon Line")))
            {
                if (!sampling) { originalShader=beamMaterial.shader; beamMaterial.shader=LaserSamplingShader.Shader; sampling=true; }
            }
            if(Reports.Add(key))Main.Log("Laser native ribbon: "+key+" measured="+measured+
                " particle="+particle+" widthSource="+(particle ? "authoredEffectScale" : "graphicalAperture")+
                " apertureDiameter="+diameter.ToString("G7")+" ribbonWidth="+ribbonWidth.ToString("G7")+
                " filtered="+sampling+" longitudinalScaleRatio="+Patches.BattleScaleRuntime.ModelRatio.ToString("G7"));
            if (report && beamMaterial!=null)
            {
                Color afterTint=hasTint ? beamMaterial.GetColor("_Color") : default(Color);
                Vector2 afterUV=beamMaterial.HasProperty("_MainTex") ? beamMaterial.mainTextureScale : default(Vector2);
                Texture texture=beamMaterial.HasProperty("_MainTex") ? beamMaterial.mainTexture : null;
                Main.Log("Laser material audit: "+key+" particle="+particle+" effect="+emitter.weaponTemplate.effectResource+
                    " shaderBefore="+beforeShader+" shaderAfter="+(beamMaterial.shader!=null ? beamMaterial.shader.name : "none")+
                    " tintBefore="+TintText(beforeTint)+" tintAfter="+TintText(afterTint)+
                    " tintChanged="+!beforeTint.Equals(afterTint)+
                    " textureBefore="+(beforeTexture!=null ? beforeTexture.name : "none")+
                    " textureAfter="+(texture!=null ? texture.name : "none")+
                    " textureChanged="+(beforeTexture!=texture)+
                    " uvBefore="+beforeUV+" uvAfter="+afterUV+" uvChanged="+!beforeUV.Equals(afterUV));
            }
        }
        private static string TintText(Color c)
        {
            var culture=System.Globalization.CultureInfo.InvariantCulture;
            return "("+c.r.ToString("G7",culture)+","+c.g.ToString("G7",culture)+","+
                c.b.ToString("G7",culture)+","+c.a.ToString("G7",culture)+")";
        }
        public void Restore()
        {
            if(captured&&line!=null)line.widthMultiplier=originalMultiplier;
            if(beamMaterial!=null&&beamMaterial.HasProperty("_MainTex"))
                beamMaterial.mainTextureScale=texturePeriod.Restore(beamMaterial.mainTextureScale);
            if (sampling && beamMaterial != null) beamMaterial.shader=originalShader;
            sampling=false;
            active=false;
        }
    }
}
