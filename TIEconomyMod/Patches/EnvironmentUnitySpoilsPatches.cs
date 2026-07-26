using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "environmentPrioritySustainabilityChange", MethodType.Getter)]
    public static class EnvironmentSustainabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Sustainability is national carbon intensity, so a completion must do less
            // in a larger economy: -0.10 at $100B GDP and -0.01 at $1T means one and ten
            // monthly IP respectively both clean up at -0.10/month under full investment.
            // This removes vanilla's population and PCGDP bias without making rich legacy
            // grids easier than poor grids that can leapfrog directly to hydro or solar.
            float baseChange = -settings.cleanupAtReferenceGdp;
            baseChange += TIEffectsState.SumEffectsModifiers(
                Context.Environment_SustainabilityChange, __instance, baseChange, null);
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));

            // Nuclear damage depends on detonations per land area, while the separate
            // decontamination threshold still charges the same number of IP per blast.
            // With a 100,000 km2 reference, one strike leaves Kazakhstan-sized 2.7M km2
            // territory at x0.964 cleanup, but Singapore-sized 700 km2 territory at x0.007.
            float landAreaKm2 = 0f;
            int detonations = 0;
            foreach (TIRegionState region in __instance.regions)
            {
                landAreaKm2 += Math.Max(0f, region.area_km2);
                detonations += Math.Max(0, region.nuclearDetonations);
            }
            float falloutLoad = detonations * settings.falloutReferenceAreaKm2 /
                Math.Max(landAreaKm2, settings.minimumLandAreaKm2);
            float calculated = baseChange * settings.referenceGdpBillions /
                gdpBillions / (1f + falloutLoad);

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Environment sustainability produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityCO2Removed")]
    public static class EnvironmentCo2RemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Direct atmospheric removal is a fixed physical return per IP, not a return
            // per citizen. A nation with 10x the GDP has about 10x the IP and therefore
            // removes about 10x as much under equal priority allocation. Vanilla 1.0.39
            // divides this effect by its nonlinear population-scaling factor.
            float baseChange = TemplateManager.global.WelCO2_ppm;
            float calculated = (baseChange + TIEffectsState.SumEffectsModifiers(
                Context.Welfare_CO2_ppm, __instance, baseChange, null)) *
                settings.atmosphericRemovalMultiplier;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Environment CO2 removal produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityCH4Removed")]
    public static class EnvironmentMethaneRemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Methane follows the same fixed-capital rule as CO2: the configured vanilla
            // amount and project modifiers apply once per completed IP, with no population
            // divisor. Ten times the available IP therefore buys ten times the cleanup.
            float baseChange = TemplateManager.global.WelCH4_ppm;
            float calculated = (baseChange + TIEffectsState.SumEffectsModifiers(
                Context.Welfare_CH4_ppm, __instance, baseChange, null)) *
                settings.atmosphericRemovalMultiplier;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Environment methane removal produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityN2ORemoved")]
    public static class EnvironmentNitrousOxideRemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Nitrous oxide also remains a fixed cleanup return per IP. This preserves
            // live TI and project modifiers while removing vanilla's population divisor,
            // so national size changes total spending rather than the value of each IP.
            float baseChange = TemplateManager.global.WelN2O_ppm;
            float calculated = (baseChange + TIEffectsState.SumEffectsModifiers(
                Context.Welfare_N2O_ppm, __instance, baseChange, null)) *
                settings.atmosphericRemovalMultiplier;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Environment nitrous-oxide removal produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "GHGsFromEconomy_tons")]
    public static class EconomyEmissionsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref System.Tuple<double, double, double> __result,
            TINationState __instance,
            bool monthly,
            float proposedSustainabilityChange)
        {
            EmissionsSettings settings = Main.settings.emissions;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Emissions are GDP times carbon intensity, with no independent population
            // term. At Sustainability 1, $100B emits 27.5M base tons/year and $1T emits
            // 275M: economic growth raises total emissions but not emissions per dollar.
            // Vanilla 1.0.39 adds a large PCGDP-weighted population term, which makes two
            // equally sized economies emit differently merely because borders changed.
            double gdpBillions = Math.Max(0d, __instance.GDP / 1000000000d);
            double sustainability = Math.Max(0d,
                __instance.sustainability + proposedSustainabilityChange);

            // Resource intensity uses the same resources/GDP curve as growth. One region
            // at $100B gives ratio 1 and curve 0.5, so the default x1.25 ceiling yields
            // x1.125 emissions; at $1T the same region gives only x1.023.
            double resourceIntensity = 1d;
            AbundanceSettings abundance = Main.settings.abundance;
            if (Main.FeatureEnabled(abundance.enabled))
            {
                double resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max(gdpBillions, abundance.minimumGdpBillions);
                double poweredRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                double resourceCurve = double.IsPositiveInfinity(poweredRatio)
                    ? 1d
                    : poweredRatio / (1d + poweredRatio);
                resourceIntensity = 1d +
                    (settings.maximumResourceIntensityMultiplier - 1d) * resourceCurve;
            }

            double baseTons = gdpBillions * settings.tonsPerGdpBillion *
                sustainability * resourceIntensity;
            if (monthly)
            {
                baseTons /= settings.monthsPerYear;
            }
            double co2 = baseTons * settings.co2TonsMultiplier;
            double methane = baseTons * settings.methaneTonsMultiplier;
            double nitrousOxide = baseTons * settings.nitrousOxideTonsMultiplier;

            if (double.IsNaN(co2) || double.IsInfinity(co2) ||
                double.IsNaN(methane) || double.IsInfinity(methane) ||
                double.IsNaN(nitrousOxide) || double.IsInfinity(nitrousOxide))
            {
                Main.Warn("GDP-based economy emissions produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = System.Tuple.Create(co2, methane, nitrousOxide);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "unityPriorityCohesionChange", MethodType.Getter)]
    public static class UnityCohesionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            UnitySettings settings = Main.settings.unity;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Cohesion is inverse-population so linear GDP/IP cannot make large nations
            // easier to reshape. Education plus Government reduces the effect by 2.5%
            // per level, floored at 50%: at population 100M and scores 8 + 6, the base
            // +0.0333 becomes +0.0217. Vanilla uses nonlinear population scaling.
            float penalty = Math.Max(settings.minimumCohesionMultiplier, Math.Min(1f,
                1f - settings.educationAndGovernmentPenaltyPerLevel *
                (__instance.education + __instance.democracy)));
            float calculated = settings.cohesionPopulationDivisor /
                Math.Max(1f, __instance.population) * penalty;
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Unity cohesion produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "unityPriorityEducationChange", MethodType.Getter)]
    public static class UnityEducationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            UnitySettings settings = Main.settings.unity;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Unity's secondary cost is a small inverse-population Education loss:
            // -33,333 / 100M = -0.000333 per completion. Linear IP then keeps the
            // monthly rate comparable across equally wealthy populations instead of
            // making a unified country easier to homogenize than its former parts.
            float calculated = settings.educationPopulationDivisor /
                Math.Max(1f, __instance.population);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Unity education produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "spoilsPriorityDemocracyChange", MethodType.Getter)]
    public static class SpoilsGovernmentPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            SpoilsSettings settings = Main.settings.spoils;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Spoils damages Government by -66,667 / population. At 100M population
            // that is -0.000667 per completion; ten times the population needs ten
            // times the completed IP for the same institutional damage. This is the
            // population-linear counterpart to the mod's GDP-linear IP generation.
            float calculated = settings.governmentPopulationDivisor /
                Math.Max(1f, __instance.population);
            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Spoils Government change produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "spoilsSustainabilityChange", MethodType.Getter)]
    public static class SpoilsSustainabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            SpoilsSettings settings = Main.settings.spoils;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            // Spoils worsens carbon intensity by +0.05 at $100B GDP and +0.005 at
            // $1T, so GDP-linear IP gives the same base monthly damage at equal priority
            // allocation. Resources raise intensity through the resources/GDP curve:
            // one region at $100B gives x1.5 and +0.075; at $1T it gives x1.091
            // and +0.00545. Vanilla scales this national effect by population.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceRatio = __instance.currentResourceRegions *
                abundance.referenceGdpPerResourceRegionBillions /
                Math.Max(gdpBillions, abundance.minimumGdpBillions);
            double poweredRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
            float resourceCurve = double.IsPositiveInfinity(poweredRatio)
                ? 1f
                : (float)(poweredRatio / (1d + poweredRatio));
            float resourceMultiplier = 1f +
                (settings.maximumResourceSustainabilityMultiplier - 1f) * resourceCurve;
            float calculated = settings.sustainabilityChangeAtReferenceGdp *
                settings.referenceGdpBillions / gdpBillions * resourceMultiplier;

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Spoils sustainability produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = calculated;
            return false;
        }
    }
}
