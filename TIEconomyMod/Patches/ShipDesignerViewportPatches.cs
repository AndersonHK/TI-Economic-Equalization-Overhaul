using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TIEconomyMod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TIEconomyMod.Patches
{
    // Enabled only for bounded layout passes after relevant UI events.
    public sealed class ShipDesignerViewportFitter : MonoBehaviour
    {
        private FleetsScreenController controller;
        private RectTransform center, grid, panel, region;
        private Dictionary<Vector2Int, ShipModuleDragDestination> slots;
        private ShipDesignerLayout data;
        private DesignerRect catalog;
        private Vector3 baseScale;
        private float panelWidth, panelHeight, centerWidth, centerHeight, extraWidth, extraHeight;
        private float originX, originY;
        private int remaining;
        private bool fitting;
        private string lastReport;
        private readonly Vector3[] corners = new Vector3[4];

        public void Initialize(FleetsScreenController owner, RectTransform central, RectTransform cells,
            Dictionary<Vector2Int, ShipModuleDragDestination> destinations, ShipDesignerLayout layout)
        {
            controller = owner; center = central; grid = cells; panel = owner.primaryPanelTransform; slots = destinations; data = layout;
            region = center.Find(data.Viewport.RegionPath) as RectTransform;
            if (region == null) throw new InvalidOperationException("Designer visible-region path changed.");
            panelWidth = panel.rect.width; panelHeight = panel.rect.height;
            centerWidth = center.rect.width; centerHeight = center.rect.height; baseScale = panel.localScale;
            originX = data.OriginX; originY = data.OriginY;
            bool any = false;
            foreach (var hull in TemplateManager.GetAllTemplates<TIShipHullTemplate>())
            {
                DesignerRect bounds = data.ValidateHull(hull);
                if (bounds.Width <= 0 || bounds.Height <= 0) continue;
                catalog = any ? DesignerRect.Union(catalog, bounds) : bounds; any = true;
            }
            Request();
        }

        public void Request() { if (controller == null || fitting) return; remaining = 3; enabled = true; }

        private void LateUpdate()
        {
            if (controller == null) { enabled = false; return; }
            try
            {
                if (controller.Canvas != null && controller.Canvas.enabled && controller.ShipDesignerCanvas.enabled)
                {
                    fitting = true;
                    Fit();
                }
            }
            catch (Exception exception) { Report("Designer viewport fit could not complete: " + exception.Message, true); }
            finally { fitting = false; if (--remaining <= 0) enabled = false; }
        }

        private void Fit()
        {
            GridLayoutGroup competing = grid.GetComponent<GridLayoutGroup>();
            if (competing != null) competing.enabled = false;
            SetCellPositions(originX, originY);
            Canvas.ForceUpdateCanvases();
            DesignerRect content = catalog;
            DesignerRect actual;
            if (TryActualBounds(out actual)) content = DesignerRect.Union(content, actual.Translate(-originX, originY));
            DesignerPlacement fit = new DesignerPlacement();
            DesignerRect viewport = new DesignerRect();
            for (int pass = 0; pass < 3; ++pass)
            {
                ApplyWindowSize();
                Canvas.ForceUpdateCanvases();
                viewport = VisibleRect();
                fit = DesignerViewportMath.Place(content, viewport, data.OriginX, data.OriginY);
                if (fit.Fits) break;
                if (!DesignerViewportMath.Usable(viewport)) return; // Not a settled visible canvas yet.
                extraWidth += fit.MissingWidth + .1f;
                extraHeight += fit.MissingHeight + .1f;
            }
            if (!fit.Fits) { Report("Designer viewport has insufficient room after measured expansion.", true); return; }
            originX = fit.OriginX; originY = fit.OriginY;
            SetCellPositions(originX, originY);
            Canvas.ForceUpdateCanvases();
            viewport = VisibleRect();
            DesignerRect positioned = content.Translate(originX, -originY);
            bool allInside = viewport.Contains(positioned, .5f);
            if (TryActualBounds(out actual)) allInside &= viewport.Contains(actual, .5f);
            RectTransform canvas = controller.Canvas.rootCanvas.transform as RectTransform;
            DesignerRect outer = ReadRect(panel, canvas);
            bool onScreen = outer.Left >= data.Viewport.ScreenMargin - .5f && outer.Right <= canvas.rect.width - data.Viewport.ScreenMargin + .5f &&
                outer.Top >= -.5f && outer.Bottom <= canvas.rect.height - data.Viewport.ScreenMargin + .5f;
            string report = string.Format(CultureInfo.InvariantCulture,
                "Designer viewport: catalog={0:F2}x{1:F2}, visible={2:F2}x{3:F2}, origin=({4:F2},{5:F2}), panel={6:F2}x{7:F2}, scale={8:F4}, all-content-inside={9}, window-on-screen={10}.",
                content.Width, content.Height, viewport.Width, viewport.Height, originX, originY,
                panel.rect.width, panel.rect.height, panel.localScale.y / baseScale.y, allInside, onScreen);
            Report(report, !allInside || !onScreen);
        }

        private void ApplyWindowSize()
        {
            panel.localScale = baseScale;
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth + extraWidth);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight + extraHeight);
            center.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, centerWidth + extraWidth);
            center.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, centerHeight + extraHeight);
            Canvas.ForceUpdateCanvases();
            RectTransform canvas = controller.Canvas.rootCanvas.transform as RectTransform;
            DesignerRect outer = ReadRect(panel, canvas);
            float scale = DesignerViewportMath.WindowScale(outer.Width, outer.Height,
                canvas.rect.width - 2 * data.Viewport.ScreenMargin,
                canvas.rect.height - Math.Max(0, outer.Top) - data.Viewport.ScreenMargin);
            if (scale <= 0) return;
            panel.localScale = baseScale * scale;
        }

        private DesignerRect VisibleRect()
        {
            DesignerRect view = ReadRect(region, grid);
            foreach (string path in data.Viewport.HeaderPaths)
            {
                RectTransform header = center.Find(path) as RectTransform;
                if (header == null) throw new InvalidOperationException("Missing designer header keepout: " + path);
                view.Top = Math.Max(view.Top, ReadRect(header, grid).Bottom);
            }
            // Respect any native clipping component as well as the panel's
            // nominal rectangle. The current prefab has no grid mask.
            for (Transform t = grid; t != null && t.IsChildOf(center); t = t.parent)
            {
                RectMask2D rectMask = t.GetComponent<RectMask2D>();
                Mask mask = t.GetComponent<Mask>();
                if ((rectMask != null && rectMask.isActiveAndEnabled) || (mask != null && mask.isActiveAndEnabled))
                {
                    DesignerRect clip = ReadRect(t as RectTransform, grid);
                    view.Left = Math.Max(view.Left, clip.Left); view.Top = Math.Max(view.Top, clip.Top);
                    view.Right = Math.Min(view.Right, clip.Right); view.Bottom = Math.Min(view.Bottom, clip.Bottom);
                }
            }
            view.Left += data.Viewport.PaddingLeft; view.Right -= data.Viewport.PaddingRight;
            view.Top += data.Viewport.PaddingTop; view.Bottom -= data.Viewport.PaddingBottom;
            return view;
        }

        private DesignerRect ReadRect(RectTransform rect, RectTransform relative)
        {
            rect.GetWorldCorners(corners);
            DesignerRect result = new DesignerRect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (Vector3 corner in corners)
            {
                Vector3 p = relative.InverseTransformPoint(corner);
                float x = p.x - relative.rect.xMin, y = relative.rect.yMax - p.y;
                result.Left = Math.Min(result.Left, x); result.Right = Math.Max(result.Right, x);
                result.Top = Math.Min(result.Top, y); result.Bottom = Math.Max(result.Bottom, y);
            }
            return result;
        }

        private bool TryActualBounds(out DesignerRect result)
        {
            bool any = false; result = new DesignerRect();
            foreach (var destination in slots.Values)
            {
                if (!destination.gameObject.activeInHierarchy || destination.shipModuleSlotType == ShipModuleSlotType.None) continue;
                if (destination.shipModuleSlotType != ShipModuleSlotType.Propellant)
                    Include(destination.transform as RectTransform, ref result, ref any);
                if (destination.spinnerPanel != null && destination.spinnerPanel.activeInHierarchy)
                {
                    Include(destination.spinnerPanel.transform as RectTransform, ref result, ref any);
                    if (destination.spinnerValueText != null && destination.spinnerValueText.gameObject.activeInHierarchy)
                        Include(destination.spinnerValueText.rectTransform, ref result, ref any);
                }
            }
            return any;
        }
        private void Include(RectTransform rect, ref DesignerRect result, ref bool any)
        {
            if (rect == null) return;
            DesignerRect next = ReadRect(rect, grid); result = any ? DesignerRect.Union(result, next) : next; any = true;
        }
        private void SetCellPositions(float x, float y)
        {
            foreach (var pair in slots)
            {
                RectTransform rect = pair.Value.transform.parent as RectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = new Vector2(x + pair.Key.x * data.ColumnPitch, y - pair.Key.y * data.HalfRowPitch);
            }
        }
        private void Report(string message, bool warning)
        {
            if (lastReport == message) return; lastReport = message;
            if (warning) Main.Warn(message); else Main.Log(message);
        }
    }

    [HarmonyPatch]
    public static class ShipDesignerViewportFitPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(FleetsScreenController), "SetupDesignerLayout");
            yield return AccessTools.Method(typeof(FleetsScreenController), nameof(FleetsScreenController.UpdateUIScaling));
            yield return AccessTools.Method(typeof(CanvasControllerBase), nameof(CanvasControllerBase.Show));
        }
        [HarmonyPostfix]
        public static void Postfix(CanvasControllerBase __instance)
        {
            FleetsScreenController controller = __instance as FleetsScreenController;
            if (controller == null) return;
            ShipDesignerViewportFitter fitter = controller.GetComponent<ShipDesignerViewportFitter>();
            if (fitter != null) fitter.Request();
        }
    }
}
