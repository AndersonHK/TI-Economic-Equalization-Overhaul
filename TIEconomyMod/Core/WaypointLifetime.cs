using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Lifecycle-only: no Update/LateUpdate hook, pose scan or follow-policy change.
    public static class WaypointLifetime
    {
        private sealed class State { internal bool Retired; }
        private static readonly HashSet<WaypointNavigationController> Live=new HashSet<WaypointNavigationController>();
        private static readonly ConditionalWeakTable<WaypointNavigationController,State> States=new ConditionalWeakTable<WaypointNavigationController,State>();
        private static readonly FieldInfo Active=AccessTools.Field(typeof(WaypointNavigationController),"_activeInputHandlingController");
        private static readonly FieldInfo Placement=AccessTools.Field(typeof(WaypointNavigationController),"_waypointPlacementVisual");
        private static readonly FieldInfo Controllers=AccessTools.Field(typeof(WaypointNavigationController),"_waypointControllers");
        private static readonly FieldInfo Container=AccessTools.Field(typeof(WaypointNavigationController),"_waypointContainer");
        private static readonly FieldInfo ContainerObject=AccessTools.Field(typeof(GameObjectDictionary<string>),"_gameObject");
        private static readonly FieldInfo Waypoint=AccessTools.Field(typeof(WaypointController),"_waypoint");
        private static readonly FieldInfo CameraField=AccessTools.Field(typeof(WaypointController),"_combatCamera");
        private static readonly MethodInfo CameraCallback=AccessTools.Method(typeof(WaypointNavigationController),"OnPreRenderCallback");
        private static readonly MethodInfo CameraMoved=AccessTools.Method(typeof(WaypointController),"SetWaypointState",new[]{typeof(Vector3),typeof(Quaternion)});
        private static readonly MethodInfo MouseBegin=AccessTools.Method(typeof(WaypointController),"HandleOnWaypointMouseOverBegin");
        private static readonly MethodInfo MouseEnd=AccessTools.Method(typeof(WaypointController),"HandleOnWaypointMouseOverEnd");
        private static readonly FieldInfo[] Events={
            AccessTools.Field(typeof(WaypointController),"OnWaypointReadyForInput"),
            AccessTools.Field(typeof(WaypointController),"OnWaypointEndingInput"),
            AccessTools.Field(typeof(WaypointController),"OnWaypointRemovalRequested")};
        public static int LiveCount {get{return Live.Count;}}
        public static void Register(WaypointNavigationController owner)
        {
            State state;if(!States.TryGetValue(owner,out state)){state=new State();States.Add(owner,state);}
            if(!state.Retired)Live.Add(owner);
        }
        public static bool HasLiveVisual(WaypointController controller)
        {
            if(controller==null||controller._visual==null)return false;
            WaypointVisual v=controller._visual;
            return !v.isPlayerWaypoint||(v.YawRotationGizmo!=null&&v.PitchRotationGizmo!=null&&v.RollRotationGizmo!=null&&
                v.BurnGizmo!=null&&v.AltitudeGizmo!=null&&v.LateralGizmo!=null&&v.MovementGizmo!=null);
        }
        private static void ReleaseInput(WaypointController controller)
        {
            if(controller==null||!ReferenceEquals(Active.GetValue(null),controller))return;
            Active.SetValue(null,null);
            var camera=(SpaceCombatCameraController)CameraField.GetValue(controller);
            if(camera!=null)camera.IsCameraMovementBlocked=false;
            if(GameControl.spaceCombat!=null)GameControl.spaceCombat.SetWaypointDragging(false);
        }
        public static void DropStaleInput()
        {
            var active=(WaypointController)Active.GetValue(null);
            if(active!=null&&!HasLiveVisual(active))ReleaseInput(active);
        }
        public static void DestroyController(WaypointController controller)
        {
            if(controller==null)return;
            ReleaseInput(controller);
            foreach(FieldInfo e in Events)e.SetValue(controller,null);
            var waypoint=(AdjustableWaypoint)Waypoint.GetValue(controller);
            if(waypoint!=null)waypoint.OnPositionRotationChange-=controller.UpdateVisualPositionRotation;
            var camera=(SpaceCombatCameraController)CameraField.GetValue(controller);
            // C# event fields can still be detached from a destroyed Unity wrapper.
            if(!ReferenceEquals(camera,null))camera.OnCameraMovementFinished-=
                (SpaceCombatCameraController.CameraMovement)Delegate.CreateDelegate(typeof(SpaceCombatCameraController.CameraMovement),controller,CameraMoved);
            WaypointVisual visual=controller._visual;
            if(!ReferenceEquals(visual,null))
            {
                visual.OnWaypointMouseOverBegin-=(Action)Delegate.CreateDelegate(typeof(Action),controller,MouseBegin);
                visual.OnWaypointMouseOverEnd-=(Action)Delegate.CreateDelegate(typeof(Action),controller,MouseEnd);
            }
            controller._visual=null;
            if(visual!=null)UnityEngine.Object.Destroy(visual.gameObject);
        }
        public static void Retire(WaypointNavigationController owner)
        {
            if(owner==null)return;
            State state;if(!States.TryGetValue(owner,out state)){state=new State();States.Add(owner,state);}
            if(state.Retired)return;
            state.Retired=true;Live.Remove(owner);
            Camera.onPreRender-=(Camera.CameraCallback)Delegate.CreateDelegate(typeof(Camera.CameraCallback),owner,CameraCallback);
            var controllers=(Dictionary<AdjustableWaypoint,WaypointController>)Controllers.GetValue(owner);
            if(controllers!=null){foreach(var controller in controllers.Values)DestroyController(controller);controllers.Clear();}
            var container=(GameObjectDictionary<string>)Container.GetValue(owner);
            if(container==null)return;
            // The public getter creates a new GameObject when the old one died.
            var root=(GameObject)ContainerObject.GetValue(container);
            container.Clear();if(root!=null)root.SetActive(false);
        }
        public static void ResetBattle(SpaceCombatManager manager)
        {
            var active=(WaypointController)Active.GetValue(null);ReleaseInput(active);
            var owners=new WaypointNavigationController[Live.Count];Live.CopyTo(owners);
            foreach(var owner in owners)Retire(owner);
            Active.SetValue(null,null);Placement.SetValue(null,null);
            if(manager!=null){manager.SetWaypointDragging(false);if(manager.combatCamera!=null)manager.combatCamera.IsCameraMovementBlocked=false;}
        }
    }
}
