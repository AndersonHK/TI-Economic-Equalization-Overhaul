using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    public static class CombatViewRuntime
    {
        private static readonly AccessTools.FieldRef<SpaceCombatCameraController, CombatantController> Focused =
            AccessTools.FieldRefAccess<SpaceCombatCameraController, CombatantController>("_focusedTarget");
        public static float Ratio { get { return BattleScaleRuntime.Enabled ? BattleScaleRuntime.ModelRatio : 1f; } }
        public static float VisualSize(float value) { return value * Ratio; }
        public static Vector3 VisualVector(Vector3 value) { return value * Ratio; }
        public static CombatantController Subject(SpaceCombatCameraController camera)
        {
            CombatantController subject = Focused(camera);
            if (subject != null && !subject.destructionTriggered) return subject;
            if (GameControl.spaceCombat != null && GameControl.spaceCombat.combatHUD != null)
                return GameControl.spaceCombat.combatHUD.selectedFriendlyShip;
            return null;
        }
        public static float ObjectMinimum(SpaceCombatCameraController camera, float baseline)
        {
            CombatantController subject = Subject(camera);
            if (subject == null) return baseline;
            CombatMeshGeometry geometry = subject.GetComponent<CombatMeshGeometry>();
            Bounds bounds;
            return geometry != null && geometry.TryBounds(out bounds)
                ? Math.Max(baseline, bounds.extents.magnitude * 1.1f) : baseline;
        }
        public static Vector3 SafeDolly(Vector3 proposed, SpaceCombatCameraController camera)
        {
            if (!BattleScaleRuntime.Enabled) return proposed;
            CombatantController subject = Subject(camera);
            if (subject == null) return proposed;
            Vector3 center = subject.position;
            CombatMeshGeometry geometry = subject.GetComponent<CombatMeshGeometry>();
            Bounds bounds;
            if (geometry != null && geometry.TryBounds(out bounds)) center = bounds.center;
            float nativeMinimum = TemplateManager.global.combatCamera_minZoom * 0.05f * SpaceCombatManager.GetScalingAdjustmentFactor();
            float minimum = ObjectMinimum(camera, nativeMinimum * Ratio);
            return CombatViewMath.SafeDollyPoint(camera.transform.position, proposed, center, minimum);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "Initialize")]
    public static class BattleHabRenderBakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatManager __instance)
        {
            if (!BattleScaleRuntime.Enabled || __instance.habModelController == null || __instance.combatState.autoresolve) return;
            CombatRenderGeometry.Apply(__instance.habModelController.transform);
            foreach (CombatMeshGeometry geometry in __instance.habModelController.GetComponentsInChildren<CombatMeshGeometry>(true))
                geometry.RefreshAfterRenderBake();
        }
    }

    [HarmonyPatch(typeof(SpaceCombatCameraController), "get_ScaledMinZoom")]
    public static class BattleClosestZoomPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatCameraController __instance, ref float __result)
        {
            if (BattleScaleRuntime.Enabled)
                __result = CombatViewRuntime.ObjectMinimum(__instance, __result * CombatViewRuntime.Ratio);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatCameraController), "InitializeCamera")]
    public static class BattleCameraClipPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatCameraController __instance)
        {
            if (!BattleScaleRuntime.Enabled) return;
            CombatCameraClipState state = __instance.GetComponent<CombatCameraClipState>();
            if (state == null) state = __instance.gameObject.AddComponent<CombatCameraClipState>();
            state.Apply(__instance.GetComponent<Camera>(), CombatViewRuntime.Ratio);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "OnDisable")]
    public static class BattleCameraClipRestorePatch
    {
        [HarmonyPrefix]
        public static void Prefix(SpaceCombatManager __instance)
        {
            if (__instance.mainCamera == null) return;
            CombatCameraClipState state = __instance.mainCamera.GetComponent<CombatCameraClipState>();
            if (state != null) state.Restore();
        }
        [HarmonyPostfix]
        public static void Postfix() { BattleScaleRuntime.Release(); }
    }

    [HarmonyPatch(typeof(SpaceCombatCameraController), "get_minCameraMovementSpeed")]
    public static class BattleCloseMovementScalePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            // The packaged 0.25 baseline is the earlier physical 1x trial.
            if (BattleScaleRuntime.Enabled) __result *= BattleScaleRuntime.ModelRatio / 0.05f;
        }
    }
    [HarmonyPatch(typeof(SpaceCombatCameraController), "get_minScrollSpeedOffset")]
    public static class BattleCloseScrollScalePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (BattleScaleRuntime.Enabled) __result *= BattleScaleRuntime.ModelRatio / 0.05f;
        }
    }

    [HarmonyPatch(typeof(SpaceCombatCameraController), "HandleZoom")]
    public static class BattleDollyStopPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo target = AccessTools.Field(typeof(SpaceCombatCameraController), "_targetCameraPos");
            MethodInfo helper = AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.SafeDolly));
            var result = new List<CodeInstruction>(); int count = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Stfld && Equals(instruction.operand, target))
                {
                    var load = new CodeInstruction(OpCodes.Ldarg_0);
                    load.labels.AddRange(instruction.labels); instruction.labels.Clear();
                    result.Add(load); result.Add(new CodeInstruction(OpCodes.Call, helper)); count++;
                }
                result.Add(instruction);
            }
            if (count != 1) throw new InvalidOperationException("Expected one free-look camera position target assignment.");
            return result;
        }
    }

    [HarmonyPatch(typeof(WaypointVisual), "UpdateWaypointScale")]
    public static class BattleWaypointScalePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>(); int initial = 0, minimum = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                result.Add(instruction);
                FieldInfo field = instruction.operand as FieldInfo;
                if (instruction.opcode != OpCodes.Ldfld || field == null || field.DeclaringType != typeof(WaypointVisual)) continue;
                if (field.Name == "_initialScale")
                { result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualVector)))); initial++; }
                else if (field.Name == "minScale")
                { result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualSize)))); minimum++; }
            }
            // The maximum already uses the new modelScalingFactor natively.
            if (initial != 1 || minimum != 3) throw new InvalidOperationException("Waypoint scale formula changed.");
            return result;
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "RenderLines")]
    public static class BattlePathThicknessPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>(); int count = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                MethodInfo method = instruction.operand as MethodInfo;
                if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt) && method != null &&
                    method.Name == "set_Thickness" && method.DeclaringType.FullName == "Shapes.Draw")
                {
                    var scale = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualSize)));
                    scale.labels.AddRange(instruction.labels); instruction.labels.Clear(); result.Add(scale); count++;
                }
                result.Add(instruction);
            }
            if (count != 1) throw new InvalidOperationException("Expected one world-space trajectory thickness assignment.");
            return result;
        }
    }

    public sealed class CombatCameraClipState : MonoBehaviour
    {
        private readonly Dictionary<Camera, float> originals = new Dictionary<Camera, float>();
        public void Apply(Camera main, float ratio)
        {
            int uiLayer = LayerMask.NameToLayer("Space Combat UI");
            foreach (Camera camera in Camera.allCameras)
            {
                if (camera != main && (uiLayer < 0 || (camera.cullingMask & (1 << uiLayer)) == 0)) continue;
                if (!originals.ContainsKey(camera)) originals.Add(camera, camera.nearClipPlane);
                camera.nearClipPlane = Math.Max(0.000001f, originals[camera] * ratio);
                Main.Log("Combat close camera: " + camera.name + " near=" + camera.nearClipPlane.ToString("G6") + " relativeModelScale=" + ratio.ToString("G6"));
            }
        }
        public void Restore()
        {
            foreach (KeyValuePair<Camera, float> pair in originals) if (pair.Key != null) pair.Key.nearClipPlane = pair.Value;
            originals.Clear();
        }
        private void OnDestroy() { Restore(); }
    }

    [HarmonyPatch(typeof(MissileController), "UpdateTrackingDisplay")]
    public static class BattleProjectileTacticalPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref float ___trackingDisplayEnvelope)
        {
            if (BattleScaleRuntime.Enabled)
                ___trackingDisplayEnvelope = 2f * BattleScaleRuntime.ProjectileRatio;
        }
        [HarmonyPostfix]
        public static void Postfix(MissileController __instance, float ___minDisplaySize)
        {
            if (!BattleScaleRuntime.Enabled || __instance.trackingDisplayTransform == null ||
                GameControl.spaceCombat == null || GameControl.spaceCombat.mainCameraTransform == null ||
                GameControl.spaceCombat.combatHUD.debugHideUI) return;
            float ratio = BattleScaleRuntime.ProjectileRatio;
            float distanceSquared = (GameControl.spaceCombat.mainCameraTransform.position - __instance.trackingDisplayTransform.position).sqrMagnitude;
            // Evaluate the old display in old distance units, then return its
            // size to current units. Keeps the marker readable at the new switch.
            float size = Mathf.Clamp(distanceSquared * 0.02f / ratio,
                ___minDisplaySize * ratio, 30f * ratio);
            __instance.trackingDisplayTransform.localScale = new Vector3(size, size, 1f);
        }
    }
}
