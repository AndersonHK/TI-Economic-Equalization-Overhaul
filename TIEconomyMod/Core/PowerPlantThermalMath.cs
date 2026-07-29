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
            float efficiency)
        {
            float usefulPower_GW = systemsAndWeaponsRequirement_GW;
            if (!openCycleDriveCooling)
            {
                usefulPower_GW += drivePowerRequirement_GW;
            }

            return WasteHeatFromUsefulPower_GW(usefulPower_GW, efficiency);
        }
    }
}
