using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIEconomyMod.Core
{
    public static class CapitalMountPolicy
    {
        public const int Schema = 1;

        public static bool Applies(string hull)
        {
            return hull == "Titan" || hull == "AlienTitan" || hull == "AlienMothership";
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
            if (name.Contains("Coilgun")) return 600 + tier;
            if (name.Contains("Railgun")) return 500 + tier;
            if (name.Contains("MagBattery")) return 600 + (name.StartsWith("Gen3", StringComparison.Ordinal) ? 3 : name.StartsWith("Advanced", StringComparison.Ordinal) ? 2 : 1);
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
