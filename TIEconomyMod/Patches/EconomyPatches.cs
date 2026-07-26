using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

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

            // Every completed global technology listed in the CSV multiplies the result;
            // two 2% technologies therefore give 1.02 * 1.02 = 1.0404, not 1.04.
            // Installed vanilla 1.0.47 has no comparable global-technology multiplier:
            // it applies project/effect modifiers directly to its per-capita base instead.
            // Faction projects stay out of this calculation so those vanilla bonuses are
            // neither attributed to the wrong nation nor counted twice.
            double compoundedTechnology = 1d;
            if (Main.FeatureEnabled(Main.settings.technology.enabled) && Main.techWeights != null)
            {
                foreach (string technologyId in GameStateManager.GlobalResearch().finishedTechsNames)
                {
                    float percent;
                    if (Main.techWeights.TryGetPercent(technologyId, out percent))
                    {
                        compoundedTechnology *= 1d + percent / 100d;
                    }
                }
            }
            float technologyMultiplier = (float)Math.Max(1d, Math.Min(
                Main.settings.technology.maximumMultiplier, compoundedTechnology));

            // Core Economic regions follow one smooth saturating curve:
            //   1 + maximumBonus * cores / (halfSaturation + cores)
            // Defaults give x1.20, x1.30, x1.36, and x1.40 for one through four
            // regions, approaching but never exceeding x1.60. Installed vanilla 1.0.47
            // instead adds a flat 1.5 dollars of per-capita growth per core region.
            int cores = Math.Max(0, __instance.numCoreEconomicRegions_dailyCache);
            float coreRegionMultiplier = 1f + economy.coreRegionMaximumBonus * cores /
                (economy.coreRegionHalfSaturation + cores);

            float educationMultiplier = 1f + economy.educationPerLevel * __instance.education;
            float governmentMultiplier = 1f + economy.governmentPerLevel * __instance.democracy;
            float cohesionMultiplier = economy.cohesionPeak -
                economy.cohesionPenaltyPerPoint *
                Math.Abs(__instance.cohesion - economy.cohesionCenter);
            float incomeMultiplier = economy.pcgdpScale * (float)Math.Pow(
                economy.pcgdpDecay,
                __instance.perCapitaGDP / economy.pcgdpDecayInterval);

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

                // Resource abundance is measured against national GDP, so it matters most
                // to poor resource-rich states without an artificial technology penalty.
                // Example: 1 region and $100B GDP gives ratio 1, curve 0.5, and +50%
                // growth at full stability. At $1T GDP the same region gives ratio 0.1,
                // curve 0.091, and about +9.1%. Installed vanilla 1.0.47 instead adds the
                // same flat 1.5 dollars of per-capita growth for that region at any GDP.
                float resourceRatio = __instance.currentResourceRegions *
                    abundance.referenceGdpPerResourceRegionBillions /
                    Math.Max(gdpBillions, abundance.minimumGdpBillions);
                double poweredRatio = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                float curve = double.IsPositiveInfinity(poweredRatio)
                    ? 1f
                    : (float)(poweredRatio / (1d + poweredRatio));
                resourceBonus = abundance.resourceMaximumBonus * curve * stability;

                // Population density is population / land, so inverting it measures land
                // available per person. At 50 people/km2 the ratio is 1 and the density
                // curve supplies half the 25% maximum. Wealth then reduces agriculture and
                // forestry's share without erasing cheap land's housing/industry value:
                // at $30k PCGDP the default relevance is 62.5%, making the final full-
                // stability bonus about +7.8%; at extreme wealth it approaches +3.1%.
                // Installed vanilla 1.0.47 has no land-density Economy bonus.
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

            // This formula first calculates a total GDP gain, then divides it by population
            // because the patched getter returns per-capita change. For a 50M-person nation
            // at $20k PCGDP, Education 8, Government 7, Cohesion 5, one core and one
            // resource region, defaults produce roughly $1.8B before land and technology.
            // Installed vanilla 1.0.47 produces about $875M per completion for the same
            // demographic inputs because it uses a much smaller additive per-capita model.
            float totalGainBillions = economy.baseGainBillions * economy.outputMultiplier *
                coreRegionMultiplier *
                educationMultiplier * governmentMultiplier * cohesionMultiplier *
                incomeMultiplier * technologyMultiplier *
                (1f + resourceBonus + landBonus);
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
            // is $1B GDP. TI 1.0.47 has no Spoils GDP effect, so this is an added behavior.
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
            // growth. One region in a $100B economy gives ratio 1 and curve 0.5, so the
            // default +60% maximum becomes a x1.30 raw-delta multiplier. At $1T the
            // ratio is 0.1 and the multiplier is only x1.055. Installed vanilla 1.0.47
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
            // GDP rather than headcount. Defaults give +0.0005 in a $100B economy and
            // +0.00005 in a $1T economy before resources/bounds. Since the latter also
            // produces about 10x the IP, equal priority allocation produces the same
            // monthly national change instead of rewarding either union or breakup.
            float gdpBillions = Math.Max(settings.minimumGdpBillions,
                (float)(__instance.GDP / 1000000000d));
            float rawDelta = settings.economyChangeAtReferenceGdp *
                settings.referenceGdpBillions / gdpBillions * resourceMultiplier;

            // Map TI's 1–9 scale to a continuous -1..+1 position around neutral 5.
            // This single smooth transform makes positive Economy change 2x at 1, 1x
            // at 5, and 0x at 9. Negative deltas would naturally behave in reverse.
            float position = (__instance.inequality - settings.neutral) /
                ((settings.maximum - settings.minimum) / 2f);
            float transformedDelta = rawDelta * (1f - Math.Sign(rawDelta) * position *
                (float)Math.Pow(Math.Abs(position), settings.exponent - 1f));

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
