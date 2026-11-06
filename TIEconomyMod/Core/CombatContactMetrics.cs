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
        private static readonly long[] ticks = new long[16];
        public static int CompatibilityRays,CompatibilitySnapshots,CompatibilityFallbackQueries;
        private static readonly double[] frameSamples=new double[4096];
        private static int frameSampleCount;
        private static double frameMaximum;
        public static void RecordFrame(long duration)
        {double ms=duration*1000.0/Stopwatch.Frequency;frameMaximum=Math.Max(frameMaximum,ms);if(frameSampleCount<frameSamples.Length)frameSamples[frameSampleCount++]=ms;}
        private static readonly double[] mainSamples=new double[8192];
        private static int sampleCount;
        public static void RecordMain(long elapsed)
        {if(sampleCount<mainSamples.Length)mainSamples[sampleCount++]=elapsed*1000.0/Stopwatch.Frequency;}
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
            Array.Sort(mainSamples,0,sampleCount);
            text+=" mainCollisionMs/frame="+((ticks[0]+ticks[4]+ticks[14]+ticks[15])*unit).ToString("F4")+
                " prepareMs/frame="+(ticks[4]*unit).ToString("F4")+" waitMs/frame="+(ticks[5]*unit).ToString("F4")+
                " commitMs/frame="+(ticks[6]*unit).ToString("F4")+" nativeLateBoundaryMs/frame="+(ticks[7]*unit).ToString("F4")+
                " compatibilityMs/frame="+(ticks[8]*unit).ToString("F4")+
                " boundsScanMs/frame="+(ticks[9]*unit).ToString("F4")+" ballisticSpawnMs/frame="+(ticks[10]*unit).ToString("F4")+
                " mainGeometryMs/frame="+((ticks[0]+ticks[4]+ticks[14]+ticks[15]+ticks[9]+ticks[10])*unit).ToString("F4")+
                " compatibilitySnapshotMs/frame="+(ticks[11]*unit).ToString("F4")+
                " compatibilityNativeMs/frame="+(ticks[12]*unit).ToString("F4")+" compatibilityBridgeMs/frame="+(ticks[13]*unit).ToString("F4")+
                " compatibilityRays="+CompatibilityRays+" compatibilitySnapshots="+CompatibilitySnapshots+" compatibilityFallbacks="+CompatibilityFallbackQueries+
                " ballisticBatchP95Ms="+(sampleCount==0?0:mainSamples[Math.Min(sampleCount-1,(int)(sampleCount*.95))]).ToString("F4")+
                " batchMaxMs="+(sampleCount==0?0:mainSamples[sampleCount-1]).ToString("F4")+
                " workerSurfaces="+ContactJobRuntime.SurfaceCount+" workerPoses="+ContactJobRuntime.ActivePoseCount+" queryBackend=native serialCountersOnly=True";
            sampleCount=0;
            Array.Sort(frameSamples,0,frameSampleCount);
            text+=" wallFrameMeanMs="+(seconds*1000/Math.Max(1,frames)).ToString("F3")+" wallFrameP95Ms="+
                (frameSampleCount==0?0:frameSamples[Math.Min(frameSampleCount-1,(int)(frameSampleCount*.95))]).ToString("F3")+" wallFrameMaxMs="+frameMaximum.ToString("F3");
            frameSampleCount=0;frameMaximum=0;CompatibilityRays=CompatibilitySnapshots=CompatibilityFallbackQueries=0;
            Array.Clear(ticks,0,ticks.Length);ColliderConfigurations=BvhBuilds=BvhNodes=TriangleTests=Queries=Candidates=Contacts=BonePoseReads=BodyChecks=BodyCandidates=VisualBoundsScans=BoundEvaluations=MixedVertexReads=0;maxPassMs=0;
            return text;
        }
    }
    public sealed class CombatContactFrameProbe : MonoBehaviour
    {
        private float elapsed; private int frames,reports;
        private long lastFrame;
        private GameTimeManager gameTime;
        private void Awake(){if(World.Active!=null)gameTime=World.Active.GetExistingManager<GameTimeManager>();}
        public void Arm(){elapsed=0;frames=reports=0;lastFrame=0;enabled=true;}
        private void Update()
        {
            if(!TIGlobalValuesState.isSpaceCombatEnabled||GameControl.spaceCombat==null||!GameControl.spaceCombat.initialized)
            {elapsed=0;frames=0;lastFrame=0;return;}
            long now=Stopwatch.GetTimestamp();if(lastFrame==0){lastFrame=now;return;}
            long duration=now-lastFrame;lastFrame=now;
            elapsed+=(float)((double)duration/Stopwatch.Frequency);frames++;CombatContactMetrics.RecordFrame(duration);
            if(elapsed<5f)return;
            Main.Log("Combat contact performance: speed="+(gameTime==null?"unknown":gameTime.currentSpeed.ToString("G5"))+" paused="+(gameTime!=null&&gameTime.Paused)+" "+CombatContactMetrics.Take(elapsed,frames));
            elapsed=0;frames=0;if(++reports>=120)enabled=false;
        }
    }
}
