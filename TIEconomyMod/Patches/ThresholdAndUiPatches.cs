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
        public static int Oil(TIGlobalConfig config)
        {
            return Current(config.numEcosForCoreOilRegion,
                Main.settings.regionThresholds.oilRemovalInvestmentPoints);
        }

        public static int Mining(TIGlobalConfig config)
        {
            return Current(config.numEcosForCoreMiningRegion,
                Main.settings.regionThresholds.miningRemovalInvestmentPoints);
        }

        public static int Economic(TIGlobalConfig config)
        {
            return Current(config.numEcosForCoreEcoRegion,
                Main.settings.regionThresholds.economicUpgradeInvestmentPoints);
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
                Main.Warn("Region threshold calculation was invalid; using the live TI 1.0.51 value.");
                return vanilla;
            }

            return Math.Max(1, (int)Math.Round(result));
        }
    }

    internal static class ThresholdTranspiler
    {
        public static IEnumerable<CodeInstruction> Replace(
            IEnumerable<CodeInstruction> instructions,
            IDictionary<int, MethodInfo> constantReplacements,
            IDictionary<FieldInfo, MethodInfo> fieldReplacements,
            int expectedReplacements,
            string patchName)
        {
            List<CodeInstruction> patched = new List<CodeInstruction>();
            int replacementCount = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                int loaded;
                MethodInfo replacement;
                if (TryGetLoadedInteger(instruction, out loaded) &&
                    constantReplacements.TryGetValue(loaded, out replacement))
                {
                    CodeInstruction replacementInstruction = new CodeInstruction(OpCodes.Call, replacement);
                    replacementInstruction.labels.AddRange(instruction.labels);
                    replacementInstruction.blocks.AddRange(instruction.blocks);
                    patched.Add(replacementInstruction);
                    replacementCount++;
                }
                else if (instruction.opcode == OpCodes.Ldfld &&
                    instruction.operand is FieldInfo &&
                    fieldReplacements.TryGetValue((FieldInfo)instruction.operand, out replacement))
                {
                    // ldfld consumes TIGlobalConfig and pushes the integer. The helper
                    // consumes the same config instance and pushes the configured integer,
                    // so stack shape and every surrounding gameplay branch remain intact.
                    CodeInstruction replacementInstruction = new CodeInstruction(OpCodes.Call, replacement);
                    replacementInstruction.labels.AddRange(instruction.labels);
                    replacementInstruction.blocks.AddRange(instruction.blocks);
                    patched.Add(replacementInstruction);
                    replacementCount++;
                }
                else
                {
                    patched.Add(instruction);
                }
            }
            if (replacementCount != expectedReplacements)
            {
                string message = patchName + " IL changed: expected " +
                    expectedReplacements + " threshold loads, found " +
                    replacementCount + ". Refusing a partial compatibility patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
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
            // TI 1.0.51 keeps these values in TIGlobalConfig fields.
            // Replacing exactly those three field loads preserves the complete vanilla
            // completion method. Defaults multiply 500/750/1,200 by five, producing
            // 2,500/3,750/6,000 IP; missing fields fail loudly instead of silently no-op.
            return ThresholdTranspiler.Replace(
                instructions,
                new Dictionary<int, MethodInfo>(),
                new Dictionary<FieldInfo, MethodInfo>
                {
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreOilRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Oil)) },
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreMiningRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Mining)) },
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreEcoRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Economic)) }
                },
                3,
                nameof(EconomyRegionThresholdPatch));
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnWelfarePriorityComplete")]
    public static class DecolonizationThresholdPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Vanilla requires 1,000 Welfare IP. The default x5 makes this 5,000,
            // while replacing exactly one constant preserves every other completion effect.
            return ThresholdTranspiler.Replace(
                instructions,
                new Dictionary<int, MethodInfo>
                {
                    { 1000, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Decolonization)) }
                },
                new Dictionary<FieldInfo, MethodInfo>(),
                1,
                nameof(DecolonizationThresholdPatch));
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnEnvironmentPriorityComplete")]
    public static class FalloutCleanupThresholdPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Vanilla charges 100 Environment IP per detonation. The default x5 charges
            // 500 per nuke in every country; land area affects damage, not cleanup cost.
            return ThresholdTranspiler.Replace(
                instructions,
                new Dictionary<int, MethodInfo>
                {
                    { 100, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.FalloutCleanup)) }
                },
                new Dictionary<FieldInfo, MethodInfo>(),
                1,
                nameof(FalloutCleanupThresholdPatch));
        }
    }

    [HarmonyPatch(typeof(PriorityListItemController), "priorityTipStr")]
    public static class PriorityTooltipPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // The tooltip uses the same three config fields and two constants as gameplay.
            // Requiring all five replacements prevents a UI/gameplay mismatch after updates.
            return ThresholdTranspiler.Replace(
                instructions,
                new Dictionary<int, MethodInfo>
                {
                    { 1000, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Decolonization)) },
                    { 100, AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.FalloutCleanup)) }
                },
                new Dictionary<FieldInfo, MethodInfo>
                {
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreOilRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Oil)) },
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreMiningRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Mining)) },
                    { AccessTools.Field(typeof(TIGlobalConfig), "numEcosForCoreEcoRegion"),
                        AccessTools.Method(typeof(ThresholdValues), nameof(ThresholdValues.Economic)) }
                },
                5,
                nameof(PriorityTooltipPatch));
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
                        .Append("Region thresholds: oil ").Append(ThresholdValues.Oil(TemplateManager.global))
                        .Append(", mining ").Append(ThresholdValues.Mining(TemplateManager.global))
                        .Append(", economic ").Append(ThresholdValues.Economic(TemplateManager.global));
                    __result = (__result ?? string.Empty).TrimEnd() + "\n\n" + section;
                    return;
                }

                // Recalculate the same visible components as EconomyGrowthPatch so the
                // tooltip explains the live result without replacing vanilla tooltip text.
                double productivity = 1d;
                float completedLaborWeight = 0f;
                float completedResourceWeight = 0f;
                bool technologyEnabled =
                    Main.FeatureEnabled(Main.settings.technology.enabled) &&
                    Main.techWeights != null && Main.techWeights.Count > 0;
                if (technologyEnabled)
                {
                    foreach (string technologyId in GameStateManager.GlobalResearch().finishedTechsNames)
                    {
                        TechWeights weights;
                        if (Main.techWeights.TryGetWeights(technologyId, out weights))
                        {
                            productivity *= 1d + weights.ProductivityPercent / 100d;
                            if (!TechWeightCatalog.IsStartingTechnology(technologyId))
                            {
                                completedLaborWeight += weights.LaborSubstitution;
                                completedResourceWeight += weights.ResourceSubstitution;
                            }
                        }
                    }
                }
                float productivityMultiplier = (float)Math.Max(1d, Math.Min(
                    Main.settings.technology.maximumMultiplier, productivity));
                float laborProgress = technologyEnabled
                    ? Math.Max(0f, Math.Min(1f, completedLaborWeight /
                        Main.techWeights.TotalFutureLaborWeight))
                    : 0f;
                float resourceProgress = technologyEnabled
                    ? Math.Max(0f, Math.Min(1f, completedResourceWeight /
                        Main.techWeights.TotalFutureResourceWeight))
                    : 0f;

                EconomySettings economy = Main.settings.economy;
                int cores = Math.Max(0, nation.numCoreEconomicRegions_dailyCache);
                float coreRegions = 1f + economy.coreRegionMaximumBonus * cores /
                    (economy.coreRegionHalfSaturation + cores);
                float education = 1f + economy.educationPerLevel * nation.education;
                float government = 1f + economy.governmentPerLevel * nation.democracy;
                float cohesion = economy.cohesionPeak - economy.cohesionPenaltyPerPoint *
                    Math.Abs(nation.cohesion - economy.cohesionCenter);
                float referenceCore = 1f + economy.coreRegionMaximumBonus *
                    economy.referenceCoreRegions /
                    (economy.coreRegionHalfSaturation + economy.referenceCoreRegions);
                float referenceEducation = 1f +
                    economy.educationPerLevel * economy.referenceEducation;
                float referenceGovernment = 1f +
                    economy.governmentPerLevel * economy.referenceGovernment;
                float referenceCohesion = economy.cohesionPeak -
                    economy.cohesionPenaltyPerPoint *
                    Math.Abs(economy.referenceCohesion - economy.cohesionCenter);
                float laborSupport = coreRegions * education * government * cohesion /
                    Math.Max(economy.minimumSupport, referenceCore * referenceEducation *
                        referenceGovernment * referenceCohesion);

                float resourceBonus = 0f;
                float landBonus = 0f;
                AbundanceSettings abundance = Main.settings.abundance;
                if (Main.FeatureEnabled(abundance.enabled))
                {
                    float stability = (float)Math.Pow(Math.Max(0f, Math.Min(1f,
                        1f - nation.unrest / abundance.maximumUnrest)), abundance.unrestExponent);
                    // Match EconomyGrowthPatch: one resource region at $1T GDP gives
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
                float laborPressure = Math.Max(0f, nation.perCapitaGDP) /
                    economy.laborKneePcgdp /
                    Math.Max(economy.minimumSupport, laborSupport);
                float resourceSupport = 1f + resourceBonus + landBonus;
                float resourcePressure = Math.Max(0f, nation.perCapitaGDP) /
                    economy.resourceKneePcgdp /
                    Math.Max(economy.minimumSupport, resourceSupport);
                float laborConstraint = laborFloor + (1f - laborFloor) /
                    (1f + (float)Math.Pow(laborPressure, economy.laborPressureExponent));
                float resourceConstraint = resourceFloor + (1f - resourceFloor) /
                    (1f + (float)Math.Pow(resourcePressure, economy.resourcePressureExponent));
                float perCapitaGain = nation.economyPriorityPerCapitaIncomeChange;
                float aggregateGainBillions = perCapitaGain *
                    nation.population_Millions / 1000f;

                section.AppendLine("EEO Economy")
                    .Append("Economy and Spoils +$")
                    .Append(perCapitaGain.ToString("0.##"))
                    .Append("/person (+$")
                    .Append(aggregateGainBillions.ToString("0.###"))
                    .AppendLine("B GDP)")
                    .Append("Productivity x")
                    .Append(productivityMultiplier.ToString("0.###"))
                    .Append(" / x").Append(Main.settings.technology.maximumMultiplier.ToString("0.###"))
                    .AppendLine()
                    .Append("Labor: support ").Append(laborSupport.ToString("0.###"))
                    .Append("; progress ").Append(laborProgress.ToString("P1"))
                    .Append("; pressure ").Append(laborPressure.ToString("0.###"))
                    .Append("; constraint ").Append(laborConstraint.ToString("P1"))
                    .AppendLine()
                    .Append("Resources: +").Append(resourceBonus.ToString("P1"))
                    .Append("; land +").Append(landBonus.ToString("P1"))
                    .Append("; progress ").Append(resourceProgress.ToString("P1"))
                    .Append("; pressure ").Append(resourcePressure.ToString("0.###"))
                    .Append("; constraint ").Append(resourceConstraint.ToString("P1"))
                    .AppendLine();

                InequalitySettings inequality = Main.settings.inequality;
                if (Main.FeatureEnabled(inequality.enabled) && inequality.economyEnabled)
                {
                    // One region at $1T GDP gives ratio 1 and curve 0.5, turning
                    // the default +60% maximum into a x1.30 raw-delta multiplier.
                    float curve = 0f;
                    if (Main.FeatureEnabled(abundance.enabled))
                    {
                        float resourceRatio = nation.currentResourceRegions *
                            abundance.referenceGdpPerResourceRegionBillions /
                            Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                        double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                        curve = double.IsPositiveInfinity(powered)
                            ? 1f
                            : (float)(powered / (1d + powered));
                    }
                    float resourceMultiplier = 1f +
                        inequality.economyMaximumResourceMultiplier * curve;
                    float gdpBillions = Math.Max(inequality.minimumGdpBillions,
                        (float)(nation.GDP / 1000000000d));
                    float raw = inequality.economyChangeAtReferenceGdp *
                        inequality.referenceGdpBillions / gdpBillions *
                        resourceMultiplier;
                    float bounded = nation.economyPriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x")
                        .Append((raw == 0f ? 0f : bounded / raw).ToString("0.###"))
                        .Append(" = ").Append(bounded.ToString("+0.####;-0.####;0")).AppendLine();
                }
                else
                {
                    section.AppendLine("EEO bounded Inequality disabled; vanilla applies.");
                }
                section.Append("Region thresholds: oil ").Append(ThresholdValues.Oil(TemplateManager.global))
                    .Append(", mining ").Append(ThresholdValues.Mining(TemplateManager.global))
                    .Append(", economic ").Append(ThresholdValues.Economic(TemplateManager.global));

                if (Main.FeatureEnabled(Main.settings.emissions.enabled))
                {
                    // This invokes the same patched gameplay getter, so the displayed
                    // monthly emissions cannot drift from the GDP-only implementation.
                    System.Tuple<double, double, double> gases =
                        nation.GHGsFromEconomy_tons(true, 0f);
                    section.AppendLine()
                        .Append("GDP emissions/month: CO2 ")
                        .Append(gases.Item1.ToString("N0"))
                        .Append("t; CH4 ").Append(gases.Item2.ToString("N0"))
                        .Append("t; N2O ").Append(gases.Item3.ToString("N0")).Append("t");
                }
            }
            else if (priority == PriorityType.Welfare)
            {
                section.AppendLine("EEO Welfare");
                InequalitySettings inequality = Main.settings.inequality;
                if (Main.FeatureEnabled(inequality.enabled) && inequality.welfareEnabled)
                {
                    float gdpBillions = Math.Max(inequality.minimumGdpBillions,
                        (float)(nation.GDP / 1000000000d));
                    float raw = inequality.welfareChangeAtReferenceGdp *
                        inequality.referenceGdpBillions / gdpBillions;
                    float bounded = nation.welfarePriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x")
                        .Append((raw == 0f ? 0f : bounded / raw).ToString("0.###"))
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
                    // One region at $1T GDP gives ratio 1 and curve 0.5, turning
                    // the default +100% maximum into a x1.50 raw-delta multiplier.
                    float curve = 0f;
                    if (Main.FeatureEnabled(abundance.enabled))
                    {
                        float resourceRatio = nation.currentResourceRegions *
                            abundance.referenceGdpPerResourceRegionBillions /
                            Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                        double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                        curve = double.IsPositiveInfinity(powered)
                            ? 1f
                            : (float)(powered / (1d + powered));
                    }
                    float resourceMultiplier = 1f +
                        inequality.spoilsMaximumResourceMultiplier * curve;
                    float gdpBillions = Math.Max(inequality.minimumGdpBillions,
                        (float)(nation.GDP / 1000000000d));
                    float raw = inequality.spoilsChangeAtReferenceGdp *
                        inequality.referenceGdpBillions / gdpBillions *
                        resourceMultiplier;
                    float bounded = nation.spoilsPriorityInequalityChange;
                    section.Append("Inequality raw ").Append(raw.ToString("+0.####;-0.####;0"))
                        .Append("; bounded x")
                        .Append((raw == 0f ? 0f : bounded / raw).ToString("0.###"))
                        .Append(" = ").Append(bounded.ToString("+0.####;-0.####;0")).AppendLine();
                }
                else
                {
                    section.AppendLine("EEO bounded Inequality disabled; vanilla applies.");
                }
                if (Main.FeatureEnabled(Main.settings.spoilsMoney.enabled))
                {
                    // Repeat the live one-line payout inputs: one resource at $1T gives
                    // curve .5/x2.5; Government 5 gives x1.15 and a $172.50 payout.
                    AbundanceSettings abundance = Main.settings.abundance;
                    SpoilsMoneySettings money = Main.settings.spoilsMoney;
                    float curve = 0f;
                    if (Main.FeatureEnabled(abundance.enabled))
                    {
                        float resourceRatio = nation.currentResourceRegions *
                            abundance.referenceGdpPerResourceRegionBillions /
                            Math.Max((float)(nation.GDP / 1000000000d), abundance.minimumGdpBillions);
                        double powered = Math.Pow(resourceRatio, abundance.resourceCurveExponent);
                        curve = double.IsPositiveInfinity(powered)
                            ? 1f
                            : (float)(powered / (1d + powered));
                    }
                    float resourceMultiplier = 1f +
                        (money.maximumResourceMultiplier - 1f) * curve;
                    float governmentMultiplier = money.governmentBaseMultiplier -
                        money.governmentPenaltyPerLevel * Math.Max(0f,
                            Math.Min(money.fullGovernment, nation.democracy));
                    section.Append("Money: $").Append(nation.spoilsPriorityMoney.ToString("0.##"))
                        .Append("M (resource x").Append(resourceMultiplier.ToString("0.###"))
                        .Append(", Government x").Append(governmentMultiplier.ToString("0.###"))
                        .AppendLine(")");
                }
                if (Main.FeatureEnabled(Main.settings.spoils.enabled))
                {
                    float perCapitaGain = nation.economyPriorityPerCapitaIncomeChange;
                    section.Append("Economy and Spoils +$")
                        .Append(perCapitaGain.ToString("0.##"))
                        .Append("/person (+$")
                        .Append((perCapitaGain * nation.population_Millions / 1000f)
                            .ToString("0.###"))
                        .AppendLine("B GDP)");
                    section.Append("Government ").Append(
                            nation.spoilsPriorityDemocracyChange.ToString("+0.####;-0.####;0"))
                        .Append("; Sustainability ").Append(
                            nation.spoilsSustainabilityChange.ToString("+0.####;-0.####;0"))
                        .Append("; propaganda x")
                        .Append(Main.settings.spoils.propagandaMultiplier.ToString("0.###"));
                }
            }
            else if (priority == PriorityType.Unity)
            {
                section.AppendLine("EEO Unity");
                if (Main.FeatureEnabled(Main.settings.unity.enabled))
                {
            // These are the actual patched getters; TI's full 1.0.51 completion
                    // method remains in charge of claims and all other secondary behavior.
                    section.Append("Cohesion ").Append(
                            nation.unityPriorityCohesionChange.ToString("+0.####;-0.####;0"))
                        .Append("; Education ").Append(
                            nation.unityPriorityEducationChange.ToString("+0.####;-0.####;0"))
                        .Append("; propaganda x")
                        .Append(Main.settings.unity.propagandaMultiplier.ToString("0.###"));
                }
                else
                {
                    section.Append("EEO Unity formulas disabled; vanilla applies.");
                }
            }
            else if (priority == PriorityType.Military)
            {
                if (!Main.FeatureEnabled(Main.settings.military.enabled))
                {
                    section.AppendLine("EEO Military formula disabled; vanilla applies.");
                }
                else
                {
                    ArmySettings army = Main.settings.army;
                    MilitarySettings military = Main.settings.military;
                    int armyCount = MilitaryRuntime.EligibleArmyCount(nation);
                    double currentTech = nation.militaryTechLevel;
                    double cap = nation.maxMilitaryTechLevel;
                    double targetTech = Math.Min(
                        cap, Math.Floor(currentTech + 0.0000001d) + 1d);
                    double doctrine = MilitaryMath.DoctrineCost(
                        currentTech, targetTech, cap,
                        military.doctrineBaseCostAtTechOne,
                        military.doctrineCostGrowthBase,
                        military.catchupGapCoefficient);
                    double upgrades = MilitaryMath.ArmyUpgradeCost(
                        currentTech, targetTech, armyCount,
                army.costCoefficient, army.costGrowthBase);
                    double remaining = MilitaryRuntime.Cost(nation, currentTech, cap);
                    double nextTech;
                    double nextIp = Math.Min(1d, Math.Max(0d, remaining));
                    bool solved =
                        MilitaryRuntime.TryTechnologyAfter(nation, nextIp, out nextTech);

                    section.AppendLine("EEO continuous Military investment")
                        .Append("Current ").Append(currentTech.ToString("0.####"))
                        .Append("; cap ").Append(cap.ToString("0.####"))
                        .Append("; eligible armies ").Append(armyCount)
                        .Append("; marginal catch-up x")
                        .Append(MilitaryMath.CatchUpCostMultiplier(
                            currentTech, cap,
                            military.catchupGapCoefficient).ToString("0.####"))
                        .AppendLine()
                        .Append("Cost to ").Append(targetTech.ToString("0.####"))
                        .Append(": doctrine ").Append(doctrine.ToString("0.##"))
                        .Append(" + upgrades ").Append(upgrades.ToString("0.##"))
                        .Append(" = ").Append((doctrine + upgrades).ToString("0.##"))
                        .Append(" IP; remaining to cap ")
                        .Append(remaining.ToString("0.##"));
                    if (solved)
                    {
                        section.AppendLine()
                            .Append("Next ").Append(nextIp.ToString("0.####"))
                            .Append(" IP adds ")
                            .Append((nextTech - currentTech).ToString("0.######"))
                            .Append(" Military technology.");
                    }
                }
            }
            else if (priority == PriorityType.Military_BuildArmy)
            {
                if (!Main.FeatureEnabled(Main.settings.army.enabled))
                {
                    section.AppendLine(
                        "EEO army construction and repair formula disabled; vanilla applies.");
                }
                else
                {
                    double cost = MilitaryMath.ArmyCost(
                        nation.militaryTechLevel,
                        Main.settings.army.costCoefficient,
                Main.settings.army.costGrowthBase);
                    float progress = nation.GetAccumulatedInvestmentPoints(
                        PriorityType.Military_BuildArmy);
                    section.Append("EEO army cost ").Append(cost.ToString("0.##"))
                        .Append(" IP at Military technology ")
                        .Append(nation.militaryTechLevel.ToString("0.####"));
                    if (progress < 0f)
                    {
                        section.AppendLine()
                            .Append("Repair debt ").Append((-progress).ToString("0.##"))
                            .Append(
                                " IP; new investment repays this before construction.");
                    }
                }
            }
            else if (priority == PriorityType.Environment)
            {
                section.AppendLine("EEO Environment");
                if (Main.FeatureEnabled(Main.settings.environment.enabled))
                {
                    EnvironmentSettings environment = Main.settings.environment;
                    float landArea = 0f;
                    int detonations = 0;
                    foreach (TIRegionState region in nation.regions)
                    {
                        landArea += Math.Max(0f, region.area_km2);
                        detonations += Math.Max(0, region.nuclearDetonations);
                    }
                    float falloutLoad = detonations * environment.falloutReferenceAreaKm2 /
                        Math.Max(landArea, environment.minimumLandAreaKm2);
                    if (nation.sustainability > 0f)
                    {
                        section.Append("Sustainability ").Append(
                                nation.environmentPrioritySustainabilityChange
                                    .ToString("+0.####;-0.####;0"))
                            .Append(" (fallout x")
                            .Append((1f / (1f + falloutLoad)).ToString("0.###"))
                            .AppendLine("); reduces national economy emissions only.");
                    }
                    else
                    {
                        section.AppendLine(
                            "Sustainability is zero; additional Environment IP does not remove atmospheric gases.");
                    }
                    section.Append("Warm-climate GDP damage x")
                        .Append((environment.climateGdpDamageEnabled
                            ? environment.climateGdpDamageMultiplier
                            : 1f).ToString("0.###"))
                        .AppendLine();
                }
                else
                {
                    section.AppendLine("EEO Environment formulas disabled; vanilla applies.");
                }
                section.Append("Fallout cleanup: ").Append(ThresholdValues.FalloutCleanup())
                    .Append(" IP per detonation");
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
                // IP; at $7.5k PCGDP, x.85 income and x1.05 output display 4.46.
                float basePoints = (float)(nation.GDP /
                    (investment.gdpPerInvestmentPointBillions * 1000000000d));
                float incomeProgress = Math.Max(0f, Math.Min(1f,
                    nation.perCapitaGDP / investment.lowIncomeThreshold));
                float incomeMultiplier = investment.lowIncomeMultiplierAtZero +
                    (1f - investment.lowIncomeMultiplierAtZero) * incomeProgress;
                section.Append("GDP base ").Append(basePoints.ToString("0.##"))
                    .Append("; low-income x").Append(incomeMultiplier.ToString("0.###"))
                    .Append("; output x").Append(investment.outputMultiplier.ToString("0.###"))
                    .Append("; EEO base ").Append((basePoints * incomeMultiplier *
                        investment.outputMultiplier).ToString("0.##")).AppendLine();
            }
            else
            {
                section.AppendLine("EEO base-IP formula disabled; vanilla applies.");
            }
            section.Append("Army and navy upkeep ").Append(armyUpkeep.ToString("0.##"));
            float repairDebt = nation.GetAccumulatedInvestmentPoints(
                PriorityType.Military_BuildArmy);
            if (Main.FeatureEnabled(Main.settings.army.enabled) && repairDebt < 0f)
            {
                section.AppendLine()
                    .Append("Army repair debt ").Append((-repairDebt).ToString("0.##"))
                    .Append(" IP");
            }
            __result = (__result ?? string.Empty).TrimEnd() + "\n\n" + section;
        }
    }
}
