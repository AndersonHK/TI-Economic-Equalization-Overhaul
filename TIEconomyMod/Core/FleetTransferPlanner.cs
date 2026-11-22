using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.TIVirtualFleetState;
using UnityEngine;

namespace TIEconomyMod.Core
{
    internal static class FleetTransferPlanner
    {
        internal static TransferResult Request(
            IMobileAsset fleet, TIGameState destination, Action<Trajectory[]> callback, out double lowestDv,
            bool placeholder, bool firstFeasible, float sampleSize, bool isPlayer)
        {
            lowestDv = -1;
            var targetFleet = destination as TISpaceFleetState;
            if (targetFleet != null && targetFleet.GetFleetsWeAreIntercepting().Contains(fleet))
                return Finish(callback, TransferResult.Outcome.Fail_AttemptedFleetInterceptThatWouldCauseTargetingLoop);

            // The supported native build exposes no executable linear placeholder model.
            if (placeholder)
                return Finish(callback, TransferResult.Outcome.Fail_CodePathNotImplemented);

            TIDateTime now = TITimeState.Now();
            bool movingTarget = MasterTransferPlanner.DoWeKnowThatFleetIsTransfering(targetFleet, fleet.faction);
            if (!fleet.transferAssigned && fleet.ref_orbit != null &&
                IsAlreadyAtDestination(fleet, destination, movingTarget, now))
            {
                var empty = new Trajectory_Patched();
                empty.BuildEmptyTrajectory(fleet, now, destination as TISpaceAssetState);
                lowestDv = 0;
                callback(new Trajectory[] { empty });
                return new TransferResult(TransferResult.Outcome.Success);
            }

            var target = NormalizeDestination(destination);
            var targetValue = (ITransferTarget)(target is TISpaceAssetState ? (TIGameState)target : target?.ref_orbit);
            if (targetValue == null || !(fleet.cruiseAcceleration_mps2 > 0))
                return Finish(callback, TransferResult.Outcome.Fail_CodePathNotImplemented);

            var request = new NativeTransferCandidates(fleet, target, targetValue, now, sampleSize, isPlayer);
            Trajectory old = (fleet as TISpaceFleetState)?.trajectory;
            var usable = new List<Trajectory>();
            TransferResult failure = null;
            double minimum = double.PositiveInfinity;
            double cap = NativeTransferCandidates.DurationCap(fleet.faction);

            foreach (TrajectoryModel model in request.Models(movingTarget))
            {
                var trials = new List<Trajectory>();
                double modelMinimum = double.PositiveInfinity;
                TransferResult outcome = request.Generate(model, ref trials, ref modelMinimum);
                if (!movingTarget)
                    minimum = Math.Min(minimum, modelMinimum);
                if (outcome?.Result != TransferResult.Outcome.Success)
                    failure = TransferResult.Best(failure, outcome);
                foreach (Trajectory trial in trials)
                    Validate(trial);
                if (firstFeasible && usable.Any(t => t.launchTime != t.arrivalTime))
                    break;
            }

            if (old is Trajectory_Patched previous && old.destination == destination)
                Validate(Trajectory_Patched.BuildTruncatedTrajectory((TISpaceFleetState)fleet, previous, now), true);

            lowestDv = double.IsPositiveInfinity(minimum) ? -1 : minimum;
            Trajectory[] result = firstFeasible && usable.Any(t => t.launchTime != t.arrivalTime)
                ? usable.Where(t => t.launchTime != t.arrivalTime).OrderBy(t => t.arrivalTime).ThenBy(t => t.DV_mps).Take(1).ToArray()
                : NonDominated(usable);
            callback(result);
            return result.Length != 0 ? new TransferResult(TransferResult.Outcome.Success)
                : failure ?? new TransferResult(TransferResult.Outcome.Fail_CodePathNotImplemented);

            void Validate(Trajectory trial, bool includesOldTransfer = false)
            {
                if (trial == null || trial.arrivalTime == null || trial.launchTime == null || trial.arrivalTime < trial.launchTime)
                    return;
                if (trial is Trajectory_Patched patched)
                {
                    if (!includesOldTransfer && (fleet as TISpaceFleetState)?.inTransfer == true && old is Trajectory_Patched prior)
                        patched.AddRemnantsOfExistingTransfer(prior);
                    patched.Segments.RemoveAll(segment => segment == null);
                }
                if (movingTarget)
                    trial.DV_targetFleet_mps = targetFleet.trajectory.RemainingDVatTime_mps(trial.arrivalTime);
                double cost = trial.DV_mps;
                if (!(cost >= 0) || double.IsInfinity(cost))
                    return;
                minimum = Math.Min(minimum, cost / 1000);
                if (cost > fleet.currentDeltaV_mps)
                    failure = TransferResult.Best(failure, new TransferResult(TransferResult.Outcome.Fail_InsufficientDV, cost));
                else if (!(trial.duration_s <= cap))
                    failure = TransferResult.Best(failure, new TransferResult(TransferResult.Outcome.Fail_ExceedsMaxDuration, cap));
                else
                    usable.Add(trial);
            }
        }

