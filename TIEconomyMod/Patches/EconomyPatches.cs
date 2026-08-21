using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "economyPriorityPerCapitaIncomeChange", MethodType.Getter)]
    public static class EconomyGrowthPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            EconomySettings economy = Main.settings.economy;
            if (!Main.FeatureEnabled(economy.enabled))
            {
                return true;
            }

            // Technologies do three visible jobs. Their productivity percentages compound
            // directly (two 1% starting technologies give 1.01 * 1.01 = x1.0201), while
            // their labor and resource weights measure progress from 0 to 1 through the
            // future tree. Mission to Space and Advanced Chemical Rocketry contribute to
            // productivity but not progress because their substitution is already present
            // in the modern starting floors. Faction projects remain vanilla and are not
            // counted here, avoiding ownership ambiguity and double counting.
            double productivity = 1d;
            float completedLaborWeight = 0f;
            float completedResourceWeight = 0f;
            TechnologySettings technologySettings = Main.settings.technology;
            bool technologyEnabled = Main.FeatureEnabled(technologySettings.enabled) &&
                Main.techWeights != null && Main.techWeights.Count > 0;
            if (technologyEnabled)
            {
                foreach (string technologyId in GameStateManager.GlobalResearch().finishedTechsNames)
                {
                    TechWeights technology;
                    if (Main.techWeights.TryGetWeights(technologyId, out technology))
                    {
                        productivity *= 1d + technology.ProductivityPercent / 100d;
                        if (!TechWeightCatalog.IsStartingTechnology(technologyId))
                        {
                            completedLaborWeight += technology.LaborSubstitution;
                            completedResourceWeight += technology.ResourceSubstitution;
                        }
                    }
                }
            }
            float productivityMultiplier = (float)Math.Max(1d, Math.Min(
                technologySettings.maximumMultiplier, productivity));
            float laborProgress = technologyEnabled
                ? Math.Max(0f, Math.Min(1f, completedLaborWeight /
                    Main.techWeights.TotalFutureLaborWeight))
                : 0f;
            float resourceProgress = technologyEnabled
                ? Math.Max(0f, Math.Min(1f, completedResourceWeight /
                    Main.techWeights.TotalFutureResourceWeight))
                : 0f;

            // Labor support combines the institutional and human factors that let capital
            // be used productively. Core regions follow one smooth saturating curve:
            //   1 + maximumBonus * cores / (halfSaturation + cores)
            // Defaults give x1.40, x1.60, x1.72, and x1.80 for one through four
            // regions, approaching but never reaching x2.20. The full product is divided
            // by the reference nation (one core, Education 7, Government 6, Cohesion 5),
            // so that reference has labor support 1. Installed vanilla 1.0.51 instead adds
            // a flat per-capita amount for every core region.
            int cores = Math.Max(0, __instance.numCoreEconomicRegions_dailyCache);
            float coreRegionMultiplier = 1f + economy.coreRegionMaximumBonus * cores /
                (economy.coreRegionHalfSaturation + cores);
            float educationMultiplier = 1f + economy.educationPerLevel * __instance.education;
            float governmentMultiplier = 1f + economy.governmentPerLevel * __instance.democracy;
            float cohesionMultiplier = economy.cohesionPeak -
                economy.cohesionPenaltyPerPoint *
                Math.Abs(__instance.cohesion - economy.cohesionCenter);
            float referenceCoreMultiplier = 1f + economy.coreRegionMaximumBonus *
                economy.referenceCoreRegions /
                (economy.coreRegionHalfSaturation + economy.referenceCoreRegions);
            float referenceEducationMultiplier = 1f +
                economy.educationPerLevel * economy.referenceEducation;
            float referenceGovernmentMultiplier = 1f +
                economy.governmentPerLevel * economy.referenceGovernment;
            float referenceCohesionMultiplier = economy.cohesionPeak -
                economy.cohesionPenaltyPerPoint *
                Math.Abs(economy.referenceCohesion - economy.cohesionCenter);
            float referenceLabor = referenceCoreMultiplier *
                referenceEducationMultiplier * referenceGovernmentMultiplier *
                referenceCohesionMultiplier;
            float laborSupport = coreRegionMultiplier * educationMultiplier *
                governmentMultiplier * cohesionMultiplier /
                Math.Max(economy.minimumSupport, referenceLabor);

            float resourceBonus = 0f;
            float landBonus = 0f;
            AbundanceSettings abundance = Main.settings.abundance;
            if (Main.FeatureEnabled(abundance.enabled))
            {
                float gdpBillions = (float)(__instance.GDP / 1000000000d);
                // Unrest is the stability gate: unrest 0 gives 100%, unrest 5 gives
                // 50%, and unrest 10 gives 0% with the default linear exponent.
                // Technology does not reduce physical land or resource advantages.
                float stability = (float)Math.Pow(Math.Max(0f, Math.Min(1f,
                    1f - __instance.unrest / abundance.maximumUnrest)),
                    abundance.unrestExponent);

                // Resource abundance is measured against national GDP and follows
                // ratio^0.30 / (1 + ratio^0.30). At the defaults, one region in a $1T
                // economy has ratio 1, curve 0.5, and a +50% bonus at unrest 0. The same
                // region in a $100B economy has ratio 10 and about +66.6%; in a $10T
                // economy it has ratio 0.1 and about +33.4%. Oil therefore matters much
                // more to a Saudi-sized economy than a US-sized one without ever becoming
                // irrelevant. Vanilla applies a flat region bonus unrelated to GDP.
                float resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max(gdpBillions, abundance.minimumGdpBillions);
                double poweredRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                float curve = double.IsPositiveInfinity(poweredRatio)
                    ? 1f
                    : (float)(poweredRatio / (1d + poweredRatio));
                resourceBonus = abundance.resourceMaximumBonus * curve * stability;

                // Population density is population / land, so its inverse measures land
                // per worker. At 50 people/km2 the ratio and curve are 1 and 0.5. At
                // $30k GDP/c the wealth relevance is 62.5%, producing +7.8% at unrest 0;
                // at extreme wealth it approaches +3.1%, retaining cheap housing and
                // industrial land after agriculture becomes less important. Vanilla has
                // no continuous land-density Economy term.
                float landRatio = abundance.referenceDensity /
                    Math.Max(__instance.populationDesnity_pop_km2, abundance.minimumDensity);
                double poweredLandRatio = Math.Pow(landRatio, abundance.landCurveExponent);
                float landCurve = double.IsPositiveInfinity(poweredLandRatio)
                    ? 1f
                    : (float)(poweredLandRatio / (1d + poweredLandRatio));
                float wealthRelevance = abundance.landMinimumWealthRelevance +
                    (1f - abundance.landMinimumWealthRelevance) /
                    (1f + Math.Max(0f, __instance.perCapitaGDP) /
                    abundance.landReferencePcgdp);
                landBonus = abundance.landMaximumBonus * landCurve *
                    stability * wealthRelevance;
            }

            // GDP/c is capital per worker. It creates labor and resource pressure relative
            // to the support available. Technology relief p*(0.10 + 0.90p) raises each
            // return floor smoothly from 0.35/0.45 to 1.00, so early capital has strongly
            // diminishing returns but a completed tree makes further capital nearly
            // linear. Better support shifts the knee rather than acting as an unrelated
            // multiplier. Example: GDP/c $37,500 with labor support 1 has pressure 1 and
            // a starting labor constraint 0.675; doubling support reduces pressure to 0.5
            // and raises the constraint to about 0.821.
            float laborRelief = laborProgress *
                (economy.technologyReliefLinearShare +
                 (1f - economy.technologyReliefLinearShare) * laborProgress);
            float resourceRelief = resourceProgress *
                (economy.technologyReliefLinearShare +
                 (1f - economy.technologyReliefLinearShare) * resourceProgress);
            float laborFloor = economy.startingLaborReturnFloor +
                (1f - economy.startingLaborReturnFloor) * laborRelief;
            float resourceFloor = economy.startingResourceReturnFloor +
                (1f - economy.startingResourceReturnFloor) * resourceRelief;
            float resourceSupport = 1f + resourceBonus + landBonus;
            float perCapitaGdp = Math.Max(0f, __instance.perCapitaGDP);
            float laborPressure = perCapitaGdp / economy.laborKneePcgdp /
                Math.Max(economy.minimumSupport, laborSupport);
            float resourcePressure = perCapitaGdp / economy.resourceKneePcgdp /
                Math.Max(economy.minimumSupport, resourceSupport);
            float laborConstraint = laborFloor + (1f - laborFloor) /
                (1f + (float)Math.Pow(laborPressure, economy.laborPressureExponent));
            float resourceConstraint = resourceFloor + (1f - resourceFloor) /
                (1f + (float)Math.Pow(resourcePressure, economy.resourcePressureExponent));

            // One completed IP first creates a national GDP gain. Productivity lifts the
            // whole curve; labor and resource constraints limit returns when capital is
            // abundant relative to the other factors; abundance also adds a modest direct
            // lift. A proportional doubling of capital, labor, and resources therefore
            // doubles the national return, while capital alone yields less than double.
            // Only this getter boundary divides by population because TI asks for GDP/c.
            float totalGainBillions = economy.baseGainBillions *
                productivityMultiplier * laborConstraint * resourceConstraint *
                (1f + economy.resourceDirectLift * resourceBonus +
                 economy.landDirectLift * landBonus);
            float calculated = totalGainBillions * 1000000000f /
                Math.Max(1f, __instance.population);

            if (float.IsNaN(calculated) || float.IsInfinity(calculated))
            {
                Main.Warn("Economy growth produced an invalid value; using zero rather than corrupting the save.");
                calculated = 0f;
            }
            __result = calculated;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnSpoilsPriorityComplete")]
    public static class SpoilsGdpGrowthPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TINationState __instance, out double __state)
        {
            // Spoils represents rapid, extractive economic expansion: it adds exactly
            // the same total GDP as one Economy completion, but keeps Spoils' separate
            // inequality, Government, Sustainability, propaganda, and faction-cash costs.
            // Capture before those effects run so a Spoils Government loss cannot alter
            // this completion's output. Example: $10 PCGDP growth in a 100M-person nation
            // is $1B GDP. TI 1.0.51 has no Spoils GDP effect, so this is an added behavior.
            __state = 0d;
            if (Main.FeatureEnabled(Main.settings.spoils.enabled))
            {
                __state = __instance.economyPriorityPerCapitaIncomeChange *
                    __instance.population_Millions * 1000000d;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, double __state)
        {
            if (!double.IsNaN(__state) && !double.IsInfinity(__state) && __state > 0d)
            {
                // TI has no Spoils GDP tracking enum; EconomyPriority is the accurate
                // ledger category because the added quantity uses that exact formula.
                __instance.ModifyGDP(__state,
                    TINationState.GDPChangeReason.GDPReason_EconomyPriority);
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "economyPriorityInequalityChange", MethodType.Getter)]
    public static class EconomyInequalityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            InequalitySettings settings = Main.settings.inequality;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.economyEnabled)
            {
                return true;
            }

            // Resource-driven inequality uses the same GDP-relative curve as Economy
            // growth. One region in a $1T economy gives ratio 1 and curve 0.5, so the
            // default +60% maximum becomes a x1.30 raw-delta multiplier. At $100B the
            // ratio is 10 and the exponent-0.30 curve is 0.666, making it x1.40.
            // Installed vanilla 1.0.51
            // adds a flat 0.0001 per resource region before its population scaling.
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
                settings.economyMaximumResourceMultiplier * resourceCurve;
            // Inequality is a proportional economic outcome, so the affected stock is
            // GDP rather than headcount. Defaults give +0.00225 in a $100B economy and
            // +0.000225 in a $1T economy before resources/bounds. Since the latter also
            // produces about 10x the IP, equal priority allocation produces the same
            // monthly national change instead of rewarding either union or breakup.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            float rawDelta = settings.economyChangeAtReferenceGdp *
                settings.referenceGdpBillions / gdpBillions * resourceMultiplier;

            // Map TI's 1–9 scale to a continuous -1..+1 position around neutral 5.
            // The directional curve makes an inward change x3 at either endpoint,
            // stays x1 at 5, and suppresses outward change to zero at the boundary.
            float transformedDelta = InequalityMath.TransformPriorityChange(rawDelta,
                __instance.inequality, settings.minimum, settings.neutral,
                settings.maximum, settings.exponent,
                settings.maximumDirectionalMultiplier);

            if (float.IsNaN(transformedDelta) || float.IsInfinity(transformedDelta))
            {
                Main.Warn("Economy inequality produced an invalid value; using zero.");
                transformedDelta = 0f;
            }
            __result = transformedDelta;
            return false;
        }
    }
}
