using System.Runtime.CompilerServices;
using TIEconomyMod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TIEconomyMod
{
    public static class UtilityFootprintIconVisuals
    {
        private sealed class PreviewState
        {
            public readonly float Width;
            public readonly float Height;
            public readonly bool PreserveAspect;
            public readonly Vector3 LocalScale;
            public GameObject VerticalDivider;
            public GameObject HorizontalDivider;

            public PreviewState(Image image)
            {
                Rect rect = image.rectTransform.rect;
                Width = rect.width;
                Height = rect.height;
                PreserveAspect = image.preserveAspect;
                LocalScale = image.rectTransform.localScale;
            }
        }

        private static readonly ConditionalWeakTable<Image, PreviewState>
            PreviewStates =
                new ConditionalWeakTable<Image, PreviewState>();

        public static void ApplyPreview(
            Image image,
            UtilityFootprintKind footprint)
        {
            if (image == null)
            {
                return;
            }

            PreviewState state = PreviewStates.GetValue(
                image, value => new PreviewState(value));
            float width = state.Width;
            float height = state.Height;
            switch (footprint)
            {
            case UtilityFootprintKind.TwoHorizontal:
                height *= 0.5f;
                break;
            case UtilityFootprintKind.TwoVertical:
                width *= 0.5f;
                break;
            }

            RectTransform rectTransform = image.rectTransform;
            rectTransform.localScale = state.LocalScale;
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, height);
            image.preserveAspect =
                footprint == UtilityFootprintKind.Single
                    ? state.PreserveAspect
                    : false;
            ApplyDividers(
                image,
                footprint == UtilityFootprintKind.Four);
        }

        public static void ApplyCatalogPreview(
            Image image,
            UtilityFootprintKind footprint)
        {
            if (image == null)
            {
                return;
            }

            PreviewState state = PreviewStates.GetValue(
                image, value => new PreviewState(value));
            RectTransform rectTransform = image.rectTransform;
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, state.Width);
            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, state.Height);

            Vector3 scale = state.LocalScale;
            switch (footprint)
            {
            case UtilityFootprintKind.TwoHorizontal:
                scale.y *= 0.5f;
                break;
            case UtilityFootprintKind.TwoVertical:
                scale.x *= 0.5f;
                break;
            }
            rectTransform.localScale = scale;
            image.preserveAspect =
                footprint == UtilityFootprintKind.Single
                    ? state.PreserveAspect
                    : false;
            ApplyDividers(
                image,
                footprint == UtilityFootprintKind.Four);
        }

        public static void ApplyDividers(Image image, bool visible)
        {
            if (image == null)
            {
                return;
            }

            PreviewState state = PreviewStates.GetValue(
                image, value => new PreviewState(value));
            if (visible && state.VerticalDivider == null)
            {
                state.VerticalDivider = CreateDivider(
                    image.rectTransform, true);
                state.HorizontalDivider = CreateDivider(
                    image.rectTransform, false);
            }

            if (state.VerticalDivider != null)
            {
                state.VerticalDivider.SetActive(visible);
                state.HorizontalDivider.SetActive(visible);
            }
        }

        public static void HideDividers(Image image)
        {
            ApplyDividers(image, false);
        }

        public static void SetDividerAlpha(Image image, bool fullyVisible)
        {
            PreviewState state;
            if (image == null ||
                !PreviewStates.TryGetValue(image, out state) ||
                state.VerticalDivider == null)
            {
                return;
            }

            float alpha = fullyVisible ? 0.75f : 0.225f;
            SetDividerAlpha(state.VerticalDivider, alpha);
            SetDividerAlpha(state.HorizontalDivider, alpha);
        }

        private static GameObject CreateDivider(
            RectTransform parent,
            bool vertical)
        {
            GameObject divider = new GameObject(
                vertical
                    ? "EEO_FootprintVerticalDivider"
                    : "EEO_FootprintHorizontalDivider");
            RectTransform transform =
                divider.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            if (vertical)
            {
                transform.anchorMin = new Vector2(0.5f, 0f);
                transform.anchorMax = new Vector2(0.5f, 1f);
                transform.sizeDelta = new Vector2(1.5f, 0f);
            }
            else
            {
                transform.anchorMin = new Vector2(0f, 0.5f);
                transform.anchorMax = new Vector2(1f, 0.5f);
                transform.sizeDelta = new Vector2(0f, 1.5f);
            }
            transform.anchoredPosition = Vector2.zero;

            Image line = divider.AddComponent<Image>();
            line.color = new Color(0.8f, 0.95f, 1f, 0.75f);
            line.raycastTarget = false;
            return divider;
        }

        private static void SetDividerAlpha(GameObject divider, float alpha)
        {
            if (divider == null)
            {
                return;
            }

            Image line = divider.GetComponent<Image>();
            if (line != null)
            {
                Color color = line.color;
                color.a = alpha;
                line.color = color;
            }
        }
    }
}
