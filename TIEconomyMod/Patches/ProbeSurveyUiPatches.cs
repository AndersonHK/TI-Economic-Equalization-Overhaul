using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    internal static class SurveyedSiteIconRuntime
    {
        private static readonly object InstallLock = new object();
        private static readonly FieldInfo ProspectedHabSiteIcon =
            AccessTools.Field(
                typeof(AssetCacheManager),
                "prospectedHabSiteIcon");
        private static bool installed;

        internal static void EnsureInstalled()
        {
            if (installed)
            {
                return;
            }

            lock (InstallLock)
            {
                if (installed)
                {
                    return;
                }

                try
                {
                    MethodInfo target = AccessTools.Method(
                        typeof(HabSiteController),
                        "GetEmptyHabSiteIcon");
                    MethodInfo prefix = AccessTools.Method(
                        typeof(SurveyedSiteIconRuntime),
                        nameof(Prefix));
                    if (target == null || prefix == null)
                    {
                        throw new MissingMethodException(
                            "The delayed empty-site icon hook is unavailable.");
                    }

                    new Harmony(Main.mod.Info.Id).Patch(
                        target,
                        prefix: new HarmonyMethod(prefix));
                    installed = true;
                }
                catch (Exception exception)
                {
                    Main.Warn(
                        "Could not install the delayed per-site marker hook: " +
                        exception);
                }
            }
        }

        private static bool Prefix(
            TIHabSiteState site,
            TIFactionState faction,
            ref Sprite __result)
        {
            if (!ProbeSurveyRuntime.SiteProspected(faction, site) ||
                ProspectedHabSiteIcon == null)
            {
                return true;
            }

            Sprite icon = ProspectedHabSiteIcon.GetValue(null) as Sprite;
            if (icon == null)
            {
                return true;
            }

            __result = icon;
            return false;
        }
    }

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
        typeof(TIHabSiteState),
        nameof(TIHabSiteState.ProductivityString))]
    internal static class SurveyedSiteProductivityPatch
    {
        [HarmonyPrefix]
        internal static void Prefix(
            TIHabSiteState __instance,
            ref bool probed)
        {
            if (probed || GameControl.control == null)
            {
                return;
            }

            probed = ProbeSurveyRuntime.SiteProspected(
                GameControl.control.activePlayer,
                __instance);
        }
    }

    [HarmonyPatch(
        typeof(BaseSiteListItemController),
        nameof(BaseSiteListItemController.SetListItem))]
    internal static class SurveyedBodySiteListItemPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            BaseSiteListItemController __instance,
            TIHabSiteState habSite,
            TIFactionState viewingFaction,
            ref int ___statusTipValue)
        {
            if (habSite == null || viewingFaction == null)
            {
                return;
            }

            bool knownHab = habSite.hasPlannedOrOperatingBase &&
                GameControl.control.activePlayer
                    .HasIntelOnSpaceAssetLocation(habSite.hab);
            bool surveyed = ProbeSurveyRuntime.SiteProspected(
                viewingFaction,
                habSite);
            if (!knownHab)
            {
                if (surveyed)
                {
                    __instance.statusImage.enabled = true;
                    __instance.tip.enabled = true;
                    GameControl.assetLoader.LoadAssetForImageAssignment(
                        "icons_2d/ICO_probe",
                        __instance.statusImage);
                    ___statusTipValue = 1;
                }
                else if (ProbeSurveyRuntime.SiteProspectorEnRoute(
                    viewingFaction,
                    habSite))
                {
                    __instance.statusImage.enabled = true;
                    __instance.tip.enabled = true;
                    GameControl.assetLoader.LoadAssetForImageAssignment(
                        "icons_2d/ICO_probe_en_route",
                        __instance.statusImage);
                    ___statusTipValue = 2;
                }
                else
                {
                    __instance.statusImage.enabled = false;
                    __instance.tip.enabled = false;
                    ___statusTipValue = 3;
                }
            }

            TIHabModuleState mine = habSite.hab == null
                ? null
                : habSite.hab.mine;
            if (!surveyed ||
                (habSite.hab != null &&
                 habSite.hab.faction == viewingFaction &&
                 mine != null &&
                 mine.active))
            {
                return;
            }

            const int bigCap = 1;
            const int smallCap = 7;
            __instance.Water.SetText(TIUtilities.FormatBigOrSmallNumber(
                habSite.GetMonthlyProduction(FactionResource.Water),
                bigCap,
                smallCap));
            __instance.Volatiles.SetText(TIUtilities.FormatBigOrSmallNumber(
                habSite.GetMonthlyProduction(FactionResource.Volatiles),
                bigCap,
                smallCap));
            __instance.Metals.SetText(TIUtilities.FormatBigOrSmallNumber(
                habSite.GetMonthlyProduction(FactionResource.Metals),
                bigCap,
                smallCap));
            __instance.Nobles.SetText(TIUtilities.FormatBigOrSmallNumber(
                habSite.GetMonthlyProduction(FactionResource.NobleMetals),
                bigCap,
                smallCap));
            __instance.Fissiles.SetText(TIUtilities.FormatBigOrSmallNumber(
                habSite.GetMonthlyProduction(FactionResource.Fissiles),
                bigCap,
                smallCap));
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
            bool knownHab = site != null &&
                site.hasPlannedOrOperatingBase &&
                faction != null &&
                faction.HasIntelOnSpaceAssetLocation(site.hab);
            if (site != null && faction != null && !knownHab)
            {
                // The original empty-site path has now completed successfully,
                // so Unity's AssetCacheManager is safe for Harmony to compile.
                SurveyedSiteIconRuntime.EnsureInstalled();
            }

            if (site == null ||
                faction == null ||
                !ProbeSurveyRuntime.SiteProspected(faction, site) ||
                knownHab ||
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
