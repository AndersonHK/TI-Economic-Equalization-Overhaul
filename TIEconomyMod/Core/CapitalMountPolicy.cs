using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIEconomyMod.Core
{
    public static class CapitalMountPolicy
    {
        public const int Schema = 3;

        public static int ShipTier(string hull)
        {
            switch (hull)
            {
                case "Gunship": case "Escort": case "Corvette":
                case "AlienGunship": case "AlienEscort": case "AlienCorvette": return 1;
                case "Frigate": case "Monitor": case "Destroyer":
                case "AlienFrigate": case "AlienMonitor": case "AlienDestroyer": case "AlienCruiser": return 2;
                case "Cruiser": case "Battlecruiser": case "Battleship":
                case "AlienBattlecruiser": case "AlienBattleship": case "AlienLancer": return 3;
                case "Lancer": case "Dreadnought": case "AlienDreadnought":
                case "AlienTitan": case "AlienAssaultCarrier": return 4;
                case "Titan": case "AlienMothership": return 5;
                default: return 0; // Specialized fighters and unknown future hulls.
            }
        }

        public static bool Applies(string hull)
        {
            return ShipTier(hull) >= 4;
        }

        public static int NoseGrade(string hull) { return ShipTier(hull) == 5 ? 5 : 4; }

        public static bool IsSize5(string weapon)
        {
            return weapon == "SpinalSiegeCoilerMk1" || weapon == "SpinalSiegeCoilerMk2" ||
                weapon == "SpinalSiegeCoilerMk3" || weapon == "AlienRelativisticParticleCannon" ||
                weapon == "Alien1024cmGraserCannon";
        }

        public static int CapacitySize(string weapon, int nativeSize) { return IsSize5(weapon) ? 5 : Math.Max(1, nativeSize); }

        public static bool WeaponAllowed(string hull, string weapon, string mount, bool hullWeapon)
        {
            if (IsSize5(weapon)) return !hullWeapon && ShipTier(hull) == 5 && mount == "FourNose";
            if (!Applies(hull)) return true;
            return hullWeapon ? mount == "FourHull" : NoseGrade(hull) == 4 && mount == "FourNose";
        }

        public static string StandardEquivalent(string name)
        {
            if (name == "AlienRelativisticParticleCannon") return "AlienSpinalParticleCannon";
            if (name == "Alien1024cmGraserCannon") return "Alien1024cmXaserCannon";
            return name != null && Regex.IsMatch(name, @"^SpinalSiegeCoilerMk[123]$") ? name.Replace("SiegeCoiler", "Coiler") : null;
        }

        public static string Size5Equivalent(string name)
        {
            if (IsSize5(name)) return name;
            if (name == "AlienSpinalParticleCannon") return "AlienRelativisticParticleCannon";
            if (name == "Alien1024cmXaserCannon") return "Alien1024cmGraserCannon";
            return name != null && Regex.IsMatch(name, @"^SpinalCoilerMk[123]$") ? name.Replace("Coiler", "SiegeCoiler") : null;
        }

        public static bool HadCompactHull(string hull, int schema)
        {
            return schema >= 3 && Applies(hull) ||
                schema >= 2 && (hull == "Dreadnought" || hull == "AlienDreadnought") ||
                schema >= 1 && (hull == "Titan" || hull == "AlienTitan" || hull == "AlienMothership");
        }

        public static bool HadCompactNose(string hull, int schema)
        {
            return schema >= 3 && Applies(hull) || schema >= 2 &&
                (hull == "Dreadnought" || hull == "AlienDreadnought" || hull == "Titan" || hull == "AlienTitan" || hull == "AlienMothership");
        }

        public static string NoseEquivalent(string name)
        {
            if (name == null) return null;
            if (Regex.IsMatch(name, @"^(Light|Heavy)?RailCannonMk[123]$"))
                return "SpinalRailgun" + name.Substring(name.Length - 3);
            if (Regex.IsMatch(name, @"^(Light|Heavy)?CoilCannonMk[123]$"))
                return "SpinalCoiler" + name.Substring(name.Length - 3);
            if (Regex.IsMatch(name, @"^(Light|Heavy)?SiegeCoil(er|Cannon)Mk[123]$"))
                return "SpinalSiegeCoiler" + name.Substring(name.Length - 3);
            if (Regex.IsMatch(name, @"^(Advanced|Gen3)?Alien(Light|Heavy)?MagCannon$"))
                return Regex.Replace(name, @"Alien(Light|Heavy)?MagCannon$", "AlienSpinalMagCannon");
            if (Regex.IsMatch(name, @"^\d+cm.*Cannon$")) return Regex.Replace(name, @"^\d+cm", "960cm");
            if (Regex.IsMatch(name, @"^Alien\d+cm.*Cannon$")) return Regex.Replace(name, @"^Alien\d+cm", "Alien1024cm");
            if (Regex.IsMatch(name, @"^PlasmaCannonMk[123]$")) return "Heavy" + name;
            if (name == "AlienPlasmaCannon") return "AlienHeavyPlasmaCannon";
            foreach (string family in new[] { "ElectronLance", "IonCannon", "ParticleLance", "AntimatterParticleCannon" })
                if (name == family || name == "Light" + family || name == "Heavy" + family) return "Spinal" + family;
            if (name == "AlienParticleCannon" || name == "AlienHeavyParticleCannon") return "AlienSpinalParticleCannon";
            return null;
        }

        // Coordinates use the native designer's half-height rows.
        public static IList<UtilityGridCell> LegacyOffsets(string mount)
        {
            switch (mount)
            {
                case "HalfHull":
                case "OneHull": return UtilityFootprintMath.GetOffsets(UtilityFootprintKind.Single);
                case "TwoHullHoriz": return UtilityFootprintMath.GetOffsets(UtilityFootprintKind.TwoHorizontal);
                case "TwoHullVert": return UtilityFootprintMath.GetOffsets(UtilityFootprintKind.TwoVertical);
                case "FourHull": return UtilityFootprintMath.GetOffsets(UtilityFootprintKind.Four);
                case "ThreeHullHoriz": return new[] { new UtilityGridCell(0, 0), new UtilityGridCell(1, 0), new UtilityGridCell(2, 0) };
                default: throw new ArgumentException("Unsupported legacy hull mount: " + mount);
            }
        }

        public static string HeavyEquivalent(string name)
        {
            if (name == null) return null;
            if (Regex.IsMatch(name, @"^(Light)?(Railgun|Coilgun)BatteryMk[123]$"))
                return "Heavy" + Regex.Replace(name, "^Light", "");
            if (Regex.IsMatch(name, @"^(Advanced|Gen3)?Alien(Light)?MagBattery$"))
                return name.Replace("LightMagBattery", "HeavyMagBattery").Replace("AlienMagBattery", "AlienHeavyMagBattery");
            if (Regex.IsMatch(name, @"^(60|120)cm.*Battery$"))
                return Regex.Replace(name, @"^\d+cm", "360cm");
            if (Regex.IsMatch(name, @"^Alien(64|128)cm.*Battery$"))
                return Regex.Replace(name, @"^Alien\d+cm", "Alien384cm");
            if (Regex.IsMatch(name, @"^PlasmaBatteryMk[123]$")) return "Heavy" + name;
            if (name == "AlienPlasmaBattery") return "AlienHeavyPlasmaBattery";
            foreach (string family in new[] { "E-BeamBattery", "IonBattery", "ParticleBeamBattery", "AntimatterPointDefenseBattery" })
                if (name == family || name == "Light" + family) return "Heavy" + family;
            return null;
        }

        public static int FallbackRank(string name)
        {
            int tier = name.EndsWith("Mk3", StringComparison.Ordinal) ? 3 : name.EndsWith("Mk2", StringComparison.Ordinal) ? 2 : 1;
            if (name.Contains("Coilgun") || name.Contains("Coiler") || name.Contains("CoilCannon")) return 600 + tier;
            if (name.Contains("Railgun")) return 500 + tier;
            if (name.Contains("MagBattery") || name.Contains("MagCannon")) return 600 + (name.StartsWith("Gen3", StringComparison.Ordinal) ? 3 : name.StartsWith("Advanced", StringComparison.Ordinal) ? 2 : 1);
            if (name.Contains("Plasma")) return 400 + tier;
            if (name.Contains("Phaser") || name.Contains("Xaser")) return 330;
            if (name.Contains("ArcLaser") || name.Contains("Violet")) return 320;
            if (name.Contains("Laser")) return 310;
            return 200;
        }

        public static int ConvertedAmmo(int remaining, int oldMaximum, int newMaximum)
        {
            if (newMaximum <= 0) return 0;
            if (oldMaximum <= 0) return 0; // Missing/corrupt state must never refill ammunition.
            double fraction = Math.Max(0, Math.Min(1, (double)remaining / oldMaximum));
            return (int)Math.Floor(fraction * newMaximum + 1e-9);
        }
    }
}
