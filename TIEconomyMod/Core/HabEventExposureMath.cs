using System;

namespace TIEconomyMod
{
    internal static class HabEventExposureMath
    {
        internal const int NativeFrequencyOrbitalHabCount = 30;
        internal const string MeteorStrikeEvent = "event_MeteorStrike";
        internal const string HabAccidentEvent = "event_HabAccident";

        internal static bool UsesOrbitalHabExposure(string eventName)
        {
            return string.Equals(
                       eventName,
                       MeteorStrikeEvent,
                       StringComparison.Ordinal) ||
                   string.Equals(
                       eventName,
                       HabAccidentEvent,
                       StringComparison.Ordinal);
        }

        internal static float ExposureMultiplier(int orbitalHabCount)
        {
            int nonnegativeCount = Math.Max(0, orbitalHabCount);
            return Math.Min(
                1f,
                nonnegativeCount / (float)NativeFrequencyOrbitalHabCount);
        }

        internal static float AdjustSelectionWeight(
            string eventName,
            float nativeWeight,
            int orbitalHabCount)
        {
            return UsesOrbitalHabExposure(eventName)
                ? nativeWeight * ExposureMultiplier(orbitalHabCount)
                : nativeWeight;
        }
    }
}
