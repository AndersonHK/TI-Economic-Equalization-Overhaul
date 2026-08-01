using System;

namespace TIEconomyMod
{
    public static class LandCombatMath
    {
        public static double ScaleContribution(double contribution, double scale)
        {
            if (!MilitaryMath.IsFinite(contribution) || !MilitaryMath.IsFinite(scale) ||
                scale < 0d)
            {
                return double.NaN;
            }
            return contribution * scale;
        }

        public static double StrengthPenalty(double strength, double maximumPenalty)
        {
            if (!MilitaryMath.IsFinite(strength) || !MilitaryMath.IsFinite(maximumPenalty) ||
                maximumPenalty < 0d)
            {
                return double.NaN;
            }

            double clampedStrength = Math.Max(0d, Math.Min(1d, strength));
            return maximumPenalty * (1d - clampedStrength);
        }

        public static double RatingAfterStrength(double uninjuredRating, double strength, double maximumPenalty)
        {
            double penalty = StrengthPenalty(strength, maximumPenalty);
            return MilitaryMath.IsFinite(uninjuredRating) && MilitaryMath.IsFinite(penalty)
                ? uninjuredRating - penalty
                : double.NaN;
        }

        public static double HitChance(double attack, double defense, double curveBase)
        {
            if (!MilitaryMath.IsFinite(attack) || !MilitaryMath.IsFinite(defense) ||
                !MilitaryMath.IsFinite(curveBase) || curveBase <= 1d)
            {
                return double.NaN;
            }

            double difference = attack - defense;
            double probability = difference >= 0d
                ? 1d - 0.5d * Math.Pow(curveBase, -difference)
                : 0.5d * Math.Pow(curveBase, difference);
            if (!MilitaryMath.IsFinite(probability))
            {
                return difference > 0d ? 1d : 0d;
            }
            return Math.Max(0d, Math.Min(1d, probability));
        }
    }
}
