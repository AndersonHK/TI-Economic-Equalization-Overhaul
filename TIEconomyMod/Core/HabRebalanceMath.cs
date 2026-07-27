using System;

namespace TIEconomyMod
{
    internal static class HabRebalanceMath
    {
        internal const float MinimumRebalancedMaterialFraction = 0.5f;
        internal const float MaximumRebalancedMaterialFraction = 0.75f;
        internal const float FractionTolerance = 0.0001f;
        internal const float UpgradeRate = 2f / 3f;
        internal const int CostDecimalPlaces = 4;

        internal static bool HasRebalancedMaterialFraction(float materialFraction)
        {
            return materialFraction >=
                    MinimumRebalancedMaterialFraction - FractionTolerance &&
                materialFraction <=
                    MaximumRebalancedMaterialFraction + FractionTolerance;
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

            return RoundCost(
                destinationAdjustedMass *
                Math.Max(0f, 1f - materialFraction) *
                rateMultiplier);
        }

        internal static float OrdinaryMaterialMass(
            float destinationAdjustedMass,
            float materialFraction,
            float rateMultiplier)
        {
            if (destinationAdjustedMass <= 0f || rateMultiplier <= 0f)
            {
                return 0f;
            }

            return RoundCost(
                destinationAdjustedMass *
                Math.Max(0f, materialFraction) *
                rateMultiplier);
        }

        internal static float NormalizeMaterialCost(
            float materialWeight,
            float materialWeightSum,
            float ordinaryMaterialCost)
        {
            if (materialWeight <= 0f ||
                materialWeightSum <= 0f ||
                ordinaryMaterialCost <= 0f)
            {
                return 0f;
            }

            return RoundCost(
                materialWeight / materialWeightSum * ordinaryMaterialCost);
        }

        internal static float RoundCost(float value)
        {
            return (float)Math.Round(
                value,
                CostDecimalPlaces,
                MidpointRounding.AwayFromZero);
        }

        internal static float ConstructionRate(bool isUpgrade)
        {
            return isUpgrade ? UpgradeRate : 1f;
        }

        internal static bool HasEarthDelivery(float boost)
        {
            return boost > FractionTolerance;
        }

        internal static int ConnectorTierRequirement(
            int habTier,
            bool isStation,
            bool isAlien,
            bool sectorActive)
        {
            return habTier == 1 &&
                isStation &&
                !isAlien &&
                sectorActive
                ? 1
                : 2;
        }
    }
}
