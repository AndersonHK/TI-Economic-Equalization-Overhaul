using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    internal static class EnvironmentRuntime
    {
        private const double RatingEpsilon = EnvironmentMath.NeutralEpsilon;

        public static double Rating(TINationState nation)
        {
            return nation == null
                ? double.NaN
                : EnvironmentMath.RatingFromStored(
                    nation.sustainability,
                    Main.settings.environment.storageRatingOffset);
        }

        public static double TechnologyCap()
        {
            EnvironmentSettings settings = Main.settings.environment;
            double unlocked = 0d;
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains(
                "ArrivalInternationalDevelopment"))
            {
                unlocked += 1d;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains("CleanEnergy"))
            {
                unlocked += 2d;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains(
                "ClimateChangeMitigation"))
            {
                unlocked += 2d;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains(
                "DesignerLifeforms"))
            {
                unlocked += 1d;
            }
            if (GameStateManager.GlobalResearch().finishedTechsNames.Contains(
                "IntegratedEarthSpaceEconomy"))
            {
                unlocked += 1d;
            }
            return EnvironmentMath.TechnologyCap(
                settings.startingTechnologyCap,
                settings.maximumTechnologyCap,
                unlocked);
        }

        public static double RemainingCost(TINationState nation)
        {
            double rating = Rating(nation);
            double cap = TechnologyCap();
            if (!EnvironmentMath.IsFinite(rating) || !EnvironmentMath.IsFinite(cap) ||
                rating >= cap - RatingEpsilon)
            {
                return 0d;
            }

            EnvironmentSettings settings = Main.settings.environment;
            return EnvironmentMath.AdvancementCost(
                rating,
                cap,
                cap,
                Math.Max(settings.minimumGdpBillions, nation.GDP / 1000000000d),
                settings.referenceGdpBillions,
                settings.advancementBaseIp,
                settings.advancementCostGrowthBase);
        }

        public static bool AtAbsoluteCap(TINationState nation)
        {
            double rating = Rating(nation);
            return EnvironmentMath.IsFinite(rating) &&
                rating >= Main.settings.environment.maximumTechnologyCap - RatingEpsilon;
        }

        public static bool HasPersistentGreenhouseWarming()
        {
            TIGlobalValuesState values = TIGlobalValuesState.GlobalValues;
            return values != null &&
                (values.earthAtmosphericCO2_ppm > TIGlobalValuesState.safeAtmosphericCO2_ppm ||
                 values.earthAtmosphericCH4_ppm > TIGlobalValuesState.safeAtmosphericCH4_ppm ||
                 values.earthAtmosphericN2O_ppm > TIGlobalValuesState.safeAtmosphericN2O_ppm);
        }
    }

    [HarmonyPatch(typeof(TINationState), "MeanAnnualGDPDamage")]
    public static class ClimateGdpDamagePatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            float tempAnomaly_C,
            ref float __result)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled) ||
                !settings.climateGdpDamageEnabled ||
                tempAnomaly_C <= 0.25f ||
                __result >= 0f)
            {
                return;
            }

            // TI's common climate-damage method feeds both actual GDP loss and its UI
            // displays, so scaling it here keeps them synchronized. Only negative damage
            // above the game's 0.25 C warm threshold is reduced: a vanilla -2.0% result
            // becomes -1.8% at the default x0.90. Cold benefits, neutral results, and
            // climate-driven Inequality remain untouched.
            __result *= settings.climateGdpDamageMultiplier;
        }
    }

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

            double rating = EnvironmentRuntime.Rating(__instance);
            double cap = EnvironmentRuntime.TechnologyCap();
            if (!EnvironmentMath.IsFinite(rating) || !EnvironmentMath.IsFinite(cap))
            {
                Main.Warn("Environment rating conversion failed; retaining vanilla.");
                return true;
            }
            if (rating >= cap - 0.000001d)
            {
                __result = 0f;
                return false;
            }

            // Project modifiers change the effective IP delivered by one completion.
            // The underlying cost already scales with GDP and normalized cap progress.
            float effectBase = -1f;
            float modifiedEffect = effectBase + TIEffectsState.SumEffectsModifiers(
                Context.Environment_SustainabilityChange, __instance, effectBase, null);
            double effectiveInvestment = Math.Max(0d, -modifiedEffect);

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
            double remaining = EnvironmentRuntime.RemainingCost(__instance);
            effectiveInvestment *= Math.Min(1d, Math.Max(0d, remaining)) /
                (1d + falloutLoad);
            double nextRating;
            if (!EnvironmentMath.TryRatingAfterInvestment(
                rating,
                cap,
                Math.Max(settings.minimumGdpBillions, __instance.GDP / 1000000000d),
                effectiveInvestment,
                settings.referenceGdpBillions,
                settings.advancementBaseIp,
                settings.advancementCostGrowthBase,
                out nextRating))
            {
                Main.Warn("Environment investment inversion failed; retaining vanilla.");
                return true;
            }
            double storedAfter = EnvironmentMath.StoredFromRating(
                nextRating, settings.storageRatingOffset);
            double calculated = storedAfter - __instance.sustainability;

            if (double.IsNaN(calculated) || double.IsInfinity(calculated))
            {
                Main.Warn("Environment sustainability produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = (float)calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "GetRequiredInvestmentPointsForPriority")]
    public static class EnvironmentPriorityCostPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result,
            TINationState __instance,
            PriorityType priority)
        {
            if (priority != PriorityType.Environment ||
                !Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }

            double remaining = EnvironmentRuntime.RemainingCost(__instance);
            if (!EnvironmentMath.IsFinite(remaining) || remaining < 0d)
            {
                Main.Warn("Environment priority threshold was invalid; retaining vanilla.");
                return true;
            }

            __result = (float)(remaining > 0d ? Math.Min(1d, remaining) : 1d);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "BestCurrentSustainabilityValue")]
    public static class EnvironmentTechnologyFloorPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }

            double stored = EnvironmentMath.StoredFromRating(
                EnvironmentRuntime.TechnologyCap(),
                Main.settings.environment.storageRatingOffset);
            if (!EnvironmentMath.IsFinite(stored))
            {
                return true;
            }
            __result = (float)stored;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityCO2Removed")]
    public static class EnvironmentCo2RemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            TIGlobalValuesState values = TIGlobalValuesState.GlobalValues;
            __result = values == null ? 0f : (float)EnvironmentMath.ClippedRemoval(
                values.earthAtmosphericCO2_ppm,
                TIGlobalValuesState.safeAtmosphericCO2_ppm,
                settings.cleanupCO2_ppm);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityCH4Removed")]
    public static class EnvironmentMethaneRemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            TIGlobalValuesState values = TIGlobalValuesState.GlobalValues;
            __result = values == null ? 0f : (float)EnvironmentMath.ClippedRemoval(
                values.earthAtmosphericCH4_ppm,
                TIGlobalValuesState.safeAtmosphericCH4_ppm,
                settings.cleanupCH4_ppm);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "EnvPriorityN2ORemoved")]
    public static class EnvironmentNitrousOxideRemovalPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            EnvironmentSettings settings = Main.settings.environment;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            TIGlobalValuesState values = TIGlobalValuesState.GlobalValues;
            __result = values == null ? 0f : (float)EnvironmentMath.ClippedRemoval(
                values.earthAtmosphericN2O_ppm,
                TIGlobalValuesState.safeAtmosphericN2O_ppm,
                settings.cleanupN2O_ppm);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnEnvironmentPriorityComplete")]
    public static class EnvironmentPostCapCleanupPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TINationState __instance)
        {
            if (Main.FeatureEnabled(Main.settings.environment.enabled) &&
                EnvironmentRuntime.AtAbsoluteCap(__instance) &&
                EnvironmentRuntime.HasPersistentGreenhouseWarming())
            {
                TIGlobalValuesState.GlobalValues.AddEnvironmentPriorityEnvEffect(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "ValidPriority")]
    public static class EnvironmentPriorityValidityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref bool __result,
            TINationState __instance,
            PriorityType priority)
        {
            if (priority == PriorityType.Environment &&
                Main.FeatureEnabled(Main.settings.environment.enabled) &&
                EnvironmentRuntime.AtAbsoluteCap(__instance))
            {
                __result = EnvironmentRuntime.HasPersistentGreenhouseWarming() ||
                    __instance.canAccumulateDecontaminateTriggers;
            }
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

            // CO2 follows GDP and the steep energy-system curve. CH4 and N2O follow
            // population and gentler agriculture/waste curves. All three hard-stop at 10.
            double gdpBillions = Math.Max(0d, __instance.GDP / 1000000000d);
            double stored = Math.Max(0.000001d,
                __instance.sustainability + proposedSustainabilityChange);
            double rating = EnvironmentMath.RatingFromStored(
                stored, Main.settings.environment.storageRatingOffset);

            // Resource intensity remains configurable, but the calibrated default is 1:
            // starting extraction differences are already represented by national score.
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

            double co2 = EnvironmentMath.GdpGasTons(
                gdpBillions,
                rating,
                settings.co2TonsPerGdpBillionAtScoreZero,
                settings.co2DecayBase,
                resourceIntensity,
                settings.neutralRating);
            double methane = EnvironmentMath.PopulationGasTons(
                Math.Max(0d, __instance.population_Millions),
                rating,
                settings.methaneTonsPerMillionPeopleAtScoreZero,
                settings.methaneDecayBase,
                settings.neutralRating);
            double nitrousOxide = EnvironmentMath.PopulationGasTons(
                Math.Max(0d, __instance.population_Millions),
                rating,
                settings.nitrousOxideTonsPerMillionPeopleAtScoreZero,
                settings.nitrousOxideDecayBase,
                settings.neutralRating);
            if (monthly)
            {
                co2 /= settings.monthsPerYear;
                methane /= settings.monthsPerYear;
                nitrousOxide /= settings.monthsPerYear;
            }

            if (double.IsNaN(co2) || double.IsInfinity(co2) ||
                double.IsNaN(methane) || double.IsInfinity(methane) ||
                double.IsNaN(nitrousOxide) || double.IsInfinity(nitrousOxide))
            {
                Main.Warn("Geometric economy emissions produced an invalid value; retaining vanilla.");
                return true;
            }
            __result = System.Tuple.Create(co2, methane, nitrousOxide);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "SustainabilityValueForDisplay")]
    public static class EnvironmentRatingDisplayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string __result, float sustainability)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }
            double rating = EnvironmentMath.RatingFromStored(
                sustainability, Main.settings.environment.storageRatingOffset);
            if (!EnvironmentMath.IsFinite(rating))
            {
                return true;
            }
            __result = rating.ToString("0.###");
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "SustainabilityChangeForDisplay")]
    public static class EnvironmentRatingChangeDisplayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref string __result,
            TINationState __instance,
            float proposedChange)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }
            double before = EnvironmentRuntime.Rating(__instance);
            double after = EnvironmentMath.RatingFromStored(
                Math.Max(0.000001d, __instance.sustainability + proposedChange),
                Main.settings.environment.storageRatingOffset);
            if (!EnvironmentMath.IsFinite(before) || !EnvironmentMath.IsFinite(after))
            {
                return true;
            }
            __result = (after - before).ToString("+0.####;-0.####;0");
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "BestCurrentSustainabilityValueForDisplay")]
    public static class EnvironmentCapDisplayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string __result)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }
            __result = EnvironmentRuntime.TechnologyCap().ToString("0.###");
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "SustainabilityIcon")]
    public static class EnvironmentRatingIconPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string __result, TINationState __instance)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }
            double rating = EnvironmentRuntime.Rating(__instance);
            __result = rating >= 8d ? "icons_2d/ICO_GHG_emission_5" :
                rating >= 6d ? "icons_2d/ICO_GHG_emission_4" :
                rating >= 4d ? "icons_2d/ICO_GHG_emission_3" :
                rating >= 2d ? "icons_2d/ICO_GHG_emission_2" :
                "icons_2d/ICO_GHG_emission_1";
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "SustainabilityIconInlinePath")]
    public static class EnvironmentRatingInlineIconPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref string __result, TINationState __instance)
        {
            if (!Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                return true;
            }
            double rating = EnvironmentRuntime.Rating(__instance);
            __result = rating >= 8d ? TIGlobalConfig.globalConfig.sustainabilityInlineSpritePath_Green :
                rating >= 6d ? TIGlobalConfig.globalConfig.sustainabilityInlineSpritePath_Blue :
                rating >= 4d ? TIGlobalConfig.globalConfig.sustainabilityInlineSpritePath_Yellow :
                rating >= 2d ? TIGlobalConfig.globalConfig.sustainabilityInlineSpritePath_Orange :
                TIGlobalConfig.globalConfig.sustainabilityInlineSpritePath_Red;
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

        [HarmonyPostfix]
        public static void Postfix(ref float __result, TINationState __instance)
        {
            GovernmentChangeCurvePatch.Transform(ref __result, __instance);
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
            // one region at $1T gives curve 0.5 and therefore x1.5 and +0.0075;
            // at $100B curve 0.666 gives x1.666 and about +0.0833.
            // Vanilla scales this national effect by population.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            AbundanceSettings abundance = Main.settings.abundance;
            float resourceCurve = 0f;
            if (Main.FeatureEnabled(abundance.enabled))
            {
                float resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max(gdpBillions, abundance.minimumGdpBillions);
                double poweredRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                resourceCurve = double.IsPositiveInfinity(poweredRatio)
                    ? 1f
                    : (float)(poweredRatio / (1d + poweredRatio));
            }
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
