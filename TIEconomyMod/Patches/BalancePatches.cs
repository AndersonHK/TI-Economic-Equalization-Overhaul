using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIMegafaunaArmyState), "techLevel", MethodType.Getter)]
    public static class XenofaunaStrengthPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, TIMegafaunaArmyState __instance)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.megafaunaEnabled))
            {
                return;
            }

            // Vanilla xenofauna begins at 2, gains 1 tech per 100 abductions, and
            // stops at 6 plus any bonusTechLevel earned after faction control. This
            // changes only that natural ceiling to 5: a vanilla 6.0 becomes 5.0,
            // while a controlled army with +0.4 bonus may still reach 5.4.
            __result = Math.Min(__result,
                settings.megafaunaMaximumTechLevel + __instance.bonusTechLevel);
        }
    }

    [HarmonyPatch(typeof(TITechTemplate), "GetResearchCost")]
    public static class GlobalTechnologyResearchCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            TechnologySettings settings = Main.settings.technology;
            if (!Main.FeatureEnabled(settings.researchCostEnabled))
            {
                return;
            }

            // This method exists only on global technologies; faction projects use
            // TIProjectTemplate and are untouched. The default x1.20 changes a 1,000
            // research technology to 1,200 after vanilla applies difficulty and speed.
            __result *= settings.researchCostMultiplier;
        }
    }
}
