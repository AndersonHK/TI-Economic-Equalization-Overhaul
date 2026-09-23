using System;

namespace TIEconomyMod
{
    internal static class NationalSpaceAssetMath
    {
        internal static double Upkeep(double mc, double boostMonth, double fundingMonth,
            double mcRate, double boostRate, double fundingRate)
        {
            return Math.Max(0d, mc) * mcRate + Math.Max(0d, boostMonth) * boostRate +
                Math.Max(0d, fundingMonth) * fundingRate;
        }

        internal static double Available(double beforeUpkeep, double upkeep)
        {
            return Math.Max(0d, beforeUpkeep - upkeep);
        }

        internal static double BoostCapMonth(double gdpBillions, double education, double factor)
        {
            return factor * Math.Max(0d, gdpBillions) / Math.Max(200d, 300d - 6d * education);
        }

        internal static double BoostHeadroomYear(double currentYear, double capMonth)
        {
            double capYear = capMonth * 12d;
            double remaining = capYear - currentYear;
            // Float-backed region stocks may round the final addition slightly below cap.
            return remaining > Math.Max(0.00001d, capYear * 0.000001d) ? remaining : 0d;
        }

        internal static double ClampBoostChange(double changeYear, double currentYear, double capMonth)
        {
            return changeYear <= 0d ? changeYear :
                Math.Min(changeYear, BoostHeadroomYear(currentYear, capMonth));
        }

        internal static int DirectInvestmentLimit(double headroomYear, double largestGainYear,
            double completionCost, double accumulated, int vanillaLimit)
        {
            if (headroomYear <= 0d || largestGainYear <= 0d || completionCost <= 0d)
                return 0;
            // Conservative batch limit: even if every completion uses the best latitude,
            // at most the final completion is partial. Slower sites can buy another batch.
            double completions = Math.Ceiling(headroomYear / largestGainYear);
            double needed = Math.Max(0d, completions * completionCost - Math.Max(0d, accumulated));
            return (int)Math.Min(Math.Max(0, vanillaLimit), Math.Ceiling(needed));
        }
    }
}
