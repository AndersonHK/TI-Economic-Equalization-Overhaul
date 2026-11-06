using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace TIEconomyMod.Core
{
    public struct ContactRay
    {
        public Vector3 Origin,Direction;
        public float Maximum;
        public int Mask;
    }

    // Fitted native-query proxies are data, not PhysX actors. One current pose
    // snapshot/index serves a missile-update phase; rays traverse candidates only.
    public static class CompatibilityContactWorld
    {
        private sealed class Group
        {
            internal int Start,Count;
            internal bool Primitive;
        }
        private static readonly Dictionary<int,Group> groups=new Dictionary<int,Group>();
        private static readonly List<ContactSurface> authored=new List<ContactSurface>(16384);
        private static readonly List<Collider> identities=new List<Collider>(16384);
        private static NativeArray<ContactSurface> surfaces;
        private static NativeArray<Bounds> bounds;
        private static NativeArray<int> order;
        private static NativeArray<ContactWorldNode> nodes;
        private static NativeArray<ContactResult> results;
        private static readonly RaycastHit[] Empty=new RaycastHit[0];
        private static int frame=-1;
        private static bool topology=true;
        private static int lastNearest=-1;
        private static readonly List<int> rejected=new List<int>(32);
        public static int ProxyCount {get{return authored.Count;}}
        public static bool UnbridgedCandidate {get;private set;}
        public static void Invalidate()
        {
            if(surfaces.IsCreated)foreach(int id in rejected)surfaces[id]=authored[id];
            rejected.Clear();frame=-1;
        }
        public static void RejectLast()
        {
            if(lastNearest<0)throw new InvalidOperationException("No compatibility candidate to reject.");
            ContactSurface s=surfaces[lastNearest];s.Live=0;surfaces[lastNearest]=s;rejected.Add(lastNearest);
        }

        public static void Register(int owner,Collider identity,ContactSurface source,Bounds local)
        {
            var group=new Group{Start=authored.Count,Count=1,Primitive=source.Kind>=2};groups[owner]=group;
            source.Kind=source.Kind==3?3:2;source.PrimitiveBounds=local;
            authored.Add(source);identities.Add(identity);topology=true;Invalidate();
        }
        public static void RegisterSkin(int owner,Collider identity,ContactSurface source,int pose,PreparedSkinnedContact skin)
        {
            groups[owner]=new Group{Start=authored.Count,Count=skin.EnvelopeCount};
            for(int i=0;i<skin.EnvelopeCount;i++)
            {
                var e=skin.Envelope(i);ContactSurface proxy=source;proxy.Pose=pose+e.Bone;proxy.Kind=2;
                Vector3 size=e.Max-e.Min;
                proxy.PrimitiveBounds=new Bounds((e.Min+e.Max)*.5f,new Vector3(Math.Max(1e-8f,size.x),Math.Max(1e-8f,size.y),Math.Max(1e-8f,size.z)));
                authored.Add(proxy);identities.Add(identity);
            }
            topology=true;Invalidate();
        }
        public static void Update(int owner,ContactSurface source,Collider identity)
        {
            Group group;if(!groups.TryGetValue(owner,out group))return;
            for(int i=group.Start;i<group.Start+group.Count;i++)
            {
                ContactSurface p=authored[i];
                // Removal cannot reveal a missed candidate, so no pose recapture
                // is needed. Additions/changed live geometry require a fresh index.
                if(source.Live!=0&&(p.Live==0||p.Generation!=source.Generation))Invalidate();
                p.Live=source.Live;p.Generation=source.Generation;p.Layer=source.Layer;p.Faction=source.Faction;
                if(group.Primitive){p.Pose=source.Pose;p.Kind=source.Kind;p.PrimitiveBounds=source.PrimitiveBounds;}
                authored[i]=p;identities[i]=identity;
                if(surfaces.IsCreated&&i<surfaces.Length)surfaces[i]=p;
            }
        }
        public static void EnsureSnapshot(bool outerMeasured=false)
        {
            if(frame==Time.frameCount)return;
            long timer=CombatContactMetrics.Begin();
            try
            {
                PreparedContactWorld.RefreshWorkerBodies();
                ContactJobRuntime.CaptureCompatibilityPose();
                int capacity=Math.Max(16384,authored.Count*2);
                if(!surfaces.IsCreated||surfaces.Length<authored.Count)
                {
                    Dispose(ref surfaces);Dispose(ref bounds);Dispose(ref order);Dispose(ref nodes);Dispose(ref results);
                    surfaces=new NativeArray<ContactSurface>(capacity,Allocator.Persistent);bounds=new NativeArray<Bounds>(capacity,Allocator.Persistent);
                    order=new NativeArray<int>(capacity,Allocator.Persistent);nodes=new NativeArray<ContactWorldNode>(capacity*2,Allocator.Persistent);
                    results=new NativeArray<ContactResult>(capacity,Allocator.Persistent);topology=true;
                    for(int i=0;i<authored.Count;i++)surfaces[i]=authored[i];
                }
                else if(topology)for(int i=0;i<authored.Count;i++)surfaces[i]=authored[i];
                ContactJobRuntime.IndexCompatibility(surfaces,bounds,order,nodes,authored.Count,topology);
                topology=false;frame=Time.frameCount;CombatContactMetrics.CompatibilitySnapshots++;
            }
            finally{CombatContactMetrics.End(11,timer);if(!outerMeasured&&!BallisticContactRuntime.Resolving)CombatContactMetrics.End(14,timer);}
        }
        public static RaycastHit[] Cast(Vector3 origin,Vector3 direction,float maximum,int mask,int faction=-1)
        {
            UnbridgedCandidate=false;
            EnsureSnapshot(true);CombatContactMetrics.CompatibilityRays++;
            long timer=CombatContactMetrics.Begin();int count;
            try
            {
                count=NativeContact.RaycastProxies(surfaces,ContactJobRuntime.CompatibilityPoses,nodes,order,results,
                    new ContactRay{Origin=origin,Direction=direction,Maximum=maximum,Mask=mask},faction);
                if(count<0)throw new InvalidOperationException("Compatibility query capacity was exceeded; refusing truncated contacts.");
            }
            finally{CombatContactMetrics.End(12,timer);}
            lastNearest=count==0?-1:results[0].Surface;
            if(count==0)return Empty;
            timer=CombatContactMetrics.Begin();
            try
            {
                var hits=new RaycastHit[count];int live=0;
                for(int i=0;i<count;i++)
                {
                    ContactResult result=results[i];Collider identity=identities[result.Surface];
                    if(identity==null||!identity.gameObject.activeInHierarchy)continue;
                    hits[live++]=PreparedContactWorld.Hit(identity,result.Point,result.Normal,result.Distance);
                }
                if(live==0){UnbridgedCandidate=faction>=0&&lastNearest>=0;return Empty;}
                if(live!=hits.Length)Array.Resize(ref hits,live);return hits;
            }
            finally{CombatContactMetrics.End(13,timer);}
        }
        private static void Dispose<T>(ref NativeArray<T> data) where T:struct {if(data.IsCreated)data.Dispose();data=default(NativeArray<T>);}
        public static void Clear()
        {
            Dispose(ref surfaces);Dispose(ref bounds);Dispose(ref order);Dispose(ref nodes);Dispose(ref results);
            authored.Clear();identities.Clear();groups.Clear();rejected.Clear();lastNearest=-1;frame=-1;topology=true;
        }
    }
}
