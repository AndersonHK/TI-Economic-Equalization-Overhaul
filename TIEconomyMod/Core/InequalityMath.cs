using System;

namespace TIEconomyMod.Core
{
    public static class InequalityMath
    {
        public static float TransformPriorityChange(float rawDelta,
            float inequality, float minimum, float neutral, float maximum,
            float exponent, float maximumDirectionalMultiplier)
        {
            if (rawDelta == 0f)
            {
                return 0f;
            }

            float span = inequality < neutral
                ? neutral - minimum
                : maximum - neutral;
            if (span <= 0f)
            {
                return float.NaN;
            }

            float position = Math.Max(-1f, Math.Min(1f,
                (inequality - neutral) / span));
            float magnitude = (float)Math.Pow(Math.Abs(position), exponent);
            float direction = Math.Sign(rawDelta) * position;
            float multiplier = direction < 0f
                ? 1f + (maximumDirectionalMultiplier - 1f) * magnitude
                : 1f - magnitude;
            return rawDelta * multiplier;
        }
    }
}
