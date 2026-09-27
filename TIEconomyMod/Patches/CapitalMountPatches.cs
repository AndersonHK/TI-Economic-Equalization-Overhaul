using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.UI;
using PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIShipHullTemplate), nameof(TIShipHullTemplate.WeaponSlotSet))]
    public static class CapitalWeaponFootprintPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIShipHullTemplate __instance, TIShipHullTemplate.ShipModuleSlot coreSlot,
            Mount mount, ref List<TIShipHullTemplate.ShipModuleSlot> __result)
        {
            if (!CapitalMountPolicy.Applies(__instance.dataName) || coreSlot.moduleSlotType != ShipModuleSlotType.HullHardPoint)
                return true;
            __result = new List<TIShipHullTemplate.ShipModuleSlot>();
            if (mount == Mount.FourHull) __result.Add(coreSlot);
            return false;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.ValidPartForDesign))]
    public static class CapitalWeaponAvailabilityPatch
    {
        internal static bool HiddenFromCatalog(TISpaceShipTemplate design, TIShipPartTemplate part)
        {
            TIShipWeaponTemplate weapon = part?.ref_weapon;
            return design != null && CapitalMountPolicy.Applies(design.hullName) &&
                weapon != null && weapon.hullWeapon && weapon.mount != Mount.FourHull;
        }

        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance, TIShipPartTemplate part, ref bool __result)
        {
            if (HiddenFromCatalog(__instance, part)) __result = false;
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), "FilterAvailableShipModules")]
    public static class CapitalWeaponCatalogPatch
    {
        private static readonly MethodInfo RefreshWidths = AccessTools.Method(typeof(FleetsScreenController), "RefreshModuleTableWidths");

        [HarmonyPostfix]
        public static void Postfix(FleetsScreenController __instance, bool ___loadingExistingTemplate,
            List<ShipModuleListItem> ___shipModuleListItems, List<ShipModuleListItem> ___shipModuleListItemsB)
        {
            if (___loadingExistingTemplate || !CapitalMountPolicy.Applies(__instance.newShipTemplate?.hullName)) return;
            // Native filtering restores eligible rows on each hull/obsolete/view
            // change. Only remove forbidden weapons; never reveal locked parts.
            HideRows(__instance.newShipTemplate, ___shipModuleListItems);
            if (HideRows(__instance.newShipTemplate, ___shipModuleListItemsB))
                RefreshWidths.Invoke(__instance, null);
        }

        private static bool HideRows(TISpaceShipTemplate design, List<ShipModuleListItem> rows)
        {
            bool changed = false;
            foreach (ShipModuleListItem row in rows)
            {
                if (!CapitalWeaponAvailabilityPatch.HiddenFromCatalog(design, row.GetModuleTemplate())) continue;
                row.draggable = false;
                if (row.addModuleButton != null) row.addModuleButton.interactable = false;
                if (!row.gameObject.activeSelf) continue;
                row.gameObject.SetActive(false);
                changed = true;
            }
            return changed;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.ValidAssignedSlotForLocation), new[] { typeof(TIShipPartTemplate), typeof(int) })]
    public static class CapitalWeaponAssignmentPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TISpaceShipTemplate __instance, TIShipPartTemplate partTemplate, int slot, ref bool __result)
        {
            if (!CapitalMountPolicy.Applies(__instance.hullName) || partTemplate == null || partTemplate.ref_weapon == null || !partTemplate.ref_weapon.hullWeapon)
                return true;
            // Legacy entries must remain readable until the transaction has converted them.
            if (CapitalMountRuntime.State(__instance).Schema == 0) return true;
            __result = partTemplate.ref_weapon.mount == Mount.FourHull && slot >= 0 &&
                slot < __instance.hullTemplate.shipModuleSlots.Count &&
                __instance.hullTemplate.shipModuleSlots[slot].moduleSlotType == ShipModuleSlotType.HullHardPoint;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipModuleDragDestination), nameof(ShipModuleDragDestination.LegalModuleForSlot))]
    public static class CapitalWeaponDropPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ShipModuleDragDestination __instance, TIShipPartTemplate moduleTemplate,
            ref Vector2Int coordinates, ref bool __result)
        {
            TISpaceShipTemplate design = __instance.FleetsScreenController == null ? null : __instance.FleetsScreenController.newShipTemplate;
            if (design == null || !CapitalMountPolicy.Applies(design.hullName) || moduleTemplate == null || moduleTemplate.ref_weapon == null || !moduleTemplate.ref_weapon.hullWeapon)
                return true;
            coordinates = __instance.SlotCoordinates;
            __result = moduleTemplate.ref_weapon.mount == Mount.FourHull &&
                __instance.shipModuleSlotType == ShipModuleSlotType.HullHardPoint &&
                design.GetPartInHullSlot(coordinates, true) == null;
            return false;
        }
    }

    [HarmonyPatch(typeof(ShipModuleDragDestination), nameof(ShipModuleDragDestination.SetImage))]
    public static class CapitalWeaponIconPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ShipModuleDragDestination __instance, ref Mount mount)
        {
            if (mount == Mount.FourHull && __instance.FleetsScreenController != null &&
                CapitalMountPolicy.Applies(__instance.FleetsScreenController.newShipTemplate?.hullName))
                mount = Mount.OneHull; // Argument only; the actual weapon remains FourHull.
        }
    }

    [HarmonyPatch]
    public static class CapitalWeaponModelAnchorPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return new[] { typeof(TitanController), typeof(AlienTitanController), typeof(AlienMothershipController) }
                .Select(type => AccessTools.Method(type, "SlotToWeaponMountIndex"));
        }

        [HarmonyPrefix]
        public static void Prefix(ref Mount mount)
        {
            if (mount == Mount.FourHull) mount = Mount.OneHull;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), "SetShipDesignHullWeapons")]
    public static class CapitalAiHullWeaponsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIFactionState __instance, bool playerAutoDesign, ref TISpaceShipTemplate design,
            bool allowExotics, IEnumerable<TIShipWeaponTemplate> choices)
        {
            if (design == null || !CapitalMountPolicy.Applies(design.hullName)) return true;
            List<TIShipWeaponTemplate> weapons = (choices ?? __instance.allowedHullWeapons)
                .Where(w => w.mount == Mount.FourHull && w.FactionCanBuild(__instance) &&
                    (allowExotics || w.buildCost().GetSingleCostValue(FactionResource.Exotics) == 0f)).ToList();
            List<TIShipWeaponTemplate> current = weapons.Where(w => !__instance.obsoletedShipParts.Contains(w.dataName)).ToList();
            if (current.Count > 0) weapons = current;
            design.hullWeaponTemplateEntries.Clear();
            CapitalMountRuntime.State(design).Schema = CapitalMountPolicy.Schema;
            if (weapons.Count == 0) return false;
            ShipRole role = design.role;
            TIShipWeaponTemplate attack = weapons.Where(w => w.attackMode)
                .OrderByDescending(w => w.GetCuratedDesignScore(role, weapons, !playerAutoDesign))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).FirstOrDefault();
            TIShipWeaponTemplate defense = weapons.Where(w => w.defenseMode)
                .OrderByDescending(w => w.GetCuratedDesignScore(ShipRole.LM_Protector, weapons, !playerAutoDesign))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).FirstOrDefault();
            List<TIShipHullTemplate.ShipModuleSlot> slots = design.hullTemplate.GetAllSlotsOfType(ShipModuleSlotType.HullHardPoint);
            for (int i = 0; i < slots.Count; ++i)
            {
                TIShipWeaponTemplate weapon = role == ShipRole.LM_Protector ? defense ?? attack :
                    (i == slots.Count - 1 && defense != null ? defense : attack ?? defense);
                if (weapon == null) break;
                design.hullWeaponTemplateEntries.Add(new ModuleDataTemplateEntry(weapon, design.hullTemplate.slotIndex(slots[i])));
            }
            CapitalMountRuntime.Invalidate(design);
            return false;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.InitAtRunTime))]
    public static class CapitalNewDesignSchemaPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance)
        {
            CapitalMountRuntime.State(__instance).Schema = CapitalMountPolicy.Schema;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.Clone))]
    public static class CapitalCloneSchemaPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TISpaceShipTemplate __instance)
        {
            if (!CapitalMountRuntime.Planning) CapitalMountRuntime.EnsureDesign(__instance);
        }
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance, TISpaceShipTemplate __result)
        {
            CapitalMountRuntime.State(__result).Schema = CapitalMountRuntime.State(__instance).Schema;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), "get_ValidTemplate")]
    public static class CapitalDesignValidationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance, ref bool __result)
        {
            if (CapitalMountPolicy.Applies(__instance.hullName) && CapitalMountRuntime.State(__instance).Schema > 0)
                __result &= CapitalMountRuntime.ValidLayout(__instance);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.FinishDesigningShip))]
    public static class CapitalCompleteDesignPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TISpaceShipTemplate __instance)
        {
            CapitalMountRuntime.EnsureDesign(__instance);
            if (CapitalMountPolicy.Applies(__instance.hullName) && !CapitalMountRuntime.ValidLayout(__instance))
                throw new InvalidOperationException("Capital hull weapons require one FourHull weapon per hull cell.");
        }
    }

    [HarmonyPatch(typeof(SaveShipDesignAction), nameof(SaveShipDesignAction.Execute))]
    public static class CapitalSavedDesignGuardPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(SaveShipDesignAction __instance)
        {
            TISpaceShipTemplate design = __instance.shipDesign;
            CapitalMountRuntime.EnsureDesign(design);
            return !CapitalMountPolicy.Applies(design.hullName) || CapitalMountRuntime.ValidLayout(design);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.IsAValidRefitFor))]
    public static class CapitalRefitLayoutGuardPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance, ref bool __result, ref string reason)
        {
            if (CapitalMountPolicy.Applies(__instance.hullName) && !CapitalMountRuntime.ValidLayout(__instance))
            {
                __result = false;
                reason = "Capital hull mounts require one heavy weapon per cell.";
            }
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), nameof(FleetsScreenController.LoadShipTemplateIntoUI))]
    public static class CapitalImportedDesignPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void Prefix(TISpaceShipTemplate ship) { CapitalMountRuntime.EnsureDesign(ship); }
    }

}
