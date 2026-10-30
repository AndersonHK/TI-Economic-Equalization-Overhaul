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
    [HarmonyPatch(typeof(CombatGrid), "Update")]
    public static class BattleGridThicknessPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>(); int count = 0;
            foreach (var instruction in instructions)
            {
                MethodInfo method = instruction.operand as MethodInfo;
                if (method != null && method.Name == "set_Thickness" && method.DeclaringType.FullName == "Shapes.Line")
                {
                    var call = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualSize)));
                    call.labels.AddRange(instruction.labels); instruction.labels.Clear(); result.Add(call); count++;
                }
                result.Add(instruction);
            }
            if (count != 1) throw new InvalidOperationException("Combat grid thickness setter changed.");
            return result;
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "GetFormationScalingFactor")]
    public static class BattleFormationSpacingPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (BattleScaleRuntime.Enabled) __result *= CombatViewRuntime.Ratio;
        }
    }

    [HarmonyPatch(typeof(CombatShipController), "Initialize")]
    public static class BattleShipExplosionGeometryPatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(CombatShipController __instance)
        {
            if (BattleScaleRuntime.Enabled) CombatExplosionGeometry.BindShip(__instance);
        }
    }

    [HarmonyPatch(typeof(CombatShipController), "ToggleExplosions")]
    public static class BattleExplosionJitterPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>(); int small = 0, medium = 0, large = 0;
            foreach (var instruction in instructions)
            {
                result.Add(instruction);
                if (instruction.opcode != OpCodes.Ldc_R4 || !(instruction.operand is float)) continue;
                float value = (float)instruction.operand;
                if (Math.Abs(value-.1f)<.000001f) small++;
                else if (Math.Abs(value-.2f)<.000001f) medium++;
                else if (Math.Abs(value-.4f)<.000001f) large++;
                else continue;
                result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualSize))));
            }
            if (small != 2 || medium != 3 || large != 1) throw new InvalidOperationException("Ship explosion position jitter changed.");
            return result;
        }
    }

    [HarmonyPatch(typeof(RadiatorVisController), "OnRadiatorDestroyed")]
    public static class BattleRadiatorExplosionGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(RadiatorVisController __instance, ParticleSystem ___explosionParticles)
        {
            if (!BattleScaleRuntime.Enabled || !TIGlobalValuesState.isSpaceCombatEnabled || ___explosionParticles == null) return;
            CombatExplosionGeometry.MatchSpawn(___explosionParticles.transform, __instance.transform, __instance);
        }
    }

    [HarmonyPatch(typeof(ShipWeaponVisController), "OnWeaponDestroyedExplosion")]
    public static class BattleWeaponExplosionGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipWeaponVisController __instance)
        {
            if (!BattleScaleRuntime.Enabled || !TIGlobalValuesState.isSpaceCombatEnabled || __instance.weaponExplosionInstance == null) return;
            CombatExplosionGeometry.MatchSpawn(__instance.weaponExplosionInstance.transform, __instance.weaponObject.transform, __instance);
        }
    }

    [HarmonyPatch(typeof(CombatHabModuleController), "InitializeForCombat")]
    public static class BattleStationExplosionGeometryPatch
    {
        [HarmonyPostfix]
        public static void Postfix(CombatHabModuleController __instance)
        {
            if (!BattleScaleRuntime.Enabled || __instance.combatHitColliders == null) return;
            CombatStationExplosionObserver observer = __instance.GetComponent<CombatStationExplosionObserver>();
            if (observer == null) observer = __instance.gameObject.AddComponent<CombatStationExplosionObserver>();
            observer.Arm(__instance);
        }
    }

    [HarmonyPatch(typeof(ParticleGroupEffect), "OnPlay")]
    public static class BattleParticleGroupExplosionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ParticleGroupEffect __instance, List<GameObject> ___m_particleInstances)
        {
            if (!BattleScaleRuntime.Enabled || !TIGlobalValuesState.isSpaceCombatEnabled) return;
            Component owner = __instance.GetComponentInParent<CombatShipController>();
            if (owner == null) owner = __instance.GetComponentInParent<CombatHabModuleController>();
            if (owner == null) return;
            Transform anchor = __instance.m_spawnAnchor != null ? __instance.m_spawnAnchor : __instance.transform;
            Transform visual = anchor.GetComponent<MeshRenderer>() != null ? anchor : anchor.parent;
            if (visual == null || CombatExplosionGeometry.VisualDiameter(visual) <= 0f) visual = owner.transform;
            foreach (GameObject instance in ___m_particleInstances)
                if (instance != null) CombatExplosionGeometry.MatchSpawn(instance.transform, visual, owner);
        }
    }
}
