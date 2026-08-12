using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIArmyState), "AssaultAlienAsset")]
    public static class AlienFloraArmyDamagePatch
    {
        private static readonly MethodInfo AlienFaction = AccessTools.Method(
            typeof(GameStateManager), nameof(GameStateManager.AlienFaction));
        private static readonly MethodInfo ScaleDamageMethod = AccessTools.Method(
            typeof(AlienFloraArmyDamagePatch), nameof(ScaleDamage));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            // TI 1.0.51 computes its final damage immediately before loading the
            // Alien faction for TakeDamage. Insert only the flora-level scale at
            // that point, preserving its outcome ranges and technology divisor.
            List<CodeInstruction> patched = new List<CodeInstruction>();
            int replacements = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call &&
                    Equals(instruction.operand, AlienFaction))
                {
                    CodeInstruction loadAsset =
                        new CodeInstruction(OpCodes.Ldarg_1);
                    loadAsset.labels.AddRange(instruction.labels);
                    loadAsset.blocks.AddRange(instruction.blocks);
                    instruction.labels.Clear();
                    instruction.blocks.Clear();
                    patched.Add(loadAsset);
                    patched.Add(new CodeInstruction(
                        OpCodes.Call, ScaleDamageMethod));
                    replacements++;
                }
                patched.Add(instruction);
            }

            if (replacements != 1)
            {
                string message = "Alien-flora army-damage IL changed: expected " +
                    "one AlienFaction load before TakeDamage, found " +
                    replacements + ". Refusing a partial compatibility patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        public static float ScaleDamage(
            float vanillaDamage,
            TIRegionAlienAssetState alienAsset)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(
                    settings.enabled &&
                    settings.alienFloraDamageScalingEnabled) ||
                alienAsset == null ||
                !alienAsset.isRegionXenoformingState ||
                alienAsset.ref_xenoforming == null)
            {
                return vanillaDamage;
            }

            double scaled = AlienFloraAssaultMath.ScaledDamage(
                vanillaDamage,
                alienAsset.ref_xenoforming.xenoformingLevel,
                settings.alienFloraFullDamageLevel);
            if (!MilitaryMath.IsFinite(scaled) ||
                scaled < 0d || scaled > float.MaxValue)
            {
                Main.Warn("Alien-flora assault damage was invalid; retaining vanilla.");
                return vanillaDamage;
            }
            return (float)scaled;
        }
    }
}
