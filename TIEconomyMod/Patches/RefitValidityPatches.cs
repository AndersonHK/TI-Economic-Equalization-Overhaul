using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Text;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(
        typeof(TISpaceShipTemplate),
        nameof(TISpaceShipTemplate.IsAValidRefitFor))]
    public static class ShipAppearanceRefitValidityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate __instance,
            TISpaceShipTemplate oldShipTemplate,
            bool getReason,
            ref string reason,
            ref bool __result)
        {
            if (!__result || !Main.FeatureEnabled(true) ||
                oldShipTemplate == null ||
                __instance.GetHullAppearanceIndex ==
                    oldShipTemplate.GetHullAppearanceIndex)
            {
                return;
            }

            __result = false;
            if (getReason)
            {
                reason = new StringBuilder()
                    .Append(Environment.NewLine)
                    .Append(Environment.NewLine)
                    .Append(TIUtilities.RedLine(
                        Loc.T("UI.Fleets.RefitFailHullAppearance")))
                    .ToString();
            }
        }
    }
}
