using PavonisInteractive.TerraInvicta;
using System;
using System.IO;
using TIEconomyMod.Patches;

namespace TIEconomyMod.FormulaTests
{
    internal static class Program
    {
        private static int assertions;

        private static int Main(string[] args)
        {
            try
            {
                string weights = args.Length > 0 ? args[0] :
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "..", "..", "..", "..", "TIEconomyMod", "ModFiles", "Config",
                        "economy-tech-weights.csv"));
                TIEconomyMod.Main.techWeights = TechWeightCatalog.Load(
                    weights, delegate { }, delegate { return true; });

                TestNationalValues();
                TestEconomyAndTechnology();
                TestBalanceTuning();
                TestAbundance();
                TestInequality();
                TestSocialPriorities();
                TestNationalMergers();
                TestEnvironmentUnitySpoilsAndEmissions();
                TestWeightValidation(weights);
                TestDisabledFallback();
                Console.WriteLine("PASS: " + assertions + " patch-formula assertions.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("FAIL: " + exception);
                return 1;
            }
        }

        private static void Reset()
        {
            TIEconomyMod.Main.enabled = true;
            TIEconomyMod.Main.settings = new Settings();
            GameStateManager.Research.finishedTechsNames.Clear();
            GameStateManager.TimeState.template.CPMaintenanceModifier = 1f;
        }

