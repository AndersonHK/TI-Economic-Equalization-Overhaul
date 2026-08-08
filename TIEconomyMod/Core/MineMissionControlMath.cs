using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    internal enum MissionControlUsageDisplayState
    {
        Normal,
        Warning,
        OverCapacity
    }

    internal static class MineMissionControlMath
    {
        internal static int TierCost(int tier)
        {
            return Math.Max(0, tier);
        }

        internal static int NetworkCost(IEnumerable<int> activeMineTiers)
        {
            if (activeMineTiers == null)
            {
                return 0;
            }

            int total = 0;
            foreach (int tier in activeMineTiers)
            {
                total += TierCost(tier);
            }
            return total;
        }

        internal static MissionControlUsageDisplayState UsageDisplayState(
            float usage, float capacity)
        {
            if (usage > capacity)
            {
                return MissionControlUsageDisplayState.OverCapacity;
            }

            return capacity > 0f && usage > capacity * 0.75f
                ? MissionControlUsageDisplayState.Warning
                : MissionControlUsageDisplayState.Normal;
        }
    }
}
