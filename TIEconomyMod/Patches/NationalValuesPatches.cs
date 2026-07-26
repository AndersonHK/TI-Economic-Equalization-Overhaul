using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
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

            // Monthly IP is GDP / $100B, followed by a low-income multiplier that
            // rises linearly from 70% at $0 PCGDP to 100% at $15k. For example,
            // a $500B economy produces 3.5 IP at $0 PCGDP and 5 IP at $15k.
            // Installed vanilla 1.0.39 instead exposes a cached nonlinear economy score,
            // so this patch deliberately makes national output directly legible from GDP.
            float baseInvestmentPoints = (float)(__instance.GDP /
                (settings.gdpPerInvestmentPointBillions * 1000000000d));
            float incomeProgress = Math.Max(0f, Math.Min(1f,
                __instance.perCapitaGDP / settings.lowIncomeThreshold));
            float incomeMultiplier = settings.lowIncomeMultiplierAtZero +
                (1f - settings.lowIncomeMultiplierAtZero) * incomeProgress;
            float calculated = baseInvestmentPoints * incomeMultiplier;

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
        private static readonly string[] LegacyTechnologies =
        {
            "ArrivalInternationalRelations",
            "UnityMovements",
            "GreatNations",
            "ArrivalGovernance",
            "Accelerando"
        };

        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            ControlCostSettings settings = Main.settings.controlCost;
            if (!Main.FeatureEnabled(settings.enabled) || __result == 0f)
            {
                return; // Alien control points have zero vanilla cost and must remain free.
            }

            int completed = LegacyTechnologies.Count(
                id => GameStateManager.GlobalResearch().finishedTechsNames.Contains(id));

            // The five listed social technologies lower the economy-score exponent through
            // 1, .98, .95, .90, .85, and .80; the result is divided evenly among CPs.
            // With economy score 200 and four CPs, vanilla's unmodified 200 / 4 is 50,
            // one technology gives 200^.98 / 4 = about 45, and all five give about 17.
            float exponent;
            switch (completed)
            {
                case 1: exponent = settings.exponentOneTech; break;
                case 2: exponent = settings.exponentTwoTechs; break;
                case 3: exponent = settings.exponentThreeTechs; break;
                case 4: exponent = settings.exponentFourTechs; break;
                case 5: exponent = settings.exponentFiveTechs; break;
                default: exponent = 1f; break;
            }
            float calculated = (float)Math.Pow(__instance.economyScore, exponent) /
                Math.Max(1, __instance.numControlPoints);

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Control Point cost produced an invalid value; retaining vanilla.");
                return;
            }
            __result = calculated;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "investmentArmyFactor", MethodType.Getter)]
    public static class ArmyUpkeepPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TIArmyState __instance)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Home and deployed armies start at the installed vanilla bases of 0.5 and
            // 1 IP, then multiply by max(1, 1 + 2 * (miltech - 3)). A home army at
            // miltech 5 therefore costs 0.5 * 5 = 2.5 IP instead of vanilla's 0.5;
            // technology at or below 3 never reduces the base cost.
            float baseCost = __instance.useHomeInvestmentFactor
                ? settings.homeBaseCost
                : settings.awayBaseCost;
            float technologyMultiplier = Math.Max(1f,
                1f + settings.costPerTechnologyLevel *
                (__instance.homeNation.militaryTechLevel - settings.technologyBaseline));
            float calculated = baseCost * technologyMultiplier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Army upkeep produced an invalid value; using unscaled 1.0.32 upkeep.");
                calculated = baseCost;
            }
            __result = calculated;
            return false;
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
            // defaults produce about 13.5 research/month. Installed vanilla 1.0.39
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
