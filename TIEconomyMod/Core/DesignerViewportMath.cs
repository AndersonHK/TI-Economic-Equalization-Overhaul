using System;

namespace TIEconomyMod.Core
{
    // Coordinates use right/down-positive UI units. No Unity calls: usable in
    // headless checks as well as with measured runtime RectTransforms.
    public struct DesignerRect
    {
        public float Left, Top, Right, Bottom;
        public float Width { get { return Right - Left; } }
        public float Height { get { return Bottom - Top; } }
        public DesignerRect(float left, float top, float right, float bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
        public DesignerRect Translate(float x, float y) { return new DesignerRect(Left + x, Top + y, Right + x, Bottom + y); }
        public bool Overlaps(DesignerRect b) { return Math.Min(Right, b.Right) - Math.Max(Left, b.Left) > .001f && Math.Min(Bottom, b.Bottom) - Math.Max(Top, b.Top) > .001f; }
        public bool Contains(DesignerRect b, float tolerance)
        {
            return b.Left >= Left - tolerance && b.Top >= Top - tolerance && b.Right <= Right + tolerance && b.Bottom <= Bottom + tolerance;
        }
        public static DesignerRect Union(DesignerRect a, DesignerRect b)
        {
            return new DesignerRect(Math.Min(a.Left, b.Left), Math.Min(a.Top, b.Top), Math.Max(a.Right, b.Right), Math.Max(a.Bottom, b.Bottom));
        }
    }

    public struct DesignerPlacement
    {
        public bool Fits;
        public float OriginX, OriginY, MissingWidth, MissingHeight;
    }

    public static class DesignerViewportMath
    {
        public static DesignerPlacement Place(DesignerRect content, DesignerRect viewport, float preferredX, float preferredY)
        {
            if (!Usable(content) || !Usable(viewport) || !Finite(preferredX) || !Finite(preferredY)) return new DesignerPlacement();
            var fit = new DesignerPlacement { OriginX = preferredX, OriginY = preferredY,
                MissingWidth = Math.Max(0f, content.Width - viewport.Width), MissingHeight = Math.Max(0f, content.Height - viewport.Height) };
            if (viewport.Width <= 0 || viewport.Height <= 0 || fit.MissingWidth > .01f || fit.MissingHeight > .01f) return fit;
            fit.Fits = true;
            fit.OriginX = Clamp(preferredX, viewport.Left - content.Left, viewport.Right - content.Right);
            fit.OriginY = -Clamp(-preferredY, viewport.Top - content.Top, viewport.Bottom - content.Bottom);
            return fit;
        }
        public static float WindowScale(float width, float height, float availableWidth, float availableHeight)
        {
            if (!Finite(width) || !Finite(height) || !Finite(availableWidth) || !Finite(availableHeight) || width <= 0 || height <= 0 || availableWidth <= 0 || availableHeight <= 0) return 0;
            return Math.Min(1f, Math.Min(availableWidth / width, availableHeight / height));
        }
        public static bool Usable(DesignerRect rect) { return Finite(rect.Left) && Finite(rect.Top) && Finite(rect.Right) && Finite(rect.Bottom) && rect.Width > 0 && rect.Height > 0; }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static float Clamp(float value, float minimum, float maximum) { return Math.Max(minimum, Math.Min(maximum, value)); }
    }
}
