using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.TIVirtualFleetState;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(ThrustProfileTool), nameof(ThrustProfileTool.UpdateUI))]
    internal static class TransferDepartureWindowMessagePatch
    {
        private static bool Prefix(ThrustProfileTool __instance)
        {
            if (__instance.transferComputationError || __instance.Actor == null ||
                !HasContradictoryAccelerationMinimum(__instance.transferResult, __instance.Actor.cruiseAcceleration_mps2))
                return true;

            __instance.DisplayFailureReport(Loc.T("UI.TransferResult.EEO_NoDepartureWindow"));
            return false;
        }

        private static bool HasContradictoryAccelerationMinimum(TransferResult failure, double cruiseAcceleration)
        {
            return failure?.Result == TransferResult.Outcome.Fail_LaunchInPast &&
                failure.TryGetMinimumAccelerationNeeded(out double minimum, cruiseAcceleration) &&
                minimum <= cruiseAcceleration;
        }
    }

    [HarmonyPatch(typeof(MasterTransferPlanner), nameof(MasterTransferPlanner.GetTrajectoryModelsForConditions))]
    internal static class InclinedOrbitTransferPatch
    {
        private const double PlaneTolerance = 1e-8;

        private static void Postfix(IMobileAsset fleet, TIGameState destination, List<TrajectoryModel> __result)
        {
            if (!__result.Contains(TrajectoryModel.OrbitPhasing) || !IsLocalPlaneChange(fleet, destination))
                return;

            // Only compare candidates priced from the thrust needed to fly them.
            __result.Remove(TrajectoryModel.OrbitPhasing);
            __result.Remove(TrajectoryModel.ImpulseMicrothrustHybrid);
            if (!__result.Contains(TrajectoryModel.InclinationChange))
                __result.Add(TrajectoryModel.InclinationChange);
        }

        internal static bool IsLocalPlaneChange(IMobileAsset fleet, TIGameState destination)
        {
            if (!(fleet is TISpaceFleetState spaceFleet) || spaceFleet.transferAssigned)
                return false;

            bool stationaryTarget = destination is TIOrbitState ||
                (destination is TIHabState station && station.IsStation) ||
                (destination is TISpaceFleetState targetFleet && !targetFleet.transferAssigned && !targetFleet.landed);
            if (!stationaryTarget)
                return false;

            TIOrbitState target = destination.ref_orbit;
            TIOrbitState origin = spaceFleet.ref_orbit;
            if (origin == null || target == null || origin == target || !(origin.barycenter is TISpaceBodyState) ||
                origin.barycenter != target.barycenter || origin.eccentricity != 0.0 || target.eccentricity != 0.0 ||
                Math.Abs(origin.semiMajorAxis_m - target.semiMajorAxis_m) > 0.001)
                return false;

            var departure = new OrbitalElementsState(origin, 0.0, default(DateTime));
            var arrival = new OrbitalElementsState(target, 0.0, default(DateTime));
            Vector3d departureNormal = departure.normalVector;
            Vector3d arrivalNormal = arrival.normalVector;
            Vector3d intersection = Vector3d.Cross(departureNormal, arrivalNormal);
            if (!(intersection.sqrMagnitude > PlaneTolerance * PlaneTolerance) ||
                !(Vector3d.Dot(departureNormal, arrivalNormal) > 0.0))
                return false;

            // This solver rotates the orbital plane about the common node axis.
            Vector3d lineOfNodes = intersection.normalized;
            return Vector3d.Cross(lineOfNodes, departure.ascendingNodeVector).sqrMagnitude <= PlaneTolerance * PlaneTolerance &&
                Vector3d.Cross(lineOfNodes, arrival.ascendingNodeVector).sqrMagnitude <= PlaneTolerance * PlaneTolerance;
        }
    }

    [HarmonyPatch(typeof(MasterTransferPlanner), "CalculateInclinationChangeTransfers")]
    internal static class FiniteThrustPlaneChangePatch
    {
        private static bool Prefix(
            ref List<Trajectory> candidateTrajectories, ref double lowestDVFound_kps, IMobileAsset fleet,
            TISpaceGameState sDestination, TINaturalSpaceObjectState commonBarycenter,
            double fleetDeltaV_mps, double fleetAcceleration_mps2, ref TransferResult __result)
        {
            if (!(commonBarycenter is TISpaceBodyState body) ||
                !InclinedOrbitTransferPatch.IsLocalPlaneChange(fleet, sDestination))
                return true;

            __result = FiniteThrustPlaneChange.Search(candidateTrajectories, ref lowestDVFound_kps,
                fleet, sDestination, body, fleetDeltaV_mps, fleetAcceleration_mps2);
            return false;
        }
    }
}
