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
    }
}
