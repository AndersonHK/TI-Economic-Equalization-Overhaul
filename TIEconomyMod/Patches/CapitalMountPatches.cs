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
            if (!CapitalMountPolicy.Applies(__instance.dataName) ||
                (coreSlot.moduleSlotType != ShipModuleSlotType.HullHardPoint && coreSlot.moduleSlotType != ShipModuleSlotType.NoseHardPoint))
                return true;
            __result = new List<TIShipHullTemplate.ShipModuleSlot>();
            if (mount == (coreSlot.moduleSlotType == ShipModuleSlotType.HullHardPoint ? Mount.FourHull : Mount.FourNose)) __result.Add(coreSlot);
            return false;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.ValidPartForDesign))]
    public static class CapitalWeaponAvailabilityPatch
    {
        internal static bool HiddenFromCatalog(TISpaceShipTemplate design, TIShipPartTemplate part)
        {
            TIShipWeaponTemplate weapon = part?.ref_weapon;
            return design != null && weapon != null && !CapitalMountPolicy.WeaponAllowed(
                design.hullName, weapon.dataName, weapon.mount.ToString(), weapon.hullWeapon);
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
            if (___loadingExistingTemplate || __instance.newShipTemplate == null) return;
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
            if (partTemplate == null || partTemplate.ref_weapon == null)
                return true;
            TIShipWeaponTemplate weapon = partTemplate.ref_weapon;
            if (CapitalMountRuntime.State(__instance).Schema >= CapitalMountPolicy.Schema &&
                !CapitalMountPolicy.WeaponAllowed(__instance.hullName, weapon.dataName, weapon.mount.ToString(), weapon.hullWeapon))
            { __result = false; return false; }
            if (!CapitalMountPolicy.Applies(__instance.hullName)) return true;
            // Legacy entries must remain readable until the transaction has converted them.
            bool hull = partTemplate.ref_weapon.hullWeapon;
            if (CapitalMountRuntime.State(__instance).Schema < CapitalMountPolicy.Schema)
            {
                __result = slot >= 0 && slot < __instance.hullTemplate.shipModuleSlots.Count &&
                    partTemplate.allowedSlots.Contains(__instance.hullTemplate.shipModuleSlots[slot].moduleSlotType);
                return false;
            }
            __result = partTemplate.ref_weapon.mount == (hull ? Mount.FourHull : Mount.FourNose) && slot >= 0 &&
                slot < __instance.hullTemplate.shipModuleSlots.Count &&
                __instance.hullTemplate.shipModuleSlots[slot].moduleSlotType == (hull ? ShipModuleSlotType.HullHardPoint : ShipModuleSlotType.NoseHardPoint);
            return false;
        }
    }

    [HarmonyPatch(typeof(TIShipHullTemplate), nameof(TIShipHullTemplate.AssignCoreSlotOnMultiMountPlacement))]
    public static class CapitalWeaponCoreSlotPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TISpaceShipTemplate ship, TIShipWeaponTemplate weapon, int droppedSlot,
            ref TIShipHullTemplate.ShipModuleSlot __result)
        {
            if (ship == null) return true;
            if (!CapitalMountPolicy.WeaponAllowed(ship.hullName, weapon.dataName, weapon.mount.ToString(), weapon.hullWeapon))
                throw new InvalidOperationException("Weapon grade is not allowed on this hull.");
            if (!CapitalMountPolicy.Applies(ship.hullName)) return true;
            if (droppedSlot < 0 || droppedSlot >= ship.hullTemplate.shipModuleSlots.Count ||
                !ship.ValidAssignedSlotForLocation(weapon, droppedSlot))
                throw new InvalidOperationException("Invalid capital weapon destination.");
            __result = ship.hullTemplate.shipModuleSlots[droppedSlot];
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
            if (design == null || moduleTemplate == null || moduleTemplate.ref_weapon == null)
                return true;
            TIShipWeaponTemplate weapon = moduleTemplate.ref_weapon;
            if (!CapitalMountPolicy.WeaponAllowed(design.hullName, weapon.dataName, weapon.mount.ToString(), weapon.hullWeapon))
            { __result = false; return false; }
            if (!CapitalMountPolicy.Applies(design.hullName)) return true;
            coordinates = __instance.SlotCoordinates;
            bool hull = moduleTemplate.ref_weapon.hullWeapon;
            __result = moduleTemplate.ref_weapon.mount == (hull ? Mount.FourHull : Mount.FourNose) &&
                __instance.shipModuleSlotType == (hull ? ShipModuleSlotType.HullHardPoint : ShipModuleSlotType.NoseHardPoint) &&
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
            if ((mount == Mount.FourHull || mount == Mount.FourNose) && __instance.FleetsScreenController != null &&
                CapitalMountPolicy.Applies(__instance.FleetsScreenController.newShipTemplate?.hullName))
                mount = mount == Mount.FourHull ? Mount.OneHull : Mount.OneNose; // Visual argument only.
        }
    }

    [HarmonyPatch]
    public static class CapitalWeaponModelAnchorPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return new[] { typeof(LancerController), typeof(DreadnoughtController), typeof(AlienDreadnoughtController), typeof(TitanController), typeof(AlienTitanController), typeof(AlienAssaultCarrierController), typeof(AlienMothershipController) }
                .Select(type => AccessTools.Method(type, "SlotToWeaponMountIndex"));
        }

        [HarmonyPrefix]
        public static bool Prefix(ShipModelController __instance, int slot, ref Mount mount, ref int __result)
        {
            if (__instance is AlienMothershipController && (mount == Mount.FourNose || mount == Mount.OneNose) && slot >= 38 && slot <= 41)
            {
                __result = slot - 33; // Appended nose cells use new controllers 5–8.
                return false;
            }
            if (__instance is AlienMothershipController && (mount == Mount.FourHull || mount == Mount.OneHull) && (slot == 42 || slot == 43))
            {
                __result = slot == 42 ? 8 : 11; // Previously dormant mirrored rim pair.
                return false;
            }
            if (mount == Mount.FourHull) mount = Mount.OneHull;
            if (mount == Mount.FourNose) mount = Mount.OneNose;
            return true;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), "SetShipDesignHullWeapons")]
    public static class CapitalAiHullWeaponsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIFactionState __instance, bool playerAutoDesign, ref TISpaceShipTemplate design,
            bool allowExotics, IEnumerable<TIShipWeaponTemplate> choices)
        {
            return Fit(__instance, playerAutoDesign, design, allowExotics, choices, false);
        }

        internal static bool Fit(TIFactionState faction, bool playerAutoDesign, TISpaceShipTemplate design,
            bool allowExotics, IEnumerable<TIShipWeaponTemplate> choices, bool nose)
        {
            if (design == null || !CapitalMountPolicy.Applies(design.hullName)) return true;
            List<TIShipWeaponTemplate> weapons = (choices ?? (nose ? faction.allowedNoseWeapons : faction.allowedHullWeapons))
                .Where(w => w.mount == (nose ? Mount.FourNose : Mount.FourHull) &&
                    CapitalMountPolicy.WeaponAllowed(design.hullName, w.dataName, w.mount.ToString(), !nose) && w.FactionCanBuild(faction) &&
                    (allowExotics || w.buildCost().GetSingleCostValue(FactionResource.Exotics) == 0f)).ToList();
            List<TIShipWeaponTemplate> current = weapons.Where(w => !faction.obsoletedShipParts.Contains(w.dataName)).ToList();
            if (current.Count > 0) weapons = current;
            var entries = nose ? design.noseWeaponTemplateEntries : design.hullWeaponTemplateEntries;
            entries.Clear();
            var slotType = nose ? ShipModuleSlotType.NoseHardPoint : ShipModuleSlotType.HullHardPoint;
            design.fireModeTemplateEntries.RemoveAll(m => m.slot >= 0 && m.slot < design.hullTemplate.shipModuleSlots.Count &&
                design.hullTemplate.shipModuleSlots[m.slot].moduleSlotType == slotType);
            CapitalMountRuntime.State(design).Schema = CapitalMountPolicy.Schema;
            if (weapons.Count == 0) { CapitalMountRuntime.Invalidate(design); return false; }
            ShipRole role = design.role;
            TIShipWeaponTemplate attack = weapons.Where(w => w.attackMode)
                .OrderByDescending(w => w.GetCuratedDesignScore(role, weapons, !playerAutoDesign))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).FirstOrDefault();
            TIShipWeaponTemplate defense = weapons.Where(w => w.defenseMode)
                .OrderByDescending(w => w.GetCuratedDesignScore(ShipRole.LM_Protector, weapons, !playerAutoDesign))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).FirstOrDefault();
            List<TIShipHullTemplate.ShipModuleSlot> slots = design.hullTemplate.GetAllSlotsOfType(slotType);
            for (int i = 0; i < slots.Count; ++i)
            {
                TIShipWeaponTemplate weapon = role == ShipRole.LM_Protector ? defense ?? attack :
                    (i == slots.Count - 1 && defense != null ? defense : attack ?? defense);
                if (weapon == null) break;
                entries.Add(new ModuleDataTemplateEntry(weapon, design.hullTemplate.slotIndex(slots[i])));
            }
            CapitalMountRuntime.Invalidate(design);
            return false;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), "SetShipDesignNoseWeapons")]
    public static class CapitalAiNoseWeaponsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIFactionState __instance, bool playerAutodesign, ref TISpaceShipTemplate design,
            bool allowExotics, ref IEnumerable<TIShipWeaponTemplate> choices)
        {
            if (design != null && !CapitalMountPolicy.Applies(design.hullName))
            {
                string hullName = design.hullName;
                choices = (choices ?? __instance.allowedNoseWeapons).Where(w =>
                    CapitalMountPolicy.WeaponAllowed(hullName, w.dataName, w.mount.ToString(), false)).ToList();
                return true;
            }
            return CapitalAiHullWeaponsPatch.Fit(__instance, playerAutodesign, design, allowExotics, choices, true);
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
            if (CapitalMountRuntime.RequiresPolicy(__instance) && CapitalMountRuntime.State(__instance).Schema >= CapitalMountPolicy.Schema)
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
            if (CapitalMountRuntime.RequiresPolicy(__instance) && !CapitalMountRuntime.ValidLayout(__instance))
                throw new InvalidOperationException("Hull requires its designated weapon grade, one weapon per matching hardpoint.");
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
            return !CapitalMountRuntime.RequiresPolicy(design) || CapitalMountRuntime.ValidLayout(design);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.IsAValidRefitFor))]
    public static class CapitalRefitLayoutGuardPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance, ref bool __result, ref string reason)
        {
            if (CapitalMountRuntime.RequiresPolicy(__instance) && !CapitalMountRuntime.ValidLayout(__instance))
            {
                __result = false;
                reason = "This hull requires its designated weapon sizes, one weapon per hardpoint.";
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
