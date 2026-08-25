using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TIEconomyMod
{
    internal static class ProbeSurveyNotifications
    {
        private static readonly MethodInfo AddItem = AccessTools.Method(
            typeof(TINotificationQueueState),
            "AddItem");

        internal static void LogSiteProbeArrived(
            TIFactionState faction,
            TIHabSiteState site)
        {
            TISpaceBodyState body = site.parentBody;
            NotificationQueueItem item = new NotificationQueueItem
            {
                templateName = nameof(TINotificationQueueState.LogProbeArrived),
                relevantFactions = new List<TIFactionState> { faction },
                primaryFactions = new List<TIFactionState> { faction },
                icon = body.iconResource,
                popupResource1 = body.iconResource,
                itemSummary = Loc.T(
                    "UI.Notifications.ProbeArrivedSummary",
                    faction.displayName,
                    site.displayName),
                itemHeadline = Loc.T("UI.Notifications.ProbeArrivedHeadline"),
                gotoGameState = site
            };

            StringBuilder detail = new StringBuilder(Loc.T(
                "UI.Notifications.ProbeArrivedDetail",
                faction.displayName,
                site.displayName,
                body.maxHabTier.ToString())).AppendLine().AppendLine();
            if (faction.AlienTerritoryToAvoid(body))
            {
                detail.AppendLine(TIUtilities.RedLine(
                    Loc.T("UI.Space.AlienTerritory"))).AppendLine();
            }

            if (site.hasPlannedOrOperatingBase)
            {
                detail.Append(Loc.T(
                    "UI.Notifications.ProbeData_Hab",
                    site.displayName,
                    site.hab.displayName,
                    site.hab.coreFaction.displayNameWithColor,
                    site.ProductivityString(true)));
            }
            else
            {
                detail.Append(Loc.T(
                    "UI.Notifications.ProbeData_NoHab",
                    site.displayName,
                    site.ProductivityString(true)));
            }

            item.notificationDelegates.Add(
                SpecialNotificationDelegate.SetBodyTagToRed);
            item.notificationDelegates.Add(
                SpecialNotificationDelegate.SetBodyTagToGreen);
            item.itemDetail = detail.ToString();

            if (AddItem == null)
            {
                Main.Log("Unable to queue per-site probe result: " +
                    "TINotificationQueueState.AddItem was not found.");
                return;
            }

            AddItem.Invoke(null, new object[] { item, false });
        }
    }
}
