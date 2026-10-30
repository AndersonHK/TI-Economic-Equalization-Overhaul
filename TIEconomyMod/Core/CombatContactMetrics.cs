using System;
using System.Diagnostics;
using PavonisInteractive.TerraInvicta;
using UnityEngine;
using Unity.Entities;
using PavonisInteractive.TerraInvicta.Systems.GameTime;

namespace TIEconomyMod.Core
{
    public static class CombatContactMetrics
    {
        public static int ColliderConfigurations, BvhBuilds, BvhNodes, TriangleTests, Queries, Candidates, Contacts, BonePoseReads, BodyChecks, BodyCandidates, VisualBoundsScans, BoundEvaluations, MixedVertexReads;
        private static readonly long[] ticks = new long[4];
        private static int slowReports;
        private static double maxPassMs;
        public static long Begin() { return Stopwatch.GetTimestamp(); }
        public static void End(int stage,long start) { ticks[stage]+=Stopwatch.GetTimestamp()-start; }
        public static void ReportSlowPass(long start)
        {
            double ms=(Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency;
            maxPassMs=Math.Max(maxPassMs,ms);
            if(ms<50||slowReports>=32)return;
            slowReports++;
            Main.Log("Slow ballistic contact pass: ms="+ms.ToString("F2")+
                " bvhBuilds="+BvhBuilds+" boundEvals="+BoundEvaluations+" mixedVertexReads="+MixedVertexReads+" boundsScans="+VisualBoundsScans+" bodyChecks="+BodyChecks+" bodyCandidates="+BodyCandidates+" bonePoseReads="+BonePoseReads+" queries="+Queries+
                " candidates="+Candidates+" nodes="+BvhNodes+" triangles="+TriangleTests);
        }
        public static string Take(float seconds,int frames)
        {
            double unit=1000.0/Stopwatch.Frequency/Math.Max(1,frames);
            string text="fps="+(frames/Math.Max(.001f,seconds)).ToString("F1")+" collisionMs/frame="+(ticks[0]*unit).ToString("F3")+
                " bvhBuildMs/frame="+(ticks[1]*unit).ToString("F3")+" syncMs/frame="+(ticks[2]*unit).ToString("F3")+
                " poseSnapshotMs/frame="+(ticks[3]*unit).ToString("F3")+" maxPassMs="+maxPassMs.ToString("F2")+
                " boundEvals="+BoundEvaluations+" mixedVertexReads="+MixedVertexReads+" boundsScans="+VisualBoundsScans+" bodyChecks="+BodyChecks+" bodyCandidates="+BodyCandidates+" bonePoseReads="+BonePoseReads+" colliderConfigs="+ColliderConfigurations+" queries="+Queries+" candidates="+Candidates+" contacts="+Contacts+
                " bvhBuilds="+BvhBuilds+" nodes="+BvhNodes+" triangles="+TriangleTests+" gc0="+GC.CollectionCount(0);
            Array.Clear(ticks,0,ticks.Length);ColliderConfigurations=BvhBuilds=BvhNodes=TriangleTests=Queries=Candidates=Contacts=BonePoseReads=BodyChecks=BodyCandidates=VisualBoundsScans=BoundEvaluations=MixedVertexReads=0;maxPassMs=0;
            return text;
        }
    }
    public sealed class CombatContactFrameProbe : MonoBehaviour
    {
        private float elapsed; private int frames,reports;
        private GameTimeManager gameTime;
        private void Awake(){if(World.Active!=null)gameTime=World.Active.GetExistingManager<GameTimeManager>();}
        public void Arm(){elapsed=0;frames=reports=0;enabled=true;}
        private void Update()
        {
            elapsed+=Time.unscaledDeltaTime;frames++;
            if(elapsed<5f)return;
            Main.Log("Combat contact performance: speed="+(gameTime==null?"unknown":gameTime.currentSpeed.ToString("G5"))+" paused="+(gameTime!=null&&gameTime.Paused)+" "+CombatContactMetrics.Take(elapsed,frames));
            elapsed=0;frames=0;if(++reports>=120)enabled=false;
        }
    }
}
