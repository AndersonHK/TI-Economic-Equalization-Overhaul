namespace TIEconomyMod.Core
{
    public static class CampaignDifficultyDefaults
    {
        public static bool EnableCombatRealism(int zeroBasedDifficulty)
        {
            // Cinematic is index 0. Normal, Veteran, and Brutal use the
            // non-cinematic scale and delta-V defaults.
            return zeroBasedDifficulty == 0;
        }
    }
}
