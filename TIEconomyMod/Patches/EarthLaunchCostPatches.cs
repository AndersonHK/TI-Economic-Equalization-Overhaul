using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(
        typeof(TISpaceObjectState),
        nameof(TISpaceObjectState.GenericTransferBoostFromEarthSurface))]
    internal static class GenericEarthLaunchCostPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState faction,
            TIGameState destination,
            float mass_tons,
            ref double __result)
        {
            if (!Main.enabled || Main.settings == null || !Main.settings.enabled)
            {
                return true;
            }

            __result = EarthLaunchCost.CalculateBoost(
                faction,
                destination,
                mass_tons);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TIHabModuleState),
        nameof(TIHabModuleState.DecommissionModuleCost))]
    internal static class HabCrewEarthLaunchCostPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TIHabModuleState __instance,
            ref TIResourcesCost __result)
        {
            if (!Main.enabled ||
                Main.settings == null ||
                !Main.settings.enabled ||
                __instance == null ||
                __result == null ||
                __instance.crew <= 0 ||
                __instance.DecommissionDuration_days() <= 0f)
            {
                return;
            }

            TIGameState destination = __instance.hab.IsBase
                ? __instance.hab.ref_habSite.ref_gameState
                : __instance.hab.ref_orbit.ref_gameState;
            ReplaceBoost(
                __result,
                (float)EarthLaunchCost.CalculateBoost(
                    __instance.ref_faction,
                    destination,
                    (float)__instance.crew *
                        TemplateManager.global.scuttlePerCrewMassCost));
        }

        internal static void ReplaceBoost(
            TIResourcesCost cost,
            float replacement)
        {
            float existing = cost.GetSingleCostValue(FactionResource.Boost);
            cost.AddCost(FactionResource.Boost, replacement - existing);
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipState),
        nameof(TISpaceShipState.ScuttleCost))]
    internal static class ShipCrewEarthLaunchCostPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TISpaceShipState __instance,
            ref TIResourcesCost __result)
        {
            if (!Main.enabled ||
                Main.settings == null ||
                !Main.settings.enabled ||
                __instance == null ||
                __instance.ref_fleet == null ||
                __result == null)
            {
                return;
            }
            if (__instance.ref_fleet.dockedAtHab &&
                __instance.ref_hab != null &&
                __instance.ref_hab.faction == __instance.faction)
            {
                return;
            }

            IEnumerable<TISpaceShipState> otherShips =
                __instance.ref_fleet.ships == null
                    ? Enumerable.Empty<TISpaceShipState>()
                    : __instance.ref_fleet.ships.Where(ship =>
                        !ReferenceEquals(ship, __instance));
            int retainedCrew = (int)(otherShips.Sum(ship =>
                ship.template.crewBillets) * 0.25f);
            int crewToMove = __instance.template.crewBillets - retainedCrew;
            if (crewToMove <= 0)
            {
                return;
            }

            TIGameState destination = __instance.ref_fleet.landed
                ? __instance.ref_habSite.ref_gameState
                : __instance.ref_orbit.ref_gameState;
            HabCrewEarthLaunchCostPatch.ReplaceBoost(
                __result,
                (float)EarthLaunchCost.CalculateBoost(
                    __instance.ref_faction,
                    destination,
                    crewToMove *
                        TemplateManager.global.scuttlePerCrewMassCost));
        }
    }
}
