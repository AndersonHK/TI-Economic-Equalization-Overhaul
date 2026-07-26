using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TINationState), "AbsorbNation")]
    public static class MilitaryMergerPatch
    {
        public sealed class Snapshot
        {
            public float absorbingTech;
            public float joiningTech;
            public int absorbingArmies;
            public int joiningArmies;
            public int absorbingNavies;
            public int joiningNavies;
            public double absorbingGdp;
            public double joiningGdp;
        }

        [HarmonyPrefix]
        public static void Prefix(TINationState __instance, TINationState joiningNationState, ref Snapshot __state)
        {
            NationalMergerSettings settings = Main.settings.nationalMergers;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.militaryEnabled)
            {
                return;
            }

            // AbsorbNation transfers regions and deletes the joining nation's armies, so preserve
            // both pre-merger force structures and economies. A navy counts as one extra army by
            // default because it adds expensive blue-water logistics to its attached land force.
            __state = new Snapshot
            {
                absorbingTech = __instance.militaryTechLevel,
                joiningTech = joiningNationState.militaryTechLevel,
                absorbingArmies = __instance.armies.Count,
                joiningArmies = joiningNationState.armies.Count,
                absorbingNavies = __instance.numNavies,
                joiningNavies = joiningNationState.numNavies,
                absorbingGdp = __instance.GDP,
                joiningGdp = joiningNationState.GDP
            };
        }

        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, Snapshot __state)
        {
            if (__state == null)
            {
                return;
            }

            NationalMergerSettings settings = Main.settings.nationalMergers;
            // Half of merged miltech follows the forces that must integrate doctrine and equipment;
            // half follows the economies that sustain future modernization. At defaults, a 4-tech
            // nation with 3 army-equivalents and $300B merging a 6-tech nation with 1 equivalent
            // and $100B gives 4.5 from both averages, hence 4.5 overall. With no forces, GDP alone
            // applies; with no valid GDP, forces alone apply; with neither, use the simple mean.
            double forceA = __state.absorbingArmies + settings.navyArmyEquivalent * __state.absorbingNavies;
            double forceB = __state.joiningArmies + settings.navyArmyEquivalent * __state.joiningNavies;
            double totalForce = forceA + forceB;
            double totalGdp = __state.absorbingGdp + __state.joiningGdp;
            bool validForce = totalForce > 0d && !double.IsNaN(totalForce) && !double.IsInfinity(totalForce);
            bool validGdp = __state.absorbingGdp >= 0d && __state.joiningGdp >= 0d &&
                totalGdp > 0d && !double.IsNaN(totalGdp) && !double.IsInfinity(totalGdp);
            double forceAverage = validForce
                ? (__state.absorbingTech * forceA + __state.joiningTech * forceB) / totalForce
                : 0d;
            double gdpAverage = validGdp
                ? (__state.absorbingTech * __state.absorbingGdp +
                    __state.joiningTech * __state.joiningGdp) / totalGdp
                : 0d;
            double merged = validForce && validGdp
                ? settings.militaryForceShare * forceAverage +
                    (1d - settings.militaryForceShare) * gdpAverage
                : validForce ? forceAverage : validGdp ? gdpAverage :
                    (__state.absorbingTech + __state.joiningTech) / 2d;

            if (double.IsNaN(merged) || double.IsInfinity(merged))
            {
                Main.Warn("National merger military technology was invalid; retaining vanilla.");
                return;
            }
            __instance.AddToMilitaryTechLevel((float)merged - __instance.militaryTechLevel);
        }
    }

    [HarmonyPatch(typeof(TINationState), "AbsorbNation")]
    public static class InequalityMergerPatch
    {
        public sealed class Snapshot
        {
            public float absorbingPopulation;
            public float joiningPopulation;
            public float absorbingIncome;
            public float joiningIncome;
            public float absorbingInequality;
            public float joiningInequality;
        }

        [HarmonyPrefix]
        public static void Prefix(TINationState __instance, TINationState joiningNationState, ref Snapshot __state)
        {
            NationalMergerSettings settings = Main.settings.nationalMergers;
            if (!Main.FeatureEnabled(settings.enabled) || !settings.inequalityEnabled)
            {
                return;
            }

            // Save both income distributions before TI transfers regions. Population supplies each
            // distribution's size, GDP/c its center, and Inequality its approximate width.
            __state = new Snapshot
            {
                absorbingPopulation = __instance.population,
                joiningPopulation = joiningNationState.population,
                absorbingIncome = __instance.perCapitaGDP,
                joiningIncome = joiningNationState.perCapitaGDP,
                absorbingInequality = __instance.inequality,
                joiningInequality = joiningNationState.inequality
            };
        }

        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, Snapshot __state)
        {
            if (__state == null)
            {
                return;
            }

            NationalMergerSettings settings = Main.settings.nationalMergers;
            double totalPopulation = __state.absorbingPopulation + __state.joiningPopulation;
            if (__state.absorbingPopulation <= 0f || __state.joiningPopulation <= 0f ||
                totalPopulation <= 1d || double.IsNaN(totalPopulation) || double.IsInfinity(totalPopulation))
            {
                Main.Warn("National merger population was invalid; retaining vanilla Inequality.");
                return;
            }

            // Treat TI's configured 1-9 rating as a 0-1 Gini. Undo each nation's finite-sample
            // correction, then approximate the cross-population income gap as the smooth combination
            // of different national means and the two distributions' existing widths. Identical
            // countries retain their Gini; separated income modes raise it. At Inequality 1,
            // $1M versus $1 gives ~9 for one person each after the N/(N-1) correction, but ~5 for
            // one billion each. Germany/France-like inputs give ~3.27; US/Egypt-like inputs ~5.23.
            double range = settings.inequalityMaximum - settings.inequalityMinimum;
            double shareA = __state.absorbingPopulation / totalPopulation;
            double shareB = __state.joiningPopulation / totalPopulation;
            double incomeA = Math.Max(settings.minimumPerCapitaGdp, __state.absorbingIncome);
            double incomeB = Math.Max(settings.minimumPerCapitaGdp, __state.joiningIncome);
            double correctedGiniA = Math.Max(0d, Math.Min(1d,
                (__state.absorbingInequality - settings.inequalityMinimum) / range));
            double correctedGiniB = Math.Max(0d, Math.Min(1d,
                (__state.joiningInequality - settings.inequalityMinimum) / range));
            double rawGiniA = correctedGiniA *
                (__state.absorbingPopulation - 1d) / __state.absorbingPopulation;
            double rawGiniB = correctedGiniB *
                (__state.joiningPopulation - 1d) / __state.joiningPopulation;
            double averageIncome = shareA * incomeA + shareB * incomeB;
            double crossDifference = Math.Sqrt(
                Math.Pow(incomeA - incomeB, 2d) +
                Math.Pow(incomeA * correctedGiniA + incomeB * correctedGiniB, 2d));
            double mergedGini =
                (shareA * shareA * incomeA * rawGiniA +
                 shareB * shareB * incomeB * rawGiniB +
                 shareA * shareB * crossDifference) / averageIncome *
                totalPopulation / (totalPopulation - 1d);
            double merged = settings.inequalityMinimum + range * mergedGini;
            merged = Math.Max(settings.inequalityMinimum + settings.inequalityBoundaryEpsilon,
                Math.Min(settings.inequalityMaximum - settings.inequalityBoundaryEpsilon, merged));

            if (double.IsNaN(merged) || double.IsInfinity(merged))
            {
                Main.Warn("National merger Inequality was invalid; retaining vanilla.");
                return;
            }
            __instance.AddToInequality((float)merged - __instance.inequality,
                TINationState.InequalityChangeReason.InqReason_Annexation);
        }
    }
}
