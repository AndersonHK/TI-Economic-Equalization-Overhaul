using PavonisInteractive.TerraInvicta;
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
                (BodyProspected(faction, site.parentBody) ||
                 faction.GetIntel(site) >= 1f);
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

            float intel = faction.GetIntel(site);
            return intel >= TIFactionState.intelMarkerForProspectorEnRoute &&
                intel < TIFactionState.intelToProspectSpaceBody;
        }

        internal static bool BodyHasProspectorEnRoute(
            TIFactionState faction,
            TISpaceBodyState body)
        {
            if (faction == null || body == null || BodyProspected(faction, body))
            {
                return false;
            }

            float legacyIntel = faction.GetIntel(body);
            if (legacyIntel >= TIFactionState.intelMarkerForProspectorEnRoute &&
                legacyIntel < TIFactionState.intelToProspectSpaceBody)
            {
                return true;
            }

            return body.habSites.Any(site =>
                SiteProspectorEnRoute(faction, site));
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
                    SiteProspected(faction, candidate)))
            {
                faction.ProspectSpaceBody(body);
            }
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
