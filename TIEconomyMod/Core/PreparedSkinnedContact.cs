using System;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Immutable bind-space geometry. Live queries never skin a whole mesh or
    // modify a vertex, a bound, or tree topology. Bone matrices are pose input.
    public sealed class PreparedSkinnedContact
    {
        public struct BoneEnvelope
        {
            public int Bone;
            public Vector3 Min, Max;
        }
        private struct Influence
        {
            internal int Bone;
            internal float Weight;
            internal Vector3 Point;
        }
        private sealed class Vertex { internal Influence[] Influences; internal int ScratchIndex=-1; }
        private sealed class RigidGroup
        {
            internal int Bone;
            internal RenderedMeshRaycast Tree;
        }
        private sealed class Node
        {
            internal BoneEnvelope[] Bounds;
            internal int Start, Count, Left=-1, Right=-1;
        }
        private static readonly Dictionary<Mesh,Dictionary<int,PreparedSkinnedContact>> Cache =
            new Dictionary<Mesh,Dictionary<int,PreparedSkinnedContact>>();
        private readonly Vertex[] vertices;
        private readonly int[] triangles, order;
        private readonly RigidGroup[] rigid;
        private readonly List<Node> nodes = new List<Node>();
        private readonly BoneEnvelope[] envelopes;
        private readonly int mixedVertexCount;
        public sealed class QueryScratch
        {
            internal readonly PreparedSkinnedContact Owner;
            internal readonly Vector3[] Points,Min,Max;
            internal readonly int[] PointStamp,NodeStamp;
            internal int Generation=1;
            internal QueryScratch(PreparedSkinnedContact owner,int vertices,int nodes)
            {Owner=owner;Points=new Vector3[vertices];PointStamp=new int[vertices];Min=new Vector3[nodes];Max=new Vector3[nodes];NodeStamp=new int[nodes];}
            public void BeginPose()
            {if(++Generation==0){Array.Clear(PointStamp,0,PointStamp.Length);Array.Clear(NodeStamp,0,NodeStamp.Length);Generation=1;}}
        }
        public QueryScratch CreateQueryScratch(){return new QueryScratch(this,mixedVertexCount,nodes.Count);}
        public int VirtualBoneIndex {get;private set;}
        public int EnvelopeCount { get { return envelopes.Length; } }
        public BoneEnvelope Envelope(int index) { return envelopes[index]; }
        public int RigidGroupCount { get { return rigid.Length; } }
        public int BlendedTriangleCount { get { return order.Length; } }

        public static PreparedSkinnedContact Prepare(Mesh mesh,int maxWeights)
        {
            Dictionary<int,PreparedSkinnedContact> variants;
            if(!Cache.TryGetValue(mesh,out variants))Cache.Add(mesh,variants=new Dictionary<int,PreparedSkinnedContact>());
            PreparedSkinnedContact result;
            if(variants.TryGetValue(maxWeights,out result))return result;
            if(mesh.blendShapeCount!=0)throw new InvalidOperationException("Unprepared blend-shape contact asset: "+mesh.name);
            if(!mesh.isReadable)throw new InvalidOperationException("Skinned collision requires readable bind data: "+mesh.name);
            long timer=CombatContactMetrics.Begin();
            result=new PreparedSkinnedContact(mesh.vertices,mesh.triangles,mesh.boneWeights,mesh.bindposes,maxWeights);
            variants.Add(maxWeights,result);CombatContactMetrics.BvhBuilds++;
            Main.Log("Prepared skin contact: "+mesh.name+" vertices="+mesh.vertexCount+" bones="+result.EnvelopeCount+
                " rigidGroups="+result.RigidGroupCount+" blendedTriangles="+result.BlendedTriangleCount+" quality="+maxWeights);
            CombatContactMetrics.End(1,timer);return result;
        }
        public PreparedSkinnedContact(Vector3[] points,int[] indices,BoneWeight[] weights,Matrix4x4[] bindposes,int maxWeights)
        {
            if(points.Length!=weights.Length&&weights.Length!=0||maxWeights<1||maxWeights>4||indices.Length%3!=0)
                throw new ArgumentException("Invalid prepared skin data.");
            VirtualBoneIndex=-1;
            vertices=new Vertex[points.Length];triangles=(int[])indices.Clone();
            for(int i=0;i<points.Length;i++)
            {
                BoneWeight w=weights.Length==0?new BoneWeight():weights[i];
                int[] bones={w.boneIndex0,w.boneIndex1,w.boneIndex2,w.boneIndex3};
                float[] values={w.weight0,w.weight1,w.weight2,w.weight3};
                var selected=new List<Influence>(4);float total=0;
                // Unity's BoneWeight contract orders influences by descending weight.
                for(int j=0;j<maxWeights;j++)if(values[j]>0)
                {
                    if(bones[j]<0||bones[j]>=bindposes.Length||Single.IsNaN(values[j])||Single.IsInfinity(values[j]))
                        throw new ArgumentException("Invalid prepared skin influence.");
                    selected.Add(new Influence{Bone=bones[j],Weight=values[j],Point=bindposes[bones[j]].MultiplyPoint3x4(points[i])});total+=values[j];
                }
                if(total<=0)
                {
                    // Unweighted authored vertices follow the renderer frame.
                    VirtualBoneIndex=bindposes.Length;
                    selected.Add(new Influence{Bone=VirtualBoneIndex,Weight=1,Point=points[i]});total=1;
                }
                Influence[] data=selected.ToArray();
                for(int j=0;j<data.Length;j++)data[j].Weight/=total;
                vertices[i]=new Vertex{Influences=data};
            }
            var all=new Dictionary<int,BoneEnvelope>();
            var rigidTriangles=new Dictionary<int,List<int>>();var blended=new List<int>();
            for(int t=0;t<triangles.Length;t+=3)
            {
                int bone=-1;bool isRigid=true;
                for(int j=0;j<3;j++)
                {
                    int v=triangles[t+j];if(v<0||v>=vertices.Length)throw new ArgumentException("Invalid collision triangle.");
                    Influence[] data=vertices[v].Influences;
                    foreach(Influence inf in data)Include(all,inf);
                    if(data.Length!=1||(bone>=0&&bone!=data[0].Bone))isRigid=false;
                    if(bone<0)bone=data[0].Bone;
                }
                if(isRigid)
                {
                    List<int> list;if(!rigidTriangles.TryGetValue(bone,out list))rigidTriangles.Add(bone,list=new List<int>());
                    list.Add(triangles[t]);list.Add(triangles[t+1]);list.Add(triangles[t+2]);
                }
                else blended.Add(t/3);
            }
            envelopes=Sorted(all);rigid=new RigidGroup[rigidTriangles.Count];int group=0;
            foreach(var pair in rigidTriangles)
            {
                var remap=new Dictionary<int,int>();var p=new List<Vector3>();var ix=new int[pair.Value.Count];
                for(int j=0;j<ix.Length;j++)
                {
                    int source=pair.Value[j],target;
                    if(!remap.TryGetValue(source,out target)){target=p.Count;remap.Add(source,target);p.Add(vertices[source].Influences[0].Point);}
                    ix[j]=target;
                }
                rigid[group++]=new RigidGroup{Bone=pair.Key,Tree=new RenderedMeshRaycast(p.ToArray(),ix)};
            }
            order=blended.ToArray();int mixedCount=0;
            foreach(int t in order)for(int j=0;j<3;j++)
            {Vertex vertex=vertices[triangles[t*3+j]];if(vertex.ScratchIndex<0)vertex.ScratchIndex=mixedCount++;}
            mixedVertexCount=mixedCount;
            if(order.Length>0)Build(0,order.Length,points);
        }
        private static void Include(Dictionary<int,BoneEnvelope> bounds,Influence inf)
        {
            BoneEnvelope b;
            if(!bounds.TryGetValue(inf.Bone,out b))b=new BoneEnvelope{Bone=inf.Bone,Min=inf.Point,Max=inf.Point};
            else{b.Min=Vector3.Min(b.Min,inf.Point);b.Max=Vector3.Max(b.Max,inf.Point);}
            bounds[inf.Bone]=b;
        }
        private static BoneEnvelope[] Sorted(Dictionary<int,BoneEnvelope> values)
        {
            var result=new BoneEnvelope[values.Count];values.Values.CopyTo(result,0);
            Array.Sort(result,(a,b)=>a.Bone.CompareTo(b.Bone));return result;
        }
        private int Build(int start,int count,Vector3[] points)
        {
            var bounds=new Dictionary<int,BoneEnvelope>();
            Vector3 min=new Vector3(Single.PositiveInfinity,Single.PositiveInfinity,Single.PositiveInfinity),max=-min;
            for(int t=start;t<start+count;t++)for(int j=0;j<3;j++)
            {
                int v=triangles[order[t]*3+j];min=Vector3.Min(min,points[v]);max=Vector3.Max(max,points[v]);
                foreach(Influence inf in vertices[v].Influences)Include(bounds,inf);
            }
            var n=new Node{Start=start,Count=count,Bounds=Sorted(bounds)};int index=nodes.Count;nodes.Add(n);
            if(count<=8)return index;
            Vector3 size=max-min;int axis=size.x>size.y?(size.x>size.z?0:2):(size.y>size.z?1:2);
            Array.Sort(order,start,count,Comparer<int>.Create((a,b)=>Centroid(a,axis,points).CompareTo(Centroid(b,axis,points))));
            int half=count/2;n.Left=Build(start,half,points);n.Right=Build(start+half,count-half,points);n.Count=0;return index;
        }
        private float Centroid(int t,int axis,Vector3[] points)
        {return (points[triangles[t*3]][axis]+points[triangles[t*3+1]][axis]+points[triangles[t*3+2]][axis])/3f;}

        public bool Intersect(Vector3 origin,Vector3 direction,float maximum,Matrix4x4[] toWorld,Matrix4x4[] toBone,out float distance,out Vector3 normal)
        {return Intersect(origin,direction,maximum,toWorld,toBone,null,out distance,out normal);}
        public bool Intersect(Vector3 origin,Vector3 direction,float maximum,Matrix4x4[] toWorld,Matrix4x4[] toBone,QueryScratch scratch,out float distance,out Vector3 normal)
        {
            if(scratch!=null&&scratch.Owner!=this)throw new ArgumentException("Pose scratch belongs to different prepared geometry.");
            distance=maximum;normal=Vector3.zero;if(maximum<0)return false;bool found=false;
            foreach(RigidGroup group in rigid)
            {
                float hit;Vector3 localNormal;Matrix4x4 inverse=toBone[group.Bone];
                if(!group.Tree.Intersect(inverse.MultiplyPoint3x4(origin),inverse.MultiplyVector(direction),distance,out hit,out localNormal))continue;
                distance=hit;normal=RenderedMeshRaycast.TransformNormal(inverse,localNormal);found=true;
            }
            if(nodes.Count>0)found|=Visit(0,origin,direction,toWorld,scratch,ref distance,ref normal);
            return found;
        }
        private bool Visit(int index,Vector3 o,Vector3 d,Matrix4x4[] poses,QueryScratch scratch,ref float distance,ref Vector3 normal)
        {
            Node node=nodes[index];CombatContactMetrics.BvhNodes++;
            Vector3 min,max;
            if(scratch!=null&&scratch.NodeStamp[index]==scratch.Generation){min=scratch.Min[index];max=scratch.Max[index];}
            else
            {
                EnvelopeBounds(node.Bounds,poses,out min,out max);CombatContactMetrics.BoundEvaluations++;
                if(scratch!=null){scratch.Min[index]=min;scratch.Max[index]=max;scratch.NodeStamp[index]=scratch.Generation;}
            }
            if(!RenderedMeshRaycast.Box(o,d,min,max,distance))return false;
            bool found=false;
            if(node.Count==0)
            {
                found|=Visit(node.Left,o,d,poses,scratch,ref distance,ref normal);
                found|=Visit(node.Right,o,d,poses,scratch,ref distance,ref normal);return found;
            }
            for(int i=node.Start;i<node.Start+node.Count;i++)
            {
                int t=order[i]*3;float hit;Vector3 face;CombatContactMetrics.TriangleTests++;
                if(!RenderedMeshRaycast.Triangle(o,d,Point(triangles[t],poses,scratch),Point(triangles[t+1],poses,scratch),Point(triangles[t+2],poses,scratch),distance,out hit,out face))continue;
                distance=hit;normal=face;found=true;
            }
            return found;
        }
        private Vector3 Point(int vertex,Matrix4x4[] poses,QueryScratch scratch)
        {
            Vertex v=vertices[vertex];int index=v.ScratchIndex;
            if(scratch!=null&&scratch.PointStamp[index]==scratch.Generation)return scratch.Points[index];
            Vector3 point=Vector3.zero;
            foreach(Influence inf in v.Influences)point+=poses[inf.Bone].MultiplyPoint3x4(inf.Point)*inf.Weight;
            CombatContactMetrics.MixedVertexReads++;
            if(scratch!=null){scratch.Points[index]=point;scratch.PointStamp[index]=scratch.Generation;}
            return point;
        }
        private static void EnvelopeBounds(BoneEnvelope[] bounds,Matrix4x4[] poses,out Vector3 min,out Vector3 max)
        {
            min=new Vector3(Single.PositiveInfinity,Single.PositiveInfinity,Single.PositiveInfinity);max=-min;
            foreach(BoneEnvelope b in bounds)
            {
                Matrix4x4 m=poses[b.Bone];Vector3 center=m.MultiplyPoint3x4((b.Min+b.Max)*.5f),e=(b.Max-b.Min)*.5f;
                Vector3 extent=new Vector3(Math.Abs(m.m00)*e.x+Math.Abs(m.m01)*e.y+Math.Abs(m.m02)*e.z,
                    Math.Abs(m.m10)*e.x+Math.Abs(m.m11)*e.y+Math.Abs(m.m12)*e.z,
                    Math.Abs(m.m20)*e.x+Math.Abs(m.m21)*e.y+Math.Abs(m.m22)*e.z);
                min=Vector3.Min(min,center-extent);max=Vector3.Max(max,center+extent);
            }
        }
    }
}
