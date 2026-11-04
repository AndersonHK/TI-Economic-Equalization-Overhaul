using System;
using System.Globalization;

namespace TIEconomyMod.Core
{
    // Prepared integer-angstrom catalog. Native integer-nanometre fields retain
    // their existing meaning. Only the two known zero-rounded records override.
    public static class WavelengthPrecision
    {
        public static bool HasPrecision(string weapon, int nativeNanometres)
        {
            return nativeNanometres == 0 && (weapon == "Alien768cmGraserCannon" || weapon == "Alien1024cmGraserCannon");
        }
        public static long Angstroms(string weapon, int nativeNanometres)
        {
            return HasPrecision(weapon, nativeNanometres) ? 1L : Math.Max(0L, (long)nativeNanometres * 10L);
        }
        public static float Metres(string weapon, int nativeNanometres)
        {
            return HasPrecision(weapon, nativeNanometres) ? (float)(Angstroms(weapon, nativeNanometres) * 1e-10) : nativeNanometres / 1e9f;
        }
        public static string NanometresText(string weapon, int nativeNanometres)
        {
            return (Angstroms(weapon, nativeNanometres) / 10m).ToString("0.###", CultureInfo.CurrentCulture);
        }
    }
}
