using System;

namespace TIEconomyMod.Core
{
    public static class CohesionRestMath
    {
        public static float InequalityImpact(float education, float inequality,
            float educationBaseMultiplier, float educationDivisor,
            float inequalityOffset, float inequalityCoefficient)
        {
            float educationMultiplier = Math.Min(1f,
                educationBaseMultiplier + education / educationDivisor);
            return educationMultiplier *
                (inequalityOffset - inequalityCoefficient * inequality);
        }

        public static float ScalePublicEliteImpact(float vanillaImpact,
            float government, float governmentDivisor)
        {
            float governmentMultiplier = Math.Max(0f,
                Math.Min(1f, government / governmentDivisor));
            return vanillaImpact * governmentMultiplier;
        }

        public static float AutocracyImpact(float government, float unrest,
            float boundary, float exponent)
        {
            if (government >= boundary)
            {
                return 0f;
            }

            return (float)((Math.Pow(boundary, exponent) -
                Math.Pow(government, exponent)) * (10f - unrest) / 10f);
        }

        public static float AnocracyImpact(float government, float boundary,
            float upperBoundary)
        {
            if (government < boundary || government > upperBoundary)
            {
                return 0f;
            }

            return 3f * Math.Abs(5f - government) - 2f;
        }

        public static float DemocracyImpact(float government,
            float originalValue, float target, float threshold,
            float coefficient)
        {
            if (government < threshold)
            {
                return 0f;
            }

            float magnitude = coefficient * (government - threshold);
            float targetDelta = target - originalValue;
            float directedImpact = Math.Sign(targetDelta) * magnitude;
            return Math.Max(Math.Min(0f, targetDelta), Math.Min(
                Math.Max(0f, targetDelta), directedImpact));
        }
    }
}
