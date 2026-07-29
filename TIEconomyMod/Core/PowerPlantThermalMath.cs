using System;

namespace TIEconomyMod
{
    public static class PowerPlantThermalMath
    {
        public static float WasteHeatFromUsefulPower_GW(
            float usefulPower_GW, float efficiency)
        {
            if (usefulPower_GW <= 0f || efficiency >= 1f)
            {
                return 0f;
            }

            // No shipped plant has zero efficiency. Keep a malformed template finite
            // so it cannot poison radiator and ship-mass calculations with infinity.
            float boundedEfficiency = Math.Max(efficiency, 0.0001f);
            return usefulPower_GW * (1f / boundedEfficiency - 1f);
        }

        public static float PlantWasteHeat_GW(
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float systemsAndWeaponsRequirement_GW,
            float efficiency,
            float openCycleDriveHeatFraction)
        {
            float boundedOpenCycleFraction = Math.Max(
                0f, Math.Min(1f, openCycleDriveHeatFraction));
            float driveHeatFraction = openCycleDriveCooling
                ? boundedOpenCycleFraction
                : 1f;
            float usefulPower_GW = systemsAndWeaponsRequirement_GW +
                drivePowerRequirement_GW * driveHeatFraction;

            return WasteHeatFromUsefulPower_GW(usefulPower_GW, efficiency);
        }
    }
}
