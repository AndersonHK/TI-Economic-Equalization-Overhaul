using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.CanFoundHabFromHabAtLocation))]
    internal static class SystemAgnosticHabFoundingAvailabilityPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            ref bool __result)
        {
            if (__instance.IsAlienFaction)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.MaxTierCanFoundAtLocation))]
    internal static class SystemAgnosticHabFoundingTierPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState __instance,
            ref int __result)
        {
            if (__instance.IsAlienFaction)
            {
                return true;
            }

            __result = 3;
            return false;
        }
    }

    [HarmonyPatch(typeof(LaunchProbeOperation), nameof(LaunchProbeOperation.SpaceCost))]
    internal static class ProbeManufacturingCostPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            LaunchProbeOperation __instance,
            TIFactionState faction,
            TIGameState target,
            ref TIResourcesCost __result)
        {
            TIHabSiteState site = ProbeSurveyRuntime.ResolveSite(
                faction,
                target);
            if (site == null)
            {
                __result = new TIResourcesCost();
                return false;
            }

            float payloadMass = ProbePayloadMass(site.parentBody);
            float conversion = TemplateManager.global.spaceResourceToTons;
            ResourceCostBuilder materials = new ResourceCostBuilder
            {
                metals = payloadMass * conversion *
                    TemplateManager.global.probeMetalsPayloadMassFraction,
                volatiles = payloadMass * conversion *
                    TemplateManager.global.probeVolatilesPayloadMassFraction,
                nobleMetals = payloadMass * conversion *
                    TemplateManager.global.probeNoblesPayloadMassFraction,
                fissiles = payloadMass * conversion *
                    TemplateManager.global.probeFissilesPayloadMassFraction
            };
            HabFreightQuote quote = HabLogistics.Quote(
                faction,
                site,
                1,
                materials,
                true,
                false,
                true);
            if (quote == null)
            {
                __result = new TIResourcesCost();
                return false;
            }

            TIResourcesCost cost = HabConstructionCostRewrite.ToResourcesCost(
                faction,
                site,
                quote);
            float spaceDays = HabLogistics.EffectiveDeliveryTime(
                faction,
                quote.Route.TrajectoryTime_days,
                LogisticsDeliveryKind.Probe);
            float earthDays = 0f;
            if (quote.EarthFreightMass_tons > 0f)
            {
                earthDays = HabLogistics.EarthDeliveryTime(
                    faction,
                    site,
                    LogisticsDeliveryKind.Probe);
            }

            float deliveryDays = Math.Max(spaceDays, earthDays);
            cost.SetCompletionTime_Days(
                TemplateManager.global.probeConstructionTime_d +
                deliveryDays +
                ProbeSurveyRuntime.ScanDuration_days(faction, site));
            __result = cost;
            return false;
        }

        internal static float ProbePayloadMass(TISpaceBodyState body)
        {
            return ProbeSurveyRuntime.PayloadMass_tons;
        }
    }

    [HarmonyPatch(typeof(LaunchProbeOperation), nameof(LaunchProbeOperation.EarthCost))]
    internal static class ProbeEarthCostPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIFactionState faction,
            TIGameState target,
            ref TIResourcesCost __result)
        {
            TIHabSiteState site = ProbeSurveyRuntime.ResolveSite(
                faction,
                target);
            if (site == null)
            {
                __result = new TIResourcesCost();
                return false;
            }

            float payloadMass = ProbeSurveyRuntime.PayloadMass_tons;
            float conversion = TemplateManager.global.spaceResourceToTons;
            float materialUnits = payloadMass * conversion;
            TIResourcesCost cost = new TIResourcesCost();
            cost.ConstructCost(
                new ResourceValue
                {
                    resource = FactionResource.Boost,
                    value = (float)EarthLaunchCost.CalculateBoost(
                        faction,
                        site,
                        payloadMass)
                },
                new ResourceValue
                {
                    resource = FactionResource.Money,
                    value = materialUnits *
                        TemplateManager.global.probeMetalsPayloadMassFraction *
                        TIGlobalValuesState.GlobalValues
                            .GetPurchaseResourceMarketValue(
                                FactionResource.Metals) +
                        materialUnits *
                        TemplateManager.global
                            .probeVolatilesPayloadMassFraction *
                        TIGlobalValuesState.GlobalValues
                            .GetPurchaseResourceMarketValue(
                                FactionResource.Volatiles) +
                        materialUnits *
                        TemplateManager.global.probeFissilesPayloadMassFraction *
                        TIGlobalValuesState.GlobalValues
                            .GetPurchaseResourceMarketValue(
                                FactionResource.Fissiles) +
                        materialUnits *
                        TemplateManager.global.probeNoblesPayloadMassFraction *
                        TIGlobalValuesState.GlobalValues
                            .GetPurchaseResourceMarketValue(
                                FactionResource.NobleMetals)
                });
            float transferDays = TISpaceObjectState
                .GenericTransferTimeFromEarthsSurface_d(faction, site);
            transferDays += TIEffectsState.SumEffectsModifiers(
                Context.ProbeTransferTime,
                faction,
                transferDays);
            cost.SetCompletionTime_Days(
                TemplateManager.global.probeConstructionTime_d +
                transferDays +
                ProbeSurveyRuntime.ScanDuration_days(faction, site));
            __result = cost;
            return false;
        }
    }

    [HarmonyPatch(typeof(LaunchProbeOperation), nameof(LaunchProbeOperation.ResourceCostOptions))]
    internal static class ProbeManufacturingOptionsPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            LaunchProbeOperation __instance,
            TIFactionState faction,
            TIGameState target,
            TIGameState actor,
            bool checkCanAfford,
            ref List<TIResourcesCost> __result)
        {
            List<TIResourcesCost> options = new List<TIResourcesCost>();
            TIResourcesCost space = __instance.SpaceCost(faction, target);
            if (space.anyDebit &&
                (!checkCanAfford || space.CanAfford(faction)))
            {
                options.Add(space);
            }

            TIResourcesCost earth = __instance.EarthCost(faction, target);
            if (!checkCanAfford || earth.CanAfford(faction))
            {
                options.Add(earth);
            }

            if (options.Count > 1)
            {
                options = options
                    .OrderBy(option => option.completionTime_days)
                    .ThenBy(option => option.GetSingleCostValue(
                        FactionResource.Boost))
                    .ToList();
            }

            __result = options;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(AIEvaluators),
        nameof(AIEvaluators.EvaluateHabModule_Strategy))]
    internal static class HabLogisticsAiPriorityPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(
            TIFactionState faction,
            TIGameState location,
            TIHabModuleTemplate moduleTemplate,
            HabPreferences preferences,
            IEnumerable<TIHabModuleTemplate> prospectiveModules,
            ref float __result)
        {
            if (faction == null ||
                faction.IsAlienFaction ||
                location == null ||
                moduleTemplate == null ||
                preferences == null)
            {
                return;
            }

            bool candidateFactory = moduleTemplate.constructionModule;
            bool candidateDock = moduleTemplate.allowsShipConstruction;
            if (!candidateFactory && !candidateDock)
            {
                return;
            }

            TISpaceBodyState system = location.ref_system;
            TIHabState hab = location.ref_hab;
            if (!IsMajorColonizedSystem(system, faction) ||
                hab == null ||
                hab.faction != faction ||
                SystemHasCommittedPair(system, faction))
            {
                return;
            }

            List<TIHabModuleState> present = hab.PresentModules();
            IEnumerable<TIHabModuleTemplate> planned = prospectiveModules ??
                Enumerable.Empty<TIHabModuleTemplate>();
            bool habHasFactory = present.Any(module =>
                    module.moduleTemplate.constructionModule) ||
                planned.Any(template => template.constructionModule);
            bool habHasDock = present.Any(module =>
                    module.moduleTemplate.allowsShipConstruction) ||
                planned.Any(template => template.allowsShipConstruction);
            bool completesPair =
                (candidateFactory && habHasDock) ||
                (candidateDock && habHasFactory);

            __result += HabRebalanceMath.LogisticsPairPriority(
                false,
                completesPair,
                system.isEarth,
                preferences.Weight);
        }

        private static bool IsMajorColonizedSystem(
            TISpaceBodyState system,
            TIFactionState faction)
        {
            if (system == null || system.isSun)
            {
                return false;
            }

            int ownedHabs = system.habsInSystem.Count(hab =>
                hab.faction == faction);
            return ownedHabs > 0 &&
                (system.isEarth ||
                 system.objectType == SpaceObjectType.Planet ||
                 system.objectType == SpaceObjectType.DwarfPlanet ||
                 ownedHabs >= 2);
        }

        private static bool SystemHasCommittedPair(
            TISpaceBodyState system,
            TIFactionState faction)
        {
            return system.habsInSystem.Any(hab =>
            {
                if (hab.faction != faction)
                {
                    return false;
                }

                List<TIHabModuleState> modules = hab.PresentModules();
                return modules.Any(module =>
                        module.moduleTemplate.constructionModule) &&
                    modules.Any(module =>
                        module.moduleTemplate.allowsShipConstruction);
            });
        }
    }

    [HarmonyPatch]
    internal static class HabLogisticsModuleInvalidationPatch
    {
        private static readonly HashSet<string> RelevantMethods =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "SetCompletedModule",
                "InitiateConstructModule",
                "CompleteConstruction",
                "SetPowerStatus",
                "DestroyModule",
                "CancelDecommissionModule",
                "CompleteDecommissionModule"
            };

        [HarmonyTargetMethods]
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(TIHabModuleState))
                .Where(method => RelevantMethods.Contains(method.Name));
        }

        [HarmonyPostfix]
        internal static void Postfix()
        {
            HabLogistics.InvalidateTopology();
        }
    }

    [HarmonyPatch]
    internal static class HabLogisticsHabInvalidationPatch
    {
        private static readonly HashSet<string> RelevantMethods =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "InitializeNewHab",
                "SetFaction",
                "BeginDecommissionModule",
                "CompleteDecommissionModule",
                "InitiateModuleConstruction",
                "CompleteModuleConstruction"
            };

        [HarmonyTargetMethods]
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(TIHabState))
                .Where(method => RelevantMethods.Contains(method.Name));
        }

        [HarmonyPostfix]
        internal static void Postfix()
        {
            HabLogistics.InvalidateTopology();
        }
    }
}
