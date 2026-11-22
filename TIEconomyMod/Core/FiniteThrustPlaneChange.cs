using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.TIVirtualFleetState;
using UnityEngine;

namespace TIEconomyMod.Core
{
    internal static class FiniteThrustPlaneChange
    {
        private static readonly double[] Raises = { 0, 0.125, 0.25, 0.5, 0.75, 1, 1.5, 2 };
        private static readonly double[] InclinationWeights = { 0, 0.5, 1 };
        private static readonly double[] NodeWeights = { 0, 1.0 / 3.0 };
        private static readonly double[] DurationMultipliers = { 1, 1.125, 1.25, 1.5, 2, 3, 4 };
        private static readonly Func<TIFactionState, double> DurationCap =
            AccessTools.MethodDelegate<Func<TIFactionState, double>>(
                AccessTools.Method(typeof(MasterTransferPlanner), "TransferDurationHardCap"));

        internal static TransferResult Search(
            List<Trajectory> candidates, ref double lowestDv_kps, IMobileAsset fleet,
            TISpaceGameState target, TISpaceBodyState body, double budget, double acceleration)
        {
            var now = TITimeState.Now();
            var origin = new OrbitalElementsState(fleet.ref_orbit, 0.0, now);
            var destination = new OrbitalElementsState(target.ref_orbit, 0.0, now);
            double radius = origin.semiMajorAxis_m;
            double maximumRadius = Math.Min(radius * 3, body.hillRadius_m);
            if (!(acceleration > 0) || !(radius > Math.Max(body.meanRadius_m, body.maxRadiusDimension_m)) ||
                !(maximumRadius >= radius) || !(body.mu > 0))
                return new SearchFailure();

            double omega = Math.Sqrt(body.mu / (radius * radius * radius));
            Vector3d node = Vector3d.Cross(origin.normalVector, destination.normalVector).normalized;
            Vector3d tangent = Vector3d.Cross(origin.normalVector, node);
            Vector3d departurePosition = fleet.ref_orbit.ToOrbitalElementsState(now, fleet.meanAnomaly_Rad(now))
                .ToCartesianStateAtTime(now.ExportTime(), body.mass_kg).position;
            double phase = Math.Atan2(Vector3d.Dot(departurePosition, tangent), Vector3d.Dot(departurePosition, node));
            double wait = PositiveModulo(-phase, Math.PI) / omega;
            // Round the epoch up to a future game-clock instant. At LEO speed
            // its millisecond resolution limits the matching error to metres.
            if (wait < 0.001)
                wait += Math.PI / omega;
            var launch = new TIDateTime(now, wait);
            if (Math.Cos(phase + wait * omega) < 0)
                node = -node;
            tangent = Vector3d.Cross(origin.normalVector, node);

            double tilt = Math.Atan2(Vector3d.Dot(node, Vector3d.Cross(origin.normalVector, destination.normalVector)),
                Vector3d.Dot(origin.normalVector, destination.normalVector));
            var template = new FiniteThrustPlaneChangeSegment
            {
                barycenter = body,
                epochTime = launch,
                startTime = launch,
                nodeDirection = node,
                departureTangent = tangent,
                referenceRadius_m = radius,
                gravitationalParameter = body.mu,
                planeAngle_rad = tilt,
                fleetCruiseAcceleration_mps2 = acceleration
            };

            bool rendezvous = !(target is TIOrbitState);
            double phaseGain = 0.0;
            if (rendezvous)
            {
                // The phase equation assumes equal circular periods.
                if (Math.Abs(destination.semiMajorAxis_m - radius) > 0.001)
                    return new SearchFailure();
                ((ITransferTarget)target).getOrbitalElementsState(launch, out var targetOrbit, out var center, out _);
                if (center != body)
                    return new SearchFailure();
                Vector3d targetPosition = targetOrbit.ToCartesianStateAtTime(launch.ExportTime(), body.mass_kg).position;
                Vector3d targetTangent = Vector3d.Cross(destination.normalVector, node);
                phaseGain = PositiveModulo(-Math.Atan2(Vector3d.Dot(targetPosition, targetTangent),
                    Vector3d.Dot(targetPosition, node)), 2 * Math.PI);
                if (phaseGain > 2 * Math.PI - 1e-10)
                    phaseGain = 0;
            }

            double availableTime = DurationCap(fleet.faction) - launch.DifferenceInSeconds(now);
            int maxHalfTurns = (int)Math.Min(32768, Math.Floor(availableTime * omega / Math.PI));
            var frontier = new List<FiniteThrustPlaneChangeSegment>();
            double maxRaise = maximumRadius - radius;
            foreach (double nodeWeight in NodeWeights)
            {
                template.nodeWeight = nodeWeight;
                foreach (double weight in InclinationWeights)
                {
                    // A fixed-phase target can gain one or more extra revolutions.
                    // A free-phase target can instead sample the temporary radius.
                    if (rendezvous)
                    {
                        for (int winding = 0; winding <= 3; winding++)
                            SampleFamily(template, frontier, acceleration, availableTime, maxHalfTurns,
                                weight, maxRaise, phaseGain + 2 * Math.PI * winding);
                    }
                    else
                    {
                        foreach (double raise in Raises)
                        {
                            if (radius * raise <= maxRaise)
                                SampleFamily(template, frontier, acceleration, availableTime, maxHalfTurns,
                                    weight, radius * raise, null);
                        }
                    }
                }
            }

            double bestDv = double.PositiveInfinity;
            int initialCandidateCount = candidates.Count;
            foreach (var route in frontier)
            {
                bestDv = Math.Min(bestDv, route.DV_mps);
                if (route.DV_mps <= budget)
                {
                    var trajectory = BuildTrajectory(fleet, target, route, acceleration);
                    if (trajectory != null)
                        candidates.Add(trajectory);
                }
            }
            if (double.IsPositiveInfinity(bestDv))
                return new SearchFailure();

            if (bestDv <= budget && candidates.Count == initialCandidateCount)
                return new SearchFailure();

            lowestDv_kps = Math.Min(lowestDv_kps, bestDv / 1000.0);
            return bestDv <= budget
                ? new TransferResult(TransferResult.Outcome.Success)
                : new TransferResult(TransferResult.Outcome.Fail_InsufficientDV, bestDv);
        }

