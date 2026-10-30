using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using TIEconomyMod.Core;

// Pure arithmetic fixtures only. Never creates a Unity object, scene or window.
public static class PreparedContactChecks
{
    private static int checks;
    private static void Require(bool condition,string message)
    {checks++;if(!condition)throw new Exception(message);}
    private static Matrix4x4 Pose(float sx,float sy,float sz,float x,float y,float z,bool turn)
    {
        Matrix4x4 m=new Matrix4x4();m.m33=1;m.m03=x;m.m13=y;m.m23=z;m.m22=sz;
        if(turn){m.m01=-sy;m.m10=sx;}else{m.m00=sx;m.m11=sy;}
        return m;
    }
    private static Matrix4x4 Inverse(Matrix4x4 m)
    {
        double det=m.m00*((double)m.m11*m.m22-m.m12*m.m21)-m.m01*((double)m.m10*m.m22-m.m12*m.m20)+m.m02*((double)m.m10*m.m21-m.m11*m.m20);
        Matrix4x4 r=new Matrix4x4();r.m33=1;
        r.m00=(float)((m.m11*m.m22-m.m12*m.m21)/det);r.m01=(float)((m.m02*m.m21-m.m01*m.m22)/det);r.m02=(float)((m.m01*m.m12-m.m02*m.m11)/det);
        r.m10=(float)((m.m12*m.m20-m.m10*m.m22)/det);r.m11=(float)((m.m00*m.m22-m.m02*m.m20)/det);r.m12=(float)((m.m02*m.m10-m.m00*m.m12)/det);
        r.m20=(float)((m.m10*m.m21-m.m11*m.m20)/det);r.m21=(float)((m.m01*m.m20-m.m00*m.m21)/det);r.m22=(float)((m.m00*m.m11-m.m01*m.m10)/det);
        Vector3 p=r.MultiplyVector(new Vector3(m.m03,m.m13,m.m23));r.m03=-p.x;r.m13=-p.y;r.m23=-p.z;return r;
    }
    private static BoneWeight Weight(int bone,float first)
    {return new BoneWeight{boneIndex0=bone,boneIndex1=1-bone,weight0=first,weight1=1-first};}
    private static Vector3 Skin(Vector3 point,BoneWeight w,Matrix4x4[] bind,Matrix4x4[] poses,int quality)
    {
        if(quality==1)return poses[w.boneIndex0].MultiplyPoint3x4(bind[w.boneIndex0].MultiplyPoint3x4(point));
        return poses[w.boneIndex0].MultiplyPoint3x4(bind[w.boneIndex0].MultiplyPoint3x4(point))*w.weight0+
            poses[w.boneIndex1].MultiplyPoint3x4(bind[w.boneIndex1].MultiplyPoint3x4(point))*w.weight1;
    }
    private static bool Brute(Vector3[] points,int[] triangles,BoneWeight[] weights,Matrix4x4[] bind,Matrix4x4[] poses,int quality,Vector3 origin,Vector3 direction,float length,out float distance)
    {
        bool found=false;distance=length;
        // Independent plane-intersection / oriented-edge reference.
        for(int i=0;i<triangles.Length;i+=3)
        {
            int ia=triangles[i],ib=triangles[i+1],ic=triangles[i+2];
            Vector3 a=Skin(points[ia],weights[ia],bind,poses,quality),b=Skin(points[ib],weights[ib],bind,poses,quality),c=Skin(points[ic],weights[ic],bind,poses,quality);
            Vector3 n=Vector3.Cross(b-a,c-a);double denom=Vector3.Dot(n,direction);
            if(Math.Abs(denom)<1e-12)continue;double t=Vector3.Dot(n,a-origin)/denom;
            if(t<0||t>distance)continue;Vector3 p=origin+direction*(float)t;
            if(Vector3.Dot(n,Vector3.Cross(b-a,p-a))< -1e-5||Vector3.Dot(n,Vector3.Cross(c-b,p-b))< -1e-5||Vector3.Dot(n,Vector3.Cross(a-c,p-c))< -1e-5)continue;
            distance=(float)t;found=true;
        }
        return found;
    }
    private static string Digest(object value)
    {
        var b=new StringBuilder();Append(value,b);return b.ToString();
    }
    private static void Append(object value,StringBuilder b)
    {
        if(value==null){b.Append('N');return;}
        Type t=value.GetType();
        if(t.IsPrimitive||t.IsEnum){b.Append(Convert.ToString(value,CultureInfo.InvariantCulture));b.Append(';');return;}
        var sequence=value as IEnumerable;
        if(sequence!=null){foreach(object v in sequence)Append(v,b);return;}
        foreach(var f in t.GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))Append(f.GetValue(value),b);
    }
    public static string Run()
    {
        checks=0;Vector3[] points={new Vector3(10,-1,-1),new Vector3(10,1,-1),new Vector3(10,0,1)};
        int[] triangles={0,1,2};Matrix4x4 identity=Pose(1,1,1,0,0,0,false);
        Matrix4x4[] bind={identity,identity},poses={identity,Pose(1,1,1,20,0,0,false)},inverse={identity,Pose(1,1,1,-20,0,0,false)};
        foreach(float blend in new float[]{1,.75f,.5f})
        {
            BoneWeight[] weights={Weight(0,blend),Weight(0,blend),Weight(0,blend)};
            var tree=new PreparedSkinnedContact(points,triangles,weights,bind,4);string digest=Digest(tree);
            float hit;Vector3 normal;float expected=10+(1-blend)*20;
            Require(tree.Intersect(Vector3.zero,Vector3.right,50,poses,inverse,out hit,out normal)&&Math.Abs(hit-expected)<.0001,"Skin translation or blended gap contact failed.");
            Require(!tree.Intersect(Vector3.zero,Vector3.right,expected-.01f,poses,inverse,out hit,out normal),"Skin admitted future contact.");
            Require(!tree.Intersect(new Vector3(0,.9f,.9f),Vector3.right,50,poses,inverse,out hit,out normal),"Skin admitted empty envelope corner.");
            Require(Digest(tree)==digest,"Prepared geometry mutated during queries.");
        }
        {
            var tree=new PreparedSkinnedContact(points,triangles,new BoneWeight[0],bind,4);
            Require(tree.VirtualBoneIndex==2,"Unweighted authored mesh must use renderer pose.");
            var pose3=new Matrix4x4[]{identity,identity,Pose(1,1,1,5,0,0,false)};
            var inverse3=new Matrix4x4[]{identity,identity,Inverse(pose3[2])};float hit;Vector3 normal;
            Require(tree.Intersect(Vector3.zero,Vector3.right,20,pose3,inverse3,out hit,out normal)&&Math.Abs(hit-15)<.0001,"Unweighted renderer-frame contact failed.");
        }
        var rng=new System.Random(731);var p=new List<Vector3>();var ix=new List<int>();var wts=new List<BoneWeight>();
        // A 12x12 articulated surface exercises both rigid and blended forests.
        for(int y=0;y<12;y++)for(int z=0;z<12;z++)
        {
            int start=p.Count;float fy=y/3f-2,fz=z/3f-2;
            p.Add(new Vector3(10,fy,fz));p.Add(new Vector3(10,fy+.3f,fz));p.Add(new Vector3(10,fy,fz+.3f));
            for(int j=0;j<3;j++){ix.Add(start+j);wts.Add(Weight((y+z)%2,(y+z)%3==0?.65f:1));}
        }
        Vector3[] surface=p.ToArray();int[] indices=ix.ToArray();BoneWeight[] boneWeights=wts.ToArray();
        bind[1]=Pose(1,1,1,-.2f,.1f,0,false);
        foreach(int quality in new int[]{1,2,4})
        {
            var tree=new PreparedSkinnedContact(surface,indices,boneWeights,bind,quality);string digest=Digest(tree);var queryScratch=tree.CreateQueryScratch();
            for(int pose=0;pose<5;pose++)
            {
                poses[0]=Pose(pose%2==0?1:-1,1.3f,.8f,pose*3,.2f,0,pose==3);
                poses[1]=Pose(.7f,pose%2==0?1.1f:-1.1f,1.4f,pose*2+4,-.1f,.2f,pose==2);
                inverse[0]=Inverse(poses[0]);inverse[1]=Inverse(poses[1]);queryScratch.BeginPose();
                var sampleOrigins=new Vector3[100];var sampleDirections=new Vector3[100];var sampleLengths=new float[100];
                for(int r=0;r<100;r++)
                {
                    int v=(r*17)%surface.Length;
                    Vector3 destination=Skin(surface[v],boneWeights[v],bind,poses,quality)+new Vector3(0,.01f,.01f);
                    Vector3 origin=destination-new Vector3(30,(float)rng.NextDouble()*.2f,(float)rng.NextDouble()*.2f);
                    Vector3 direction=(destination-origin).normalized;float length=r%4==0?15:50;
                    sampleOrigins[r]=origin;sampleDirections[r]=direction;sampleLengths[r]=length;
                    float actual,expected;Vector3 normal;
                    bool a=tree.Intersect(origin,direction,length,poses,inverse,queryScratch,out actual,out normal);
                    bool e=Brute(surface,indices,boneWeights,bind,poses,quality,origin,direction,length,out expected);
                    Require(a==e&&(!a||Math.Abs(actual-expected)<.002),"Prepared skin disagreed with independent posed triangle reference.");
                }
                int boundReads=CombatContactMetrics.BoundEvaluations,vertexReads=CombatContactMetrics.MixedVertexReads;
                for(int r=0;r<100;r++)
                {float distance;Vector3 normal;tree.Intersect(sampleOrigins[r],sampleDirections[r],sampleLengths[r],poses,inverse,queryScratch,out distance,out normal);}
                Require(boundReads==CombatContactMetrics.BoundEvaluations&&vertexReads==CombatContactMetrics.MixedVertexReads,"Repeated projectiles recomputed shared pose results.");
            }
            Require(Digest(tree)==digest,"Prepared skin arrays/tree changed across animated poses.");
        }
        foreach(bool sphere in new bool[]{false,true})
        {
            Bounds bounds=new Bounds(Vector3.zero,Vector3.one);float distance;Vector3 normal;
            Require(PreparedContactWorld.PrimitiveContact(sphere,bounds,new Vector3(-2,0,0),Vector3.right,2,out distance,out normal)&&Math.Abs(distance-1.5)<.0001,"Primitive entry failed.");
            Require(!PreparedContactWorld.PrimitiveContact(sphere,bounds,new Vector3(-2,0,0),Vector3.right,1,out distance,out normal),"Primitive admitted future hit.");
            Require(!PreparedContactWorld.PrimitiveContact(sphere,bounds,Vector3.zero,Vector3.right,.1f,out distance,out normal),"Inside primitive invented segment-end surface.");
            Require(PreparedContactWorld.PrimitiveContact(sphere,bounds,Vector3.zero,Vector3.right,1,out distance,out normal)&&Math.Abs(distance-.5)<.0001,"Inside primitive exit failed.");
        }
        // Match the largest audited radiator's scale of data (22,670 vertices,
        // 20 bones), with a deterministic 22,704-vertex articulated surface.
        var stressPoints=new List<Vector3>();var stressIndices=new List<int>();var stressWeights=new List<BoneWeight>();
        for(int y=0;y<44;y++)for(int z=0;z<86;z++)
        {
            int first=stressPoints.Count;float fy=y*.01f-.22f,fz=z*.01f-.43f;
            stressPoints.Add(new Vector3(10,fy,fz));stressPoints.Add(new Vector3(10,fy+.01f,fz));stressPoints.Add(new Vector3(10,fy,fz+.01f));
            stressPoints.Add(new Vector3(10,fy+.01f,fz));stressPoints.Add(new Vector3(10,fy+.01f,fz+.01f));stressPoints.Add(new Vector3(10,fy,fz+.01f));
            for(int j=0;j<6;j++)
            {
                stressIndices.Add(first+j);int bone=y%20;
                stressWeights.Add(new BoneWeight{boneIndex0=bone,boneIndex1=(bone+1)%20,weight0=(first+j)%126==0?.65f:1,weight1=(first+j)%126==0?.35f:0});
            }
        }
        var stressBind=new Matrix4x4[20];poses=new Matrix4x4[20];inverse=new Matrix4x4[20];
        for(int i=0;i<20;i++){stressBind[i]=identity;poses[i]=Pose(1,1,1,i*.0001f,0,0,false);inverse[i]=Inverse(poses[i]);}
        var benchTree=new PreparedSkinnedContact(stressPoints.ToArray(),stressIndices.ToArray(),stressWeights.ToArray(),stressBind,4);
        Vector3[] origins=new Vector3[48],directions=new Vector3[48];
        for(int i=0;i<48;i++){origins[i]=new Vector3(-20,(i%8)*.05f-.19f,(i/8)*.12f-.38f);directions[i]=Vector3.right;}
        var boxes=new Bounds[32];boxes[0]=new Bounds(new Vector3(10,0,0),new Vector3(5,6,6));
        for(int i=1;i<32;i++)boxes[i]=new Bounds(new Vector3(10,i*100,0),new Vector3(5,6,6));
        var scratch=new PreparedSkinnedContact.QueryScratch[19];
        for(int i=0;i<scratch.Length;i++)scratch[i]=benchTree.CreateQueryScratch();
        var times=new double[220];int hits=0;
        Func<long> allocation=null;var api=typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread",Type.EmptyTypes);
        if(api!=null)allocation=(Func<long>)Delegate.CreateDelegate(typeof(Func<long>),api);
        long allocated=0;double sum=0;
        for(int frame=0;frame<240;frame++)
        {
            long before=allocation==null?0:allocation();long start=Stopwatch.GetTimestamp();
            for(int i=0;i<scratch.Length;i++)scratch[i].BeginPose();
            for(int round=0;round<48;round++)for(int body=0;body<32;body++)
            {
                if(!RenderedMeshRaycast.Box(origins[round],directions[round],boxes[body].min,boxes[body].max,60))continue;
                float closest=60;
                for(int part=0;part<19;part++)
                {
                    float distance;Vector3 normal;
                    if(benchTree.Intersect(origins[round],directions[round],closest,poses,inverse,scratch[part],out distance,out normal)){hits++;closest=distance;}
                }
            }
            double ms=(Stopwatch.GetTimestamp()-start)*1000.0/Stopwatch.Frequency;
            long bytes=allocation==null?0:allocation()-before;
            if(frame>=20){times[frame-20]=ms;sum+=ms;allocated+=bytes;}
        }
        Array.Sort(times);Require(hits>0,"Stress workload failed to produce contacts.");
        if(allocation!=null)Require(allocated==0,"Prepared contact query workload allocated managed memory.");
        return "PASS: "+checks+" immutable rigid/blended skin and primitive checks; 48 arriving segments x 32 body bounds x 19 target skins (22,704 vertices/20 bones each) x 220 measured frames, p50="+
            times[110].ToString("F3",CultureInfo.InvariantCulture)+"ms p95="+times[209].ToString("F3",CultureInfo.InvariantCulture)+
            "ms max="+times[219].ToString("F3",CultureInfo.InvariantCulture)+"ms allocated="+(allocation==null?"unavailable":allocated.ToString())+
            " bytes. Pure managed synthetic benchmark; not Unity rendering or full 15x gameplay proof.";
    }
}
