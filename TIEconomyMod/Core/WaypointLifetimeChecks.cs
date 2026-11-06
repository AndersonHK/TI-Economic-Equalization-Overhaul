using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Explicit headless mode only. Fixtures contain no ship state or campaign.
    public static class WaypointLifetimeChecks
    {
        private static int checks;
        private static void Require(bool condition,string message){checks++;if(!condition)throw new InvalidOperationException(message);}
        private static void Set(object owner,string field,object value){AccessTools.Field(owner.GetType(),field).SetValue(owner,value);}
        private static GameObject Inactive(string name,Transform parent=null)
        {var go=new GameObject(name);go.SetActive(false);if(parent!=null)go.transform.SetParent(parent,false);return go;}
        private sealed class Fixture
        {
            internal WaypointNavigationController Nav;
            internal WaypointController Controller;
            internal AdjustableWaypoint Waypoint;
            internal WaypointVisual Visual;
            internal GameObjectDictionary<string> Container;
        }
        private static Fixture Create(SpaceCombatCameraController camera)
        {
            var f=new Fixture{Nav=(WaypointNavigationController)FormatterServices.GetUninitializedObject(typeof(WaypointNavigationController)),
                Controller=(WaypointController)FormatterServices.GetUninitializedObject(typeof(WaypointController)),
                Waypoint=(AdjustableWaypoint)FormatterServices.GetUninitializedObject(typeof(AdjustableWaypoint)),
                Container=new GameObjectDictionary<string>("EEO waypoint lifetime fixture")};
            f.Visual=Inactive("Waypoint visual fixture",f.Container.transform).AddComponent<WaypointVisual>();f.Visual.isPlayerWaypoint=true;
            foreach(string field in new[]{"YawRotationGizmo","PitchRotationGizmo","RollRotationGizmo","BurnGizmo","AltitudeGizmo","LateralGizmo","MovementGizmo"})
                Set(f.Visual,field,Inactive(field,f.Visual.transform).AddComponent<WaypointGizmoVisual>());
            f.Controller._visual=f.Visual;Set(f.Controller,"_waypoint",f.Waypoint);Set(f.Controller,"_combatCamera",camera);
            Set(f.Nav,"_waypointControllers",new Dictionary<AdjustableWaypoint,WaypointController>{{f.Waypoint,f.Controller}});
            Set(f.Nav,"_waypointContainer",f.Container);Set(f.Nav,"_spaceCombatCameraController",camera);
            f.Container.Add("visual",f.Visual.gameObject);
            f.Waypoint.OnPositionRotationChange+=f.Controller.UpdateVisualPositionRotation;
            camera.OnCameraMovementFinished+=(SpaceCombatCameraController.CameraMovement)Delegate.CreateDelegate(typeof(SpaceCombatCameraController.CameraMovement),f.Controller,
                AccessTools.Method(typeof(WaypointController),"SetWaypointState",new[]{typeof(Vector3),typeof(Quaternion)}));
            f.Visual.OnWaypointMouseOverBegin+=(Action)Delegate.CreateDelegate(typeof(Action),f.Controller,AccessTools.Method(typeof(WaypointController),"HandleOnWaypointMouseOverBegin"));
            f.Visual.OnWaypointMouseOverEnd+=(Action)Delegate.CreateDelegate(typeof(Action),f.Controller,AccessTools.Method(typeof(WaypointController),"HandleOnWaypointMouseOverEnd"));
            Camera.onPreRender+=(Camera.CameraCallback)Delegate.CreateDelegate(typeof(Camera.CameraCallback),f.Nav,AccessTools.Method(typeof(WaypointNavigationController),"OnPreRenderCallback"));
            WaypointLifetime.Register(f.Nav);return f;
        }
        public static void Run()
        {
            Require(WaypointLifetime.LiveCount==0,"Waypoint fixture requires no live battle.");
            var cameraObject=Inactive("Waypoint camera fixture");var camera=cameraObject.AddComponent<SpaceCombatCameraController>();
            var roots=new List<GameObject>();int unrelated=0;
            Camera.CameraCallback sentinel=cam=>unrelated++;
            SpaceCombatCameraController.CameraMovement moved=(offset,rotation)=>unrelated++;
            Camera.onPreRender+=sentinel;camera.OnCameraMovementFinished+=moved;
            var active=AccessTools.Field(typeof(WaypointNavigationController),"_activeInputHandlingController");
            var placement=AccessTools.Field(typeof(WaypointNavigationController),"_waypointPlacementVisual");
            try
            {
                for(int battle=0;battle<3;battle++)
                {
                    Fixture first=Create(camera),second=Create(camera);roots.Add(first.Container.gameObject);roots.Add(second.Container.gameObject);
                    active.SetValue(null,second.Controller);placement.SetValue(null,second.Visual);camera.IsCameraMovementBlocked=true;
                    Require(WaypointLifetime.HasLiveVisual(second.Controller),"Valid waypoint rejected.");
                    WaypointLifetime.DropStaleInput();Require(ReferenceEquals(active.GetValue(null),second.Controller)&&camera.IsCameraMovementBlocked,"Live input behavior changed.");
                    first.Nav.CleanUpWaypoints();first.Nav.CleanUpWaypoints();
                    Require(ReferenceEquals(active.GetValue(null),second.Controller),"Cleanup stole another navigator's input.");
                    Require(first.Controller._visual==null,"Retired visual retained.");
                    UnityEngine.Object.DestroyImmediate(second.Visual.YawRotationGizmo.gameObject);
                    Require(!WaypointLifetime.HasLiveVisual(second.Controller),"Destroyed yaw gizmo was not detected.");
                    WaypointLifetime.DropStaleInput();Require(active.GetValue(null)==null&&!camera.IsCameraMovementBlocked,"Stale input owner survived.");
                    // Destroy the hierarchy before cleanup, as in a prior-battle remnant.
                    UnityEngine.Object.DestroyImmediate(second.Container.gameObject);
                    second.Nav.CleanUpWaypoints();second.Nav.CleanUpWaypoints();
                    Require(second.Controller._visual==null,"Destroyed wrapper retained.");
                    Require((GameObject)AccessTools.Field(typeof(GameObjectDictionary<string>),"_gameObject").GetValue(second.Container)==null,"Cleanup recreated a dead container.");
                    Require(AccessTools.Field(typeof(AdjustableWaypoint),"OnPositionRotationChange").GetValue(second.Waypoint)==null,"Model subscription leaked.");
                    var movement=(Delegate)AccessTools.Field(typeof(SpaceCombatCameraController),"OnCameraMovementFinished").GetValue(camera);
                    Require(movement!=null&&movement.GetInvocationList().Length==1&&movement.Equals(moved),"Camera subscriptions leaked or unrelated subscriber removed.");
                    var callbacks=Camera.onPreRender.GetInvocationList();
                    foreach(var callback in callbacks)Require(!ReferenceEquals(callback.Target,first.Nav)&&!ReferenceEquals(callback.Target,second.Nav),"Render subscription leaked.");
                    WaypointLifetime.ResetBattle(null);WaypointLifetime.ResetBattle(null);
                    Require(WaypointLifetime.LiveCount==0&&active.GetValue(null)==null&&placement.GetValue(null)==null,"Battle state leaked into next entry.");
                }
                Main.Log("PASS: waypoint lifetime host checks="+checks+"; three synthetic battle cycles, destroyed yaw/visuals, repeated cleanup, event detachment, live-owner preservation and no container resurrection. Camera follow policy unchanged.");
            }
            finally
            {
                WaypointLifetime.ResetBattle(null);Camera.onPreRender-=sentinel;camera.OnCameraMovementFinished-=moved;
                foreach(var root in roots)if(root!=null)UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
