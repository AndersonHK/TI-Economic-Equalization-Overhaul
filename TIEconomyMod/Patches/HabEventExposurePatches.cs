using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch]
    internal static class AmbientHabHazardWeightPatch
    {
        private const string SelectorMethodName =
            "<NarrativeEventsMonthlyUpdate>b__241_2";

        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod()
        {
            Type compilerClosure = AccessTools.Inner(
                typeof(TIGlobalValuesState),
                "<>c");
            MethodInfo selector = compilerClosure == null
                ? null
                : AccessTools.Method(
                    compilerClosure,
                    SelectorMethodName,
                    new[] { typeof(KeyValuePair<string, float>) });
            if (selector == null || selector.ReturnType != typeof(float))
            {
                throw new MissingMethodException(
                    typeof(TIGlobalValuesState).FullName,
                    SelectorMethodName);
            }

            return selector;
        }

        [HarmonyPostfix]
        internal static void Postfix(
            KeyValuePair<string, float> __0,
            ref float __result)
        {
            if (!Main.enabled ||
                !HabEventExposureMath.UsesOrbitalHabExposure(__0.Key))
            {
                return;
            }

            int orbitalHabCount = GameStateManager.AllHumanFactions()
                .Where(faction => faction != null)
                .Sum(faction => faction.stations.Count);
            __result = HabEventExposureMath.AdjustSelectionWeight(
                __0.Key,
                __result,
                orbitalHabCount);
        }
    }
}
