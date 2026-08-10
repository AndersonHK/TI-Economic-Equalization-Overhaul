using System;

namespace TIEconomyMod.Core
{
    public static class GovernmentMath
    {
        public const float MinimumScore = 0f;
        public const float MidpointScore = 5f;
        public const float MaximumScore = 10f;

        public static float TransformChange(float rawChange, float government,
            float boundaryFactor)
        {
            if (rawChange == 0f || float.IsNaN(rawChange) ||
                float.IsInfinity(rawChange) || float.IsNaN(government) ||
                float.IsInfinity(government) || boundaryFactor < 1f ||
                float.IsNaN(boundaryFactor) || float.IsInfinity(boundaryFactor))
            {
                return rawChange;
            }

            float boundedGovernment = Math.Max(MinimumScore,
                Math.Min(MaximumScore, government));
            float direction = Math.Sign(rawChange);
            float exponent = direction *
                (1f - boundedGovernment / MidpointScore);
            double multiplier = Math.Pow(boundaryFactor, exponent);
            double transformed = rawChange * multiplier;
            if (double.IsNaN(transformed) || double.IsInfinity(transformed) ||
                transformed > float.MaxValue || transformed < -float.MaxValue)
            {
                return rawChange;
            }
            return (float)transformed;
        }
    }
}
