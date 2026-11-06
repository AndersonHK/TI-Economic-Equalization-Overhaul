using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Unity.Jobs;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Explicit CLI-only test entry point. Never loads a campaign or changes a save.
    public sealed class ContactHeadlessHarness : MonoBehaviour
    {
        public static void InstallIfRequested()
        {
            if(!Application.isBatchMode||Array.IndexOf(Environment.GetCommandLineArgs(),"--eeo-contact-headless")<0)return;
            var go=new GameObject("EEO headless contact verification");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<ContactHeadlessHarness>();
        }
        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(5);
            int exit=1;
            try
            {
                WaypointLifetimeChecks.Run();
                ContactJobSelfTest.RunOnce();
                ContactJobRuntime.Warmup();
                Lifecycle();
                var reports=new List<object>();
                reports.Add(Benchmark(32,256,false));reports.Add(Benchmark(32,48,true));reports.Add(Benchmark(64,1024,false));
                reports.Add(Benchmark(32,500,false,true));reports.Add(Benchmark(64,1000,false,true));reports.Add(Benchmark(32,48,true,true));
                string json=Newtonsoft.Json.JsonConvert.SerializeObject(new {version=Main.mod.Info.Version,unity=Application.unityVersion,
                    description="Actual Unity job runtime, synthetic geometry and poses. Immediate completion includes all worker waits. Not a rendered battle or GPU/FPS measurement.",reports},Newtonsoft.Json.Formatting.Indented);
                Main.Log("HEADLESS CONTACT RESULTS\n"+json);
                string[] args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"--eeo-contact-report");
                if(arg>=0&&arg+1<args.Length)File.WriteAllText(args[arg+1],json);
                exit=0;
            }
            catch(Exception e){Main.Warn("HEADLESS CONTACT FAILED: "+e);}
            finally{ContactJobRuntime.Clear();Main.Log("HEADLESS CONTACT EXIT "+exit);Application.Quit(exit);}
        }
        private static void Lifecycle()
        {
            var target=new GameObject("Contact lifecycle target");var child=new GameObject("Contact lifecycle identity");
            var shot=new GameObject("Contact lifecycle flight");child.transform.SetParent(target.transform,false);
            BoxCollider identity=child.AddComponent<BoxCollider>();identity.enabled=false;
            target.transform.position=new Vector3(10,0,0);shot.transform.position=new Vector3(20,0,0);
            try
            {
                int id=ContactJobRuntime.Primitive(identity,42,2,true,target.transform),pose=ContactJobRuntime.Pose(shot.transform);
                ContactJobRuntime.FlightPoses(pose,-1,1);ContactJobRuntime.BeginFlights(1);
                ContactJobRuntime.SetFlight(0,new ContactFlight{Pose=pose,ParentPose=-1,Body=99,Faction=1,Mask=1,Live=1,MaximumRange=10000});
                ContactJobRuntime.Schedule(default(JobHandle));ContactJobRuntime.Complete();ContactResult first=ContactJobRuntime.Result(0);
                if(first.Surface!=id||Math.Abs(first.Point.x-9.5f)>.0001f)throw new InvalidOperationException("Initial lifecycle contact failed.");
                var proxy=CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1);
                if(proxy.Length!=1||Math.Abs(proxy[0].point.x-9.5f)>.0001f)throw new InvalidOperationException("Indexed compatibility contact failed.");
                target.transform.position=new Vector3(30,0,0);CompatibilityContactWorld.Invalidate();
                if(CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1).Length!=0)throw new InvalidOperationException("Compatibility phase used stale poses.");
                target.transform.position=new Vector3(10,0,0);CompatibilityContactWorld.Invalidate();
                CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1);
                int count=ContactJobRuntime.SurfaceCount;
                ContactJobRuntime.SetLive(id,false);
                int snapshots=CombatContactMetrics.CompatibilitySnapshots;
                if(CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1).Length!=0||CombatContactMetrics.CompatibilitySnapshots!=snapshots)
                    throw new InvalidOperationException("Compatibility removal required a scan or retained a stale target.");
                ContactJobRuntime.Schedule(default(JobHandle));ContactJobRuntime.Complete();
                if(ContactJobRuntime.Result(0).Surface>=0||ContactJobRuntime.IsCurrent(id,first.Generation))throw new InvalidOperationException("Inactive/stale contact survived.");
                ContactJobRuntime.ReusePrimitive(id,identity,1,target.transform);ContactJobRuntime.Schedule(default(JobHandle));ContactJobRuntime.Complete();
                if(ContactJobRuntime.Result(0).Surface>=0)throw new InvalidOperationException("Pooled faction change was ignored.");
                if(CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1).Length!=0)throw new InvalidOperationException("Compatibility faction filter failed.");
                target.transform.position=new Vector3(12,0,0);child.transform.localScale=new Vector3(2,3,4);
                ContactJobRuntime.ReusePrimitive(id,identity,2,target.transform);ContactJobRuntime.Schedule(default(JobHandle));ContactJobRuntime.Complete();
                ContactResult reused=ContactJobRuntime.Result(0);
                if(reused.Surface!=id||Math.Abs(reused.Point.x-11)>.0001f||!ContactJobRuntime.IsCurrent(id,reused.Generation)||ContactJobRuntime.SurfaceCount!=count)
                    throw new InvalidOperationException("Pooled geometry/generation/pose reuse failed.");
                proxy=CompatibilityContactWorld.Cast(Vector3.zero,Vector3.right,20,1,1);
                if(proxy.Length!=1||Math.Abs(proxy[0].point.x-11)>.0001f)throw new InvalidOperationException("Compatibility reuse retained old bounds.");
                Main.Log("PASS: native contact lifecycle: reserved-slot reuse, activation/deactivation, stale generations, faction changes, collapsed child scale and updated poses.");
            }
            finally{ContactJobRuntime.Clear();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(shot);}
        }
        private static object Benchmark(int ships,int projectiles,bool clustered,bool compatibility=false)
        {
            const int parts=40,frames=160,warmup=24;
            var objects=new List<GameObject>();var roots=new Transform[ships];var ends=new Transform[projectiles];
            ContactFlight[] inputs=new ContactFlight[projectiles];
            var points=new List<Vector3>();var triangles=new List<int>();
            // 2,048 triangles per shared planar asset: enough to exercise BVH
            // traversal rather than benchmark only a single primitive.
            for(int y=0;y<32;y++)for(int z=0;z<32;z++)
            {
                int n=points.Count;float a=y/16f-1,b=z/16f-1;
                points.Add(new Vector3(0,a,b));points.Add(new Vector3(0,a+.0625f,b));
                points.Add(new Vector3(0,a,b+.0625f));points.Add(new Vector3(0,a+.0625f,b+.0625f));
                triangles.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});
            }
            var tree=new RenderedMeshRaycast(points.ToArray(),triangles.ToArray());
            var weights=new BoneWeight[points.Count];var bind=new Matrix4x4[20];
            for(int i=0;i<20;i++)bind[i]=Matrix4x4.identity;
            for(int i=0;i<weights.Length;i++)
            {
                int bone=(i/4)%20;bool mixed=(i/4)%128==0;
                weights[i]=new BoneWeight{boneIndex0=bone,boneIndex1=(bone+1)%20,weight0=mixed?.7f:1,weight1=mixed?.3f:0};
            }
            var skin=new PreparedSkinnedContact(points.ToArray(),triangles.ToArray(),weights,bind,4);
            try
            {
                for(int b=0;b<ships;b++)
                {
                    GameObject root=new GameObject("Contact benchmark body");objects.Add(root);roots[b]=root.transform;
                    roots[b].position=new Vector3(10+(b%8)*5,(b/8)*5,0);
                    for(int p=0;p<parts;p++)
                    {
                        GameObject part=new GameObject("Prepared part");part.transform.SetParent(root.transform,false);
                        part.transform.localPosition=new Vector3((p%5)*.02f,((p/5)%4)*.2f,(p/20)*.2f);
                        part.transform.localScale=Vector3.one*.1f;
                        BoxCollider identity=part.AddComponent<BoxCollider>();identity.enabled=false;
                        ContactJobRuntime.Static(tree,part.transform,identity,b+1,2,0,true);
                    }
                    for(int p=0;p<4;p++)
                    {
                        GameObject part=new GameObject("Prepared skin");part.transform.SetParent(root.transform,false);
                        part.transform.localPosition=new Vector3(-.1f+p*.02f,0,0);part.transform.localScale=Vector3.one*.1f;
                        BoxCollider identity=part.AddComponent<BoxCollider>();identity.enabled=false;
                        var bones=new Transform[20];
                        for(int j=0;j<bones.Length;j++)
                        {
                            GameObject bone=new GameObject("Prepared bone");bone.transform.SetParent(part.transform,false);
                            bone.transform.localRotation=Quaternion.Euler(0,j*.01f,j*.02f);bones[j]=bone.transform;
                        }
                        ContactJobRuntime.Skin(skin,bones,identity,b+1,2,0,true);
                    }
                }
                ContactJobRuntime.ReserveProjectiles();
                for(int i=0;i<projectiles;i++)
                {
                    GameObject shot=new GameObject("Contact benchmark round");objects.Add(shot);ends[i]=shot.transform;
                    Vector3 start=clustered?new Vector3(0,.01f,.01f):new Vector3(0,(i%32)*.2f,3);
                    ends[i].position=start+new Vector3(60,0,0);
                    BoxCollider round=shot.AddComponent<BoxCollider>();round.enabled=false;round.size=Vector3.one*.0001f;
                    ContactJobRuntime.Primitive(round,100000+i,1,true);
                    inputs[i]=new ContactFlight{Pose=ContactJobRuntime.Pose(ends[i]),ParentPose=-1,Body=100000+i,Faction=1,
                        Mask=1,Live=1,MaximumRange=10000,PreviousLocal=start,Origin=start};
                }
                ContactJobRuntime.BeginFlights(projectiles);
                for(int i=0;i<projectiles;i++)ContactJobRuntime.SetFlight(i,inputs[i]);
                var samples=new double[frames];var dispatch=new double[frames];long hits=0;
                var method=typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread",Type.EmptyTypes);
                Func<long> allocated=method==null?null:(Func<long>)Delegate.CreateDelegate(typeof(Func<long>),method);
                long allocationStart=0,allocationEnd=0;
                for(int frame=-warmup;frame<frames;frame++)
                {
                    for(int b=0;b<ships;b++)roots[b].position=new Vector3(10+(b%8)*5,(b/8)*5+(float)Math.Sin(frame*.02)*.001f,0);
                    if(frame==0&&allocated!=null)allocationStart=allocated();
                    long start=Stopwatch.GetTimestamp();
                    long queued;
                    if(compatibility)
                    {
                        CompatibilityContactWorld.Invalidate();
                        for(int i=0;i<projectiles;i++)
                        {
                            RaycastHit[] hit=clustered?
                                CompatibilityContactWorld.Cast(new Vector3(0,.01f,.01f),Vector3.right,11,1,1):
                                PreparedContactWorld.RaycastNearest(new Vector3(0,(i%32)*.2f,3),Vector3.right,1,1,1);
                            hits+=hit.Length;
                        }
                        queued=Stopwatch.GetTimestamp();
                    }
                    else
                    {
                        ContactJobRuntime.BeginFlights(projectiles);
                        for(int i=0;i<projectiles;i++)ContactJobRuntime.SetFlight(i,inputs[i]);
                        ContactJobRuntime.Schedule(default(JobHandle));queued=Stopwatch.GetTimestamp();
                        ContactJobRuntime.Complete();
                        for(int i=0;i<projectiles;i++)if(ContactJobRuntime.Result(i).Surface>=0)hits++;
                    }
                    long end=Stopwatch.GetTimestamp();
                    if(frame>=0){samples[frame]=(end-start)*1000.0/Stopwatch.Frequency;dispatch[frame]=(queued-start)*1000.0/Stopwatch.Frequency;}
                }
                if(allocated!=null)allocationEnd=allocated();
                Array.Sort(samples);Array.Sort(dispatch);double sum=0,queue=0;foreach(double x in samples)sum+=x;foreach(double x in dispatch)queue+=x;
                if(clustered&&hits==0)throw new InvalidOperationException("Clustered benchmark did not exercise contacts.");
                if(!clustered&&hits!=0)throw new InvalidOperationException("Distant benchmark admitted an off-path contact.");
                return new {queryPath=compatibility?(clustered?"indexed compatibility arrivals":"production missile no-contact wrapper"):"ballistic jobs",ships,projectiles,staticPartsPerShip=parts,skinsPerShip=4,bonesPerSkin=20,trianglesPerAsset=2048,clustered,frames,
                    totalMeanMs=sum/frames,totalP50Ms=samples[frames/2],totalP95Ms=samples[(int)(frames*.95)],totalMaxMs=samples[frames-1],
                    dispatchMeanMs=queue/frames,dispatchP95Ms=dispatch[(int)(frames*.95)],
                    mainAllocatedBytes=allocated==null?(long?)null:allocationEnd-allocationStart,
                    contactsIncludingWarmup=hits,activePoses=ContactJobRuntime.ActivePoseCount,surfaces=ContactJobRuntime.SurfaceCount,
                    isolatedStageMeanMsPoseIndexQuery=compatibility?null:ContactJobRuntime.DiagnosticStages()};
            }
            finally{ContactJobRuntime.Clear();foreach(GameObject go in objects)UnityEngine.Object.Destroy(go);}
        }
    }
}
