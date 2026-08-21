using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "Coup")]
    public static class CoupSocialResetPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance)
        {
            InequalitySettings settings = Main.settings.inequality;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.coupEnabled)
            {
                return;
            }

            // TI has no coup-specific Inequality reason. EventEffects is the
            // neutral direct-change path and, unlike priority changes, does not
            // pass through EEO's directional boundary curve.
            __instance.AddToInequality(settings.coupInequalityChange,
                TINationState.InequalityChangeReason.InqReason_EventEffects);

            if (!settings.coupResetCohesionToRestState)
            {
                return;
            }

            // The postfix runs after TI's Government, Unrest, random Cohesion,
            // GDP, and control-point changes. Reading the rest state here also
            // includes the new Inequality, so the coup ends at its fully updated
            // social equilibrium rather than drifting there over later months.
            float target = Math.Max(0f, __instance.cohesionRestState);
            if (float.IsNaN(target) || float.IsInfinity(target))
            {
                Main.Warn("Coup Cohesion rest state was invalid; retaining TI's coup Cohesion result.");
                return;
            }

            __instance.AddToCohesion(target - __instance.cohesion,
                TINationState.CohesionChangeReason.CohesionReason_Coup);
        }
    }

    [HarmonyPatch(typeof(TINationState), "AddToInequality")]
    public static class ClimateInequalityPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float value,
            TINationState.InequalityChangeReason reason)
        {
            InequalitySettings settings = Main.settings.inequality;
            if (!Main.FeatureEnabled(settings.enabled) ||
                reason != TINationState.InequalityChangeReason.InqReason_ClimateChange)
            {
                return;
            }

            // Vanilla converts climate damage into Inequality by adding one-fifth
            // of the modeled annual GDP-loss fraction. A 10% loss therefore adds
            // 0.02; the default x4 multiplier makes that 0.08. Priority, event,
            // revolution, secession, and annexation changes never enter this branch.
            value *= settings.climateChangeMultiplier;
        }
    }

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

            // Inequality is a proportional economic outcome, so Welfare divides by GDP.
            // Defaults give -0.01999998 at $100B and -0.001999998 at $1T; ten times the IP in
            // the larger economy restores the same monthly movement at equal allocation.
            // The 1-9 transform is 0x at 1, 1x at 5, and 3x at 9 for this negative delta.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            float rawDelta = settings.welfareChangeAtReferenceGdp *
                settings.referenceGdpBillions / gdpBillions;
            float transformedDelta = InequalityMath.TransformPriorityChange(rawDelta,
                __instance.inequality, settings.minimum, settings.neutral,
                settings.maximum, settings.exponent,
                settings.maximumDirectionalMultiplier);

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
            // one region in a $1T economy gives ratio 1 and curve 0.5, so the
            // configured +100% maximum becomes a x1.5 multiplier. The same region in
            // a $100B economy gives ratio 10 and curve 0.666 for x1.666.
            // Installed vanilla 1.0.51
            // adds a fixed 0.0015 per resource region before population scaling.
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceCurve = 0f;
            if (Main.FeatureEnabled(abundance.enabled))
            {
                float resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max((float)(__instance.GDP / 1000000000d), abundance.minimumGdpBillions);
                double poweredResourceRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                resourceCurve = double.IsPositiveInfinity(poweredResourceRatio)
                    ? 1f
                    : (float)(poweredResourceRatio / (1d + poweredResourceRatio));
            }
            float resourceMultiplier = 1f +
                settings.spoilsMaximumResourceMultiplier * resourceCurve;
            // Like the other Inequality changes, Spoils divides by GDP because it changes
            // an economic distribution ratio. Defaults give +0.01000002 at $100B and
            // +0.001000002 at $1T before resource/bound multipliers, exactly offsetting
            // the tenfold difference in GDP-linear IP production.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            float rawDelta = settings.spoilsChangeAtReferenceGdp *
                settings.referenceGdpBillions / gdpBillions * resourceMultiplier;

            // The continuous boundary transform makes a positive Spoils delta 3x at
            // Inequality 1, 1x at 5, and 0x at 9. At 100M population, Inequality 5,
            // one region, and $100B GDP, defaults give about +0.01667; installed vanilla
            // 1.0.51 gives roughly +0.0031 for the same population and resource count.
            float transformedDelta = InequalityMath.TransformPriorityChange(rawDelta,
                __instance.inequality, settings.minimum, settings.neutral,
                settings.maximum, settings.exponent,
                settings.maximumDirectionalMultiplier);

            if (float.IsNaN(transformedDelta) || float.IsInfinity(transformedDelta))
            {
                Main.Warn("Spoils inequality produced an invalid value; using zero.");
                transformedDelta = 0f;
            }
            __result = transformedDelta;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "inequalityImpactOnCohesion", MethodType.Getter)]
    public static class CohesionRestInequalityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            float calculated = CohesionRestMath.InequalityImpact(
                __instance.education, __instance.inequality,
                settings.inequalityEducationBaseMultiplier,
                settings.inequalityEducationDivisor,
                settings.inequalityOffset, settings.inequalityCoefficient);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Cohesion-rest Inequality impact produced an invalid value; retaining vanilla.");
                return true;
            }

            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "publicEliteDivideImpactOnCohesion", MethodType.Getter)]
    public static class CohesionRestPublicElitePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return;
            }

            float calculated = CohesionRestMath.ScalePublicEliteImpact(__result,
                __instance.democracy, settings.publicEliteGovernmentDivisor);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Cohesion-rest public/elite impact produced an invalid value; retaining vanilla.");
                return;
            }

            __result = calculated;
        }
    }

    [HarmonyPatch(typeof(TINationState), "autocracyImpactOnCohesion", MethodType.Getter)]
    public static class CohesionRestAutocracyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            float calculated = CohesionRestMath.AutocracyImpact(
                __instance.democracy, __instance.unrest,
                settings.autocracyAnocracyBoundary,
                settings.autocracyExponent);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Cohesion-rest Autocracy impact produced an invalid value; retaining vanilla.");
                return true;
            }

            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "anocracyImpactOnCohesion", MethodType.Getter)]
    public static class CohesionRestAnocracyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            __result = CohesionRestMath.AnocracyImpact(__instance.democracy,
                settings.autocracyAnocracyBoundary,
                settings.anocracyDemocracyBoundary);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "DemocracyImpactOnCohesion")]
    public static class CohesionRestDemocracyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance,
            float originalValue)
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            __result = CohesionRestMath.DemocracyImpact(
                __instance.democracy, originalValue, 5f,
                settings.anocracyDemocracyBoundary,
                settings.democracyCoefficient);
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
            // installed vanilla 1.0.51 gives roughly +0.00417 for the same nation.
            float calculated = settings.educationPopulationDivisor /
                Math.Max(1f, __instance.population) *
                settings.educationMaximumGain *
                (float)Math.Pow(settings.educationDecay, __instance.education);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated) ||
                calculated < 0f)
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
            // -0.00333; installed vanilla 1.0.51 applies about -0.00785 because it uses
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

            // Government adds 333,333 / population with no Education multiplier. At
            // 100M population this is +0.00333 before the boundary curve. Installed vanilla 1.0.51 multiplies its
            // +0.01 base by population scaling and Education/10, giving about +0.00628
            // at Education 8; this raw formula intentionally makes population the sole driver.
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

        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            GovernmentChangeCurvePatch.Transform(ref __result, __instance);
        }
    }

    [HarmonyPatch(typeof(TINationState), "OppressionPriorityDemocracyChange", MethodType.Getter)]
    public static class OppressionDemocracyCurvePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            GovernmentChangeCurvePatch.Transform(ref __result, __instance);
        }
    }

    [HarmonyPatch(typeof(TINationState), "AddToDemocracy")]
    public static class GovernmentChangeCurvePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float value, TINationState __instance,
            TINationState.DemocracyChangeReason reason)
        {
            GovernmentSettings settings = Main.settings.government;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return;
            }

            // Priority getters are transformed before their values reach the UI,
            // direct-investment pricing, and completion methods. Do not transform
            // those three values again here.
            if (reason == TINationState.DemocracyChangeReason.DemReason_GovernmentPriority ||
                reason == TINationState.DemocracyChangeReason.DemReason_OppressionPriority ||
                reason == TINationState.DemocracyChangeReason.DemReason_SpoilsPriority)
            {
                return;
            }

            if (reason == TINationState.DemocracyChangeReason.DemReason_LowCohesion)
            {
                value *= settings.passiveLowCohesionMultiplier;
            }
            Transform(ref value, __instance);
        }

        public static void Transform(ref float value, TINationState nation)
        {
            GovernmentSettings settings = Main.settings.government;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return;
            }
            value = GovernmentMath.TransformChange(value, nation.democracy,
                settings.boundaryCurveFactor);
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
            // Installed vanilla 1.0.51 gives about -0.118 because it uses a much larger
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

            // Resource wealth multiplies the full $60 base through the continuous
            // resources/GDP curve. With a configured ceiling of x4, one region at
            // $1T gives ratio 1, curve .5, and x2.5; at $100B it gives ratio 10,
            // curve .666, and about x3.0.
            // Installed vanilla pays additive base-IP/resource/Government amounts instead.
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceCurve = 0f;
            if (Main.FeatureEnabled(abundance.enabled))
            {
                float resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max((float)(__instance.GDP / 1000000000d), abundance.minimumGdpBillions);
                double poweredResourceRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                resourceCurve = double.IsPositiveInfinity(poweredResourceRatio)
                    ? 1f
                    : (float)(poweredResourceRatio / (1d + poweredResourceRatio));
            }
            float resourceMultiplier = 1f +
                (settings.maximumResourceMultiplier - 1f) * resourceCurve;

            // Government applies the requested 1.30 - .03*Government term: scores 0, 5,
            // and 10 give x1.30, x1.15, and x1.00. Thus one region, $1T, Government 5
            // pays 60 * 2.5 * 1.15 = 172.5; at $100B it pays about $207.
            float government = Math.Max(0f, Math.Min(
                settings.fullGovernment, __instance.democracy));
            float governmentMultiplier = settings.governmentBaseMultiplier -
                settings.governmentPenaltyPerLevel * government;
            float calculated = settings.baseMoney * resourceMultiplier *
                governmentMultiplier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated) ||
                calculated < 0f)
            {
                Main.Warn("Spoils money produced an invalid value; using the configured base.");
                calculated = settings.baseMoney;
            }
            __result = calculated;
            return false;
        }
    }
}
