using System;

namespace TIEconomyMod
{
    /// <summary>
    /// Dependency-free military investment math. Runtime patches and formula tests use
    /// the same implementation so fractional technology cannot disagree with UI costs.
    /// </summary>
    public static class MilitaryMath
    {
        private const int SolverIterations = 80;
        private const double Epsilon = 1e-10;
        private const double EulerMascheroni = 0.5772156649015328606d;

        public static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static double ArmyCost(double technology, double coefficient, double growthBase)
        {
            if (!IsFinite(technology) || !IsFinite(coefficient) || !IsFinite(growthBase) ||
                technology < 0d || coefficient < 0d || growthBase < 1d)
            {
                return double.NaN;
            }

            double cost = coefficient * Math.Pow(growthBase, technology);
            return IsFinite(cost) ? cost : double.NaN;
        }

        public static double ArmyUpgradeCost(
            double fromTechnology,
            double toTechnology,
            int armyCount,
            double coefficient,
            double growthBase)
        {
            if (armyCount < 0 || toTechnology < fromTechnology)
            {
                return double.NaN;
            }

            double fromCost = ArmyCost(fromTechnology, coefficient, growthBase);
            double toCost = ArmyCost(toTechnology, coefficient, growthBase);
            return IsFinite(fromCost) && IsFinite(toCost)
                ? armyCount * (toCost - fromCost)
                : double.NaN;
        }

        public static double CatchUpCostMultiplier(
            double technology,
            double cap,
            double gapCoefficient)
        {
            if (!IsFinite(technology) || !IsFinite(cap) || !IsFinite(gapCoefficient) ||
                gapCoefficient < 0d)
            {
                return double.NaN;
            }

            return 1d / (1d + gapCoefficient * Math.Max(0d, cap - technology));
        }

        public static double DoctrineCost(
            double fromTechnology,
            double toTechnology,
            double cap,
            double doctrineBaseCostAtTechOne,
            double doctrineCostGrowthBase,
            double gapCoefficient)
        {
            if (!IsFinite(fromTechnology) || !IsFinite(toTechnology) || !IsFinite(cap) ||
                !IsFinite(doctrineBaseCostAtTechOne) || !IsFinite(doctrineCostGrowthBase) ||
                !IsFinite(gapCoefficient) ||
                fromTechnology < 0d || toTechnology < fromTechnology || toTechnology > cap ||
                doctrineBaseCostAtTechOne < 0d || doctrineCostGrowthBase < 1d ||
                gapCoefficient < 0d)
            {
                return double.NaN;
            }

            if (toTechnology - fromTechnology <= Epsilon)
            {
                return 0d;
            }

            if (doctrineCostGrowthBase - 1d <= Epsilon)
            {
                if (gapCoefficient <= Epsilon)
                {
                    return doctrineBaseCostAtTechOne *
                        (toTechnology - fromTechnology);
                }

                double numerator = 1d + gapCoefficient * (cap - fromTechnology);
                double denominator = 1d + gapCoefficient * (cap - toTechnology);
                return numerator > 0d && denominator > 0d
                    ? doctrineBaseCostAtTechOne / gapCoefficient *
                        Math.Log(numerator / denominator)
                    : double.NaN;
            }

            double logarithmicGrowth = Math.Log(doctrineCostGrowthBase);
            double growthDenominator = doctrineCostGrowthBase - 1d;
            if (gapCoefficient <= Epsilon)
            {
                // This cumulative extension guarantees that every undiscounted
                // integer interval n->n+1 costs BaseCost * GrowthBase^(n-1).
                return doctrineBaseCostAtTechOne / growthDenominator *
                    (Math.Pow(doctrineCostGrowthBase, toTechnology - 1d) -
                     Math.Pow(doctrineCostGrowthBase, fromTechnology - 1d));
            }

            // Integrate the exponential doctrine rate and smooth catch-up divisor
            // analytically. E1 is the real exponential integral for positive inputs.
            double fromGap = 1d + gapCoefficient * (cap - fromTechnology);
            double toGap = 1d + gapCoefficient * (cap - toTechnology);
            double exponentialScale = Math.Exp(
                logarithmicGrowth * (cap - 1d + 1d / gapCoefficient));
            double rateScale = doctrineBaseCostAtTechOne * logarithmicGrowth /
                growthDenominator;
            double argumentScale = logarithmicGrowth / gapCoefficient;
            double calculated = rateScale / gapCoefficient * exponentialScale *
                (ExponentialIntegralE1(argumentScale * toGap) -
                 ExponentialIntegralE1(argumentScale * fromGap));
            return IsFinite(calculated) && calculated >= 0d
                ? calculated
                : double.NaN;
        }

