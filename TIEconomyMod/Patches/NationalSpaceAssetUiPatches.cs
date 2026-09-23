using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    internal static class NationalSpaceAssetUi
    {
        internal static bool ShowCap(TINationState nation)
        {
            return Main.FeatureEnabled(Main.settings.ui.enabled) &&
                Main.settings.ui.expandedTooltips && NationalSpaceAssets.CapEnabled(nation);
        }
    }

    [HarmonyPatch(typeof(NationInfoController), "UpdatePrimaryDisplayElements")]
    public static class NationalBoostValuePatch
    {
        [HarmonyPostfix]
        public static void Postfix(NationInfoController __instance)
        {
            TINationState nation = __instance.nation;
            if (!NationalSpaceAssetUi.ShowCap(nation) || __instance.boostNationValue == null)
                return;

            string current = TIUtilities.FormatBigOrSmallNumber(nation.currentBoost_month, 1, 2);
            string original = __instance.boostNationValue.text;
            // Replace only the current-production prefix; keep the native trend sprite.
            if (original == null || !original.StartsWith(current, StringComparison.Ordinal)) return;
            string maximum = TIUtilities.FormatBigOrSmallNumber((float)NationalSpaceAssets.CapMonth(nation), 1, 2);
            __instance.boostNationValue.SetText(Loc.T("UI.Nation.MissionControlValue", current, maximum) +
                original.Substring(current.Length));
        }
    }

    [HarmonyPatch(typeof(NationInfoController), nameof(NationInfoController.BuildBoostTooltip))]
    public static class NationalBoostTooltipPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState nation, ref string __result)
        {
            if (!NationalSpaceAssetUi.ShowCap(nation) || __result == null) return;
            string original = Loc.T("UI.Nation.Boost");
            // Keep the native localized 31-day change, including its color and units.
            if (__result.StartsWith(original, StringComparison.Ordinal))
                __result = Loc.T("UI.Nation.EEO.Boost") + __result.Substring(original.Length);
        }
    }
}
