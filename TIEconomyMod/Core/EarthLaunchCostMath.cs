using System;
using System.Collections.Generic;

namespace TIEconomyMod.Core
{
    internal struct EarthLaunchSite
    {
        internal EarthLaunchSite(double latitude_Deg)
        {
            Latitude_Deg = latitude_Deg;
        }

        internal double Latitude_Deg;
    }

    internal struct EarthParkingOption
    {
        internal EarthParkingOption(
            double altitude_km,
            double inclination_Deg,
            double onwardDeltaV_kps)
        {
            Altitude_km = altitude_km;
            Inclination_Deg = inclination_Deg;
            OnwardDeltaV_kps = onwardDeltaV_kps;
        }

        internal double Altitude_km;
        internal double Inclination_Deg;
        internal double OnwardDeltaV_kps;
    }

    internal static class EarthLaunchCostMath
    {
        internal const double ReferenceAltitude_km = 500d;
        internal const double ReferenceInclination_Deg = 0d;

        internal static double AscentDeltaV_kps(
            double gravitationalParameter_m3ps2,
            double earthRadius_m,
            double rotationPeriod_s,
            double launchLatitude_Deg,
            double targetAltitude_km,
            double targetInclination_Deg)
        {
            if (gravitationalParameter_m3ps2 <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(gravitationalParameter_m3ps2));
            }
            if (earthRadius_m <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(earthRadius_m));
            }
            if (rotationPeriod_s <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(rotationPeriod_s));
            }

            double latitude_Rad = DegreesToRadians(
                Math.Min(90d, Math.Abs(launchLatitude_Deg)));
            double inclination_Rad = DegreesToRadians(
                Math.Min(180d, Math.Abs(targetInclination_Deg)));
            if (inclination_Rad > Math.PI / 2d)
            {
                inclination_Rad = Math.PI - inclination_Rad;
            }

            double directPlane_Rad = Math.Max(latitude_Rad, inclination_Rad);
            double targetRadius_m = earthRadius_m +
                Math.Max(0d, targetAltitude_km) * 1000d;
            double transferSemiMajorAxis_m =
                (earthRadius_m + targetRadius_m) / 2d;

            double injectionSpeed_mps = Math.Sqrt(
                gravitationalParameter_m3ps2 *
                (2d / earthRadius_m - 1d / transferSemiMajorAxis_m));
            double apogeeSpeed_mps = Math.Sqrt(
                gravitationalParameter_m3ps2 *
                (2d / targetRadius_m - 1d / transferSemiMajorAxis_m));
            double circularSpeed_mps = Math.Sqrt(
                gravitationalParameter_m3ps2 / targetRadius_m);

            double rotationalSpeed_mps = 2d * Math.PI * earthRadius_m *
                Math.Cos(latitude_Rad) / rotationPeriod_s;
            double eastwardFraction = Math.Cos(directPlane_Rad) /
                Math.Max(0.000000001d, Math.Cos(latitude_Rad));
            eastwardFraction = Math.Max(-1d, Math.Min(1d, eastwardFraction));
            double injectionDeltaV_mps = Math.Sqrt(
                injectionSpeed_mps * injectionSpeed_mps +
                rotationalSpeed_mps * rotationalSpeed_mps -
                2d * injectionSpeed_mps * rotationalSpeed_mps *
                eastwardFraction);

            double dogleg_Rad = Math.Max(0d, latitude_Rad - inclination_Rad);
            double circularizationDeltaV_mps = Math.Sqrt(
                apogeeSpeed_mps * apogeeSpeed_mps +
                circularSpeed_mps * circularSpeed_mps -
                2d * apogeeSpeed_mps * circularSpeed_mps *
                Math.Cos(dogleg_Rad));

            return (injectionDeltaV_mps + circularizationDeltaV_mps) / 1000d;
        }

        internal static double ReferenceAscentDeltaV_kps(
            double gravitationalParameter_m3ps2,
            double earthRadius_m,
            double rotationPeriod_s)
        {
            return AscentDeltaV_kps(
                gravitationalParameter_m3ps2,
                earthRadius_m,
                rotationPeriod_s,
                0d,
                ReferenceAltitude_km,
                ReferenceInclination_Deg);
        }

        internal static double MinimumAscentDeltaV_kps(
            double gravitationalParameter_m3ps2,
            double earthRadius_m,
            double rotationPeriod_s,
            IEnumerable<EarthLaunchSite> sites,
            double targetAltitude_km,
            double targetInclination_Deg)
        {
            double best = double.PositiveInfinity;
            if (sites != null)
            {
                foreach (EarthLaunchSite site in sites)
                {
                    best = Math.Min(
                        best,
                        AscentDeltaV_kps(
                            gravitationalParameter_m3ps2,
                            earthRadius_m,
                            rotationPeriod_s,
                            site.Latitude_Deg,
                            targetAltitude_km,
                            targetInclination_Deg));
                }
            }

            return best;
        }

        internal static double MinimumNormalizedRouteDeltaV_kps(
            double gravitationalParameter_m3ps2,
            double earthRadius_m,
            double rotationPeriod_s,
            IEnumerable<EarthLaunchSite> sites,
            IEnumerable<EarthParkingOption> parkingOptions)
        {
            double reference = ReferenceAscentDeltaV_kps(
                gravitationalParameter_m3ps2,
                earthRadius_m,
                rotationPeriod_s);
            double best = double.PositiveInfinity;
            if (parkingOptions == null)
            {
                return best;
            }

            foreach (EarthParkingOption parking in parkingOptions)
            {
                double ascent = MinimumAscentDeltaV_kps(
                    gravitationalParameter_m3ps2,
                    earthRadius_m,
                    rotationPeriod_s,
                    sites,
                    parking.Altitude_km,
                    parking.Inclination_Deg);
                best = Math.Min(
                    best,
                    ascent - reference + parking.OnwardDeltaV_kps);
            }

            return best;
        }

        internal static double BoostCost(
            double mass_tons,
            double spaceResourceToTons,
            double normalizedDeltaV_kps,
            double genericExhaustVelocity_kps)
        {
            if (mass_tons <= 0d || spaceResourceToTons <= 0d)
            {
                return 0d;
            }
            if (genericExhaustVelocity_kps <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(genericExhaustVelocity_kps));
            }

            return mass_tons * spaceResourceToTons *
                Math.Exp(normalizedDeltaV_kps / genericExhaustVelocity_kps);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180d;
        }
    }
}
