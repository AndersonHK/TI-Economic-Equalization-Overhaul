namespace TIEconomyMod.Core
{
    public static class BattleScaleMath
    {
        // Combat positions/velocities remain 0.05 units/km in both modes.
        public const float UnitsPerMetre = 0.00005f;
        public const float DefaultMultiplier = 10f;
        public const float MinimumMultiplier = 0.01f;
        public const float MaximumMultiplier = 1000f;
        public static float ValidMultiplier(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= MinimumMultiplier && value <= MaximumMultiplier
                ? value : DefaultMultiplier;
        }
        public static float ModelScale(float multiplier)
        {
            return UnitsPerMetre * ValidMultiplier(multiplier);
        }
        public static float ProjectileScale(float multiplier)
        {
            return UnitsPerMetre * ValidMultiplier(multiplier);
        }
    }
}
