using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System.Linq;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIFactionState),
        nameof(TIFactionState.GetMissionControlRequirementFromMineNetwork))]
    public static class MineNetworkMissionControlPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIFactionState __instance, ref int __result)
        {
            __result = MineMissionControlMath.NetworkCost(
                __instance.habs
                    .Where(hab => hab != null && hab.HasActiveMine &&
                        hab.mine != null && hab.mine.moduleTemplate != null)
                    .Select(hab => hab.mine.moduleTemplate.tier));
            return false;
        }
    }

    [HarmonyPatch(typeof(TIFactionState),
        nameof(TIFactionState.GetMissionControlRequirementFromNextMine))]
    public static class NextMineMissionControlPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int __result)
        {
            // New mines begin at Tier 1, while each mine upgrade advances one
            // tier. Both operations therefore add exactly one MC.
            __result = MineMissionControlMath.TierCost(1);
            return false;
        }
    }

    [HarmonyPatch(typeof(TIFactionState),
        nameof(TIFactionState.GetMissionControlGainedFromTurningOffMine))]
    public static class DisabledMineMissionControlPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TIHabModuleState mine, ref int __result)
        {
            __result = mine != null && mine.active && mine.moduleTemplate != null
                ? MineMissionControlMath.TierCost(mine.moduleTemplate.tier)
                : 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIFactionState),
        "SafeMineNextworkSize", MethodType.Getter)]
    public static class FreeMineAllowancePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int __result)
        {
            // Ignore serialized legacy MCFreeSpaceMineNetwork effects as well
            // as current template values. The tier sum has no free allowance.
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(GeneralControlsController),
        nameof(GeneralControlsController.ResourceReportString))]
    public static class MissionControlUsageColorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TIFactionState faction,
            FactionResource resourceType, ref string __result)
        {
            if (resourceType != FactionResource.MissionControl || faction == null)
            {
                return;
            }

            float capacity = faction.GetDailyIncome(resourceType);
            float usage = faction.GetMissionControlUsage();
            string displayedUsage = usage.ToString("N0");

            switch (MineMissionControlMath.UsageDisplayState(usage, capacity))
            {
                case MissionControlUsageDisplayState.Warning:
                    displayedUsage = "<color=#EC9933>" + displayedUsage +
                        "</color>";
                    break;
                case MissionControlUsageDisplayState.OverCapacity:
                    displayedUsage = "<color=#B26A60>" + displayedUsage +
                        "</color>";
                    break;
            }

            __result = Loc.T("UI.GeneralControls.ResourcesUsage",
                displayedUsage, capacity.ToString("N0"));
        }
    }
}
