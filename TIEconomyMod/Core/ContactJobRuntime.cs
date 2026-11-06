using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Jobs;

namespace TIEconomyMod.Core
{
    // All registration, ownership and native objects remain on the main thread.
    // Persistent native buffers are exclusively leased to a batch until Complete.
    public static class ContactJobRuntime
    {
        private sealed class SkinRoots { internal int[] Roots,Bones; }
        private static ContactAssetBuilder assets=new ContactAssetBuilder();
        private static readonly Dictionary<PreparedSkinnedContact,SkinRoots> skins=new Dictionary<PreparedSkinnedContact,SkinRoots>();
        private static readonly List<Transform> transforms=new List<Transform>();
        private static readonly List<int> surfacePoseCounts=new List<int>();
        private static readonly List<Collider> identities=new List<Collider>();
        private static readonly Dictionary<Transform,int> poseLookup=new Dictionary<Transform,int>();
        private static readonly List<ContactSurface> authored=new List<ContactSurface>();
        private static TransformAccessArray access;
        private static NativeArray<int> slots;
        private static NativeArray<Matrix4x4> world,inverse;
        private static NativeArray<ContactSurface> surfaces;
        private static NativeArray<ContactNode> nodes;
        private static NativeArray<ContactVertex> vertices;
        private static NativeArray<ContactInfluence> influences;
        private static NativeArray<ContactEnvelope> envelopes;
        private static NativeArray<int> indices,order;
        private static NativeArray<Bounds> bounds;
        private static NativeArray<ContactWorldNode> indexNodes;
        private static NativeArray<ContactFlight> flights;
        private static NativeArray<ContactResult> results;
        private static JobHandle pending;
        private static bool running,dirty=true,assetDirty=true;
        private static bool rebuildIndex=true;
        private static bool reserveAdded;
        private static readonly Queue<int> freeSurfaces=new Queue<int>(4096);
        private static readonly List<int> poseReferences=new List<int>();
        private static readonly HashSet<int> poseChanges=new HashSet<int>();
        private static int flightCount;
        private static readonly List<int> activeSlots=new List<int>();
        private static readonly Dictionary<int,int> activeIndices=new Dictionary<int,int>();
        private static readonly Dictionary<object,int> factions=new Dictionary<object,int>(ReferenceIdentityComparer<object>.Instance);
        public static int Epoch {get;private set;}
        public static int Faction(object faction){if(faction==null)return 0;int id;if(!factions.TryGetValue(faction,out id)){id=factions.Count+1;factions.Add(faction,id);}return id;}
        public static int SurfaceCount {get{return authored.Count;}}
        public static int ActivePoseCount {get{return access.isCreated?access.length:0;}}
        private static readonly AccessTools.FieldRef<SpaceCombatManager,ProjectileJobContainer> Container=AccessTools.FieldRefAccess<SpaceCombatManager,ProjectileJobContainer>("_projectileJobContainer");
        private static readonly AccessTools.FieldRef<ProjectileJobContainer,JobHandle> Movement=AccessTools.FieldRefAccess<ProjectileJobContainer,JobHandle>("_jobHandle");
        internal static NativeArray<Matrix4x4> CompatibilityPoses {get{return world;}}
        internal static void CaptureCompatibilityPose()
        {
            Complete();Prepare();JobHandle dependency=default(JobHandle);
            if(GameControl.spaceCombat!=null){var container=Container(GameControl.spaceCombat);if(container!=null)dependency=Movement(container);}
            if(access.length!=0)new ContactPoseJob{Slots=slots,World=world,Inverse=inverse}.Schedule(access,dependency).Complete();
            else dependency.Complete();
        }
        internal static void IndexCompatibility(NativeArray<ContactSurface> proxies,NativeArray<Bounds> proxyBounds,
            NativeArray<int> proxyOrder,NativeArray<ContactWorldNode> proxyNodes,int count,bool rebuild)
        {
            new ContactIndexJob{Surfaces=proxies,Geometry=nodes,Envelopes=envelopes,World=world,Bounds=proxyBounds,
                Order=proxyOrder,Nodes=proxyNodes,Count=count,NativeKernel=1,Rebuild=rebuild}.Schedule().Complete();
        }

