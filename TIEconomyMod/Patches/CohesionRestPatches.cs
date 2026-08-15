using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "cohesionRestState", MethodType.Getter)]
    public static class CohesionRestBaseValuePatch
    {
        private const float VanillaBaseValue = 16f;
        private static readonly MethodInfo ConfiguredBaseValueGetter =
            AccessTools.Method(typeof(CohesionRestBaseValuePatch),
                nameof(GetConfiguredBaseValue));

        public static float GetConfiguredBaseValue()
        {
            CohesionRestSettings settings = Main.settings.cohesionRest;
            return Main.FeatureEnabled(settings.enabled)
                ? settings.baseValue
                : VanillaBaseValue;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceBaseValue(instructions, "cohesionRestState", 1);
        }

        internal static IEnumerable<CodeInstruction> ReplaceBaseValue(
            IEnumerable<CodeInstruction> instructions, string targetName,
            int expectedReplacements)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            int replacements = 0;
            for (int index = 0; index < patched.Count; index++)
            {
                CodeInstruction instruction = patched[index];
                if (instruction.opcode != OpCodes.Ldc_R4 ||
                    !(instruction.operand is float value) ||
                    value != VanillaBaseValue)
                {
                    continue;
                }

                CodeInstruction replacement = new CodeInstruction(
                    OpCodes.Call, ConfiguredBaseValueGetter);
                replacement.labels.AddRange(instruction.labels);
                replacement.blocks.AddRange(instruction.blocks);
                patched[index] = replacement;
                replacements++;
            }

            if (replacements != expectedReplacements)
            {
                string message = "Cohesion rest-state IL changed: expected " +
                    expectedReplacements + " base-value constant(s) in " +
                    targetName + ", found " + replacements +
                    ". Refusing a partial patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }
    }

    [HarmonyPatch(typeof(TINationState), "CohesionRestStateDetail", MethodType.Getter)]
    public static class CohesionRestDetailBaseValuePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return CohesionRestBaseValuePatch.ReplaceBaseValue(instructions,
                "CohesionRestStateDetail", 2);
        }
    }
}
