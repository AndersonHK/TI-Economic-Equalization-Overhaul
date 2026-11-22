using System;
using System.Linq;
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

    [HarmonyPatch(typeof(MasterTransferPlanner), nameof(MasterTransferPlanner.RequestTrajectories))]
    internal static class UnifiedFleetTransferRequestPatch
    {
        private static bool Prefix(IMobileAsset fleet, TIGameState destination, Action<Trajectory[]> callback,
            out double lowestDVFound_kps, bool usePlaceholderTrajectories, bool stopOnFirstSuccess,
            float sampleSizeMultiplier, bool isPlayer, ref TransferResult __result)
        {
            __result = FleetTransferPlanner.Request(fleet, destination, callback, out lowestDVFound_kps,
                usePlaceholderTrajectories, stopOnFirstSuccess, sampleSizeMultiplier, isPlayer);
            return false;
        }
    }

    [HarmonyPatch(typeof(MasterTransferPlanner), nameof(MasterTransferPlanner.GetEstimatedTransferTime_s))]
    internal static class UnifiedFleetTransferEstimatePatch
    {
        private static bool Prefix(TISpaceAssetState asset, TISpaceObjectState destination, double acceleration_mps2,
            double deltaV_mps, out bool impossible, ref double __result)
        {
            TIOrbitState origin = asset.ref_orbit ?? asset.ref_naturalSpaceObject?.orbits
                .OrderBy(orbit => orbit.semiMajorAxis_m).FirstOrDefault();
            if (origin == null)
            {
                impossible = true;
                __result = double.PositiveInfinity;
                return false;
            }
            var fleet = asset.orbitState != null
                ? new TIVirtualSpaceFleet(asset, (float)acceleration_mps2, (float)deltaV_mps, asset.faction)
                : new TIVirtualSpaceFleet(origin, (float)acceleration_mps2, (float)deltaV_mps, asset.faction);
            double eta = double.PositiveInfinity;
            FleetTransferPlanner.Request(fleet, destination, choices =>
            {
                foreach (Trajectory choice in choices)
                    eta = Math.Min(eta, choice.duration_s);
            }, out _, false, false, 0.2f, false);
            impossible = double.IsPositiveInfinity(eta);
            __result = eta;
            return false;
        }
    }
}
