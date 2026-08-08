using System;

namespace TIEconomyMod
{
    public static class WeaponCadenceMath
    {
        public const double CheckInterval_s = 0.05;
        public const int MaximumChecksPerUpdate = 20;

        public static int AccumulateChecks(
            ref double accumulated_s, double elapsed_s)
        {
            if (double.IsNaN(elapsed_s) ||
                double.IsInfinity(elapsed_s) ||
                elapsed_s <= 0.0)
            {
                return 0;
            }

            accumulated_s += elapsed_s;
            if (accumulated_s + 1e-9 < CheckInterval_s)
            {
                return 0;
            }

            int checks = (int)Math.Floor(
                (accumulated_s + 1e-9) / CheckInterval_s);
            if (checks > MaximumChecksPerUpdate)
            {
                checks = MaximumChecksPerUpdate;
                accumulated_s = 0.0;
                return checks;
            }

            accumulated_s -= checks * CheckInterval_s;
            if (accumulated_s < 0.0)
            {
                accumulated_s = 0.0;
            }
            return checks;
        }

        public static double OldestCheckOffset_s(
            double remainder_s, int checks)
        {
            if (checks <= 0)
            {
                return 0.0;
            }
            return Math.Max(0.0, remainder_s) +
                (checks - 1) * CheckInterval_s;
        }
    }
}
