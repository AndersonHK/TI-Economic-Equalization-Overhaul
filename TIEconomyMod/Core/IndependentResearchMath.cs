using System;

namespace TIEconomyMod
{
    public static class IndependentResearchMath
    {
        public const float DaysPerYear = 365.2422f;
        public const int GlobalTechnologySlots = 3;
        public const float CompletionEpsilon = 0.001f;

        public static float MonthlyNeutralShare(
            float nationResearchMonth,
            int neutralControlPoints,
            int totalControlPoints)
        {
            if (float.IsNaN(nationResearchMonth) ||
                float.IsInfinity(nationResearchMonth) ||
                nationResearchMonth <= 0f ||
                neutralControlPoints <= 0 ||
                totalControlPoints <= 0)
            {
                return 0f;
            }

            int boundedNeutral = Math.Min(neutralControlPoints, totalControlPoints);
            return nationResearchMonth * boundedNeutral / totalControlPoints;
        }

        public static bool IsNeutralResearchControlPoint(
            bool owned,
            bool benefitsDisabled)
        {
            // A faction allocates a share only while it owns the point and its
            // benefits are active. Every complementary share enters the neutral pool.
            return !owned || benefitsDisabled;
        }

        public static int NeutralControlPointCount(
            int totalControlPoints,
            int factionAllocatableControlPoints)
        {
            if (totalControlPoints <= 0)
            {
                return 0;
            }

            int boundedFactionAllocatable = Math.Max(
                0,
                Math.Min(factionAllocatableControlPoints, totalControlPoints));
            return totalControlPoints - boundedFactionAllocatable;
        }

        public static float DailyPerGlobalTechnology(float monthlyIndependentResearch)
        {
            if (float.IsNaN(monthlyIndependentResearch) ||
                float.IsInfinity(monthlyIndependentResearch) ||
                monthlyIndependentResearch <= 0f)
            {
                return 0f;
            }

            return monthlyIndependentResearch * 12f /
                DaysPerYear / GlobalTechnologySlots;
        }

        public static float IndependentProgress(
            float accumulatedResearch,
            float attributedResearch)
        {
            if (float.IsNaN(accumulatedResearch) ||
                float.IsInfinity(accumulatedResearch) ||
                float.IsNaN(attributedResearch) ||
                float.IsInfinity(attributedResearch))
            {
                return 0f;
            }

            return Math.Max(0f, accumulatedResearch - Math.Max(0f, attributedResearch));
        }

        public static float GuardUnattributedCompletion(
            float accumulatedResearch,
            float researchCost,
            bool hasFactionContribution)
        {
            if (hasFactionContribution ||
                researchCost <= 0f ||
                accumulatedResearch < researchCost)
            {
                return accumulatedResearch;
            }

            return Math.Max(0f, researchCost - CompletionEpsilon);
        }
    }
}
