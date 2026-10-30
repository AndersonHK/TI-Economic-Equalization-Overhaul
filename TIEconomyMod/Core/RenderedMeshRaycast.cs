using System;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Broad colliders stay simple boxes. Only candidate contacts traverse this
    // shared local-space triangle BVH; no per-frame collider cooking or meshes.
    public sealed class RenderedMeshRaycast
    {
        private sealed class Node
        {
            internal Vector3 Min, Max;
            internal int Start, Count, Left = -1, Right = -1;
        }
        private static readonly Dictionary<Mesh, RenderedMeshRaycast> Cache = new Dictionary<Mesh, RenderedMeshRaycast>();
        private readonly Vector3[] vertices;
        private readonly int[] triangles, order;
        private readonly List<Node> nodes = new List<Node>();
        public static void Invalidate(Mesh mesh){Cache.Remove(mesh);}
        public static RenderedMeshRaycast For(Mesh mesh)
        {
            RenderedMeshRaycast result;
            if (!Cache.TryGetValue(mesh, out result))
            {
                long start = CombatContactMetrics.Begin();
                result = new RenderedMeshRaycast(mesh.vertices, mesh.triangles);
                Cache.Add(mesh, result); CombatContactMetrics.BvhBuilds++;
                CombatContactMetrics.End(1, start);
            }
            return result;
        }
        public RenderedMeshRaycast(Vector3[] points, int[] indices)
        {
            vertices = (Vector3[])points.Clone(); triangles = (int[])indices.Clone();
            order = new int[triangles.Length / 3];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            if (order.Length > 0) Build(0, order.Length);
        }
        private int Build(int start, int count)
        {
            var node = new Node { Start = start, Count = count,
                Min = new Vector3(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity),
                Max = new Vector3(float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity) };
            for (int i = start; i < start+count; i++)
                for (int v = 0; v < 3; v++)
                {
                    Vector3 point = vertices[triangles[order[i]*3+v]];
                    node.Min = Vector3.Min(node.Min,point); node.Max = Vector3.Max(node.Max,point);
                }
            int index = nodes.Count; nodes.Add(node);
            if (count <= 8) return index;
            Vector3 extent = node.Max-node.Min;
            int axis = extent.x > extent.y ? (extent.x > extent.z ? 0 : 2) : (extent.y > extent.z ? 1 : 2);
            float pivot=(node.Min[axis]+node.Max[axis])*0.5f;
            int split=start,last=start+count-1;
            while(split<=last)
            {
                if(Centroid(order[split],axis)<pivot){split++;continue;}
                int temp=order[split];order[split]=order[last];order[last]=temp;last--;
            }
            int half=split-start;
            if(half==0||half==count)half=count/2;
            node.Left = Build(start,half); node.Right = Build(start+half,count-half); node.Count = 0;
            return index;
        }
        private float Centroid(int triangle,int axis)
        {
            int i = triangle*3;
            return (vertices[triangles[i]][axis]+vertices[triangles[i+1]][axis]+vertices[triangles[i+2]][axis])/3f;
        }
        public bool Intersect(Vector3 origin,Vector3 direction,float maximum,out float distance,out Vector3 normal)
        {
            distance = maximum; normal = Vector3.zero;
            if (nodes.Count == 0 || maximum < 0f) return false;
            var ray=new PreparedContactRay(origin,direction);
            return Visit(0,ref ray,ref distance,ref normal);
        }
        private bool Visit(int index,ref PreparedContactRay ray,ref float closest,ref Vector3 normal)
        {
            Node n = nodes[index]; CombatContactMetrics.BvhNodes++;
            if (!ray.Intersects(n.Min,n.Max,closest)) return false;
            bool found = false;
            if (n.Count == 0)
            {
                found |= Visit(n.Left,ref ray,ref closest,ref normal);
                found |= Visit(n.Right,ref ray,ref closest,ref normal);
                return found;
            }
            for (int i = n.Start; i < n.Start+n.Count; i++)
            {
                int t = order[i]*3; float d; Vector3 face;
                CombatContactMetrics.TriangleTests++;
                if (Triangle(ray.Origin,ray.Direction,vertices[triangles[t]],vertices[triangles[t+1]],vertices[triangles[t+2]],closest,out d,out face))
                { closest = d; normal = face; found = true; }
            }
            return found;
        }
        public static bool Box(Vector3 o,Vector3 d,Vector3 min,Vector3 max,float length)
        {
            var ray=new PreparedContactRay(o,d);return ray.Intersects(min,max,length);
        }
        public static Vector3 TransformNormal(Matrix4x4 inverse,Vector3 normal)
        {return new Vector3(inverse.m00*normal.x+inverse.m10*normal.y+inverse.m20*normal.z,
            inverse.m01*normal.x+inverse.m11*normal.y+inverse.m21*normal.z,
            inverse.m02*normal.x+inverse.m12*normal.y+inverse.m22*normal.z).normalized;}
        public static bool Triangle(Vector3 o,Vector3 d,Vector3 a,Vector3 b,Vector3 c,float maximum,out float distance,out Vector3 normal)
        {
            distance=0;normal=Vector3.zero;
            Vector3 e1=b-a,e2=c-a,p=Vector3.Cross(d,e2);
            double det=(double)e1.x*p.x+(double)e1.y*p.y+(double)e1.z*p.z;
            if(Math.Abs(det)<1e-20)return false;
            Vector3 s=o-a;double u=((double)s.x*p.x+(double)s.y*p.y+(double)s.z*p.z)/det;
            if(u<0||u>1)return false;
            Vector3 q=Vector3.Cross(s,e1);double v=((double)d.x*q.x+(double)d.y*q.y+(double)d.z*q.z)/det;
            if(v<0||u+v>1)return false;
            double t=((double)e2.x*q.x+(double)e2.y*q.y+(double)e2.z*q.z)/det;
            if(t<0||t>maximum)return false;
            distance=(float)t;normal=Vector3.Cross(e1,e2).normalized;
            if(Vector3.Dot(normal,d)>0)normal=-normal;
            return true;
        }
    }
    // Scalar slab traversal: reciprocal/sign/parallel state is computed once per
    // ray, not by repeated vector indexing and division at every visited node.
    internal struct PreparedContactRay
    {
        internal readonly Vector3 Origin,Direction;
        private readonly double ix,iy,iz;
        private readonly bool px,py,pz;
        internal PreparedContactRay(Vector3 origin,Vector3 direction)
        {
            Origin=origin;Direction=direction;
            px=Math.Abs(direction.x)<1e-20f;py=Math.Abs(direction.y)<1e-20f;pz=Math.Abs(direction.z)<1e-20f;
            ix=px?0:1.0/direction.x;iy=py?0:1.0/direction.y;iz=pz?0:1.0/direction.z;
        }
        internal bool Intersects(Vector3 min,Vector3 max,float maximum)
        {
            if(maximum<0)return false;
            double near=0,far=maximum,a,b,t;
            if(px){if(Origin.x<min.x||Origin.x>max.x)return false;}
            else
            {
                a=(min.x-(double)Origin.x)*ix;b=(max.x-(double)Origin.x)*ix;
                if(ix<0){t=a;a=b;b=t;}if(a>far||b<near)return false;
                if(a>near)near=a;if(b<far)far=b;
            }
            if(py){if(Origin.y<min.y||Origin.y>max.y)return false;}
            else
            {
                a=(min.y-(double)Origin.y)*iy;b=(max.y-(double)Origin.y)*iy;
                if(iy<0){t=a;a=b;b=t;}if(a>far||b<near)return false;
                if(a>near)near=a;if(b<far)far=b;
            }
            if(pz){if(Origin.z<min.z||Origin.z>max.z)return false;}
            else
            {
                a=(min.z-(double)Origin.z)*iz;b=(max.z-(double)Origin.z)*iz;
                if(iz<0){t=a;a=b;b=t;}if(a>far||b<near)return false;
            }
            return true;
        }
    }

}