        private static bool IsAlreadyAtDestination(IMobileAsset fleet, TIGameState destination, bool moving, TIDateTime now)
        {
            if (destination is TIOrbitState orbit)
                return orbit == fleet.ref_orbit;
            if (moving || !(destination is TISpaceAssetState asset))
                return false;
            fleet.getOrbitalElementsState(now, out var origin, out var from, out _);
            asset.getOrbitalElementsState(now, out var target, out var to, out _);
            return from == to && origin.Approximately(target) &&
                (fleet.ToGlobalCartesianStateAtTime(now).position - asset.ToGlobalCartesianStateAtTime(now).position).magnitude < 5000;
        }

        private static TISpaceGameState NormalizeDestination(TIGameState destination)
        {
            if (destination == null)
                return null;
            if (destination.isNaturalSpaceObjectState || destination.ref_hab?.IsBase == true ||
                destination.isHabSiteState || destination.isRegionState)
                return destination.ref_naturalSpaceObject?.orbits.OrderBy(orbit => orbit.semiMajorAxis_km).FirstOrDefault();
            return destination as TISpaceGameState;
        }

        private static TransferResult Finish(Action<Trajectory[]> callback, TransferResult.Outcome reason)
        {
            callback(Array.Empty<Trajectory>());
            return new TransferResult(reason);
        }

        private static Trajectory[] NonDominated(List<Trajectory> choices)
        {
            var winners = new List<Trajectory>();
            for (int i = 0; i < choices.Count; i++)
            {
                Trajectory candidate = choices[i];
                bool dominated = false;
                for (int j = 0; j < choices.Count && !dominated; j++)
                {
                    if (j == i)
                        continue;
                    Trajectory other = choices[j];
                    if (Mathd.Approximately(candidate.DV_mps, other.DV_mps) &&
                        Mathd.Approximately(candidate.arrivalTime.ToJulianDateInSeconds(), other.arrivalTime.ToJulianDateInSeconds()))
                        dominated = j < i;
                    else
                        dominated = candidate.DV_mps >= other.DV_mps && candidate.arrivalTime >= other.arrivalTime;
                }
                if (!dominated)
                    winners.Add(candidate);
            }
            return winners.ToArray();
        }
    }

    // Native numerical searches supply candidates to the EEO request owner.
    internal sealed class NativeTransferCandidates
    {
        private static readonly Type Planner = typeof(MasterTransferPlanner);
        private static readonly MethodInfo Simplify = AccessTools.Method(Planner, "GetSimplifiedPositions");
        private static readonly MethodInfo Hybrid = AccessTools.Method(Planner, "CalculateImpulseMicrothrustHybridTransfers");
        private static readonly MethodInfo Phasing = AccessTools.Method(Planner, "CalculateOrbitPhasingTransfers");
        private static readonly MethodInfo Microthrust = AccessTools.Method(Planner, "CalculateMicrothrustTransfer");
        private static readonly MethodInfo Inclination = AccessTools.Method(Planner, "CalculateInclinationChangeTransfers");
        private static readonly Func<IMobileAsset, TIOrbitState, bool> OrbitMatches =
            AccessTools.MethodDelegate<Func<IMobileAsset, TIOrbitState, bool>>(AccessTools.Method(Planner, "DoesOrbitMatch"));
        internal static readonly Func<TIFactionState, double> DurationCap =
            AccessTools.MethodDelegate<Func<TIFactionState, double>>(AccessTools.Method(Planner, "TransferDurationHardCap"));

        private readonly IMobileAsset fleet;
        private readonly TISpaceGameState target;
        private readonly ITransferTarget targetValue;
        private readonly TIDateTime now;
        private readonly double sample;
        private readonly bool isPlayer;
        private readonly TINaturalSpaceObjectState center;
        private readonly double originRadius;
        private readonly double targetRadius;

        internal NativeTransferCandidates(IMobileAsset fleet, TISpaceGameState target, ITransferTarget targetValue,
            TIDateTime now, double sample, bool isPlayer)
        {
            this.fleet = fleet;
            this.target = target;
            this.targetValue = targetValue;
            this.now = now;
            this.sample = sample;
            this.isPlayer = isPlayer;
            object geometry = Simplify.Invoke(null, new object[] { fleet, targetValue, now, now });
            Type type = geometry.GetType();
            center = (TINaturalSpaceObjectState)type.GetField("commonBarycenter").GetValue(geometry);
            originRadius = (double)type.GetField("originDistToCommonBarycenter_m").GetValue(geometry);
            targetRadius = (double)type.GetField("destinationDistToCommonBarycenter_m").GetValue(geometry);
        }

