using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class GlobalTechnologySelectionMath
    {
        public const double TierZeroMultiplier = 1d;
        public const double TierOneMultiplier = 2d;
        public const double TierTwoMultiplier = 4d;
        public const double TierFiveMultiplier = 10d;
        public const double TierSevenMultiplier = 14d;
        public const double MinimumSelectionWeight = 1e-37d;

        public static double Median(IList<double> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0d;
            }

            double[] sorted = new double[values.Count];
            for (int index = 0; index < values.Count; index++)
            {
                double value = values[index];
                if (!IsFinite(value) || value <= 0d)
                {
                    return 0d;
                }
                sorted[index] = value;
            }
            Array.Sort(sorted);

            int middle = sorted.Length / 2;
            return sorted.Length % 2 == 1
                ? sorted[middle]
                : (sorted[middle - 1] + sorted[middle]) / 2d;
        }

        public static double PriorityMultiplier(int tier)
        {
            if (tier >= 7)
            {
                return TierSevenMultiplier;
            }
            if (tier >= 5)
            {
                return TierFiveMultiplier;
            }
            if (tier >= 2)
            {
                return TierTwoMultiplier;
            }
            if (tier >= 1)
            {
                return TierOneMultiplier;
            }
            return TierZeroMultiplier;
        }

        public static double CostMultiplier(
            double researchCost,
            double medianResearchCost,
            double exponent,
            double minimumMultiplier,
            double maximumMultiplier)
        {
            if (!IsFinite(researchCost) || researchCost <= 0d ||
                !IsFinite(medianResearchCost) || medianResearchCost <= 0d ||
                !IsFinite(exponent) || exponent <= 0d ||
                !IsFinite(minimumMultiplier) || minimumMultiplier <= 0d ||
                !IsFinite(maximumMultiplier) ||
                maximumMultiplier < minimumMultiplier)
            {
                return 0d;
            }

            double multiplier = Math.Pow(
                medianResearchCost / researchCost, exponent);
            if (!IsFinite(multiplier))
            {
                return 0d;
            }
            return Math.Max(minimumMultiplier,
                Math.Min(maximumMultiplier, multiplier));
        }

        public static double SelectionWeight(
            double categoryPreference,
            double rolePreference,
            int effectiveTier,
            double costMultiplier,
            double contextMultiplier)
        {
            double weight = categoryPreference * rolePreference *
                PriorityMultiplier(effectiveTier) * costMultiplier *
                contextMultiplier;
            if (!IsFinite(weight) || weight < 0d)
            {
                return 0d;
            }
            return Math.Max(MinimumSelectionWeight, weight);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
