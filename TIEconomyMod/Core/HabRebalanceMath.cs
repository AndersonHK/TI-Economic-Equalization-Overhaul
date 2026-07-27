using System;

namespace TIEconomyMod
{
    internal static class HabRebalanceMath
    {
        internal const float TargetMaterialFraction = 2f / 3f;
        internal const float FractionTolerance = 0.0001f;
        internal const float UpgradeRate = 2f / 3f;

        internal static bool HasRebalancedMaterialFraction(float materialFraction)
        {
            return Math.Abs(materialFraction - TargetMaterialFraction) <= FractionTolerance;
        }

        internal static float MandatoryEarthMass(
            float destinationAdjustedMass,
            float materialFraction,
            float rateMultiplier)
        {
            if (destinationAdjustedMass <= 0f || rateMultiplier <= 0f)
            {
                return 0f;
            }

            float missingFraction = Math.Max(0f, 1f - materialFraction);
            return destinationAdjustedMass * missingFraction * rateMultiplier;
        }

        internal static float ConstructionRate(bool isUpgrade)
        {
            return isUpgrade ? UpgradeRate : 1f;
        }

        internal static bool NeedsEarthTransferDelay(float existingBoost)
        {
            return existingBoost <= FractionTolerance;
        }
    }
}
