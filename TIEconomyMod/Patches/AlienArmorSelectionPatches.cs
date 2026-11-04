using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod.Patches
{
    public static class AlienArmorSelection
    {
        private sealed class Candidate
        {
            internal TIShipArmorTemplate Armor;
            internal double Cost;
            internal float Mass;
        }

        // One native build-mass unit, valued from data rather than armor IDs
        // or an exotic-free eligibility test. No campaign market state needed.
        public static double MaterialCost(TIShipArmorTemplate armor)
        {
            TIGlobalConfig g = TemplateManager.global;
            double total = 0;
            foreach (ResourceValue material in armor.buildCost(1f).resourceCosts)
            {
                double value;
                switch (material.resource)
                {
                    case FactionResource.Water: value = g.initialWaterValue; break;
                    case FactionResource.Volatiles: value = g.initialVolatilesValue; break;
                    case FactionResource.Metals: value = g.initialMetalsValue; break;
                    case FactionResource.NobleMetals: value = g.initialNobleMetalsValue; break;
                    case FactionResource.Fissiles: value = g.initialFissilesValue; break;
                    case FactionResource.Antimatter: value = g.initialAntimatterValue; break;
                    case FactionResource.Exotics: value = g.initialExoticsValue; break;
                    default: value = 1d; break;
                }
                total += Math.Max(0d, material.value) * Math.Max(0d, value);
            }
            return total;
        }

        public static TIShipArmorTemplate Select(TIFactionState faction, bool premium)
        {
            var candidates = (faction.allowedArmors ?? Enumerable.Empty<TIShipArmorTemplate>())
                .Where(a => a != null).Select(a => new Candidate { Armor = a, Cost = MaterialCost(a), Mass = a.mass_damagePoint_kg })
                .Where(c => !double.IsNaN(c.Cost) && !double.IsInfinity(c.Cost) &&
                    !float.IsNaN(c.Mass) && !float.IsInfinity(c.Mass) && c.Mass > 0f).ToList();
            if (candidates.Count == 0) return null; // No unlocks are granted.
            if (!premium)
            {
                double minimum = candidates.Min(c => c.Cost);
                candidates = candidates.Where(c => c.Cost == minimum).ToList();
            }
            List<Candidate> current = candidates.Where(c => faction.obsoletedShipParts == null ||
                !faction.obsoletedShipParts.Contains(c.Armor.dataName)).ToList();
            if (current.Count != 0) candidates = current;
            return candidates.OrderBy(c => c.Mass).ThenBy(c => c.Cost)
                .ThenBy(c => c.Armor.dataName, StringComparer.Ordinal).First().Armor;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.GetBestArmor))]
    public static class AlienBestArmorSelectionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIFactionState __instance, bool allowExotics, ref TIShipArmorTemplate __result)
        {
            if (!__instance.IsAlienFaction) return true;
            __result = AlienArmorSelection.Select(__instance, allowExotics);
            return false;
        }
    }
}
