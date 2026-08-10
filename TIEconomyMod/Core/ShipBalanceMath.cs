using System;

namespace TIEconomyMod
{
    public static class ShipBalanceMath
    {
        public static float CrewMass_tons(
            int crewBillets, float massPerCrew_tons)
        {
            return Math.Max(0, crewBillets) *
                Math.Max(0f, massPerCrew_tons);
        }

        public static float HumanHullDriveScale(
            string hullDataName, bool alien)
        {
            if (alien || string.IsNullOrEmpty(hullDataName))
            {
                return 1f;
            }

            switch (hullDataName)
            {
                case "Cruiser":
                    return 1.3f;
                case "Battlecruiser":
                    return 1.5f;
                case "Lancer":
                    return 1.72f;
                case "Battleship":
                    return 1.75f;
                case "Dreadnought":
                    return 2f;
                case "Titan":
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        public static float ScaledDriveValue(float baseValue, float scale)
        {
            return Math.Max(0f, baseValue) * Math.Max(1f, scale);
        }

        public static float AdditionalScaledDriveValue(
            float baseValue, float scale)
        {
            return Math.Max(0f, baseValue) *
                (Math.Max(1f, scale) - 1f);
        }
    }
}
