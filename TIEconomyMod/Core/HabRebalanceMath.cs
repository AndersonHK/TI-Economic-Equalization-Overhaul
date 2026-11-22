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

        internal static void CollinearFreightLeg(
            double mu, double innerRadius_m, double hillRadius_m, double bodyPeriod_s,
            out double deltaV_kps, out float time_days)
        {
            double axis = (innerRadius_m + hillRadius_m) * 0.5d;
            double innerTransferSpeed = Math.Sqrt(mu * (2d / innerRadius_m - 1d / axis));
            double outerTransferSpeed = Math.Sqrt(mu * (2d / hillRadius_m - 1d / axis));
            double haloSpeed = 2d * Math.PI * hillRadius_m / bodyPeriod_s;
            deltaV_kps = (Math.Abs(innerTransferSpeed - Math.Sqrt(mu / innerRadius_m)) +
                Math.Abs(outerTransferSpeed - haloSpeed)) / 1000d;
            time_days = (float)(Math.PI * Math.Sqrt(axis * axis * axis / mu) / 86400d);
        }

        internal static bool PreferFreightLeg(float days, double deltaV, float currentDays, double currentDeltaV)
        {
            return !float.IsNaN(days) && !float.IsInfinity(days) && days >= 0f &&
                !double.IsNaN(deltaV) && !double.IsInfinity(deltaV) && deltaV >= 0d &&
                (days < currentDays || (days == currentDays && deltaV < currentDeltaV));
        }

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

        internal static int SectorUnlockTier(bool isStation, int sectorIndex)
        {
            if (sectorIndex == 0)
            {
                return 1;
            }
            if (sectorIndex < 1 || sectorIndex > 4)
            {
                return int.MaxValue;
            }
            return isStation
                ? (sectorIndex == 2 || sectorIndex == 4 ? 1 : 2)
                : (sectorIndex <= 2 ? 1 : 2);
        }
    }
}
