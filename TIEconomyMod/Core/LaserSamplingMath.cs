using System;

namespace TIEconomyMod.Core
{
    public static class LaserSamplingMath
    {
        public const double CoreRibbonRatio = 0.46;
        public static double RibbonWidth(double coreDiameter)
        {
            if (Double.IsNaN(coreDiameter) || Double.IsInfinity(coreDiameter) || coreDiameter <= 0)
                throw new ArgumentOutOfRangeException(nameof(coreDiameter));
            return coreDiameter / CoreRibbonRatio;
        }
        public static double ParticleRibbonWidth(double authoredWidth, double inheritedScale)
        {
            if (Double.IsNaN(authoredWidth) || Double.IsInfinity(authoredWidth) || authoredWidth<=0 ||
                Double.IsNaN(inheritedScale) || Double.IsInfinity(inheritedScale) || inheritedScale<=0)
                throw new ArgumentOutOfRangeException(nameof(authoredWidth));
            return authoredWidth*inheritedScale;
        }
        private static double Primitive(double x, double exponent)
        {
            x = Math.Max(-1, Math.Min(1, x));
            return x - x * Math.Pow(Math.Abs(x), exponent) / (exponent + 1);
        }
        public static double NativeCore(double offset, double radius, double exponent, double halfUVFootprint)
        {
            double point=1-Math.Min(1,Math.Pow(Math.Abs(offset),exponent)/radius);
            double support=Math.Pow(radius,1/exponent);
            double h=halfUVFootprint/support;
            if (h<=0.25) return point;
            double filtered=FilteredCore(offset/support,h,exponent);
            double t=Math.Max(0,Math.Min(1,(h-0.25)/0.25));
            double weight=t*t*(3-2*t);
            return point+(filtered-point)*weight;
        }
        // Independent CPU reference for shader footprint validation, never a frame-time path.
        public static double FilteredCore(double normalizedDistance, double halfFootprint, double exponent)
        {
            if (halfFootprint < 0 || Double.IsNaN(halfFootprint) || Double.IsInfinity(halfFootprint) ||
                exponent <= 0 || Double.IsNaN(exponent) || Double.IsInfinity(exponent) ||
                Double.IsNaN(normalizedDistance) || Double.IsInfinity(normalizedDistance))
                throw new ArgumentOutOfRangeException(nameof(halfFootprint));
            if (halfFootprint < 0.001)
                return Math.Max(0, 1 - Math.Pow(Math.Abs(normalizedDistance), exponent));
            return Math.Max(0, Math.Min(1, (Primitive(normalizedDistance + halfFootprint, exponent) -
                Primitive(normalizedDistance - halfFootprint, exponent)) / (2 * halfFootprint)));
        }
    }
}
