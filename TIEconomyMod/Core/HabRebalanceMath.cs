using System;

namespace TIEconomyMod
{
    internal static class HabRebalanceMath
    {
        internal const float MinimumRebalancedMaterialFraction = 0.5f;
        internal const float MaximumRebalancedMaterialFraction = 0.75f;
        internal const float FractionTolerance = 0.0001f;
        internal const float UpgradeRate = 2f / 3f;
        internal const float MandatoryTransportFraction = 1f / 3f;
        internal const float EarthLogisticsPairPriority = 45f;
        internal const float OtherLogisticsPairPriority = 34f;
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

        internal static float FullMaterialMass(
            float destinationAdjustedMass,
            float rateMultiplier)
        {
            if (destinationAdjustedMass <= 0f || rateMultiplier <= 0f)
            {
                return 0f;
            }

            return RoundCost(destinationAdjustedMass * rateMultiplier);
        }

        internal static float MandatoryTransportMass(
            float materialMass,
            float earthDeliveredMass)
        {
            if (materialMass <= 0f)
            {
                return 0f;
            }

            return RoundCost(Math.Max(
                0f,
                materialMass * MandatoryTransportFraction -
                Math.Max(0f, earthDeliveredMass)));
        }

        internal static float EarthFallbackMass(
            float materialMass,
            float earthShortfallMass)
        {
            if (materialMass <= 0f)
            {
                return 0f;
            }

            return RoundCost(Math.Max(
                Math.Max(0f, earthShortfallMass),
                materialMass * MandatoryTransportFraction));
        }

        internal static float PropellantMass(
            float payloadMass,
            double deltaV_kps,
            double exhaustVelocity_kps)
        {
            if (payloadMass <= 0f ||
                deltaV_kps <= 0d ||
                exhaustVelocity_kps <= 0d)
            {
                return 0f;
            }

            return RoundCost((float)(
                payloadMass *
                (Math.Exp(deltaV_kps / exhaustVelocity_kps) - 1d)));
        }

        internal static int EffectiveExportTier(int factoryTier, int dockTier)
        {
            if (factoryTier <= 0 || dockTier <= 0)
            {
                return 0;
            }

            return Math.Min(factoryTier, dockTier);
        }

        internal static float LogisticsPairPriority(
            bool systemHasCommittedPair,
            bool candidateCompletesPair,
            bool earthSystem,
            float preferenceWeight)
        {
            if (systemHasCommittedPair ||
                !candidateCompletesPair ||
                preferenceWeight <= 0f)
            {
                return 0f;
            }

            return (earthSystem
                ? EarthLogisticsPairPriority
                : OtherLogisticsPairPriority) * preferenceWeight;
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
