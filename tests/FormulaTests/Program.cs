using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.IO;
using TIEconomyMod.Core;
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
                string configDirectory = Path.GetDirectoryName(weights);
                TIEconomyMod.Main.hullDriveScales =
                    HullDriveScaleCatalog.Load(
                        Path.Combine(
                            configDirectory,
                            "hull-variant-drive-scales.csv"),
                        delegate { });
                TIEconomyMod.Main.hullVolumes = HullVolumeCatalog.Load(
                    Path.Combine(
                        configDirectory,
                        "hull-variant-main-volumes.csv"),
                    delegate { });

                TestNationalValues();
                TestMilitaryMath();
                TestMineMissionControl();
                TestIndependentResearch();
                TestGlobalTechnologySelection();
                TestEconomyAndTechnology();
                TestBalanceTuning();
                TestUtilityFootprints();
                TestPerformanceCaches();
                TestWeaponCadence();
                TestAbundance();
                TestInequality();
                TestSocialPriorities();
                TestNationalMergers();
                TestEnvironmentUnitySpoilsAndEmissions();
                TestHabRebalanceMath();
                TestEarthLaunchCostMath();
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
            TIEffectsState.FactionEffects.Clear();
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
            Near(60f, result, 0.0001f, "control cost 20 percent increase");
            GameStateManager.Research.finishedTechsNames.Add("ArrivalInternationalRelations");
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.98f) / 4f * 1.2f, result, 0.0001f,
                "earliest technology reduces the exponent by 0.02");
            GameStateManager.Research.finishedTechsNames.Add("UnityMovements");
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.95f) / 4f * 1.2f, result, 0.0001f,
                "second technology reduces the exponent by 0.03");
            GameStateManager.TimeState.template.CPMaintenanceModifier = 1.2f;
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.95f) / 4f * 1.2f * 1.2f, result, 0.0001f,
            "TI 1.0.51 scenario control-maintenance multiplier");

            GameStateManager.Research.finishedTechsNames.Clear();
            GameStateManager.Research.finishedTechsNames.Add("Accelerando");
            GameStateManager.TimeState.template.CPMaintenanceModifier = 1f;
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.95f) / 4f * 1.2f, result, 0.0001f,
                "late technology independently reduces the exponent by 0.05");

            GameStateManager.Research.finishedTechsNames.UnionWith(new[]
            {
                "ArrivalInternationalRelations",
                "UnityMovements",
                "GreatNations",
                "ArrivalGovernance"
            });
            ControlPointCostPatch.Postfix(ref result, nation);
            Near((float)Math.Pow(200f, 0.80f) / 4f * 1.2f, result, 0.0001f,
                "all five explicit reductions total 0.20");

            TIFactionState faction = new TIFactionState();
            TIEffectsState.FactionEffects.Add(new TIEffectTemplate
            {
                dataName = "Effect_ControlPointMaintenanceBonus40",
                value = -40f
            });
            result = 350f; // 310 from every non-project flat source, plus vanilla's 40.
            ControlPointCapacityPatch.Postfix(ref result, faction);
            Near(434f, result, 0.0001f,
                "project percentage multiplies all non-project flat capacity");

            TIEffectsState.FactionEffects.Clear();
            TIEffectsState.FactionEffects.Add(new TIEffectTemplate
            {
                dataName = "Effect_ControlPointMaintenanceBonus3",
                value = -5f
            });
            result = 315f;
            ControlPointCapacityPatch.Postfix(ref result, faction);
            Near(325.5f, result, 0.0001f, "Management Research remains five percent");

            TIEffectsState.FactionEffects.Add(new TIEffectTemplate
            {
                dataName = "Effect_ControlPointMaintenanceBonus3",
                value = -5f
            });
            result = 320f;
            ControlPointCapacityPatch.Postfix(ref result, faction);
            Near(341f, result, 0.0001f, "repeatable project percentages stack additively");

            faction.IsAlienFaction = true;
            result = 20000f;
            ControlPointCapacityPatch.Postfix(ref result, faction);
            Near(20000f, result, 0.0001f, "alien control capacity remains unchanged");

            Near(0.5f, (float)MilitaryMath.Upkeep(5d, true, 10d, 3d), 0.0001f,
                "home army upkeep");
            Near(5f / 3f, (float)MilitaryMath.Upkeep(5d, false, 10d, 3d), 0.0001f,
                "away army upkeep");

            nation.population_Millions = 50f;
            nation.education = 8f;
            nation.perCapitaGDP = 10000f;
            nation.democracy = 5f;
            nation.cohesion = 5f;
            nation.unrest = 2f;
            nation.adviserScienceBonus = 0.1f;
            ResearchPatch.Prefix(ref result, nation);
            float expected = 0.0038f * 50f * 64f * 1.1f *
                (float)Math.Pow(5f, 0.2f) * 1.25f * 1f * 1.1f;
            Near(expected, result, 0.0001f,
                "complete research formula uses offset PCGDP multiplier");

            nation.perCapitaGDP = 0f;
            ResearchPatch.Prefix(ref result, nation);
            expected = 0.0038f * 50f * 64f * 0.6f *
                (float)Math.Pow(5f, 0.2f) * 1.25f * 1f * 1.1f;
            Near(expected, result, 0.0001f,
                "zero PCGDP retains a sixty-percent income multiplier");
        }

        private static void TestGlobalTechnologySelection()
        {
            Near(2000f, (float)GlobalTechnologySelectionMath.Median(
                new double[] { 3000d, 1000d, 2000d }), 0.000001f,
                "technology selection odd median");
            Near(2500f, (float)GlobalTechnologySelectionMath.Median(
                new double[] { 4000d, 1000d }), 0.000001f,
                "technology selection even median");
            Near(0f, (float)GlobalTechnologySelectionMath.Median(
                new double[] { 1000d, double.NaN }), 0.000001f,
                "technology selection rejects invalid median input");

            Near(1f, (float)GlobalTechnologySelectionMath.PriorityMultiplier(-2),
                0.000001f, "technology selection nonpositive tier multiplier");
            Near(2f, (float)GlobalTechnologySelectionMath.PriorityMultiplier(1),
                0.000001f, "technology selection tier one multiplier");
            Near(4f, (float)GlobalTechnologySelectionMath.PriorityMultiplier(2),
                0.000001f, "technology selection tier two multiplier");
            Near(10f, (float)GlobalTechnologySelectionMath.PriorityMultiplier(5),
                0.000001f, "technology selection tier five multiplier");
            Near(14f, (float)GlobalTechnologySelectionMath.PriorityMultiplier(7),
                0.000001f, "technology selection tier seven multiplier");

            double exponent = 0.75d;
            double minimum = 0.25d;
            double maximum = 4d;
            Near(1f, (float)GlobalTechnologySelectionMath.CostMultiplier(
                10000d, 10000d, exponent, minimum, maximum), 0.000001f,
                "technology selection median cost is neutral");
            Near((float)Math.Pow(0.25d, exponent),
                (float)GlobalTechnologySelectionMath.CostMultiplier(
                    4000d, 1000d, exponent, minimum, maximum), 0.000001f,
                "technology selection fourfold cost penalty");
            Near(4f, (float)GlobalTechnologySelectionMath.CostMultiplier(
                250d, 10000d, exponent, minimum, maximum), 0.000001f,
                "technology selection cheap-cost multiplier is capped");
            Near(0.25f, (float)GlobalTechnologySelectionMath.CostMultiplier(
                1000000d, 10000d, exponent, minimum, maximum), 0.000001f,
                "technology selection expensive-cost multiplier is floored");
            Near(
                (float)GlobalTechnologySelectionMath.CostMultiplier(
                    4000d, 10000d, exponent, minimum, maximum),
                (float)GlobalTechnologySelectionMath.CostMultiplier(
                    5600d, 14000d, exponent, minimum, maximum),
                0.000001f,
                "technology selection ignores uniform research-cost scaling");

            double cheapCostMultiplier =
                GlobalTechnologySelectionMath.CostMultiplier(
                    1000d, 2500d, exponent, minimum, maximum);
            double expensiveCostMultiplier =
                GlobalTechnologySelectionMath.CostMultiplier(
                    4000d, 2500d, exponent, minimum, maximum);
            double cheapWeight = GlobalTechnologySelectionMath.SelectionWeight(
                1d, 1d, 0, cheapCostMultiplier, 1d);
            double expensiveWeight =
                GlobalTechnologySelectionMath.SelectionWeight(
                    1d, 1d, 0, expensiveCostMultiplier, 1d);
            Near(0.2612039f,
                (float)(expensiveWeight / (cheapWeight + expensiveWeight)),
                0.000001f,
                "fourfold-cost peer retains meaningful selection probability");

            cheapCostMultiplier = GlobalTechnologySelectionMath.CostMultiplier(
                1000d, 10000d, exponent, minimum, maximum);
            double strategicCostMultiplier =
                GlobalTechnologySelectionMath.CostMultiplier(
                    20000d, 10000d, exponent, minimum, maximum);
            cheapWeight = GlobalTechnologySelectionMath.SelectionWeight(
                1d, 1d, 0, cheapCostMultiplier, 1d);
            double strategicWeight =
                GlobalTechnologySelectionMath.SelectionWeight(
                    1d, 1d, 5, strategicCostMultiplier, 1d);
            True(cheapWeight > 0d && strategicWeight > 0d,
                "soft tiers keep cheap and strategic technologies eligible");
            True(strategicWeight / cheapWeight > 1d &&
                strategicWeight / cheapWeight < 2d,
                "soft tier balances 1k tier zero against 20k tier five");
            Near((float)GlobalTechnologySelectionMath.MinimumSelectionWeight,
                (float)GlobalTechnologySelectionMath.SelectionWeight(
                    0d, 1d, 0, 1d, 1d), 0f,
                "zero-valued technology retains minimum lottery weight");
            Near(0f, (float)GlobalTechnologySelectionMath.SelectionWeight(
                double.NaN, 1d, 0, 1d, 1d), 0f,
                "invalid technology score requests vanilla fallback");
        }

        private static void TestUtilityFootprints()
        {
            UtilityGridCell anchor = new UtilityGridCell(5, 1);
            List<UtilityGridCell> horizontal =
                UtilityFootprintMath.GetCells(
                    anchor, UtilityFootprintKind.TwoHorizontal);
            True(horizontal.Count == 2,
                "two-horizontal footprint cell count");
            True(horizontal[0].Equals(new UtilityGridCell(5, 1)),
                "two-horizontal anchor cell");
            True(horizontal[1].Equals(new UtilityGridCell(6, 1)),
                "two-horizontal secondary cell");

            List<UtilityGridCell> vertical = UtilityFootprintMath.GetCells(
                anchor, UtilityFootprintKind.TwoVertical);
            True(vertical[1].Equals(new UtilityGridCell(5, 3)),
                "vertical utility rows follow hull y-step");

            List<UtilityGridCell> four = UtilityFootprintMath.GetCells(
                anchor, UtilityFootprintKind.Four);
            True(four.Count == 4, "four-slot footprint cell count");
            True(new HashSet<UtilityGridCell>(four).Count == 4,
                "four-slot footprint has unique cells");
            True(four.Contains(new UtilityGridCell(6, 3)),
                "four-slot footprint includes lower-right cell");

            List<UtilityGridCell> candidates = new List<UtilityGridCell>
            {
                new UtilityGridCell(5, 1),
                new UtilityGridCell(6, 1),
                new UtilityGridCell(7, 1)
            };
            HashSet<UtilityGridCell> available =
                new HashSet<UtilityGridCell>(candidates);
            HashSet<UtilityGridCell> occupied =
                new HashSet<UtilityGridCell>();
            UtilityGridCell resolved;
            True(UtilityFootprintMath.TryResolveAnchor(
                    new UtilityGridCell(6, 1),
                    UtilityFootprintKind.TwoHorizontal,
                    candidates,
                    available,
                    occupied,
                    true,
                    out resolved),
                "drop on a covered cell resolves a two-slot anchor");
            True(resolved.Equals(new UtilityGridCell(6, 1)),
                "alternate placement prefers dropped cell as anchor");

            available.Remove(new UtilityGridCell(7, 1));
            True(UtilityFootprintMath.TryResolveAnchor(
                    new UtilityGridCell(6, 1),
                    UtilityFootprintKind.TwoHorizontal,
                    candidates,
                    available,
                    occupied,
                    true,
                    out resolved),
                "drop on right cell resolves the only containing pair");
            True(resolved.Equals(new UtilityGridCell(5, 1)),
                "right-cell drop shifts to the valid left anchor");

            occupied.Add(new UtilityGridCell(5, 1));
            True(!UtilityFootprintMath.TryResolveAnchor(
                    new UtilityGridCell(6, 1),
                    UtilityFootprintKind.TwoHorizontal,
                    candidates,
                    available,
                    occupied,
                    true,
                    out resolved),
                "occupied primary rejects the whole footprint");

            True(UtilityFootprintMath.HasCompatibleAnchor(
                    UtilityFootprintKind.TwoHorizontal,
                    candidates,
                    available),
                "catalog compatibility ignores current occupancy");

            occupied.Clear();
            True(!UtilityFootprintMath.TryResolveAnchor(
                    new UtilityGridCell(6, 1),
                    UtilityFootprintKind.TwoHorizontal,
                    candidates,
                    available,
                    occupied,
                    false,
                    out resolved),
                "strict placement does not shift from an invalid anchor");
        }

        private static void TestMilitaryMath()
        {
            const double armyCoefficient = 2d;
            const double armyGrowthBase = 2d;
            const double doctrineBaseCost = 500d;
            const double doctrineGrowthBase = 2d;
            const double catchUpCoefficient = 1d;

            Near(8f, (float)MilitaryMath.ArmyCost(2d, armyCoefficient, armyGrowthBase),
                0.000001f, "tech 2 army cost");
            Near(16f, (float)MilitaryMath.ArmyCost(3d, armyCoefficient, armyGrowthBase),
                0.000001f, "tech 3 army cost");
            Near(32f, (float)MilitaryMath.ArmyCost(4d, armyCoefficient, armyGrowthBase),
                0.000001f, "tech 4 army cost");
            Near(64f, (float)MilitaryMath.ArmyCost(5d, armyCoefficient, armyGrowthBase),
                0.000001f, "tech 5 army cost");
            Near(22.627417f, (float)MilitaryMath.ArmyCost(3.5d, armyCoefficient, armyGrowthBase),
                0.000001f, "fractional-tech army cost");
            Near(32f, (float)MilitaryMath.ArmyUpgradeCost(
                4d, 5d, 1, armyCoefficient, armyGrowthBase), 0.000001f,
                "4 to 5 upgrade cost per army");

            Near(0.5f, (float)MilitaryMath.CatchUpCostMultiplier(
                4d, 5d, catchUpCoefficient), 0.000001f,
                "tech 4 cap 5 catch-up multiplier");
            for (int technology = 1; technology <= 5; technology++)
            {
                double intervalCost = MilitaryMath.DoctrineCost(
                    technology, technology + 1d, technology + 1d,
                    doctrineBaseCost, doctrineGrowthBase, 0d);
                Near((float)(500d * Math.Pow(2d, technology - 1)),
                    (float)intervalCost, 0.001f,
                    "undiscounted doctrine cost at tech " + technology);
            }
            double doctrineFourToFive = MilitaryMath.DoctrineCost(
                4d, 5d, 5d, doctrineBaseCost, doctrineGrowthBase,
                catchUpCoefficient);
            Near(2883.5919f, (float)doctrineFourToFive,
                0.001f, "continuously discounted doctrine cost 4 to 5");
            Near((float)(doctrineFourToFive + 32d * 3d),
                (float)MilitaryMath.MiltechCost(
                    4d, 5d, 3, 5d, armyCoefficient, armyGrowthBase,
                    doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient),
                0.001f, "doctrine plus three army upgrades");

            int[] armyCounts = { 0, 1, 12 };
            foreach (int armyCount in armyCounts)
            {
                double invested = MilitaryMath.MiltechCost(
                    2.75d, 4.35d, armyCount, 5d, armyCoefficient, armyGrowthBase,
                    doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient);
                double solved;
                True(MilitaryMath.TrySolveTechAfterInvestment(
                    2.75d, 5d, armyCount, invested, armyCoefficient, armyGrowthBase,
                    doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                    out solved),
                    "miltech inversion succeeds for " + armyCount + " armies");
                True(Math.Abs(solved - 4.35d) < 1e-8d,
                    "miltech inversion round trip for " + armyCount + " armies");
            }

            double firstTechnology;
            True(MilitaryMath.TrySolveTechAfterInvestment(
                3d, 5d, 1, 500d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out firstTechnology),
                "first sequential military investment solves");
            double secondTechnology;
            True(MilitaryMath.TrySolveTechAfterInvestment(
                firstTechnology, 5d, 8, 500d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out secondTechnology),
                "army-count change reprices next military investment");
            double sameCountTechnology;
            True(MilitaryMath.TrySolveTechAfterInvestment(
                firstTechnology, 5d, 1, 500d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out sameCountTechnology),
                "comparison military investment solves");
            True(secondTechnology < sameCountTechnology,
                "more armies reduce later tech gain");

            double cappedTechnology;
            True(MilitaryMath.TrySolveTechAfterInvestment(
                4.9999d, 5d, 20, 1d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out cappedTechnology),
                "partial final cap investment solves");
            Near(5f, (float)cappedTechnology, 0.000001f,
                "partial final investment clamps exactly to cap");

            Near(0.16f, (float)MilitaryMath.RepairCharge(
                4d, 0.01d, 0.5d, armyCoefficient, armyGrowthBase),
                0.000001f, "one-percent tech-4 repair charge");
            Near(0.32f, (float)MilitaryMath.RepairCharge(
                4d, 0.02d, 0.5d, armyCoefficient, armyGrowthBase),
                0.000001f, "two-percent tech-4 repair charge");
            Near(0.08f, (float)MilitaryMath.RepairCharge(
                4d, 0.005d, 0.5d, armyCoefficient, armyGrowthBase),
                0.000001f, "repair charge uses healing capped near full strength");
            double progress;
            True(MilitaryMath.TryApplyBuildArmyProgress(
                0d, -0.32d, false, out progress),
                "repair charge can create debt");
            Near(-0.32f, (float)progress, 0.000001f,
                "negative Build Army progress persists");
            MilitaryMath.TryApplyBuildArmyProgress(
                progress, 0.10d, false, out progress);
            Near(-0.22f, (float)progress, 0.000001f,
                "future investment first repays debt");
            MilitaryMath.TryApplyBuildArmyProgress(
                progress, 0.30d, false, out progress);
            Near(0.08f, (float)progress, 0.000001f,
                "investment creates construction progress only after debt");
            MilitaryMath.TryApplyBuildArmyProgress(
                -0.50d, 0.20d, false, out progress);
            Near(-0.30f, (float)progress, 0.000001f,
                "army destruction refund offsets debt");
            MilitaryMath.TryApplyBuildArmyProgress(
                -1d, -0.32d, false, out progress);
            Near(-1.32f, (float)progress, 0.000001f,
                "peaceful unification transfers all joining debt");

            double updatedDebt;
            double remainder;
            True(MilitaryMath.TryDivertRepairDebt(
                -5d, 3d, out updatedDebt, out remainder),
                "Military investment can partially repay repair debt");
            Near(-2f, (float)updatedDebt, 0.000001f,
                "partial diversion leaves the unpaid repair debt");
            Near(0f, (float)remainder, 0.000001f,
                "partial repayment consumes the entire investment");
            True(MilitaryMath.TryDivertRepairDebt(
                -3d, 5d, out updatedDebt, out remainder),
                "Navy or Nuclear Weapons investment can overshoot repair debt");
            Near(0f, (float)updatedDebt, 0.000001f,
                "overshoot clears repair debt exactly");
            Near(2f, (float)remainder, 0.000001f,
                "overshoot preserves investment for the selected priority");
            True(MilitaryMath.TryDivertRepairDebt(
                4d, 3d, out updatedDebt, out remainder),
                "investment without repair debt remains unchanged");
            Near(4f, (float)updatedDebt, 0.000001f,
                "positive Build Army progress is not redirected");
            Near(3f, (float)remainder, 0.000001f,
                "no-debt investment remains available to its priority");
            Near(5f, (float)MilitaryMath.RepairDebtAmount(-5d), 0.000001f,
                "negative Build Army progress exposes direct-investment debt capacity");
            Near(0f, (float)MilitaryMath.RepairDebtAmount(2d), 0.000001f,
                "positive Build Army progress exposes no repair debt");
            True(!MilitaryMath.TryDivertRepairDebt(
                -3d, -1d, out updatedDebt, out remainder),
                "negative adjustments are rejected by debt diversion math");

            double previous = -1d;
            for (int difference = -3; difference <= 3; difference++)
            {
                double chance = LandCombatMath.HitChance(difference, 0d, 2d);
                True(chance > previous, "hit curve monotonic at " + difference);
                previous = chance;
                double opposite = LandCombatMath.HitChance(-difference, 0d, 2d);
                True(Math.Abs(opposite - (1d - chance)) < 1e-12d,
                    "hit curve symmetry at " + difference);
            }
            Near(0.25f, (float)LandCombatMath.HitChance(-1d, 0d, 2d),
                0.000001f, "minus one hit chance");
            Near(0.5f, (float)LandCombatMath.HitChance(0d, 0d, 2d),
                0.000001f, "equal-rating hit chance");
            Near(0.75f, (float)LandCombatMath.HitChance(1d, 0d, 2d),
                0.000001f, "plus one hit chance");
            True(MilitaryMath.IsFinite(LandCombatMath.HitChance(1000d, 0d, 2d)),
                "positive extreme hit chance finite");
            True(MilitaryMath.IsFinite(LandCombatMath.HitChance(-1000d, 0d, 2d)),
                "negative extreme hit chance finite");
            Near(5f, (float)LandCombatMath.RatingAfterStrength(5d, 1d, 1d),
                0.000001f, "full-strength rating");
            Near(4.5f, (float)LandCombatMath.RatingAfterStrength(5d, 0.5d, 1d),
                0.000001f, "half-strength additive penalty");
            Near(4f, (float)LandCombatMath.RatingAfterStrength(5d, 0d, 1d),
                0.000001f, "zero-strength additive penalty");
            Near(0.15f, (float)LandCombatMath.ScaleContribution(0.30d, 0.5d),
                0.000001f, "adviser Command contribution halved");
            Near(0.10f, (float)LandCombatMath.ScaleContribution(0.20d, 0.5d),
                0.000001f, "LEO combat contribution halved");
            Near(0.10f, (float)LandCombatMath.ScaleContribution(0.20d, 0.5d),
                0.000001f, "own-region contribution halved");
            Near(0.10f, (float)LandCombatMath.ScaleContribution(0.20d, 0.5d),
                0.000001f, "rugged own-region contribution halved");
            Near(0.05f, (float)LandCombatMath.ScaleContribution(0.10d, 0.5d),
                0.000001f, "core-economic contribution halved");
            Near(-0.10f, (float)LandCombatMath.ScaleContribution(-0.20d, 0.5d),
                0.000001f, "crackdown penalty halved");
            Near(0.125f, (float)LandCombatMath.ScaleContribution(0.25d, 0.5d),
                0.000001f, "friendly cohesion contribution halved");
            Near(0.20f, (float)LandCombatMath.ScaleContribution(0.40d, 0.5d),
                0.000001f, "rugged project contribution halved");
            Near(0.30f, (float)LandCombatMath.ScaleContribution(0.60d, 0.5d),
                0.000001f, "urban project contribution halved");

            double noLowArmies;
            True(MilitaryMath.TrySolveConservedTechnology(
                3d, 0, 5d, 4, 5d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out noLowArmies),
                "no-lower-army conservation solves");
            Near(5f, (float)noLowArmies, 0.000001f,
                "no lower armies preserve high tech");

            double balanced;
            True(MilitaryMath.TrySolveConservedTechnology(
                3d, 4, 5d, 4, 5d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out balanced),
                "balanced conservation solves");
            double moreLow;
            MilitaryMath.TrySolveConservedTechnology(
                3d, 12, 5d, 4, 5d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out moreLow);
            double moreHigh;
            MilitaryMath.TrySolveConservedTechnology(
                3d, 4, 5d, 12, 5d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out moreHigh);
            True(moreLow < balanced, "more lower-tech armies lower merger tech");
            True(moreHigh > balanced, "more higher-tech armies raise merger tech");

            double reversed;
            MilitaryMath.TrySolveConservedTechnology(
                5d, 4, 3d, 4, 5d, armyCoefficient, armyGrowthBase,
                doctrineBaseCost, doctrineGrowthBase, catchUpCoefficient,
                out reversed);
            True(Math.Abs(reversed - balanced) < 1e-10d,
                "absorption direction does not change merger tech");

            double residual =
                MilitaryMath.DoctrineCost(
                    balanced, 5d, 5d, doctrineBaseCost, doctrineGrowthBase,
                    catchUpCoefficient) +
                MilitaryMath.ArmyUpgradeCost(
                balanced, 5d, 4, armyCoefficient, armyGrowthBase) -
                MilitaryMath.ArmyUpgradeCost(
                3d, balanced, 4, armyCoefficient, armyGrowthBase);
            True(Math.Abs(residual) < 1e-7d,
                "merger conservation residual within tolerance");
            True(!MilitaryMath.ResolveDestroyArmies(
                true, true, true, false),
                "peaceful unification preserves armies");
            True(MilitaryMath.ResolveDestroyArmies(
                false, false, true, false),
                "human conquest absorption destroys armies");
            True(!MilitaryMath.ResolveDestroyArmies(
                true, false, true, true),
                "Alien Nation territorial transfer preserves human armies");
            True(!MilitaryMath.ResolveDestroyArmies(
                false, false, false, false),
                "unrelated preserving transfer remains vanilla");
            True(MilitaryMath.ResolveDestroyArmies(
                true, false, false, false),
                "unrelated destructive transfer remains vanilla");
        }

        private static void TestEconomyAndTechnology()
        {
            Reset();
            TINationState nation = Nation();
            TIEconomyMod.Main.settings.abundance.enabled = false;
            TIEconomyMod.Main.settings.technology.enabled = false;
            float result = 0f;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float core = 1f + 1.20f * 3f / 5f;
            float referenceCore = 1f + 1.20f / 3f;
            float laborSupport = core * 2.2f * 1.3f * 1.2f /
                (referenceCore * 2.05f * 1.3f * 1.2f);
            float laborPressure = (20000f / 37500f) / laborSupport;
            float resourcePressure = 20000f / 55000f;
            float laborConstraint = 0.35f + 0.65f /
                (1f + (float)Math.Pow(laborPressure, 1.4f));
            float resourceConstraint = 0.45f + 0.55f /
                (1f + (float)Math.Pow(resourcePressure, 1.2f));
            float totalBillions = laborConstraint * resourceConstraint;
            Near(totalBillions * 1000000000f / nation.population, result, 0.0001f,
                "exact factor-balance formula with technology and abundance disabled");

            nation.numCoreEconomicRegions_dailyCache = 1;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float oneCore = result;
            nation.numCoreEconomicRegions_dailyCache = 2;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float twoCores = result;
            nation.numCoreEconomicRegions_dailyCache = 3;
            EconomyGrowthPatch.Prefix(ref result, nation);
            float threeCores = result;
            True(oneCore < twoCores && twoCores < threeCores,
                "smooth core curve raises labor support monotonically");
            True(threeCores - twoCores < twoCores - oneCore,
                "smooth core curve has diminishing returns");

            TIEconomyMod.Main.settings.technology.enabled = true;
            GameStateManager.Research.finishedTechsNames.Add("MissionToSpace");
            GameStateManager.Research.finishedTechsNames.Add("AdvancedChemicalRocketry");
            EconomyGrowthPatch.Prefix(ref result, nation);
            Near(totalBillions * 1.0201f * 1000000000f / nation.population,
                result, 0.0001f, "starting technologies compound to 1.0201 without progress");

            TIEconomyMod.Main.settings.technology.maximumMultiplier = 1.01f;
            EconomyGrowthPatch.Prefix(ref result, nation);
            Near(totalBillions * 1.01f * 1000000000f / nation.population,
                result, 0.0001f, "technology cap");

            Reset();
            nation = Nation();
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float modernGain = AggregateEconomyGainBillions(nation);
            nation.perCapitaGDP *= 2f;
            float capitalOnlyGain = AggregateEconomyGainBillions(nation);
            True(capitalOnlyGain < modernGain,
                "doubling capital alone lowers the return to each Economy IP");

            nation = Nation();
            float originalScaleGain = AggregateEconomyGainBillions(nation);
            nation.GDP *= 2d;
            nation.population *= 2f;
            nation.population_Millions *= 2f;
            nation.currentResourceRegions *= 2;
            float doubledScaleGain = AggregateEconomyGainBillions(nation);
            Near(originalScaleGain, doubledScaleGain, 0.000001f,
                "identical-country factor doubling is neutral per IP");

            nation = Nation();
            float normalLabor = AggregateEconomyGainBillions(nation);
            nation.education = 2f;
            nation.democracy = 1f;
            float scarceLabor = AggregateEconomyGainBillions(nation);
            True(scarceLabor < normalLabor,
                "weak education and institutions create a labor bottleneck");

            nation = Nation();
            TIEconomyMod.Main.settings.abundance.enabled = true;
            nation.perCapitaGDP = 100000f;
            nation.currentResourceRegions = 0;
            nation.populationDesnity_pop_km2 = 1000f;
            float scarceResources = AggregateEconomyGainBillions(nation);
            nation.currentResourceRegions = 5;
            nation.populationDesnity_pop_km2 = 5f;
            float abundantResources = AggregateEconomyGainBillions(nation);
            True(abundantResources > scarceResources,
                "resources and land relieve a high-capital resource bottleneck");

            foreach (string technologyId in TIEconomyMod.Main.techWeights.TechnologyIds)
            {
                GameStateManager.Research.finishedTechsNames.Add(technologyId);
            }
            TIEconomyMod.Main.settings.abundance.enabled = false;
            nation = Nation();
            nation.perCapitaGDP = 10000f;
            float fullTreeLowCapital = AggregateEconomyGainBillions(nation);
            nation.perCapitaGDP = 1000000f;
            float fullTreeHighCapital = AggregateEconomyGainBillions(nation);
            Near(fullTreeLowCapital, fullTreeHighCapital, 0.00001f,
                "full substitution makes capital returns independent of GDP per capita");
            Near(3.40f, fullTreeLowCapital, 0.00001f,
                "full tree produces the normalized 3.40x productivity result");
        }

        private static void TestIndependentResearch()
        {
            Near(75f, IndependentResearchMath.MonthlyNeutralShare(100f, 3, 4),
                0.0001f, "neutral Control Point shares conserve national research");
            Near(100f, IndependentResearchMath.MonthlyNeutralShare(100f, 9, 4),
                0.0001f, "neutral Control Point count is bounded by the nation total");
            Near(0f, IndependentResearchMath.MonthlyNeutralShare(100f, 0, 4),
                0.0001f, "fully owned nations add no independent research");
            Near(0f, IndependentResearchMath.MonthlyNeutralShare(-10f, 2, 4),
                0.0001f, "negative national research cannot reduce global progress");
            True(IndependentResearchMath.IsNeutralResearchControlPoint(false, false),
                "an unowned Control Point contributes neutral research");
            True(!IndependentResearchMath.IsNeutralResearchControlPoint(true, false),
                "an ordinarily owned Control Point is not double counted");
            True(IndependentResearchMath.IsNeutralResearchControlPoint(true, true),
                "a cracked-down Control Point contributes neutral research because its owner receives none");
            True(IndependentResearchMath.NeutralControlPointCount(4, 1) == 3,
                "neutral shares are the exact complement of faction-allocatable shares");
            True(IndependentResearchMath.NeutralControlPointCount(4, 9) == 0,
                "invalid excess faction allocation cannot create negative neutral shares");
            Near(100f,
                IndependentResearchMath.MonthlyNeutralShare(100f, 1, 4) +
                IndependentResearchMath.MonthlyNeutralShare(100f, 3, 4),
                0.0001f,
                "faction-allocatable and neutral shares partition national research exactly once");

            float daily = IndependentResearchMath.DailyPerGlobalTechnology(
                IndependentResearchMath.DaysPerYear / 12f * 3f);
            Near(1f, daily, 0.0001f,
                "monthly independent research divides evenly among three daily slots");
            Near(40f, IndependentResearchMath.IndependentProgress(100f, 60f),
                0.0001f, "independent progress is the unattributed remainder");
            Near(0f, IndependentResearchMath.IndependentProgress(60f, 100f),
                0.0001f, "rounding cannot produce negative independent progress");

            Near(999.999f, IndependentResearchMath.GuardUnattributedCompletion(
                1010f, 1000f, false), 0.0001f,
                "unattributed research waits below completion for a faction");
            Near(1010f, IndependentResearchMath.GuardUnattributedCompletion(
                1010f, 1000f, true), 0.0001f,
                "a real faction contribution permits completion");
        }

        private static void TestBalanceTuning()
        {
            Reset();
            Near(1f, (float)AlienFloraAssaultMath.DamageScale(100d, 100d),
                0.0001f, "mature alien flora retains full assault damage");
            Near(0.3f, (float)AlienFloraAssaultMath.DamageScale(30d, 100d),
                0.0001f, "light alien flora deals proportionally less damage");
            Near(0.008f, (float)AlienFloraAssaultMath.ScaledDamage(
                0.08d, 10d, 100d), 0.0001f,
                "flora level scales the completed vanilla damage roll");
            Near(0f, (float)AlienFloraAssaultMath.ScaledDamage(
                0.08d, -5d, 100d), 0.0001f,
                "negative flora state cannot produce negative damage");
            True(double.IsNaN(AlienFloraAssaultMath.ScaledDamage(
                0.08d, 10d, 0d)),
                "invalid full-damage level is rejected");

            float technologyCost = 1000f;
            GlobalTechnologyResearchCostPatch.Postfix(ref technologyCost);
            Near(2000f, technologyCost, 0.0001f,
                "global technology costs double");

            float projectCost = 1000f;
            FactionProjectResearchCostPatch.Postfix(ref projectCost);
            Near(1400f, projectCost, 0.0001f,
                "faction project costs increase by forty percent");

            float xenofaunaTech = 6f;
            TIMegafaunaArmyState xenofauna = new TIMegafaunaArmyState();
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(5f, xenofaunaTech, 0.0001f, "xenofauna natural ceiling");
            xenofauna.bonusTechLevel = 0.4f;
            xenofaunaTech = 6.4f;
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(5.4f, xenofaunaTech, 0.0001f,
                "xenofauna keeps post-control bonuses");

            Near(0.002364214f,
                PowerPlantThermalMath.WasteHeatFromUsefulPower_GW(
                    0.0055165f, 0.70f),
                0.0000001f,
                "power-plant heat is input power minus delivered power");
            Near(3f,
                PowerPlantThermalMath.PlantWasteHeat_GW(
                    false, 4f, 2f, 2f / 3f, 0.01f),
                0.0001f,
                "closed-cycle drive load contributes to plant waste heat");
            Near(1.02f,
                PowerPlantThermalMath.PlantWasteHeat_GW(
                    true, 4f, 2f, 2f / 3f, 0.01f),
                0.0001f,
                "open-cycle drive retains one percent of drive-associated heat");
            Near(1f,
                PowerPlantThermalMath.PlantWasteHeat_GW(
                    true, 4f, 2f, 2f / 3f, 0f),
                0.0001f,
                "zero open-cycle coefficient reproduces the vanilla exemption");
            float gunInput_GJ = WeaponPowerMath.ElectricalInput_GJ(
                8.7f, 0f, 0.9f);
            Near(0.009666667f, gunInput_GJ, 0.000000001f,
                "gun useful work is divided by module efficiency");
            float gunHeat_GJ = WeaponPowerMath.ModuleWasteHeat_GJ(
                gunInput_GJ, 0.9f);
            Near(0.000966667f, gunHeat_GJ, 0.000000001f,
                "gun module heat is electrical input minus useful work");
            Near(0.001288889f,
                WeaponPowerMath.DesignHeatRate_GW(
                    gunHeat_GJ, 6, 4f, 0.75f),
                0.000000001f,
                "salvo radiator heat follows the design burst interval");
            Near(0.000193333f,
                WeaponPowerMath.DesignHeatRate_GW(
                    gunHeat_GJ, 1, 5f, 0f),
                0.000000001f,
                "single-shot radiator heat follows ordinary cooldown");
            Near(0f,
                WeaponPowerMath.ModuleWasteHeat_GJ(0.002f, 1f),
                0f,
                "perfectly efficient modules add no local weapon heat");
            Near(0.05067075f,
                ProjectileCollisionMath.CrossSectionalArea_m2(254f),
                0.00000001f,
                "10-inch projectile cross-section follows physical diameter");
            Near(0.000706858f,
                ProjectileCollisionMath.CrossSectionalArea_m2(30f),
                0.00000001f,
                "30mm projectile cross-section follows physical diameter");
            Near(0.00254f,
                ProjectileCollisionMath.WorldDiameter_gameUnits(254f, 0.01f),
                0.00000001f,
                "projectile collider uses the ship-model cinematic scale");
            Near(40.40647f,
                ProjectileCollisionMath.MagneticProjectileDiameter_mm(10f),
                0.0001f,
                "10 kg magnetic projectile uses tungsten-equivalent 10-to-1 geometry");
            True(ProjectileCollisionMath.MagneticProjectileDiameter_mm(14f) >
                    ProjectileCollisionMath.MagneticProjectileDiameter_mm(12f),
                "increased magnetic projectile mass increases physical diameter");
            Near(0f,
                ProjectileCollisionMath.MagneticProjectileDiameter_mm(-1f),
                0f,
                "invalid magnetic projectile mass cannot create a collider");
            Near(90f,
                ProjectileCollisionMath.MassDamage_kg(0.75f, 0.15f),
                0.0001f,
                "direct and chipping damage share projectile durability");
            True(ProjectileCollisionMath.MassDamage_kg(
                    0.6615f * 0.8f, 0f) < 90f,
                "minimum current 30mm head-on damage does not erase a 10-inch projectile");
            True(ProjectileCollisionMath.MassDamage_kg(
                    4.41f * 0.8f, 0f) >= 90f,
                "minimum current 6-inch head-on damage erases a 10-inch projectile");
            True(ProjectileCollisionMath.MassDamage_kg(
                    0.33075f * 0.8f, 0f) < 180f,
                "planned lighter 30mm projectile does not erase a heavier 10-inch projectile");
            True(ProjectileCollisionMath.MassDamage_kg(
                    7.84f * 0.8f, 0f) >= 180f,
                "planned 6-inch projectile still erases a heavier 10-inch projectile");
            Near(0f,
                ProjectileCollisionMath.MassDamage_kg(-1f, 0f),
                0f,
                "malformed negative damage cannot repair a projectile");
            Near(1f,
                ProjectileCollisionMath.MovementSweepMultiplier,
                0f,
                "ballistic movement sweep has no anticipatory margin");
            Near(6f,
                DirectFireCommitmentMath.EstimatedKillThreshold_points(
                    1, 0f),
                0f,
                "unarmored direct-fire saturation mirrors the vanilla missile threshold");
            Near(12f,
                DirectFireCommitmentMath.EstimatedKillThreshold_points(
                    1, 20f),
                0f,
                "armor scales direct-fire saturation like vanilla missile saturation");
            Near(0f,
                DirectFireCommitmentMath.EstimatedKillThreshold_points(
                    -1, -20f),
                0f,
                "malformed negative target durability cannot create a negative threshold");
            True(!DirectFireCommitmentMath.IsSaturated(6f, 6f),
                "commitment equal to the threshold preserves vanilla strict comparison");
            True(DirectFireCommitmentMath.IsSaturated(6.01f, 6f),
                "commitment above the threshold saturates automatic direct fire");
            True(DirectFireCommitmentMath.IsAutomaticCandidateAvailable(
                    false, true),
                "unsaturated direct-fire candidates remain eligible");
            True(!DirectFireCommitmentMath.IsAutomaticCandidateAvailable(
                    true, true),
                "saturated candidates receive a priority malus while an unsaturated target exists");
            True(DirectFireCommitmentMath.IsAutomaticCandidateAvailable(
                    true, false),
                "all-saturated fleets retain every candidate for vanilla priority ordering");
            True(DirectFireCommitmentMath.ShouldSuppressSaturatedTarget(
                    true, true),
                "automatic fire pauses on a saturated target when an unsaturated alternative exists");
            True(!DirectFireCommitmentMath.ShouldSuppressSaturatedTarget(
                    true, false),
                "automatic fire continues when every eligible target is saturated");
            Near(0f,
                DirectFireCommitmentMath.SanitizeExpectedDamage_points(
                    float.NaN),
                0f,
                "invalid expected projectile damage cannot saturate a target");
            Near(9f, ShipBalanceMath.CrewMass_tons(3, 3f), 0f,
                "settled crew support mass is three tonnes per billet");
            float catalogDeLaval;
            float catalogMagnetic;
            True(TIEconomyMod.Main.hullDriveScales.TryGetScales(
                    "Cruiser", 0, out catalogDeLaval, out catalogMagnetic),
                "Cruiser appearance 0 has measured drive-art scales");
            Near(4.150085f, catalogDeLaval, 0.000001f,
                "default Cruiser De Laval uses measured art scale");
            Near(2.859339f, catalogMagnetic, 0.000001f,
                "default Cruiser magnetic uses measured art scale");
            True(TIEconomyMod.Main.hullDriveScales.TryGetScales(
                    "Gunship", 2, out catalogDeLaval, out catalogMagnetic),
                "premium Gunship art has measured drive-art scales");
            Near(0.397033f, catalogMagnetic, 0.000001f,
                "measured human art can scale below the Gunship reference");
            string humanDriveDiagnostic;
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "Cruiser", false, 2, "Magnetic",
                    out humanDriveDiagnostic),
                0f, "human scale lookup without its catalog uses vanilla");
            True(!string.IsNullOrEmpty(humanDriveDiagnostic),
                "catalog-free human lookup reports a diagnostic");
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "Cruiser", false, 0, "Pulsed",
                    out humanDriveDiagnostic),
                0f, "pulsed drive remains a fixed-size appendage");
            True(string.IsNullOrEmpty(humanDriveDiagnostic),
                "intentional pulse-drive policy is not an error");
            Near(3.97033f,
                ShipBalanceMath.ScaledDriveValue(10f, 0.397033f),
                0.00001f,
                "drive values honor measured scales below one");
            Near(-6.02967f,
                ShipBalanceMath.AdditionalScaledDriveValue(
                    10f, 0.397033f),
                0.00001f,
                "smaller drive art removes the unused hardware mass");
            float flatVariantMass;
            True(ShipBalanceMath.TryGetVariantEmptyHullMass_tons(
                    "Cruiser", 1, out flatVariantMass),
                "Cruiser appearance 1 has an authored empty-hull mass");
            Near(1788f, flatVariantMass, 0f,
                "Cruiser appearance 1 uses its flat empty-hull mass");
            string[] humanVariantHulls =
            {
                "Gunship", "Escort", "Corvette", "Frigate",
                "Monitor", "Destroyer", "Cruiser", "Battlecruiser",
                "Lancer", "Battleship", "Dreadnought", "Titan"
            };
            float[,] humanVariantMasses =
            {
                { 171f, 187f, 174f, 205f },
                { 338f, 375f, 345f, 406f },
                { 385f, 599f, 708f, 677f },
                { 576f, 633f, 802f, 891f },
                { 679f, 980f, 1622f, 1595f },
                { 873f, 1730f, 1858f, 2055f },
                { 964f, 1788f, 1549f, 2286f },
                { 1170f, 2460f, 1900f, 3024f },
                { 1958f, 2472f, 3848f, 3865f },
                { 1558f, 1961f, 1854f, 2251f },
                { 2346f, 2906f, 2521f, 3559f },
                { 3143f, 4208f, 3408f, 5089f }
            };
            for (int hullIndex = 0;
                hullIndex < humanVariantHulls.Length;
                hullIndex++)
            {
                for (int appearanceIndex = 0;
                    appearanceIndex < 4;
                    appearanceIndex++)
                {
                    True(ShipBalanceMath.TryGetVariantEmptyHullMass_tons(
                            humanVariantHulls[hullIndex],
                            appearanceIndex,
                            out flatVariantMass),
                        humanVariantHulls[hullIndex] + " appearance " +
                            appearanceIndex + " has a flat hull mass");
                    Near(humanVariantMasses[hullIndex, appearanceIndex],
                        flatVariantMass,
                        0f,
                        humanVariantHulls[hullIndex] + " appearance " +
                            appearanceIndex + " flat mass is locked");
                }
            }
            True(!ShipBalanceMath.TryGetVariantEmptyHullMass_tons(
                    "Cruiser", 4, out flatVariantMass),
                "unknown human appearance has no silent mass fallback");
            Near(7.531f,
                ShipBalanceMath.DriveScale(
                    "AlienTitan", true, 0, "DeLaval"),
                0f, "alien Titan uses its hull-specific visual factor");
            Near(26.216f,
                ShipBalanceMath.DriveScale(
                    "AlienMothership", true, 0, "Magnetic"),
                0f, "alien factors are independent of nozzle physics");
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "AlienCorvette", true, 0, "Magnetic"),
                0f, "alien Corvette never scales below baseline");
            string driveDiagnostic;
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "FutureAlienHull", true, 0, "Magnetic",
                    out driveDiagnostic),
                0f, "unknown alien hull uses safe baseline");
            True(!string.IsNullOrEmpty(driveDiagnostic),
                "unknown alien hull reports a drive-scale diagnostic");
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "AlienTitan", true, 2, "Magnetic",
                    out driveDiagnostic),
                0f, "unknown alien appearance uses safe baseline");
            True(!string.IsNullOrEmpty(driveDiagnostic),
                "unknown alien appearance reports a drive-scale diagnostic");
            Near(1f,
                ShipBalanceMath.DriveScale(
                    "AlienCorvette", true, 0, "Magnetic",
                    out driveDiagnostic),
                0f, "known clamped alien factor remains baseline");
            True(string.IsNullOrEmpty(driveDiagnostic),
                "intentional alien Corvette clamp is not an error");
            string[] alienDriveHulls =
            {
                "AlienGunship", "AlienEscort", "AlienCorvette",
                "AlienFrigate", "AlienMonitor", "AlienDestroyer",
                "AlienCruiser", "AlienBattlecruiser", "AlienLancer",
                "AlienBattleship", "AlienDreadnought", "AlienTitan",
                "AlienAssaultCarrier", "AlienMothership",
                "SalamanderGunship"
            };
            float[] alienDriveScales =
            {
                1f, 1f, 1f, 1.144f, 2.202f, 3.384f, 3.291f,
                3.445f, 3.291f, 3.445f, 3.445f, 7.531f, 3.445f,
                26.216f, 1f
            };
            for (int alienHullIndex = 0;
                alienHullIndex < alienDriveHulls.Length;
                alienHullIndex++)
            {
                Near(
                    alienDriveScales[alienHullIndex],
                    ShipBalanceMath.DriveScale(
                        alienDriveHulls[alienHullIndex],
                        true,
                        0,
                        "DeLaval"),
                    0f,
                    alienDriveHulls[alienHullIndex] +
                        " locks its measured graphical drive scale");
            }

            string[] reactorBayHulls =
            {
                "Gunship", "Escort", "Corvette", "Frigate", "Monitor",
                "Destroyer", "Cruiser", "Battlecruiser", "Lancer",
                "Battleship", "Dreadnought", "Titan"
            };
            float[,] reactorBayVolumes =
            {
                { 264.240616f, 452.197326f, 317.310118f, 712.241612f },
                { 264.240558f, 452.197326f, 317.310118f, 712.241612f },
                { 264.240616f, 452.197235f, 604.707011f, 837.587811f },
                { 332.341240f, 675.443739f, 1246.492028f, 1233.527032f },
                { 384.582064f, 675.443717f, 2617.607109f, 2028.674504f },
                { 384.582064f, 675.443717f, 2617.606700f, 2028.674504f },
                { 1989.241734f, 1384.983819f, 3930.637720f, 3505.550347f },
                { 1989.242548f, 1384.983819f, 3930.637720f, 3505.550347f },
                { 2365.773019f, 2090.292333f, 10223.879025f, 8072.643840f },
                { 5648.074162f, 2090.291983f, 5464.773080f, 6945.700026f },
                { 11476.330412f, 2090.293033f, 10223.879025f, 10952.622272f },
                { 15955.575747f, 6290.836709f, 16549.539439f, 15840.889300f }
            };
            for (int hullIndex = 0; hullIndex < reactorBayHulls.Length;
                hullIndex++)
            {
                for (int appearanceIndex = 0; appearanceIndex < 4;
                    appearanceIndex++)
                {
                    float measuredVolume;
                    True(ShipBalanceMath.TryGetMeasuredReactorBayVolume_m3(
                            reactorBayHulls[hullIndex], appearanceIndex,
                            out measuredVolume),
                        reactorBayHulls[hullIndex] + " appearance " +
                        appearanceIndex + " has a maintained bay measurement");
                    Near(reactorBayVolumes[hullIndex, appearanceIndex],
                        measuredVolume, 0.002f,
                        reactorBayHulls[hullIndex] + " appearance " +
                        appearanceIndex + " locks measured reactor-bay volume");
                }
            }
            float absentVolume;
            True(!ShipBalanceMath.TryGetMeasuredReactorBayVolume_m3(
                    "FutureHull", 0, out absentVolume),
                "unknown hulls do not masquerade as measured variants");
            True(!ShipBalanceMath.TryGetMeasuredReactorBayVolume_m3(
                    "Gunship", 4, out absentVolume),
                "unknown appearances do not masquerade as measured variants");

            bool usedBayFallback;
            string baySizeBand;
            Near(2617.607109f, ShipBalanceMath.ReactorBayVolume_m3(
                    "FutureSmall", 9, true, false, false, false,
                    out usedBayFallback, out baySizeBand), 0.002f,
                "unknown small hull uses the maximum measured small fallback");
            True(usedBayFallback && baySizeBand == "Small",
                "small fallback reports its diagnostic size band");
            Near(3930.637720f, ShipBalanceMath.ReactorBayVolume_m3(
                    "FutureMedium", 9, false, true, false, false,
                    out usedBayFallback, out baySizeBand), 0.002f,
                "unknown medium hull uses the maximum measured medium fallback");
            Near(16549.539439f, ShipBalanceMath.ReactorBayVolume_m3(
                    "FutureLarge", 9, false, false, true, false,
                    out usedBayFallback, out baySizeBand), 0.002f,
                "unknown large hull uses the maximum measured large fallback");
            Near(16549.539439f, ShipBalanceMath.ReactorBayVolume_m3(
                    "FutureHuge", 9, false, false, false, true,
                    out usedBayFallback, out baySizeBand), 0.002f,
                "unknown huge hull uses the maximum measured huge fallback");

            string[] reactorClasses =
            {
                "Fuel_Cell", "Solid_Core_Fission", "Molten_Salt_Core_Fission",
                "Liquid_Core_Fission", "Gas_Core_Fission",
                "Electrostatic_Confinement_Fusion",
                "Mirrored_Magnetic_Confinement_Fusion",
                "Any_Magnetic_Confinement_Fusion",
                "Toroid_Magnetic_Confinement_Fusion",
                "Hybrid_Confinement_Fusion", "Z_Pinch_Fusion",
                "Inertial_Confinement_Fusion", "Antimatter_Plasma_Core",
                "Antimatter_Beam_Core", "Antimatter_Solid_Core",
                "Antimatter_Gas_Core", "Future_Reactor_Class"
            };
            float[] reactorDensities =
            {
                1.2f, 2.5f, 3.5f, 2.5f, 2f, 1f, 1.2f, 2f, 2f, 2f,
                2.5f, 1.5f, 2.5f, 3f, 2.5f, 2f, 2f
            };
            float[] reactorBayFractions =
            {
                0.25f, 0.5f, 0.55f, 0.55f, 0.45f, 0.75f, 0.75f,
                0.75f, 0.75f, 0.75f, 0.6f, 0.6f, 0.6f, 0.4f, 0.5f,
                0.45f, 0.75f
            };
            for (int reactorClassIndex = 0;
                reactorClassIndex < reactorClasses.Length;
                reactorClassIndex++)
            {
                Near(reactorDensities[reactorClassIndex],
                    ShipBalanceMath.ReactorInstalledDensity_tonsPerM3(
                        reactorClasses[reactorClassIndex]), 0f,
                    reactorClasses[reactorClassIndex] +
                        " locks its installed density");
                Near(reactorBayFractions[reactorClassIndex],
                    ShipBalanceMath.ReactorReportedMassBayFraction(
                        reactorClasses[reactorClassIndex]), 0f,
                    reactorClasses[reactorClassIndex] +
                        " locks its reactor-bay mass fraction");
            }

            float moltenSaltGunship0 = ShipBalanceMath.ReactorBayOutputLimit_GW(
                264.240616f, "Molten_Salt_Core_Fission", 8f, 400f);
            float moltenSaltGunship1 = ShipBalanceMath.ReactorBayOutputLimit_GW(
                452.197326f, "Molten_Salt_Core_Fission", 8f, 400f);
            float moltenSaltGunship2 = ShipBalanceMath.ReactorBayOutputLimit_GW(
                317.310118f, "Molten_Salt_Core_Fission", 8f, 400f);
            float moltenSaltGunship3 = ShipBalanceMath.EffectiveReactorOutput_GW(
                400f, ShipBalanceMath.ReactorBayOutputLimit_GW(
                    712.241612f, "Molten_Salt_Core_Fission", 8f, 400f));
            True(moltenSaltGunship0 >= 3f * 65.8824f &&
                    moltenSaltGunship0 < 4f * 65.8824f,
                "Molten Salt II Gunship appearance 0 fits Pegasus x3 only");
            True(moltenSaltGunship1 >= 5f * 65.8824f &&
                    moltenSaltGunship1 < 6f * 65.8824f,
                "Molten Salt II Gunship appearance 1 fits Pegasus x5 only");
            True(moltenSaltGunship2 >= 3f * 65.8824f &&
                    moltenSaltGunship2 < 4f * 65.8824f,
                "Molten Salt II Gunship appearance 2 fits Pegasus x3 only");
            True(moltenSaltGunship3 >= 6f * 65.8824f,
                "Molten Salt II Gunship appearance 3 reaches Pegasus x6");
            Near(400f, moltenSaltGunship3, 0.001f,
                "large Molten Salt II bay remains capped by reactor rating");
            float moltenSaltFrigate0 =
                ShipBalanceMath.ReactorBayOutputLimit_GW(
                    332.341240f, "Molten_Salt_Core_Fission", 8f, 400f);
            True(moltenSaltFrigate0 >= 4f * 65.8824f &&
                    moltenSaltFrigate0 < 5f * 65.8824f,
                "Molten Salt II Frigate appearance 0 fits Pegasus x4 only");
            foreach (float moltenSaltLargeFrigate in new[]
            {
                675.443739f, 1246.492028f, 1233.527032f
            })
            {
                Near(400f, ShipBalanceMath.EffectiveReactorOutput_GW(
                        400f, ShipBalanceMath.ReactorBayOutputLimit_GW(
                            moltenSaltLargeFrigate,
                            "Molten_Salt_Core_Fission", 8f, 400f)),
                    0.001f,
                    "Molten Salt II Frigate appearances 1-3 fit Pegasus x6");
            }
            float solidVGunship0 = ShipBalanceMath.ReactorBayOutputLimit_GW(
                264.240616f, "Solid_Core_Fission", 32f, 60f);
            float solidVGunship1 = ShipBalanceMath.EffectiveReactorOutput_GW(
                60f, ShipBalanceMath.ReactorBayOutputLimit_GW(
                    452.197326f, "Solid_Core_Fission", 32f, 60f));
            True(solidVGunship0 < 3f * 19.5347f,
                "Solid V Gunship appearance 0 rejects Heavy Dumbo x3 by bay");
            Near(60f, solidVGunship1, 0.001f,
                "Solid V Gunship appearance 1 becomes reactor-rating limited");
            Near(400f, ShipBalanceMath.ReactorBayOutputLimit_GW(
                    100f, "Molten_Salt_Core_Fission", 0f, 400f), 0f,
                "invalid specific mass safely preserves theoretical output");
            Near(248.47075f, ShipBalanceMath.ReactorBayVolumeUsed_m3(
                    3f * 65.8824f, "Molten_Salt_Core_Fission", 8f),
                0.001f,
                "Molten Salt II Pegasus x3 reports occupied bay volume");
            Near(0f, ShipBalanceMath.ReactorBayVolumeUsed_m3(
                    100f, "Molten_Salt_Core_Fission", 0f), 0f,
                "invalid specific mass cannot report occupied bay volume");
            Near(0f, ShipBalanceMath.ReactorBayVolumeUsed_m3(
                    0f, "Molten_Salt_Core_Fission", 8f), 0f,
                "a design without drive demand uses no reactor bay volume");

            Near(9920f, ShipBalanceMath.FuelVolume_m3(
                    11019.2f, 800f, 6, 50f), 0f,
                "fuel volume rounds up after module and crew reservations");
            Near(0f, ShipBalanceMath.FuelVolume_m3(
                    500f, 400f, 3, 50f), 0f,
                "module and crew reservations cannot produce negative fuel volume");
            Near(1001f, ShipBalanceMath.FuelVolume_m3(
                    1000.01f, 0f, 0, 50f), 0f,
                "fractional remaining hull volume rounds upward");
            float hydrogenTankVolume =
                ShipBalanceMath.PropellantTankVolume_m3(100f, 70.85f);
            Near(1411.4326f, hydrogenTankVolume, 0.001f,
                "one hundred tons of liquid hydrogen uses its bulk volume");
            Near(100f, ShipBalanceMath.PropellantTankVolume_m3(
                    100f, 1000f), 0.0001f,
                "water-equivalent propellant preserves one cubic metre per ton");
            True(ShipBalanceMath.MaximumPropellantTanks(
                    9920f, hydrogenTankVolume) == 7,
                "liquid hydrogen capacity floors to complete 100-ton tanks");
            True(ShipBalanceMath.MaximumPropellantTanks(
                    9920f, 100f) == 99,
                "water-equivalent capacity floors to complete 100-ton tanks");
            True(ShipBalanceMath.MaximumPropellantTanks(
                    99.99f, 100f) == 0,
                "partial tank volume does not create a tank");
            Near(0f, ShipBalanceMath.PropellantTankVolume_m3(
                    100f, 0f), 0f,
                "invalid propellant density produces no tank volume");
            True(ShipBalanceMath.MaximumPropellantTanks(
                    float.PositiveInfinity, 100f) == int.MaxValue,
                "unbounded valid fuel volume saturates the tank count");
            Near(69.31472f,
                ShipBalanceMath.DeltaVForPropellantTanks_kps(
                    100f, 1000f, 100f, 10),
                0.0001f,
                "capped AI delta-v uses the rocket equation at the legal tank count");
            Near(0f,
                ShipBalanceMath.DeltaVForPropellantTanks_kps(
                    100f, 1000f, 100f, 0),
                0f,
                "zero legal tanks safely reports zero achievable delta-v");
            Near(0f,
                ShipBalanceMath.DeltaVForPropellantTanks_kps(
                    100f, 0f, 100f, 10),
                0f,
                "invalid dry mass cannot produce an AI delta-v estimate");

            TIPowerPlantTemplate plant = new TIPowerPlantTemplate();
            plant.efficiency = 0.70f;
            float wasteHeat = 0f;
            True(!PowerPlantWasteHeatPatch.Prefix(
                    ref wasteHeat, plant, false, 0f, 0.0055165f),
                "enabled power-plant heat patch replaces vanilla");
            Near(0.002364214f, wasteHeat, 0.0000001f,
                "power-plant heat patch returns corrected radiator load");
            TISpaceShipTemplate ship = new TISpaceShipTemplate();
            ship.crewBillets = 3;
            float crewMass = 0f;
            True(!ShipCrewSupportMassPatch.Prefix(ref crewMass, ship),
                "enabled crew support patch replaces vanilla");
            Near(9f, crewMass, 0f,
                "crew support patch returns three tonnes per billet");
            ship.hullTemplate = new TIShipHullTemplate
            {
                dataName = "Cruiser",
                alien = false,
                mass_tons = 964f
            };
            ship.driveTemplate = new TIDriveTemplate
            {
                mass_tons = 100f,
                powerRequirement_GW = 20f,
                cost = new TIResourcesCost { value = 40f }
            };
            ship.powerPlantTemplate = new TIPowerPlantTemplate
            {
                dataName = "TestSolidCorePlant",
                maxOutput_GW = 90f,
                specificPower_tGW = 1f,
                powerPlantClass = PowerPlantRequirement.Solid_Core_Fission
            };
            float scaledThrust = 10f;
            HullScaledDriveThrustPatch.Postfix(ref scaledThrust, ship);
            Near(41.50085f, scaledThrust, 0.0001f,
                "Cruiser drive thrust uses default De Laval art");
            float scaledPower = 20f;
            HullScaledDrivePowerPatch.Postfix(ref scaledPower, ship);
            Near(83.0017f, scaledPower, 0.0001f,
                "Cruiser drive power scales at constant exhaust velocity");
            float scaledDryMass = 1000f;
            HullScaledDriveMassPatch.Postfix(ref scaledDryMass, ship);
            Near(1315.0085f, scaledDryMass, 0.001f,
                "Cruiser dry mass includes the larger drive hardware");
            TIResourcesCost scaledCost = new TIResourcesCost { value = 400f };
            HullScaledDriveConstructionCostPatch.Postfix(
                ref scaledCost, ship, null);
            Near(526.0034f, scaledCost.value, 0.001f,
                "Cruiser construction cost includes the larger drive");
            bool compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, ship.driveTemplate);
            True(compatible,
                "scaled Cruiser drive remains within a 90 GW plant cap");
            ship.powerPlantTemplate.maxOutput_GW = 80f;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, ship.driveTemplate);
            True(!compatible,
                "scaled De Laval drive power respects the existing plant output cap");
            TIDriveTemplate magneticCandidate = new TIDriveTemplate
            {
                nozzle = Nozzle.Magnetic,
                powerRequirement_GW = 20f
            };
            ship.powerPlantTemplate.maxOutput_GW = 60f;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, magneticCandidate);
            True(compatible,
                "candidate compatibility uses magnetic art rather than the installed nozzle");

            TIShipHullTemplate cruiserHull = ship.hullTemplate;
            TIDriveTemplate cruiserDrive = ship.driveTemplate;
            TIPowerPlantTemplate cruiserPlant = ship.powerPlantTemplate;
            ship.hullAppearanceIndex = 1;
            float variantMass = 1000f;
            HullVariantEmptyMassPatch.Postfix(ref variantMass, ship);
            Near(1824f, variantMass, 0.001f,
                "Cruiser appearance 1 adds its 824-ton structural penalty");

            ship.hullTemplate = new TIShipHullTemplate
            {
                dataName = "Gunship",
                alien = false,
                smallHull = true,
                mass_tons = 171f
            };
            ship.hullAppearanceIndex = 2;
            ship.driveTemplate = new TIDriveTemplate
            {
                nozzle = Nozzle.Magnetic,
                mass_tons = 100f,
                powerRequirement_GW = 20f,
                cost = new TIResourcesCost { value = 40f }
            };
            scaledThrust = 10f;
            HullScaledDriveThrustPatch.Postfix(ref scaledThrust, ship);
            Near(3.97033f, scaledThrust, 0.0001f,
                "small magnetic art reduces runtime thrust below baseline");
            scaledDryMass = 1000f;
            HullScaledDriveMassPatch.Postfix(ref scaledDryMass, ship);
            Near(939.7033f, scaledDryMass, 0.001f,
                "small magnetic art reduces installed drive hardware mass");
            HullVariantEmptyMassPatch.Postfix(ref scaledDryMass, ship);
            Near(942.7033f, scaledDryMass, 0.001f,
                "flat hull mass and scaled drive mass remain independent");
            scaledCost = new TIResourcesCost { value = 400f };
            HullScaledDriveConstructionCostPatch.Postfix(
                ref scaledCost, ship, null);
            Near(375.88132f, scaledCost.value, 0.001f,
                "small magnetic art removes unused drive construction cost");
            ship.driveTemplate.nozzle = Nozzle.Pulsed;
            scaledThrust = 10f;
            HullScaledDriveThrustPatch.Postfix(ref scaledThrust, ship);
            Near(10f, scaledThrust, 0f,
                "pulsed runtime thrust remains exactly vanilla");

            ship.hullTemplate = new TIShipHullTemplate
            {
                dataName = "Gunship",
                alien = false,
                smallHull = true
            };
            ship.powerPlantTemplate = new TIPowerPlantTemplate
            {
                dataName = "MoltenSaltFissionReactorII",
                maxOutput_GW = 400f,
                specificPower_tGW = 8f,
                powerPlantClass = PowerPlantRequirement.Molten_Salt_Core_Fission
            };
            TIDriveTemplate pegasusX3 = new TIDriveTemplate
            {
                powerRequirement_GW = 3f * 65.8824f
            };
            TIDriveTemplate pegasusX4 = new TIDriveTemplate
            {
                powerRequirement_GW = 4f * 65.8824f
            };
            TIDriveTemplate pegasusX5 = new TIDriveTemplate
            {
                powerRequirement_GW = 5f * 65.8824f
            };
            TIDriveTemplate pegasusX6 = new TIDriveTemplate
            {
                powerRequirement_GW = 6f * 65.8824f
            };
            ship.hullAppearanceIndex = 0;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX3);
            True(compatible,
                "Gunship appearance 0 accepts Pegasus x3 with Molten Salt II");
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX4);
            True(!compatible,
                "Gunship appearance 0 rejects Pegasus x4 by reactor bay");
            ship.hullAppearanceIndex = 1;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX3);
            True(compatible,
                "Gunship appearance 1 accepts scaled Pegasus x3");
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX4);
            True(compatible,
                "Gunship appearance 1 accepts scaled Pegasus x4");
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX5);
            True(!compatible,
                "Gunship appearance 1 rejects scaled Pegasus x5 by reactor bay");
            ship.hullAppearanceIndex = 2;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX4);
            True(compatible,
                "smaller premium Gunship De Laval art fits scaled Pegasus x4");
            ship.hullAppearanceIndex = 3;
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX3);
            True(compatible,
                "DLCA Gunship appearance accepts scaled Pegasus x3");
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pegasusX4);
            True(!compatible,
                "DLCA Gunship appearance rejects scaled Pegasus x4");
            TIDriveTemplate pulsedPegasusX6 = new TIDriveTemplate
            {
                nozzle = Nozzle.Pulsed,
                powerRequirement_GW = 6f * 65.8824f
            };
            compatible = true;
            HullScaledDriveCompatibilityPatch.Postfix(
                ref compatible, ship, pulsedPegasusX6);
            True(compatible,
                "pulsed x6 remains vanilla and fits the DLCA reactor bay");

            ship.hullAppearanceIndex = 0;
            ship.driveTemplate = pegasusX4;
            compatible = true;
            HullScaledPowerPlantCompatibilityPatch.Postfix(
                ref compatible, ship, ship.powerPlantTemplate);
            True(!compatible,
                "power-plant selection applies the same appearance-0 bay cap");
            ship.driveTemplate = pegasusX3;
            compatible = true;
            HullScaledPowerPlantCompatibilityPatch.Postfix(
                ref compatible, ship, ship.powerPlantTemplate);
            True(compatible,
                "power-plant selection accepts a demand within the bay cap");
            TIEconomyMod.Main.settings.shipBalance.reactorBayCapacityEnabled =
                false;
            ship.driveTemplate = pegasusX4;
            compatible = true;
            HullScaledPowerPlantCompatibilityPatch.Postfix(
                ref compatible, ship, ship.powerPlantTemplate);
            True(compatible,
                "disabled reactor-bay capacity restores theoretical output");
            TIEconomyMod.Main.settings.shipBalance.reactorBayCapacityEnabled =
                true;
            ship.hullTemplate = cruiserHull;
            ship.hullAppearanceIndex = 0;
            ship.driveTemplate = cruiserDrive;
            ship.powerPlantTemplate = cruiserPlant;

            TISpaceShipState liveShip = new TISpaceShipState { template = ship };
            float liveThrust = 10f;
            HullScaledLiveShipThrustPatch.Postfix(ref liveThrust, liveShip);
            Near(41.50085f, liveThrust, 0.0001f,
                "live ship thrust uses the same measured art factor");
            TIEconomyMod.Main.settings.shipBalance.hullDriveScalingEnabled =
                false;
            scaledThrust = 10f;
            HullScaledDriveThrustPatch.Postfix(ref scaledThrust, ship);
            Near(10f, scaledThrust, 0f,
                "disabled hull drive scaling returns vanilla thrust");
            TIEconomyMod.Main.settings.shipBalance.hullDriveScalingEnabled =
                true;

            TIEconomyMod.Main.settings.technology.researchCostEnabled = false;
            technologyCost = 1000f;
            GlobalTechnologyResearchCostPatch.Postfix(ref technologyCost);
            Near(1000f, technologyCost, 0.0001f,
                "disabled technology-cost adjustment returns vanilla");
            TIEconomyMod.Main.settings.technology.projectResearchCostEnabled = false;
            projectCost = 1000f;
            FactionProjectResearchCostPatch.Postfix(ref projectCost);
            Near(1000f, projectCost, 0.0001f,
                "disabled project-cost adjustment returns vanilla");
            TIEconomyMod.Main.settings.army.megafaunaEnabled = false;
            xenofaunaTech = 6f;
            XenofaunaStrengthPatch.Postfix(ref xenofaunaTech, xenofauna);
            Near(6f, xenofaunaTech, 0.0001f,
                "disabled xenofauna adjustment returns vanilla");
            TIEconomyMod.Main.settings.shipBalance.correctPowerPlantWasteHeat = false;
            wasteHeat = 123f;
            True(PowerPlantWasteHeatPatch.Prefix(
                    ref wasteHeat, plant, false, 0f, 0.0055165f),
                "disabled power-plant heat correction returns vanilla");
            Near(123f, wasteHeat, 0f,
                "disabled power-plant heat correction leaves result untouched");
            TIEconomyMod.Main.settings.shipBalance.crewSupportMassEnabled = false;
            crewMass = 123f;
            True(ShipCrewSupportMassPatch.Prefix(ref crewMass, ship),
                "disabled crew support patch returns vanilla");
            Near(123f, crewMass, 0f,
                "disabled crew support patch leaves result untouched");

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

        private static void TestHabRebalanceMath()
        {
            Near(150f,
                HabRebalanceMath.FullMaterialMass(150f, 1f),
                0.0001f,
                "new module charges its full physical material mass");
            Near(50f,
                HabRebalanceMath.MandatoryTransportMass(150f, 0f),
                0.0001f,
                "stocked construction transports one third");
            Near(5f,
                HabRebalanceMath.MandatoryTransportMass(30f, 5f),
                0.0001f,
                "Earth shortfall counts toward transported minimum");
            Near(0f,
                HabRebalanceMath.MandatoryTransportMass(30f, 15f),
                0.0001f,
                "30/15 case requires no additional factory payload");
            Near(15f,
                HabRebalanceMath.EarthFallbackMass(30f, 15f),
                0.0001f,
                "Earth fallback preserves larger shortfall");
            Near(10f,
                HabRebalanceMath.EarthFallbackMass(30f, 0f),
                0.0001f,
                "Earth fallback manufactures one third when fully stocked");
            Near(50f / 3f,
                HabRebalanceMath.FullMaterialMass(
                    25f,
                    HabRebalanceMath.ConstructionRate(true)),
                0.0001f,
                "upgrades charge two thirds of full physical mass");
            Near(0f,
                HabRebalanceMath.PropellantMass(10f, 0d, 2.11d),
                0f,
                "same-hab manufacturing consumes no freight propellant");
            Near(10f * (float)(Math.Exp(1d / 2.11d) - 1d),
                HabRebalanceMath.PropellantMass(10f, 1d, 2.11d),
                0.0001f,
                "space freight uses the probe rocket equation");
            float priorRocketPropellant = HabRebalanceMath.PropellantMass(
                10f,
                10d,
                2.11d);
            foreach (double rocketExhaustVelocity in new[]
            {
                2.6d,
                3.3d,
                4.44d,
                5.6d,
                10d,
                21.6d
            })
            {
                float improvedRocketPropellant =
                    HabRebalanceMath.PropellantMass(
                        10f,
                        10d,
                        rocketExhaustVelocity);
                True(improvedRocketPropellant < priorRocketPropellant,
                    "each Rocket project EV step reduces freight propellant");
                priorRocketPropellant = improvedRocketPropellant;
            }
            Near(1f,
                HabRebalanceMath.EffectiveExportTier(3, 1),
                0f,
                "dock tier caps factory exports");
            Near(0f,
                HabRebalanceMath.EffectiveExportTier(3, 0),
                0f,
                "undocked factory has no remote export tier");
            Near(45f,
                HabRebalanceMath.LogisticsPairPriority(
                    false,
                    true,
                    true,
                    1f),
                0f,
                "AI prioritizes its first Earth-Moon logistics pair");
            Near(34f,
                HabRebalanceMath.LogisticsPairPriority(
                    false,
                    true,
                    false,
                    1f),
                0f,
                "AI prioritizes its first logistics pair in another system");
            Near(0f,
                HabRebalanceMath.LogisticsPairPriority(
                    true,
                    true,
                    true,
                    1f),
                0f,
                "AI does not duplicate an existing logistics pair");
            Near(0f,
                HabRebalanceMath.LogisticsPairPriority(
                    false,
                    false,
                    true,
                    1f),
                0f,
                "AI only boosts candidates that complete a same-hab pair");
            Near(2f,
                HabRebalanceMath.NormalizeMaterialCost(
                    0.6666666f,
                    0.6666666f,
                    2f),
                0.0001f,
                "material normalization removes JSON float dust");
            Near(2.5f, HabRebalanceMath.RoundCost(2.5000002f), 0f,
                "clean physical mass retains one-decimal Earth Boost");
            Near(1f, HabRebalanceMath.ConstructionRate(false), 0f,
                "new construction rate");
            Near(2f,
                HabRebalanceMath.GeneratorConstructionCostMultiplier(true),
                0f,
                "direct generators represent two plants at construction");
            Near(1f,
                HabRebalanceMath.GeneratorConstructionCostMultiplier(false),
                0f,
                "non-generators retain ordinary construction cost");
            True(HabRebalanceMath.HasRebalancedMaterialFraction(0.6666667f),
                "two-thirds material marker");
            True(HabRebalanceMath.HasRebalancedMaterialFraction(0.6f),
                "rounded-mass material marker");
            True(!HabRebalanceMath.HasRebalancedMaterialFraction(0.95f),
                "vanilla alien partial materials are not marked");
            True(HabRebalanceMath.HasEarthDelivery(1f),
                "mandatory equipment adds Earth transfer time");
            True(!HabRebalanceMath.HasEarthDelivery(0f),
                "zero Boost has no Earth transfer time");
            Near(1f,
                HabRebalanceMath.ConnectorTierRequirement(1, true, false, true),
                0f,
                "active human T1 station sector connects");
            Near(2f,
                HabRebalanceMath.ConnectorTierRequirement(1, true, false, false),
                0f,
                "inactive human T1 station sector stays gated");
            Near(2f,
                HabRebalanceMath.ConnectorTierRequirement(1, true, true, true),
                0f,
                "alien T1 station sector stays gated");
            Near(2f,
                HabRebalanceMath.ConnectorTierRequirement(1, false, false, true),
                0f,
                "T1 base sector stays gated");
            Near(2f,
                HabRebalanceMath.ConnectorTierRequirement(2, true, false, true),
                0f,
                "T2 station retains vanilla connector threshold");
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
            True(stableWithAbundance > halfStableWithAbundance &&
                halfStableWithAbundance > halfStableWithoutAbundance,
                "stability continuously gates abundance from zero to full effect");

            Reset();
            nation = Nation();
            nation.currentResourceRegions = 1;
            nation.populationDesnity_pop_km2 = 50f;
            foreach (string technologyId in TIEconomyMod.Main.techWeights.TechnologyIds)
            {
                GameStateManager.Research.finishedTechsNames.Add(technologyId);
            }
            float fullTreeWithAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref fullTreeWithAbundance, nation);
            TIEconomyMod.Main.settings.abundance.enabled = false;
            float fullTreeWithoutAbundance = 0f;
            EconomyGrowthPatch.Prefix(ref fullTreeWithoutAbundance, nation);
            True(fullTreeWithAbundance > fullTreeWithoutAbundance,
                "physical abundance remains beneficial at full technology");

            Reset();
            nation = Nation();
            nation.currentResourceRegions = 0;
            nation.populationDesnity_pop_km2 = 50f;
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
            Near(0.0015f, economyDefault, 0.000001f, "Economy priority Inequality value");
            Near(-0.01333332f, welfareDefault, 0.000001f, "Welfare priority Inequality value");
            Near(0.00666668f, spoilsDefault, 0.000001f, "Spoils priority Inequality value");
            float climateChange = 0.02f;
            ClimateInequalityPatch.Prefix(ref climateChange,
                TINationState.InequalityChangeReason.InqReason_ClimateChange);
            Near(0.08f, climateChange, 0.000001f, "climate Inequality quadruples");
            float annexationChange = 0.02f;
            ClimateInequalityPatch.Prefix(ref annexationChange,
                TINationState.InequalityChangeReason.InqReason_Annexation);
            Near(0.02f, annexationChange, 0f, "non-climate Inequality is unchanged");
            TIEconomyMod.Main.settings.inequality.economyChangeAtReferenceGdp = 0.1f;
            TIEconomyMod.Main.settings.inequality.welfareChangeAtReferenceGdp = -0.1f;
            TIEconomyMod.Main.settings.inequality.spoilsChangeAtReferenceGdp = 0.1f;
            float[] points = { 1f, 3f, 5f, 7f, 9f };
            float[] positive = { 0.3f, 0.15f, 0.1f, 0.075f, 0f };
            float[] negative = { 0f, -0.075f, -0.1f, -0.15f, -0.3f };

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
            nation.GDP = 1000000000000d;
            float trillionEconomy = 0f;
            EconomyInequalityPatch.Prefix(ref trillionEconomy, nation);
            Near(0.013f, trillionEconomy, 0.000001f,
                "Economy Inequality shares the one-trillion resource curve");
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
            nation.education = 0f;
            nation.inequality = 1f;
            True(!CohesionRestInequalityPatch.Prefix(ref result, nation),
                "Cohesion-rest Inequality prefix replaces vanilla");
            Near(2.25f, result, 0.000001f,
                "low Inequality strengthens Cohesion rest state");
            nation.education = 10f;
            nation.inequality = 3f;
            CohesionRestInequalityPatch.Prefix(ref result, nation);
            Near(0f, result, 0f,
                "Inequality 3 is neutral for Cohesion rest state");
            nation.inequality = 5f;
            CohesionRestInequalityPatch.Prefix(ref result, nation);
            Near(-4.5f, result, 0.000001f,
                "higher Inequality penalizes Cohesion rest state");

            nation.democracy = 1f;
            result = -2f;
            CohesionRestPublicElitePatch.Postfix(ref result, nation);
            Near(-0.2f, result, 0.000001f,
                "elite-public divide has one-tenth force at Government 1");
            nation.democracy = 10f;
            result = -2f;
            CohesionRestPublicElitePatch.Postfix(ref result, nation);
            Near(-2f, result, 0.000001f,
                "elite-public divide has full force at Government 10");

            Near(10.5f, TIEconomyMod.Main.settings.cohesionRest.baseValue,
                0f, "Cohesion rest-state base default");
            Near(4f, TIEconomyMod.Main.settings.cohesionRest.autocracyAnocracyBoundary,
                0f, "Autocracy-Anocracy boundary default");
            Near(1.285f, TIEconomyMod.Main.settings.cohesionRest.autocracyExponent,
                0f, "Autocracy Cohesion exponent default");
            Near(6f,
                TIEconomyMod.Main.settings.cohesionRest.anocracyDemocracyBoundary,
                0f, "Anocracy-Democracy boundary default");
            nation.democracy = 1f;
            nation.unrest = 2f;
            result = 99f;
            True(!CohesionRestAutocracyPatch.Prefix(ref result, nation),
                "Autocracy Cohesion prefix replaces vanilla");
            Near((float)((Math.Pow(4f, 1.285f) - Math.Pow(1f, 1.285f)) * 0.8f),
                result, 0.000001f,
                "Autocracy Cohesion uses the four-point boundary formula");
            nation.democracy = 4f;
            CohesionRestAutocracyPatch.Prefix(ref result, nation);
            Near(0f, result, 0f,
                "Autocracy Cohesion ends at Government four");
            True(!CohesionRestAnocracyPatch.Prefix(ref result, nation),
                "Anocracy Cohesion prefix replaces vanilla");
            Near(1f, result, 0f,
                "Anocracy Cohesion starts at Government four");
            nation.democracy = 3.9f;
            CohesionRestAnocracyPatch.Prefix(ref result, nation);
            Near(0f, result, 0f,
                "Anocracy Cohesion is absent below Government four");
            nation.democracy = 6f;
            CohesionRestAnocracyPatch.Prefix(ref result, nation);
            Near(1f, result, 0f,
                "Anocracy Cohesion includes Government six");
            nation.democracy = 5f;
            CohesionRestAnocracyPatch.Prefix(ref result, nation);
            Near(-2f, result, 0f,
                "Anocracy Cohesion reaches negative two at Government five");
            nation.democracy = 6.01f;
            CohesionRestAnocracyPatch.Prefix(ref result, nation);
            Near(0f, result, 0f,
                "Anocracy Cohesion ends above Government six");
            Near(1f,
                TIEconomyMod.Main.settings.cohesionRest.democracyCoefficient,
                0f, "Democracy Cohesion coefficient default");
            nation.democracy = 8f;
            result = 99f;
            True(!CohesionRestDemocracyPatch.Prefix(ref result, nation, 8f),
                "Democracy Cohesion prefix replaces vanilla");
            Near(-2f, result, 0.000001f,
                "Democracy Cohesion uses coefficient times score above six");
            nation.democracy = 10f;
            CohesionRestDemocracyPatch.Prefix(ref result, nation, 5.2f);
            Near(-0.2f, result, 0.000001f,
                "Democracy Cohesion pull does not overshoot five");
            CohesionRestDemocracyPatch.Prefix(ref result, nation, 2f);
            Near(3f, result, 0.000001f,
                "Democracy Cohesion pull reaches but does not cross five");
            nation.democracy = 6f;
            True(!CohesionRestDemocracyPatch.Prefix(ref result, nation, 8f),
                "Democracy Cohesion begins at Government six");
            Near(0f, result, 0f,
                "Shared boundary has zero Democracy Cohesion magnitude");

            nation = Nation();
            KnowledgeEducationPatch.Prefix(ref result, nation);
            Near(166667f / nation.population * 4f * (float)Math.Pow(0.87f, 8f),
                result, 0.000001f, "knowledge education");
            nation.cohesion = 7f;
            KnowledgeCohesionPatch.Prefix(ref result, nation);
            Near(-333333f / nation.population, result, 0.000001f, "knowledge cohesion");
            nation.democracy = 5f;
            GovernmentDemocracyPatch.Prefix(ref result, nation);
            GovernmentDemocracyPatch.Postfix(ref result, nation);
            Near(333333f / nation.population, result, 0.000001f,
                "government democracy doubled at curve midpoint");

            float[] governmentPoints = { 0f, 2.5f, 5f, 7.5f, 10f };
            float[] positiveMultipliers = { 3f, 1.7320508f, 1f, 0.5773503f, 0.3333333f };
            float[] negativeMultipliers = { 0.3333333f, 0.5773503f, 1f, 1.7320508f, 3f };
            for (int index = 0; index < governmentPoints.Length; index++)
            {
                Near(positiveMultipliers[index], GovernmentMath.TransformChange(
                    1f, governmentPoints[index], 3f), 0.000001f,
                    "positive Government curve at " + governmentPoints[index]);
                Near(-negativeMultipliers[index], GovernmentMath.TransformChange(
                    -1f, governmentPoints[index], 3f), 0.000001f,
                    "negative Government curve at " + governmentPoints[index]);
            }

            nation.democracy = 0f;
            result = 333333f / nation.population;
            GovernmentDemocracyPatch.Postfix(ref result, nation);
            Near(333333f / nation.population * 3f, result, 0.000001f,
                "Government investment triples near zero");
            nation.democracy = 10f;
            result = 333333f / nation.population;
            GovernmentDemocracyPatch.Postfix(ref result, nation);
            Near(333333f / nation.population / 3f, result, 0.000001f,
                "Government investment falls to one third near ten");

            nation.democracy = 5f;
            result = -0.01f;
            GovernmentChangeCurvePatch.Prefix(ref result, nation,
                TINationState.DemocracyChangeReason.DemReason_LowCohesion);
            Near(-0.005f, result, 0.000001f,
                "passive low-Cohesion Government loss is halved");
            nation.democracy = 10f;
            result = -0.01f;
            GovernmentChangeCurvePatch.Prefix(ref result, nation,
                TINationState.DemocracyChangeReason.DemReason_LowCohesion);
            Near(-0.015f, result, 0.000001f,
                "low-Cohesion loss is halved before the high-Government curve");
            result = -0.01f;
            GovernmentChangeCurvePatch.Prefix(ref result, nation,
                TINationState.DemocracyChangeReason.DemReason_OppressionPriority);
            Near(-0.01f, result, 0f,
                "priority Government changes are not transformed twice");

            nation.democracy = 5f;
            nation.unrest = 3f;
            OppressionUnrestPatch.Prefix(ref result, nation);
            Near(-2222222f / nation.population * 0.5f, result, 0.000001f, "oppression unrest");
            nation.currentResourceRegions = 1;
            nation.GDP = 1000000000000d;
            nation.democracy = 5f;
            SpoilsMoneyPatch.Prefix(ref result, nation);
            Near(172.5f, result, 0.0001f, "spoils money retains the full base payout");
            float smallEconomyPayout = result;
            nation.GDP = 5000000000000d;
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

            float climateDamage = -0.02f;
            ClimateGdpDamagePatch.Postfix(1.25f, ref climateDamage);
            Near(-0.018f, climateDamage, 0.000001f,
                "warm-climate GDP damage is ninety percent of vanilla");
            climateDamage = -0.02f;
            ClimateGdpDamagePatch.Postfix(0.25f, ref climateDamage);
            Near(-0.02f, climateDamage, 0f,
                "neutral and cold climate results remain vanilla");
            TIEconomyMod.Main.settings.environment.climateGdpDamageEnabled = false;
            climateDamage = -0.02f;
            ClimateGdpDamagePatch.Postfix(1.25f, ref climateDamage);
            Near(-0.02f, climateDamage, 0f,
                "disabled climate GDP adjustment remains vanilla");
            TIEconomyMod.Main.settings.environment.climateGdpDamageEnabled = true;
            climateDamage = 0.01f;
            ClimateGdpDamagePatch.Postfix(1.25f, ref climateDamage);
            Near(0.01f, climateDamage, 0f,
                "positive climate result remains unchanged");

            nation.GDP = 100000000000d;
            nation.regions[0].nuclearDetonations = 1;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            Near(-0.05f, result, 0.000001f, "fallout burden is proportional to land area");
            nation.regions[0].area_km2 = 1000f;
            EnvironmentSustainabilityPatch.Prefix(ref result, nation);
            True(Math.Abs(result) < 0.001f, "small countries suffer denser nuclear damage");

            result = 123f;
            True(!EnvironmentCo2RemovalPatch.Prefix(ref result),
                "CO2 atmospheric cleanup is suppressed");
            Near(0f, result, 0f, "Environment IP does not remove global CO2");
            result = 123f;
            True(!EnvironmentMethaneRemovalPatch.Prefix(ref result),
                "methane atmospheric cleanup is suppressed");
            Near(0f, result, 0f, "Environment IP does not remove global methane");
            result = 123f;
            True(!EnvironmentNitrousOxideRemovalPatch.Prefix(ref result),
                "nitrous-oxide atmospheric cleanup is suppressed");
            Near(0f, result, 0f, "Environment IP does not remove global nitrous oxide");

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
            Near(1.125f, (float)(resourceLarge.Item1 / emissions.Item1), 0.0001f,
                "one resource in a one-trillion-dollar economy uses curve 0.5");
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
            nation.democracy = 5f;
            SpoilsGovernmentPatch.Prefix(ref result, nation);
            SpoilsGovernmentPatch.Postfix(ref result, nation);
            Near(-66667f / nation.population, result, 0.000001f,
                "Spoils Government demographic scaling at curve midpoint");

            nation.currentResourceRegions = 1;
            nation.GDP = 1000000000000d;
            SpoilsSustainabilityPatch.Prefix(ref result, nation);
            Near(0.0075f, result, 0.000001f,
                "Spoils carbon-intensity damage shares the one-trillion resource curve");
            TIEconomyMod.Main.settings.abundance.enabled = false;
            SpoilsSustainabilityPatch.Prefix(ref result, nation);
            Near(0.005f, result, 0.000001f,
                "disabled abundance removes the Spoils sustainability resource premium");
        }

        private static void TestNationalMergers()
        {
            Reset();
            TINationState absorbing = MergerNation(1f, 1000000f, 1f);
            TINationState joining = MergerNation(1f, 1f, 1f);
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
            TIEconomyMod.Main.settings.nationalMergers.inequalityEnabled = false;
            inequalityState = null;
            InequalityMergerPatch.Prefix(absorbing, joining, ref inequalityState);
            True(inequalityState == null, "disabled Inequality merger retains vanilla");
        }

        private static void TestEarthLaunchCostMath()
        {
            const double mu = 3.986004418e14;
            const double radius = 6356751.107d;
            const double rotationPeriod = 86164.0884d;
            const double genericEv = 2.11d;
            double reference = EarthLaunchCostMath.ReferenceAscentDeltaV_kps(
                mu,
                radius,
                rotationPeriod);

            Near(
                1f,
                (float)EarthLaunchCostMath.BoostCost(
                    10d,
                    0.1d,
                    0d,
                    genericEv),
                0.000001f,
                "reference Earth launch is one Boost per ten tonnes");

            double plus40 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 40d, 500d, 40d);
            double minus40 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, -40d, 500d, -40d);
            Near(
                (float)plus40,
                (float)minus40,
                0.000001f,
                "signed inclination and latitude symmetry");

            double altitude500 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 0d, 500d, 0d);
            double altitude1000 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 0d, 1000d, 0d);
            True(
                altitude1000 > altitude500,
                "Earth launch cost increases with altitude");

            double direct20 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 20d, 500d, 20d);
            double dogleg20 = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 60d, 500d, 20d);
            True(
                dogleg20 > direct20 + 2d,
                "poleward launch site pays a large lower-inclination dogleg");

            Near(
                1.013f,
                (float)Math.Exp((direct20 - reference) / genericEv),
                0.002f,
                "20-degree illustrative Boost calibration");
            Near(
                1.053f,
                (float)Math.Exp((plus40 - reference) / genericEv),
                0.002f,
                "40-degree illustrative Boost calibration");
            double polar = EarthLaunchCostMath.AscentDeltaV_kps(
                mu, radius, rotationPeriod, 90d, 500d, 90d);
            Near(
                1.247f,
                (float)Math.Exp((polar - reference) / genericEv),
                0.003f,
                "polar illustrative Boost calibration");
            Near(
                1.132f,
                (float)Math.Exp((altitude1000 - reference) / genericEv),
                0.003f,
                "1000-kilometer illustrative Boost calibration");

            List<EarthLaunchSite> sites = new List<EarthLaunchSite>
            {
                new EarthLaunchSite(60d),
                new EarthLaunchSite(20d),
                new EarthLaunchSite(0d)
            };
            List<EarthParkingOption> parking = new List<EarthParkingOption>
            {
                new EarthParkingOption(500d, 40d, 0.5d),
                new EarthParkingOption(1000d, 0d, 0.1d)
            };
            double ordered = EarthLaunchCostMath.MinimumNormalizedRouteDeltaV_kps(
                mu, radius, rotationPeriod, sites, parking);
            sites.Reverse();
            parking.Reverse();
            double reversed = EarthLaunchCostMath.MinimumNormalizedRouteDeltaV_kps(
                mu, radius, rotationPeriod, sites, parking);
            Near(
                (float)ordered,
                (float)reversed,
                0.000001f,
                "launch-site and parking-orbit minimum ignores collection order");
        }

        private static void TestWeightValidation(string path)
        {
            TechWeightCatalog catalog = TechWeightCatalog.Load(path, delegate { }, delegate { return true; });
        True(catalog.Count == 149, "technology CSV covers all 149 TI 1.0.51 technologies");

            TechWeights solid = Weight(catalog, "SolidCoreFissionSystems");
            TechWeights gas = Weight(catalog, "GasCoreFissionSystems");
            TechWeights advanced = Weight(catalog, "AdvancedFissionSystems");
            TechWeights terawatt = Weight(catalog, "TerawattFusionReactors");
            Near(1.5f, gas.ProductivityPercent / solid.ProductivityPercent, 0.00001f,
                "Gas Core retains the 1.5 versus 1 semantic magnitude");
            Near(2f, advanced.ProductivityPercent / solid.ProductivityPercent, 0.00001f,
                "Advanced Fission retains the 2 versus 1 semantic magnitude");
            Near(4f, terawatt.ProductivityPercent / solid.ProductivityPercent, 0.00001f,
                "Terawatt Fusion retains the 4 versus 1 semantic magnitude");

            TechWeights mission = Weight(catalog, "MissionToSpace");
            TechWeights chemical = Weight(catalog, "AdvancedChemicalRocketry");
            Near(1f, mission.ProductivityPercent, 0f,
                "Mission to Space keeps its fixed starting productivity");
            Near(1f, chemical.ProductivityPercent, 0f,
                "Advanced Chemical Rocketry keeps its fixed starting productivity");
            Near(1.0201f,
                (1f + mission.ProductivityPercent / 100f) *
                (1f + chemical.ProductivityPercent / 100f),
                0.000001f, "starting technology product is 1.0201");

            TechWeights artificialIntelligence =
                Weight(catalog, "AppliedArtificialIntelligence");
            TechWeights fusion = Weight(catalog, "TerawattFusionReactors");
            TechWeights generalPurpose = Weight(catalog, "MolecularAssemblers");
            TechWeights narrow = Weight(catalog, "MassDrivers");
            True(artificialIntelligence.LaborSubstitution >
                artificialIntelligence.ResourceSubstitution,
                "AI emphasizes labor substitution");
            True(fusion.ResourceSubstitution > fusion.LaborSubstitution,
                "fusion emphasizes resource substitution");
            Near(generalPurpose.LaborSubstitution,
                generalPurpose.ResourceSubstitution, 0f,
                "general-purpose breakthroughs emphasize both axes");
            True(narrow.LaborSubstitution > 0f &&
                narrow.ResourceSubstitution > 0f &&
                narrow.LaborSubstitution < solid.LaborSubstitution,
                "narrow technologies keep small positive spillovers");

            double fullProduct = 1d;
            float futureLabor = 0f;
            float futureResources = 0f;
            foreach (string technologyId in catalog.TechnologyIds)
            {
                TechWeights weights = Weight(catalog, technologyId);
                fullProduct *= 1d + weights.ProductivityPercent / 100d;
                if (!TechWeightCatalog.IsStartingTechnology(technologyId))
                {
                    futureLabor += weights.LaborSubstitution;
                    futureResources += weights.ResourceSubstitution;
                }
            }
            Near(3.40f, (float)fullProduct, 0.00001f,
                "normalized full technology tree compounds to 3.40");
            Near(catalog.TotalFutureLaborWeight, futureLabor, 0.00001f,
                "future labor progress reaches one");
            Near(catalog.TotalFutureResourceWeight, futureResources, 0.00001f,
                "future resource progress reaches one");

            Reset();
            TIEconomyMod.Main.settings.abundance.enabled = false;
            TINationState technologyNation = Nation();
            technologyNation.perCapitaGDP = 250000f;
            float previousGain = AggregateEconomyGainBillions(technologyNation);
            foreach (string technologyId in catalog.TechnologyIds)
            {
                GameStateManager.Research.finishedTechsNames.Add(technologyId);
                float currentGain = AggregateEconomyGainBillions(technologyNation);
                True(currentGain > previousGain,
                    technologyId + " monotonically improves Economy return");
                previousGain = currentGain;
            }

            string unknownFile = Path.GetTempFileName();
            File.WriteAllText(unknownFile,
                "tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale\n" +
                "Unknown,true,2,1,1,test\n");
            True(TechWeightCatalog.Load(unknownFile, delegate { }, delegate { return false; }).Count == 0,
                "unknown CSV ID skipped");

            string duplicateFile = Path.GetTempFileName();
            File.WriteAllText(duplicateFile,
                "tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale\n" +
                "Known,false,1,1,1,test\nKnown,true,2,1,1,test\n");
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

            string zeroFutureAxisFile = Path.GetTempFileName();
            File.WriteAllText(zeroFutureAxisFile,
                "tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale\n" +
                "MissionToSpace,true,1,1,1,test\n");
            threw = false;
            try
            {
                TechWeightCatalog.Load(
                    zeroFutureAxisFile, delegate { }, delegate { return true; });
            }
            catch (InvalidDataException)
            {
                threw = true;
            }
            True(threw, "zero future-axis totals fail validation");
            File.Delete(unknownFile);
            File.Delete(duplicateFile);
            File.Delete(zeroFutureAxisFile);
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
            TechWeightCatalog savedCatalog = TIEconomyMod.Main.techWeights;
            TIEconomyMod.Main.techWeights = null;
            TIEconomyMod.Main.settings.technology.enabled = true;
            result = 0f;
            True(!EconomyGrowthPatch.Prefix(ref result, nation) && result > 0f,
                "unavailable technology catalog falls back to productivity one and zero progress");
            TIEconomyMod.Main.techWeights = savedCatalog;
            TIEconomyMod.Main.settings.environment.enabled = false;
            True(EnvironmentSustainabilityPatch.Prefix(ref result, nation),
                "disabled Environment returns to vanilla");
            result = 123f;
            True(EnvironmentCo2RemovalPatch.Prefix(ref result),
                "disabled Environment restores vanilla atmospheric cleanup");
            Near(123f, result, 0f,
                "disabled atmospheric-cleanup prefix leaves the vanilla result untouched");
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

        private static void TestPerformanceCaches()
        {
            ReferenceContextVariantCache<object> cache =
                new ReferenceContextVariantCache<object>();
            object ships = new object();
            object imports = new object();
            int builds = 0;
            Func<object> build = delegate
            {
                builds++;
                return new object();
            };

            object ordinary = cache.GetOrCreate(
                ships, 46, imports, 0, "en", false, build);
            bool stableHits = true;
            for (int index = 0; index < 10000; index++)
            {
                stableHits = stableHits && ReferenceEquals(
                    ordinary,
                    cache.GetOrCreate(
                        ships, 46, imports, 0, "en", false, build));
            }
            True(stableHits,
                "skirmish option cache returns the ordinary catalog");
            True(builds == 1,
                "ten thousand stable roster rows build one ordinary catalog");

            object alien = cache.GetOrCreate(
                ships, 46, imports, 0, "en", true, build);
            True(!ReferenceEquals(ordinary, alien) && builds == 2,
                "human and alien-eligible dropdown variants cache separately");

            cache.GetOrCreate(
                ships, 47, imports, 0, "en", false, build);
            True(builds == 3,
                "ship-list count changes invalidate both dropdown variants");
            cache.GetOrCreate(
                ships, 47, imports, 1, "en", false, build);
            True(builds == 4,
                "import-list changes invalidate dropdown options");
            cache.GetOrCreate(
                ships, 47, imports, 1, "deu", false, build);
            True(builds == 5,
                "localization changes invalidate dropdown text");
            cache.Invalidate();
            cache.GetOrCreate(
                ships, 47, imports, 1, "deu", false, build);
            True(builds == 6,
                "explicit dropdown invalidation rebuilds the catalog");

            IdentityProbe first = new IdentityProbe(1);
            IdentityProbe equalButDistinct = new IdentityProbe(1);
            Dictionary<IdentityProbe, float> identityValues =
                new Dictionary<IdentityProbe, float>(
                    ReferenceIdentityComparer<IdentityProbe>.Instance);
            identityValues.Add(first, 8.7f);
            identityValues.Add(equalButDistinct, 2.2f);
            True(identityValues.Count == 2,
                "template identity lookup does not merge equal distinct instances");

            float power = 0f;
            for (int index = 0; index < 100000; index++)
            {
                if (!identityValues.TryGetValue(first, out power))
                {
                    throw new InvalidOperationException(
                        "identity lookup lost a hydrated template");
                }
            }
            Near(8.7f, power, 0f,
                "one hundred thousand hydrated power lookups retain their value");
        }

        private static void TestMineMissionControl()
        {
            Near(0f, MineMissionControlMath.NetworkCost(new int[0]), 0f,
                "empty mine network costs no MC");
            Near(1f, MineMissionControlMath.TierCost(1), 0f,
                "Tier 1 mine costs one MC");
            Near(2f, MineMissionControlMath.TierCost(2), 0f,
                "Tier 2 mine costs two MC");
            Near(3f, MineMissionControlMath.TierCost(3), 0f,
                "Tier 3 mine costs three MC");
            Near(10f, MineMissionControlMath.NetworkCost(
                new[] { 1, 1, 2, 3, 3 }), 0f,
                "mine network cost is the sum of active tiers");
            Near(0f, MineMissionControlMath.NetworkCost(null), 0f,
                "missing mine list costs no MC");
            Near(0f, MineMissionControlMath.TierCost(-1), 0f,
                "invalid negative mine tier is clamped to zero");
            True(MineMissionControlMath.UsageDisplayState(75f, 100f) ==
                    MissionControlUsageDisplayState.Normal,
                "Mission Control usage at exactly 75 percent stays normal");
            True(MineMissionControlMath.UsageDisplayState(75.01f, 100f) ==
                    MissionControlUsageDisplayState.Warning,
                "Mission Control usage above 75 percent turns orange");
            True(MineMissionControlMath.UsageDisplayState(100f, 100f) ==
                    MissionControlUsageDisplayState.Warning,
                "Mission Control usage at the limit stays orange");
            True(MineMissionControlMath.UsageDisplayState(100.01f, 100f) ==
                    MissionControlUsageDisplayState.OverCapacity,
                "Mission Control usage above the limit turns red");
            True(MineMissionControlMath.UsageDisplayState(0f, 0f) ==
                    MissionControlUsageDisplayState.Normal,
                "zero usage with zero capacity stays normal");
        }

        private static void TestWeaponCadence()
        {
            double accumulated = 0.0;
            True(WeaponCadenceMath.AccumulateChecks(
                    ref accumulated, 0.049) == 0,
                "weapon cadence waits for a complete 50 ms interval");
            True(WeaponCadenceMath.AccumulateChecks(
                    ref accumulated, 0.001) == 1,
                "weapon cadence checks exactly at 50 ms");
            Near(0f, (float)accumulated, 0.000001f,
                "weapon cadence consumes the complete interval");

            True(WeaponCadenceMath.AccumulateChecks(
                    ref accumulated, 0.26) == 5,
                "weapon cadence catches up each elapsed 50 ms interval");
            Near(0.01f, (float)accumulated, 0.000001f,
                "weapon cadence preserves sub-interval remainder");
            Near(0.21f,
                (float)WeaponCadenceMath.OldestCheckOffset_s(
                    accumulated, 5),
                0.000001f,
                "weapon cadence dates catch-up checks on the 50 ms grid");

            accumulated = 0.0;
            True(WeaponCadenceMath.AccumulateChecks(
                    ref accumulated, 5.0) ==
                WeaponCadenceMath.MaximumChecksPerUpdate,
                "weapon cadence bounds anomalous catch-up work");
            Near(0f, (float)accumulated, 0.000001f,
                "weapon cadence drops backlog beyond its safety bound");
        }

        private sealed class IdentityProbe
        {
            private readonly int value;

            public IdentityProbe(int value)
            {
                this.value = value;
            }

            public override bool Equals(object obj)
            {
                IdentityProbe other = obj as IdentityProbe;
                return other != null && other.value == value;
            }

            public override int GetHashCode()
            {
                return value;
            }
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

        private static float AggregateEconomyGainBillions(TINationState nation)
        {
            float perCapita = 0f;
            EconomyGrowthPatch.Prefix(ref perCapita, nation);
            return perCapita * nation.population / 1000000000f;
        }

        private static TechWeights Weight(TechWeightCatalog catalog, string id)
        {
            TechWeights weights;
            True(catalog.TryGetWeights(id, out weights), "weight exists for " + id);
            return weights;
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
