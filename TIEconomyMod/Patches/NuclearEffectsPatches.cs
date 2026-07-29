using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIRegionState), "ApplyDamageToRegion")]
    public static class NuclearGlobalGdpPatch
    {
        private static readonly MethodInfo VanillaGdpPctChange = AccessTools.Method(
            typeof(TINationState),
            nameof(TINationState.GDPPctChange),
            new[]
            {
                typeof(float),
                typeof(TINationState.GDPChangeReason)
            });
        private static readonly MethodInfo ConfiguredGdpChange = AccessTools.Method(
            typeof(NuclearGlobalGdpPatch), nameof(ApplyConfiguredGlobalGdpChange));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> patched = new List<CodeInstruction>(instructions);
            Dictionary<int, int> replacedReasons = new Dictionary<int, int>();
            for (int index = 1; index < patched.Count; index++)
            {
                CodeInstruction instruction = patched[index];
                if ((instruction.opcode != OpCodes.Call &&
                     instruction.opcode != OpCodes.Callvirt) ||
                    !Equals(instruction.operand, VanillaGdpPctChange) ||
                    !TryReadSmallInteger(patched[index - 1], out int reason) ||
                    (reason != 3 && reason != 7 && reason != 8))
                {
                    continue;
                }

                CodeInstruction replacement = new CodeInstruction(
                    OpCodes.Call,
                    ConfiguredGdpChange);
                replacement.labels.AddRange(instruction.labels);
                replacement.blocks.AddRange(instruction.blocks);
                patched[index] = replacement;
                replacedReasons[reason] = replacedReasons.TryGetValue(
                    reason,
                    out int count)
                    ? count + 1
                    : 1;
            }

            if (replacedReasons.Count != 3 ||
                !replacedReasons.TryGetValue(3, out int regionDamageCount) ||
                !replacedReasons.TryGetValue(7, out int coreEconomicCount) ||
                !replacedReasons.TryGetValue(8, out int coreResourceCount) ||
                regionDamageCount != 1 ||
                coreEconomicCount != 1 ||
                coreResourceCount != 1)
            {
                string message =
                    "Nuclear regional-damage IL changed: expected global GDP reasons " +
                    "3, 7, and 8 exactly once. Refusing a partial compatibility patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        public static void ApplyConfiguredGlobalGdpChange(
            TINationState nation,
            float fraction,
            TINationState.GDPChangeReason reason)
        {
            if (Main.settings == null ||
                !Main.FeatureEnabled(Main.settings.environment.enabled))
            {
                nation.GDPPctChange(fraction, reason);
            }
        }

        private static bool TryReadSmallInteger(
            CodeInstruction instruction,
            out int value)
        {
            if (instruction.opcode == OpCodes.Ldc_I4_3)
            {
                value = 3;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldc_I4_7)
            {
                value = 7;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldc_I4_8)
            {
                value = 8;
                return true;
            }
            value = 0;
            return false;
        }
    }
}
