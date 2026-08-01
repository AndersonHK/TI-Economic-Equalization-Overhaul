using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "economyScore", MethodType.Getter)]
    public static class InvestmentPointsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            InvestmentSettings settings = Main.settings.investment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Monthly IP is GDP / $100B. The low-income multiplier rises linearly
            // from 70% at $0 PCGDP to 100% at $15k, then the configured x1.05 output
            // adjustment applies. A $500B economy therefore produces 3.675 IP at
            // $0 PCGDP and 5.25 IP at $15k instead of the previous 3.5 and 5.
            // Installed vanilla 1.0.49 instead exposes a cached nonlinear economy score,
            // so this patch deliberately makes national output directly legible from GDP.
            float baseInvestmentPoints = (float)(__instance.GDP /
                (settings.gdpPerInvestmentPointBillions * 1000000000d));
            float incomeProgress = Math.Max(0f, Math.Min(1f,
                __instance.perCapitaGDP / settings.lowIncomeThreshold));
            float incomeMultiplier = settings.lowIncomeMultiplierAtZero +
                (1f - settings.lowIncomeMultiplierAtZero) * incomeProgress;
            float calculated = baseInvestmentPoints * incomeMultiplier *
                settings.outputMultiplier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Investment Points produced an invalid value; using the safe 70% low-income floor.");
                calculated = baseInvestmentPoints * 0.70f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "ControlPointMaintenanceCost", MethodType.Getter)]
    public static class ControlPointCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            ControlCostSettings settings = Main.settings.controlCost;
            if (!Main.FeatureEnabled(settings.enabled) || __result == 0f)
            {
                return; // Alien control points have zero vanilla cost and must remain free.
            }

            // Each technology contributes its own explicit reduction instead of merely
            // advancing a count-based sequence. This produces the same intended path
            // (1, .98, .95, .90, .85, .80) in normal research order while remaining
            // correct if technologies are granted or completed out of order.
            float exponentReduction = 0f;
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains(
                "ArrivalInternationalRelations"))
            {
                exponentReduction += settings.arrivalInternationalRelationsReduction;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains("UnityMovements"))
            {
                exponentReduction += settings.unityMovementsReduction;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains("GreatNations"))
            {
                exponentReduction += settings.greatNationsReduction;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains("ArrivalGovernance"))
            {
                exponentReduction += settings.arrivalGovernanceReduction;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains("Accelerando"))
            {
                exponentReduction += settings.accelerandoReduction;
            }
            float exponent = Math.Max(0.01f, 1f - exponentReduction);

            // TI 1.0.49 then applies both EEO's x1.20 country-cost increase and the
            // active scenario's CP-maintenance multiplier, preserving the new-start
            // balance knob without adopting vanilla's global-GDP normalization.
            // With economy score 200, four CPs, and a x1.2 scenario, no technology
            // costs 200 / 4 * 1.2 * 1.2 = 72 and all five cost about 25.
            float calculated = (float)Math.Pow(__instance.economyScore, exponent) /
                Math.Max(1, __instance.numControlPoints) *
                settings.countryCostMultiplier *
                GameStateManager.Time().template.CPMaintenanceModifier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Control Point cost produced an invalid value; retaining vanilla.");
                return;
            }
            __result = calculated;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), "GetControlPointMaintenanceFreebieCap")]
    public static class ControlPointCapacityPatch
    {
        // These are the five stackable project effects used by TI 1.0.49. Their
        // stored negative additive values become positive percentage points here.
        // Restricting the conversion to known IDs preserves every unrelated
        // ControlPointMaintenance modifier from the game or another mod.
        private static readonly HashSet<string> PercentageProjectEffects =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Effect_ControlPointMaintenanceBonus160",
                "Effect_ControlPointMaintenanceBonus40",
                "Effect_ControlPointMaintenanceBonus20",
                "Effect_ControlPointMaintenanceBonus10",
                "Effect_ControlPointMaintenanceBonus3"
            };

        [HarmonyPostfix]
        public static void Postfix(ref float __result, TIFactionState __instance)
        {
            ControlCostSettings settings = Main.settings.controlCost;
            if (!Main.FeatureEnabled(settings.enabled) ||
                !settings.projectBonusesAsPercent ||
                __instance.IsAlienFaction)
            {
                return;
            }

            float projectPercent = TIEffectsState
                .GetFactionEffectsForContext(Context.ControlPointMaintenance, __instance)
                .Where(effect =>
                    effect != null &&
                    effect.value < 0f &&
                    PercentageProjectEffects.Contains(effect.dataName))
                .Sum(effect => -effect.value);
            if (projectPercent <= 0f)
            {
                return;
            }

            // Vanilla has already added these project values as flat capacity.
            // Remove that contribution, then multiply the complete remaining base:
            // campaign/scenario freebies, AI bonuses, councilors, and LEO modules.
            float flatCapacity = __result - projectPercent;
            float calculated = flatCapacity * (1f + projectPercent / 100f);
            if (flatCapacity < 0f || float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Control Point capacity produced an invalid value; retaining vanilla.");
                return;
            }
            __result = calculated;
        }
    }

    [HarmonyPatch(typeof(TINationState), "research_month", MethodType.Getter)]
    public static class ResearchPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            ResearchSettings settings = Main.settings.research;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Research is coefficient * population(millions) * Education^2, then scaled
            // by PCGDP, Government, Cohesion, Unrest, and the live adviser bonus. PCGDP
            // bottoms out at 60% of the $20k reference. For a 50M nation at Education 8,
            // $10k PCGDP, Government 5, Cohesion 5, Unrest 2, and +10% adviser science,
            // defaults produce about 13.5 research/month. Installed vanilla 1.0.49
            // produces about 51.6 because it uses a larger coefficient and an IP crutch.
            float income = Math.Max(
                __instance.perCapitaGDP / settings.referencePcgdp,
                settings.minimumPcgdpMultiplier);
            float democracy = (float)Math.Pow(
                Math.Max(__instance.democracy, settings.democracyFloor),
                settings.democracyExponent);
            float cohesion = settings.cohesionPeak -
                Math.Abs(__instance.cohesion - settings.cohesionCenter) *
                settings.cohesionPenaltyPerPoint;
            float unrest = 1f - Math.Max(__instance.unrest - settings.unrestGrace, 0f) /
                settings.unrestPenaltyDivisor;
            float calculated = settings.coefficient * __instance.population_Millions *
                (float)Math.Pow(Math.Max(0f, __instance.education), settings.educationExponent) *
                income * democracy * cohesion * unrest * (1f + __instance.adviserScienceBonus);

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Research produced an invalid value; using zero rather than corrupting the save.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }
}
