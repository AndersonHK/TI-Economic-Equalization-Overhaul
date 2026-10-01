using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TransferRegionsOption), nameof(TransferRegionsOption.GetPossibleTargets))]
    internal static class AlienTerritoryDemandTargetsPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(TINationState __0, ref IList<TIGameState> __result)
        {
            if (!Main.enabled || __0 == null || !__0.alienNation || __result == null)
            {
                return;
            }

            // Preserve the native eligibility checks; only narrow Servant targets.
            __result = __result.Where(target =>
                !LacksFullServantControl(target.ref_nation)).ToList();
        }

        internal static bool LacksFullServantControl(TINationState nation)
        {
            if (nation == null || nation.controlPoints == null ||
                nation.controlPoints.Count == 0)
            {
                return true;
            }

            TIFactionState executive = nation.executiveFaction;
            return executive != null && executive.IsAlienProxy &&
                nation.controlPoints.Any(point => point == null || point.faction != executive);
        }
    }
}
