using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "OnUnityPriorityComplete")]
    public static class UnityPropagandaPatch
    {
        private static readonly FieldInfo VanillaStrength = AccessTools.Field(
            typeof(TIGlobalConfig), "unityPublicOpinionBaseStrength");
        private static readonly MethodInfo ConfiguredStrength = AccessTools.Method(
            typeof(UnityPropagandaPatch), nameof(GetConfiguredStrength));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Keep TI 1.0.51's complete method—including its current propaganda signature,
            // Religion CP bonus, Cohesion/Education effects, and claim-legitimization flow.
            // Only the one field load is replaced, turning vanilla strength into 20%.
            // A maintained-main full prefix would silently delete later vanilla behavior.
            List<CodeInstruction> patched = new List<CodeInstruction>();
            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld &&
                    Equals(instruction.operand, VanillaStrength))
                {
                    CodeInstruction replacement = new CodeInstruction(OpCodes.Call, ConfiguredStrength);
                    replacement.labels.AddRange(instruction.labels);
                    replacement.blocks.AddRange(instruction.blocks);
                    patched.Add(replacement);
                    replacements++;
                }
                else
                {
                    patched.Add(instruction);
                }
            }
            if (replacements != 1)
            {
                string message = "Unity propaganda IL changed: expected one strength field load, found " +
                    replacements + ". Refusing a partial compatibility patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        public static float GetConfiguredStrength(TIGlobalConfig config)
        {
            return config.unityPublicOpinionBaseStrength *
                (Main.FeatureEnabled(Main.settings.unity.enabled)
                    ? Main.settings.unity.propagandaMultiplier
                    : 1f);
        }
    }

    [HarmonyPatch(typeof(TINationState), "OnSpoilsPriorityComplete")]
    public static class SpoilsPropagandaPatch
    {
        private static readonly FieldInfo VanillaScaling = AccessTools.Field(typeof(TIGlobalConfig), "spoilsPriorityPublicOpinionScaling");
        private static readonly MethodInfo DirectEmissions = AccessTools.Method(typeof(TIGlobalValuesState), "AddSpoilsPriorityEnvEffect");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Keep every vanilla effect through AddToSustainability, then remove the final
            // eight instructions that calculate and inject a second CO2/CH4/N2O pulse.
            // Spoils now raises future GDP emissions only through Sustainability.
            List<CodeInstruction> patched = new List<CodeInstruction>(instructions);
            int scalingLoads = 0, emissionsCalls = 0;
            for (int index = 0; index < patched.Count; index++)
            {
                CodeInstruction instruction = patched[index];
                if (instruction.opcode == OpCodes.Ldfld &&
                    Equals(instruction.operand, VanillaScaling))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(
                        typeof(SpoilsPropagandaPatch), nameof(GetConfiguredScaling));
                    scalingLoads++;
                }
                else if ((instruction.opcode == OpCodes.Call ||
                    instruction.opcode == OpCodes.Callvirt) &&
                    Equals(instruction.operand, DirectEmissions))
                {
                    patched.RemoveRange(index - 7, 8);
                    index -= 8;
                    emissionsCalls++;
                }
            }
            if (scalingLoads != 1 || emissionsCalls != 1)
            {
                string message = "Spoils IL changed: expected one propaganda load and one direct-emissions call; found " +
                    scalingLoads + " and " + emissionsCalls + ".";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        public static float GetConfiguredScaling(TIGlobalConfig config)
        {
            return config.spoilsPriorityPublicOpinionScaling * (Main.FeatureEnabled(
                Main.settings.spoils.enabled) ? Main.settings.spoils.propagandaMultiplier : 1f);
        }
    }
}
