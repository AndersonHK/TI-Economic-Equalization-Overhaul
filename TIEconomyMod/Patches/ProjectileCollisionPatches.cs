using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

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

    internal static class ProjectileColliderSizer
    {
        private static bool unsupportedColliderReported;

        public static void Apply(
            ProjectileController projectile, float diameter_mm)
        {
            Collider collider = projectile.projectileCollider;
            if (collider == null || diameter_mm <= 0f)
            {
                return;
            }

            // Ship combat models use metres times modelScalingFactor. Use the
            // same cinematic scale for projectile caliber: much smaller than
            // BulletGun's generic box, while remaining stable in Unity physics.
            float worldDiameter =
                ProjectileCollisionMath.WorldDiameter_gameUnits(
                    diameter_mm,
                    GameControl.spaceCombat.modelScalingFactor);
            Vector3 scale = collider.transform.lossyScale;
            float scaleX = SafeScale(scale.x);
            float scaleY = SafeScale(scale.y);
            float scaleZ = SafeScale(scale.z);

            BoxCollider box = collider as BoxCollider;
            if (box != null)
            {
                box.size = new Vector3(
                    worldDiameter / scaleX,
                    worldDiameter / scaleY,
                    worldDiameter / scaleZ);
                return;
            }

            SphereCollider sphere = collider as SphereCollider;
            if (sphere != null)
            {
                sphere.radius = worldDiameter /
                    (2f * Math.Max(scaleX, Math.Max(scaleY, scaleZ)));
                return;
            }

            CapsuleCollider capsule = collider as CapsuleCollider;
            if (capsule != null)
            {
                float maximumScale = Math.Max(
                    scaleX, Math.Max(scaleY, scaleZ));
                capsule.radius = worldDiameter / (2f * maximumScale);
                capsule.height = 2f * capsule.radius;
                return;
            }

            string replacedType = collider.GetType().Name;
            collider.enabled = false;
            BoxCollider replacement =
                collider.gameObject.AddComponent<BoxCollider>();
            replacement.center = Vector3.zero;
            replacement.size = new Vector3(
                worldDiameter / scaleX,
                worldDiameter / scaleY,
                worldDiameter / scaleZ);
            projectile.hitColliders.Remove(collider);
            projectile.hitColliders.Add(replacement);
            projectile.projectileCollider = replacement;

            if (!unsupportedColliderReported)
            {
                unsupportedColliderReported = true;
                Main.Warn("Replaced unsupported projectile collider type '" +
                    replacedType + "' with a caliber-sized box.");
            }
        }

        private static float SafeScale(float value)
        {
            return Math.Max(0.000001f, Math.Abs(value));
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

            float diameter_mm;
            if (ProjectileGeometryRegistry.TryGetDiameter_mm(
                __instance.projectileState.originWeapon, out diameter_mm))
            {
                ProjectileColliderSizer.Apply(__instance, diameter_mm);
            }
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
            foreach (CodeInstruction instruction in patched)
            {
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
            }

            if (replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected exactly one ballistic movement sweep " +
                    "multiplier, found " + replacements + ".");
            }

            return patched;
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
