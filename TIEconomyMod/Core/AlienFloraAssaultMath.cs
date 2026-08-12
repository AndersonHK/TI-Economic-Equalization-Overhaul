using System;

namespace TIEconomyMod
{
    /// <summary>
    /// Dependency-free scaling for damage received while armies clear alien flora.
    /// </summary>
    public static class AlienFloraAssaultMath
    {
        public static double DamageScale(
            double xenoformingLevel,
            double fullDamageLevel)
        {
            if (!MilitaryMath.IsFinite(xenoformingLevel) ||
                !MilitaryMath.IsFinite(fullDamageLevel) ||
                fullDamageLevel <= 0d)
            {
                return double.NaN;
            }

            return Math.Max(0d, Math.Min(1d,
                xenoformingLevel / fullDamageLevel));
        }

        public static double ScaledDamage(
            double vanillaDamage,
            double xenoformingLevel,
            double fullDamageLevel)
        {
            if (!MilitaryMath.IsFinite(vanillaDamage) || vanillaDamage < 0d)
            {
                return double.NaN;
            }

            double scale = DamageScale(xenoformingLevel, fullDamageLevel);
            return MilitaryMath.IsFinite(scale)
                ? vanillaDamage * scale
                : double.NaN;
        }
    }
}