        private static void TestNationalValues()
        {
            Reset();
            TINationState nation = Nation();
            nation.GDP = 500000000000d;
            nation.perCapitaGDP = 0f;
            float result = 0f;
            True(!InvestmentPointsPatch.Prefix(ref result, nation), "IP prefix replaces vanilla");
            Near(3.675f, result, 0.0001f, "IP zero-income penalty and output increase");
            nation.perCapitaGDP = 15000f;
            InvestmentPointsPatch.Prefix(ref result, nation);
            Near(5.25f, result, 0.0001f, "IP threshold and output increase");

            nation.economyScore = 200f;
            nation.numControlPoints = 4;
            result = 50f;
            ControlPointCostPatch.Postfix(ref result, nation);
            Near(50f, result, 0.0001f, "control cost no technology");
            GameStateManager.Research.finishedTechsNames.Add("ArrivalInternationalRelations");
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.98f) / 4f, result, 0.0001f, "control exponent sequence");
            GameStateManager.TimeState.template.CPMaintenanceModifier = 1.2f;
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.98f) / 4f * 1.2f, result, 0.0001f,
                "TI 1.0.47 scenario control-maintenance multiplier");

            TIArmyState army = new TIArmyState { homeNation = nation, useHomeInvestmentFactor = true };
            nation.militaryTechLevel = 5f;
            ArmyUpkeepPatch.Prefix(ref result, army);
            Near(2.5f, result, 0.0001f, "army upkeep");

            nation.population_Millions = 50f;
            nation.education = 8f;
            nation.perCapitaGDP = 10000f;
            nation.democracy = 5f;
            nation.cohesion = 5f;
            nation.unrest = 2f;
            nation.adviserScienceBonus = 0.1f;
            ResearchPatch.Prefix(ref result, nation);
            float expected = 0.0037f * 50f * 64f * 0.6f *
                (float)Math.Pow(5f, 0.2f) * 1.25f * 1f * 1.1f;
            Near(expected, result, 0.0001f, "complete research formula");
        }

        private static void TestEconomyAndTechnology()
        {
            Reset();
            TINationState nation = Nation();
            TIEconomyMod.Main.settings.abundance.enabled = false;
            TIEconomyMod.Main.settings.technology.enabled = false;
            float result = 0f;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float totalBillions = 0.330f * 0.40f * 1.36f * 2.2f * 1.3f * 1.2f *
                6f * (float)Math.Pow(0.96f, 20f);
            Near(totalBillions * 1000000000f / nation.population, result, 0.0001f,
                "base Economy formula with optional modifiers disabled");

            nation.numCoreEconomicRegions_dailyCache = 1;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float oneCore = result;
            nation.numCoreEconomicRegions_dailyCache = 2;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float twoCores = result;
            nation.numCoreEconomicRegions_dailyCache = 3;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float threeCores = result;
            Near(1.20f / 1.30f, oneCore / twoCores, 0.0001f,
                "core curve gives 20 and 30 percent at one and two regions");
            Near(1.36f / 1.30f, threeCores / twoCores, 0.0001f,
                "core curve is smooth at three regions");
            nation.numCoreEconomicRegions_dailyCache = 3;

            TIEconomyMod.Main.settings.technology.enabled = true;
            GameStateManager.Research.finishedTechsNames.Add("SolidCoreFissionSystems");
            GameStateManager.Research.finishedTechsNames.Add("MoltenCoreFissionSystems");
            EconomyGrowthPatch.Prefix(ref result, nation);
            Near(totalBillions * 1.0201f * 1000000000f / nation.population,
                result, 0.0001f, "technology compounding");

            TIEconomyMod.Main.settings.technology.maximumMultiplier = 1.01f;
            EconomyGrowthPatch.Prefix(ref result, nation);
            Near(totalBillions * 1.01f * 1000000000f / nation.population,
                result, 0.0001f, "technology cap");
        }

        private static void TestBalanceTuning()
        {
            Reset();
            float technologyCost = 1000f;
            GlobalTechnologyResearchCostPatch.Postfix(ref technologyCost);
            Near(1200f, technologyCost, 0.0001f,
                "global technology costs increase by twenty percent");

            float xenofaunaTech = 6f;
            TIMegafaunaArmyState xenofauna = new TIMegafaunaArmyState();
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(5f, xenofaunaTech, 0.0001f, "xenofauna natural ceiling");
            xenofauna.bonusTechLevel = 0.4f;
            xenofaunaTech = 6.4f;
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(5.4f, xenofaunaTech, 0.0001f,
                "xenofauna keeps post-control bonuses");

            TIEconomyMod.Main.settings.technology.researchCostEnabled = false;
            technologyCost = 1000f;
            GlobalTechnologyResearchCostPatch.Postfix(ref technologyCost);
            Near(1000f, technologyCost, 0.0001f,
                "disabled technology-cost adjustment returns vanilla");
            TIEconomyMod.Main.settings.army.megafaunaEnabled = false;
            xenofaunaTech = 6f;
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(6f, xenofaunaTech, 0.0001f,
                "disabled xenofauna adjustment returns vanilla");

            TINationState nation = Nation();
            nation.economyPriorityPerCapitaIncomeChange = 12f;
            double spoilsGdp;
            SpoilsGdpGrowthPatch.Prefix(nation, out spoilsGdp);
            Near(1200000000f, (float)spoilsGdp, 1f,
                "Spoils captures the same total GDP as Economy");
            double startingGdp = nation.GDP;
            SpoilsGdpGrowthPatch.Postfix(nation, spoilsGdp);
            Near((float)(startingGdp + spoilsGdp), (float)nation.GDP, 1f,
                "Spoils applies the captured Economy GDP gain");
            TIEconomyMod.Main.settings.spoils.enabled = false;
            SpoilsGdpGrowthPatch.Prefix(nation, out spoilsGdp);
            Near(0f, (float)spoilsGdp, 0f,
                "disabled Spoils adds no GDP");
        }

        private static void TestAbundance()
        {
            Reset();
            TIEconomyMod.Main.settings.technology.enabled = false;
            TINationState nation = Nation();
            nation.currentResourceRegions = 0;
            float zeroResource = 0f;
            EconomyGrowthPatch.Prefix(ref zeroResource, nation);

            nation.currentResourceRegions = 1;
            float oneResource = 0f;
            EconomyGrowthPatch.Prefix(ref oneResource, nation);
            True(oneResource > zeroResource, "resource regions increase growth");

            nation.GDP = 2000000000000d;
            float oneResourceLargerEconomy = 0f;
            EconomyGrowthPatch.Prefix(ref oneResourceLargerEconomy, nation);
            True(oneResourceLargerEconomy < oneResource,
                "economy resource bonus is relative to GDP");

            nation.GDP = 1000000000000d;
            nation.currentResourceRegions = 2;
            float twoResources = 0f;
            EconomyGrowthPatch.Prefix(ref twoResources, nation);
            True(twoResources > oneResource, "resource curve monotonic");

            nation.currentResourceRegions = 0;
            nation.populationDesnity_pop_km2 = 1f;
            float lowDensity = 0f;
            EconomyGrowthPatch.Prefix(ref lowDensity, nation);
            nation.populationDesnity_pop_km2 = 100f;
            float highDensity = 0f;
            EconomyGrowthPatch.Prefix(ref highDensity, nation);
            True(lowDensity > highDensity, "land curve monotonic");

            nation.populationDesnity_pop_km2 = 0f;
            nation.GDP = 0d;
            float finite = 0f;
            EconomyGrowthPatch.Prefix(ref finite, nation);
            True(!float.IsNaN(finite) && !float.IsInfinity(finite), "zero density and GDP finite");

            nation = Nation();
            nation.currentResourceRegions = 1;
            nation.populationDesnity_pop_km2 = 50f;
            nation.unrest = 10f;
            float unstable = 0f;
            EconomyGrowthPatch.Prefix(ref unstable, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float noAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref noAbundance, nation);
            Near(noAbundance, unstable, 0.0001f, "unrest removes abundance bonuses");

            nation = Nation();
            TIEconomyMod.Main.settings.abundance.enabled = true;
            nation.currentResourceRegions = 1;
            nation.populationDesnity_pop_km2 = 50f;
            float stableWithAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref stableWithAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float stableWithoutAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref stableWithoutAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = true;
            nation.unrest = 5f;
            float halfStableWithAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref halfStableWithAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float halfStableWithoutAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref halfStableWithoutAbundance, nation);
            float fullBonus = stableWithAbundance / stableWithoutAbundance - 1f;
            float halfBonus = halfStableWithAbundance / halfStableWithoutAbundance - 1f;
            Near(fullBonus * 0.5f, halfBonus, 0.0001f,
                "abundance bonus scales linearly with stability");

            Reset();
            nation = Nation();
            nation.currentResourceRegions = 1;
            nation.populationDesnity_pop_km2 = 50f;
            float abundanceWithoutTech = 0f;
            EconomyGrowthPatch.Prefix(ref abundanceWithoutTech, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float baseWithoutTech = 0f;
            EconomyGrowthPatch.Prefix(ref baseWithoutTech, nation);
            TIEconomyMod.Main.settings.abundance.enabled = true;
            GameStateManager.Research.finishedTechsNames.Add("TerawattFusionReactors");
            float abundanceWithTech = 0f;
            EconomyGrowthPatch.Prefix(ref abundanceWithTech, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float baseWithTech = 0f;
            EconomyGrowthPatch.Prefix(ref baseWithTech, nation);
            Near(abundanceWithoutTech / baseWithoutTech,
                abundanceWithTech / baseWithTech, 0.0001f,
                "technology does not fade physical abundance");

            Reset();
            nation = Nation();
            nation.currentResourceRegions = 0;
            nation.populationDesnity_pop_km2 = 50f;
            // Isolate the land bonus from Economy's separate income-decay term so the
            // test can probe the configured wealth floor at deliberately extreme PCGDP.
            TIEconomyMod.Main.settings.economy.pcgdpDecay = 1f;
            nation.perCapitaGDP = 10000f;
            float poorerLand = AbundanceMultiplier(nation);
            nation.perCapitaGDP = 120000f;
            float wealthierLand = AbundanceMultiplier(nation);
            nation.perCapitaGDP = 10000000f;
            float richestLand = AbundanceMultiplier(nation);
            True(poorerLand > wealthierLand, "land relevance declines with wealth");
            True(richestLand > 1f, "land retains a wealth-floor bonus");
        }

        private static void TestInequality()
        {
            Reset();
            TINationState nation = Nation();
            nation.GDP = 100000000000d;
            float economyDefault = 0f;
            float welfareDefault = 0f;
            float spoilsDefault = 0f;
            EconomyInequalityPatch.Prefix(ref economyDefault, nation);
            WelfareInequalityPatch.Prefix(ref welfareDefault, nation);
            SpoilsInequalityPatch.Prefix(ref spoilsDefault, nation);
            Near(0.0005f, economyDefault, 0.000001f, "Economy priority Inequality value");
            Near(-0.00666666f, welfareDefault, 0.000001f, "Welfare priority Inequality value");
            Near(0.00333334f, spoilsDefault, 0.000001f, "Spoils priority Inequality value");
            float climateChange = 0.02f;
            ClimateInequalityPatch.Prefix(ref climateChange,
                TINationState.InequalityChangeReason.InqReason_ClimateChange);
            Near(0.04f, climateChange, 0.000001f, "climate Inequality doubles");
            float annexationChange = 0.02f;
            ClimateInequalityPatch.Prefix(ref annexationChange,
                TINationState.InequalityChangeReason.InqReason_Annexation);
            Near(0.02f, annexationChange, 0f, "non-climate Inequality is unchanged");
            TIEconomyMod.Main.settings.inequality.economyChangeAtReferenceGdp = 0.1f;
            TIEconomyMod.Main.settings.inequality.welfareChangeAtReferenceGdp = -0.1f;
            TIEconomyMod.Main.settings.inequality.spoilsChangeAtReferenceGdp = 0.1f;
            float[] points = { 1f, 3f, 5f, 7f, 9f };
            float[] positive = { 0.2f, 0.125f, 0.1f, 0.075f, 0f };
            float[] negative = { 0f, -0.075f, -0.1f, -0.125f, -0.2f };

            for (int index = 0; index < points.Length; index++)
            {
                nation.inequality = points[index];
                float economy = 0f;
                float welfare = 0f;
                EconomyInequalityPatch.Prefix(ref economy, nation);
                WelfareInequalityPatch.Prefix(ref welfare, nation);
                Near(positive[index], economy, 0.000001f, "positive inequality at " + points[index]);
                Near(negative[index], welfare, 0.000001f, "negative inequality at " + points[index]);
            }

            nation.inequality = 5f;
            nation.currentResourceRegions = 0;
            nation.GDP = 100000000000d;
            float referenceGdp = 0f;
            EconomyInequalityPatch.Prefix(ref referenceGdp, nation);
            nation.GDP = 1000000000000d;
            float tenfoldGdp = 0f;
            EconomyInequalityPatch.Prefix(ref tenfoldGdp, nation);
            Near(referenceGdp / 10f, tenfoldGdp, 0.000001f,
                "Inequality proportional effect divides by GDP");

            nation.GDP = 100000000000d;
            nation.currentResourceRegions = 1;
            float oneResource = 0f;
            EconomyInequalityPatch.Prefix(ref oneResource, nation);
            nation.currentResourceRegions = 2;
            float twoResources = 0f;
            EconomyInequalityPatch.Prefix(ref twoResources, nation);
            nation.currentResourceRegions = 1;
            nation.GDP = 200000000000d;
            float largerEconomy = 0f;
            EconomyInequalityPatch.Prefix(ref largerEconomy, nation);
            True(twoResources > oneResource, "resource inequality curve is continuous and monotonic");
            True(largerEconomy < oneResource, "resource inequality is relative to GDP");
            TIEconomyMod.Main.settings.abundance.enabled = false;
            nation.GDP = 100000000000d;
            float abundanceDisabled = 0f;
            EconomyInequalityPatch.Prefix(ref abundanceDisabled, nation);
            Near(0.1f, abundanceDisabled, 0.000001f,
                "disabled abundance removes the Economy Inequality resource premium");
        }

        private static void TestSocialPriorities()
        {
            Reset();
            TINationState nation = Nation();
            float result = 0f;
            KnowledgeEducationPatch.Prefix(ref result, nation);
            Near(166667f / nation.population * 4f * (float)Math.Pow(0.87f, 8f),
                result, 0.000001f, "knowledge education");
            nation.cohesion = 7f;
            KnowledgeCohesionPatch.Prefix(ref result, nation);
            Near(-333333f / nation.population, result, 0.000001f, "knowledge cohesion");
            GovernmentDemocracyPatch.Prefix(ref result, nation);
            Near(166667f / nation.population, result, 0.000001f, "government democracy");
            nation.militaryTechLevel = 4f;
            nation.maxMilitaryTechLevel = 6f;
            for (int index = 0; index < 5; index++)
            {
                nation.armies.Add(new TIArmyState { homeNation = nation });
            }
            MilitaryTechnologyPatch.Prefix(ref result, nation);
            Near(0.00275f / 5f * 2f, result, 0.000001f,
                "military technology divides by affected armies");
            nation.democracy = 5f;
            nation.unrest = 3f;
            OppressionUnrestPatch.Prefix(ref result, nation);
            Near(-2222222f / nation.population * 0.5f, result, 0.000001f, "oppression unrest");
            nation.currentResourceRegions = 1;
            nation.GDP = 100000000000d;
            nation.democracy = 5f;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            Near(172.5f, result, 0.0001f, "spoils money retains the full base payout");
            float smallEconomyPayout = result;
            nation.GDP = 500000000000d;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            True(result < smallEconomyPayout, "spoils resource payout is relative to GDP");
            TIEconomyMod.Main.settings.abundance.enabled = false;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            Near(69f, result, 0.0001f,
                "disabled abundance removes the Spoils payout resource premium");
        }

        private static void TestEnvironmentUnitySpoilsAndEmissions()
        {
            Reset();
            TINationState nation = Nation();
            nation.GDP = 100000000000d;
            nation.sustainability = 1f;
            nation.regions.Add(new TIRegionState { area_km2 = 100000f });

            float result = 0f;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            Near(-0.10f, result, 0.000001f, "environment cleanup at reference GDP");
            nation.GDP = 1000000000000d;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            Near(-0.01f, result, 0.000001f, "environment cleanup divides by GDP");

            nation.GDP = 100000000000d;
            nation.regions[0].nuclearDetonations = 1;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            Near(-0.05f, result, 0.000001f, "fallout burden is proportional to land area");
            nation.regions[0].area_km2 = 1000f;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            True(Math.Abs(result) < 0.001f, "small countries suffer denser nuclear damage");

            EnvironmentCo2RemovalPatch.Prefix(ref result, nation);
            Near(TemplateManager.global.WelCO2_ppm, result, 0f,
                "atmospheric cleanup is fixed per IP");

            nation.regions[0].nuclearDetonations = 0;
            nation.currentResourceRegions = 0;
            Tuple<double, double, double> emissions = null;
            EconomyEmissionsPatch.Prefix(ref emissions, nation, false, 0f);
            double smallEconomyCo2 = emissions.Item1;
            nation.GDP = 1000000000000d;
            EconomyEmissionsPatch.Prefix(ref emissions, nation, false, 0f);
            Near(10f, (float)(emissions.Item1 / smallEconomyCo2), 0.0001f,
                "economy emissions are linear in GDP");
            nation.population *= 10f;
            Tuple<double, double, double> sameGdpEmissions = null;
            EconomyEmissionsPatch.Prefix(ref sameGdpEmissions, nation, false, 0f);
            Near((float)emissions.Item1, (float)sameGdpEmissions.Item1, 0f,
                "economy emissions have no independent population term");

            nation.population = 100000000f;
            nation.currentResourceRegions = 1;
            nation.GDP = 100000000000d;
            Tuple<double, double, double> resourceSmall = null;
            EconomyEmissionsPatch.Prefix(ref resourceSmall, nation, false, 0f);
            nation.GDP = 1000000000000d;
            Tuple<double, double, double> resourceLarge = null;
            EconomyEmissionsPatch.Prefix(ref resourceLarge, nation, false, 0f);
            True(resourceSmall.Item1 / smallEconomyCo2 >
                resourceLarge.Item1 / emissions.Item1,
                "resource emissions intensity is relative to GDP");
            Tuple<double, double, double> cleanerProposal = null;
            EconomyEmissionsPatch.Prefix(ref cleanerProposal, nation, false, -0.5f);
            Near(0.5f, (float)(cleanerProposal.Item1 / resourceLarge.Item1), 0.0001f,
                "proposed Sustainability change updates emissions intensity");

            nation.population = 100000000f;
            nation.GDP = 100000000000d;
            nation.education = 8f;
            nation.democracy = 6f;
            UnityCohesionPatch.Prefix(ref result, nation);
            Near(3333333f / nation.population * 0.65f, result, 0.000001f,
                "Unity cohesion demographic scaling and education/government penalty");
            UnityEducationPatch.Prefix(ref result, nation);
            Near(-33333f / nation.population, result, 0.000001f,
                "Unity secondary education effect");
            SpoilsGovernmentPatch.Prefix(ref result, nation);
            Near(-66667f / nation.population, result, 0.000001f,
                "Spoils Government demographic scaling");

            nation.currentResourceRegions = 1;
            SpoilsSustainabilityPatch.Prefix(ref result, nation);
            Near(0.075f, result, 0.000001f,
                "Spoils carbon-intensity damage uses GDP and resource ratio");
            TIEconomyMod.Main.settings.abundance.enabled = false;
            SpoilsSustainabilityPatch.Prefix(ref result, nation);
            Near(0.05f, result, 0.000001f,
                "disabled abundance removes the Spoils sustainability resource premium");
        }

        private static void TestNationalMergers()
        {
            Reset();
            TINationState absorbing = Nation();
            TINationState joining = Nation();
            absorbing.militaryTechLevel = 4f;
            joining.militaryTechLevel = 6f;
            absorbing.GDP = 300000000000d;
            joining.GDP = 100000000000d;
            absorbing.armies.Add(new TIArmyState());
            absorbing.armies.Add(new TIArmyState());
            absorbing.numNavies = 1;
            joining.armies.Add(new TIArmyState());

            MilitaryMergerPatch.Snapshot militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            absorbing.militaryTechLevel = 5.75f; // stand in for TI's region-by-region result
            MilitaryMergerPatch.Postfix(absorbing, militaryState);
            Near(4.5f, absorbing.militaryTechLevel, 0.000001f,
                "merger miltech combines equal force and GDP averages");

            absorbing = Nation();
            joining = Nation();
            absorbing.militaryTechLevel = 4f;
            joining.militaryTechLevel = 8f;
            absorbing.GDP = 100000000000d;
            joining.GDP = 900000000000d;
            absorbing.armies.Add(new TIArmyState());
            absorbing.armies.Add(new TIArmyState());
            absorbing.armies.Add(new TIArmyState());
            absorbing.armies.Add(new TIArmyState());
            joining.armies.Add(new TIArmyState());
            militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            MilitaryMergerPatch.Postfix(absorbing, militaryState);
            Near(6.2f, absorbing.militaryTechLevel, 0.000001f,
                "merger miltech is exactly 50 percent force and 50 percent GDP");

            absorbing.armies.Clear();
            joining.armies.Clear();
            absorbing.militaryTechLevel = 4f;
            militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            MilitaryMergerPatch.Postfix(absorbing, militaryState);
            Near(7.6f, absorbing.militaryTechLevel, 0.000001f,
                "merger miltech without forces uses GDP");

            absorbing = Nation();
            joining = Nation();
            absorbing.militaryTechLevel = 4f;
            joining.militaryTechLevel = 8f;
            absorbing.GDP = -1d;
            joining.GDP = 0d;
            absorbing.armies.Add(new TIArmyState());
            joining.armies.Add(new TIArmyState());
            joining.armies.Add(new TIArmyState());
            joining.armies.Add(new TIArmyState());
            militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            MilitaryMergerPatch.Postfix(absorbing, militaryState);
            Near(7f, absorbing.militaryTechLevel, 0.000001f,
                "merger miltech with invalid GDP uses forces");

            absorbing.armies.Clear();
            joining.armies.Clear();
            absorbing.militaryTechLevel = 4f;
            joining.militaryTechLevel = 8f;
            militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            MilitaryMergerPatch.Postfix(absorbing, militaryState);
            Near(6f, absorbing.militaryTechLevel, 0.000001f,
                "merger miltech without forces or GDP uses the simple mean");

            absorbing = MergerNation(1f, 1000000f, 1f);
            joining = MergerNation(1f, 1f, 1f);
            InequalityMergerPatch.Snapshot inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            absorbing.inequality = 3f; // stand in for TI's population-only merge
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(8.999984f, absorbing.inequality, 0.00001f,
                "two-person extreme income split approaches Inequality 9");

            absorbing = MergerNation(1000000000f, 1000000f, 1f);
            joining = MergerNation(1000000000f, 1f, 1f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(5f, absorbing.inequality, 0.00001f,
                "two-billion-person extreme split approaches Inequality 5");

            absorbing = MergerNation(84000000f, 55000f, 3.2f);
            joining = MergerNation(68000000f, 48000f, 3.3f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(3.273239f, absorbing.inequality, 0.00001f,
                "Germany-France-like merger barely changes Inequality");

            absorbing = MergerNation(340000000f, 80000f, 4f);
            joining = MergerNation(110000000f, 4000f, 5.5f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(5.234088f, absorbing.inequality, 0.00001f,
                "US-Egypt-like merger creates a large bimodal Inequality increase");

            TINationState reverseA = MergerNation(110000000f, 4000f, 5.5f);
            TINationState reverseB = MergerNation(340000000f, 80000f, 4f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(reverseA, reverseB, ref inequalityState);
            InequalityMergerPatch.Postfix(reverseA, inequalityState);
            Near(absorbing.inequality, reverseA.inequality, 0.00001f,
                "merger Inequality is symmetric");

            TINationState orderOne = MergeInequalityForTest(
                MergeInequalityForTest(
                    MergerNation(340000000f, 80000f, 4f),
                    MergerNation(110000000f, 4000f, 5.5f)),
                MergerNation(84000000f, 55000f, 3.2f));
            TINationState orderTwo = MergeInequalityForTest(
                MergerNation(340000000f, 80000f, 4f),
                MergeInequalityForTest(
                    MergerNation(110000000f, 4000f, 5.5f),
                    MergerNation(84000000f, 55000f, 3.2f)));
            True(Math.Abs(orderOne.inequality - orderTwo.inequality) < 0.5f,
                "three-country merger order sensitivity remains bounded");

            absorbing = MergerNation(100000000f, 50000f, 4f);
            joining = MergerNation(200000000f, 50000f, 4f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(4f, absorbing.inequality, 0.00001f,
                "identical income distributions retain Inequality");

            absorbing = MergerNation(100000000f, 1000000f, 9f);
            joining = MergerNation(100000000f, 1f, 9f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            True(absorbing.inequality < 9f && absorbing.inequality > 8.99f,
                "merger Inequality remains inside the configured upper bound");

            TIEconomyMod.Main.settings.nationalMergers.enabled = false;
            absorbing = MergerNation(100f, 1000f, 2f);
            joining = MergerNation(100f, 100000f, 8f);
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            absorbing.inequality = 6f;
            InequalityMergerPatch.Postfix(absorbing, inequalityState);
            Near(6f, absorbing.inequality, 0f,
                "disabled merger patch retains vanilla");

            Reset();
            TIEconomyMod.Main.settings.nationalMergers.militaryEnabled = false;
            militaryState = null;
            MilitaryMergerPatch.Prefix(absorbing, joining, ref militaryState);
            True(militaryState == null, "disabled military merger retains vanilla");
            TIEconomyMod.Main.settings.nationalMergers.militaryEnabled = true;
            TIEconomyMod.Main.settings.nationalMergers.inequalityEnabled = false;
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            True(inequalityState == null, "disabled Inequality merger retains vanilla");
        }

        private static void TestWeightValidation(string path)
        {
            TechWeightCatalog catalog = TechWeightCatalog.Load(path, delegate { }, delegate { return true; });
            AssertWeight(catalog, "SolidCoreFissionSystems", 1f);
            AssertWeight(catalog, "MoltenCoreFissionSystems", 1f);
            AssertWeight(catalog, "GasCoreFissionSystems", 1.5f);
            AssertWeight(catalog, "AdvancedFissionSystems", 2f);
            AssertWeight(catalog, "CleanEnergy", 3f);
            AssertWeight(catalog, "TerawattFusionReactors", 4f);
            AssertWeight(catalog, "AppliedArtificialIntelligence", 2f);
            AssertWeight(catalog, "MolecularAssemblers", 2f);
            AssertWeight(catalog, "IntegratedEarthSpaceEconomy", 2f);
            AssertWeight(catalog, "Accelerando", 4f);
            AssertWeight(catalog, "MissionToSpace", 1f);
            AssertWeight(catalog, "NextGenerationAerospace", 2f);
            AssertWeight(catalog, "SpaceMiningandRefining", 2f);
            AssertWeight(catalog, "IndustrializationofSpace", 3f);

            string unknownFile = Path.GetTempFileName();
            File.WriteAllText(unknownFile,
                "tech_id,enabled,percent,rationale\nUnknown,true,2,test\n");
            True(TechWeightCatalog.Load(unknownFile, delegate { }, delegate { return false; }).Count == 0,
                "unknown CSV ID skipped");

            string duplicateFile = Path.GetTempFileName();
            File.WriteAllText(duplicateFile,
                "tech_id,enabled,percent,rationale\nKnown,false,1,test\nKnown,true,2,test\n");
            bool threw = false;
            try
            {
                TechWeightCatalog.Load(duplicateFile, delegate { }, delegate { return true; });
            }
            catch (InvalidDataException)
            {
                threw = true;
            }
            True(threw, "duplicate CSV ID fails validation");
            File.Delete(unknownFile);
            File.Delete(duplicateFile);
        }

        private static void TestDisabledFallback()
        {
            Reset();
            TINationState nation = Nation();
            float result = 123f;
            TIEconomyMod.Main.settings.economy.enabled = false;
            True(EconomyGrowthPatch.Prefix(ref result, nation), "feature toggle returns to vanilla");
            Near(123f, result, 0f, "disabled prefix leaves result untouched");
            TIEconomyMod.Main.settings.economy.enabled = true;
            TIEconomyMod.Main.settings.environment.enabled = false;
            True(EnvironmentSustainabilityPatch.Prefix(ref result, nation),
                "disabled Environment returns to vanilla");
            TIEconomyMod.Main.settings.environment.enabled = true;
            TIEconomyMod.Main.settings.unity.enabled = false;
            True(UnityCohesionPatch.Prefix(ref result, nation),
                "disabled Unity returns to vanilla");
            TIEconomyMod.Main.settings.unity.enabled = true;
            TIEconomyMod.Main.settings.spoils.enabled = false;
            True(SpoilsSustainabilityPatch.Prefix(ref result, nation),
                "disabled Spoils returns to vanilla");
            TIEconomyMod.Main.settings.spoils.enabled = true;
            TIEconomyMod.Main.settings.emissions.enabled = false;
            Tuple<double, double, double> gases = null;
            True(EconomyEmissionsPatch.Prefix(ref gases, nation, false, 0f),
                "disabled GDP emissions returns to vanilla");
            TIEconomyMod.Main.settings.emissions.enabled = true;
            TIEconomyMod.Main.settings.enabled = false;
            True(EconomyGrowthPatch.Prefix(ref result, nation), "global toggle returns to vanilla");
            TIEconomyMod.Main.settings.enabled = true;
            TIEconomyMod.Main.enabled = false;
            True(EconomyGrowthPatch.Prefix(ref result, nation), "loader toggle returns to vanilla");
        }

        private static TINationState Nation()
        {
            return new TINationState
            {
                GDP = 1000000000000d,
                perCapitaGDP = 20000f,
                economyScore = 10f,
                numControlPoints = 4,
                population = 100000000f,
                population_Millions = 100f,
                education = 8f,
                democracy = 6f,
                cohesion = 5f,
                unrest = 0f,
                inequality = 5f,
                numCoreEconomicRegions_dailyCache = 3,
                currentResourceRegions = 0,
                populationDesnity_pop_km2 = 100f,
                militaryTechLevel = 4f,
                maxMilitaryTechLevel = 5f
            };
        }

        private static TINationState MergerNation(float population, float income, float inequality)
        {
            TINationState nation = Nation();
            nation.population = population;
            nation.population_Millions = population / 1000000f;
            nation.perCapitaGDP = income;
            nation.GDP = population * (double)income;
            nation.inequality = inequality;
            return nation;
        }

        private static TINationState MergeInequalityForTest(TINationState absorbing, TINationState joining)
        {
            InequalityMergerPatch.Snapshot state = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref state);
            absorbing.GDP += joining.GDP;
            absorbing.population += joining.population;
            absorbing.population_Millions = absorbing.population / 1000000f;
            absorbing.perCapitaGDP = (float)(absorbing.GDP / absorbing.population);
            InequalityMergerPatch.Postfix(absorbing, state);
            return absorbing;
        }

        private static float AbundanceMultiplier(TINationState nation)
        {
            float withAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref withAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float withoutAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref withoutAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = true;
            return withAbundance / withoutAbundance;
        }

        private static void AssertWeight(TechWeightCatalog catalog, string id, float expected)
        {
            float actual;
            True(catalog.TryGetPercent(id, out actual), "weight exists for " + id);
            Near(expected, actual, 0f, "semantic weight for " + id);
        }

        private static void Near(float expected, float actual, float tolerance, string name)
        {
            assertions++;
            if (Math.Abs(expected - actual) > tolerance)
            {
                throw new InvalidOperationException(
                    name + ": expected " + expected + " but got " + actual + ".");
            }
        }

        private static void True(bool value, string name)
        {
            assertions++;
            if (!value)
            {
                throw new InvalidOperationException(name + ".");
            }
        }
    }
}