        public static int Pose(Transform value)
        {
            if(value==null)return -1;
            int slot;if(poseLookup.TryGetValue(value,out slot))return slot;
            Complete();slot=transforms.Count;transforms.Add(value);poseReferences.Add(0);poseLookup.Add(value,slot);dirty=true;return slot;
        }
        public static int Static(RenderedMeshRaycast tree,Transform transform,Collider identity,int body,int faction,int layer,bool live,bool pooled=false)
        {
            Complete();int before=assets.Nodes.Count;int root=assets.Add(tree);assetDirty|=assets.Nodes.Count!=before;
            int id=Surface(new ContactSurface{Root=root,Pose=Pose(transform),Body=body,Faction=faction,Layer=layer,Live=live?1:0},identity,1,pooled);
            BoxCollider box=(BoxCollider)identity;
            CompatibilityContactWorld.Register(id,identity,authored[id],new Bounds(box.center,box.size));return id;
        }
        public static int[] Skin(PreparedSkinnedContact tree,Transform[] bones,Collider identity,int body,int faction,int layer,bool live,bool pooled=false)
        {
            Complete();SkinRoots roots=SkinAsset(tree);
            // Bone-local indices require one contiguous pose range. Null/unused
            // slots are never sampled and are not referenced by prepared data.
            int pose=transforms.Count;
            transforms.AddRange(bones);foreach(var unused in bones)poseReferences.Add(0);dirty=true;
            int[] result=new int[roots.Roots.Length];
            for(int i=0;i<result.Length;i++)result[i]=Surface(new ContactSurface{Root=roots.Roots[i],
                Pose=pose+(roots.Bones[i]<0?0:roots.Bones[i]),Kind=roots.Bones[i]<0?1:0,Body=body,
                Faction=faction,Layer=layer,Live=live?1:0},identity,roots.Bones[i]<0?bones.Length:1,pooled);
            if(result.Length>0)CompatibilityContactWorld.RegisterSkin(result[0],identity,authored[result[0]],pose,tree);
            return result;
        }
        private static SkinRoots SkinAsset(PreparedSkinnedContact tree)
        {
            SkinRoots roots;if(!skins.TryGetValue(tree,out roots))
            {roots=new SkinRoots();tree.Export(assets,out roots.Roots,out roots.Bones);skins.Add(tree,roots);assetDirty=true;}
            return roots;
        }
        public static void Prewarm(RenderedMeshRaycast tree)
        {Complete();int before=assets.Nodes.Count;assets.Add(tree);assetDirty|=assets.Nodes.Count!=before;}
        public static void Prewarm(PreparedSkinnedContact tree){Complete();SkinAsset(tree);}
        private static Bounds PrimitiveBounds(Collider collider,Transform root)
        {
            SphereCollider sphere=collider as SphereCollider;BoxCollider box=collider as BoxCollider;
            Bounds b=sphere!=null?new Bounds(sphere.center,Vector3.one*(sphere.radius*2)):new Bounds(box.center,box.size);
            if(root!=null&&collider.transform!=root)
            {
                Transform child=collider.transform;
                if(child.parent!=root||child.localPosition!=Vector3.zero||child.localRotation!=Quaternion.identity)
                    throw new InvalidOperationException("Projectile identity must be an unrotated direct child.");
                Vector3 scale=child.localScale;
                b=new Bounds(Vector3.Scale(b.center,scale),Vector3.Scale(b.size,new Vector3(Math.Abs(scale.x),Math.Abs(scale.y),Math.Abs(scale.z))));
            }
            return b;
        }
        public static int Primitive(Collider collider,int body,int faction,bool live,Transform root=null)
        {
            Complete();
            int id=Surface(new ContactSurface{Pose=Pose(root==null?collider.transform:root),Kind=collider is SphereCollider?3:2,
                PrimitiveBounds=PrimitiveBounds(collider,root),Body=body,Faction=faction,Layer=collider.gameObject.layer,Live=live?1:0},collider,1,true);
            CompatibilityContactWorld.Register(id,collider,authored[id],authored[id].PrimitiveBounds);return id;
        }
        public static void ReusePrimitive(int index,Collider collider,int faction,Transform root=null)
        {
            Complete();ContactSurface s=authored[index];if(s.Live!=0)References(s.Pose,1,-1);
            s.PrimitiveBounds=PrimitiveBounds(collider,root);s.Pose=Pose(root==null?collider.transform:root);
            s.Kind=collider is SphereCollider?3:2;s.Faction=faction;s.Layer=collider.gameObject.layer;s.Live=1;s.Generation++;
            identities[index]=collider;
            References(s.Pose,1,1);WriteSurface(index,s);
        }
        private static void WriteSurface(int index,ContactSurface surface)
        {authored[index]=surface;if(surfaces.IsCreated&&index<surfaces.Length)surfaces[index]=surface;
            CompatibilityContactWorld.Update(index,surface,identities[index]);}
        public static void SetFaction(int index,int faction)
        {Complete();ContactSurface s=authored[index];if(s.Faction==faction)return;s.Faction=faction;s.Generation++;WriteSurface(index,s);}
        private static void References(int pose,int count,int delta)
        {
            for(int i=pose;i<pose+count;i++)
            {
                int old=poseReferences[i],next=old+delta;if(next<0)throw new InvalidOperationException("Contact pose reference underflow.");
                poseReferences[i]=next;if((old==0)!=(next==0)){poseChanges.Add(i);dirty=true;}
            }
        }
        public static void FlightPoses(int pose,int parent,int delta)
        {Complete();References(pose,1,delta);if(parent>=0)References(parent,1,delta);}
        public static void ReserveProjectiles()
        {if(!reserveAdded){Complete();GrowReserve();reserveAdded=true;}}
        private static void GrowReserve()
        {
            for(int i=0;i<4096;i++)
            {int slot=authored.Count;authored.Add(new ContactSurface());identities.Add(null);surfacePoseCounts.Add(0);freeSurfaces.Enqueue(slot);
                if(surfaces.IsCreated&&slot<surfaces.Length)surfaces[slot]=new ContactSurface();}
            dirty=rebuildIndex=true;
        }
        private static int Surface(ContactSurface s,Collider identity,int count,bool pooled=false)
        {
            int result;
            if(pooled)
            {
                ReserveProjectiles();if(freeSurfaces.Count==0)GrowReserve();result=freeSurfaces.Dequeue();
                surfacePoseCounts[result]=count;identities[result]=identity;WriteSurface(result,s);
            }
            else
            {result=authored.Count;authored.Add(s);identities.Add(identity);surfacePoseCounts.Add(count);
                if(surfaces.IsCreated&&result<surfaces.Length)surfaces[result]=s;dirty=rebuildIndex=true;}
            if(s.Live!=0)References(s.Pose,count,1);return result;
        }
        public static void SetLive(int surface,bool live)
        {
            if(surface<0||surface>=authored.Count)return;
            Complete();ContactSurface s=authored[surface];int next=live?1:0;if(s.Live==next)return;
            References(s.Pose,surfacePoseCounts[surface],next==1?1:-1);s.Live=next;s.Generation++;WriteSurface(surface,s);
        }
        public static bool IsCurrent(int surface,int generation)
        {return surface>=0&&surface<authored.Count&&authored[surface].Live!=0&&authored[surface].Generation==generation;}
        public static void Invalidate(int surface)
        {Complete();ContactSurface s=authored[surface];s.Generation++;WriteSurface(surface,s);}
        public static Collider Identity(int surface) {return surface>=0&&surface<identities.Count?identities[surface]:null;}
        public static void BeginFlights(int count)
        {
            Complete();flightCount=count;
            if(!flights.IsCreated||flights.Length<count)
            {Dispose(ref flights);Dispose(ref results);int size=Math.Max(4096,count*2);flights=new NativeArray<ContactFlight>(size,Allocator.Persistent);results=new NativeArray<ContactResult>(size,Allocator.Persistent);dirty=true;}
        }
        public static void SetFlight(int index,ContactFlight flight){flights[index]=flight;}
        private static void Prepare()
        {
            if(assetDirty)
            {
                Replace(ref nodes,assets.Nodes);Replace(ref vertices,assets.Vertices);Replace(ref influences,assets.Influences);
                Replace(ref envelopes,assets.Envelopes);Replace(ref indices,assets.Indices);assetDirty=false;
            }
            if(!dirty)return;
            if(!access.isCreated)access=new TransformAccessArray(Math.Max(32768,transforms.Count*2));
            if(!slots.IsCreated||slots.Length<transforms.Count)
            {
                Dispose(ref slots);slots=new NativeArray<int>(Math.Max(32768,transforms.Count*2),Allocator.Persistent);
                for(int i=0;i<activeSlots.Count;i++)slots[i]=activeSlots[i];
            }
            foreach(int slot in poseChanges)
            {
                int current;
                bool exists=activeIndices.TryGetValue(slot,out current);
                bool wanted=poseReferences[slot]>0&&transforms[slot]!=null;
                if(exists&&!wanted)
                {
                    int last=activeSlots.Count-1,moved=activeSlots[last];access.RemoveAtSwapBack(current);
                    activeSlots[current]=moved;slots[current]=moved;activeIndices[moved]=current;activeSlots.RemoveAt(last);activeIndices.Remove(slot);
                }
                else if(!exists&&wanted)
                {slots[activeSlots.Count]=slot;activeIndices.Add(slot,activeSlots.Count);activeSlots.Add(slot);access.Add(transforms[slot]);}
            }
            poseChanges.Clear();
            if(!world.IsCreated||world.Length<transforms.Count)
            {Dispose(ref world);Dispose(ref inverse);int size=Math.Max(32768,transforms.Count*2);world=new NativeArray<Matrix4x4>(size,Allocator.Persistent);inverse=new NativeArray<Matrix4x4>(size,Allocator.Persistent);}
            int capacity=Math.Max(16384,authored.Count*2);
            if(!surfaces.IsCreated||surfaces.Length<authored.Count)
            {Dispose(ref surfaces);surfaces=new NativeArray<ContactSurface>(capacity,Allocator.Persistent);for(int i=0;i<authored.Count;i++)surfaces[i]=authored[i];}
            if(!bounds.IsCreated||bounds.Length<authored.Count)
            {
                Dispose(ref bounds);Dispose(ref order);Dispose(ref indexNodes);
                bounds=new NativeArray<Bounds>(capacity,Allocator.Persistent);order=new NativeArray<int>(capacity,Allocator.Persistent);
                indexNodes=new NativeArray<ContactWorldNode>(capacity*2,Allocator.Persistent);
            }
            dirty=false;
        }
        public static void Schedule(JobHandle dependency)
        {
            CompatibilityContactWorld.Invalidate();
            Prepare();
            JobHandle pose=access.length==0?dependency:new ContactPoseJob{Slots=slots,World=world,Inverse=inverse}.Schedule(access,dependency);
            JobHandle index=new ContactIndexJob{Surfaces=surfaces,Geometry=nodes,Envelopes=envelopes,World=world,
                Bounds=bounds,Order=order,Nodes=indexNodes,Count=authored.Count,NativeKernel=1,Rebuild=rebuildIndex}.Schedule(pose);
            rebuildIndex=false;
            pending=flightCount==0?index:new ContactFlightJob{Flights=flights,Surfaces=surfaces,Geometry=nodes,Vertices=vertices,Indices=indices,
                Influences=influences,Envelopes=envelopes,World=world,Inverse=inverse,Nodes=indexNodes,Order=order,Results=results,NativeKernel=1,FlightCount=flightCount}.Schedule((flightCount+7)/8,1,index);
            running=true;JobHandle.ScheduleBatchedJobs();
        }
        public static void Warmup(){ContactJobSelfTest.RunOnce();ReserveProjectiles();BeginFlights(0);Schedule(default(JobHandle));Complete();CompatibilityContactWorld.EnsureSnapshot();CompatibilityContactWorld.Invalidate();}
        public static void Complete(){if(!running)return;try{pending.Complete();}finally{running=false;}}
        public static ContactResult Result(int index){return results[index];}
        public static double[] DiagnosticStages()
        {
            Complete();Prepare();var elapsed=new double[3];
            for(int i=0;i<16;i++)
            {
                long start=System.Diagnostics.Stopwatch.GetTimestamp();
                new ContactPoseJob{Slots=slots,World=world,Inverse=inverse}.Schedule(access).Complete();
                long pose=System.Diagnostics.Stopwatch.GetTimestamp();
                new ContactIndexJob{Surfaces=surfaces,Geometry=nodes,Envelopes=envelopes,World=world,
                    Bounds=bounds,Order=order,Nodes=indexNodes,Count=authored.Count,NativeKernel=1,Rebuild=false}.Schedule().Complete();
                long index=System.Diagnostics.Stopwatch.GetTimestamp();
                new ContactFlightJob{Flights=flights,Surfaces=surfaces,Geometry=nodes,Vertices=vertices,Indices=indices,
                    Influences=influences,Envelopes=envelopes,World=world,Inverse=inverse,Nodes=indexNodes,Order=order,Results=results,NativeKernel=1,FlightCount=flightCount}.Schedule((flightCount+7)/8,1).Complete();
                long query=System.Diagnostics.Stopwatch.GetTimestamp();
                elapsed[0]+=(pose-start);elapsed[1]+=(index-pose);elapsed[2]+=(query-index);
            }
            for(int i=0;i<3;i++)elapsed[i]*=1000.0/System.Diagnostics.Stopwatch.Frequency/16;
            return elapsed;
        }
        public static void TopologyChanged(){Complete();dirty=true;}
        private static void Dispose<T>(ref NativeArray<T> value) where T:struct
        {if(value.IsCreated)value.Dispose();value=default(NativeArray<T>);}
        private static void Replace<T>(ref NativeArray<T> value,List<T> source) where T:struct
        {Dispose(ref value);value=new NativeArray<T>(source.ToArray(),Allocator.Persistent);}
        public static void Clear()
        {
            Complete();if(access.isCreated)access.Dispose();access=default(TransformAccessArray);
            CompatibilityContactWorld.Clear();
            Dispose(ref slots);Dispose(ref world);Dispose(ref inverse);Dispose(ref surfaces);Dispose(ref nodes);
            Dispose(ref vertices);Dispose(ref influences);Dispose(ref envelopes);Dispose(ref indices);Dispose(ref order);
            Dispose(ref bounds);Dispose(ref indexNodes);Dispose(ref flights);Dispose(ref results);
            transforms.Clear();poseReferences.Clear();poseChanges.Clear();freeSurfaces.Clear();reserveAdded=false;activeSlots.Clear();activeIndices.Clear();poseLookup.Clear();authored.Clear();identities.Clear();surfacePoseCounts.Clear();skins.Clear();
            assets=new ContactAssetBuilder();dirty=assetDirty=rebuildIndex=true;flightCount=0;
            factions.Clear();Epoch++;
        }
    }
}