        private static void SampleFamily(
            FiniteThrustPlaneChangeSegment template, List<FiniteThrustPlaneChangeSegment> frontier, double acceleration,
            double availableTime, int maxHalfTurns, double weight, double maxRaise, double? phaseGain)
        {
            double baseAngularSpeed = Math.Sqrt(template.gravitationalParameter / Math.Pow(template.referenceRadius_m, 3));
            int first = 1;
            int last = maxHalfTurns;
            while (first <= last)
            {
                int middle = first + (last - first) / 2;
                if (Configure(template, middle, weight, maxRaise, phaseGain, baseAngularSpeed, acceleration))
                    last = middle - 1;
                else
                    first = middle + 1;
            }

            int previous = 0;
            foreach (double multiplier in DurationMultipliers)
            {
                int turns = (int)Math.Ceiling(first * multiplier);
                if (turns > maxHalfTurns)
                    break;
                if (turns == previous)
                    continue;
                previous = turns;
                if (!Configure(template, turns, weight, maxRaise, phaseGain, baseAngularSpeed, acceleration))
                    continue;

                var path = template.Build(turns);
                if (path.Duration_s > availableTime)
                    break;
                if (frontier.Exists(other => other.Duration_s <= path.Duration_s && other.DV_mps <= path.DV_mps))
                    continue;
                frontier.RemoveAll(other => path.Duration_s <= other.Duration_s && path.DV_mps <= other.DV_mps);
                frontier.Add(path);
            }
        }

        private static bool Configure(
            FiniteThrustPlaneChangeSegment path, int halfTurns, double weight, double maxRaise, double? phaseGain,
            double baseAngularSpeed, double acceleration)
        {
            path.angularSpan_rad = halfTurns * Math.PI;
            path.inclinationWeight = weight;
            path.raise_m = maxRaise;
            if (phaseGain.HasValue)
            {
                double requiredTime = (path.angularSpan_rad + phaseGain.Value) / baseAngularSpeed;
                if (path.TotalTime() + 1e-8 < requiredTime)
                    return false;
                double min = 0;
                double max = maxRaise;
                for (int i = 0; i < 40; i++)
                {
                    path.raise_m = (min + max) * 0.5;
                    if (path.TotalTime() < requiredTime)
                        min = path.raise_m;
                    else
                        max = path.raise_m;
                }
            }
            path.peakThrustBound_mps2 = path.PeakThrustBound();
            return path.peakThrustBound_mps2 <= acceleration;
        }

