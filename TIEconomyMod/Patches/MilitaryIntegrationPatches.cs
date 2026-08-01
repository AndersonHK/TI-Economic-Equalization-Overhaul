using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod.Patches
{
    internal static class MilitaryIntegrationContext
    {
        [ThreadStatic]
        public static int PeacefulUnificationDepth;

        [ThreadStatic]
        public static int AbsorptionDepth;

        public static bool Enabled
        {
            get
            {
                NationalMergerSettings settings = Main.settings.nationalMergers;
                return Main.FeatureEnabled(settings.enabled) && settings.militaryEnabled;
            }
        }

        public static List<TIArmyState> SnapshotEligibleArmies(TINationState nation)
        {
            List<TIArmyState> result = new List<TIArmyState>();
            if (nation == null || nation.armies == null)
            {
                return result;
            }

            foreach (TIArmyState army in nation.armies)
            {
                if (army != null && TIGameState.Valid(army) && army.HumanArmy && !army.destroyed)
                {
                    result.Add(army);
                }
            }
            return result;
        }

        public static int CountSurvivors(List<TIArmyState> cohort, TINationState destination)
        {
            if (cohort == null || destination == null || destination.armies == null)
            {
                return 0;
            }

            int count = 0;
            foreach (TIArmyState army in cohort)
            {
                if (army != null &&
                    TIGameState.Valid(army) &&
                    army.HumanArmy &&
                    !army.destroyed &&
                    destination.armies.Contains(army))
                {
                    count++;
                }
            }
            return count;
        }

        public static bool TryApplyConservation(
            TINationState destination,
            float firstTechnology,
            List<TIArmyState> firstCohort,
            float secondTechnology,
            List<TIArmyState> secondCohort)
        {
            ArmySettings army = Main.settings.army;
            MilitarySettings military = Main.settings.military;
            double technology;
            bool solved = MilitaryMath.TrySolveConservedTechnology(
                firstTechnology,
                CountSurvivors(firstCohort, destination),
                secondTechnology,
                CountSurvivors(secondCohort, destination),
                destination.maxMilitaryTechLevel,
                army.costCoefficient,
                    army.costGrowthBase,
                military.doctrineBaseCostAtTechOne,
                military.doctrineCostGrowthBase,
                military.catchupGapCoefficient,
                out technology);
            if (!solved || !MilitaryMath.IsFinite(technology))
            {
                Main.Warn("Military technology conservation failed; retaining vanilla transfer result.");
                return false;
            }

            destination.AddToMilitaryTechLevel(
                (float)technology - destination.militaryTechLevel);
            return true;
        }
    }

    [HarmonyPatch(typeof(TINationState), "AbsorbNation")]
    public static class MilitaryAbsorptionContextPatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (MilitaryIntegrationContext.Enabled)
            {
                MilitaryIntegrationContext.AbsorptionDepth++;
            }
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception)
        {
            if (MilitaryIntegrationContext.Enabled &&
                MilitaryIntegrationContext.AbsorptionDepth > 0)
            {
                MilitaryIntegrationContext.AbsorptionDepth--;
            }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TINationState), "Unification")]
    public static class PeacefulMilitaryUnificationPatch
    {
        public sealed class Snapshot
        {
            public TINationState destination;
            public float absorbingTechnology;
            public float joiningTechnology;
            public List<TIArmyState> absorbingCohort;
            public List<TIArmyState> joiningCohort;
            public float joiningRepairDebt;
        }

        [HarmonyPrefix]
        public static void Prefix(
            TINationState __instance,
            TINationState joiningNationState,
            ref Snapshot __state)
        {
            if (!MilitaryIntegrationContext.Enabled)
            {
                return;
            }

            MilitaryIntegrationContext.PeacefulUnificationDepth++;
            __state = new Snapshot
            {
                destination = __instance,
                absorbingTechnology = __instance.militaryTechLevel,
                joiningTechnology = joiningNationState.militaryTechLevel,
                absorbingCohort =
                    MilitaryIntegrationContext.SnapshotEligibleArmies(__instance),
                joiningCohort =
                    MilitaryIntegrationContext.SnapshotEligibleArmies(joiningNationState),
                joiningRepairDebt = Math.Min(
                    0f,
                    joiningNationState.GetAccumulatedInvestmentPoints(
                        PriorityType.Military_BuildArmy))
            };
        }

        [HarmonyPostfix]
        public static void Postfix(Snapshot __state)
        {
            if (__state == null)
            {
                return;
            }

            MilitaryIntegrationContext.TryApplyConservation(
                __state.destination,
                __state.absorbingTechnology,
                __state.absorbingCohort,
                __state.joiningTechnology,
                __state.joiningCohort);

            if (__state.joiningRepairDebt < 0f)
            {
                // Vanilla already transfers half of positive progress. Debt is a
                // liability, so peaceful integration transfers all of it.
                __state.destination.ModifyAccumulatedInvestment(
                    PriorityType.Military_BuildArmy,
                    __state.joiningRepairDebt,
                    multiply: false,
                    triggerUpdate: true);
            }
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(Exception __exception, Snapshot __state)
        {
            if (__state != null && MilitaryIntegrationContext.PeacefulUnificationDepth > 0)
            {
                MilitaryIntegrationContext.PeacefulUnificationDepth--;
            }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TINationState), "TransferRegionsControlTo")]
    public static class MilitaryRegionTransferPatch
    {
        public sealed class Snapshot
        {
            public TINationState destination;
            public float sourceTechnology;
            public float destinationTechnology;
            public List<TIArmyState> incomingCohort;
            public List<TIArmyState> destinationCohort;
        }

        [HarmonyPrefix]
        public static void Prefix(
            TINationState __instance,
            List<TIRegionState> regions,
            TINationState newNation,
            ref bool destroyArmies,
            ref Snapshot __state)
        {
            if (!MilitaryIntegrationContext.Enabled || newNation == null)
            {
                return;
            }

            bool alienTransfer =
                newNation.alienNation &&
                MilitaryIntegrationContext.PeacefulUnificationDepth == 0;

            // AbsorbNation outside the explicit Unification context is conquest
            // integration. Its armies must not silently survive a vanilla false flag.
            destroyArmies = MilitaryMath.ResolveDestroyArmies(
                destroyArmies,
                MilitaryIntegrationContext.PeacefulUnificationDepth > 0,
                MilitaryIntegrationContext.AbsorptionDepth > 0,
                alienTransfer);

            if (!alienTransfer)
            {
                return;
            }

            List<TIArmyState> incoming = new List<TIArmyState>();
            foreach (TIArmyState army in
                MilitaryIntegrationContext.SnapshotEligibleArmies(__instance))
            {
                if (army.homeRegion != null && regions != null && regions.Contains(army.homeRegion))
                {
                    incoming.Add(army);
                }
            }
            __state = new Snapshot
            {
                destination = newNation,
                sourceTechnology = __instance.militaryTechLevel,
                destinationTechnology = newNation.militaryTechLevel,
                incomingCohort = incoming,
                destinationCohort =
                    MilitaryIntegrationContext.SnapshotEligibleArmies(newNation)
            };
        }

        [HarmonyPostfix]
        public static void Postfix(Snapshot __state)
        {
            if (__state == null)
            {
                return;
            }

            MilitaryIntegrationContext.TryApplyConservation(
                __state.destination,
                __state.sourceTechnology,
                __state.incomingCohort,
                __state.destinationTechnology,
                __state.destinationCohort);
        }
    }
}
