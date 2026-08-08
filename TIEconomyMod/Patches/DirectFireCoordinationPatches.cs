using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.GamePlayScript.AI;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    internal static class DirectFireCoordinationFeature
    {
        public static bool Enabled
        {
            get
            {
                return Main.FeatureEnabled(Main.settings.shipBalance.enabled);
            }
        }
    }

    internal static class DirectFireCommitmentRegistry
    {
        private sealed class Commitment
        {
            public ProjectileController Projectile;
            public CombatShipController Target;
            public float ExpectedDamage_points;
        }

        private sealed class TargetCommitments
        {
            public readonly HashSet<ProjectileController> Projectiles =
                new HashSet<ProjectileController>();
            public float ExpectedDamage_points;
            public int LastPrunedFrame = -1;
        }

        [ThreadStatic]
        private static ProjectileWeapon firingWeapon;

        private static readonly Dictionary<ProjectileController, Commitment>
            commitmentsByProjectile =
                new Dictionary<ProjectileController, Commitment>();

        private static readonly Dictionary<CombatShipController, TargetCommitments>
            commitmentsByTarget =
                new Dictionary<CombatShipController, TargetCommitments>();

        public static ProjectileWeapon BeginFire(ProjectileWeapon weapon)
        {
            ProjectileWeapon previous = firingWeapon;
            firingWeapon = weapon;
            return previous;
        }

        public static void EndFire(ProjectileWeapon previous)
        {
            firingWeapon = previous;
        }

        public static void Register(ProjectileController projectile)
        {
            ProjectileWeapon weapon = firingWeapon;
            if (!DirectFireCoordinationFeature.Enabled ||
                projectile == null ||
                weapon == null ||
                weapon.weaponTemplate.isMissileWeapon)
            {
                return;
            }

            CombatShipController target = weapon.target as CombatShipController;
            if (!TargetIsActive(target))
            {
                return;
            }

            float range_km = SpaceCombatManager.scale_to_km(
                Vector3.Distance(projectile.position, weapon.targetedPosition));
            float expectedDamage =
                DirectFireCommitmentMath.SanitizeExpectedDamage_points(
                    weapon.weaponTemplate.BaseDamageAtRange_points(
                        range_km, false));
            if (expectedDamage <= 0f)
            {
                return;
            }

            Unregister(projectile);

            Commitment commitment = new Commitment
            {
                Projectile = projectile,
                Target = target,
                ExpectedDamage_points = expectedDamage
            };
            commitmentsByProjectile.Add(projectile, commitment);

            TargetCommitments targetCommitments;
            if (!commitmentsByTarget.TryGetValue(
                target, out targetCommitments))
            {
                targetCommitments = new TargetCommitments();
                commitmentsByTarget.Add(target, targetCommitments);
            }

            targetCommitments.Projectiles.Add(projectile);
            targetCommitments.ExpectedDamage_points += expectedDamage;
            targetCommitments.LastPrunedFrame = -1;
        }

        public static void Unregister(ProjectileController projectile)
        {
            if (ReferenceEquals(projectile, null))
            {
                return;
            }

            Commitment commitment;
            if (!commitmentsByProjectile.TryGetValue(
                projectile, out commitment))
            {
                return;
            }

            commitmentsByProjectile.Remove(projectile);

            TargetCommitments targetCommitments;
            if (!ReferenceEquals(commitment.Target, null) &&
                commitmentsByTarget.TryGetValue(
                    commitment.Target, out targetCommitments))
            {
                targetCommitments.Projectiles.Remove(projectile);
                targetCommitments.ExpectedDamage_points = Math.Max(
                    0f,
                    targetCommitments.ExpectedDamage_points -
                        commitment.ExpectedDamage_points);
                targetCommitments.LastPrunedFrame = -1;
                if (targetCommitments.Projectiles.Count == 0)
                {
                    commitmentsByTarget.Remove(commitment.Target);
                }
            }
        }

        public static bool IsSaturated(CombatShipController target)
        {
            if (!DirectFireCoordinationFeature.Enabled ||
                !TargetIsActive(target))
            {
                return false;
            }

            Prune(target);

            TargetCommitments targetCommitments;
            if (!commitmentsByTarget.TryGetValue(
                target, out targetCommitments))
            {
                return false;
            }

            float threshold =
                DirectFireCommitmentMath.EstimatedKillThreshold_points(
                    target.ShipState.hull.structuralIntegrity,
                    target.ShipState.sumArmorValue);
            return DirectFireCommitmentMath.IsSaturated(
                targetCommitments.ExpectedDamage_points, threshold);
        }

        public static void Clear()
        {
            commitmentsByProjectile.Clear();
            commitmentsByTarget.Clear();
            firingWeapon = null;
        }

        private static void Prune(CombatShipController target)
        {
            TargetCommitments targetCommitments;
            if (!commitmentsByTarget.TryGetValue(
                target, out targetCommitments))
            {
                return;
            }

            if (targetCommitments.LastPrunedFrame == Time.frameCount)
            {
                return;
            }
            targetCommitments.LastPrunedFrame = Time.frameCount;

            List<ProjectileController> stale =
                new List<ProjectileController>();
            foreach (ProjectileController projectile in
                targetCommitments.Projectiles)
            {
                if (ProjectileIsStale(projectile, target))
                {
                    stale.Add(projectile);
                }
            }

            foreach (ProjectileController projectile in stale)
            {
                Unregister(projectile);
            }
        }

        private static bool ProjectileIsStale(
            ProjectileController projectile, CombatShipController target)
        {
            if (projectile == null ||
                projectile.projectileState == null ||
                projectile.hasHit ||
                projectile.beenDestroyed ||
                !TargetIsActive(target))
            {
                return true;
            }

            return !TIUtilities.MovingTowardsTarget(
                target.position,
                target.velocityVector,
                projectile.position,
                projectile.velocityVector);
        }

        private static bool TargetIsActive(CombatShipController target)
        {
            return target != null &&
                !target.isDestroyed &&
                !target.destructionTriggered &&
                !target.departed;
        }
    }

    internal static class DirectFireTargetingRuntime
    {
        public static bool IsAutomaticTargetAvailable(
            CombatShipController shooter, CombatShipController target)
        {
            if (!DirectFireCoordinationFeature.Enabled ||
                shooter == null ||
                shooter.AI_IsMissileBoat)
            {
                return true;
            }

            return DirectFireCommitmentMath.IsAutomaticCandidateAvailable(
                DirectFireCommitmentRegistry.IsSaturated(target),
                HasUnsaturatedAutomaticShipTarget(shooter));
        }

        public static bool ShouldSuppressAutomaticFire(
            Weapon weapon, IDamageable target)
        {
            if (!DirectFireCoordinationFeature.Enabled ||
                weapon == null ||
                weapon.weaponTemplate.isMissileWeapon ||
                weapon.currentFireMode == null ||
                weapon.currentFireMode.mode != FireMode.Offense)
            {
                return false;
            }

            CombatShipController targetShip = target as CombatShipController;
            if (targetShip == null)
            {
                return false;
            }

            CombatShipController shooter =
                weapon.combatant.ref_shipController;
            if (shooter == null || shooter.AI_IsMissileBoat)
            {
                return false;
            }

            if (!shooter.ShipState.combatAIControl &&
                shooter.primaryTarget == targetShip)
            {
                return false;
            }

            return DirectFireCommitmentMath.ShouldSuppressSaturatedTarget(
                DirectFireCommitmentRegistry.IsSaturated(targetShip),
                HasUnsaturatedAutomaticShipTarget(shooter));
        }

        private static bool HasUnsaturatedAutomaticShipTarget(
            CombatShipController shooter)
        {
            if (shooter == null || shooter.enemyCombatants == null)
            {
                return false;
            }

            foreach (CombatantController combatant in shooter.enemyCombatants)
            {
                CombatShipController target =
                    combatant as CombatShipController;
                if (IsEligibleAutomaticShipTarget(target) &&
                    !DirectFireCommitmentRegistry.IsSaturated(target))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEligibleAutomaticShipTarget(
            CombatShipController target)
        {
            return target != null &&
                !target.isDestroyed &&
                !target.destructionTriggered &&
                !target.departed;
        }
    }

    [HarmonyPatch(typeof(ProjectileWeapon), "TryFire")]
    public static class ProjectileWeaponCommitmentContextPatch
    {
        [HarmonyPrefix]
        public static void Prefix(
            ProjectileWeapon __instance, out ProjectileWeapon __state)
        {
            __state = DirectFireCommitmentRegistry.BeginFire(__instance);
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(
            ProjectileWeapon __state, Exception __exception)
        {
            DirectFireCommitmentRegistry.EndFire(__state);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(BallisticProjectileController), "Fire")]
    public static class BallisticProjectileCommitmentPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BallisticProjectileController __instance)
        {
            DirectFireCommitmentRegistry.Register(__instance);
        }
    }

    [HarmonyPatch(typeof(ProjectileController), "Destruct")]
    public static class ProjectileCommitmentCleanupPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ProjectileController __instance)
        {
            DirectFireCommitmentRegistry.Unregister(__instance);
        }
    }

    [HarmonyPatch(typeof(ProjectileJobContainer), "ClearAllJobs")]
    public static class ProjectileCommitmentBattleCleanupPatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            DirectFireCommitmentRegistry.Clear();
            WeaponCadenceRuntime.Clear();
        }
    }

    [HarmonyPatch(typeof(TIAttackFireMode), "GetExpectedDamage")]
    public static class AutomaticFireCommitmentGatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TIAttackFireMode __instance,
            IDamageable target,
            ref float __result)
        {
            Weapon weapon = __instance.weapon as Weapon;
            if (__result > 0f &&
                DirectFireTargetingRuntime.ShouldSuppressAutomaticFire(
                    weapon, target))
            {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(FindTargetShipLeafNode), "TryAssignTargetShip")]
    public static class AutomaticShipTargetCommitmentGatePatch
    {
        private static readonly FieldInfo SharedDataField =
            AccessTools.Field(typeof(LeafNode), "_sharedData");

        private static readonly FieldInfo ShipControllerField =
            AccessTools.Field(
                typeof(CombatShipBehaviourTree.SharedBehaviourData),
                "ShipController");

        private static readonly MethodInfo CandidateAvailableMethod =
            AccessTools.Method(
                typeof(DirectFireTargetingRuntime),
                "IsAutomaticTargetAvailable");

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(instructions);
            List<CodeInstruction> patched =
                new List<CodeInstruction>(codes.Count + 18);
            int injections = 0;

            for (int index = 0; index < codes.Count; index++)
            {
                CodeInstruction instruction = codes[index];
                patched.Add(instruction);

                MethodInfo method = instruction.operand as MethodInfo;
                if (method == null ||
                    method.Name != "get_Current" ||
                    method.ReturnType != typeof(CombatShipController) ||
                    index + 1 >= codes.Count ||
                    !IsStoreLocal(codes[index + 1]))
                {
                    continue;
                }

                CodeInstruction store = codes[index + 1];
                object skipTarget = FindDestroyedBranchTarget(
                    codes, index + 2);
                if (skipTarget == null)
                {
                    throw new InvalidOperationException(
                        "Could not locate the candidate rejection branch " +
                        "for automatic ship targeting.");
                }

                index++;
                patched.Add(store);
                patched.Add(new CodeInstruction(OpCodes.Ldarg_0));
                patched.Add(new CodeInstruction(
                    OpCodes.Ldfld, SharedDataField));
                patched.Add(new CodeInstruction(
                    OpCodes.Ldfld, ShipControllerField));
                patched.Add(LoadStoredLocal(store));
                patched.Add(new CodeInstruction(
                    OpCodes.Call, CandidateAvailableMethod));
                patched.Add(new CodeInstruction(
                    OpCodes.Brfalse, skipTarget));
                injections++;
            }

            if (injections != 3)
            {
                throw new InvalidOperationException(
                    "Expected exactly three automatic ship target " +
                    "candidate loops, found " + injections + ".");
            }

            return patched;
        }

        private static object FindDestroyedBranchTarget(
            IList<CodeInstruction> codes, int start)
        {
            int end = Math.Min(codes.Count - 1, start + 10);
            for (int index = start; index < end; index++)
            {
                MethodInfo method = codes[index].operand as MethodInfo;
                if (method != null &&
                    method.Name == "get_isDestroyed" &&
                    (codes[index + 1].opcode == OpCodes.Brtrue ||
                     codes[index + 1].opcode == OpCodes.Brtrue_S))
                {
                    return codes[index + 1].operand;
                }
            }

            return null;
        }

        private static bool IsStoreLocal(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Stloc ||
                instruction.opcode == OpCodes.Stloc_S ||
                instruction.opcode == OpCodes.Stloc_0 ||
                instruction.opcode == OpCodes.Stloc_1 ||
                instruction.opcode == OpCodes.Stloc_2 ||
                instruction.opcode == OpCodes.Stloc_3;
        }

        private static CodeInstruction LoadStoredLocal(
            CodeInstruction store)
        {
            if (store.opcode == OpCodes.Stloc_0)
            {
                return new CodeInstruction(OpCodes.Ldloc_0);
            }
            if (store.opcode == OpCodes.Stloc_1)
            {
                return new CodeInstruction(OpCodes.Ldloc_1);
            }
            if (store.opcode == OpCodes.Stloc_2)
            {
                return new CodeInstruction(OpCodes.Ldloc_2);
            }
            if (store.opcode == OpCodes.Stloc_3)
            {
                return new CodeInstruction(OpCodes.Ldloc_3);
            }
            if (store.opcode == OpCodes.Stloc_S)
            {
                return new CodeInstruction(OpCodes.Ldloc_S, store.operand);
            }

            return new CodeInstruction(OpCodes.Ldloc, store.operand);
        }
    }
}
