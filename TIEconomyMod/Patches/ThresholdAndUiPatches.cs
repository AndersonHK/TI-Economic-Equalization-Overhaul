using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace TIEconomyMod.Patches
{
    public static class ThresholdValues
    {
        public static int Oil()
        {
            return Current(500, Main.settings.regionThresholds.oilRemovalInvestmentPoints);
        }

        public static int Mining()
        {
            return Current(750, Main.settings.regionThresholds.miningRemovalInvestmentPoints);
        }

        public static int Economic()
        {
            return Current(1200, Main.settings.regionThresholds.economicUpgradeInvestmentPoints);
        }

        public static int Decolonization()
        {
            return Current(1000, Main.settings.regionThresholds.decolonizationInvestmentPoints);
        }

        public static int FalloutCleanup()
        {
            return Current(100, Main.settings.regionThresholds.falloutCleanupInvestmentPoints);
        }

        private static int Current(int vanilla, float configured)
        {
            RegionThresholdSettings settings = Main.settings.regionThresholds;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return vanilla;
            }

            float result = configured * settings.multiplier;
            if (float.IsNaN(result) || float.IsInfinity(result) || result <= 0f)
            {
                Main.Warn("Region threshold calculation was invalid; using the TI 1.0.32 value.");
                return vanilla;
            }

            return Math.Max(1, (int)Math.Round(result));
        }
    }

    internal static class ThresholdTranspiler
    {
        public static IEnumerable<CodeInstruction> Replace(
            IEnumerable<CodeInstruction> instructions,
            IDictionary<int, MethodInfo> replacements)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                int loaded;
                MethodInfo replacement;
                if (TryGetLoadedInteger(instruction, out loaded) &&
                    replacements.TryGetValue(loaded, out replacement))
                {
                    CodeInstruction replacementInstruction = new CodeInstruction(OpCodes.Call, replacement);
                    replacementInstruction.labels.AddRange(instruction.labels);
                    replacementInstruction.blocks.AddRange(instruction.blocks);
                    yield return replacementInstruction;
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        private static bool TryGetLoadedInteger(CodeInstruction instruction, out int value)
        {
            if (instruction.opcode == OpCodes.Ldc_I4)
            {
                value = (int)instruction.operand;
                return true;
            }

            if (instruction.opcode == OpCodes.Ldc_I4_S)
            {
                value = Convert.ToInt32(instruction.operand);
                return true;
            }

            if (instruction.opcode == OpCodes.Ldc_I4_M1) { value = -1; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_0) { value = 0; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_1) { value = 1; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_2) { value = 2; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_3) { value = 3; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_4) { value = 4; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_5) { value = 5; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_6) { value = 6; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_7) { value = 7; return true; }
            if (instruction.opcode == OpCodes.Ldc_I4_8) { value = 8; return true; }
            value = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnEconomyPriorityComplete")]
    public static class EconomyRegionThresholdPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // The game normally upgrades regions after 500 oil, 750 mining, or 1,200
            // economic completions. Replacing only those constants preserves the rest of
            // the completion flow; multiplier 2 would make them 1,000/1,500/2,400.
            return ThresholdTranspiler.Replace(instructions, new Dictionary<int, MethodInfo>
            {
                { 500, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Oil)) },
                { 750, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Mining)) },
                { 1200, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Economic)) }
            });
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnWelfarePriorityComplete")]
    public static class DecolonizationThresholdPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Vanilla decolonization requires 1,000 Welfare completions. The configured
            // base and multiplier alter only that threshold; e.g. 1,000 * 1.5 = 1,500.
            return ThresholdTranspiler.Replace(instructions, new Dictionary<int, MethodInfo>
            {
                { 1000, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Decolonization)) }
            });
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnEnvironmentPriorityComplete")]
    public static class FalloutCleanupThresholdPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Vanilla fallout cleanup requires 100 Environment completions. The configured
            // base and multiplier alter only that number; e.g. 100 * 2 = 200.
            return ThresholdTranspiler.Replace(instructions, new Dictionary<int, MethodInfo>
            {
                { 100, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.FalloutCleanup)) }
            });
        }
    }

    [HarmonyPatch(typeof(PriorityListItemController), "priorityTipStr")]
    public static class PriorityTooltipPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return ThresholdTranspiler.Replace(instructions, new Dictionary<int, MethodInfo>
            {
                { 500, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Oil)) },
                { 750, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Mining)) },
                { 1200, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Economic)) },
                { 1000, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Decolonization)) },
                { 100, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.FalloutCleanup)) }
            });
        }

        [HarmonyPostfix]
        public static void Postfix(ref string __result, TINationState nation, PriorityType priority)
        {
            if (!Main.FeatureEnabled(Main.settings.ui.enabled) ||
                !Main.settings.ui.expandedTooltips ||
                nation == null)
            {
                return;
            }

            StringBuilder section = new StringBuilder();
            if (priority == PriorityType.Economy)
            {
                if (!Main.FeatureEnabled(Main.settings.economy.enabled))
                {
                    section.AppendLine("EEO Economy formula disabled; vanilla applies.")
                        .Append("Region thresholds: oil ").Append(ThresholdValues.Oil())
                        .Append(", mining ").Append(ThresholdValues.Mining())
                        .Append(", economic ").Append(ThresholdValues.Economic());
                    __result = (__result ?? string.Empty).TrimEnd() + "\n\n" + section;
                    return;
                }

                // Recalculate the same visible components as EconomyGrowthPatch so the
                // tooltip explains the live result without replacing vanilla tooltip text.
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
                float technology = (float)Math.Max(1d, Math.Min(
                    Main.settings.technology.maximumMultiplier, compoundedTechnology));

                EconomySettings economy = Main.settings.economy;
                int cores = Math.Max(0, nation.numCoreEconomicRegions_dailyCache);
                float coreRegions = 1f + economy.coreRegionMaximumBonus * cores /
                    (economy.coreRegionHalfSaturation + cores);
                float education = 1f + economy.educationPerLevel * nation.education;
                float government = 1f + economy.governmentPerLevel * nation.democracy;
                float cohesion = economy.cohesionPeak - economy.cohesionPenaltyPerPoint *
                    Math.Abs(nation.cohesion - economy.cohesionCenter);
                float income = economy.pcgdpScale * (float)Math.Pow(
                    economy.pcgdpDecay, nation.perCapitaGDP / economy.pcgdpDecayInterval);

                float resourceBonus = 0f;
                float landBonus = 0f;
                AbundanceSettings abundance = Main.settings.abundance;
                if (Main.FeatureEnabled(abundance.enabled))
                {
                    float stability = (float)Math.Pow(Math.Max(0f, Math.Min(1f,
                        1f - nation.unrest / abundance.maximumUnrest)), abundance.unrestExponent);
                    // Match EconomyGrowthPatch: one resource region at $100B GDP gives
                    // ratio 1, curve 0.5, and half the configured maximum bonus.
                    float resourceRatio = nation.currentResourceRegions *
                        abundance.referenceGdpPerResourceRegionBillions /
                        Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                    double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                    float curve = double.IsPositiveInfinity(powered)
                        ? 1f
                        : (float)(powered / (1d + powered));
                    resourceBonus = abundance.resourceMaximumBonus * curve * stability;
                    // Density 50/km2 gives land ratio 1 and curve 0.5. At $30k PCGDP,
                    // wealth relevance is 62.5%, so the default visible bonus is 7.8%
                    // at full stability; installed vanilla has no land-density term.
                    float landRatio = abundance.referenceDensity /
                        Math.Max(nation.populationDesnity_pop_km2, abundance.minimumDensity);
                    double poweredLand = Math.Pow(landRatio, abundance.landCurveExponent);
                    float landCurve = double.IsPositiveInfinity(poweredLand)
                        ? 1f
                        : (float)(poweredLand / (1d + poweredLand));
                    float wealthRelevance = abundance.landMinimumWealthRelevance +
                        (1f - abundance.landMinimumWealthRelevance) /
                        (1f + Math.Max(0f, nation.perCapitaGDP) /
                        abundance.landReferencePcgdp);
                    landBonus = abundance.landMaximumBonus * landCurve *
                        stability * wealthRelevance;
                }

                section.AppendLine("EEO Economy")
                    .Append("Core regions x").Append(coreRegions.ToString("0.###"))
                    .Append("; education x").Append(education.ToString("0.###"))
                    .Append("; government x").Append(government.ToString("0.###"))
                    .Append("; cohesion x").Append(cohesion.ToString("0.###"))
                    .Append("; income x").Append(income.ToString("0.###")).AppendLine()
                    .Append("Weighted technology x").Append(technology.ToString("0.###"))
                    .Append(" / x").Append(Main.settings.technology.maximumMultiplier.ToString("0.###"))
                    .Append("; resources +").Append(resourceBonus.ToString("P1"))
                    .Append("; land +").Append(landBonus.ToString("P1")).AppendLine();

                InequalitySettings inequality = Main.settings.inequality;
                if (Main.FeatureEnabled(inequality.enabled) && inequality.economyEnabled)
                {
                    // One region at $100B GDP gives ratio 1 and curve 0.5, turning
                    // the default +60% maximum into a x1.30 raw-delta multiplier.
                    float resourceRatio = nation.currentResourceRegions *
                        abundance.referenceGdpPerResourceRegionBillions /
                        Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                    double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                    float curve = double.IsPositiveInfinity(powered)
                        ? 1f
                        : (float)(powered / (1d + powered));
                    float resourceMultiplier = 1f +
                        inequality.economyMaximumResourceMultiplier * curve;
                    float raw = inequality.economyPopulationDivisor /
                        Math.Max(1f, nation.population) * resourceMultiplier;
                    float bounded = nation.economyPriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x").Append((bounded / raw).ToString("0.###"))
                        .Append(" = ").Append(bounded.ToString("+0.####;-0.####;0")).AppendLine();
                }
                else
                {
                    section.AppendLine("EEO bounded Inequality disabled; vanilla applies.");
                }
                section.Append("Region thresholds: oil ").Append(ThresholdValues.Oil())
                    .Append(", mining ").Append(ThresholdValues.Mining())
                    .Append(", economic ").Append(ThresholdValues.Economic());
            }
            else if (priority == PriorityType.Welfare)
            {
                section.AppendLine("EEO Welfare");
                InequalitySettings inequality = Main.settings.inequality;
                if (Main.FeatureEnabled(inequality.enabled) && inequality.welfareEnabled)
                {
                    float raw = inequality.welfarePopulationDivisor /
                        Math.Max(1f, nation.population);
                    float bounded = nation.welfarePriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x").Append((bounded / raw).ToString("0.###"))
                        .Append(" = ").Append(bounded.ToString("+0.####;-0.####;0")).AppendLine();
                }
                else
                {
                    section.AppendLine("EEO bounded Inequality disabled; vanilla applies.");
                }
                section.Append("Decolonization threshold: ").Append(ThresholdValues.Decolonization());
            }
            else if (priority == PriorityType.Spoils)
            {
                section.AppendLine("EEO Spoils");
                InequalitySettings inequality = Main.settings.inequality;
                if (Main.FeatureEnabled(inequality.enabled) && inequality.spoilsEnabled)
                {
                    AbundanceSettings abundance = Main.settings.abundance;
                    // One region at $100B GDP gives ratio 1 and curve 0.5, turning
                    // the default +100% maximum into a x1.50 raw-delta multiplier.
                    float resourceRatio = nation.currentResourceRegions *
                        abundance.referenceGdpPerResourceRegionBillions /
                        Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                    double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                    float curve = double.IsPositiveInfinity(powered)
                        ? 1f
                        : (float)(powered / (1d + powered));
                    float resourceMultiplier = 1f +
                        inequality.spoilsMaximumResourceMultiplier * curve;
                    float raw = inequality.spoilsPopulationDivisor /
                        Math.Max(1f, nation.population) * resourceMultiplier;
                    float bounded = nation.spoilsPriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x").Append((bounded / raw).ToString("0.###"))
                        .Append(" = ").Append(bounded.ToString("+0.####;-0.####;0")).AppendLine();
                }
                else
                {
                    section.AppendLine("EEO bounded Inequality disabled; vanilla applies.");
                }
            }
            else if (priority == PriorityType.Environment)
            {
                section.Append("EEO fallout-cleanup threshold: ").Append(ThresholdValues.FalloutCleanup());
            }

            if (section.Length > 0)
            {
                __result = (__result ?? string.Empty).TrimEnd() + "\n\n" + section.ToString().TrimEnd();
            }
        }

    }

    [HarmonyPatch(typeof(NationInfoController), "BuildInvestmentTooltip")]
    public static class InvestmentTooltipPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result, TINationState nation)
        {
            if (!Main.FeatureEnabled(Main.settings.ui.enabled) ||
                !Main.settings.ui.expandedTooltips ||
                nation == null)
            {
                return;
            }

            InvestmentSettings investment = Main.settings.investment;
            float armyUpkeep = 0f;
            foreach (TIArmyState army in nation.armies)
            {
                armyUpkeep += army.investmentArmyFactor + army.investmentNavyFactor;
            }

            StringBuilder section = new StringBuilder().AppendLine("EEO Investment Points");
            if (Main.FeatureEnabled(investment.enabled))
            {
                // This mirrors InvestmentPointsPatch. A $500B nation produces 5 GDP-base
                // IP; at $7.5k PCGDP the halfway income multiplier is 85%, displaying 4.25.
                float basePoints = (float)(nation.GDP /
                    (investment.gdpPerInvestmentPointBillions * 1000000000d));
                float incomeProgress = Math.Max(0f, Math.Min(1f,
                    nation.perCapitaGDP / investment.lowIncomeThreshold));
                float incomeMultiplier = investment.lowIncomeMultiplierAtZero +
                    (1f - investment.lowIncomeMultiplierAtZero) * incomeProgress;
                section.Append("GDP base ").Append(basePoints.ToString("0.##"))
                    .Append("; low-income x").Append(incomeMultiplier.ToString("0.###"))
                    .Append("; EEO base ").Append((basePoints * incomeMultiplier).ToString("0.##")).AppendLine();
            }
            else
            {
                section.AppendLine("EEO base-IP formula disabled; vanilla applies.");
            }
            section.Append("Army and navy upkeep ").Append(armyUpkeep.ToString("0.##"));
            __result = (__result ?? string.Empty).TrimEnd() + "\n\n" + section;
        }
    }
}
