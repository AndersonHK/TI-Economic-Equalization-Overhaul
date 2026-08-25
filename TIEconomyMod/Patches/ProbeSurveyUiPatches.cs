using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(
        typeof(IntelSpaceBodyListItemController),
        nameof(IntelSpaceBodyListItemController.OnClickProspectButton))]
    internal static class IntelProbeSiteSelectionPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            IntelSpaceBodyListItemController __instance,
            IntelScreenController ___intelController)
        {
            TIFactionState faction = ___intelController == null
                ? null
                : ___intelController.activePlayer;
            if (ProbeSurveyRuntime.EligibleSites(
                faction,
                __instance.spaceBody).Count == 0)
            {
                return false;
            }

            ___intelController.Close();
            TIUtilities.GotoGameState(__instance.spaceBody);
            OperationCanvasController.Singleton.OnOperationSelected(
                OperationsManager.operationsLookup[
                    typeof(LaunchProbeOperation)].GetTemplate());
            return false;
        }
    }

    [HarmonyPatch(
        typeof(HabSiteController),
        nameof(HabSiteController.BuildOutputString))]
    internal static class SurveyedSiteOutputPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIHabSiteState site,
            ref string __result)
        {
            TIFactionState faction = GameControl.control.activePlayer;
            if (!ProbeSurveyRuntime.SiteProspected(faction, site) ||
                ProbeSurveyRuntime.BodyProspected(faction, site.parentBody))
            {
                return true;
            }

            TIHabModuleState mine = site.hab == null
                ? null
                : site.hab.mine;
            if (site.hab != null &&
                site.hab.faction == faction &&
                mine != null &&
                mine.active)
            {
                return true;
            }

            StringBuilder output = new StringBuilder();
            AppendResource(
                output,
                site,
                FactionResource.Water,
                TemplateManager.global.waterInlineSpritePath,
                false);
            AppendResource(
                output,
                site,
                FactionResource.Volatiles,
                TemplateManager.global.volatilesInlineSpritePath,
                false);
            AppendResource(
                output,
                site,
                FactionResource.Metals,
                TemplateManager.global.metalsInlineSpritePath,
                false);
            AppendResource(
                output,
                site,
                FactionResource.NobleMetals,
                TemplateManager.global.noblesInlineSpritePath,
                false);
            AppendResource(
                output,
                site,
                FactionResource.Fissiles,
                TemplateManager.global.fissilesInlineSpritePath,
                true);
            __result = output.ToString().TrimEnd();
            return false;
        }

        private static void AppendResource(
            StringBuilder output,
            TIHabSiteState site,
            FactionResource resource,
            string icon,
            bool final)
        {
            if (site.GetMonthlyProduction(resource) <= 0f)
            {
                return;
            }

            output.Append(icon).Append(TIUtilities.FormatSmallNumber(
                site.GetMonthlyProduction(resource),
                7,
                0,
                false));
            if (!final)
            {
                output.Append(" ");
            }
        }
    }

    [HarmonyPatch(
        typeof(HabSiteController),
        nameof(HabSiteController.SetMarkerData))]
    internal static class SurveyedSiteMarkerPatch
    {
        private static readonly FieldInfo ProspectedHabSiteIcon =
            AccessTools.Field(
                typeof(AssetCacheManager),
                "prospectedHabSiteIcon");

        [HarmonyPostfix]
        internal static void Postfix(HabSiteController __instance)
        {
            TIHabSiteState site = __instance.site;
            TIFactionState faction = GameControl.control.activePlayer;
            if (site == null ||
                faction == null ||
                !ProbeSurveyRuntime.SiteProspected(faction, site) ||
                (site.hasPlannedOrOperatingBase &&
                 faction.HasIntelOnSpaceAssetLocation(site.hab)) ||
                ProspectedHabSiteIcon == null)
            {
                return;
            }

            Sprite icon = ProspectedHabSiteIcon.GetValue(null) as Sprite;
            if (icon != null)
            {
                __instance.habSiteMarker.sprite = icon;
            }
        }
    }

    [HarmonyPatch(
        typeof(IntelScreenController),
        nameof(IntelScreenController.SetHabSiteListModelData))]
    internal static class SurveyedIntelSiteModelsPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(IntelScreenController __instance)
        {
            __instance.habSiteModels.Clear();
            foreach (TIHabSiteState site in
                ProbeSurveyRuntime.SurveyedSites(__instance.activePlayer))
            {
                IntelScreenHabSiteListItem_Data data =
                    new IntelScreenHabSiteListItem_Data
                    {
                        controller = __instance,
                        showInList = true
                    };
                data.SetData(site);
                __instance.habSiteModels.Add(
                    new IntelScreenHabSiteListItemModel
                    {
                        IntelScreenHabSiteListItemData = data
                    });
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(IntelHabSiteListPane), "ItemsToDisplay")]
    internal static class SurveyedIntelSiteItemsPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ref IEnumerable<TIHabSiteState> __result)
        {
            __result = ProbeSurveyRuntime.SurveyedSites(
                GameControl.control.activePlayer);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(IntelScreenController),
        nameof(IntelScreenController.UpdateActivePlayerUIElements))]
    internal static class SurveyedIntelSiteTabActivePlayerPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(IntelScreenController __instance)
        {
            bool visible = ProbeSurveyRuntime.HasSurveyedSite(
                __instance.activePlayer);
            __instance.habSiteTab.gameObject.SetActive(visible);
        }
    }

    [HarmonyPatch(
        typeof(IntelScreenController),
        nameof(IntelScreenController.RefreshAll))]
    internal static class SurveyedIntelSiteTabRefreshPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(IntelScreenController __instance)
        {
            bool visible = ProbeSurveyRuntime.HasSurveyedSite(
                __instance.activePlayer);
            __instance.habSiteTab.gameObject.SetActive(visible);
            __instance.sitesTabButtonObject.SetActive(visible);
        }
    }
}
