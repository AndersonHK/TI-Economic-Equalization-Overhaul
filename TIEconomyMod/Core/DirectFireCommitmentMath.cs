using System;

namespace TIEconomyMod
{
    public static class DirectFireCommitmentMath
    {
        public static float EstimatedKillThreshold_points(
            int structuralIntegrity, float totalArmorValue)
        {
            float integrity = Math.Max(0, structuralIntegrity);
            float armor = Math.Max(0f, totalArmorValue);
            return integrity * 6f * (1f + armor / 20f);
        }

        public static float SanitizeExpectedDamage_points(float damage)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage))
            {
                return 0f;
            }

            return Math.Max(0f, damage);
        }

        public static bool IsSaturated(
            float committedDamage_points, float killThreshold_points)
        {
            return SanitizeExpectedDamage_points(committedDamage_points) >
                Math.Max(0f, killThreshold_points);
        }
    }
}
