using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.UI;
using PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen;
using System.Collections.Generic;
using System.Reflection;
using TIEconomyMod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(
        typeof(TISpaceShipTemplate),
        nameof(TISpaceShipTemplate.GetPartInHullSlotIndex),
        new[] { typeof(int), typeof(bool) })]
    public static class UtilitySecondarySlotOccupancyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate __instance,
            int slotIndex,
            ref TIShipPartTemplate __result)
        {
            if (__result != null || !UtilityFootprintRegistry.Enabled)
            {
                return;
            }

            TIShipPartTemplate part;
            int anchorSlot;
            List<int> footprintSlots;
            if (UtilityFootprintRuntime.TryFindFootprintOccupant(
                __instance,
                slotIndex,
                out part,
                out anchorSlot,
                out footprintSlots))
            {
                __result = part;
            }
        }
    }

    [HarmonyPatch(
        typeof(ShipModuleDragDestination),
        nameof(ShipModuleDragDestination.LegalModuleForSlot))]
    public static class MultiSlotUtilityDropLegalityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ShipModuleDragDestination __instance,
            TIShipPartTemplate moduleTemplate,
            bool allowAlts,
            ref Vector2Int coordinates,
            ref bool __result)
        {
            UtilityFootprintKind footprint =
                UtilityFootprintRegistry.GetFootprint(moduleTemplate);
            if (moduleTemplate == null ||
                footprint == UtilityFootprintKind.Single)
            {
                return true;
            }

            FleetsScreenController controller =
                __instance.FleetsScreenController;
            if (controller == null || controller.newShipTemplate == null)
            {
                coordinates = __instance.SlotCoordinates;
                __result = false;
                return false;
            }

            __result = UtilityFootprintRuntime.TryResolvePlacement(
                controller.newShipTemplate,
                moduleTemplate,
                __instance.SlotCoordinates,
                allowAlts,
                out coordinates);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate),
        nameof(TISpaceShipTemplate.ValidPartForDesign))]
    public static class MultiSlotUtilityDesignAvailabilityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate __instance,
            TIShipPartTemplate part,
            ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            if (part == null ||
                UtilityFootprintRegistry.GetFootprint(part) ==
                    UtilityFootprintKind.Single)
            {
                return;
            }

            __result = UtilityFootprintRuntime.HasCompatiblePlacement(
                __instance, part);
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate),
        nameof(TISpaceShipTemplate.ValidPartForDesign))]
    public static class CyclotronProspectivePlacementPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate __instance,
            TIShipPartTemplate part,
            ref bool __result)
        {
            TIUtilityModuleTemplate utility =
                part == null ? null : part.ref_utilityModule;
            if (__result || utility == null ||
                utility.dataName != "Cyclotron")
            {
                return;
            }

            int ruleIndex = utility.specialModuleRules.IndexOf(
                SpecialModuleRule.ParticleBeamPowerBonus);
            if (ruleIndex < 0)
            {
                return;
            }

            utility.specialModuleRules.RemoveAt(ruleIndex);
            try
            {
                __result = __instance.ValidPartForDesign(part);
            }
            finally
            {
                utility.specialModuleRules.Insert(
                    ruleIndex,
                    SpecialModuleRule.ParticleBeamPowerBonus);
            }
        }
    }

    [HarmonyPatch(typeof(ShipModuleListItem), "UpdateItem")]
    public static class MultiSlotUtilityCatalogIconPatch
    {
        private static readonly FieldInfo ModuleTemplateField =
            AccessTools.Field(typeof(ShipModuleListItem), "moduleTemplate");

        private static readonly FieldInfo ModuleIconField =
            AccessTools.Field(typeof(ShipModuleListItem), "moduleIcon");

        [HarmonyPostfix]
        public static void Postfix(ShipModuleListItem __instance)
        {
            TIShipPartTemplate part =
                (TIShipPartTemplate)ModuleTemplateField.GetValue(__instance);
            Image image = (Image)ModuleIconField.GetValue(__instance);
            UtilityFootprintIconVisuals.ApplyCatalogPreview(
                image,
                UtilityFootprintRegistry.GetFootprint(part));
        }
    }

    [HarmonyPatch(typeof(ShipModuleListItem), nameof(ShipModuleListItem.SetAlpha))]
    public static class MultiSlotUtilityCatalogAlphaPatch
    {
        private static readonly FieldInfo ModuleTemplateField =
            AccessTools.Field(typeof(ShipModuleListItem), "moduleTemplate");

        private static readonly FieldInfo ModuleIconField =
            AccessTools.Field(typeof(ShipModuleListItem), "moduleIcon");

        [HarmonyPostfix]
        public static void Postfix(
            ShipModuleListItem __instance,
            bool fullyVisible)
        {
            TIShipPartTemplate part =
                (TIShipPartTemplate)ModuleTemplateField.GetValue(__instance);
            Image image = (Image)ModuleIconField.GetValue(__instance);
            UtilityFootprintIconVisuals.ApplyCatalogPreview(
                image,
                UtilityFootprintRegistry.GetFootprint(part));
            UtilityFootprintIconVisuals.SetDividerAlpha(
                image, fullyVisible);
        }
    }

    [HarmonyPatch(
        typeof(FleetsScreenController),
        nameof(FleetsScreenController.UpdateModuleDataPanel))]
    public static class MultiSlotUtilityDetailIconPatch
    {
        private static readonly FieldInfo SelectedIconField =
            AccessTools.Field(
                typeof(FleetsScreenController),
                "selectedModuleDataIcon");

        private static readonly FieldInfo InstalledIconField =
            AccessTools.Field(
                typeof(FleetsScreenController),
                "installedModuleDataIcon");

        [HarmonyPostfix]
        public static void Postfix(
            FleetsScreenController __instance,
            bool isSelected,
            TIShipPartTemplate partTemplate)
        {
            FieldInfo iconField = isSelected
                ? SelectedIconField
                : InstalledIconField;
            Image image = (Image)iconField.GetValue(__instance);
            UtilityFootprintIconVisuals.ApplyPreview(
                image,
                UtilityFootprintRegistry.GetFootprint(partTemplate));
        }
    }

    [HarmonyPatch(
        typeof(FleetsScreenController),
        nameof(FleetsScreenController.SetModuleInSlot))]
    public static class MultiSlotUtilityDesignerPlacementPatch
    {
        private static readonly FieldInfo SlotDictionaryField =
            AccessTools.Field(
                typeof(FleetsScreenController),
                "shipModuleSlotDictionary");

        private static readonly FieldInfo SlotImageField =
            AccessTools.Field(
                typeof(ShipModuleDragDestination),
                "slotImage");

        private static readonly FieldInfo DefaultPositionField =
            AccessTools.Field(
                typeof(ShipModuleDragDestination),
                "defaultPosition");

        private static readonly FieldInfo IconSizeField =
            AccessTools.Field(
                typeof(ShipModuleDragDestination),
                "iconSize");

        [HarmonyPostfix]
        public static void Postfix(
            FleetsScreenController __instance,
            TIShipPartTemplate module,
            ShipModuleDragDestination dropDestination)
        {
            UtilityFootprintKind footprint =
                UtilityFootprintRegistry.GetFootprint(module);
            if (module == null ||
                footprint == UtilityFootprintKind.Single ||
                __instance.newShipTemplate == null ||
                UtilityFootprintRuntime.IsLegacyLayout(
                    __instance.newShipTemplate))
            {
                return;
            }

            TIShipHullTemplate hull =
                __instance.newShipTemplate.hullTemplate;
            TIShipHullTemplate.ShipModuleSlot anchor =
                hull.GetSlotByCoordinates(dropDestination.SlotCoordinates);
            int anchorSlotIndex = hull.slotIndex(anchor);
            List<int> footprintSlots =
                UtilityFootprintRuntime.GetFootprintSlotIndices(
                    hull, anchorSlotIndex, footprint);
            if (footprintSlots.Count <= 1)
            {
                return;
            }

            Dictionary<Vector2Int, ShipModuleDragDestination> destinations =
                GetDestinations(__instance);
            SetMultiSlotImage(
                dropDestination,
                module.iconResource,
                UtilityFootprintRuntime.GetDisplayMount(footprint));
            for (int index = 1; index < footprintSlots.Count; index++)
            {
                Vector2Int coordinates =
                    hull.shipModuleSlots[footprintSlots[index]].slotPosition;
                ShipModuleDragDestination destination;
                if (destinations.TryGetValue(coordinates, out destination))
                {
                    destination.BlockDestination();
                }
            }
        }

        public static Dictionary<Vector2Int, ShipModuleDragDestination>
            GetDestinations(FleetsScreenController controller)
        {
            return (Dictionary<Vector2Int, ShipModuleDragDestination>)
                SlotDictionaryField.GetValue(controller);
        }

        public static void RestoreUtilityDestinationVisual(
            ShipModuleDragDestination destination)
        {
            Image slotImage = (Image)SlotImageField.GetValue(destination);
            int iconSize = (int)IconSizeField.GetValue(destination);
            RectTransform rectTransform = slotImage.rectTransform;
            rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            rectTransform.localPosition =
                (Vector3)DefaultPositionField.GetValue(destination);
            slotImage.preserveAspect = true;
            UtilityFootprintIconVisuals.HideDividers(slotImage);
        }

        private static void SetMultiSlotImage(
            ShipModuleDragDestination destination,
            string iconResource,
            Mount mount)
        {
            destination.SetImage(iconResource, mount);
            Image slotImage = (Image)SlotImageField.GetValue(destination);
            slotImage.preserveAspect = false;
            UtilityFootprintIconVisuals.ApplyDividers(
                slotImage,
                mount == Mount.FourHull);
        }
    }

    [HarmonyPatch(
        typeof(FleetsScreenController),
        nameof(FleetsScreenController.RemoveModuleFromSlot))]
    public static class MultiSlotUtilityDesignerRemovalPatch
    {
        public sealed class RemovalState
        {
            public bool ClearSecondaryCells;
            public Vector2Int AnchorCoordinates;
            public List<Vector2Int> SecondaryCoordinates;
        }

        [HarmonyPrefix]
        public static bool Prefix(
            FleetsScreenController __instance,
            Vector2Int coordinates,
            bool updateRole,
            bool suppressSCVUpdate,
            ref RemovalState __state)
        {
            __state = new RemovalState();
            if (!UtilityFootprintRegistry.Enabled ||
                __instance.newShipTemplate == null)
            {
                return true;
            }

            TIShipHullTemplate hull =
                __instance.newShipTemplate.hullTemplate;
            TIShipHullTemplate.ShipModuleSlot queriedSlot =
                hull.GetSlotByCoordinates(coordinates);
            int queriedSlotIndex = hull.slotIndex(queriedSlot);
            TIShipPartTemplate part;
            int anchorSlotIndex;
            List<int> footprintSlots;
            if (!UtilityFootprintRuntime.TryFindFootprintOccupant(
                    __instance.newShipTemplate,
                    queriedSlotIndex,
                    out part,
                    out anchorSlotIndex,
                    out footprintSlots) ||
                footprintSlots == null || footprintSlots.Count <= 1)
            {
                return true;
            }

            Vector2Int anchorCoordinates =
                hull.shipModuleSlots[anchorSlotIndex].slotPosition;
            if (anchorSlotIndex != queriedSlotIndex)
            {
                __instance.RemoveModuleFromSlot(
                    anchorCoordinates,
                    updateRole,
                    suppressSCVUpdate);
                return false;
            }

            __state.ClearSecondaryCells = true;
            __state.AnchorCoordinates = anchorCoordinates;
            __state.SecondaryCoordinates = new List<Vector2Int>();
            for (int index = 1; index < footprintSlots.Count; index++)
            {
                __state.SecondaryCoordinates.Add(
                    hull.shipModuleSlots[footprintSlots[index]].slotPosition);
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(
            FleetsScreenController __instance,
            RemovalState __state)
        {
            if (__state == null || !__state.ClearSecondaryCells)
            {
                return;
            }

            Dictionary<Vector2Int, ShipModuleDragDestination> destinations =
                MultiSlotUtilityDesignerPlacementPatch.GetDestinations(
                    __instance);
            ShipModuleDragDestination anchorDestination;
            if (destinations.TryGetValue(
                __state.AnchorCoordinates, out anchorDestination))
            {
                MultiSlotUtilityDesignerPlacementPatch
                    .RestoreUtilityDestinationVisual(anchorDestination);
            }

            for (int index = 0;
                index < __state.SecondaryCoordinates.Count;
                index++)
            {
                ShipModuleDragDestination destination;
                if (destinations.TryGetValue(
                    __state.SecondaryCoordinates[index], out destination))
                {
                    destination.SetEmpty();
                    MultiSlotUtilityDesignerPlacementPatch
                        .RestoreUtilityDestinationVisual(destination);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TIFactionState), "GetBestUtilityModules")]
    public static class MultiSlotUtilityAiPackingPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate design,
            ref List<ModuleDataTemplateEntry> __result)
        {
            if (!UtilityFootprintRegistry.Enabled || __result == null)
            {
                return;
            }

            __result = UtilityFootprintRuntime.PackUtilityModules(
                design, __result);
        }
    }

}
