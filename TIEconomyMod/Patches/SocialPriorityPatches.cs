using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "welfarePriorityInequalityChange", MethodType.Getter)]
    public static class WelfareInequalityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            InequalitySettings settings = Main.settings.inequality;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.welfareEnabled)
            {
                return true;
            }

            // Welfare starts at -333,333 / population, then maps TI's 1-9 Inequality
            // range continuously to -1..+1 around 5. The smooth transform makes the
            // reduction 0x at 1, 1x at 5, and 2x at 9. At 100M population and
            // Inequality 5 this is -0.00333; installed vanilla 1.0.39 is about -0.00393.
            float rawDelta = settings.welfarePopulationDivisor /
                Math.Max(1f, __instance.population);
            float position = (__instance.inequality - settings.neutral) /
                ((settings.maximum - settings.minimum) / 2f);
            float transformedDelta = rawDelta * (1f - Math.Sign(rawDelta) * position *
                (float)Math.Pow(Math.Abs(position), settings.exponent - 1f));

            if (float.IsNaN(transformedDelta) || float.IsInfinity(transformedDelta))
            {
                Main.Warn("Welfare inequality produced an invalid value; using zero.");
                transformedDelta = 0f;
            }
            __result = transformedDelta;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "spoilsPriorityInequalityChange", MethodType.Getter)]
    public static class SpoilsInequalityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            InequalitySettings settings = Main.settings.inequality;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.spoilsEnabled)
            {
                return true;
            }

            // Resource inequality is proportional to resources relative to GDP:
            // one region in a $100B economy gives ratio 1 and curve 0.5, so the
            // configured +100% maximum becomes a x1.5 multiplier. The same region in
            // a $1T economy gives ratio 0.1 and only x1.091. Installed vanilla 1.0.39
            // adds a fixed 0.0015 per resource region before population scaling.
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceRatio = __instance.currentResourceRegions *
                abundance.referenceGdpPerResourceRegionBillions /
                Math.Max((float)(__instance.GDP / 1000000000d), abundance.minimumGdpBillions);
            double poweredResourceRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
            float resourceCurve = double.IsPositiveInfinity(poweredResourceRatio)
                ? 1f
                : (float)(poweredResourceRatio / (1d + poweredResourceRatio));
            float resourceMultiplier = 1f +
                settings.spoilsMaximumResourceMultiplier * resourceCurve;
            float rawDelta = settings.spoilsPopulationDivisor /
                Math.Max(1f, __instance.population) * resourceMultiplier;

            // The continuous boundary transform makes a positive Spoils delta 2x at
            // Inequality 1, 1x at 5, and 0x at 9. At 100M population, Inequality 5,
            // one region, and $100B GDP, defaults give +0.0025; installed vanilla
            // 1.0.39 gives roughly +0.0031 for the same population and resource count.
            float position = (__instance.inequality - settings.neutral) /
                ((settings.maximum - settings.minimum) / 2f);
            float transformedDelta = rawDelta * (1f - Math.Sign(rawDelta) * position *
                (float)Math.Pow(Math.Abs(position), settings.exponent - 1f));

            if (float.IsNaN(transformedDelta) || float.IsInfinity(transformedDelta))
            {
                Main.Warn("Spoils inequality produced an invalid value; using zero.");
                transformedDelta = 0f;
            }
            __result = transformedDelta;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "knowledgePriorityEducationChange", MethodType.Getter)]
    public static class KnowledgeEducationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            KnowledgeSettings settings = Main.settings.knowledge;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Education change is (166,667 / population) * 4 * 0.87^Education, giving
            // inverse-population scaling and smooth diminishing returns. At 100M
            // population and Education 8 the result is about +0.00219 per completion;
            // installed vanilla 1.0.39 gives roughly +0.00417 for the same nation.
            float calculated = settings.educationPopulationDivisor /
                Math.Max(1f, __instance.population) *
                settings.educationMaximumGain *
                (float)Math.Pow(settings.educationDecay, __instance.education);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Knowledge education produced an invalid value; using zero.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "knowledgePriorityCohesionChange", MethodType.Getter)]
    public static class KnowledgeCohesionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            KnowledgeSettings settings = Main.settings.knowledge;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Knowledge moves Cohesion toward 5 by at most 333,333 / population and
            // never crosses the target. At 100M population and Cohesion 7 it applies
            // -0.00333; installed vanilla 1.0.39 applies about -0.00785 because it uses
            // the shared population exponent rather than direct inverse population.
            float distance = Math.Abs(__instance.cohesion - settings.cohesionTarget);
            float step = Math.Min(distance,
                settings.cohesionPopulationDivisor / Math.Max(1f, __instance.population));
            float calculated = __instance.cohesion > settings.cohesionTarget ? -step : step;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Knowledge cohesion produced an invalid value; using zero.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "governmentPriorityDemocracyChange", MethodType.Getter)]
    public static class GovernmentDemocracyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            GovernmentSettings settings = Main.settings.government;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Government adds 166,667 / population with no Education multiplier. At
            // 100M population this is +0.00167. Installed vanilla 1.0.39 multiplies its
            // +0.01 base by population scaling and Education/10, giving about +0.00628
            // at Education 8; this formula intentionally makes population the sole driver.
            float calculated = settings.democracyPopulationDivisor /
                Math.Max(1f, __instance.population);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Government democracy produced an invalid value; using zero.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "militaryPriorityTechLevelChange", MethodType.Getter)]
    public static class MilitaryTechnologyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            MilitarySettings settings = Main.settings.military;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Military technology starts at 55,000 / population and gains 50% for each
            // full level behind the global maximum; the multiplier never falls below 1.
            // At 100M population, miltech 4, and global 6, this gives +0.0011.
            // Installed vanilla 1.0.39 gives +0.001875 from 0.00125 * (6 / 4).
            float baseChange = settings.technologyPopulationDivisor /
                Math.Max(1f, __instance.population);
            float catchup = Math.Max(1f, 1f + settings.catchupBonus *
                (__instance.maxMilitaryTechLevel - __instance.militaryTechLevel));
            float calculated = baseChange * catchup;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Military technology produced an invalid value; using zero.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "OppressionPriorityUnrestChange", MethodType.Getter)]
    public static class OppressionUnrestPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            OppressionSettings settings = Main.settings.oppression;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Oppression removes up to 2,222,222 / population Unrest, fading linearly
            // from full strength at Government 0 to zero at 10, without crossing zero.
            // At 100M population, Government 5, and Unrest 3 this gives -0.0111.
            // Installed vanilla 1.0.39 gives about -0.118 because it uses a much larger
            // shared population-scaled cap.
            float democracyMultiplier = Math.Max(0f, Math.Min(1f,
                (settings.fullDemocracy - __instance.democracy) / settings.fullDemocracy));
            float reduction = settings.unrestPopulationDivisor /
                Math.Max(1f, __instance.population) * democracyMultiplier;
            float calculated = -Math.Min(Math.Max(0f, __instance.unrest), reduction);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Oppression unrest produced an invalid value; using zero.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "spoilsPriorityMoney", MethodType.Getter)]
    public static class SpoilsMoneyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            SpoilsMoneySettings settings = Main.settings.spoilsMoney;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Money is a flat 240 plus a resource bonus proportional to resources/GDP.
            // One region in a $100B economy gives ratio 1, curve 0.5, and +80; the
            // same region in a $1T economy gives ratio 0.1 and about +14.55.
            // Installed vanilla 1.0.39 instead pays 5 * base IP + 5 per resource region
            // + 2.5 * (10 - Government), so a 5-IP, one-region autocracy receives 55.
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceRatio = __instance.currentResourceRegions *
                abundance.referenceGdpPerResourceRegionBillions /
                Math.Max((float)(__instance.GDP / 1000000000d), abundance.minimumGdpBillions);
            double poweredResourceRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
            float resourceCurve = double.IsPositiveInfinity(poweredResourceRatio)
                ? 1f
                : (float)(poweredResourceRatio / (1d + poweredResourceRatio));
            float resourceMoney = settings.maximumResourceBonus * resourceCurve;

            // Low Government multiplies the whole payout by 1.30 at 0, 1.15 at 5,
            // and 1.00 at 10. Thus the $100B, one-region autocracy above receives 416.
            float governmentProgress = Math.Max(0f, Math.Min(1f,
                __instance.democracy / settings.fullGovernment));
            float governmentMultiplier = 1f +
                settings.maximumLowGovernmentBonus * (1f - governmentProgress);
            float calculated = (settings.baseMoney + resourceMoney) * governmentMultiplier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Spoils money produced an invalid value; using the configured base.");
                calculated = settings.baseMoney;
            }
            __result = calculated;
            return false;
        }
    }
}
