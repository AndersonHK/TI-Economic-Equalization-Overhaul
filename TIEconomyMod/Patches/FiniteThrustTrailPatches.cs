using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems;
using PavonisInteractive.TerraInvicta.Systems.Camera;
using PavonisInteractive.TerraInvicta.Systems.SolarSystem;
using TIEconomyMod.Core;
using UnityEngine;
using Vectrosity;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(OrbitExtensions), nameof(OrbitExtensions.FillTransferOrbit))]
    internal static class FiniteThrustTrailRegistrationPatch
    {
        internal static readonly ConditionalWeakTable<VectorLine, FiniteThrustTrail> Trails =
            new ConditionalWeakTable<VectorLine, FiniteThrustTrail>();

        private static void Postfix(Trajectory trajectory, Orbit __result)
        {
            if (__result.OrbitTrail != null && trajectory is Trajectory_Patched patched &&
                patched.Segments.Count == 1 && patched.Segments[0] is FiniteThrustPlaneChangeSegment path)
                Trails.Add(__result.OrbitTrail, new FiniteThrustTrail(path));
        }
    }

    [HarmonyPatch(typeof(OrbitTrailRendering), "DrawTransferOrbit")]
    internal static class FiniteThrustTrailRenderingPatch
    {
        private static bool Prefix(FleetTransferPlan plan, int segment, SpaceObjectLOD LOD, CameraManager ___cameraManager)
        {
            VectorLine line = plan.TransferSegments[segment].OrbitTrail;
            if (line == null || !FiniteThrustTrailRegistrationPatch.Trails.TryGetValue(line, out var trail))
                return true;

            TIDateTime now = TITimeState.Now();
            line.active = now < trail.EndTime && (LOD.DisplayOrbitTrail || plan.planningOnly);
            if (line.active && line.rectTransform != null)
            {
                trail.Draw(line, ___cameraManager, now);
                if (line.rectTransform != null)
                    GameControl.solarSystem.AddOrbitTrailToContainer(line.rectTransform.gameObject);
            }
            return false;
        }
    }

    internal sealed class FiniteThrustTrail
    {
        private const int SamplesPerRevolution = 24;
        private const int MaxSamples = 1024;

        private readonly FiniteThrustPlaneChangeSegment path;
        private readonly int subdivisions;
        private readonly Vector3d[] points;
        private int firstSample = -1;
        private int lastSample;

        internal TIDateTime EndTime => path.endTime;

        internal FiniteThrustTrail(FiniteThrustPlaneChangeSegment path)
        {
            this.path = path;
            subdivisions = Math.Max(1, (int)Math.Ceiling(path.angularSpan_rad * SamplesPerRevolution / (2 * Math.PI)));
            points = new Vector3d[Math.Min(MaxSamples, subdivisions + 1)];
        }

        internal void Draw(VectorLine line, CameraManager camera, TIDateTime now)
        {
            TIDateTime visibleFrom = now < path.startTime ? path.startTime : now;
            double start = path.FractionAt(visibleFrom);
            double end = path.FractionAt(path.endTime);
            int next = (int)Math.Floor(start * subdivisions) + 1;
            int through = (int)Math.Ceiling(end * subdivisions) - 1;
            int batch = (next - 1) / SamplesPerRevolution * SamplesPerRevolution;
            if (batch != firstSample)
            {
                firstSample = batch;
                lastSample = Math.Min(batch + points.Length - 1, through);
                for (int sample = batch; sample <= lastSample; sample++)
                    points[sample - batch] = path.PreviewPoint(sample / (double)subdivisions);
            }

            int count = Math.Max(0, lastSample - next + 1);
            bool includeEnd = lastSample == through;
            line.Resize(1 + count + (includeEnd ? 1 : 0));
            line.points3[0] = camera.ScaledPosition(path.GlobalPositionAtTime(visibleFrom));
            for (int i = 0; i < count; i++)
                line.points3[i + 1] = camera.ScaledPosition(points[next + i - firstSample]);
            if (includeEnd)
                line.points3[count + 1] = camera.ScaledPosition(path.GlobalPositionAtTime(path.endTime));
            line.Draw3D();
        }
    }
}
