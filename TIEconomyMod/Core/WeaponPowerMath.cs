using System;

namespace TIEconomyMod
{
    public static class WeaponPowerMath
    {
        private const float MinimumEfficiency = 0.0001f;

        public static float ElectricalInput_GJ(
            float usefulPower_MJ, float extraInput_MJ, float efficiency)
        {
            float usefulEnergy_MJ = Math.Max(0f, usefulPower_MJ + extraInput_MJ);
            if (usefulEnergy_MJ <= 0f)
            {
                return 0f;
            }

            return usefulEnergy_MJ /
                Math.Max(MinimumEfficiency, efficiency) / 1000f;
        }

        public static float ModuleWasteHeat_GJ(
            float electricalInput_GJ, float efficiency)
        {
            if (electricalInput_GJ <= 0f || efficiency >= 1f)
            {
                return 0f;
            }

            return electricalInput_GJ *
                (1f - Math.Max(0f, efficiency));
        }

        public static float DesignHeatRate_GW(
            float heatPerShot_GJ,
            int salvoShots,
            float cooldown_s,
            float intraSalvoCooldown_s)
        {
            if (heatPerShot_GJ <= 0f)
            {
                return 0f;
            }

            float designInterval_s = salvoShots == 1
                ? cooldown_s
                : intraSalvoCooldown_s;
            return designInterval_s > 0f
                ? heatPerShot_GJ / designInterval_s
                : 0f;
        }
    }
}