        internal IEnumerable<TrajectoryModel> Models(bool movingTarget)
        {
            TIOrbitState origin = fleet.ref_orbit;
            TIOrbitState destination = target.ref_orbit;
            var existing = (fleet as TISpaceFleetState)?.trajectory;
            var incoming = (target as TISpaceFleetState)?.trajectory;
            if (center is TISpaceBodyState && FiniteThrustPlaneChange.Supports(fleet, target))
                return new[] { TrajectoryModel.InclinationChange };

            var models = new List<TrajectoryModel>();
            if (origin != null && (origin == destination || Mathd.Approximately(originRadius, targetRadius)))
            {
                bool shortPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(originRadius, 3) / center.mu) < 63113848;
                if (shortPeriod)
                    models.Add(TrajectoryModel.OrbitPhasing);
                if (!shortPeriod || UsesImpulseAt(originRadius, center) || movingTarget)
                    models.Add(TrajectoryModel.ImpulseMicrothrustHybrid);
            }
            else
            {
                models.Add(TrajectoryModel.ImpulseMicrothrustHybrid);
                if (origin != null && destination != null && origin.barycenter == destination.barycenter &&
                    (Math.Abs(origin.inclination_Rad - destination.inclination_Rad) > 0.1 ||
                    origin.inclination_Rad * Math.Abs(origin.longitudeAscendingNode_Rad - destination.longitudeAscendingNode_Rad) > 0.05))
                    models.Add(TrajectoryModel.InclinationChange);
            }

            bool targetOrbitMatch = movingTarget && OrbitMatches(fleet, incoming.destinationOrbit);
            if (movingTarget && (existing == null || existing.launchTime >= now) && !target.barycenter.isLagrangePointState &&
                (targetOrbitMatch || (incoming.launchTime > now && OrbitMatches(fleet, incoming.originOrbit))))
                Add(TrajectoryModel.OrbitPhasing);
            if (existing?.destination?.barycenter != null && !existing.destroyOnArrival && !existing.destination.barycenter.isLagrangePointState &&
                (movingTarget ? incoming.destinationOrbit : destination) == existing.destinationOrbit)
                Add(TrajectoryModel.OrbitPhasing);
            if (movingTarget && !targetOrbitMatch && incoming.destination is TIOrbitState orbit && orbit.barycenter == fleet.barycenter() &&
                origin != null && !UsesImpulseAt(origin.semiMajorAxis_m, orbit.barycenter) && !UsesImpulseAt(orbit.semiMajorAxis_m, orbit.barycenter))
                Add(TrajectoryModel.Microthrust);
            if (movingTarget)
                Add(TrajectoryModel.ImpulseMicrothrustHybrid);
            return models;

            void Add(TrajectoryModel model)
            {
                if (!models.Contains(model))
                    models.Add(model);
            }
        }

        private bool UsesImpulseAt(double radius, TINaturalSpaceObjectState body) =>
            new MicrothrustSphere(fleet.cruiseAcceleration_mps2, body.mu, body.sphereOfInfluence_m).Radius_m < radius;

        internal TransferResult Generate(TrajectoryModel model, ref List<Trajectory> candidates, ref double lowestDv)
        {
            double budget = fleet.currentDeltaV_mps;
            double acceleration = fleet.cruiseAcceleration_mps2;
            MethodInfo method;
            object[] args;
            switch (model)
            {
                case TrajectoryModel.InclinationChange:
                    if (center is TISpaceBodyState body && FiniteThrustPlaneChange.Supports(fleet, target))
                        return FiniteThrustPlaneChange.Search(candidates, ref lowestDv, fleet, target, body, budget, acceleration);
                    method = Inclination;
                    args = new object[] { candidates, lowestDv, sample, fleet, budget, acceleration, fleet, target, targetValue, center, false };
                    break;
                case TrajectoryModel.OrbitPhasing:
                    method = Phasing;
                    args = new object[] { candidates, lowestDv, (int)Math.Ceiling(64 * sample), fleet, budget, acceleration, fleet, target, targetValue, center, false };
                    break;
                case TrajectoryModel.Microthrust:
                    method = Microthrust;
                    args = new object[] { candidates, lowestDv, fleet, budget, acceleration, fleet, target, targetValue, center, now };
                    break;
                default:
                    method = Hybrid;
                    var parameters = Activator.CreateInstance(Hybrid.GetParameters()[2].ParameterType, true);
                    var fields = Traverse.Create(parameters);
                    fields.Field("sampleSizeMultiplier").SetValue(sample);
                    fields.Field("fleet").SetValue(fleet);
                    fields.Field("fleetDeltaV_mps").SetValue(budget);
                    fields.Field("fleetAcceleration_mps2").SetValue(acceleration);
                    fields.Field("originValue").SetValue(fleet);
                    fields.Field("sDestination").SetValue(target);
                    fields.Field("destinationValue").SetValue(targetValue);
                    fields.Field("commonBarycenter").SetValue(center);
                    fields.Field("now").SetValue(now);
                    fields.Field("isPlayer").SetValue(isPlayer);
                    args = new object[] { candidates, lowestDv, parameters };
                    break;
            }
            var result = (TransferResult)method.Invoke(null, args);
            candidates = (List<Trajectory>)args[0];
            lowestDv = (double)args[1];
            return result;
        }
    }
}
