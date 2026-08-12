using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.DesignAlienShip))]
    public static class AlienShipArmorAllocationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            int replacements = 0;
            for (int index = 0; index < patched.Count; index++)
            {
                CodeInstruction instruction = patched[index];
                if (instruction.opcode != OpCodes.Ldc_R4 ||
                    !(instruction.operand is float value) ||
                    value != 3500f)
                {
                    continue;
                }

                CodeInstruction replacement =
                    new CodeInstruction(OpCodes.Ldc_R4, 10000f);
                replacement.labels.AddRange(instruction.labels);
                replacement.blocks.AddRange(instruction.blocks);
                patched[index] = replacement;
                replacements++;
            }

            if (replacements != 1)
            {
                string message =
                    "Alien ship-design IL changed: expected one 3500 kg/m3 " +
                    "armor-allocation constant. Refusing a partial patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }
    }
}
