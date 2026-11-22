using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace TIEconomyMod.Core
{
    internal enum OrbitalWeaponRole
    {
        Laser,
        Magnetic,
        Plasma,
        PointDefense
    }

    internal sealed class OrbitalMountPose
    {
        [JsonProperty(Required = Required.Always)] public float[] Seat { get; private set; }
        [JsonProperty(Required = Required.Always)] public float[] FittedOrigin { get; private set; }
        [JsonProperty(Required = Required.Always)] public float[] Normal { get; private set; }

        internal Vector3 Position => ToVector(FittedOrigin);
        internal Vector3 Surface => ToVector(Seat);
        internal Quaternion Rotation
        {
            get
            {
                Vector3 up = ToVector(Normal).normalized;
                Vector3 reference = Math.Abs(up.z) < 0.9f ? Vector3.forward : Vector3.right;
                return Quaternion.LookRotation(Vector3.ProjectOnPlane(reference, up), up);
            }
        }

        private static Vector3 ToVector(float[] v) => new Vector3(v[0], v[1], v[2]);

        internal bool IsValid()
        {
            return Valid(Seat) && Valid(FittedOrigin) && Valid(Normal) &&
                Math.Abs(ToVector(Normal).sqrMagnitude - 1f) < 0.002f &&
                Vector3.Dot(Position - Surface, ToVector(Normal)) >= 0f;
        }

        private static bool Valid(float[] v)
        {
            return v != null && v.Length == 3 &&
                v.All(x => !float.IsNaN(x) && !float.IsInfinity(x));
        }
    }

    internal sealed class OrbitalMount
    {
        [JsonProperty(Required = Required.Always)] public string Id { get; private set; }
        [JsonProperty(Required = Required.Always)] public OrbitalWeaponRole Role { get; private set; }
        [JsonProperty(Required = Required.Always)] public OrbitalMountPose Top { get; private set; }
        [JsonProperty(Required = Required.Always)] public OrbitalMountPose Bottom { get; private set; }

        internal bool IsPointDefense => Role == OrbitalWeaponRole.PointDefense;
    }

    internal sealed class OrbitalDefenseLayout
    {
        [JsonProperty(Required = Required.Always)] public string Module { get; private set; }
        [JsonProperty(Required = Required.Always)] public int Tier { get; private set; }
        [JsonProperty(Required = Required.Always)]
        public Dictionary<string, Dictionary<string, float>> ModelScales { get; private set; }
        [JsonProperty(Required = Required.Always)] public OrbitalMount[] Mounts { get; private set; }

        internal int MaximumSize => Tier == 3 ? 4 : Tier;

        internal float ScaleFor(OrbitalMount mount, string model)
        {
            return ModelScales[mount.IsPointDefense ? "pd" : "battery"]
                .TryGetValue(model.ToLowerInvariant(), out float scale) ? scale : 1f;
        }
    }

    internal sealed class OrbitalDefenseDocument
    {
        [JsonProperty(Required = Required.Always)] public int Schema { get; private set; }
        [JsonProperty(Required = Required.Always)] public OrbitalDefenseLayout[] Layouts { get; private set; }
    }

    internal static class OrbitalDefense
    {
        private static Dictionary<string, OrbitalDefenseLayout> layouts;

        internal static void Load(string modPath)
        {
            string path = Path.Combine(modPath, "Config", "orbital-defense-layouts.eeo");
            var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
            settings.Converters.Add(new StringEnumConverter { AllowIntegerValues = false });
            var document = JsonConvert.DeserializeObject<OrbitalDefenseDocument>(File.ReadAllText(path), settings);
            if (document == null || document.Schema != 1 || document.Layouts.Length != 6)
                throw new InvalidDataException("Invalid orbital defense catalog.");
            var validated = new Dictionary<string, OrbitalDefenseLayout>(StringComparer.Ordinal);
            foreach (OrbitalDefenseLayout layout in document.Layouts)
            {
                if (layout == null || string.IsNullOrEmpty(layout.Module) || layout.Tier < 1 || layout.Tier > 3 ||
                    layout.Mounts.Length == 0 || layout.ModelScales.Count != 2 ||
                    !layout.ModelScales.ContainsKey("pd") || !layout.ModelScales.ContainsKey("battery") ||
                    layout.ModelScales.Values.Any(scales => scales == null ||
                        scales.Values.Any(scale => !(scale > 0f && scale <= 1f))))
                    throw new InvalidDataException("Invalid orbital defense layout in " + path);
                var ids = new HashSet<string>(StringComparer.Ordinal);
                foreach (OrbitalMount mount in layout.Mounts)
                {
                    if (mount == null || string.IsNullOrEmpty(mount.Id) || !ids.Add(mount.Id) ||
                        mount.Top == null || mount.Bottom == null || !mount.Top.IsValid() || !mount.Bottom.IsValid() ||
                        mount.Top.Normal[1] <= 0f || mount.Bottom.Normal[1] >= 0f)
                        throw new InvalidDataException("Invalid paired mount on " + layout.Module);
                }
                validated.Add(layout.Module, layout);
            }
            layouts = validated;
        }

        internal static bool TryGet(TIHabModuleTemplate template, bool isBase, out OrbitalDefenseLayout layout)
        {
            layout = null;
            return !isBase && template != null && layouts != null &&
                layouts.TryGetValue(template.dataName, out layout);
        }

        internal static List<TIShipWeaponTemplate> Weapons(
            OrbitalDefenseLayout layout, TIFactionState faction, TISpaceBodyState body, bool includePD = true,
            List<TIShipWeaponTemplate> additions = null)
        {
            var picks = new Dictionary<OrbitalWeaponRole, TIShipWeaponTemplate>();
            var result = new List<TIShipWeaponTemplate>(layout.Mounts.Length);
            foreach (OrbitalMount mount in layout.Mounts)
            {
                if (!includePD && mount.IsPointDefense)
                    continue;
                if (!picks.TryGetValue(mount.Role, out TIShipWeaponTemplate weapon))
                {
                    weapon = Select(layout, faction, body, mount.Role, additions);
                    picks.Add(mount.Role, weapon);
                }
                result.Add(weapon);
            }
            return result;
        }

        private static TIShipWeaponTemplate Select(
            OrbitalDefenseLayout layout, TIFactionState faction, TISpaceBodyState body,
            OrbitalWeaponRole role, List<TIShipWeaponTemplate> additions)
        {
            if (role == OrbitalWeaponRole.PointDefense)
            {
                if (faction.IsAlienFaction)
                    return Find("AlienPointDefenseLaserTurret");
                // Completion notices can include parts that are not yet in the
                // faction's cache. Use the same ranking for both consumers.
                var defense = faction.allowedHullWeapons.Concat(additions ?? Enumerable.Empty<TIShipWeaponTemplate>())
                    .Where(w => w.hullWeapon && w.defenseMode && !w.attackMode && !w.isMissileWeapon);
                var best = defense.OrderByDescending(w => w.isLaserWeapon).ThenBy(w => w.cooldown_s).FirstOrDefault();
                return best ?? Find("30mmAutocannon");
            }

            WeaponClass category = role == OrbitalWeaponRole.Laser ? WeaponClass.Laser :
                role == OrbitalWeaponRole.Magnetic ? WeaponClass.Magnetic : WeaponClass.Plasma;
            var selected = Find(faction.GetBestHabWeapon(false, layout.Tier, category, body, additions));
            if (role == OrbitalWeaponRole.Plasma && selected?.weaponClass != WeaponClass.Plasma)
                return Select(layout, faction, body, OrbitalWeaponRole.Magnetic, additions);
            if (selected != null && selected.hullWeapon && selected.internalSize <= layout.MaximumSize)
                return selected;

            // The native orphan fallback is a two-slot naval battery. An otherwise
            // unarmed T1 uses the one-slot model, which the fitted layout covers.
            return Find(faction.IsAlienFaction ? "AlienLightMagBattery" : "30mmAutocannon");
        }

        private static TIShipWeaponTemplate Find(string name)
        {
            return string.IsNullOrEmpty(name) ? null : TemplateManager.Find<TIShipWeaponTemplate>(name, allowChild: true);
        }

        internal static bool IsResearchUpgrade(TIFactionState faction, int tier,
            TIShipWeaponTemplate candidate, List<TIShipWeaponTemplate> additions)
        {
            return layouts.Values.Where(layout => layout.Tier == tier).Any(layout =>
                TemplateManager.Find<TIHabModuleTemplate>(layout.Module)?.alienModule == faction.IsAlienFaction &&
                Weapons(layout, faction, null, true, additions).Contains(candidate));
        }
    }
}
