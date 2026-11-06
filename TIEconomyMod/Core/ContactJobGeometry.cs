using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace TIEconomyMod.Core
{
    // Flat, immutable asset data. No Collider, Renderer or other Unity object is
    // reachable by the query jobs. Bone indices are relative to a surface pose.
    public struct ContactNode
    {
        public Vector3 Min, Max;
        public int Start, Count, Skip, EnvelopeStart, EnvelopeCount;
    }
    public struct ContactVertex
    {
        public Vector3 Point;
        public int InfluenceStart, InfluenceCount;
    }
    public struct ContactInfluence { public Vector3 Point; public float Weight; public int Bone; }
    public struct ContactEnvelope { public Vector3 Min, Max; public int Bone; }
    public struct ContactSurface
    {
        public int Root, Pose, Body, Faction, Layer, Kind, Live, Generation;
        public Bounds PrimitiveBounds;
    }
    public struct ContactFlight
    {
        public Vector3 PreviousLocal, Origin;
        public int Pose, ParentPose, Body, Faction, Mask, Live;
        public float MaximumRange;
    }
    public struct ContactResult
    {
        public Vector3 Start, End, NextLocal, Point, Normal;
        public float Distance;
        public int Surface, Expired, Generation;
    }
    public struct ContactWorldNode
    {
        public Vector3 Min, Max;
        public int Start, Count, Skip;
    }

    public sealed class ContactAssetBuilder
    {
        public readonly List<ContactNode> Nodes = new List<ContactNode>();
        public readonly List<ContactVertex> Vertices = new List<ContactVertex>();
        public readonly List<ContactInfluence> Influences = new List<ContactInfluence>();
        public readonly List<ContactEnvelope> Envelopes = new List<ContactEnvelope>();
        public readonly List<int> Indices = new List<int>();
        private readonly Dictionary<RenderedMeshRaycast,int> roots = new Dictionary<RenderedMeshRaycast,int>();
        public int Add(RenderedMeshRaycast tree)
        {
            int root;
            if (!roots.TryGetValue(tree, out root)) { root = tree.Export(this); roots.Add(tree, root); }
            return root;
        }
    }

    // TransformAccess is the sole Unity transform access on worker threads.
    public struct ContactPoseJob : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction] public NativeArray<Matrix4x4> World, Inverse;
        [ReadOnly] public NativeArray<int> Slots;
        public int CaptureInverse;
        public void Execute(int index, TransformAccess transform)
        {
            int slot=Slots[index];
            World[slot] = transform.localToWorldMatrix;
            if(CaptureInverse!=0)Inverse[slot] = transform.worldToLocalMatrix;
        }
    }

    public struct ContactIndexJob : IJob
    {
        [ReadOnly] public NativeArray<ContactSurface> Surfaces;
        [ReadOnly] public NativeArray<ContactNode> Geometry;
        [ReadOnly] public NativeArray<ContactEnvelope> Envelopes;
        [ReadOnly] public NativeArray<Matrix4x4> World;
        public NativeArray<Bounds> Bounds;
        public NativeArray<int> Order;
        public NativeArray<ContactWorldNode> Nodes;
        public int Count;
        public bool Rebuild;
        public int NativeKernel;
        private int next;
        public void Execute()
        {
            if(NativeKernel!=0){NativeContact.Index(this);return;}
            if(!Rebuild&&Nodes[0].Skip>0)
            {
                for(int i=0;i<Count;i++)if(Surfaces[i].Live!=0)Bounds[i]=ContactQuery.SurfaceBounds(Surfaces[i],Geometry,Envelopes,World);
                for(int i=Nodes[0].Skip-1;i>=0;i--)
                {
                    ContactWorldNode n=Nodes[i];if(n.Start<0)continue;
                    if(n.Count>0)
                    {
                        Bounds total=Bounds[Order[n.Start]];
                        for(int j=1;j<n.Count;j++)total.Encapsulate(Bounds[Order[n.Start+j]]);
                        n.Min=total.min;n.Max=total.max;
                    }
                    else
                    {
                        ContactWorldNode left=Nodes[i+1],right=Nodes[left.Skip];
                        n.Min=Vector3.Min(left.Min,right.Min);n.Max=Vector3.Max(left.Max,right.Max);
                    }
                    Nodes[i]=n;
                }
                return;
            }
            int live = 0;
            for (int i = 0; i < Count; i++)
            {
                ContactSurface s = Surfaces[i];
                if (s.Live == 0) continue;
                Bounds[i] = ContactQuery.SurfaceBounds(s, Geometry, Envelopes, World);
                Order[live++] = i;
            }
            next = 0;
            if (live > 0) Build(0, live);
            else Nodes[0] = new ContactWorldNode { Skip = 1, Count = 0, Start = -1 };
        }
        private int Build(int start, int count)
        {
            int slot = next++;
            Bounds total = Bounds[Order[start]];
            for (int i = start + 1; i < start + count; i++) total.Encapsulate(Bounds[Order[i]]);
            ContactWorldNode n = new ContactWorldNode { Min = total.min, Max = total.max, Start = start, Count = count };
            if (count > 4)
            {
                Vector3 size = total.size;
                int axis = size.x > size.y ? (size.x > size.z ? 0 : 2) : (size.y > size.z ? 1 : 2);
                float pivot = total.center[axis];
                int left = start, right = start + count - 1;
                while (left <= right)
                {
                    if (Bounds[Order[left]].center[axis] < pivot) { left++; continue; }
                    int swap = Order[left]; Order[left] = Order[right]; Order[right--] = swap;
                }
                int half = left - start;
                if (half == 0 || half == count) half = count / 2;
                Build(start, half); Build(start + half, count - half); n.Count = 0;
            }
            n.Skip = next; Nodes[slot] = n; return slot;
        }
    }

    public static class ContactQuery
    {
        public static Bounds TransformBounds(Vector3 min, Vector3 max, Matrix4x4 m)
        {
            Vector3 e = (max - min) * .5f;
            return new Bounds(m.MultiplyPoint3x4((min + max) * .5f), new Vector3(
                Math.Abs(m.m00)*e.x + Math.Abs(m.m01)*e.y + Math.Abs(m.m02)*e.z,
                Math.Abs(m.m10)*e.x + Math.Abs(m.m11)*e.y + Math.Abs(m.m12)*e.z,
                Math.Abs(m.m20)*e.x + Math.Abs(m.m21)*e.y + Math.Abs(m.m22)*e.z) * 2);
        }
        public static Bounds NodeBounds(ContactNode n, int pose, NativeArray<ContactEnvelope> envelopes, NativeArray<Matrix4x4> world)
        {
            if (n.EnvelopeCount == 0) return TransformBounds(n.Min, n.Max, world[pose]);
            ContactEnvelope e = envelopes[n.EnvelopeStart];
            Bounds bounds = TransformBounds(e.Min, e.Max, world[pose + e.Bone]);
            for (int i = 1; i < n.EnvelopeCount; i++)
            { e = envelopes[n.EnvelopeStart+i]; bounds.Encapsulate(TransformBounds(e.Min, e.Max, world[pose+e.Bone])); }
            return bounds;
        }
        public static Bounds SurfaceBounds(ContactSurface s, NativeArray<ContactNode> nodes,
            NativeArray<ContactEnvelope> envelopes, NativeArray<Matrix4x4> world)
        {
            return s.Kind >= 2 ? TransformBounds(s.PrimitiveBounds.min, s.PrimitiveBounds.max, world[s.Pose]) :
                NodeBounds(nodes[s.Root], s.Pose, envelopes, world);
        }
        private static Vector3 Point(ContactVertex vertex, int pose, NativeArray<ContactInfluence> influences, NativeArray<Matrix4x4> world)
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < vertex.InfluenceCount; i++)
            { ContactInfluence inf = influences[vertex.InfluenceStart+i]; result += world[pose+inf.Bone].MultiplyPoint3x4(inf.Point)*inf.Weight; }
            return result;
        }
        public static bool Intersect(ContactSurface surface, Vector3 origin, Vector3 direction, float maximum,
            NativeArray<ContactNode> nodes, NativeArray<ContactVertex> vertices, NativeArray<int> indices,
            NativeArray<ContactInfluence> influences, NativeArray<ContactEnvelope> envelopes,
            NativeArray<Matrix4x4> world, NativeArray<Matrix4x4> inverse, out float distance, out Vector3 normal)
        {
            distance = maximum; normal = Vector3.zero;
            Matrix4x4 inv = inverse[surface.Pose];
            if (surface.Kind >= 2)
            {
                Vector3 localNormal;
                bool hit = PreparedContactWorld.PrimitiveContact(surface.Kind == 3, surface.PrimitiveBounds,
                    inv.MultiplyPoint3x4(origin), inv.MultiplyVector(direction), maximum, out distance, out localNormal);
                if (hit) normal = RenderedMeshRaycast.TransformNormal(inv, localNormal);
                return hit;
            }
            bool mixed = surface.Kind == 1, found = false;
            Vector3 o = mixed ? origin : inv.MultiplyPoint3x4(origin), d = mixed ? direction : inv.MultiplyVector(direction);
            int end = nodes[surface.Root].Skip;
            for (int i = surface.Root; i < end;)
            {
                ContactNode n = nodes[i];
                Bounds b = mixed ? NodeBounds(n, surface.Pose, envelopes, world) : new Bounds((n.Min+n.Max)*.5f,n.Max-n.Min);
                if (!RenderedMeshRaycast.Box(o,d,b.min,b.max,distance)) { i=n.Skip; continue; }
                if (n.Count == 0) { i++; continue; }
                for (int j = n.Start; j < n.Start+n.Count; j+=3)
                {
                    ContactVertex a=vertices[indices[j]], bv=vertices[indices[j+1]], c=vertices[indices[j+2]];
                    float t; Vector3 face;
                    if (!RenderedMeshRaycast.Triangle(o,d,
                        mixed ? Point(a,surface.Pose,influences,world) : a.Point,
                        mixed ? Point(bv,surface.Pose,influences,world) : bv.Point,
                        mixed ? Point(c,surface.Pose,influences,world) : c.Point,distance,out t,out face)) continue;
                    distance=t; normal=mixed ? face : RenderedMeshRaycast.TransformNormal(inv,face); found=true;
                }
                i=n.Skip;
            }
            return found;
        }
    }

    public struct ContactFlightJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ContactFlight> Flights;
        [ReadOnly] public NativeArray<ContactSurface> Surfaces;
        [ReadOnly] public NativeArray<ContactNode> Geometry;
        [ReadOnly] public NativeArray<ContactVertex> Vertices;
        [ReadOnly] public NativeArray<int> Indices, Order;
        [ReadOnly] public NativeArray<ContactInfluence> Influences;
        [ReadOnly] public NativeArray<ContactEnvelope> Envelopes;
        [ReadOnly] public NativeArray<ContactWorldNode> Nodes;
        [ReadOnly] public NativeArray<Matrix4x4> World, Inverse;
        [NativeDisableParallelForRestriction] public NativeArray<ContactResult> Results;
        public int NativeKernel,FlightCount;
        public void Execute(int index)
        {
            if(NativeKernel!=0){int first=index*8;NativeContact.Query(this,first,Math.Min(8,FlightCount-first));return;}
            ContactFlight f=Flights[index]; ContactResult result=new ContactResult { Surface=-1 };
            if (f.Live==0) { Results[index]=result; return; }
            result.Start=f.ParentPose<0 ? f.PreviousLocal : World[f.ParentPose].MultiplyPoint3x4(f.PreviousLocal);
            result.End=World[f.Pose].MultiplyPoint3x4(Vector3.zero);
            result.NextLocal=f.ParentPose<0 ? result.End : Inverse[f.ParentPose].MultiplyPoint3x4(result.End);
            result.Expired=(result.End-f.Origin).magnitude/.05f>f.MaximumRange ? 1 : 0;
            Vector3 delta=result.End-result.Start;
            float length=delta.magnitude; result.Distance=length;
            if (result.Expired!=0 || length<=0) { Results[index]=result; return; }
            Vector3 direction=delta/length;
            int end=Nodes[0].Skip;
            for (int i=0;i<end;)
            {
                ContactWorldNode n=Nodes[i];
                if(n.Start<0 || !RenderedMeshRaycast.Box(result.Start,direction,n.Min,n.Max,result.Distance)) { i=n.Skip; continue; }
                if(n.Count==0) { i++; continue; }
                for(int j=n.Start;j<n.Start+n.Count;j++)
                {
                    int id=Order[j]; ContactSurface s=Surfaces[id];
                    if(s.Live==0 || s.Body==f.Body || s.Faction==f.Faction || (f.Mask&(1<<s.Layer))==0)continue;
                    float distance; Vector3 normal;
                    if(!ContactQuery.Intersect(s,result.Start,direction,result.Distance,Geometry,Vertices,Indices,
                        Influences,Envelopes,World,Inverse,out distance,out normal))continue;
                    // Deterministic ties use registration order, independent of BVH partitioning.
                    if(result.Surface>=0 && distance==result.Distance && id<result.Surface)continue;
                    result.Surface=id; result.Distance=distance; result.Normal=normal;
                    result.Generation=s.Generation;
                }
                i=n.Skip;
            }
            result.Point=result.Start+direction*result.Distance; Results[index]=result;
        }
    }
}
