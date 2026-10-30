using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Geometry is prepared independently of the physics scene. Only small live
    // pose snapshots and projectile lifecycle records change during combat.
    public static class PreparedContactWorld
    {
        private sealed class Primitive
        {
            internal ProjectileController Owner;
            internal Collider Collider;
            internal Transform Transform;
            internal Matrix4x4 Inverse;
            internal Bounds LocalBounds,WorldBounds;
            internal bool Sphere,Live;
            internal int Frame=-1,Layer;
        }
        private static readonly List<CombatMeshGeometry> Bodies=new List<CombatMeshGeometry>(512);
        private static readonly Dictionary<ProjectileController,Primitive> Rounds=new Dictionary<ProjectileController,Primitive>(4096);
        private static readonly List<Primitive> ActiveRounds=new List<Primitive>(4096);
        private static readonly AccessTools.StructFieldRef<RaycastHit,int> ColliderId=
            AccessTools.StructFieldRefAccess<RaycastHit,int>("m_Collider");
        public static void Register(CombatMeshGeometry body){if(!Bodies.Contains(body))Bodies.Add(body);}
        public static void Unregister(CombatMeshGeometry body){Bodies.Remove(body);}
        public static void RegisterRound(ProjectileController owner,Collider collider)
        {
            Primitive p;
            if(!Rounds.TryGetValue(owner,out p))
            {p=new Primitive{Owner=owner};Rounds.Add(owner,p);}
            if(!ActiveRounds.Contains(p))ActiveRounds.Add(p);
            p.Collider=collider;p.Transform=collider.transform;p.Layer=collider.gameObject.layer;p.Frame=-1;
            var sphere=collider as SphereCollider;
            p.Sphere=sphere!=null;
            if(sphere!=null)p.LocalBounds=new Bounds(sphere.center,Vector3.one*(sphere.radius*2));
            else{var box=(BoxCollider)collider;p.LocalBounds=new Bounds(box.center,box.size);}
        }
        public static void RemoveRound(ProjectileController owner)
        {Primitive p;if(Rounds.TryGetValue(owner,out p))ActiveRounds.Remove(p);}
        public static void ClearRounds(){ActiveRounds.Clear();Rounds.Clear();}
        public static RaycastHit HitForId(int id,Vector3 point,Vector3 normal,float distance)
        {
            RaycastHit hit=new RaycastHit{point=point,normal=normal,distance=distance};ColliderId(ref hit)=id;return hit;
        }
        public static RaycastHit Hit(Collider collider,Vector3 point,Vector3 normal,float distance)
        {return HitForId(collider.GetInstanceID(),point,normal,distance);}
        public static bool Cast(BallisticProjectileController shooter,Vector3 origin,Vector3 direction,float length,int mask,out RaycastHit best)
        {
            bool found=false;best=new RaycastHit();TIFactionState faction=shooter.projectileState.shootingFaction;
            foreach(var body in Bodies)
            {
                if(body==null||!body.ContactLive)continue;
                var combatant=body.Damageable as CombatantController;
                var missile=body.Damageable as ProjectileController;
                if(combatant!=null?combatant.faction==faction:missile==null||missile.projectileState.shootingFaction==faction)continue;
                CombatContactMetrics.BodyChecks++;RaycastHit hit;
                if(!body.TryContact(origin,direction,length,mask,out hit))continue;
                best=hit;length=hit.distance;found=true;
            }
            foreach(var p in ActiveRounds)
            {
                if(p.Owner==null||p.Owner==shooter||p.Owner.beenDestroyed||p.Owner.hasHit||p.Owner.projectileState.shootingFaction==faction||
                    (mask&(1<<p.Layer))==0||p.Collider==null)continue;
                if(p.Frame!=Time.frameCount)
                {
                    p.Frame=Time.frameCount;p.Live=p.Collider.enabled&&p.Collider.gameObject.activeInHierarchy;
                    if(!p.Live)continue;
                    p.Inverse=p.Transform.worldToLocalMatrix;
                    p.WorldBounds=TransformBounds(p.LocalBounds,p.Transform.localToWorldMatrix);
                }
                if(!p.Live||!RenderedMeshRaycast.Box(origin,direction,p.WorldBounds.min,p.WorldBounds.max,length))continue;
                float distance;Vector3 normal;
                if(!PrimitiveContact(p.Sphere,p.LocalBounds,p.Inverse.MultiplyPoint3x4(origin),p.Inverse.MultiplyVector(direction),length,out distance,out normal))continue;
                best=Hit(p.Collider,origin+direction*distance,RenderedMeshRaycast.TransformNormal(p.Inverse,normal),distance);
                length=distance;found=true;
            }
            return found;
        }
        private static Bounds TransformBounds(Bounds b,Matrix4x4 m)
        {
            Vector3 e=b.extents;
            return new Bounds(m.MultiplyPoint3x4(b.center),new Vector3(
                Math.Abs(m.m00)*e.x+Math.Abs(m.m01)*e.y+Math.Abs(m.m02)*e.z,
                Math.Abs(m.m10)*e.x+Math.Abs(m.m11)*e.y+Math.Abs(m.m12)*e.z,
                Math.Abs(m.m20)*e.x+Math.Abs(m.m21)*e.y+Math.Abs(m.m22)*e.z)*2);
        }
        public static bool PrimitiveContact(bool sphere,Bounds bounds,Vector3 origin,Vector3 direction,float maximum,out float distance,out Vector3 normal)
        {
            normal=Vector3.zero;distance=maximum;
            if(sphere)
            {
                Vector3 offset=origin-bounds.center;
                double dx=direction.x,dy=direction.y,dz=direction.z,ox=offset.x,oy=offset.y,oz=offset.z;
                double a=dx*dx+dy*dy+dz*dz;if(a<=0)return false;
                double cx=oy*dz-oz*dy,cy=oz*dx-ox*dz,cz=ox*dy-oy*dx;
                double radial=bounds.extents.x*bounds.extents.x-(cx*cx+cy*cy+cz*cz)/a;
                if(radial<0)return false;
                double middle=-(ox*dx+oy*dy+oz*dz)/a,half=Math.Sqrt(radial/a);
                double t=middle-half;if(t<0)t=middle+half;
                if(t<0||t>maximum)return false;distance=(float)t;normal=(offset+direction*distance).normalized;return true;
            }
            double near=0,far=maximum;Vector3 entry=Vector3.zero,exit=Vector3.zero;
            for(int axis=0;axis<3;axis++)
            {
                if(Math.Abs(direction[axis])<1e-20f){if(origin[axis]<bounds.min[axis]||origin[axis]>bounds.max[axis])return false;continue;}
                double a=(bounds.min[axis]-origin[axis])/direction[axis],b=(bounds.max[axis]-origin[axis])/direction[axis];
                Vector3 n=Vector3.zero;n[axis]=-1;
                if(a>b){double t=a;a=b;b=t;n=-n;}
                if(a>=near){near=a;entry=n;}if(b<=far){far=b;exit=-n;}if(near>far)return false;
            }
            if(entry==Vector3.zero){if(exit==Vector3.zero||far<0||far>maximum)return false;distance=(float)far;normal=exit;}
            else{distance=(float)near;normal=entry;}
            return true;
        }
    }
}