        private static Trajectory_Patched BuildTrajectory(
            IMobileAsset fleet, TISpaceGameState target, FiniteThrustPlaneChangeSegment path, double acceleration)
        {
            var trajectory = new Trajectory_Patched();
            trajectory.BuildSingleTrajectory_Common(fleet, target, path.barycenter, path.startTime, path.Duration_s);
            if (trajectory.launchTime != path.startTime)
                return null;
            path.endTime = trajectory.arrivalTime;
            trajectory.Segments = new List<Trajectory_Patched.IPatchSegment> { path };
            var builder = Traverse.Create(trajectory);
            builder.Property(nameof(Trajectory.duration)).SetValue(TimeSpan.FromSeconds(trajectory.duration_s));
            builder.Property(nameof(Trajectory.fleetCruiseAcceleration_mps2)).SetValue(acceleration);
            builder.Property(nameof(Trajectory.boostDV_mps)).SetValue(path.DV_mps);
            builder.Property(nameof(Trajectory.destinationPosition)).SetValue(path.GlobalPositionAtTime(path.endTime));
            return trajectory;
        }

        private static double PositiveModulo(double value, double modulus)
        {
            double result = value % modulus;
            return result < 0 ? result + modulus : result;
        }

        private sealed class SearchFailure : TransferResult
        {
            internal SearchFailure() : base(Outcome.Fail_OrbitPeriod) { }

            public override string ToString() => Loc.T("UI.TransferResult.EEO_NoFeasiblePlaneChange");
        }
    }

