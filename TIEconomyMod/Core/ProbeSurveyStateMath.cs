namespace TIEconomyMod
{
    internal static class ProbeSurveyStateMath
    {
        internal static bool SiteProspected(
            float bodyIntel,
            float siteIntel,
            float completionThreshold)
        {
            return bodyIntel >= completionThreshold ||
                siteIntel >= completionThreshold;
        }

        internal static bool SiteProspectorEnRoute(
            float bodyIntel,
            float siteIntel,
            float routeMarker,
            float completionThreshold)
        {
            return bodyIntel < completionThreshold &&
                siteIntel >= routeMarker &&
                siteIntel < completionThreshold;
        }
    }
}