        public static double MiltechCost(
            double fromTechnology,
            double toTechnology,
            int armyCount,
            double cap,
            double armyCostCoefficient,
            double armyCostGrowthBase,
            double doctrineBaseCostAtTechOne,
            double doctrineCostGrowthBase,
            double gapCoefficient)
        {
            double doctrine = DoctrineCost(
                fromTechnology, toTechnology, cap, doctrineBaseCostAtTechOne,
                doctrineCostGrowthBase, gapCoefficient);
            double upgrades = ArmyUpgradeCost(
                fromTechnology, toTechnology, armyCount, armyCostCoefficient, armyCostGrowthBase);
            return IsFinite(doctrine) && IsFinite(upgrades) ? doctrine + upgrades : double.NaN;
        }

        public static bool TrySolveTechAfterInvestment(
            double currentTechnology,
            double cap,
            int armyCount,
            double investment,
            double armyCostCoefficient,
            double armyCostGrowthBase,
            double doctrineBaseCostAtTechOne,
            double doctrineCostGrowthBase,
            double gapCoefficient,
            out double technology)
        {
            technology = currentTechnology;
            if (!IsFinite(currentTechnology) || !IsFinite(cap) || !IsFinite(investment) ||
                currentTechnology < 0d || cap < currentTechnology || armyCount < 0 ||
                investment < 0d)
            {
                return false;
            }

            if (investment <= Epsilon || cap - currentTechnology <= Epsilon)
            {
                technology = Math.Min(currentTechnology, cap);
                return true;
            }

            double remaining = MiltechCost(
                currentTechnology, cap, armyCount, cap, armyCostCoefficient,
                armyCostGrowthBase, doctrineBaseCostAtTechOne,
                doctrineCostGrowthBase, gapCoefficient);
            if (!IsFinite(remaining) || remaining < 0d)
            {
                return false;
            }
            if (investment >= remaining - Epsilon)
            {
                technology = cap;
                return true;
            }

            double low = currentTechnology;
            double high = cap;
            for (int index = 0; index < SolverIterations; index++)
            {
                double middle = (low + high) * 0.5d;
                double cost = MiltechCost(
                    currentTechnology, middle, armyCount, cap, armyCostCoefficient,
                    armyCostGrowthBase, doctrineBaseCostAtTechOne,
                    doctrineCostGrowthBase, gapCoefficient);
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

            technology = Math.Max(currentTechnology, Math.Min(cap, (low + high) * 0.5d));
            return IsFinite(technology);
        }

        public static bool TrySolveConservedTechnology(
            double firstTechnology,
            int firstArmyCount,
            double secondTechnology,
            int secondArmyCount,
            double cap,
            double armyCostCoefficient,
            double armyCostGrowthBase,
            double doctrineBaseCostAtTechOne,
            double doctrineCostGrowthBase,
            double gapCoefficient,
            out double technology)
        {
            technology = Math.Max(firstTechnology, secondTechnology);
            if (!IsFinite(firstTechnology) || !IsFinite(secondTechnology) || !IsFinite(cap) ||
                firstTechnology < 0d || secondTechnology < 0d ||
                firstArmyCount < 0 || secondArmyCount < 0)
            {
                return false;
            }

            double lowTech;
            double highTech;
            int lowCount;
            int highCount;
            if (firstTechnology <= secondTechnology)
            {
                lowTech = firstTechnology;
                lowCount = firstArmyCount;
                highTech = secondTechnology;
                highCount = secondArmyCount;
            }
            else
            {
                lowTech = secondTechnology;
                lowCount = secondArmyCount;
                highTech = firstTechnology;
                highCount = firstArmyCount;
            }

            if (highTech > cap + Epsilon)
            {
                return false;
            }
            if (highTech - lowTech <= Epsilon || lowCount == 0)
            {
                technology = highTech;
                return true;
            }

            double low = lowTech;
            double high = highTech;
            for (int index = 0; index < SolverIterations; index++)
            {
                double middle = (low + high) * 0.5d;
                double relinquishedDoctrine = DoctrineCost(
                    middle, highTech, cap, doctrineBaseCostAtTechOne,
                    doctrineCostGrowthBase, gapCoefficient);
                double releasedHighEquipment = ArmyUpgradeCost(
                    middle, highTech, highCount, armyCostCoefficient, armyCostGrowthBase);
                double requiredLowEquipment = ArmyUpgradeCost(
                    lowTech, middle, lowCount, armyCostCoefficient, armyCostGrowthBase);
                if (!IsFinite(relinquishedDoctrine) || !IsFinite(releasedHighEquipment) ||
                    !IsFinite(requiredLowEquipment))
                {
                    return false;
                }

                double residual =
                    relinquishedDoctrine + releasedHighEquipment - requiredLowEquipment;
                if (residual > 0d)
                {
                    low = middle;
                }
                else
                {
                    high = middle;
                }
            }

            technology = Math.Max(lowTech, Math.Min(highTech, (low + high) * 0.5d));
            return IsFinite(technology);
        }

        public static double Upkeep(double technology, bool atHome, double homeDivisor, double awayDivisor)
        {
            double divisor = atHome ? homeDivisor : awayDivisor;
            if (!IsFinite(technology) || !IsFinite(divisor) || technology < 0d || divisor <= 0d)
            {
                return double.NaN;
            }
            return technology / divisor;
        }

        public static double RepairCharge(
            double technology,
            double actualHealing,
            double repairShare,
            double armyCostCoefficient,
            double armyCostGrowthBase)
        {
            double cost = ArmyCost(technology, armyCostCoefficient, armyCostGrowthBase);
            if (!IsFinite(cost) || !IsFinite(actualHealing) || !IsFinite(repairShare) ||
                actualHealing < 0d || repairShare < 0d)
            {
                return double.NaN;
            }
            return repairShare * cost * actualHealing;
        }

        public static bool TryApplyBuildArmyProgress(
            double currentProgress,
            double change,
            bool multiply,
            out double progress)
        {
            progress = currentProgress;
            if (!IsFinite(currentProgress) || !IsFinite(change))
            {
                return false;
            }

            progress = multiply ? currentProgress * change : currentProgress + change;
            return IsFinite(progress);
        }

        private static double ExponentialIntegralE1(double value)
        {
            if (!IsFinite(value) || value <= 0d)
            {
                return double.NaN;
            }

            if (value <= 1d)
            {
                double sum = 0d;
                double term = 1d;
                for (int index = 1; index <= 100; index++)
                {
                    term *= -value / index;
                    double addition = term / index;
                    sum += addition;
                    if (Math.Abs(addition) <= Math.Abs(sum) * 1e-16d)
                    {
                        break;
                    }
                }
                return -EulerMascheroni - Math.Log(value) - sum;
            }

            const double minimum = 1e-300d;
            double b = value + 1d;
            double c = 1d / minimum;
            double d = 1d / b;
            double result = d;
            for (int index = 1; index <= 100; index++)
            {
                double a = -index * (double)index;
                b += 2d;
                d = a * d + b;
                if (Math.Abs(d) < minimum)
                {
                    d = minimum;
                }
                c = b + a / c;
                if (Math.Abs(c) < minimum)
                {
                    c = minimum;
                }
                d = 1d / d;
                double delta = d * c;
                result *= delta;
                if (Math.Abs(delta - 1d) <= 1e-15d)
                {
                    break;
                }
            }
            return result * Math.Exp(-value);
        }

        public static bool ResolveDestroyArmies(
            bool callerRequestedDestruction,
            bool peacefulUnification,
            bool absorption,
            bool transferToAlienNation)
        {
            if (peacefulUnification || transferToAlienNation)
            {
                return false;
            }
            if (absorption)
            {
                return true;
            }
            return callerRequestedDestruction;
        }
    }
}
