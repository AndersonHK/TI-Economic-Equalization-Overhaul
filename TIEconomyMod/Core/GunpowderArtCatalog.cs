using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Independent visual schema. This file is never deserialized as a game template.
    public sealed class GunpowderArtCatalog
    {
        private static readonly HashSet<string> AllowedWeapons = new HashSet<string>(StringComparer.Ordinal)
        {
            "30mmAutocannon", "35mmAutocannon", "40mmAutocannon", "40mmNoseAutocannon",
            "6-inchCannon", "8-inchCannon", "10-inchCannon", "12-inchCannon"
        };
        private readonly Dictionary<string, GunpowderArtProfile> profiles;
        private GunpowderArtCatalog(Dictionary<string, GunpowderArtProfile> values) { profiles = values; }
        public int Count { get { return profiles.Count; } }
        public bool TryGet(string name, out GunpowderArtProfile profile)
        {
            profile = null;
            return name != null && profiles.TryGetValue(name, out profile);
        }

        public static GunpowderArtCatalog Load(string path, string assemblyPath, string bundlePath)
        {
            GunpowderArtDocument doc = JsonConvert.DeserializeObject<GunpowderArtDocument>(
                File.ReadAllText(path), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            if (doc == null || doc.Schema != 1 || doc.Profiles == null || doc.Profiles.Length != 8)
                throw new InvalidDataException("Gunpowder art requires schema 1 and exactly eight profiles.");
            CheckHash(assemblyPath, doc.AssemblySha256);
            CheckHash(bundlePath, doc.WeaponBundleSha256);
            var result = new Dictionary<string, GunpowderArtProfile>(StringComparer.Ordinal);
            foreach (GunpowderArtProfile p in doc.Profiles)
            {
                if (p == null || p.Weapon == null || !AllowedWeapons.Contains(p.Weapon) || result.ContainsKey(p.Weapon))
                    throw new InvalidDataException("Unknown or duplicate gunpowder art weapon.");
                p.Validate();
                result.Add(p.Weapon, p);
            }
            return new GunpowderArtCatalog(result);
        }

        private static void CheckHash(string path, string expected)
        {
            if (expected == null || !Regex.IsMatch(expected, "^[0-9A-Fa-f]{64}$"))
                throw new InvalidDataException("Invalid gunpowder source hash.");
            string actual;
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create()) actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
            if (!String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Gunpowder art source changed: " + Path.GetFileName(path));
        }

        internal static bool Finite(double value) { return !Double.IsNaN(value) && !Double.IsInfinity(value); }
        internal static void CheckVector(float[] values, bool positive)
        {
            if (values == null || values.Length != 3) throw new InvalidDataException("Expected a three-component art vector.");
            foreach (float x in values)
                if (!Finite(x) || (positive && x <= 0f)) throw new InvalidDataException("Invalid art vector component.");
        }
        public static Vector3 Vector(float[] values) { return new Vector3(values[0], values[1], values[2]); }
        public static bool Near(Vector3 a, Vector3 b)
        {
            return Math.Abs(a.x - b.x) <= .0005f && Math.Abs(a.y - b.y) <= .0005f && Math.Abs(a.z - b.z) <= .0005f;
        }
        public static bool MaterialLayoutMatches(int baseSubmeshes, int weaponSubmeshes, int baseMaterials, int weaponMaterials)
        {
            return baseSubmeshes == 1 && weaponSubmeshes == 1 && baseMaterials == 1 && weaponMaterials == 1;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GunpowderArtDocument
    {
        [JsonProperty("schema", Required = Required.Always)] public int Schema { get; private set; }
        [JsonProperty("assemblySha256", Required = Required.Always)] public string AssemblySha256 { get; private set; }
        [JsonProperty("weaponBundleSha256", Required = Required.Always)] public string WeaponBundleSha256 { get; private set; }
        [JsonProperty("profiles", Required = Required.Always)] public GunpowderArtProfile[] Profiles { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GunpowderArtProfile
    {
        [JsonProperty("weapon", Required = Required.Always)] public string Weapon { get; private set; }
        [JsonProperty("resource", Required = Required.Always)] public string Resource { get; private set; }
        [JsonProperty("caliberMm", Required = Required.Always)] public double CaliberMm { get; private set; }
        [JsonProperty("sourceInscribedMm", Required = Required.Always)] public double SourceInscribedMm { get; private set; }
        [JsonProperty("rootScale", Required = Required.Always)] public float[] RootScale { get; private set; }
        [JsonProperty("childScale", Required = Required.Always)] public float[] ChildScale { get; private set; }
        [JsonProperty("childPosition", Required = Required.Always)] public float[] ChildPosition { get; private set; }
        [JsonProperty("firePointPosition", Required = Required.Always)] public float[] FirePointPosition { get; private set; }
        [JsonProperty("baseMesh", Required = Required.Always)] public GunpowderMeshFingerprint BaseMesh { get; private set; }
        [JsonProperty("movingMesh", Required = Required.Always)] public GunpowderMeshFingerprint MovingMesh { get; private set; }
        public float Factor { get { return (float)(CaliberMm / SourceInscribedMm); } }
        public Vector3 FinalBaseScale { get { return GunpowderArtCatalog.Vector(RootScale) * Factor; } }
        public Vector3 FinalChildScale { get { return GunpowderArtCatalog.Vector(ChildScale); } }
        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(Resource) || !Resource.StartsWith("shipweapons/", StringComparison.Ordinal) ||
                !GunpowderArtCatalog.Finite(CaliberMm) || !GunpowderArtCatalog.Finite(SourceInscribedMm) ||
                CaliberMm <= 0 || SourceInscribedMm <= 0 || Factor < .05f || Factor > 4f)
                throw new InvalidDataException("Invalid gunpowder art calibration: " + Weapon);
            GunpowderArtCatalog.CheckVector(RootScale, true);
            GunpowderArtCatalog.CheckVector(ChildScale, true);
            GunpowderArtCatalog.CheckVector(ChildPosition, false);
            GunpowderArtCatalog.CheckVector(FirePointPosition, false);
            if (BaseMesh == null || MovingMesh == null) throw new InvalidDataException("Missing mesh fingerprint.");
            BaseMesh.Validate(); MovingMesh.Validate();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GunpowderMeshFingerprint
    {
        [JsonProperty("name", Required = Required.Always)] public string Name { get; private set; }
        [JsonProperty("vertices", Required = Required.Always)] public int Vertices { get; private set; }
        [JsonProperty("boundsCenter", Required = Required.Always)] public float[] BoundsCenter { get; private set; }
        [JsonProperty("boundsSize", Required = Required.Always)] public float[] BoundsSize { get; private set; }
        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(Name) || Vertices <= 0) throw new InvalidDataException("Invalid mesh fingerprint.");
            GunpowderArtCatalog.CheckVector(BoundsCenter, false); GunpowderArtCatalog.CheckVector(BoundsSize, true);
        }
        public bool Matches(Mesh mesh)
        {
            return mesh != null && mesh.name == Name && mesh.vertexCount == Vertices &&
                GunpowderArtCatalog.Near(mesh.bounds.center, GunpowderArtCatalog.Vector(BoundsCenter)) &&
                GunpowderArtCatalog.Near(mesh.bounds.size, GunpowderArtCatalog.Vector(BoundsSize));
        }
    }

    // Managed lifecycle logic is independently testable without creating a Unity scene.
    public sealed class GunpowderScaleSnapshot
    {
        public bool Applied { get; private set; }
        public Vector3 NativeBase { get; private set; }
        public Vector3 NativeChild { get; private set; }
        public Vector3 AppliedBase { get; private set; }
        public Vector3 AppliedChild { get; private set; }
        public void Capture(Vector3 nativeBase, Vector3 nativeChild, Vector3 appliedBase, Vector3 appliedChild)
        {
            NativeBase = nativeBase; NativeChild = nativeChild;
            AppliedBase = appliedBase; AppliedChild = appliedChild; Applied = true;
        }
        public bool Release(Vector3 currentBase, Vector3 currentChild)
        {
            bool restore = Applied && GunpowderArtCatalog.Near(currentBase, AppliedBase) &&
                GunpowderArtCatalog.Near(currentChild, AppliedChild);
            Applied = false;
            return restore;
        }
    }
}
