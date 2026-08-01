using System;

namespace TIEconomyMod
{
    public static class ProjectileCollisionMath
    {
        public const float MovementSweepMultiplier = 1f;
        public const float DurabilityMassPerDamagePoint_kg = 100f;

        public static float CrossSectionalArea_m2(float diameter_mm)
        {
            float radius_m = Math.Max(0f, diameter_mm) / 2000f;
            return (float)Math.PI * radius_m * radius_m;
        }

        public static float WorldDiameter_gameUnits(
            float diameter_mm, float modelScalingFactor)
        {
            return Math.Max(0f, diameter_mm) / 1000f *
                Math.Max(0f, modelScalingFactor);
        }

        public static float MassDamage_kg(
            float directDamage_points, float chippingDamage_points)
        {
            return Math.Max(
                0f, directDamage_points + chippingDamage_points) *
                DurabilityMassPerDamagePoint_kg;
        }
    }
}
