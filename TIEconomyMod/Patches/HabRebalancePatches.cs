using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;

namespace TIEconomyMod.Patches
{
    internal static class HabConstructionRebalance
    {
        internal static bool IsRebalanced(TIHabModuleTemplate template)
        {
            if (template == null ||
                template.tier < 1 ||
                template.tier > 3 ||
                template.noBuild ||
                template.destroyed ||
                template.alienModule ||
                template.dataName.StartsWith("Alien", StringComparison.Ordinal))
            {
                return false;
            }

            return HabRebalanceMath.HasRebalancedMaterialFraction(
                MaterialFraction(template.weightedBuildMaterials));
        }

        internal static float MaterialFraction(ResourceCostBuilder materials)
        {
            return materials.water +
                materials.volatiles +
                materials.metals +
                materials.nobleMetals +
                materials.fissiles +
                materials.antimatter +
                materials.exotics;
        }

        internal static TISpaceBodyState ResolveSpaceBody(TIGameState destination)
        {
            TISpaceBodyState spaceBody = destination.ref_spaceBody;
            if (destination.isHabSiteState)
            {
                spaceBody = destination.ref_habSite.ref_spaceBody;
            }
            else if (destination.isHabState && destination.ref_hab.IsBase)
            {
                spaceBody = destination.ref_hab.habSite.ref_spaceBody;
            }

            if (spaceBody == null)
            {
                spaceBody = destination.ref_naturalSpaceObject
                    .GetSunOrbitingRelatedObject.ref_spaceBody;
            }

            return spaceBody;
        }

        internal static float MandatoryEarthMass(
            TIHabModuleTemplate template,
            TISpaceBodyState spaceBody,
            TIFactionState faction,
            TIGameState destination,
            float rateMultiplier)
        {
            float nominalMass = template.Mass_tons(
                1f,
                spaceBody,
                destination.ref_naturalSpaceObject,
                faction);
            return HabRebalanceMath.MandatoryEarthMass(
                nominalMass,
                MaterialFraction(template.weightedBuildMaterials),
                rateMultiplier);
        }
    }

    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.BoostCostFromEarth))]
    internal static class HabBoostCostFromEarthPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TIHabModuleTemplate __instance,
            TISpaceBodyState spaceBody,
            TIFactionState faction,
            TIGameState destination,
            float rateMultiplier,
            ref float __result)
        {
            if (!HabConstructionRebalance.IsRebalanced(__instance))
            {
                return;
            }

            float earthMass = HabConstructionRebalance.MandatoryEarthMass(
                __instance,
                spaceBody,
                faction,
                destination,
                rateMultiplier);
            __result += (float)TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                faction,
                destination,
                earthMass);
        }
    }

    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.CostFromSpace))]
    internal static class HabCostFromSpacePatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TIHabModuleTemplate __instance,
            TIFactionState faction,
            TIGameState destinationState,
            bool isUpgrade,
            ref TIResourcesCost __result)
        {
            if (__result == null || !HabConstructionRebalance.IsRebalanced(__instance))
            {
                return;
            }

            float existingBoost = __result.GetSingleCostValue(FactionResource.Boost);
            TISpaceBodyState spaceBody =
                HabConstructionRebalance.ResolveSpaceBody(destinationState);
            float earthMass = HabConstructionRebalance.MandatoryEarthMass(
                __instance,
                spaceBody,
                faction,
                destinationState,
                HabRebalanceMath.ConstructionRate(isUpgrade));
            float mandatoryBoost =
                (float)TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                    faction,
                    destinationState,
                    earthMass);
            __result.AddCost(FactionResource.Boost, mandatoryBoost, false);

            if (HabRebalanceMath.NeedsEarthTransferDelay(existingBoost))
            {
                float transferDays =
                    TISpaceObjectState.GenericTransferTimeFromEarthsSurface_d(
                        faction,
                        destinationState);
                transferDays += TIEffectsState.SumEffectsModifiers(
                    Context.GenericModuleTransferTime,
                    faction,
                    transferDays,
                    null);
                __result.AddToCompletionTime_Days(transferDays);
            }
        }
    }

    internal static class HabStationSectorRebalance
    {
        internal static void ReconcileTierOneStation(TIHabState hab)
        {
            if (hab == null ||
                !hab.IsStation ||
                hab.tier != 1 ||
                hab.IsAlien() ||
                hab.sectors == null ||
                hab.sectors.Count <= 2)
            {
                return;
            }

            TISectorState coreSector = hab.sectors[0];
            TISectorState secondSector = hab.sectors[2];
            if (coreSector.faction != null && !secondSector.active)
            {
                secondSector.SetFaction(coreSector.faction);
            }
        }
    }

    [HarmonyPatch(typeof(TIHabState), nameof(TIHabState.InitializeNewHab))]
    internal static class InitializeNewHabSectorPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(TIHabState __instance)
        {
            HabStationSectorRebalance.ReconcileTierOneStation(__instance);
        }
    }

    [HarmonyPatch(typeof(TIHabState), nameof(TIHabState.PostEverythingSaveRepair_8))]
    internal static class RepairExistingHabSectorPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(TIHabState __instance)
        {
            HabStationSectorRebalance.ReconcileTierOneStation(__instance);
        }
    }
}
