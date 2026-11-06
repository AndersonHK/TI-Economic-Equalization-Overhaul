using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace TIEconomyMod.Core
{
    // Once per process, during battle preparation. Exercises the actual installed
    // job runtime; headless arithmetic checks cannot establish this contract.
    public static class ContactJobSelfTest
    {
        private static bool passed;
        public static void RunOnce()
        {
            if(passed)return;
            const int count=128;
            Vector3[] triangle={new Vector3(0,-1,-1),new Vector3(0,1,-1),new Vector3(0,0,1)};
            int[] triangles={0,1,2};var builder=new ContactAssetBuilder();int rigid=builder.Add(new RenderedMeshRaycast(triangle,triangles));
            BoneWeight weight=new BoneWeight{boneIndex0=0,boneIndex1=1,weight0=.5f,weight1=.5f};
            var skin=new PreparedSkinnedContact(triangle,triangles,new[]{weight,weight,weight},new[]{Matrix4x4.identity,Matrix4x4.identity},4);
            int[] roots,bones;skin.Export(builder,out roots,out bones);int mixed=roots[0];
            GameObject[] objects=new GameObject[count+2];Transform[] transforms=new Transform[objects.Length];
            for(int i=0;i<objects.Length;i++){objects[i]=new GameObject("EEO contact validation");transforms[i]=objects[i].transform;}
            transforms[0].position=new Vector3(10,2,3);transforms[1].position=new Vector3(15,2,3);
            transforms[0].localScale=transforms[1].localScale=new Vector3(1,2,3);
            var access=new TransformAccessArray(transforms);
            var slots=new NativeArray<int>(objects.Length,Allocator.TempJob);
            var world=new NativeArray<Matrix4x4>(objects.Length,Allocator.TempJob);
            var inverse=new NativeArray<Matrix4x4>(objects.Length,Allocator.TempJob);
            var geometry=new NativeArray<ContactNode>(builder.Nodes.ToArray(),Allocator.TempJob);
            var vertices=new NativeArray<ContactVertex>(builder.Vertices.ToArray(),Allocator.TempJob);
            var indices=new NativeArray<int>(builder.Indices.ToArray(),Allocator.TempJob);
            var influences=new NativeArray<ContactInfluence>(builder.Influences.ToArray(),Allocator.TempJob);
            var envelopes=new NativeArray<ContactEnvelope>(builder.Envelopes.ToArray(),Allocator.TempJob);
            var surfaces=new NativeArray<ContactSurface>(2,Allocator.TempJob);
            var bounds=new NativeArray<Bounds>(2,Allocator.TempJob);
            var order=new NativeArray<int>(2,Allocator.TempJob);
            var nodes=new NativeArray<ContactWorldNode>(4,Allocator.TempJob);
            var flights=new NativeArray<ContactFlight>(count,Allocator.TempJob);
            var results=new NativeArray<ContactResult>(count,Allocator.TempJob);
            JobHandle handle=default(JobHandle);int checks=0;
            try
            {
                for(int i=0;i<objects.Length;i++)slots[i]=i;
                for(int backend=0;backend<2;backend++)for(int mode=0;mode<7;mode++)
                {
                    transforms[1].position=new Vector3(mode==6?20:15,2,3);
                    surfaces[0]=new ContactSurface{Root=mode==3||mode==6?mixed:rigid,Kind=mode==3||mode==6?1:mode==4?2:0,
                        Pose=0,Body=1,Faction=mode==2?1:2,Layer=0,Live=mode==1?0:1,PrimitiveBounds=new Bounds(Vector3.zero,Vector3.one)};
                    surfaces[1]=new ContactSurface{Root=rigid,Pose=1,Body=2,Faction=2,Layer=0,Live=mode==3||mode==4||mode==6?0:1};
                    for(int i=0;i<count;i++)
                    {
                        float y=(i-64)/50f;Vector3 start=new Vector3(0,2+y*2,3.6f);
                        transforms[i+2].position=start+new Vector3(mode==5?9.9f:20,0,0);
                        flights[i]=new ContactFlight{Pose=i+2,ParentPose=-1,Body=1000+i,Faction=1,Mask=1,Live=1,MaximumRange=10000,PreviousLocal=start,Origin=start};
                    }
                    handle=new ContactPoseJob{Slots=slots,World=world,Inverse=inverse,CaptureInverse=backend==0?1:0}.Schedule(access);
                    handle=new ContactIndexJob{Surfaces=surfaces,Geometry=geometry,Envelopes=envelopes,World=world,Bounds=bounds,Order=order,Nodes=nodes,Count=2,Rebuild=true,NativeKernel=backend}.Schedule(handle);
                    handle=new ContactFlightJob{Flights=flights,Surfaces=surfaces,Geometry=geometry,Vertices=vertices,Indices=indices,Influences=influences,
                        Envelopes=envelopes,World=world,Inverse=inverse,Order=order,Nodes=nodes,Results=results,NativeKernel=backend,FlightCount=count}.Schedule(backend==0?count:(count+7)/8,backend==0?16:1,handle);
                    handle.Complete();
                    for(int i=0;i<count;i++)
                    {
                        float y=(i-64)/50f;bool expected=mode!=5&&Math.Abs(y)<(mode==4?.5f:.4f)-.00001f;
                        // Exclude exactly-on-edge samples from the independent
                        // analytic predicate: float transform rounding can choose either side.
                        if(Math.Abs(Math.Abs(y)-(mode==4?.5f:.4f))<.0001f)continue;
                        ContactResult r=results[i];checks++;
                        if((r.Surface>=0)!=expected)throw new InvalidOperationException("Worker contact admission mode="+mode+" ray="+i);
                        float x=mode==1||mode==2?15:mode==3?12.5f:mode==6?15:mode==4?9.5f:10;
                        if(expected&&Math.Abs(r.Point.x-x)>.0001f)throw new InvalidOperationException("Worker nearest surface changed.");
                    }
                }
                BoxCollider identity=objects[0].AddComponent<BoxCollider>();identity.enabled=false;
                if(PreparedContactWorld.Hit(identity,Vector3.one,Vector3.up,1).collider!=identity)throw new InvalidOperationException("Disabled identity cannot bridge native hits.");
                passed=true;Main.Log("PASS: native worker contact self-test checks="+checks+"; rigid/blended/primitive, nearest/faction/inactive/strict endpoint, nonuniform transform jobs and disabled collider identity.");
            }
            finally
            {
                handle.Complete();access.Dispose();slots.Dispose();world.Dispose();inverse.Dispose();geometry.Dispose();vertices.Dispose();indices.Dispose();
                influences.Dispose();envelopes.Dispose();surfaces.Dispose();bounds.Dispose();order.Dispose();nodes.Dispose();flights.Dispose();results.Dispose();
                foreach(var go in objects)UnityEngine.Object.Destroy(go);
            }
        }
    }
}
