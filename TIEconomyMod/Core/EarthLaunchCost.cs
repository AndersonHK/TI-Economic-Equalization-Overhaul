using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using TIEconomyMod.Core;

namespace TIEconomyMod
{
    internal static class EarthLaunchCost
    {
        internal static double CalculateBoost(
            TIFactionState faction,
            TIGameState destination,
            float mass_tons)
        {
            if (destination == null || mass_tons <= 0f)
            {
                return 0d;
            }

            double normalizedDeltaV = CalculateNormalizedDeltaV_kps(
                faction,
                destination);
            return EarthLaunchCostMath.BoostCost(
                mass_tons,
                TemplateManager.global.spaceResourceToTons,
                normalizedDeltaV,
                TISpaceObjectState.ModifiedGenericTransferEV_kps(faction));
        }

        internal static double CalculateNormalizedDeltaV_kps(
            TIFactionState faction,
            TIGameState destination)
        {
            TISpaceBodyState earth = GameStateManager.Earth();
            List<EarthLaunchSite> sites = LaunchSites();
            double reference = EarthLaunchCostMath.ReferenceAscentDeltaV_kps(
                earth.mu,
                earth.meanRadius_m,
                earth.rotationperiod_s);

            TIOrbitState targetOrbit = destination.ref_orbit;
            if (targetOrbit != null &&
                targetOrbit.barycenter != null &&
                targetOrbit.barycenter.isEarth &&
                targetOrbit.interfaceOrbit)
            {
                return MinimumAscent(
                    earth,
                    sites,
                    targetOrbit) - reference;
            }

            double landing = LandingDeltaV_kps(destination);
            bool ignoreDestinationInclination =
                destination.ref_spaceBody != null &&
                destination.ref_spaceBody.isEarth;
            double best = double.PositiveInfinity;
            foreach (TIOrbitState parking in GameStateManager.LEOStates()
                .Where(orbit => orbit != null &&
                    orbit.barycenter != null &&
                    orbit.barycenter.isEarth &&
                    orbit.interfaceOrbit))
            {
                double launchSurcharge = MinimumAscent(
                    earth,
                    sites,
                    parking) - reference;
                double transfer = TISpaceObjectState.GenericTransferDeltaV_mps(
                    parking,
                    destination,
                    ignoreDestinationInclination) / 1000d;
                best = Math.Min(
                    best,
                    launchSurcharge + transfer + landing);
            }

            if (double.IsPositiveInfinity(best))
            {
                throw new InvalidOperationException(
                    "No instantiated Earth interface orbit is available for launch costing.");
            }

            return best;
        }

        private static double MinimumAscent(
            TISpaceBodyState earth,
            IEnumerable<EarthLaunchSite> sites,
            TIOrbitState orbit)
        {
            return EarthLaunchCostMath.MinimumAscentDeltaV_kps(
                earth.mu,
                earth.meanRadius_m,
                earth.rotationperiod_s,
                sites,
                (orbit.semiMajorAxis_m - earth.meanRadius_m) / 1000d,
                orbit.inclination_Rad * 180d / Math.PI);
        }

        private static List<EarthLaunchSite> LaunchSites()
        {
            TIRegionState[] regions = GameStateManager.AllRegions() ??
                new TIRegionState[0];
            IEnumerable<TIRegionState> candidates = regions.Where(region =>
                region != null && region.boostPerYear_dekatons > 0f);
            if (!candidates.Any())
            {
                candidates = regions.Where(region => region != null);
            }

            List<EarthLaunchSite> sites = candidates
                .Select(region => new EarthLaunchSite(region.boostLatitude))
                .OrderBy(site => Math.Abs(site.Latitude_Deg))
                .ThenBy(site => site.Latitude_Deg)
                .ToList();
            if (sites.Count == 0)
            {
                sites.Add(new EarthLaunchSite(0d));
            }

            return sites;
        }

        private static double LandingDeltaV_kps(TIGameState destination)
        {
            if (destination.ref_habSite != null)
            {
                return destination.ref_habSite.DeltaVToLandFromInterface_kps(
                    null,
                    9.8d,
                    true,
                    true);
            }
            if (destination.isSpaceBodyState &&
                destination.ref_spaceBody.habSites.Length != 0)
            {
                return destination.ref_spaceBody.habSites[0]
                    .DeltaVToLandFromInterface_kps(null, 9.8d, true, true);
            }

            return 0d;
        }
    }
}
