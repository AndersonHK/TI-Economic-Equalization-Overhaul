using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;

namespace TIEconomyMod.Core
{
    public static class BallisticContactRuntime
    {
        private sealed class Flight
        {
            internal BallisticProjectileController Controller;
            internal ProjectileJobContainer Container;
            internal Transform Root;
            internal Vector3 LastLocal, Start, End;
            internal int Index=-1;
            internal bool Consumed;
        }
        private static readonly Dictionary<BallisticProjectileController,Flight> Flights = new Dictionary<BallisticProjectileController,Flight>();
        private static readonly List<Flight> Active = new List<Flight>(), Work = new List<Flight>();
        private static readonly RaycastHit[] Empty = new RaycastHit[0];
        [ThreadStatic] private static Flight current;
        private static int traces;
        private static readonly RaycastHit[] SingleHit=new RaycastHit[1];
        public static bool Resolving { get { return current != null; } }
        public static bool RegisteredContact(RaycastHit hit)
        {
            Collider collider=hit.collider;
            if(collider==null||collider.isTrigger||!collider.enabled||!collider.gameObject.activeInHierarchy)return false;
            CombatMeshGeometry geometry;
            if(CombatMeshGeometry.TryOwner(collider,out geometry))return geometry.VisibleDamagePart(collider);
            CombatantController target=collider.GetComponentInParent<CombatantController>();
            if(target!=null)return target.hitColliders.Contains(collider);
            ProjectileController projectile=collider.GetComponentInParent<ProjectileController>();
            return projectile!=null&&!projectile.beenDestroyed&&projectile.hitColliders.Contains(collider);
        }
        public static void Fired(BallisticProjectileController projectile,ProjectileJobContainer container)
        {
            Flight flight;
            if(!Flights.TryGetValue(projectile,out flight)){flight=new Flight{Controller=projectile};Flights.Add(projectile,flight);}
            flight.Container=container;flight.Root=projectile.transform.parent;
            flight.LastLocal=flight.Root==null?projectile.position:flight.Root.InverseTransformPoint(projectile.position);
            if(flight.Index<0){flight.Index=Active.Count;Active.Add(flight);}
            if(projectile.velocityVector.sqrMagnitude>0f)projectile.transform.rotation=Quaternion.LookRotation(projectile.velocityVector);
        }
        public static void Remove(ProjectileController controller)
        {
            PreparedContactWorld.RemoveRound(controller);
            var ballistic=controller as BallisticProjectileController;Flight flight;
            if(ballistic==null||!Flights.TryGetValue(ballistic,out flight)||flight.Index<0)return;
            int last=Active.Count-1;Flight moved=Active[last];Active[flight.Index]=moved;moved.Index=flight.Index;
            Active.RemoveAt(last);flight.Index=-1;
        }
        public static void Clear(){Active.Clear();Work.Clear();Flights.Clear();current=null;traces=0;PreparedContactWorld.ClearRounds();}
        public static void AfterMovement(ProjectileJobContainer container)
        {
            if(!Patches.BattleScaleRuntime.Enabled||Active.Count==0)return;
            long timer=CombatContactMetrics.Begin();Work.Clear();Work.AddRange(Active);
            try
            {
                foreach(Flight flight in Work)
                {
                    var p=flight.Controller;
                    if(p==null||flight.Index<0||p.beenDestroyed||p.hasHit||flight.Container!=container)continue;
                    flight.Start=flight.Root==null?flight.LastLocal:flight.Root.TransformPoint(flight.LastLocal);
                    flight.End=p.position;flight.Consumed=false;
                    current=flight;
                    try {p.UpdateController();}
                    finally
                    {
                        current=null;
                        if(flight.Consumed)flight.LastLocal=flight.Root==null?p.position:flight.Root.InverseTransformPoint(p.position);
                    }
                }
            }
            finally{current=null;CombatContactMetrics.End(0,timer);CombatContactMetrics.ReportSlowPass(timer);}
        }
        public static RaycastHit[] CastCompletedStep(Vector3 nativeOrigin,Vector3 nativeDirection,float nativeLength,int mask,BallisticProjectileController projectile)
        {
            if(!Patches.BattleScaleRuntime.Enabled)return Physics.RaycastAll(nativeOrigin,nativeDirection,nativeLength,mask);
            if(current==null||current.Controller!=projectile||projectile.beenDestroyed||projectile.hasHit)return Empty;
            current.Consumed=true;
            Vector3 delta=current.End-current.Start;float length=delta.magnitude;if(length<=0f)return Empty;
            Vector3 direction=delta/length;RaycastHit best;
            CombatContactMetrics.Queries++;
            if(!PreparedContactWorld.Cast(projectile,current.Start,direction,length,mask,out best))return Empty;
            CombatContactMetrics.Contacts++;
            if(traces++<16)Main.Log("Ballistic completed-step contact: weapon="+projectile.projectileState.originWeapon.dataName+
                " collider="+best.collider.name+" start="+current.Start.ToString("G7")+" end="+current.End.ToString("G7")+
                " point="+best.point.ToString("G7")+" step="+length.ToString("G7")+" contactAlong="+best.distance.ToString("G7"));
            SingleHit[0]=best;return SingleHit;
        }
        public static Vector3 ReflectFlight(Vector3 original,Vector3 normal,BallisticProjectileController projectile)
        {return Vector3.Reflect(Patches.BattleScaleRuntime.Enabled?projectile.velocityVector.normalized:original,normal);}
    }
}
