using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using System.Reflection;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    internal static class ProjectileCollisionFeature
    {
        public static bool Enabled
        {
            get
            {
                return Main.FeatureEnabled(
                    Main.settings.shipBalance.enabled);
            }
        }
    }

    [HarmonyPatch(typeof(BallisticProjectileController), "Fire")]
    public static class ProjectileColliderSizingPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BallisticProjectileController __instance)
        {
            if (!ProjectileCollisionFeature.Enabled)
            {
                return;
            }

            TIEconomyMod.Core.BattleProjectileGeometry.Apply(__instance);
        }
    }

    [HarmonyPatch(
        typeof(TISpaceCombatProjectileState),
        "CrossSectionalArea_m2",
        new Type[] { typeof(TIShipWeaponTemplate), typeof(float) })]
    public static class ProjectileCrossSectionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TIShipWeaponTemplate weaponTemplate)
        {
            TIProjectileWeaponTemplate projectileTemplate =
                weaponTemplate as TIProjectileWeaponTemplate;
            float diameter_mm;
            if (!ProjectileCollisionFeature.Enabled ||
                projectileTemplate == null ||
                !ProjectileGeometryRegistry.TryGetDiameter_mm(
                    projectileTemplate, out diameter_mm))
            {
                return true;
            }

            __result = ProjectileCollisionMath.CrossSectionalArea_m2(
                diameter_mm);
            return false;
        }
    }

    [HarmonyPatch(typeof(BallisticProjectileController), "UpdateController")]
    public static class ProjectileMovementSweepPatch
    {
        public static float ActiveMovementSweepMultiplier()
        {
            return ProjectileCollisionFeature.Enabled
                ? ProjectileCollisionMath.MovementSweepMultiplier
                : 1.2f;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            int replacements = 0;
            int casts=0,reflections=0;
            var result=new List<CodeInstruction>();
            foreach (CodeInstruction instruction in patched)
            {
                MethodInfo method=instruction.operand as MethodInfo;
                if(method!=null&&method.DeclaringType==typeof(Physics)&&method.Name=="RaycastAll")
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();
                    result.Add(load);instruction.operand=AccessTools.Method(typeof(BallisticContactRuntime),nameof(BallisticContactRuntime.CastCompletedStep));casts++;
                }
                if(method!=null&&method.DeclaringType==typeof(Vector3)&&method.Name=="Reflect")
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();
                    result.Add(load);instruction.operand=AccessTools.Method(typeof(BallisticContactRuntime),nameof(BallisticContactRuntime.ReflectFlight));reflections++;
                }
                if (instruction.opcode == OpCodes.Ldc_R4 &&
                    instruction.operand is float &&
                    Math.Abs((float)instruction.operand - 1.2f) < 0.000001f)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(
                        typeof(ProjectileMovementSweepPatch),
                        "ActiveMovementSweepMultiplier");
                    replacements++;
                }
                result.Add(instruction);
            }

            if (replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected exactly one ballistic movement sweep " +
                    "multiplier, found " + replacements + ".");
            }

            if(casts!=1||reflections!=1)throw new InvalidOperationException("Ballistic physical contact call sites changed.");
            return result;
        }
    }

    [HarmonyPatch(typeof(ProjectileController), "ApplyDamage")]
    public static class NavalGunProjectileDurabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ProjectileController __instance,
            DamageSource source,
            ref float __result)
        {
            if (!ProjectileCollisionFeature.Enabled ||
                __instance.projectileState.originWeapon.weaponClass !=
                    WeaponClass.NavalGun)
            {
                return true;
            }

            float massDamage_kg = ProjectileCollisionMath.MassDamage_kg(
                source.damage.amount,
                source.damage.chippingAmount);
            __instance.projectileState.massDamage_kg += massDamage_kg;
            if (__instance.projectileState.effectiveMass_kg <= 0f)
            {
                __instance.Destruct();
            }

            __result = source.damage.amount;
            return false;
        }
    }
}
