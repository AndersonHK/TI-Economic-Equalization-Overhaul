using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TIEconomyMod.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TIEconomyMod.Patches
{
    // The native parser accepts integer coordinates but the UI only allocates
    // prefab destinations. Prepare the missing destinations from JSON once.
    [HarmonyPatch(typeof(FleetsScreenController), "InitializeShipDesigner")]
    public static class ShipDesignerDataLayoutPatch
    {
        public static void ValidateTemplates()
        {
            if (Main.shipDesignerLayout == null) return;
            foreach (var hull in TemplateManager.GetAllTemplates<TIShipHullTemplate>()) Main.shipDesignerLayout.ValidateHull(hull);
        }
        private sealed class Prepared { internal bool Done; }
        private static readonly ConditionalWeakTable<FleetsScreenController, Prepared> States =
            new ConditionalWeakTable<FleetsScreenController, Prepared>();

        [HarmonyPostfix]
        public static void Postfix(FleetsScreenController __instance,
            ref ShipModuleDragDestination[] ___moduleDragDestinations,
            Dictionary<Vector2Int, ShipModuleDragDestination> ___shipModuleSlotDictionary)
        {
            Prepared state = States.GetOrCreateValue(__instance);
            if (state.Done) return;
            ValidateTemplates();
            ShipDesignerLayout data = Main.shipDesignerLayout;
            if (data == null) throw new InvalidOperationException("Ship designer layout data was not loaded.");
            var slots = ___shipModuleSlotDictionary;
            if (slots.Count == 0 || slots.Count % data.Columns != 0)
                throw new InvalidOperationException("Native ship designer grid is not rectangular.");
            RectTransform grid = slots.First().Value.transform.parent.parent as RectTransform;
            GridLayoutGroup nativeLayout = grid.GetComponent<GridLayoutGroup>();
            if (nativeLayout != null) nativeLayout.enabled = false; // JSON owns coordinates, not centered prefab allocation.
            int nativeRows = slots.Count / data.Columns;
            var declared = TemplateManager.GetAllTemplates<TIShipHullTemplate>()
                .SelectMany(h => h.shipModuleSlots).ToList();
            int maximumY = declared.Max(s => s.y);
            int rows = data.ResolveRows(nativeRows, maximumY);
            RectTransform center = __instance.ShipDesignerCanvas.transform.Find(data.CenterPanelPath) as RectTransform;
            if (center == null || __instance.primaryPanelTransform == null)
                throw new InvalidOperationException("Ship designer panel paths changed.");
            // Keep header/input controls at their authored height while their
            // parent acquires the additional data-declared grid space.
            foreach (string path in data.FixedHeightPanels)
            {
                RectTransform rect = center.Find(path) as RectTransform;
                if (rect == null) throw new InvalidOperationException("Missing fixed-height designer panel: " + path);
                float anchorReference = center.rect.height * (rect.anchorMin.y +
                    (rect.anchorMax.y - rect.anchorMin.y) * rect.pivot.y);
                float top = center.rect.height - anchorReference - rect.anchoredPosition.y -
                    rect.rect.height * (1f - rect.pivot.y);
                float height = rect.rect.height;
                rect.anchorMin = new Vector2(rect.anchorMin.x, 1f);
                rect.anchorMax = new Vector2(rect.anchorMax.x, 1f);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -top - height * (1f - rect.pivot.y));
            }
            float growth = data.HeightGrowth(nativeRows, rows);
            __instance.primaryPanelTransform.sizeDelta += new Vector2(0f, growth);
            center.sizeDelta += new Vector2(0f, growth);
            var all = new List<ShipModuleDragDestination>(___moduleDragDestinations);
            var prototypes = Enumerable.Range(0, data.Columns)
                .Select(x => slots[new Vector2Int(x, nativeRows - 1)]).ToArray();
            for (int y = nativeRows; y < rows; ++y)
                for (int x = 0; x < data.Columns; ++x)
                {
                    // Last native row supplies complete armor spinner wiring.
                    ShipModuleDragDestination destination = CloneDestination(prototypes[x], __instance, x, y);
                    slots.Add(new Vector2Int(x, y), destination);
                    all.Add(destination);
                }
            // Native prefab controls differ by coordinate. A JSON relocation
            // must bring complete spinner wiring to its new destination too.
            foreach (var point in declared.Where(s => s.moduleSlotType == ShipModuleSlotType.Drive ||
                s.moduleSlotType == ShipModuleSlotType.Propellant || s.armorSlot).Select(s => s.slotPosition).Distinct())
            {
                ShipModuleDragDestination old = slots[point];
                if (old.hasSpinner && old.spinnerValueInput != null && old.spinnerValueText != null) continue;
                ShipModuleDragDestination complete = CloneDestination(prototypes[point.x], __instance, point.x, point.y);
                complete.transform.parent.SetSiblingIndex(old.transform.parent.GetSiblingIndex());
                old.transform.parent.gameObject.SetActive(false);
                all[all.IndexOf(old)] = complete;
                slots[point] = complete;
            }
            foreach (var pair in slots)
            {
                RectTransform rect = pair.Value.transform.parent as RectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(data.X(pair.Key.x), data.Y(pair.Key.y));
            }
            ___moduleDragDestinations = all.ToArray();
            state.Done = true;
            var fitter = __instance.GetComponent<ShipDesignerViewportFitter>() ?? __instance.gameObject.AddComponent<ShipDesignerViewportFitter>();
            fitter.Initialize(__instance, center, grid, slots, data);
        }

        private static ShipModuleDragDestination CloneDestination(ShipModuleDragDestination prototype,
            FleetsScreenController controller, int x, int y)
        {
            GameObject clone = UnityEngine.Object.Instantiate(prototype.transform.parent.gameObject,
                prototype.transform.parent.parent, false);
            clone.name = "ShipModuleSlot (data " + x + "," + y + ")";
            ShipModuleDragDestination destination = clone.GetComponentInChildren<ShipModuleDragDestination>(true);
            destination.SetControllerBase(controller);
            destination.defaultPosition = Vector3.zero;
            destination.DisableDestination();
            return destination;
        }
    }

    [HarmonyPatch(typeof(ShipModuleDragDestination), nameof(ShipModuleDragDestination.EnableDestination))]
    public static class ShipDesignerSpinnerLabelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipModuleDragDestination __instance, ShipModuleSlotType slotType)
        {
            if (__instance.spinnerValueText != null)
            {
                __instance.spinnerValueText.gameObject.SetActive(slotType != ShipModuleSlotType.Drive);
                if (slotType == ShipModuleSlotType.Propellant)
                {
                    // Native propellant labels are wider than one cell. Keep
                    // them inside their own column when JSON fills neighbors.
                    var style = Main.shipDesignerLayout.PropellantLabel;
                    RectTransform rect = __instance.spinnerValueText.rectTransform;
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                    rect.sizeDelta = new Vector2(style.Width, style.Height);
                    rect.anchoredPosition = new Vector2(0f, style.OffsetY);
                    __instance.spinnerValueText.textWrappingMode = TextWrappingModes.Normal;
                    __instance.spinnerValueText.enableAutoSizing = true;
                    __instance.spinnerValueText.fontSizeMax = style.MaximumFontSize;
                    __instance.spinnerValueText.fontSizeMin = Math.Min(10f, style.MaximumFontSize);
                }
            }
        }
    }
}
