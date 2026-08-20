using System;

namespace TIEconomyMod
{
    /// <summary>
    /// Dependency-free Environment rating, investment, emissions, and cleanup math.
    /// The serialized TI sustainability value remains an inverse carrier for save
    /// compatibility; all gameplay formulas operate on the bounded 0-10 rating.
    /// </summary>
    public static class EnvironmentMath
    {
        private const int SolverIterations = 80;
        private const double Epsilon = 1e-10d;
        public const double NeutralEpsilon = 1e-5d;

        public static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static double RatingFromStored(double stored, double storageOffset)
        {
            if (!IsFinite(stored) || !IsFinite(storageOffset) ||
                stored <= 0d || storageOffset < 0d)
            {
                return double.NaN;
            }

            return Math.Max(0d, Math.Min(10d, 1d / stored - storageOffset));
        }

        public static double StoredFromRating(double rating, double storageOffset)
        {
            if (!IsFinite(rating) || !IsFinite(storageOffset) ||
                rating < 0d || storageOffset <= 0d)
            {
                return double.NaN;
            }

            return 1d / (Math.Min(10d, rating) + storageOffset);
        }

        public static double TechnologyCap(
            double baseCap,
            double maximumCap,
            double unlockedPoints)
        {
            if (!IsFinite(baseCap) || !IsFinite(maximumCap) ||
                !IsFinite(unlockedPoints) || baseCap <= 0d ||
                maximumCap < baseCap)
            {
                return double.NaN;
            }

            return Math.Max(baseCap, Math.Min(maximumCap, baseCap + unlockedPoints));
        }

        public static double AdvancementCost(
            double fromRating,
            double toRating,
            double cap,
            double gdpBillions,
            double referenceGdpBillions,
            double baseIp,
            double growthBase)
        {
            if (!IsFinite(fromRating) || !IsFinite(toRating) || !IsFinite(cap) ||
                !IsFinite(gdpBillions) || !IsFinite(referenceGdpBillions) ||
                !IsFinite(baseIp) || !IsFinite(growthBase) ||
                fromRating < 0d || toRating < fromRating || toRating > cap + Epsilon ||
                cap <= 0d || gdpBillions < 0d || referenceGdpBillions <= 0d ||
                baseIp < 0d || growthBase < 1d)
            {
                return double.NaN;
            }

            if (toRating - fromRating <= Epsilon || gdpBillions <= Epsilon)
            {
                return 0d;
            }

            double fromLevel = 10d * fromRating / cap;
            double toLevel = 10d * toRating / cap;
            double normalizedCost;
            if (growthBase - 1d <= Epsilon)
            {
                normalizedCost = toLevel - fromLevel;
            }
            else
            {
                normalizedCost = (Math.Pow(growthBase, toLevel) -
                    Math.Pow(growthBase, fromLevel)) / (growthBase - 1d);
            }

            double cost = gdpBillions / referenceGdpBillions * baseIp * normalizedCost;
            return IsFinite(cost) && cost >= 0d ? cost : double.NaN;
        }

        public static bool TryRatingAfterInvestment(
            double currentRating,
            double cap,
            double gdpBillions,
            double investment,
            double referenceGdpBillions,
            double baseIp,
            double growthBase,
            out double rating)
        {
            rating = currentRating;
            if (!IsFinite(currentRating) || !IsFinite(cap) ||
                !IsFinite(gdpBillions) || !IsFinite(investment) ||
                currentRating < 0d || currentRating > cap + Epsilon ||
                cap <= 0d || gdpBillions < 0d || investment < 0d)
            {
                return false;
            }

            if (investment <= Epsilon || cap - currentRating <= Epsilon)
            {
                rating = Math.Min(currentRating, cap);
                return true;
            }

            double remaining = AdvancementCost(
                currentRating, cap, cap, gdpBillions, referenceGdpBillions,
                baseIp, growthBase);
            if (!IsFinite(remaining) || remaining < 0d)
            {
                return false;
            }
            if (investment >= remaining - Epsilon)
            {
                rating = cap;
                return true;
            }

            double low = currentRating;
            double high = cap;
            for (int index = 0; index < SolverIterations; index++)
            {
                double middle = (low + high) * 0.5d;
                double cost = AdvancementCost(
                    currentRating, middle, cap, gdpBillions,
                    referenceGdpBillions, baseIp, growthBase);
                if (!IsFinite(cost))
                {
                    return false;
                }
                if (cost < investment)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            rating = Math.Max(currentRating, Math.Min(cap, (low + high) * 0.5d));
            return IsFinite(rating);
        }

        public static double GdpGasTons(
            double gdpBillions,
            double rating,
            double tonsPerGdpBillionAtScoreZero,
            double decayBase,
            double resourceMultiplier,
            double neutralRating)
        {
            if (!IsFinite(gdpBillions) || !IsFinite(rating) ||
                !IsFinite(tonsPerGdpBillionAtScoreZero) || !IsFinite(decayBase) ||
                !IsFinite(resourceMultiplier) || !IsFinite(neutralRating) ||
                gdpBillions < 0d || rating < 0d || tonsPerGdpBillionAtScoreZero < 0d ||
                decayBase <= 0d || decayBase > 1d || resourceMultiplier < 0d ||
                neutralRating <= 0d)
            {
                return double.NaN;
            }

            if (rating >= neutralRating - NeutralEpsilon)
            {
                return 0d;
            }

            double tons = gdpBillions * tonsPerGdpBillionAtScoreZero *
                Math.Pow(decayBase, rating) * resourceMultiplier;
            return IsFinite(tons) && tons >= 0d ? tons : double.NaN;
        }

        public static double PopulationGasTons(
            double populationMillions,
            double rating,
            double tonsPerMillionAtScoreZero,
            double decayBase,
            double neutralRating)
        {
            if (!IsFinite(populationMillions) || !IsFinite(rating) ||
                !IsFinite(tonsPerMillionAtScoreZero) || !IsFinite(decayBase) ||
                !IsFinite(neutralRating) || populationMillions < 0d || rating < 0d ||
                tonsPerMillionAtScoreZero < 0d || decayBase <= 0d ||
                decayBase > 1d || neutralRating <= 0d)
            {
                return double.NaN;
            }

            if (rating >= neutralRating - NeutralEpsilon)
            {
                return 0d;
            }

            double tons = populationMillions * tonsPerMillionAtScoreZero *
                Math.Pow(decayBase, rating);
            return IsFinite(tons) && tons >= 0d ? tons : double.NaN;
        }

        public static double ClippedRemoval(double current, double safe, double packet)
        {
            if (!IsFinite(current) || !IsFinite(safe) || !IsFinite(packet) || packet < 0d)
            {
                return double.NaN;
            }

            return -Math.Min(packet, Math.Max(0d, current - safe));
        }
    }
}
