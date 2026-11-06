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
            internal int Worker=-1;
            internal int ActiveIndex=-1;
        }
        private static readonly List<CombatMeshGeometry> Bodies=new List<CombatMeshGeometry>(4096);
        private static readonly List<CombatMeshGeometry> Combatants=new List<CombatMeshGeometry>(512);
        private static readonly HashSet<CombatMeshGeometry> RegisteredBodies=new HashSet<CombatMeshGeometry>(ReferenceIdentityComparer<CombatMeshGeometry>.Instance);
        private static readonly HashSet<CombatMeshGeometry> NativeFallbackBodies=new HashSet<CombatMeshGeometry>(ReferenceIdentityComparer<CombatMeshGeometry>.Instance);
        private static readonly Dictionary<ProjectileController,Primitive> Rounds=new Dictionary<ProjectileController,Primitive>(4096);
        private static readonly List<Primitive> ActiveRounds=new List<Primitive>(4096);
        private static readonly Dictionary<Collider,Primitive> ByCollider=new Dictionary<Collider,Primitive>(4096);
        private static readonly AccessTools.StructFieldRef<RaycastHit,int> ColliderId=
            AccessTools.StructFieldRefAccess<RaycastHit,int>("m_Collider");
        public static void Register(CombatMeshGeometry body){if(RegisteredBodies.Add(body)){Bodies.Add(body);if(body.IsCombatant)Combatants.Add(body);if(!body.HasPreparedParts)NativeFallbackBodies.Add(body);}}
        public static void Unregister(CombatMeshGeometry body){if(RegisteredBodies.Remove(body)){Bodies.Remove(body);if(body.IsCombatant)Combatants.Remove(body);NativeFallbackBodies.Remove(body);}}
        public static void RegisterRound(ProjectileController owner,Collider collider)
        {
            Primitive p;
            if(!Rounds.TryGetValue(owner,out p))
            {p=new Primitive{Owner=owner};Rounds.Add(owner,p);}
            if(p.ActiveIndex<0){p.ActiveIndex=ActiveRounds.Count;ActiveRounds.Add(p);}
            if(!ReferenceEquals(p.Collider,null)&&p.Collider!=collider)ByCollider.Remove(p.Collider);
            ByCollider[collider]=p;
            p.Collider=collider;p.Transform=collider.transform;p.Layer=collider.gameObject.layer;p.Frame=-1;
            var sphere=collider as SphereCollider;
            p.Sphere=sphere!=null;
            if(sphere!=null)p.LocalBounds=new Bounds(sphere.center,Vector3.one*(sphere.radius*2));
            else{var box=(BoxCollider)collider;p.LocalBounds=new Bounds(box.center,box.size);}
            if(p.Worker<0)p.Worker=ContactJobRuntime.Primitive(collider,owner.gameObject.GetInstanceID(),ContactJobRuntime.Faction(owner.projectileState.shootingFaction),true,owner.projectileTransform);
            else ContactJobRuntime.ReusePrimitive(p.Worker,collider,ContactJobRuntime.Faction(owner.projectileState.shootingFaction),owner.projectileTransform);
        }
        public static void RemoveRound(ProjectileController owner)
        {
            Primitive p;if(!Rounds.TryGetValue(owner,out p)||p.ActiveIndex<0)return;
            int last=ActiveRounds.Count-1;Primitive moved=ActiveRounds[last];ActiveRounds[p.ActiveIndex]=moved;moved.ActiveIndex=p.ActiveIndex;
            ActiveRounds.RemoveAt(last);p.ActiveIndex=-1;if(p.Worker>=0)ContactJobRuntime.SetLive(p.Worker,false);
        }
        public static void RefreshWorkerBodies(){foreach(var body in Combatants)if(body!=null)body.RefreshWorkerLife();}
        public static bool RegisteredPrimitive(Collider collider)
        {Primitive p;return ByCollider.TryGetValue(collider,out p)&&p.ActiveIndex>=0&&p.Owner!=null&&!p.Owner.beenDestroyed&&!p.Owner.hasHit;}
        public static bool RaycastCollider(Collider collider,Ray ray,out RaycastHit hit,float maximum)
        {
            if(!Patches.BattleScaleRuntime.Enabled)return collider.Raycast(ray,out hit,maximum);
            long timer=CombatContactMetrics.Begin();
            try{return RaycastColliderCore(collider,ray,out hit,maximum);}
            finally{CombatContactMetrics.End(8,timer);if(!BallisticContactRuntime.Resolving)CombatContactMetrics.End(15,timer);}
        }
        private static bool RaycastColliderCore(Collider collider,Ray ray,out RaycastHit hit,float maximum)
        {
            CombatMeshGeometry geometry;
            if(Patches.BattleScaleRuntime.Enabled&&CombatMeshGeometry.TryOwner(collider,out geometry))
                return geometry.RaycastProxy(collider,new Ray(ray.origin,ray.direction.normalized),maximum,out hit,true);
            if(Patches.BattleScaleRuntime.Enabled&&RegisteredPrimitive(collider))
            {
                return RaycastPrimitive(ByCollider[collider],ray,maximum,out hit);
            }
            return collider.Raycast(ray,out hit,maximum);
        }
        private static bool RaycastPrimitive(Primitive p,Ray ray,float maximum,out RaycastHit hit)
        {
            hit=new RaycastHit();if(p.Owner==null||p.Owner.beenDestroyed||p.Owner.hasHit||!p.Collider.gameObject.activeInHierarchy)return false;
            Vector3 direction=ray.direction.normalized;Matrix4x4 inv=p.Transform.worldToLocalMatrix;
            Vector3 origin=inv.MultiplyPoint3x4(ray.origin);
            if(p.Sphere?(origin-p.LocalBounds.center).sqrMagnitude<=p.LocalBounds.extents.x*p.LocalBounds.extents.x:p.LocalBounds.Contains(origin))return false;
            float distance;Vector3 normal;
            if(!PrimitiveContact(p.Sphere,p.LocalBounds,origin,inv.MultiplyVector(direction),maximum,out distance,out normal))return false;
            hit=Hit(p.Collider,ray.origin+direction*distance,RenderedMeshRaycast.TransformNormal(inv,normal),distance);return true;
        }
        public static RaycastHit[] RaycastAll(Vector3 origin,Vector3 direction,float maximum,int mask)
        {
            if(!Patches.BattleScaleRuntime.Enabled)return Physics.RaycastAll(origin,direction,maximum,mask);
            long timer=CombatContactMetrics.Begin();
            try
            {
                RaycastHit[] prepared=CompatibilityContactWorld.Cast(origin,direction,maximum,mask);
                if(NativeFallbackBodies.Count==0)return prepared;
                CombatContactMetrics.CompatibilityFallbackQueries++;
                var result=new List<RaycastHit>(prepared);
                foreach(var hit in Physics.RaycastAll(origin,direction,maximum,mask))
                {
                    var owner=hit.collider.GetComponentInParent<CombatantController>();
                    var geometry=owner==null?null:CombatMeshGeometry.ForCombatant(owner);
                    if(geometry!=null&&geometry.HasPreparedParts)continue;
                    result.Add(hit);
                }
                return result.ToArray();
            }
            finally{CombatContactMetrics.End(8,timer);if(!BallisticContactRuntime.Resolving)CombatContactMetrics.End(15,timer);}
        }
        public static RaycastHit[] RaycastMissile(Vector3 origin,Vector3 direction,float maximum,int mask,MissileController missile)
        {
            if(!Patches.BattleScaleRuntime.Enabled)return Physics.RaycastAll(origin,direction,maximum,mask);
            return RaycastNearest(origin,direction,maximum,mask,ContactJobRuntime.Faction(missile.projectileState.shootingFaction));
        }
        public static RaycastHit[] RaycastNearest(Vector3 origin,Vector3 direction,float maximum,int mask,int faction)
        {
            if(NativeFallbackBodies.Count!=0)return RaycastAll(origin,direction,maximum,mask);
            long timer=CombatContactMetrics.Begin();
            try
            {
                for(int attempts=0;attempts<=CompatibilityContactWorld.ProxyCount;attempts++)
                {
                    RaycastHit[] hit=CompatibilityContactWorld.Cast(origin,direction,maximum,mask,faction);
                    if(hit.Length==0){if(CompatibilityContactWorld.UnbridgedCandidate){CompatibilityContactWorld.RejectLast();continue;}return hit;}
                    CombatMeshGeometry geometry;
                    bool live=!CombatMeshGeometry.TryOwner(hit[0].collider,out geometry)||geometry.ContactLive;
                    if(live&&BallisticContactRuntime.RegisteredContact(hit[0]))return hit;
                    CompatibilityContactWorld.RejectLast();
                }
                throw new InvalidOperationException("Compatibility candidate rejection failed to make progress.");
            }
            finally{CombatContactMetrics.End(8,timer);if(!BallisticContactRuntime.Resolving)CombatContactMetrics.End(15,timer);}
        }
        public static void ClearRounds(){ActiveRounds.Clear();Rounds.Clear();ByCollider.Clear();}
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
                    p.Frame=Time.frameCount;p.Live=p.Collider.gameObject.activeInHierarchy;
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
