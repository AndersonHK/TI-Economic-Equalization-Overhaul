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
