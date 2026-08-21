using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(StartMenuController), "ResetCampaignDifficultyOptions")]
    public static class CampaignDifficultyRealismDefaultsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(StartMenuController __instance)
        {
            if (!Main.FeatureEnabled(true))
            {
                return;
            }

            bool enabled = CampaignDifficultyDefaults.EnableCombatRealism(
                __instance.selectDifficultyDropdown.value);
            __instance.realismCombatScaleToggle.SetIsOnWithoutNotify(enabled);
            __instance.realismCombatDVMovementToggle.SetIsOnWithoutNotify(enabled);
        }
    }
}
