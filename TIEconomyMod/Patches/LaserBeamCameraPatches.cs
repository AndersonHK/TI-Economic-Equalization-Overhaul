using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(ShipWeaponVisController), "Fire")]
    public static class BattleLaserEmitterPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipWeaponVisController __instance, BeamWeaponController ___beamController)
        {
            if (!BattleScaleRuntime.Enabled || !TIGlobalValuesState.isSpaceCombatEnabled ||
                !(__instance.weaponTemplate is TILaserWeaponTemplate) || ___beamController == null) return;
            var effect = ___beamController.GetComponent<BeamEffectController>();
            if (effect == null) return;
            var geometry = effect.GetComponent<LaserBeamGeometry>();
            if (geometry == null) geometry = effect.gameObject.AddComponent<LaserBeamGeometry>();
            geometry.Bind(__instance);
        }
    }
    [HarmonyPatch(typeof(BeamEffectController), "OnWillRenderObject")]
    public static class BattleLaserRibbonPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(BeamEffectController __instance)
        {
            var geometry = __instance.GetComponent<LaserBeamGeometry>();
            return geometry == null || !geometry.Apply();
        }
    }
    [HarmonyPatch(typeof(BeamEffectController), "OnDisable")]
    public static class BattleLaserRibbonRestorePatch
    {
        [HarmonyPostfix]
        public static void Postfix(BeamEffectController __instance)
        {
            var geometry = __instance.GetComponent<LaserBeamGeometry>();
            if (geometry != null) geometry.Restore();
        }
    }
    [HarmonyPatch(typeof(SpaceCombatCameraController), "InitializeCamera")]
    public static class BattleInitialCameraScalePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var counts = new Dictionary<float, int> { { 400f, 0 }, {20f,0}, {-40f,0}, {3f,0}, {-6f,0} };
            var result = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                result.Add(instruction);
                if (instruction.opcode != OpCodes.Ldc_R4 || !(instruction.operand is float)) continue;
                float value = (float)instruction.operand;
                if (!counts.ContainsKey(value)) continue;
                counts[value]++;
                result.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CombatViewRuntime), nameof(CombatViewRuntime.VisualSize))));
            }
            if (counts[400f] != 2 || counts[20f] != 1 || counts[-40f] != 1 || counts[3f] != 1 || counts[-6f] != 1)
                throw new InvalidOperationException("Initial battle camera radius/offset constants changed.");
            return result;
        }
    }
}
