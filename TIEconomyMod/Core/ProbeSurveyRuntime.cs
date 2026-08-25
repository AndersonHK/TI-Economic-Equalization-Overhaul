using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems.GameTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod
{
    internal static class ProbeSurveyRuntime
    {
        internal static float PayloadMass_tons
        {
            get
            {
                return Math.Max(
                    0f,
                    TemplateManager.global.probePayloadBaseline_tons);
            }
        }

        internal static bool BodyProspected(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            return faction != null &&
                body != null &&
                faction.GetIntel(body) >= 1f;
        }

        internal static bool SiteProspected(
            TIFactionState faction,
            TIHabSiteState site)
        {
            return faction != null &&
                site != null &&
                ProbeSurveyStateMath.SiteProspected(
                    faction.GetIntel(site.parentBody),
                    faction.GetIntel(site),
                    TIFactionState.intelToProspectSpaceBody);
        }

        internal static bool SiteProspectorEnRoute(
            TIFactionState faction,
            TIHabSiteState site)
        {
            if (faction == null || site == null ||
                BodyProspected(faction, site.parentBody))
            {
                return false;
            }

            bool hasMarker = ProbeSurveyStateMath.SiteProspectorEnRoute(
                faction.GetIntel(site.parentBody),
                faction.GetIntel(site),
                TIFactionState.intelMarkerForProspectorEnRoute,
                TIFactionState.intelToProspectSpaceBody);
            return hasMarker && PendingProbeArrival(faction, site) != null;
        }

        internal static bool BodyHasProspectorEnRoute(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            if (faction == null || body == null || BodyProspected(faction, body))
            {
                return false;
            }

            if (LegacyBodyProspectorEnRoute(faction, body))
            {
                return true;
            }

            return body.habSites.Any(site =>
                SiteProspectorEnRoute(faction, site));
        }

        internal static bool LegacyBodyProspectorEnRoute(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            if (faction == null || body == null || BodyProspected(faction, body))
            {
                return false;
            }

            float intel = faction.GetIntel(body);
            bool hasMarker =
                intel >= TIFactionState.intelMarkerForProspectorEnRoute &&
                intel < TIFactionState.intelToProspectSpaceBody;
            return hasMarker && PendingProbeArrival(faction, body) != null;
        }

        internal static TIDateTime PendingProbeArrival(
            TIFactionState faction,
            TIGameState target)
        {
            if (faction == null || target == null)
            {
                return null;
            }

            TIDateTime earliest = null;
            foreach (TITimeEvent pendingEvent in
                GameStateManager.GetAllGameStates<TITimeEvent>())
            {
                if (pendingEvent == null ||
                    pendingEvent.isComplete ||
                    pendingEvent.archived ||
                    pendingEvent.eventName !=
                        faction.factionOperationCompleteName ||
                    pendingEvent.eventDataTemplateName !=
                        typeof(LaunchProbeOperation).Name ||
                    !ReferenceEquals(pendingEvent.eventObject, faction) ||
                    !ReferenceEquals(pendingEvent.eventObject2, target))
                {
                    continue;
                }

                TIDateTime arrival = pendingEvent.time;
                if (arrival != null &&
                    (earliest == null || arrival < earliest))
                {
                    earliest = arrival;
                }
            }

            return earliest;
        }

        internal static TIDateTime EarliestProspectorArrival(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            if (faction == null || body == null)
            {
                return null;
            }

            TIDateTime earliest = PendingProbeArrival(faction, body);
            foreach (TIHabSiteState site in body.habSites)
            {
                TIDateTime arrival = PendingProbeArrival(faction, site);
                if (arrival != null &&
                    (earliest == null || arrival < earliest))
                {
                    earliest = arrival;
                }
            }

            return earliest;
        }

        internal static List<TIHabSiteState> EligibleSites(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            if (faction == null ||
                body == null ||
                body.habSites == null ||
                body.habSites.Length == 0 ||
                BodyProspected(faction, body) ||
                !faction.CanExplore(body))
            {
                return new List<TIHabSiteState>();
            }

            return body.habSites
                .Where(site =>
                    site != null &&
                    !SiteProspected(faction, site) &&
                    !SiteProspectorEnRoute(faction, site))
                .ToList();
        }

        internal static List<TIHabSiteState> SurveyedSites(
            TIFactionState faction)
        {
            if (faction == null)
            {
                return new List<TIHabSiteState>();
            }

            return GameStateManager.AllSpaceBodies()
                .Where(body => body != null && body.habSites != null)
                .SelectMany(body => body.habSites)
                .Where(site => SiteProspected(faction, site))
                .OrderBy(site => site.parentBody.ID)
                .ThenBy(site => site.ID)
                .ToList();
        }

        internal static bool HasSurveyedSite(TIFactionState faction)
        {
            return faction != null &&
                GameStateManager.AllSpaceBodies().Any(body =>
                    body != null &&
                    body.habSites != null &&
                    body.habSites.Any(site => SiteProspected(faction, site)));
        }

        internal static TIHabSiteState ResolveSite(
            TIFactionState faction,
            TIGameState target)
        {
            if (target == null)
            {
                return null;
            }

            TIHabSiteState supplied = target.ref_habSite;
            if (supplied != null)
            {
                return EligibleSites(faction, supplied.parentBody)
                    .FirstOrDefault(site => ReferenceEquals(site, supplied));
            }

            TISpaceBodyState body = target.ref_spaceBody;
            List<TIHabSiteState> eligible = EligibleSites(faction, body);
            if (eligible.Count == 0)
            {
                return null;
            }

            return eligible
                .OrderByDescending(site =>
                    AIEvaluators.EvaluateHabSite(faction, site))
                .ThenBy(site => site.ID)
                .First();
        }

        internal static void LaunchSiteProspector(
            TIFactionState faction,
            TIHabSiteState site)
        {
            if (faction == null || site == null)
            {
                return;
            }

            faction.SetIntel(
                site,
                TIFactionState.intelMarkerForProspectorEnRoute);
        }

        internal static void ProspectSite(
            TIFactionState faction,
            TIHabSiteState site)
        {
            if (faction == null || site == null)
            {
                return;
            }

            faction.SetIntel(site, TIFactionState.intelToProspectSpaceBody);
            TISpaceBodyState body = site.parentBody;
            if (!BodyProspected(faction, body) &&
                body.habSites.All(candidate =>
                    candidate != null &&
                    faction.GetIntel(candidate) >=
                        TIFactionState.intelToProspectSpaceBody))
            {
                faction.ProspectSpaceBody(body);
                return;
            }

            // Site intel does not raise the vanilla body event. Reuse that
            // event as a body-scoped UI invalidation signal so each site
            // controller can re-evaluate its own per-site state immediately.
            GameControl.eventManager.TriggerEvent(
                new SpaceBodyProspected(faction, body),
                null,
                faction,
                body);
        }

        internal static float ScanDuration_days(
            TIFactionState faction,
            TIHabSiteState site)
        {
            if (faction == null || site == null)
            {
                return 1f;
            }

            string effect = site.parentBody.template.effectToExplore;
            TITechTemplate technology = TemplateManager
                .IterateByClass<TITechTemplate>()
                .FirstOrDefault(candidate => candidate.Effects.Any(
                    candidateEffect => candidateEffect.dataName == effect));
            float contributionRemaining = 1f;
            if (technology != null &&
                faction.techContributionHistory.ContainsKey(technology))
            {
                contributionRemaining = Math.Max(
                    0f,
                    Math.Min(
                        1f,
                        1f - faction.techContributionHistory[technology]));
            }

            return Math.Max(1f, 2f * contributionRemaining);
        }

        internal static bool AnySurveyedVacantSite(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            return faction != null &&
                body != null &&
                body.vacantHabSites.Any(site =>
                    SiteProspected(faction, site));
        }
    }
}
