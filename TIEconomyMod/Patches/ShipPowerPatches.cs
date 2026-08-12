using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

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
            ProjectileGeometryRegistry.Refresh();
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
            MethodInfo localizedThrust = AccessTools.Method(
                typeof(TIDriveTemplate), "GetLocalizedThrust");
            MethodInfo localizedPower = AccessTools.Method(
                typeof(TIDriveTemplate), "GetLocalizedRequiredPower");
            MethodInfo localizedCost = AccessTools.Method(
                typeof(TIShipPartTemplate), "GetLocalizedCost");
            MethodInfo scaledThrust = AccessTools.Method(
                typeof(ShipModuleEnergyColumnCompatibilityPatch),
                "GetHullScaledDriveThrust",
                new[] { typeof(TIDriveTemplate), typeof(ShipModuleListItem) });
            MethodInfo scaledPower = AccessTools.Method(
                typeof(ShipModuleEnergyColumnCompatibilityPatch),
                "GetHullScaledDrivePower",
                new[] { typeof(TIDriveTemplate), typeof(ShipModuleListItem) });
            MethodInfo scaledCost = AccessTools.Method(
                typeof(ShipModuleEnergyColumnCompatibilityPatch),
                "GetHullScaledDriveCost",
                new[] { typeof(TIShipPartTemplate), typeof(ShipModuleListItem) });
            int energyUsageCalls = 0;
            int replacements = 0;
            int localizedCostCalls = 0;
            int driveDisplayReplacements = 0;
            List<CodeInstruction> result = new List<CodeInstruction>();

            foreach (CodeInstruction instruction in patched)
            {
                if (instruction.Calls(energyUsage))
                {
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

                if (instruction.Calls(localizedThrust))
                {
                    AddListItemLoad(result, instruction);
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = scaledThrust;
                    driveDisplayReplacements++;
                }
                else if (instruction.Calls(localizedPower))
                {
                    AddListItemLoad(result, instruction);
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = scaledPower;
                    driveDisplayReplacements++;
                }
                else if (instruction.Calls(localizedCost))
                {
                    localizedCostCalls++;
                    if (localizedCostCalls == 3)
                    {
                        // The third part-cost call is in the drive branch.
                        AddListItemLoad(result, instruction);
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = scaledCost;
                        driveDisplayReplacements++;
                    }
                }

                result.Add(instruction);
            }

            if (energyUsageCalls != 2 || replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected two EnergyUsage_GJ calls and one visibility " +
                    "replacement in ShipModuleListItem.GenerateEntries; found " +
                    energyUsageCalls + " and " + replacements + ".");
            }

            if (localizedCostCalls != 3 || driveDisplayReplacements != 3)
            {
                throw new InvalidOperationException(
                    "Expected one drive thrust, one drive power, and the third " +
                    "of three localized cost calls in " +
                    "ShipModuleListItem.GenerateEntries; found " +
                    localizedCostCalls + " cost calls and " +
                    driveDisplayReplacements + " replacements.");
            }

            return result;
        }

        private static void AddListItemLoad(
            List<CodeInstruction> result, CodeInstruction originalCall)
        {
            CodeInstruction loadListItem =
                new CodeInstruction(OpCodes.Ldarg_0);
            loadListItem.labels.AddRange(originalCall.labels);
            loadListItem.blocks.AddRange(originalCall.blocks);
            originalCall.labels.Clear();
            originalCall.blocks.Clear();
            result.Add(loadListItem);
        }

        public static string GetHullScaledDriveThrust(
            TIDriveTemplate drive, ShipModuleListItem listItem)
        {
            float scale = DriveDisplayScale(drive, listItem);
            if (scale <= 1f)
            {
                return drive.GetLocalizedThrust();
            }

            float thrust_N = ShipBalanceMath.ScaledDriveValue(
                drive.thrust_N, scale);
            float thrustRating = 1f +
                (float)(Math.Log(thrust_N / 1000f) / Math.Log(2d));
            return Loc.T(
                "TIDriveTemplate.Thrust",
                thrust_N.ToString("N0"),
                thrustRating.ToString("N1"));
        }

        public static string GetHullScaledDrivePower(
            TIDriveTemplate drive, ShipModuleListItem listItem)
        {
            float power_GW = ShipBalanceMath.ScaledDriveValue(
                drive.powerRequirement_GW,
                DriveDisplayScale(drive, listItem));
            return TIUtilities.LocalizeGW(
                "UI.Fleets.RequiredPowerGW", power_GW);
        }

        public static string GetHullScaledDriveMass(
            TIDriveTemplate drive, TISpaceShipTemplate ship)
        {
            float mass_tons = ShipBalanceMath.ScaledDriveValue(
                drive.buildMass_tons(), DriveDisplayScale(drive, ship));
            return Loc.T(
                "UI.Fleets.Mass",
                TIUtilities.FormatBigOrSmallNumber(
                    mass_tons, 1, 7, 0, false, false));
        }

        public static string GetHullScaledCombatThrust(
            TIDriveTemplate drive,
            TISpaceShipTemplate shipTemplate,
            TISpaceShipState ship)
        {
            float thrust_N = ShipBalanceMath.ScaledDriveValue(
                drive.thrust_N, DriveDisplayScale(drive, shipTemplate));
            float thrustCap = ship == null
                ? drive.thrustCap
                : ship.modifiedThrustCap;
            return Loc.T(
                "TIDriveTemplate.CombatThrust",
                TIUtilities.FormatBigOrSmallNumber(thrustCap),
                TIUtilities.FormatBigOrSmallNumber(
                    thrust_N * thrustCap).ToString());
        }

        public static string GetHullScaledDriveCost(
            TIShipPartTemplate part, ShipModuleListItem listItem)
        {
            TIDriveTemplate drive = part as TIDriveTemplate;
            if (drive == null)
            {
                return part.GetLocalizedCost();
            }

            float scale = DriveDisplayScale(drive, listItem);
            if (scale <= 1f)
            {
                return drive.GetLocalizedCost();
            }

            TIResourcesCost cost = drive.buildCost().MultiplyCost(scale);
            return Loc.T(
                "UI.Fleets.Cost",
                cost.ToString(
                    "Relevant", false, false, null, false,
                    default(FactionResource)));
        }

        private static float DriveDisplayScale(
            TIDriveTemplate drive, ShipModuleListItem listItem)
        {
            TISpaceShipTemplate ship = listItem == null ||
                listItem.controller == null
                    ? null
                    : listItem.controller.newShipTemplate;
            return DriveDisplayScale(drive, ship);
        }

        private static float DriveDisplayScale(
            TIDriveTemplate drive, TISpaceShipTemplate ship)
        {
            return HullDriveScalingFeature.Multiplier(ship, drive);
        }

        public static string GetHullScaledDriveDescription(
            string description,
            TIDriveTemplate drive,
            TISpaceShipTemplate shipTemplate,
            TISpaceShipState ship)
        {
            if (string.IsNullOrEmpty(description) || drive == null ||
                DriveDisplayScale(drive, shipTemplate) <= 1f)
            {
                return description;
            }

            string baselineCombatThrust = drive.GetLocalizedCombatThrust(ship);
            string baselineThrust = drive.GetLocalizedThrust();
            string baselinePower = drive.GetLocalizedRequiredPower();
            string baselineMass = drive.GetLocalizedMass();
            string baselineCost = drive.GetLocalizedCost();
            return description
                .Replace(
                    baselineCombatThrust,
                    GetHullScaledCombatThrust(
                        drive, shipTemplate, ship))
                .Replace(
                    baselineThrust,
                    GetHullScaledDriveThrust(drive, shipTemplate))
                .Replace(
                    baselinePower,
                    GetHullScaledDrivePower(drive, shipTemplate))
                .Replace(
                    baselineMass,
                    GetHullScaledDriveMass(drive, shipTemplate))
                .Replace(
                    baselineCost,
                    GetHullScaledDriveCost(drive, shipTemplate));
        }

        public static string GetHullScaledDriveThrust(
            TIDriveTemplate drive, TISpaceShipTemplate ship)
        {
            float scale = DriveDisplayScale(drive, ship);
            if (scale <= 1f)
            {
                return drive.GetLocalizedThrust();
            }

            float thrust_N = ShipBalanceMath.ScaledDriveValue(
                drive.thrust_N, scale);
            float thrustRating = 1f +
                (float)(Math.Log(thrust_N / 1000f) / Math.Log(2d));
            return Loc.T(
                "TIDriveTemplate.Thrust",
                thrust_N.ToString("N0"),
                thrustRating.ToString("N1"));
        }

        public static string GetHullScaledDrivePower(
            TIDriveTemplate drive, TISpaceShipTemplate ship)
        {
            float power_GW = ShipBalanceMath.ScaledDriveValue(
                drive.powerRequirement_GW, DriveDisplayScale(drive, ship));
            return TIUtilities.LocalizeGW(
                "UI.Fleets.RequiredPowerGW", power_GW);
        }

        public static string GetHullScaledDriveCost(
            TIDriveTemplate drive, TISpaceShipTemplate ship)
        {
            float scale = DriveDisplayScale(drive, ship);
            if (scale <= 1f)
            {
                return drive.GetLocalizedCost();
            }

            TIResourcesCost cost = drive.buildCost().MultiplyCost(scale);
            return Loc.T(
                "UI.Fleets.Cost",
                cost.ToString(
                    "Relevant", false, false, null, false,
                    default(FactionResource)));
        }

        public static void RefreshHullScaledDriveRow(
            ShipModuleListItem row, TISpaceShipTemplate ship)
        {
            if (row == null || row.GetModuleTemplate() == null ||
                row.GetModuleTemplate().ref_drive == null)
            {
                return;
            }

            List<ShipModuleListItemEntry> entries = row.entries.ToList();
            if (entries.Count < 6)
            {
                return;
            }

            TIDriveTemplate drive = row.GetModuleTemplate().ref_drive;
            float scale = DriveDisplayScale(drive, ship);
            entries[1].textElement.text = SanitizeModuleTableText(
                GetHullScaledDriveThrust(drive, ship), false);
            entries[1].value = ShipBalanceMath.ScaledDriveValue(
                drive.thrust_N, scale);
            entries[4].textElement.text = SanitizeModuleTableText(
                GetHullScaledDrivePower(drive, ship), false);
            entries[4].value = ShipBalanceMath.ScaledDriveValue(
                drive.powerRequirement_GW, scale);
            entries[5].textElement.text = SanitizeModuleTableText(
                GetHullScaledDriveCost(drive, ship), true);
            entries[5].value = drive.buildCost().resourceCosts
                .Sum(resourceCost => resourceCost.value) * scale;
        }

        private static string SanitizeModuleTableText(
            string text, bool resourceCost)
        {
            text = Regex.Replace(text, "\\t|\\n|\\r", "");
            if (!resourceCost)
            {
                text = Regex.Replace(text, "<.*?>", "");
            }
            else
            {
                text = Regex.Replace(text, "</?align.*?>", "");
                text = Regex.Replace(text, "</?line-height.*?>", "");
                text = Regex.Replace(
                    text,
                    "(<sprite.*?>.*?){3}.*?[0-9.]+",
                    match => match.Value + "\n");
            }

            if (text.Contains(":"))
            {
                text = text.Split(new[] { ':' }, 2).Last();
            }

            return Regex.Replace(
                text,
                ".*?,.*?, ",
                match => match.Value.Trim(' ') + "\n");
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

    [HarmonyPatch(typeof(TIDriveTemplate), "GetDescriptionData")]
    public static class HullScaledDriveDescriptionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref string __result,
            TIDriveTemplate __instance,
            TISpaceShipState ship,
            TISpaceShipTemplate shipTemplate)
        {
            __result = ShipModuleEnergyColumnCompatibilityPatch
                .GetHullScaledDriveDescription(
                    __result, __instance, shipTemplate, ship);
        }
    }

    [HarmonyPatch(typeof(ShipModuleListItem), "ModuleTTString")]
    public static class HullScaledDriveTooltipPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref string __result,
            ShipModuleListItem __instance,
            TIShipPartTemplate module)
        {
            TIDriveTemplate drive = module == null
                ? null
                : module.ref_drive;
            TISpaceShipTemplate ship = __instance == null ||
                __instance.controller == null
                    ? null
                    : __instance.controller.newShipTemplate;
            __result = ShipModuleEnergyColumnCompatibilityPatch
                .GetHullScaledDriveDescription(
                    __result, drive, ship, null);
        }
    }

    [HarmonyPatch(typeof(ShipModuleListItem), "SetAlpha")]
    public static class HullScaledDriveTableRefreshPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipModuleListItem __instance)
        {
            if (__instance == null || !__instance.isRow ||
                __instance.controller == null)
            {
                return;
            }

            ShipModuleEnergyColumnCompatibilityPatch
                .RefreshHullScaledDriveRow(
                    __instance, __instance.controller.newShipTemplate);
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
