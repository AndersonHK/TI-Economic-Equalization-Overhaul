using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    public static class BattleScaleRuntime
    {
        private static bool captured;
        private static float activeShipMultiplier, activeProjectileMultiplier;
        public static bool Enabled { get { return ProjectileCollisionFeature.Enabled; } }
        public static bool Cinematic { get { return SpaceCombatManager.GetScalingAdjustmentFactor() > 0.5f; } }
        public static float ShipMultiplier { get { return captured ? activeShipMultiplier : ReadShipMultiplier(); } }
        public static float ProjectileMultiplier { get { return captured ? activeProjectileMultiplier : ReadProjectileMultiplier(); } }
        public static float ModelScale { get { return BattleScaleMath.ModelScale(ShipMultiplier); } }
        public static float ProjectileScale { get { return BattleScaleMath.ProjectileScale(ProjectileMultiplier); } }
        public static float ModelRatio { get { return CombatViewMath.RelativeModelScale(Cinematic, ShipMultiplier); } }
        public static float ProjectileRatio { get { return CombatViewMath.RelativeProjectileScale(ProjectileMultiplier); } }
        public static float LegacyModelScale { get { return 0.01f * SpaceCombatManager.GetScalingAdjustmentFactor(); } }
        private static float ReadShipMultiplier()
        { return BattleScaleMath.ValidMultiplier(Main.settings != null && Main.settings.shipBalance != null ? Main.settings.shipBalance.shipScaleMultiplier : BattleScaleMath.DefaultMultiplier); }
        private static float ReadProjectileMultiplier()
        { return BattleScaleMath.ValidMultiplier(Main.settings != null && Main.settings.shipBalance != null ? Main.settings.shipBalance.projectileScaleMultiplier : BattleScaleMath.DefaultMultiplier); }
        public static void Capture()
        {
            if (captured) return;
            activeShipMultiplier = ReadShipMultiplier(); activeProjectileMultiplier = ReadProjectileMultiplier(); captured = true;
            Main.Log("Combat scale settings captured: ships=" + activeShipMultiplier + "x projectiles=" + activeProjectileMultiplier + "x");
        }
        public static void Release() { captured = false; }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "Initialize")]
    public static class BattleScaleSettingsCapturePatch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        public static void Prefix(SpaceCombatManager __instance)
        {
            if (BattleScaleRuntime.Enabled && !__instance.initialized) BattleScaleRuntime.Capture();
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "get_modelScalingFactor")]
    public static class BattleModelScalePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (BattleScaleRuntime.Enabled) __result = BattleScaleRuntime.ModelScale;
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "get_projectileScalingFactor")]
    public static class BattleProjectileScalePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (BattleScaleRuntime.Enabled) __result = BattleScaleRuntime.ProjectileScale;
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "OnEnable")]
    public static class BattleGridScalePatch
    {
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatManager __instance)
        {
            // The tactical input plane is not a physical model. Keep its reach.
            if (BattleScaleRuntime.Enabled && __instance.combatGrid != null)
            {
                __instance.combatGrid.transform.localScale = Vector3.one * (25f * BattleScaleRuntime.LegacyModelScale);
                var probe=__instance.GetComponent<CombatContactFrameProbe>();
                if(probe==null)probe=__instance.gameObject.AddComponent<CombatContactFrameProbe>();
                probe.Arm();
            }
        }
    }

    [HarmonyPatch(typeof(CombatShipController), "get_BoundingBoxSize")]
    public static class BattleShipBoundsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CombatShipController __instance, ref Vector3 __result)
        {
            if (!BattleScaleRuntime.Enabled || __instance.ModelController == null) return true;
            CombatMeshGeometry geometry = CombatMeshGeometry.ForShip(__instance);
            Bounds bounds;
            if (geometry == null || !geometry.TryBounds(out bounds)) return true;
            // Native navigation is initialized with this property during Initialize.
            __result = bounds.size;
            return false;
        }
    }

    [HarmonyPatch(typeof(CombatShipController), "Initialize")]
    public static class BattleShipGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CombatShipController __instance)
        {
            if (BattleScaleRuntime.Enabled)
            {
                CombatMeshGeometry.ForShip(__instance);
                if(__instance.ModelController!=null)CombatMeshGeometry.PrimeProjectiles(__instance.ModelController.transform);
            }
        }
    }

    [HarmonyPatch(typeof(CombatShipController), "ReturnToStrategyLayerFleet")]
    public static class BattleShipGeometryRestorePatch
    {
        [HarmonyPrefix]
        public static void Prefix(CombatShipController __instance)
        {
            CombatExplosionGeometry explosions = __instance.GetComponent<CombatExplosionGeometry>();
            if (explosions != null) explosions.Restore();
            if (__instance.ModelController != null) CombatRenderGeometry.RestoreOn(__instance.ModelController.transform);
            CombatMeshGeometry geometry = __instance.GetComponent<CombatMeshGeometry>();
            if (geometry != null) geometry.Restore();
        }
    }

    [HarmonyPatch(typeof(CombatHabModuleController), "InitializeForCombat")]
    public static class BattleHabGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CombatHabModuleController __instance)
        {
            if (!BattleScaleRuntime.Enabled || __instance.combatHitColliders == null) return;
            CombatMeshGeometry.ForHab(__instance);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "ReturnHabToStrategyLayer")]
    public static class BattleHabGeometryRestorePatch
    {
        [HarmonyPrefix]
        public static void Prefix(SpaceCombatManager __instance)
        {
            if (__instance.habModelController == null) return;
            foreach (CombatStationExplosionObserver observer in __instance.habModelController.GetComponentsInChildren<CombatStationExplosionObserver>(true))
                observer.Disarm();
            CombatRenderGeometry.RestoreOn(__instance.habModelController.transform);
            foreach (CombatMeshGeometry geometry in __instance.habModelController.GetComponentsInChildren<CombatMeshGeometry>(true))
                geometry.Restore();
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "ConfigureHabCollisionObjectsForCombat")]
    public static class BattleHabFormationGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatManager __instance)
        {
            if (!BattleScaleRuntime.Enabled || __instance.habModelController == null) return;
            foreach (CombatMeshGeometry geometry in __instance.habModelController.GetComponentsInChildren<CombatMeshGeometry>(true))
                geometry.SuppressNativeDamageColliders();
        }
    }

    [HarmonyPatch(typeof(MissileController), "Fire")]
    public static class BattleMissileGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MissileController __instance)
        {
            if (BattleScaleRuntime.Enabled) CombatMeshGeometry.ForMissile(__instance);
        }
    }

    [HarmonyPatch(typeof(ProjectileController), "Destruct")]
    public static class BattleProjectileHidePatch
    {
        [HarmonyPrefix]
        public static void Prefix(ProjectileController __instance)
        {
            BallisticContactRuntime.Remove(__instance);
            BattleProjectileGeometry body = __instance.GetComponent<BattleProjectileGeometry>();
            if (body != null) body.Hide();
        }
    }

    [HarmonyPatch(typeof(TISpaceCombatProjectileState), "WillHitSphere")]
    public static class BattleThreatGeometryPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TISpaceCombatProjectileState __instance, Vector3 targetPosition,
            Vector3 targetVelocity, CombatantController targetController, ref bool __result)
        {
            if (!BattleScaleRuntime.Enabled || targetController == null) return true;
            CombatMeshGeometry geometry = CombatMeshGeometry.ForCombatant(targetController);
            Bounds bounds;
            if (geometry == null || !geometry.TryBounds(out bounds)) return true;
            float radius = bounds.extents.magnitude + Vector3.Distance(bounds.center, targetPosition);
            Vector3 relativePosition = targetPosition - __instance.position;
            Vector3 relativeVelocity = targetVelocity - __instance.velocityVector_kps * 0.05f;
            float speedSquared = relativeVelocity.sqrMagnitude;
            float time = speedSquared > 0f ? -Vector3.Dot(relativePosition, relativeVelocity) / speedSquared : 0f;
            __result = time >= 0f && (relativePosition + Math.Max(0f, time) * relativeVelocity).sqrMagnitude <= radius * radius;
            return false;
        }
    }

    [HarmonyPatch(typeof(MissileController), "HasHitTarget")]
    public static class BattleMissileSweepPatch
    {
        public static float Padding() { return BattleScaleRuntime.Enabled ? 0f : 0.023f; }
        public static float Sweep() { return BattleScaleRuntime.Enabled ? 1f : 3.5f; }
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int padding = 0, sweep = 0;
            var result = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in result)
            {
                if (instruction.opcode != OpCodes.Ldc_R4 || !(instruction.operand is float)) continue;
                float value = (float)instruction.operand;
                if (Math.Abs(value - 0.023f) < 0.0000001f)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(BattleMissileSweepPatch), nameof(Padding));
                    padding++;
                }
                else if (Math.Abs(value - 3.5f) < 0.0000001f)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(BattleMissileSweepPatch), nameof(Sweep));
                    sweep++;
                }
            }
            if (padding != 1 || sweep != 1)
                throw new InvalidOperationException("Missile collision sweep changed: expected one padding and one sweep factor.");
            return result;
        }
    }
}
