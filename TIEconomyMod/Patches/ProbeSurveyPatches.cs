using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems.GameTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(
        typeof(LaunchAllProbeOperation),
        nameof(LaunchAllProbeOperation.GetPossibleTargets))]
    internal static class BulkProbeSiteTargetsPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIGameState actorState,
            ref List<TIGameState> __result)
        {
            TIFactionState faction = actorState == null
                ? null
                : actorState.ref_faction;
            if (faction == null)
            {
                __result = new List<TIGameState>();
                return false;
            }

            __result = GameStateManager.AllSpaceBodies()
                .SelectMany(body =>
                    ProbeSurveyRuntime.EligibleSites(faction, body))
                .OrderBy(site => site.parentBody.ID)
                .ThenBy(site => site.ID)
                .Cast<TIGameState>()
                .ToList();
            return false;
        }
    }

    [HarmonyPatch(
        typeof(LaunchProbeOperation),
        nameof(LaunchProbeOperation.GetTargetingMethod))]
    internal static class ProbeSiteTargetingMethodPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(ref Type __result)
        {
            __result = typeof(TIOperationTargeting_HabSite);
        }
    }

    [HarmonyPatch(
        typeof(LaunchProbeOperation),
        nameof(LaunchProbeOperation.GetPossibleTargets))]
    internal static class ProbeSiteTargetsPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIGameState actorState,
            TIGameState defaultTarget,
            ref List<TIGameState> __result)
        {
            TIFactionState faction = actorState == null
                ? null
                : actorState.ref_faction;
            TISpaceBodyState body = defaultTarget == null
                ? null
                : defaultTarget.ref_spaceBody;
            __result = ProbeSurveyRuntime.EligibleSites(faction, body)
                .Cast<TIGameState>()
                .ToList();
            return false;
        }
    }

    [HarmonyPatch(
        typeof(LaunchProbeOperation),
        nameof(LaunchProbeOperation.OpVisibleToActor))]
    internal static class ProbeSiteVisibilityPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIGameState actorState,
            TIGameState targetState,
            ref bool __result)
        {
            TIFactionState faction = actorState == null
                ? null
                : actorState.ref_faction;
            TISpaceBodyState body = targetState == null
                ? null
                : targetState.ref_spaceBody;
            __result = faction != null &&
                !faction.IsAlienFaction &&
                body != null &&
                faction.CanProspectWithProbe(body, false);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(LaunchProbeOperation),
        nameof(LaunchProbeOperation.OnOperationConfirm))]
    internal static class ProbeSiteLaunchPatch
    {
        private static readonly MethodInfo ConfirmBase = AccessTools.Method(
            typeof(TIOperationTemplate),
            "OnOperationConfirm_Base");

        [HarmonyPrefix]
        internal static bool Prefix(
            LaunchProbeOperation __instance,
            TIGameState actorState,
            ref TIGameState target,
            TIResourcesCost resourcesCost,
            Trajectory trajectory,
            ref bool __result)
        {
            TIFactionState faction = actorState == null
                ? null
                : actorState.ref_faction;
            TIHabSiteState site = ProbeSurveyRuntime.ResolveSite(
                faction,
                target);
            if (faction == null || site == null || ConfirmBase == null)
            {
                __result = false;
                return false;
            }

            target = site;
            __result = (bool)ConfirmBase.Invoke(
                __instance,
                new object[]
                {
                    actorState,
                    target,
                    resourcesCost,
                    trajectory
                });
            if (!__result)
            {
                return false;
            }

            ProbeSurveyRuntime.LaunchSiteProspector(faction, site);
            TISpaceBodyState body = site.parentBody;
            GameControl.eventManager.TriggerEvent(
                new ProspectingBody(faction, body),
                null,
                faction,
                body);
            TINotificationQueueState.LogProbeLaunched(faction, body);
            TINotificationQueueState.LogEnemyProbeLaunched(faction, body);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(LaunchProbeOperation),
        nameof(LaunchProbeOperation.ExecuteOperation))]
    internal static class ProbeSiteCompletionPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIGameState actorState,
            TIGameState target)
        {
            TIHabSiteState site = target == null
                ? null
                : target.ref_habSite;
            if (site == null)
            {
                // Pending probes saved before this change still target a body
                // and retain their original body-wide completion behavior.
                return true;
            }

            TIFactionState faction = actorState.ref_faction;
            ProbeSurveyRuntime.ProspectSite(faction, site);
            TINotificationQueueState.LogProbeArrived(
                faction,
                site.parentBody);
            TINotificationQueueState.LogEnemyProbeArrived(
                faction,
                site.parentBody);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.Prospected),
        new Type[] { typeof(TIHabSiteState) })]
    internal static class SiteProspectedStatePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TIHabSiteState habSite,
            ref bool __result)
        {
            __result = ProbeSurveyRuntime.SiteProspected(
                __instance,
                habSite);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.CandidateForProspecting))]
    internal static class BodyProspectingCandidatePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            ref bool __result)
        {
            __result = spaceBody != null &&
                spaceBody.habSites.Length != 0 &&
                !ProbeSurveyRuntime.BodyProspected(__instance, spaceBody) &&
                __instance.CanExplore(spaceBody) &&
                !ProbeSurveyRuntime.BodyHasProspectorEnRoute(
                    __instance,
                    spaceBody) &&
                !__instance.FleetSurveyingPlanet(spaceBody) &&
                ProbeSurveyRuntime.EligibleSites(__instance, spaceBody)
                    .Count > 0;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.CanProspectWithProbe))]
    internal static class BodyProbeAvailabilityPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            ref bool __result)
        {
            __result = spaceBody != null &&
                __instance.CanProspectFromShip(spaceBody) &&
                !__instance.FleetSurveyingPlanet(spaceBody) &&
                ProbeSurveyRuntime.EligibleSites(__instance, spaceBody)
                    .Count > 0;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.ProspectingSpaceBody))]
    internal static class BodyProspectingStatePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            ref bool __result)
        {
            __result = spaceBody != null &&
                !ProbeSurveyRuntime.BodyProspected(__instance, spaceBody) &&
                (ProbeSurveyRuntime.BodyHasProspectorEnRoute(
                    __instance,
                    spaceBody) ||
                 __instance.FleetSurveyingPlanet(spaceBody));
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.ProspectorEnRoute))]
    internal static class BodyProspectorEnRoutePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            ref bool __result)
        {
            __result = ProbeSurveyRuntime.BodyHasProspectorEnRoute(
                __instance,
                spaceBody);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.ProspectorArrival))]
    internal static class BodyProspectorArrivalPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            GameTimeManager ___gameTime,
            ref TIDateTime __result)
        {
            float bodyIntel = __instance.GetIntel(spaceBody);
            if (bodyIntel >= TIFactionState.intelMarkerForProspectorEnRoute &&
                bodyIntel < TIFactionState.intelToProspectSpaceBody)
            {
                return true;
            }

            __result = null;
            if (spaceBody == null || ___gameTime == null)
            {
                return false;
            }

            TIOperationTemplate operation = OperationsManager.operationsLookup[
                typeof(LaunchProbeOperation)].GetTemplate();
            foreach (TIHabSiteState site in spaceBody.habSites.Where(candidate =>
                ProbeSurveyRuntime.SiteProspectorEnRoute(
                    __instance,
                    candidate)))
            {
                TIDateTime arrival = ___gameTime.GetTimeForPendingEvent(
                    __instance.factionOperationCompleteName,
                    __instance,
                    site,
                    operation);
                if (arrival != null &&
                    (__result == null || arrival < __result))
                {
                    __result = arrival;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.SpaceBodiesWithProspectorEnRoute))]
    internal static class ProspectorBodiesListPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            ref List<TISpaceBodyState> __result)
        {
            __result = GameStateManager.AllSpaceBodies()
                .Where(body => ProbeSurveyRuntime.BodyHasProspectorEnRoute(
                    __instance,
                    body))
                .ToList();
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIFactionState),
        nameof(TIFactionState.EligibleForFoundingBase))]
    internal static class SurveyedSiteFoundingAvailabilityPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            TISpaceBodyState spaceBody,
            ref bool __result)
        {
            __result = spaceBody != null &&
                __instance.EligibleforColonization(spaceBody) &&
                ProbeSurveyRuntime.AnySurveyedVacantSite(
                    __instance,
                    spaceBody);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(FoundBaseOperation),
        nameof(FoundBaseOperation.GetPossibleTargets))]
    internal static class SurveyedBaseTargetsPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TIGameState actorState,
            ref List<TIGameState> __result)
        {
            TIFactionState faction = actorState == null
                ? null
                : actorState.ref_faction;
            if (faction == null || __result == null)
            {
                __result = new List<TIGameState>();
                return;
            }

            __result = __result
                .Where(target =>
                    target != null &&
                    ProbeSurveyRuntime.SiteProspected(
                        faction,
                        target.ref_habSite))
                .ToList();
        }
    }
}
