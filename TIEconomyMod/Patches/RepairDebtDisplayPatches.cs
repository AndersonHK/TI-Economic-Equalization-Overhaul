using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TIEconomyMod.Patches
{
    internal sealed class RepairDebtOverlayState
    {
        public GameObject container;
        public RectTransform rectTransform;
        public Image background;
        public TMP_Text label;
        public readonly List<GameObject> hiddenValues = new List<GameObject>();
    }

    internal static class RepairDebtOverlayRuntime
    {
        private static readonly Dictionary<NationInfoController, RepairDebtOverlayState>
            States = new Dictionary<NationInfoController, RepairDebtOverlayState>();
        private static bool warned;

        private static bool IsDebtFundingPriority(PriorityType priority)
        {
            return priority == PriorityType.Military ||
                priority == PriorityType.Military_BuildArmy ||
                priority == PriorityType.Military_BuildNavy ||
                priority == PriorityType.Military_BuildNuclearWeapons;
        }

        public static void Prepare(NationInfoController controller)
        {
            RepairDebtOverlayState state;
            if (controller == null || !States.TryGetValue(controller, out state))
            {
                return;
            }

            RestoreHiddenValues(state);
            if (state.container != null)
            {
                state.container.SetActive(false);
            }
        }

        public static void Apply(NationInfoController controller)
        {
            ArmySettings settings = Main.settings.army;
            if (controller == null ||
                controller.nation == null ||
                controller.priorityList == null ||
                !Main.FeatureEnabled(settings.enabled) ||
                !settings.repairDebtMergedDisplayEnabled)
            {
                return;
            }

            float progress = controller.nation.GetAccumulatedInvestmentPoints(
                PriorityType.Military_BuildArmy);
            if (progress >= 0f)
            {
                return;
            }

            try
            {
                List<PriorityListItemController> rows = ActiveFundingRows(controller);
                if (rows.Count == 0)
                {
                    return;
                }

                Canvas.ForceUpdateCanvases();
                RectTransform commonParent = rows[0].transform.parent as RectTransform;
                if (commonParent == null)
                {
                    throw new InvalidOperationException(
                        "priority rows have no shared RectTransform parent");
                }

                float minX = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;
                float minY = float.PositiveInfinity;
                float maxY = float.NegativeInfinity;
                Vector3[] corners = new Vector3[4];
                foreach (PriorityListItemController row in rows)
                {
                    if (row.transform.parent != commonParent)
                    {
                        throw new InvalidOperationException(
                            "priority rows do not share one layout parent");
                    }

                    row.priorityAccumulation.rectTransform.GetWorldCorners(corners);
                    for (int index = 0; index < corners.Length; index++)
                    {
                        Vector3 local = commonParent.InverseTransformPoint(corners[index]);
                        minX = Math.Min(minX, local.x);
                        maxX = Math.Max(maxX, local.x);
                        minY = Math.Min(minY, local.y);
                        maxY = Math.Max(maxY, local.y);
                    }
                }

                if (maxX <= minX || maxY <= minY)
                {
                    throw new InvalidOperationException(
                        "priority investment-column bounds are empty");
                }

                RepairDebtOverlayState state = GetOrCreateState(
                    controller, rows[0], commonParent);
                state.rectTransform.anchorMin = commonParent.pivot;
                state.rectTransform.anchorMax = commonParent.pivot;
                state.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                state.rectTransform.localScale = Vector3.one;
                state.rectTransform.localRotation = Quaternion.identity;
                state.rectTransform.anchoredPosition3D = new Vector3(
                    (minX + maxX) * 0.5f,
                    (minY + maxY) * 0.5f,
                    0f);
                state.rectTransform.sizeDelta = new Vector2(maxX - minX, maxY - minY);
                CopyBackgroundStyle(state.background, FindRowBackground(rows[0]));
                state.label.SetText(
                    ArmyRepairDebtDisplayPatch.RepairDebtText(progress));
                state.container.transform.SetAsLastSibling();
                state.container.SetActive(true);

                foreach (PriorityListItemController row in rows)
                {
                    GameObject value = row.priorityAccumulation.gameObject;
                    if (value.activeSelf)
                    {
                        state.hiddenValues.Add(value);
                        value.SetActive(false);
                    }
                }
            }
            catch (Exception exception)
            {
                Prepare(controller);
                if (!warned)
                {
                    warned = true;
                    Main.Warn(
                        "Repair-debt merged display could not be created; retaining per-row values. " +
                        exception.Message);
                }
            }
        }

        public static void Dispose(NationInfoController controller)
        {
            RepairDebtOverlayState state;
            if (controller == null || !States.TryGetValue(controller, out state))
            {
                return;
            }

            RestoreHiddenValues(state);
            if (state.container != null)
            {
                UnityEngine.Object.Destroy(state.container);
            }
            States.Remove(controller);
        }

        private static List<PriorityListItemController> ActiveFundingRows(
            NationInfoController controller)
        {
            List<PriorityListItemController> rows =
                new List<PriorityListItemController>();
            foreach (object item in controller.priorityList)
            {
                PriorityListItemController row = item as PriorityListItemController;
                if (row != null &&
                    row.priorityAccumulation != null &&
                    row.gameObject.activeInHierarchy &&
                    IsDebtFundingPriority(row.priority))
                {
                    rows.Add(row);
                }
            }
            return rows;
        }

        private static RepairDebtOverlayState GetOrCreateState(
            NationInfoController controller,
            PriorityListItemController referenceRow,
            RectTransform commonParent)
        {
            RepairDebtOverlayState state;
            if (!States.TryGetValue(controller, out state))
            {
                state = new RepairDebtOverlayState();
                States.Add(controller, state);
            }

            if (state.container != null && state.container.transform.parent != commonParent)
            {
                state.container.SetActive(false);
                UnityEngine.Object.Destroy(state.container);
                state.container = null;
            }

            if (state.container == null)
            {
                state.container = new GameObject(
                    "EEO Repair Debt Investment Span",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(LayoutElement));
                state.container.transform.SetParent(commonParent, false);
                state.rectTransform = state.container.GetComponent<RectTransform>();
                state.background = state.container.GetComponent<Image>();
                state.background.raycastTarget = false;
                state.container.GetComponent<LayoutElement>().ignoreLayout = true;

                GameObject labelObject = UnityEngine.Object.Instantiate(
                    referenceRow.priorityAccumulation.gameObject,
                    state.container.transform,
                    false);
                labelObject.name = "Repair Debt Value";
                state.label = labelObject.GetComponent<TMP_Text>();
                state.label.alignment = TextAlignmentOptions.Center;
                state.label.raycastTarget = false;
                RectTransform labelRect = state.label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                labelRect.localScale = Vector3.one;
                labelRect.localRotation = Quaternion.identity;
            }

            return state;
        }

        private static Image FindRowBackground(PriorityListItemController row)
        {
            Transform current = row.transform;
            Transform boundary = row.transform.parent;
            while (current != null && current != boundary)
            {
                Image image = current.GetComponent<Image>();
                if (image != null && image.enabled)
                {
                    return image;
                }
                current = current.parent;
            }
            return null;
        }

        private static void CopyBackgroundStyle(Image destination, Image source)
        {
            if (source == null)
            {
                destination.color = Color.clear;
                destination.sprite = null;
                return;
            }

            destination.color = source.color;
            destination.sprite = source.sprite;
            destination.material = source.material;
            destination.type = source.type;
            destination.preserveAspect = source.preserveAspect;
            destination.fillCenter = source.fillCenter;
            destination.fillMethod = source.fillMethod;
            destination.fillAmount = source.fillAmount;
            destination.fillClockwise = source.fillClockwise;
            destination.fillOrigin = source.fillOrigin;
        }

        private static void RestoreHiddenValues(RepairDebtOverlayState state)
        {
            foreach (GameObject value in state.hiddenValues)
            {
                if (value != null)
                {
                    value.SetActive(true);
                }
            }
            state.hiddenValues.Clear();
        }
    }

    [HarmonyPatch(typeof(NationInfoController), "UpdatePriorityList")]
    public static class RepairDebtPriorityOverlayPatch
    {
        [HarmonyPrefix]
        public static void Prefix(NationInfoController __instance)
        {
            RepairDebtOverlayRuntime.Prepare(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(NationInfoController __instance)
        {
            RepairDebtOverlayRuntime.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(NationInfoController), "Hide")]
    public static class RepairDebtPriorityOverlayCleanupPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NationInfoController __instance)
        {
            RepairDebtOverlayRuntime.Dispose(__instance);
        }
    }
}
