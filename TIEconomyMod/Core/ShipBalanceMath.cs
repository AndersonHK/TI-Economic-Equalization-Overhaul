using System;

namespace TIEconomyMod
{
    public static class ShipBalanceMath
    {
        public static float CrewMass_tons(
            int crewBillets, float massPerCrew_tons)
        {
            return Math.Max(0, crewBillets) *
                Math.Max(0f, massPerCrew_tons);
        }

        public static float HumanHullDriveScale(
            string hullDataName, bool alien)
        {
            if (alien || string.IsNullOrEmpty(hullDataName))
            {
                return 1f;
            }

            switch (hullDataName)
            {
                case "Cruiser":
                    return 1.3f;
                case "Battlecruiser":
                    return 1.5f;
                case "Lancer":
                    return 1.72f;
                case "Battleship":
                    return 1.75f;
                case "Dreadnought":
                    return 2f;
                case "Titan":
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        public static float DriveScale(
            string hullDataName,
            bool alien,
            int appearanceIndex,
            string nozzleFamily)
        {
            string ignoredDiagnostic;
            return DriveScale(
                hullDataName,
                alien,
                appearanceIndex,
                nozzleFamily,
                out ignoredDiagnostic);
        }

        public static float DriveScale(
            string hullDataName,
            bool alien,
            int appearanceIndex,
            string nozzleFamily,
            out string diagnostic)
        {
            diagnostic = null;
            if (string.IsNullOrEmpty(hullDataName))
            {
                diagnostic = "Drive scale lookup received an empty hull data name.";
                return 1f;
            }

            if (alien)
            {
                return AlienDriveScale(
                    hullDataName, appearanceIndex, out diagnostic);
            }

            if (!IsKnownHumanHull(hullDataName))
            {
                diagnostic = "No human drive scale is configured for hull '" +
                    hullDataName + "'.";
                return 1f;
            }

            // Preserve the approved pre-variant human balance exactly. Human
            // appearance and nozzle measurements remain research inputs only;
            // this pass changes graphical scaling for alien hulls.
            return HumanHullDriveScale(hullDataName, false);
        }

        private static bool IsKnownHumanHull(string hullDataName)
        {
            switch (hullDataName)
            {
                case "Gunship":
                case "Escort":
                case "Corvette":
                case "Frigate":
                case "Monitor":
                case "Destroyer":
                case "Cruiser":
                case "Battlecruiser":
                case "Lancer":
                case "Battleship":
                case "Dreadnought":
                case "Titan":
                    return true;
                default:
                    return false;
            }
        }

        private static float AlienDriveScale(
            string hullDataName,
            int appearanceIndex,
            out string diagnostic)
        {
            diagnostic = null;
            // The installed alien templates have one authored appearance. An
            // unknown future appearance must not inherit index 0 art.
            if (appearanceIndex != 0)
            {
                diagnostic = "No alien drive scale is configured for hull '" +
                    hullDataName + "' appearance " + appearanceIndex + ".";
                return 1f;
            }

            switch (hullDataName)
            {
                case "AlienGunship":
                case "AlienEscort":
                case "AlienCorvette":
                    // Corvette measures below the template baseline and is
                    // intentionally clamped to the 1.0 gameplay floor.
                    return 1f;
                case "AlienFrigate":
                    return 1.144f;
                case "AlienMonitor":
                    return 2.202f;
                case "AlienDestroyer":
                    return 3.384f;
                case "AlienCruiser":
                case "AlienLancer":
                    return 3.291f;
                case "AlienBattlecruiser":
                case "AlienBattleship":
                case "AlienDreadnought":
                case "AlienAssaultCarrier":
                    return 3.445f;
                case "AlienTitan":
                    return 7.531f;
                case "AlienMothership":
                    return 26.216f;
                case "SalamanderGunship":
                    diagnostic = "SalamanderGunship has no standalone alien " +
                        "drive resource; no measured scale can be applied.";
                    return 1f;
                default:
                    diagnostic = "No alien drive scale is configured for hull '" +
                        hullDataName + "'.";
                    return 1f;
            }
        }

        public static bool TryGetMeasuredReactorBayVolume_m3(
            string hullDataName, int appearanceIndex, out float volume_m3)
        {
            volume_m3 = 0f;
            switch (hullDataName)
            {
                case "Gunship":
                    volume_m3 = VariantValue(appearanceIndex,
                        264.240616f, 452.197326f, 317.310118f, 712.241612f);
                    break;
                case "Escort":
                    volume_m3 = VariantValue(appearanceIndex,
                        264.240558f, 452.197326f, 317.310118f, 712.241612f);
                    break;
                case "Corvette":
                    volume_m3 = VariantValue(appearanceIndex,
                        264.240616f, 452.197235f, 604.707011f, 837.587811f);
                    break;
                case "Frigate":
                    volume_m3 = VariantValue(appearanceIndex,
                        332.341240f, 675.443739f, 1246.492028f, 1233.527032f);
                    break;
                case "Monitor":
                    volume_m3 = VariantValue(appearanceIndex,
                        384.582064f, 675.443717f, 2617.607109f, 2028.674504f);
                    break;
                case "Destroyer":
                    volume_m3 = VariantValue(appearanceIndex,
                        384.582064f, 675.443717f, 2617.606700f, 2028.674504f);
                    break;
                case "Cruiser":
                    volume_m3 = VariantValue(appearanceIndex,
                        1989.241734f, 1384.983819f, 3930.637720f, 3505.550347f);
                    break;
                case "Battlecruiser":
                    volume_m3 = VariantValue(appearanceIndex,
                        1989.242548f, 1384.983819f, 3930.637720f, 3505.550347f);
                    break;
                case "Lancer":
                    volume_m3 = VariantValue(appearanceIndex,
                        2365.773019f, 2090.292333f, 10223.879025f, 8072.643840f);
                    break;
                case "Battleship":
                    volume_m3 = VariantValue(appearanceIndex,
                        5648.074162f, 2090.291983f, 5464.773080f, 6945.700026f);
                    break;
                case "Dreadnought":
                    volume_m3 = VariantValue(appearanceIndex,
                        11476.330412f, 2090.293033f, 10223.879025f, 10952.622272f);
                    break;
                case "Titan":
                    volume_m3 = VariantValue(appearanceIndex,
                        15955.575747f, 6290.836709f, 16549.539439f, 15840.889300f);
                    break;
            }

            return volume_m3 > 0f;
        }

        private static float VariantValue(
            int appearanceIndex, float index0, float index1,
            float index2, float index3)
        {
            switch (appearanceIndex)
            {
                case 0:
                    return index0;
                case 1:
                    return index1;
                case 2:
                    return index2;
                case 3:
                    return index3;
                default:
                    return 0f;
            }
        }

        public static float ReactorBayVolume_m3(
            string hullDataName,
            int appearanceIndex,
            bool smallHull,
            bool mediumHull,
            bool largeHull,
            bool hugeHull,
            out bool usedFallback,
            out string sizeBand)
        {
            float measured;
            if (TryGetMeasuredReactorBayVolume_m3(
                hullDataName, appearanceIndex, out measured))
            {
                usedFallback = false;
                sizeBand = null;
                return measured;
            }

            usedFallback = true;
            if (hugeHull)
            {
                sizeBand = "Huge";
                return 16549.539439f;
            }
            if (largeHull)
            {
                sizeBand = "Large";
                return 16549.539439f;
            }
            if (mediumHull)
            {
                sizeBand = "Medium";
                return 3930.637720f;
            }

            // Small is also the conservative fallback for malformed hulls that
            // do not report any vanilla size-band property.
            sizeBand = smallHull ? "Small" : "Unknown (small fallback)";
            return 2617.607109f;
        }

        public static float ReactorInstalledDensity_tonsPerM3(
            string powerPlantClass)
        {
            switch (powerPlantClass)
            {
                case "Fuel_Cell":
                    return 1.2f;
                case "Solid_Core_Fission":
                case "Antimatter_Solid_Core":
                    return 2.5f;
                case "Molten_Salt_Core_Fission":
                    return 3.5f;
                case "Liquid_Core_Fission":
                case "Z_Pinch_Fusion":
                case "Antimatter_Plasma_Core":
                    return 2.5f;
                case "Gas_Core_Fission":
                case "Antimatter_Gas_Core":
                    return 2f;
                case "Electrostatic_Confinement_Fusion":
                    return 1f;
                case "Mirrored_Magnetic_Confinement_Fusion":
                    return 1.2f;
                case "Inertial_Confinement_Fusion":
                    return 1.5f;
                case "Antimatter_Beam_Core":
                    return 3f;
                case "Any_Magnetic_Confinement_Fusion":
                case "Toroid_Magnetic_Confinement_Fusion":
                case "Hybrid_Confinement_Fusion":
                default:
                    return 2f;
            }
        }

        public static float ReactorReportedMassBayFraction(
            string powerPlantClass)
        {
            switch (powerPlantClass)
            {
                case "Fuel_Cell":
                    return 0.25f;
                case "Solid_Core_Fission":
                case "Antimatter_Solid_Core":
                    return 0.5f;
                case "Molten_Salt_Core_Fission":
                case "Liquid_Core_Fission":
                    return 0.55f;
                case "Gas_Core_Fission":
                case "Antimatter_Gas_Core":
                    return 0.45f;
                case "Z_Pinch_Fusion":
                case "Inertial_Confinement_Fusion":
                case "Antimatter_Plasma_Core":
                    return 0.6f;
                case "Antimatter_Beam_Core":
                    return 0.4f;
                case "Electrostatic_Confinement_Fusion":
                case "Mirrored_Magnetic_Confinement_Fusion":
                case "Any_Magnetic_Confinement_Fusion":
                case "Toroid_Magnetic_Confinement_Fusion":
                case "Hybrid_Confinement_Fusion":
                default:
                    return 0.75f;
            }
        }

        public static float ReactorBayMassAllowance_tons(
            float bayVolume_m3, string powerPlantClass)
        {
            float volume = Math.Max(0f, bayVolume_m3);
            float density = ReactorInstalledDensity_tonsPerM3(powerPlantClass);
            float fraction = ReactorReportedMassBayFraction(powerPlantClass);
            if (volume <= 0f || density <= 0f || fraction <= 0f)
            {
                return 0f;
            }

            return volume * density / fraction;
        }

        public static float ReactorBayVolumeUsed_m3(
            float requiredPower_GW,
            string powerPlantClass,
            float specificPower_tGW)
        {
            if (requiredPower_GW <= 0f || specificPower_tGW <= 0f ||
                float.IsNaN(requiredPower_GW) ||
                float.IsInfinity(requiredPower_GW) ||
                float.IsNaN(specificPower_tGW) ||
                float.IsInfinity(specificPower_tGW))
            {
                return 0f;
            }

            float density = ReactorInstalledDensity_tonsPerM3(
                powerPlantClass);
            float fraction = ReactorReportedMassBayFraction(
                powerPlantClass);
            if (density <= 0f || fraction <= 0f)
            {
                return 0f;
            }

            double reportedMass_tons =
                (double)requiredPower_GW * specificPower_tGW;
            double volumeUsed_m3 = reportedMass_tons * fraction / density;
            if (double.IsNaN(volumeUsed_m3) || volumeUsed_m3 <= 0d)
            {
                return 0f;
            }
            if (double.IsInfinity(volumeUsed_m3) ||
                volumeUsed_m3 >= float.MaxValue)
            {
                return float.MaxValue;
            }
            return (float)volumeUsed_m3;
        }

        public static float ReactorBayOutputLimit_GW(
            float bayVolume_m3,
            string powerPlantClass,
            float specificPower_tGW,
            float theoreticalMaximum_GW)
        {
            float theoretical = Math.Max(0f, theoreticalMaximum_GW);
            if (specificPower_tGW <= 0f || float.IsNaN(specificPower_tGW) ||
                float.IsInfinity(specificPower_tGW))
            {
                return theoretical;
            }

            double allowance = ReactorBayMassAllowance_tons(
                bayVolume_m3, powerPlantClass);
            double limit = allowance / specificPower_tGW;
            if (double.IsNaN(limit) || limit <= 0d)
            {
                return theoretical;
            }
            if (double.IsInfinity(limit) || limit >= float.MaxValue)
            {
                return float.MaxValue;
            }
            return (float)limit;
        }

        public static float EffectiveReactorOutput_GW(
            float theoreticalMaximum_GW,
            float bayOutputLimit_GW)
        {
            return Math.Min(
                Math.Max(0f, theoreticalMaximum_GW),
                Math.Max(0f, bayOutputLimit_GW));
        }

        public static float ScaledDriveValue(float baseValue, float scale)
        {
            return Math.Max(0f, baseValue) * Math.Max(1f, scale);
        }

        public static float AdditionalScaledDriveValue(
            float baseValue, float scale)
        {
            return Math.Max(0f, baseValue) *
                (Math.Max(1f, scale) - 1f);
        }
    }
}
