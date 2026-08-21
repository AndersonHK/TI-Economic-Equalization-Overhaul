using System;

namespace TIEconomyMod.Core
{
    public struct NationalHarmonizationResult
    {
        public bool valid;
        public double governmentDifference;
        public double inequalityDifference;
        public double knowledgeDifference;
        public double perCapitaGdpRatio;
        public double core;
        public double modifier;
        public double score;
    }

    public static class NationalHarmonizationMath
    {
        public static NationalHarmonizationResult Calculate(
            double sourceGovernment,
            double sourceInequality,
            double sourceKnowledge,
            double sourcePerCapitaGdp,
            double sourceCohesion,
            double targetGovernment,
            double targetInequality,
            double targetKnowledge,
            double targetPerCapitaGdp,
            double targetUnrest)
        {
            NationalHarmonizationResult result = new NationalHarmonizationResult
            {
                valid = false,
                score = double.PositiveInfinity
            };

            if (!IsFinite(sourceGovernment) || !IsFinite(sourceInequality) ||
                !IsFinite(sourceKnowledge) || !IsFinite(sourcePerCapitaGdp) ||
                !IsFinite(sourceCohesion) || !IsFinite(targetGovernment) ||
                !IsFinite(targetInequality) || !IsFinite(targetKnowledge) ||
                !IsFinite(targetPerCapitaGdp) || !IsFinite(targetUnrest) ||
                sourcePerCapitaGdp <= 0d || targetPerCapitaGdp <= 0d)
            {
                return result;
            }

            result.governmentDifference = Math.Abs(
                sourceGovernment - targetGovernment);
            result.inequalityDifference = Math.Abs(
                sourceInequality - targetInequality);
            result.knowledgeDifference = Math.Abs(
                sourceKnowledge - targetKnowledge);
            result.perCapitaGdpRatio = Math.Max(
                sourcePerCapitaGdp / targetPerCapitaGdp,
                targetPerCapitaGdp / sourcePerCapitaGdp);
            result.core = result.governmentDifference +
                result.inequalityDifference + result.knowledgeDifference +
                result.perCapitaGdpRatio;

            double boundedTargetUnrest = ClampTen(targetUnrest);
            double boundedSourceCohesion = ClampTen(sourceCohesion);
            result.modifier = (10d - boundedTargetUnrest) / 10d +
                (10d - boundedSourceCohesion) / 10d;
            result.score = result.modifier * result.core;
            result.valid = IsFinite(result.core) && IsFinite(result.modifier) &&
                IsFinite(result.score);
            if (!result.valid)
            {
                result.score = double.PositiveInfinity;
            }
            return result;
        }

        public static bool Passes(NationalHarmonizationResult result,
            double inclusiveThreshold)
        {
            return result.valid && IsFinite(inclusiveThreshold) &&
                inclusiveThreshold >= 0d && result.score <= inclusiveThreshold;
        }

        private static double ClampTen(double value)
        {
            return Math.Max(0d, Math.Min(10d, value));
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
