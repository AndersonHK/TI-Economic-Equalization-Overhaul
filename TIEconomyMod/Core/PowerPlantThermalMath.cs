using System;

namespace TIEconomyMod
{
    public static class PowerPlantThermalMath
    {
        private const float MinimumEfficiency = 0.0001f;

        public static float WasteHeatFromUsefulPower_GW(
            float usefulPower_GW, float efficiency)
        {
            if (!IsFinite(usefulPower_GW) || usefulPower_GW <= 0f)
            {
                return 0f;
            }

            // No shipped plant has zero efficiency. Keep a malformed template finite
            // so it cannot poison radiator and ship-mass calculations with infinity.
            float boundedEfficiency = BoundedEfficiency(efficiency);
            if (boundedEfficiency >= 1f)
            {
                return 0f;
            }

            return usefulPower_GW * (1f / boundedEfficiency - 1f);
        }

        public static float OpenCycleEffectiveCoupling(
            float efficiency, float retainedHeatFraction)
        {
            float boundedEfficiency = BoundedEfficiency(efficiency);
            float boundedFraction = BoundedFraction(retainedHeatFraction);
            return Math.Max(
                MinimumEfficiency,
                1f - boundedFraction * (1f - boundedEfficiency));
        }

        public static float OpenCycleReactorOutput_GW(
            float usefulDrivePower_GW,
            float efficiency,
            float retainedHeatFraction)
        {
            if (!IsFinite(usefulDrivePower_GW) || usefulDrivePower_GW <= 0f)
            {
                return 0f;
            }

            float output_GW = usefulDrivePower_GW /
                OpenCycleEffectiveCoupling(efficiency, retainedHeatFraction);
            return IsFinite(output_GW) ? output_GW : usefulDrivePower_GW;
        }

        public static float OpenCycleResidualHeat_GW(
            float reactorOutput_GW,
            float efficiency,
            float retainedHeatFraction)
        {
            if (!IsFinite(reactorOutput_GW) || reactorOutput_GW <= 0f)
            {
                return 0f;
            }

            float heat_GW = reactorOutput_GW *
                BoundedFraction(retainedHeatFraction) *
                (1f - BoundedEfficiency(efficiency));
            return IsFinite(heat_GW) ? heat_GW : 0f;
        }

        public static float PlantWasteHeat_GW(
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float systemsAndWeaponsRequirement_GW,
            float efficiency,
            float openCycleDriveHeatFraction)
        {
            float systemsAndWeaponsHeat_GW = WasteHeatFromUsefulPower_GW(
                systemsAndWeaponsRequirement_GW, efficiency);
            float driveHeat_GW = openCycleDriveCooling
                ? OpenCycleResidualHeat_GW(
                    drivePowerRequirement_GW,
                    efficiency,
                    openCycleDriveHeatFraction)
                : WasteHeatFromUsefulPower_GW(
                    drivePowerRequirement_GW, efficiency);

            return systemsAndWeaponsHeat_GW + driveHeat_GW;
        }

        private static float BoundedEfficiency(float efficiency)
        {
            if (!IsFinite(efficiency))
            {
                return 1f;
            }

            return Math.Max(MinimumEfficiency, Math.Min(1f, efficiency));
        }

        private static float BoundedFraction(float fraction)
        {
            if (!IsFinite(fraction))
            {
                return 0f;
            }

            return Math.Max(0f, Math.Min(1f, fraction));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
