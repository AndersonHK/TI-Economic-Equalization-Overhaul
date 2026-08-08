using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Audio;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    internal sealed class WeaponCadenceState
    {
        public double accumulated_s;
    }

    internal static class WeaponCadenceRuntime
    {
        private static readonly AccessTools.FieldRef<
            SpaceCombatManager,
            List<CombatShipController>> ActiveShips =
                AccessTools.FieldRefAccess<
                    SpaceCombatManager,
                    List<CombatShipController>>("activeShips");

        private static readonly AccessTools.FieldRef<
            SpaceCombatManager,
            List<CombatHabModuleController>> HabModules =
                AccessTools.FieldRefAccess<
                    SpaceCombatManager,
                    List<CombatHabModuleController>>(
                        "combatHabModuleControllers");

        private static readonly AccessTools.FieldRef<
            SpaceCombatManager,
            bool> ShotFired =
                AccessTools.FieldRefAccess<SpaceCombatManager, bool>(
                    "shotFired");

        private static readonly AccessTools.FieldRef<
            SpaceCombatManager,
            TIDateTime> TimeOfLastShot =
                AccessTools.FieldRefAccess<
                    SpaceCombatManager,
                    TIDateTime>("timeOfLastShotFired");

        private static readonly Dictionary<
            SpaceCombatManager,
            WeaponCadenceState> States =
                new Dictionary<
                    SpaceCombatManager,
                    WeaponCadenceState>(
                        ReferenceIdentityComparer<
                            SpaceCombatManager>.Instance);

        public static bool SuppressNativeAcquireTarget(
            IWeapon weapon, DateTime currentTime)
        {
            return false;
        }

        public static void Run(
            SpaceCombatManager manager, double elapsed_s)
        {
            if (manager == null)
            {
                return;
            }

            WeaponCadenceState state;
            if (!States.TryGetValue(manager, out state))
            {
                state = new WeaponCadenceState();
                States.Add(manager, state);
            }

            int checks = WeaponCadenceMath.AccumulateChecks(
                ref state.accumulated_s, elapsed_s);
            if (checks == 0)
            {
                return;
            }

            TIDateTime timeState = TITimeState.Now();
            if (timeState == null)
            {
                return;
            }

            DateTime checkTime = timeState.ExportTime().AddSeconds(
                -WeaponCadenceMath.OldestCheckOffset_s(
                    state.accumulated_s, checks));
            for (int index = 0; index < checks; index++)
            {
                CheckAllWeapons(manager, checkTime, timeState);
                checkTime = checkTime.AddSeconds(
                    WeaponCadenceMath.CheckInterval_s);
            }
        }

        public static void Clear()
        {
            States.Clear();
        }

        private static void CheckAllWeapons(
            SpaceCombatManager manager,
            DateTime currentTime,
            TIDateTime timeState)
        {
            bool shipWeaponFired = false;
            bool anyWeaponFired = false;
            List<CombatShipController> ships = ActiveShips(manager);
            if (ships != null)
            {
                foreach (CombatShipController ship in ships)
                {
                    if (ship == null || ship.destructionTriggered ||
                        ship.hull == null)
                    {
                        continue;
                    }

                    foreach (IWeapon weapon in
                        ship.hull.IterateByClass<IWeapon>())
                    {
                        if (TryWeapon(weapon, currentTime))
                        {
                            shipWeaponFired = true;
                            anyWeaponFired = true;
                        }
                    }
                }
            }

            List<CombatHabModuleController> habModules =
                HabModules(manager);
            if (habModules != null)
            {
                foreach (CombatHabModuleController module in habModules)
                {
                    if (module == null || module.destructionTriggered ||
                        module.weapons == null)
                    {
                        continue;
                    }

                    foreach (IWeapon weapon in module.weapons)
                    {
                        if (TryWeapon(weapon, currentTime))
                        {
                            anyWeaponFired = true;
                        }
                    }
                }
            }

            if (shipWeaponFired)
            {
                if (!ShotFired(manager))
                {
                    AudioManager.SetIntensity(0.45f);
                }
                ShotFired(manager) = true;
            }
            if (anyWeaponFired)
            {
                TimeOfLastShot(manager) = timeState;
            }
        }

        private static bool TryWeapon(
            IWeapon weapon, DateTime currentTime)
        {
            if (weapon == null || !weapon.AcquireTarget(currentTime))
            {
                return false;
            }

            Weapon concreteWeapon = weapon as Weapon;
            if (concreteWeapon != null)
            {
                ShipWeaponVisController visualization =
                    concreteWeapon.SelectWeaponVisualization(
                        concreteWeapon.targetedPosition);
                if (visualization != null)
                {
                    visualization.RotateToTarget(false);
                }
            }
            return weapon.TryFire(currentTime);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager),
        "CombatQuarterSecond")]
    public static class NativeWeaponCadenceSuppressionPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo acquireTarget = AccessTools.Method(
                typeof(IWeapon), "AcquireTarget");
            MethodInfo replacement = AccessTools.Method(
                typeof(WeaponCadenceRuntime),
                "SuppressNativeAcquireTarget");
            int replacements = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(acquireTarget))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacement;
                    replacements++;
                }
                yield return instruction;
            }

            if (replacements != 2)
            {
                throw new InvalidOperationException(
                    "Expected two native one-second weapon acquisition " +
                    "calls, found " + replacements + ".");
            }
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager),
        "CombatFractionalSecond")]
    public static class FiftyMillisecondWeaponCadencePatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            SpaceCombatManager __instance, double timeElapsed_s)
        {
            WeaponCadenceRuntime.Run(__instance, timeElapsed_s);
        }
    }
}
