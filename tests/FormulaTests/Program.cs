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
                TestAbundance();
                TestInequality();
                TestSocialPriorities();
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
        }

        private static void TestNationalValues()
        {
            Reset();
            TINationState nation = Nation();
            nation.GDP = 500000000000d;
            nation.perCapitaGDP = 0f;
            float result = 0f;
            True(!InvestmentPointsPatch.Prefix(ref result, nation), "IP prefix replaces vanilla");
            Near(3.5f, result, 0.0001f, "IP zero-income penalty");
            nation.perCapitaGDP = 15000f;
            InvestmentPointsPatch.Prefix(ref result, nation);
            Near(5f, result, 0.0001f, "IP threshold");

            nation.economyScore = 200f;
            nation.numControlPoints = 4;
            result = 50f;
            ControlPointCostPatch.Postfix(ref result, nation);
            Near(50f, result, 0.0001f, "control cost no technology");
            GameStateManager.Research.finishedTechsNames.Add("ArrivalInternationalRelations");
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.98f) / 4f, result, 0.0001f, "control exponent sequence");

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
            float totalBillions = 0.330f * 1.36f * 2.2f * 1.3f * 1.2f *
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
            nation.population = 1000000f;
            TIEconomyMod.Main.settings.inequality.economyPopulationDivisor = 100000f;
            TIEconomyMod.Main.settings.inequality.welfarePopulationDivisor = -100000f;
            TIEconomyMod.Main.settings.inequality.spoilsPopulationDivisor = 100000f;
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
            MilitaryTechnologyPatch.Prefix(ref result, nation);
            Near(55000f / nation.population * 2f, result, 0.000001f, "military catchup");
            nation.democracy = 5f;
            nation.unrest = 3f;
            OppressionUnrestPatch.Prefix(ref result, nation);
            Near(-2222222f / nation.population * 0.5f, result, 0.000001f, "oppression unrest");
            nation.currentResourceRegions = 3;
            nation.democracy = 0f;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            Near(360f, result, 0.0001f, "spoils money resource curve");

            nation.currentResourceRegions = 1;
            nation.GDP = 100000000000d;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            float smallEconomyPayout = result;
            nation.GDP = 500000000000d;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            True(result < smallEconomyPayout, "spoils resource payout is relative to GDP");
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
