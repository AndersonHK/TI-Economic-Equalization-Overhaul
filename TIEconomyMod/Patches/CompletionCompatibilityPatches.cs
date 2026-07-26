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
            // Keep TI 1.0.39's complete method—including its current propaganda signature,
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
        private static readonly FieldInfo VanillaScaling = AccessTools.Field(
            typeof(TIGlobalConfig), "spoilsPriorityPublicOpinionScaling");
        private static readonly MethodInfo ConfiguredScaling = AccessTools.Method(
            typeof(SpoilsPropagandaPatch), nameof(GetConfiguredScaling));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Preserve vanilla's entire Spoils completion: payout distribution, Aristocracy
            // and Extractive Sector CP effects, corruption checks, Government/Inequality,
            // Sustainability, and emissions. Only anti-propaganda strength is reduced to
            // 20%, preventing linear IP from multiplying its nonlinear population effect.
            List<CodeInstruction> patched = new List<CodeInstruction>();
            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld &&
                    Equals(instruction.operand, VanillaScaling))
                {
                    CodeInstruction replacement = new CodeInstruction(OpCodes.Call, ConfiguredScaling);
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
                string message = "Spoils propaganda IL changed: expected one scaling field load, found " +
                    replacements + ". Refusing a partial compatibility patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        public static float GetConfiguredScaling(TIGlobalConfig config)
        {
            return config.spoilsPriorityPublicOpinionScaling *
                (Main.FeatureEnabled(Main.settings.spoils.enabled)
                    ? Main.settings.spoils.propagandaMultiplier
                    : 1f);
        }
    }
}
