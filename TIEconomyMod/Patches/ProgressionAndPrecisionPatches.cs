using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIShipHullTemplate), "get_description")]
    public static class ShipTierDescriptionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TIShipHullTemplate __instance, ref string __result)
        {
            int tier = CapitalMountPolicy.ShipTier(__instance.dataName);
            if (tier == 0) return;
            __result += "\n" + Loc.T("UI.Fleets.ModShipTier", tier);
            if (!CapitalMountPolicy.Applies(__instance.dataName)) return;
            __result += "\n" + Loc.T("UI.Fleets.CompactHeavyHullRules");
            if (__instance.noseHardpoints > 0)
                __result += "\n" + Loc.T("UI.Fleets.CapitalNoseGradeRules", CapitalMountPolicy.NoseGrade(__instance.dataName));
        }
    }

    [HarmonyPatch(typeof(TIShipWeaponTemplate), nameof(TIShipWeaponTemplate.GetLocalizedMountType))]
    public static class Size5NoseMountTypePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIShipWeaponTemplate __instance, ref string __result)
        {
            if (!CapitalMountPolicy.IsSize5(__instance.dataName)) return true;
            __result = Loc.T("UI.Fleets.Size5NoseMount");
            return false;
        }
    }

    public static class LaserWavelengthFeature
    {
        private static readonly FieldInfo Spot = AccessTools.Field(typeof(TILaserWeaponTemplate), "_SpotDiameterPreciseFactor_m");
        private static readonly FieldInfo Ranges = AccessTools.Field(typeof(TILaserWeaponTemplate), "_damageMinRange");
        private static readonly FieldInfo Scores = AccessTools.Field(typeof(TIShipWeaponTemplate), "_combatScoresForRoles");

        public static void Validate()
        {
            if (Spot == null || Spot.FieldType != typeof(float) || Ranges == null || Scores == null ||
                AccessTools.Field(typeof(TILaserWeaponTemplate), "wavelength_nm")?.FieldType != typeof(int))
                throw new MissingFieldException("Laser wavelength precision/cache contract changed.");
        }

        public static float PhysicalMetres(TILaserWeaponTemplate weapon)
        {
            return WavelengthPrecision.Metres(weapon.dataName, weapon.wavelength_nm);
        }

        public static void Invalidate(TILaserWeaponTemplate weapon)
        {
            if (weapon == null || !WavelengthPrecision.HasPrecision(weapon.dataName, weapon.wavelength_nm)) return;
            Spot.SetValue(weapon, -1f);
            (Ranges.GetValue(weapon) as IDictionary)?.Clear();
            (Scores.GetValue(weapon) as IDictionary)?.Clear();
        }

        public static void Refresh()
        {
            if (TemplateManager.self == null) return;
            foreach (TIShipWeaponTemplate weapon in TemplateManager.IterateByClass<TIShipWeaponTemplate>())
            {
                if (CapitalMountPolicy.IsSize5(weapon.dataName)) (Scores.GetValue(weapon) as IDictionary)?.Clear();
                Invalidate(weapon as TILaserWeaponTemplate);
            }
        }
    }

    [HarmonyPatch(typeof(TILaserWeaponTemplate), "get_wavelength_m")]
    public static class LaserWavelengthMetresPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TILaserWeaponTemplate __instance, ref float __result)
        {
            if (!WavelengthPrecision.HasPrecision(__instance.dataName, __instance.wavelength_nm)) return true;
            __result = LaserWavelengthFeature.PhysicalMetres(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(TILaserWeaponTemplate), "SpotDiameterPrecise_m")]
    public static class LaserWavelengthSpotPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getter = AccessTools.PropertyGetter(typeof(TILaserWeaponTemplate), "wavelength_m");
            MethodInfo physical = AccessTools.Method(typeof(LaserWavelengthFeature), nameof(LaserWavelengthFeature.PhysicalMetres));
            var result = new List<CodeInstruction>(); int count = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(getter)) { instruction.opcode = OpCodes.Call; instruction.operand = physical; count++; }
                result.Add(instruction);
            }
            if (count != 1) throw new InvalidOperationException("Expected one laser physical-wavelength conversion edge, found " + count);
            return result;
        }
    }

    [HarmonyPatch(typeof(TILaserWeaponTemplate), nameof(TILaserWeaponTemplate.GetLocalizedWaveLength))]
    public static class LaserWavelengthDescriptionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TILaserWeaponTemplate __instance, ref string __result)
        {
            if (!WavelengthPrecision.HasPrecision(__instance.dataName, __instance.wavelength_nm)) return true;
            __result = Loc.T("TILaserWeaponTemplate.Wavelength", WavelengthPrecision.NanometresText(__instance.dataName, __instance.wavelength_nm));
            return false;
        }
    }
}
