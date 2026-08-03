using System;

namespace TIEconomyMod
{
    public static class ProjectileCollisionMath
    {
        public const float MovementSweepMultiplier = 1f;
        public const float DurabilityMassPerDamagePoint_kg = 100f;
        public const float MagneticProjectileDensity_kgm3 = 19300f;
        public const float MagneticProjectileLengthToDiameter = 10f;

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

        public static float MagneticProjectileDiameter_mm(
            float completeProjectileMass_kg)
        {
            float mass_kg = Math.Max(0f, completeProjectileMass_kg);
            if (mass_kg <= 0f)
            {
                return 0f;
            }

            double diameter_m = Math.Pow(
                4.0 * mass_kg /
                    (Math.PI * MagneticProjectileDensity_kgm3 *
                        MagneticProjectileLengthToDiameter),
                1.0 / 3.0);
            return (float)(diameter_m * 1000.0);
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