    // FullSerializer records the fields of this segment inside the existing
    // Trajectory_Patched. One representation drives the trail, state and fuel.
    public sealed class FiniteThrustPlaneChangeSegment : Trajectory_Patched.MicrothrustSegment,
        Trajectory_Patched.IPatchSegment
    {
        private const int TimePanels = 16;
        private static readonly double[] GaussNodes = { 0.1834346424956498, 0.5255324099163290, 0.7966664774136267, 0.9602898564975363 };
        private static readonly double[] GaussWeights = { 0.3626837833783620, 0.3137066458778873, 0.2223810344533745, 0.1012285362903763 };

        public Vector3d nodeDirection;
        public Vector3d departureTangent;
        public double referenceRadius_m;
        public double gravitationalParameter;
        public double angularSpan_rad;
        public double planeAngle_rad;
        public double raise_m;
        public double inclinationWeight;
        public double nodeWeight;
        public double peakThrustBound_mps2;
        public TIDateTime profileEndTime;
        public double[] elapsedTimes_s;
        public double[] integratedDv_mps;

        internal double Duration_s => elapsedTimes_s[TimePanels];

        public override double DV_mps => DVConsumedByTime(endTime);

        public override double DVConsumedByTime(TIDateTime time) =>
            IntegratedDv(FractionAt(time)) - IntegratedDv(FractionAt(startTime));

        public override CartesianState GlobalCartesianStateAtTime(TIDateTime time) =>
            LocalState(FractionAt(time)).ToGlobal(barycenter, time);

        public override OrbitalElementsState OrbitalElementsAtTime(TIDateTime time) =>
            LocalState(FractionAt(time)).ToOrbitalElementsState(gravitationalParameter, time.ExportTime());

        double Trajectory_Patched.IPatchSegment.getDistFromBarycenterAtTime_m(
            TIDateTime time, out TINaturalSpaceObjectState center)
        {
            center = barycenter;
            return Radius(FractionAt(time));
        }

        public override Trajectory_Patched.ISupportsReducedCopy ReducedCopy(TIDateTime start, TIDateTime end)
        {
            var copy = (FiniteThrustPlaneChangeSegment)MemberwiseClone();
            copy.startTime = start;
            copy.endTime = end;
            return copy;
        }

        public override bool isPlausible(IMobileAsset fleet) =>
            peakThrustBound_mps2 <= fleet.cruiseAcceleration_mps2 && endTime >= startTime;

        public override string DumpSegment() => "finite thrust plane change," + DV_mps;

        public override string deepDump() => DumpSegment();

        internal FiniteThrustPlaneChangeSegment Build(int halfTurns)
        {
            var path = (FiniteThrustPlaneChangeSegment)MemberwiseClone();
            path.elapsedTimes_s = new double[TimePanels + 1];
            for (int i = 0; i < TimePanels; i++)
                path.elapsedTimes_s[i + 1] = path.elapsedTimes_s[i] +
                    TimeIntegral(i / (double)TimePanels, (i + 1) / (double)TimePanels);
            path.profileEndTime = path.endTime = new TIDateTime(path.epochTime, path.Duration_s);

            // Split the thrust integral at the nodes and quadratures.
            int intervals = 2 * halfTurns;
            path.integratedDv_mps = new double[intervals + 1];
            for (int i = 0; i < intervals; i++)
                path.integratedDv_mps[i + 1] = path.integratedDv_mps[i] +
                    Integrate(i / (double)intervals, (i + 1) / (double)intervals, thrust: true);
            return path;
        }

        internal double TotalTime()
        {
            double total = 0;
            for (int i = 0; i < TimePanels; i++)
                total += TimeIntegral(i / (double)TimePanels, (i + 1) / (double)TimePanels);
            return total;
        }

        private double TimeIntegral(double from, double to) => Integrate(from, to, thrust: false);

        internal double PeakThrustBound()
        {
            double peak = 0;
            double span = angularSpan_rad;
            double radiusSlope = Math.PI * raise_m / span;
            double radiusCurvature = 2 * Math.PI * Math.PI * raise_m / (span * span);
            double maxPhaseSlope = (1 + nodeWeight) / span;
            double envelopeCurvature = 2 * Math.PI * Math.Abs(planeAngle_rad) * inclinationWeight *
                maxPhaseSlope * maxPhaseSlope;
            for (int i = 0; i < TimePanels; i++)
            {
                double left = i / (double)TimePanels;
                double right = (i + 1) / (double)TimePanels;
                // Radius and tilt slope are monotone on each half of the path.
                double minRadius = Math.Min(Radius(left), Radius(right));
                double maxRadius = Math.Max(Radius(left), Radius(right));
                double tiltSlope = Math.Max(Math.Abs(TiltEnvelope(left)), Math.Abs(TiltEnvelope(right))) * maxPhaseSlope;
                double tiltSquared = maxRadius * tiltSlope * tiltSlope;
                double radial = radiusCurvature + 1.5 * radiusSlope * radiusSlope / minRadius + tiltSquared;
                double tangential = 0.5 * (radiusSlope + tiltSquared);
                // Combine the oscillatory slope and curvature terms before
                // bounding them. Their sum peaks at a node for both laws.
                double normal = 0.5 * radiusSlope * tiltSlope + maxRadius * (2 * tiltSlope + envelopeCurvature);
                double omegaSquared = gravitationalParameter / (minRadius * minRadius * minRadius);
                peak = Math.Max(peak, omegaSquared * Math.Sqrt(radial * radial + tangential * tangential + normal * normal));
            }
            return peak;
        }

        internal double FractionAt(TIDateTime time)
        {
            // Use the rounded engine arrival instant for the final endpoint.
            TIDateTime clampedTime = time > endTime ? endTime : time;
            double elapsed = clampedTime.DifferenceInSeconds(epochTime);
            if (elapsed <= 0)
                return 0;
            if (elapsed >= Duration_s || clampedTime >= profileEndTime)
                return 1;
            int panel = Array.BinarySearch(elapsedTimes_s, elapsed);
            if (panel >= 0)
                return panel / (double)TimePanels;
            panel = ~panel - 1;
            double from = panel / (double)TimePanels;
            double relativeTime = elapsed - elapsedTimes_s[panel];
            double fraction = from + relativeTime / ((elapsedTimes_s[panel + 1] - elapsedTimes_s[panel]) * TimePanels);
            for (int i = 0; i < 3; i++)
                fraction += (relativeTime - TimeIntegral(from, fraction)) * Omega(Radius(fraction)) / angularSpan_rad;
            return fraction;
        }

        internal Vector3d PreviewPoint(double fraction)
        {
            int panel = Math.Min((int)(fraction * TimePanels), TimePanels - 1);
            double elapsed = elapsedTimes_s[panel] + TimeIntegral(panel / (double)TimePanels, fraction);
            var date = new TIDateTime(epochTime, elapsed);
            return LocalState(fraction).ToGlobal(barycenter, date).position;
        }

        private double IntegratedDv(double fraction)
        {
            int intervals = integratedDv_mps.Length - 1;
            int index = Math.Min((int)(fraction * intervals), intervals);
            return integratedDv_mps[index] + Integrate(index / (double)intervals, fraction, thrust: true);
        }

        private CartesianState LocalState(double fraction)
        {
            double phase = fraction * angularSpan_rad;
            double progress = TiltProgress(fraction);
            double tilt = planeAngle_rad * (progress - inclinationWeight * Math.Sin(2 * Math.PI * progress) / (2 * Math.PI));
            Vector3d normal = Vector3d.Cross(nodeDirection, departureTangent);
            Vector3d tangent = Math.Cos(tilt) * departureTangent + Math.Sin(tilt) * normal;
            normal = Vector3d.Cross(nodeDirection, tangent);
            double sine = Math.Sin(phase);
            double cosine = Math.Cos(phase);
            Vector3d direction = cosine * nodeDirection + sine * tangent;
            Vector3d alongTrack = -sine * nodeDirection + cosine * tangent;
            double radius = Radius(fraction);
            Vector3d velocityPerRadian = RadiusSlope(fraction) * direction +
                radius * (alongTrack + sine * TiltSlope(fraction) * normal);
            return new CartesianState(radius * direction, Omega(radius) * velocityPerRadian);
        }

        private double Thrust(double fraction, double radius, double omega)
        {
            // Differentiate the same position law used in LocalState and
            // subtract central gravity. Components use the moving orbit basis.
            double sine = Math.Sin(fraction * angularSpan_rad);
            double cosine = Math.Cos(fraction * angularSpan_rad);
            double span = angularSpan_rad;
            double slope = RadiusSlope(fraction);
            double tilt = TiltSlope(fraction);
            double radiusCurvature = 2 * Math.PI * Math.PI * raise_m * Math.Cos(2 * Math.PI * fraction) / (span * span);
            double phaseSlope = (1 + nodeWeight * Math.Cos(2 * fraction * span)) / span;
            double tiltCurvature = 2 * Math.PI * planeAngle_rad * inclinationWeight *
                Math.Sin(2 * Math.PI * TiltProgress(fraction)) * phaseSlope * phaseSlope -
                TiltEnvelope(fraction) * 2 * nodeWeight * Math.Sin(2 * fraction * span) / span;
            double radial = radiusCurvature - 1.5 * slope * slope / radius - radius * sine * sine * tilt * tilt;
            double tangential = 0.5 * slope - radius * sine * cosine * tilt * tilt;
            double normal = 0.5 * slope * sine * tilt + radius * (2 * cosine * tilt + sine * tiltCurvature);
            return omega * omega * Math.Sqrt(radial * radial + tangential * tangential + normal * normal);
        }

        private double Integrate(double from, double to, bool thrust)
        {
            if (from == to)
                return 0;
            double center = (from + to) * 0.5;
            double halfWidth = (to - from) * 0.5;
            double sum = 0;
            for (int i = 0; i < GaussNodes.Length; i++)
            {
                double offset = halfWidth * GaussNodes[i];
                sum += GaussWeights[i] * (Integrand(center - offset, thrust) + Integrand(center + offset, thrust));
            }
            return sum * halfWidth * angularSpan_rad;
        }

        private double Integrand(double fraction, bool thrust)
        {
            double radius = Radius(fraction);
            double omega = Omega(radius);
            return thrust ? Thrust(fraction, radius, omega) / omega : 1 / omega;
        }

        private double Radius(double fraction) =>
            referenceRadius_m + 0.5 * raise_m * (1 - Math.Cos(2 * Math.PI * fraction));

        private double RadiusSlope(double fraction) =>
            Math.PI * raise_m * Math.Sin(2 * Math.PI * fraction) / angularSpan_rad;

        // The second law concentrates normal thrust near the nodes. At fixed
        // radius its normal thrust varies with cos³(phase) instead of cos(phase).
        private double TiltProgress(double fraction) => fraction +
            nodeWeight * Math.Sin(2 * fraction * angularSpan_rad) / (2 * angularSpan_rad);

        private double TiltEnvelope(double fraction) =>
            planeAngle_rad * (1 - inclinationWeight * Math.Cos(2 * Math.PI * TiltProgress(fraction)));

        private double TiltSlope(double fraction) => TiltEnvelope(fraction) *
            (1 + nodeWeight * Math.Cos(2 * fraction * angularSpan_rad)) / angularSpan_rad;

        private double Omega(double radius) => Math.Sqrt(gravitationalParameter / (radius * radius * radius));
    }
}
