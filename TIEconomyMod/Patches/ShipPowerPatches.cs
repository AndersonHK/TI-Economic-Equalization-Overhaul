using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    internal static class ShipPowerFeature
    {
        public static bool Enabled
        {
            get
            {
                return Main.FeatureEnabled(Main.settings.shipBalance.enabled);
            }
        }

        public static bool ThermalAccountingEnabled
        {
            get
            {
                return Enabled &&
                    Main.settings.shipBalance.correctPowerPlantWasteHeat;
            }
        }
    }

    public static class ShipPowerRuntime
    {
        public static void RefreshTemplateMassCaches()
        {
            if (TemplateManager.self == null ||
                !TemplateManager.self.Initialized)
            {
                return;
            }

            foreach (TISpaceShipTemplate template in
                TemplateManager.GetAllTemplates<TISpaceShipTemplate>())
            {
                template.dryMass_tons(true);
            }
        }
    }

    [HarmonyPatch(typeof(TemplateManager), "Initialize")]
    public static class GunPowerTemplateInitializationPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            GunPowerRegistry.Refresh();
            ShipPowerRuntime.RefreshTemplateMassCaches();
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState),
        "PostGlobalGameStateCreateInit_2")]
    public static class ShipPowerSaveLoadCachePatch
    {
        private static readonly AccessTools.FieldRef<TISpaceShipState, float>
            CurrentMass = AccessTools.FieldRefAccess<TISpaceShipState, float>(
                "<currentMass_kg>k__BackingField");

        [HarmonyPostfix]
        public static void Postfix(TISpaceShipState __instance)
        {
            if (!ShipPowerFeature.Enabled)
            {
                return;
            }

            // Templates are process data, not save data. Reconcile the serialized
            // live mass with the freshly recalculated dry mass and retained fuel.
            __instance.template.dryMass_tons(true);
            CurrentMass(__instance) = __instance.template.dryMass_kg +
                __instance.propellant_tons * 1000f;
            __instance.SetPropulsionValuesDirty();
        }
    }

    [HarmonyPatch(typeof(TIGunTemplate), "selfPowered", MethodType.Getter)]
    public static class GunSelfPoweredPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result, TIGunTemplate __instance)
        {
            float powerUse_MJ;
            if (!ShipPowerFeature.Enabled ||
                !GunPowerRegistry.TryGetPowerUse_MJ(
                    __instance, out powerUse_MJ))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIGunTemplate), "EnergyUsage_GJ")]
    public static class GunEnergyUsagePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TIGunTemplate __instance, float __0)
        {
            float powerUse_MJ;
            if (!ShipPowerFeature.Enabled ||
                !GunPowerRegistry.TryGetPowerUse_MJ(
                    __instance, out powerUse_MJ))
            {
                return true;
            }

            __result = WeaponPowerMath.ElectricalInput_GJ(
                powerUse_MJ, __0, __instance.efficiency);
            return false;
        }
    }

    [HarmonyPatch(typeof(TIGunTemplate), "HeatGeneration_GJ")]
    public static class GunHeatGenerationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TIGunTemplate __instance, float __0)
        {
            float powerUse_MJ;
            if (!ShipPowerFeature.Enabled ||
                !GunPowerRegistry.TryGetPowerUse_MJ(
                    __instance, out powerUse_MJ))
            {
                return true;
            }

            float electricalInput_GJ = WeaponPowerMath.ElectricalInput_GJ(
                powerUse_MJ, __0, __instance.efficiency);
            __result = WeaponPowerMath.ModuleWasteHeat_GJ(
                electricalInput_GJ, __instance.efficiency);
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipModuleListItem), "GenerateEntries")]
    public static class ShipModuleEnergyColumnCompatibilityPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            MethodInfo energyUsage = AccessTools.Method(
                typeof(TIShipWeaponTemplate), "EnergyUsage_GJ");
            MethodInfo visibilityValue = AccessTools.Method(
                typeof(ShipModuleEnergyColumnCompatibilityPatch),
                "EnergyUsageForTableVisibility");
            int energyUsageCalls = 0;
            int replacements = 0;

            foreach (CodeInstruction instruction in patched)
            {
                if (!instruction.Calls(energyUsage))
                {
                    continue;
                }

                energyUsageCalls++;
                if (energyUsageCalls == 1)
                {
                    // The first call controls whether the Energy Usage cell is
                    // created; the second supplies the displayed real value.
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = visibilityValue;
                    replacements++;
                }
            }

            if (energyUsageCalls != 2 || replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected two EnergyUsage_GJ calls and one visibility " +
                    "replacement in ShipModuleListItem.GenerateEntries; found " +
                    energyUsageCalls + " and " + replacements + ".");
            }

            return patched;
        }

        public static float EnergyUsageForTableVisibility(
            TIShipWeaponTemplate weapon, float extraInput_MJ)
        {
            float actual = weapon.EnergyUsage_GJ(extraInput_MJ);
            if (actual > 0f || !ShipPowerFeature.Enabled ||
                !weapon.isGunTypeWeapon)
            {
                return actual;
            }

            TIGunTypeWeaponTemplate gun = weapon.ref_gunWeapon;
            if (gun == null || gun.isMagneticGunWeapon || gun.isPlasmaWeapon)
            {
                return actual;
            }

            // TI sizes every visible row against shared labels but creates the
            // Energy Usage cell conditionally. Once some conventional guns are
            // powered, retain a zero-valued cell on the remaining conventional
            // guns so late-game unlock sets keep a consistent column shape.
            return float.Epsilon;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "wasteHeat_GW", MethodType.Getter)]
    public static class PoweredWeaponRadiatorHeatPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            if (!ShipPowerFeature.ThermalAccountingEnabled)
            {
                return;
            }

            float weaponHeat_GW = 0f;
            foreach (TIShipWeaponTemplate weapon in
                __instance.allWeaponTemplates)
            {
                if (weapon.selfPowered)
                {
                    continue;
                }

                float heatPerShot_GJ = weapon.HeatGeneration_GJ(
                    __instance.GetBonusPowerForWeapon_GJ(weapon));
                weaponHeat_GW += WeaponPowerMath.DesignHeatRate_GW(
                    heatPerShot_GJ,
                    weapon.salvo_shots,
                    weapon.cooldown_s,
                    weapon.intraSalvoCooldown_s);
            }

            __result += weaponHeat_GW;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState),
        "WeaponFireExceedsHeatCapacity")]
    public static class WeaponHeatCapacityPrecheckPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref bool __result,
            TISpaceShipState __instance,
            ModuleDataEntry module)
        {
            if (!ShipPowerFeature.ThermalAccountingEnabled)
            {
                return true;
            }

            if (__instance.radiatorsExtended &&
                !__instance.SystemDestroyed(ShipSystem.Radiators))
            {
                __result = false;
                return false;
            }

            TIShipWeaponTemplate weapon = module.moduleTemplate.ref_weapon;
            float shotHeat_GJ = weapon.HeatGeneration_GJ(
                __instance.GetBonusPowerForWeapon_GJ(weapon));
            __result = shotHeat_GJ > 0f &&
                __instance.accumulatedHeat_GJ + shotHeat_GJ >
                    __instance.currentHeatSinkCapacity_GJ;
            return false;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState), "PerSecondPowerGain")]
    public static class AuxiliaryElectricalGenerationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result,
            TISpaceShipState __instance,
            float ____auxReactorPowerGenerationRequirement_GW)
        {
            if (!ShipPowerFeature.ThermalAccountingEnabled)
            {
                return;
            }

            float grossAuxiliaryPower_GW =
                __instance.GetSystemFunction(ShipSystem.SystemsReactor) *
                __instance.GetSystemFunction(ShipSystem.PowerCoupling) *
                ____auxReactorPowerGenerationRequirement_GW;
            float efficiency = Math.Min(
                1f, Math.Max(0f, __instance.powerPlant.efficiency));
            __result -= grossAuxiliaryPower_GW *
                (1f - efficiency);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState),
        "CombatPerQuarterSecondChanges")]
    public static class GeneratedPowerHeatPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ReplaceSingleApplyHeatCall(
                instructions,
                AccessTools.Method(typeof(GeneratedPowerHeatPatch),
                    "ApplyCorrectedGenerationHeat"),
                "CombatPerQuarterSecondChanges");
        }

        public static void ApplyCorrectedGenerationHeat(
            TISpaceShipState ship, float vanillaHeat_GJ, bool triggerUpdate)
        {
            if (!ShipPowerFeature.ThermalAccountingEnabled)
            {
                ship.ApplyHeat(vanillaHeat_GJ, triggerUpdate);
                return;
            }

            float efficiency = Math.Min(
                1f, Math.Max(0.0001f, ship.powerPlant.efficiency));
            ship.ApplyHeat(
                vanillaHeat_GJ / efficiency, triggerUpdate);
        }

        internal static IEnumerable<CodeInstruction> ReplaceSingleApplyHeatCall(
            IEnumerable<CodeInstruction> instructions,
            System.Reflection.MethodInfo replacement,
            string methodName)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            System.Reflection.MethodInfo applyHeat = AccessTools.Method(
                typeof(TISpaceShipState), "ApplyHeat");
            int replacements = 0;
            foreach (CodeInstruction instruction in patched)
            {
                if (instruction.Calls(applyHeat))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacement;
                    replacements++;
                }
            }

            if (replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected exactly one ApplyHeat call in " + methodName +
                    ", found " + replacements + ".");
            }

            return patched;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState), "CombatPerSecondChanges")]
    public static class DuplicateSystemsHeatPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return GeneratedPowerHeatPatch.ReplaceSingleApplyHeatCall(
                instructions,
                AccessTools.Method(typeof(DuplicateSystemsHeatPatch),
                    "ApplyLegacySystemsHeat"),
                "CombatPerSecondChanges");
        }

        public static void ApplyLegacySystemsHeat(
            TISpaceShipState ship, float vanillaHeat_GJ, bool triggerUpdate)
        {
            if (!ShipPowerFeature.ThermalAccountingEnabled)
            {
                ship.ApplyHeat(vanillaHeat_GJ, triggerUpdate);
            }
        }
    }
}
