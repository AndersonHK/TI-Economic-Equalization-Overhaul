using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;
using Unity.Jobs;
using HarmonyLib;
using Unity.Entities;
using PavonisInteractive.TerraInvicta.Systems.GameTime;

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
            internal int Pose,ParentPose,Body,Faction,Mask;
            internal Vector3 Origin;
            internal float MaximumRange;
            internal double ClearAt;
            internal ContactResult Result;
            internal int Generation;
        }
        private static readonly Dictionary<BallisticProjectileController,Flight> Flights = new Dictionary<BallisticProjectileController,Flight>(4096);
        private static readonly List<Flight> Active = new List<Flight>(4096), Work = new List<Flight>(4096);
        private static readonly Dictionary<ProjectileJobContainer,int> ContainerCounts=new Dictionary<ProjectileJobContainer,int>(ReferenceIdentityComparer<ProjectileJobContainer>.Instance);
        private static readonly Dictionary<Transform,Flight> ByTransform=new Dictionary<Transform,Flight>(4096);
        private static readonly RaycastHit[] Empty = new RaycastHit[0];
        [ThreadStatic] private static Flight current;
        private static int traces;
        private static readonly RaycastHit[] SingleHit=new RaycastHit[1];
        private static readonly AccessTools.FieldRef<BallisticProjectileController,float> MaximumRange=AccessTools.FieldRefAccess<BallisticProjectileController,float>("maximumDistance");
        private static readonly AccessTools.FieldRef<ProjectileController,Vector3> Origin=AccessTools.FieldRefAccess<ProjectileController,Vector3>("originPosition");
        private static readonly AccessTools.FieldRef<ProjectileController,LayerMask> Mask=AccessTools.FieldRefAccess<ProjectileController,LayerMask>("_collisionMask");
        private static readonly AccessTools.FieldRef<ProjectileController,bool> Cleared=AccessTools.FieldRefAccess<ProjectileController,bool>("<clearedLauncher>k__BackingField");
        private static bool scheduled;
        private static long preparationTicks,boundaryStart;
        private static GameTimeManager gameTime;
        private static double lastEpoch=Double.NaN;
        private static int[] batchGenerations=new int[4096];
        public static bool Resolving { get { return current != null; } }
        public static bool OwnsAllControllers(ProjectileJobContainer container,int count)
        {int owned;return count>0&&ContainerCounts.TryGetValue(container,out owned)&&owned==count;}
        private static void RemoveContainer(ProjectileJobContainer container)
        {int count;if(!ContainerCounts.TryGetValue(container,out count))return;if(count==1)ContainerCounts.Remove(container);else ContainerCounts[container]=count-1;}
        public static bool RegisteredContact(RaycastHit hit)
        {
            Collider collider=hit.collider;
            if(collider==null||collider.isTrigger||!collider.gameObject.activeInHierarchy)return false;
            CombatMeshGeometry geometry;
            if(CombatMeshGeometry.TryOwner(collider,out geometry))return geometry.VisibleDamagePart(collider);
            if(PreparedContactWorld.RegisteredPrimitive(collider))return true;
            if(!collider.enabled)return false;
            CombatantController target=collider.GetComponentInParent<CombatantController>();
            if(target!=null)return target.hitColliders.Contains(collider);
            ProjectileController projectile=collider.GetComponentInParent<ProjectileController>();
            return projectile!=null&&!projectile.beenDestroyed&&projectile.hitColliders.Contains(collider);
        }
        public static void Fired(BallisticProjectileController projectile,ProjectileJobContainer container)
        {
            long timer=CombatContactMetrics.Begin();
            try{FiredCore(projectile,container);}
            finally{if(!Resolving)CombatContactMetrics.End(10,timer);}
        }
        private static void FiredCore(BallisticProjectileController projectile,ProjectileJobContainer container)
        {
            if(gameTime==null)gameTime=World.Active.GetExistingManager<GameTimeManager>();
            Flight flight;
            if(!Flights.TryGetValue(projectile,out flight)){flight=new Flight{Controller=projectile};Flights.Add(projectile,flight);}
            if(flight.Index>=0)ContactJobRuntime.FlightPoses(flight.Pose,flight.ParentPose,-1);
            if(flight.Index>=0)RemoveContainer(flight.Container);
            flight.Generation++;
            flight.Container=container;flight.Root=projectile.transform.parent;
            int owned;ContainerCounts.TryGetValue(container,out owned);ContainerCounts[container]=owned+1;
            flight.LastLocal=flight.Root==null?projectile.position:flight.Root.InverseTransformPoint(projectile.position);
            flight.Pose=ContactJobRuntime.Pose(projectile.projectileTransform);flight.ParentPose=ContactJobRuntime.Pose(flight.Root);
            ByTransform[projectile.projectileTransform]=flight;
            flight.Body=projectile.gameObject.GetInstanceID();flight.Faction=ContactJobRuntime.Faction(projectile.projectileState.shootingFaction);
            flight.Mask=Mask(projectile);flight.Origin=Origin(projectile);flight.MaximumRange=MaximumRange(projectile);
            flight.ClearAt=GameControl.spaceCombat.combatDuration_s+5-gameTime.currentTime.DifferenceInSeconds(projectile.projectileState.launchTime);
            ContactJobRuntime.FlightPoses(flight.Pose,flight.ParentPose,1);
            if(flight.Index<0){flight.Index=Active.Count;Active.Add(flight);}
            if(projectile.velocityVector.sqrMagnitude>0f)projectile.transform.rotation=Quaternion.LookRotation(projectile.velocityVector);
        }
        public static void Remove(ProjectileController controller)
        {
            PreparedContactWorld.RemoveRound(controller);
            var ballistic=controller as BallisticProjectileController;Flight flight;
            if(ReferenceEquals(ballistic,null)||!Flights.TryGetValue(ballistic,out flight)||flight.Index<0)return;
            int last=Active.Count-1;Flight moved=Active[last];Active[flight.Index]=moved;moved.Index=flight.Index;
            Active.RemoveAt(last);flight.Index=-1;RemoveContainer(flight.Container);ContactJobRuntime.FlightPoses(flight.Pose,flight.ParentPose,-1);
        }
        public static void RemovedTransform(ProjectileJobContainer container,Transform transform)
        {Flight flight;if(!ReferenceEquals(transform,null)&&ByTransform.TryGetValue(transform,out flight)&&ReferenceEquals(flight.Container,container))Remove(flight.Controller);}
        public static void Clear(){ContactJobRuntime.Clear();Active.Clear();Work.Clear();Flights.Clear();ContainerCounts.Clear();ByTransform.Clear();current=null;traces=0;scheduled=false;lastEpoch=Double.NaN;PreparedContactWorld.ClearRounds();}
        public static void BeforeMovementComplete(ProjectileJobContainer container,JobHandle movement)
        {
            scheduled=false;
            if(!Patches.BattleScaleRuntime.Enabled||Active.Count==0||GameControl.spaceCombat==null||gameTime==null||gameTime.Paused)return;
            double epoch=GameControl.spaceCombat.combatDuration_s;if(epoch==lastEpoch)return;lastEpoch=epoch;
            boundaryStart=CombatContactMetrics.Begin();
            PreparedContactWorld.RefreshWorkerBodies();Work.Clear();
            foreach(Flight flight in Active)if(flight.Container==container&&flight.Controller!=null&&!flight.Controller.beenDestroyed&&!flight.Controller.hasHit)Work.Add(flight);
            if(Work.Count==0)return;
            ContactJobRuntime.BeginFlights(Work.Count);
            if(batchGenerations.Length<Work.Count)Array.Resize(ref batchGenerations,Work.Count*2);
            for(int i=0;i<Work.Count;i++)
            {
                Flight f=Work[i];batchGenerations[i]=f.Generation;ContactJobRuntime.SetFlight(i,new ContactFlight{Pose=f.Pose,ParentPose=f.ParentPose,Body=f.Body,
                    Faction=f.Faction,Mask=f.Mask,PreviousLocal=f.LastLocal,Origin=f.Origin,MaximumRange=f.MaximumRange,Live=1});
            }
            ContactJobRuntime.Schedule(movement);scheduled=true;
            preparationTicks=System.Diagnostics.Stopwatch.GetTimestamp()-boundaryStart;
            CombatContactMetrics.End(4,boundaryStart);
        }
        public static void AfterMovement(ProjectileJobContainer container)
        {
            if(!scheduled)return;scheduled=false;
            long timer=CombatContactMetrics.Begin();
            ContactJobRuntime.Complete();CombatContactMetrics.End(5,timer);
            long commit=CombatContactMetrics.Begin();
            try
            {
                for(int i=0;i<Work.Count;i++)
                {
                    Flight flight=Work[i];
                    var p=flight.Controller;
                    if(flight.Generation!=batchGenerations[i])continue;
                    if(p==null||flight.Index<0||p.beenDestroyed||p.hasHit||flight.Container!=container)continue;
                    flight.Result=ContactJobRuntime.Result(i);
                    flight.Start=flight.Result.Start;flight.End=flight.Result.End;flight.Consumed=false;
                    flight.LastLocal=flight.Result.NextLocal;
                    if(!p.clearedLauncher&&(float)(lastEpoch-(flight.ClearAt-5))>5f)Cleared(p)=true;
                    if(flight.Result.Expired!=0){p.Destruct();continue;}
                    if(flight.Result.Surface<0)continue;
                    current=flight;
                    try {p.UpdateController();}
                    finally
                    {
                        current=null;
                    }
                }
            }
            finally
            {
                current=null;CombatContactMetrics.End(6,commit);CombatContactMetrics.End(0,timer);
                CombatContactMetrics.End(7,boundaryStart);
                CombatContactMetrics.RecordMain(preparationTicks+System.Diagnostics.Stopwatch.GetTimestamp()-timer);
                CombatContactMetrics.ReportSlowPass(timer);
            }
        }
        public static RaycastHit[] CastCompletedStep(Vector3 nativeOrigin,Vector3 nativeDirection,float nativeLength,int mask,BallisticProjectileController projectile)
        {
            if(!Patches.BattleScaleRuntime.Enabled)return Physics.RaycastAll(nativeOrigin,nativeDirection,nativeLength,mask);
            if(current==null||current.Controller!=projectile||projectile.beenDestroyed||projectile.hasHit)return Empty;
            current.Consumed=true;
            Vector3 delta=current.End-current.Start;float length=delta.magnitude;if(length<=0f)return Empty;
            Vector3 direction=delta/length;RaycastHit best;
            CombatContactMetrics.Queries++;
            Collider identity=ContactJobRuntime.Identity(current.Result.Surface);
            best=identity==null?new RaycastHit():PreparedContactWorld.Hit(identity,current.Result.Point,current.Result.Normal,current.Result.Distance);
            // Earlier committed hits may destroy this target. Requery the same
            // completed segment rather than lose a valid surface behind it.
            if(identity==null||!ContactJobRuntime.IsCurrent(current.Result.Surface,current.Result.Generation)||!RegisteredContact(best))
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
