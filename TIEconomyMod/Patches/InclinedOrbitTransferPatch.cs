using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.TIVirtualFleetState;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(MasterTransferPlanner), nameof(MasterTransferPlanner.GetTrajectoryModelsForConditions))]
    internal static class InclinedOrbitTransferPatch
    {
        private const double PlaneTolerance = 1e-8;

        private static void Postfix(IMobileAsset fleet, TIGameState destination, List<TrajectoryModel> __result)
        {
            int phasingIndex = __result.IndexOf(TrajectoryModel.OrbitPhasing);
            if (phasingIndex < 0 || !IsLocalPlaneChange(fleet, destination))
                return;

            // Phasing either rejects a bare local orbit or omits the plane change for an asset.
            if (__result.Contains(TrajectoryModel.InclinationChange))
                __result.RemoveAt(phasingIndex);
            else
                __result[phasingIndex] = TrajectoryModel.InclinationChange;
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
                !Mathd.Approximately(origin.semiMajorAxis_m, target.semiMajorAxis_m))
                return false;

            var departure = new OrbitalElementsState(origin, 0.0, default(DateTime));
            var arrival = new OrbitalElementsState(target, 0.0, default(DateTime));
            Vector3d departureNormal = departure.normalVector;
            Vector3d arrivalNormal = arrival.normalVector;
            Vector3d intersection = Vector3d.Cross(departureNormal, arrivalNormal);
            if (!(intersection.sqrMagnitude > PlaneTolerance * PlaneTolerance) ||
                !(Vector3d.Dot(departureNormal, arrivalNormal) > 0.0))
                return false;

            // The native solver folds the node angle to an unsigned acute angle.
            // Only reroute when both ascending-node axes lie on the intersection.
            Vector3d lineOfNodes = intersection.normalized;
            return Vector3d.Cross(lineOfNodes, departure.ascendingNodeVector).sqrMagnitude <= PlaneTolerance * PlaneTolerance &&
                Vector3d.Cross(lineOfNodes, arrival.ascendingNodeVector).sqrMagnitude <= PlaneTolerance * PlaneTolerance;
        }
    }

    [HarmonyPatch(typeof(MasterTransferPlanner), "CalculateInclinationChangeTransfers")]
    internal static class InclinedOrbitBurnClearancePatch
    {
        private const int MaxNativeSplits = 8;
        private const int MaxClearanceSubdivisions = 8;

        private delegate List<Trajectory_Patched.BurnSegment> AdjustBurn(
            Trajectory_Patched trajectory, Trajectory_Patched.BurnSegment burn, TINaturalSpaceObjectState body);

        private static readonly AdjustBurn AdjustNativeBurn = AccessTools.MethodDelegate<AdjustBurn>(
            AccessTools.Method(typeof(Trajectory_Patched), "AdjustBurnSegmentToAvoidCollision"));

        private static void Prefix(ref List<Trajectory> candidateTrajectories, out int __state)
        {
            __state = candidateTrajectories.Count;
        }

        private static void Postfix(
            ref List<Trajectory> candidateTrajectories, IMobileAsset fleet, TISpaceGameState sDestination,
            TINaturalSpaceObjectState commonBarycenter, int __state, ref TransferResult __result)
        {
            if (candidateTrajectories.Count == __state || !InclinedOrbitTransferPatch.IsLocalPlaneChange(fleet, sDestination))
                return;

            for (int i = candidateTrajectories.Count - 1; i >= __state; i--)
            {
                if (!TryClearTrajectory((Trajectory_Patched)candidateTrajectories[i], (TISpaceBodyState)commonBarycenter))
                    candidateTrajectories.RemoveAt(i);
            }

            if (candidateTrajectories.Count == __state)
                __result = new TransferResult(TransferResult.Outcome.Fail_WouldCollideWithBody);
        }

        private static bool TryClearTrajectory(Trajectory_Patched trajectory, TISpaceBodyState body)
        {
            double radius = Math.Max(body.meanRadius_m, body.maxRadiusDimension_m);
            var segments = new List<Trajectory_Patched.IPatchSegment>();
            foreach (Trajectory_Patched.IPatchSegment segment in trajectory.Segments)
            {
                if (segment is Trajectory_Patched.BurnSegment burn)
                {
                    if (!AddClearBurn(trajectory, burn, body, radius, segments, MaxNativeSplits))
                        return false;
                }
                else
                {
                    segments.Add(segment);
                }
            }
            trajectory.Segments = segments;
            return true;
        }

        private static bool AddClearBurn(
            Trajectory_Patched trajectory, Trajectory_Patched.BurnSegment burn, TINaturalSpaceObjectState body,
            double radius, List<Trajectory_Patched.IPatchSegment> segments, int splitsLeft)
        {
            var curve = burn.burnDescription;
            if (ClearsBody(curve.startPosition, curve.startVelocityControlPoint, curve.endVelocityControlPoint,
                curve.endPosition, radius, MaxClearanceSubdivisions))
            {
                segments.Add(burn);
                return true;
            }

            if (splitsLeft == 0 || !(curve.startPosition.magnitude > radius) || !(curve.endPosition.magnitude > radius))
                return false;

            List<Trajectory_Patched.BurnSegment> adjusted = AdjustNativeBurn(trajectory, burn, body);
            if (adjusted.Count != 2)
                return false;

            foreach (Trajectory_Patched.BurnSegment piece in adjusted)
            {
                piece.isImpulse = burn.isImpulse;
                piece.isTorch = burn.isTorch;
                piece.isOrbitPhasing = burn.isOrbitPhasing;
                piece.interruptible = burn.interruptible;
                if (!AddClearBurn(trajectory, piece, body, radius, segments, splitsLeft - 1))
                    return false;
            }
            return true;
        }

        private static bool ClearsBody(Vector3d start, Vector3d control1, Vector3d control2, Vector3d end, double radius, int depth)
        {
            if (!(start.magnitude > radius) || !(end.magnitude > radius))
                return false;

            // The Bézier curve stays in its control hull. A separating plane
            // proves clearance for the whole subcurve, including between samples.
            Vector3d direction = (start + end).normalized;
            if (Vector3d.Dot(start, direction) > radius && Vector3d.Dot(control1, direction) > radius &&
                Vector3d.Dot(control2, direction) > radius && Vector3d.Dot(end, direction) > radius)
                return true;

            if (depth == 0)
                return false;

            Vector3d left = (start + control1) * 0.5;
            Vector3d middle = (control1 + control2) * 0.5;
            Vector3d right = (control2 + end) * 0.5;
            Vector3d leftControl = (left + middle) * 0.5;
            Vector3d rightControl = (middle + right) * 0.5;
            Vector3d midpoint = (leftControl + rightControl) * 0.5;
            return ClearsBody(start, left, leftControl, midpoint, radius, depth - 1) &&
                ClearsBody(midpoint, rightControl, right, end, radius, depth - 1);
        }
    }
}
