using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    internal static class HabConstructionCostRewrite
    {
        private static readonly FactionResource[] MaterialResources =
        {
            FactionResource.Water,
            FactionResource.Volatiles,
            FactionResource.Metals,
            FactionResource.NobleMetals,
            FactionResource.Fissiles,
            FactionResource.Antimatter,
            FactionResource.Exotics
        };

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

        internal static float MaterialValue(
            ResourceCostBuilder materials,
            FactionResource resource)
        {
            switch (resource)
            {
                case FactionResource.Water:
                    return materials.water;
                case FactionResource.Volatiles:
                    return materials.volatiles;
                case FactionResource.Metals:
                    return materials.metals;
                case FactionResource.NobleMetals:
                    return materials.nobleMetals;
                case FactionResource.Fissiles:
                    return materials.fissiles;
                case FactionResource.Antimatter:
                    return materials.antimatter;
                case FactionResource.Exotics:
                    return materials.exotics;
                default:
                    return 0f;
            }
        }

        internal static void SetMaterialValue(
            ref ResourceCostBuilder materials,
            FactionResource resource,
            float value)
        {
            switch (resource)
            {
                case FactionResource.Water:
                    materials.water = value;
                    break;
                case FactionResource.Volatiles:
                    materials.volatiles = value;
                    break;
                case FactionResource.Metals:
                    materials.metals = value;
                    break;
                case FactionResource.NobleMetals:
                    materials.nobleMetals = value;
                    break;
                case FactionResource.Fissiles:
                    materials.fissiles = value;
                    break;
                case FactionResource.Antimatter:
                    materials.antimatter = value;
                    break;
                case FactionResource.Exotics:
                    materials.exotics = value;
                    break;
            }
        }

        internal static float SumMaterials(ResourceCostBuilder materials)
        {
            float total = 0f;
            foreach (FactionResource resource in MaterialResources)
            {
                total += Math.Max(0f, MaterialValue(materials, resource));
            }

            return HabRebalanceMath.RoundCost(total);
        }

        internal static void SubtractPreSupplied(
            ref ResourceCostBuilder materials,
            List<ResourceValue> preSuppliedResources)
        {
            if (preSuppliedResources == null)
            {
                return;
            }

            foreach (ResourceValue supplied in preSuppliedResources)
            {
                float current = MaterialValue(materials, supplied.resource);
                if (current <= 0f)
                {
                    continue;
                }

                SetMaterialValue(
                    ref materials,
                    supplied.resource,
                    Math.Max(0f, current - supplied.value));
            }
        }

        internal static float EarthTransferTime(
            TIFactionState faction,
            TIGameState destination)
        {
            return HabLogistics.EarthTransferTime(faction, destination);
        }

        internal static TIResourcesCost ToResourcesCost(
            TIFactionState faction,
            TIGameState destination,
            HabFreightQuote quote)
        {
            TIResourcesCost cost = new TIResourcesCost();
            float money = 0f;
            for (int index = 0;
                index < HabLogistics.MaterialResources.Length;
                index++)
            {
                FactionResource resource =
                    HabLogistics.MaterialResources[index];
                if (quote.StockpileCosts[index] > 0f)
                {
                    cost.AddCost(resource, quote.StockpileCosts[index], false);
                }

                if (quote.EarthPurchaseCosts[index] > 0f)
                {
                    money += quote.EarthPurchaseCosts[index] *
                        TIGlobalValuesState.GlobalValues
                            .GetPurchaseResourceMarketValue(resource);
                }
            }

            if (money > 0f)
            {
                cost.AddCost(FactionResource.Money, money, false);
            }

            if (quote.EarthFreightMass_tons > 0f)
            {
                cost.AddCost(
                    FactionResource.Boost,
                    HabRebalanceMath.RoundCost((float)
                        TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                            faction,
                            destination,
                            quote.EarthFreightMass_tons)),
                    false);
            }

            return cost;
        }

    }

    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.BuildMaterials))]
    internal static class HabBuildMaterialsRewritePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIHabModuleTemplate __instance,
            float irradiatedValue,
            TISpaceBodyState spaceBody,
            TINaturalSpaceObjectState naturalSpaceObject,
            TIFactionState faction,
            float multiplier,
            ref ResourceCostBuilder __result)
        {
            if (!HabConstructionCostRewrite.IsRebalanced(__instance))
            {
                return true;
            }

            float nominalMass = __instance.Mass_tons(
                1f,
                spaceBody,
                naturalSpaceObject,
                faction);
            float radiationShieldingMass = Math.Max(
                0f,
                __instance.Mass_tons(
                    irradiatedValue,
                    spaceBody,
                    naturalSpaceObject,
                    faction) - nominalMass);
            float materialWeightSum =
                HabConstructionCostRewrite.MaterialFraction(
                    __instance.weightedBuildMaterials);
            float ordinaryMaterialCost =
                HabRebalanceMath.FullMaterialMass(
                    nominalMass,
                    multiplier) *
                TemplateManager.global.spaceResourceToTons;

            ResourceCostBuilder result = new ResourceCostBuilder();
            bool useHelium3 =
                __instance.specialRules.Contains((HabModuleSpecialRule)49) &&
                faction.He3Access;
            float waterWeight = __instance.weightedBuildMaterials.water;
            float fissileWeight = __instance.weightedBuildMaterials.fissiles;
            if (useHelium3)
            {
                waterWeight += fissileWeight;
                fissileWeight = 0f;
            }

            result.water = HabRebalanceMath.NormalizeMaterialCost(
                waterWeight,
                materialWeightSum,
                ordinaryMaterialCost);
            result.volatiles = HabRebalanceMath.NormalizeMaterialCost(
                __instance.weightedBuildMaterials.volatiles,
                materialWeightSum,
                ordinaryMaterialCost);
            result.metals = HabRebalanceMath.NormalizeMaterialCost(
                __instance.weightedBuildMaterials.metals,
                materialWeightSum,
                ordinaryMaterialCost);
            result.nobleMetals = HabRebalanceMath.NormalizeMaterialCost(
                __instance.weightedBuildMaterials.nobleMetals,
                materialWeightSum,
                ordinaryMaterialCost);
            result.fissiles = HabRebalanceMath.NormalizeMaterialCost(
                fissileWeight,
                materialWeightSum,
                ordinaryMaterialCost);
            result.antimatter = HabRebalanceMath.NormalizeMaterialCost(
                __instance.weightedBuildMaterials.antimatter,
                materialWeightSum,
                ordinaryMaterialCost);
            result.exotics = HabRebalanceMath.NormalizeMaterialCost(
                __instance.weightedBuildMaterials.exotics,
                materialWeightSum,
                ordinaryMaterialCost);

            float targetOrdinaryCost =
                HabRebalanceMath.RoundCost(ordinaryMaterialCost);
            float roundingDifference =
                targetOrdinaryCost -
                HabConstructionCostRewrite.SumMaterials(result);
            result.metals = HabRebalanceMath.RoundCost(
                result.metals + roundingDifference);
            result.metals = HabRebalanceMath.RoundCost(
                result.metals +
                radiationShieldingMass *
                TemplateManager.global.spaceResourceToTons *
                multiplier);

            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.BoostCostFromEarth))]
    internal static class HabBoostCostFromEarthRewritePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIHabModuleTemplate __instance,
            float irradiatedValue,
            TISpaceBodyState spaceBody,
            TIFactionState faction,
            TIGameState destination,
            float rateMultiplier,
            List<ResourceValue> preSuppliedResources,
            ref float __result)
        {
            if (!HabConstructionCostRewrite.IsRebalanced(__instance))
            {
                return true;
            }

            ResourceCostBuilder ordinaryMaterials = __instance.BuildMaterials(
                irradiatedValue,
                spaceBody,
                destination.ref_naturalSpaceObject,
                faction,
                rateMultiplier);
            HabConstructionCostRewrite.SubtractPreSupplied(
                ref ordinaryMaterials,
                preSuppliedResources);
            float ordinaryMass =
                HabConstructionCostRewrite.SumMaterials(ordinaryMaterials) /
                TemplateManager.global.spaceResourceToTons;

            __result = HabRebalanceMath.RoundCost(
                (float)TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                    faction,
                    destination,
                    ordinaryMass));
            return false;
        }
    }

    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.CostFromSpace))]
    internal static class HabCostFromSpaceRewritePatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            TIHabModuleTemplate __instance,
            TIFactionState faction,
            TIGameState destinationState,
            bool isUpgrade,
            bool substituteBoost,
            int maxDaysToSave,
            bool dontRecalculateIncome,
            ref TIResourcesCost __result)
        {
            if (!HabConstructionCostRewrite.IsRebalanced(__instance))
            {
                return true;
            }

            float irradiatedMultiplier =
                TIUtilities.IrradiatedMultiplier(destinationState);
            float rateMultiplier =
                HabRebalanceMath.ConstructionRate(isUpgrade);
            TISpaceBodyState spaceBody =
                HabConstructionCostRewrite.ResolveSpaceBody(destinationState);
            TIHabState destinationHab =
                destinationState.isHabState ? destinationState.ref_hab : null;
            float constructionTimeModifier = destinationHab == null
                ? 1f
                : destinationHab.GetModuleConstructionTimeModifier(false, null);

            ResourceCostBuilder materials = __instance.BuildMaterials(
                irradiatedMultiplier,
                spaceBody,
                destinationState.ref_naturalSpaceObject,
                faction,
                rateMultiplier);
            HabFreightQuote quote = HabLogistics.Quote(
                faction,
                destinationState,
                __instance.tier,
                materials,
                false,
                true,
                substituteBoost && faction.IsActiveHumanFaction);
            TIResourcesCost cost = HabConstructionCostRewrite.ToResourcesCost(
                faction,
                destinationState,
                quote);

            float earthTransferTime = quote.EarthFreightMass_tons > 0f
                ? HabConstructionCostRewrite.EarthTransferTime(
                    faction,
                    destinationState)
                : 0f;
            float spaceTransferTime = 0f;
            if (!quote.Route.IsEarthFallback &&
                quote.SpacePayloadMass_tons > 0f)
            {
                spaceTransferTime = HabLogistics.EffectiveDeliveryTime(
                    faction,
                    quote.Route.TrajectoryTime_days,
                    LogisticsDeliveryKind.HabModule);
            }

            float transferTime = Mathf.Max(
                earthTransferTime,
                spaceTransferTime);
            float completionTime =
                __instance.buildTime_Days *
                TIGlobalValuesState.GetHabModuleConstructionTimeSettingsModifier(
                    faction) *
                rateMultiplier *
                constructionTimeModifier *
                faction.GetHabConstructionDurationModifier() +
                transferTime;

            if (destinationHab != null &&
                destinationHab.coreModule.underConstruction &&
                destinationHab.tier <= __instance.tier)
            {
                completionTime = Mathf.Max(
                    completionTime,
                    (float)-TITimeState.Now().DifferenceInDays(
                        new TIDateTime(
                            destinationHab.coreModule.completionDate)));
            }

            cost.SetCompletionTime_Days(completionTime);
            __result = cost;
            return false;
        }
    }

    internal static class HabSectorProgression
    {
        internal static bool HasValidTopology(TIHabState hab)
        {
            if (hab == null || hab.sectors == null || hab.sectors.Count != 5)
            {
                return false;
            }
            for (int sectorIndex = 0; sectorIndex < 5; sectorIndex++)
            {
                TISectorState sector = hab.sectors[sectorIndex];
                int count = sectorIndex == 0 ? 5 : 4;
                if (sector == null || sector.hab != hab ||
                    sector.sectorNum != sectorIndex ||
                    sector.habModules == null || sector.habModules.Count != count)
                {
                    return false;
                }
                for (int slot = 0; slot < count; slot++)
                {
                    TIHabModuleState module = sector.habModules[slot];
                    if (module == null || module.sector != sector || module.slot != slot)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static bool Eligible(TIHabState hab)
        {
            return HasValidTopology(hab) && !hab.staticHab &&
                (hab.IsStation || hab.IsBase) && hab.tier >= 1 && hab.tier <= 3 &&
                hab.coreModule.moduleTemplate != null &&
                hab.sectors[0].faction != null && !hab.IsAlien();
        }

        // Called only after initialization of income/faction services. Keep
        // module identities, construction, and foreign sector ownership intact.
        internal static bool Reconcile(TIHabState hab)
        {
            if (!HasValidTopology(hab))
            {
                Main.Warn("Hab sector progression skipped an incomplete/native-incompatible topology.");
                return false;
            }
            if (!Eligible(hab) || hab.sectors[0].faction == null)
            {
                return false;
            }
            bool changed = false;
            for (int sectorIndex = 1; sectorIndex < 5; sectorIndex++)
            {
                TISectorState sector = hab.sectors[sectorIndex];
                if (!sector.active && hab.tier >=
                    HabRebalanceMath.SectorUnlockTier(hab.IsStation, sectorIndex))
                {
                    sector.SetFaction(hab.sectors[0].faction);
                    changed = true;
                }
            }
            return changed;
        }

        internal static void ReconcileBeforeConnectors(TIHabState hab)
        {
            Reconcile(hab);
        }

        internal static void ReconcileAndRefresh(TIHabState hab)
        {
            if (Reconcile(hab))
            {
                hab.UpdateAllModuleConnectors();
            }
        }

        internal static void ActivateUpgradeSector(TISectorState sector, TIFactionState faction)
        {
            // Native core upgrades reassign their newly unlocked sectors.
            // Those sectors can already be owned under the earlier unlocks.
            if (!Eligible(sector.hab) || !sector.active)
            {
                sector.SetFaction(faction);
            }
        }

        internal static int ConnectorTierRequirement(
            TIHabState hab, int sectorIndex, int nativeRequirement)
        {
            return Eligible(hab) && sectorIndex >= 1 && sectorIndex <= 4 &&
                hab.sectors[sectorIndex].active
                ? HabRebalanceMath.SectorUnlockTier(hab.IsStation, sectorIndex)
                : nativeRequirement;
        }
    }

    [HarmonyPatch(
        typeof(TISectorState),
        nameof(TISectorState.UpdateModuleConnectorMap),
        new Type[] { typeof(TIHabState), typeof(TIHabModuleState) })]
    internal static class HabSectorConnectorMapPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo tierGetter = AccessTools.PropertyGetter(typeof(TIHabState), "tier");
            FieldInfo sectorsField = AccessTools.Field(typeof(TIHabState), "sectors");
            MethodInfo sectorGetter = AccessTools.PropertyGetter(typeof(List<TISectorState>), "Item");
            MethodInfo hasAnyModules = AccessTools.Method(typeof(TISectorState), "HasAnyModules");
            MethodInfo requirement = AccessTools.Method(typeof(HabSectorProgression),
                nameof(HabSectorProgression.ConnectorTierRequirement));
            int[] expectedSector = { 1, 2, 3, 4, 1, 2, 3, 4 };
            int[] expectedTier = { 3, 2, 3, 2, 2, 2, 3, 3 };
            int replacements = 0;
            for (int index = 1; index + 6 < codes.Count; index++)
            {
                if (!codes[index - 1].Calls(tierGetter) ||
                    (codes[index + 1].opcode != OpCodes.Blt &&
                     codes[index + 1].opcode != OpCodes.Blt_S) ||
                    codes[index + 2].opcode != OpCodes.Ldarg_0 ||
                    !codes[index + 3].LoadsField(sectorsField) ||
                    !codes[index + 5].Calls(sectorGetter) ||
                    !codes[index + 6].Calls(hasAnyModules))
                {
                    continue;
                }
                if (replacements >= 8 ||
                    !codes[index].LoadsConstant(expectedTier[replacements]) ||
                    !codes[index + 4].LoadsConstant(expectedSector[replacements]))
                {
                    throw new InvalidOperationException("Hab connector branch/sector/tier gate drifted.");
                }
                CodeInstruction original = codes[index];
                CodeInstruction loadHab = new CodeInstruction(OpCodes.Ldarg_0);
                loadHab.labels.AddRange(original.labels);
                loadHab.blocks.AddRange(original.blocks);
                codes[index] = loadHab;
                codes.InsertRange(index + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldc_I4, expectedSector[replacements]),
                    new CodeInstruction(OpCodes.Ldc_I4, expectedTier[replacements]),
                    new CodeInstruction(OpCodes.Call, requirement)
                });
                replacements++;
                index += 3;
            }
            if (replacements != 8)
            {
                throw new InvalidOperationException("Expected eight station/base connector tier gates; found " + replacements + ".");
            }
            return codes;
        }
    }

    [HarmonyPatch(typeof(TIHabState), nameof(TIHabState.CompleteModuleConstruction))]
    internal static class HabUpgradeSectorProgressionPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo setFaction = AccessTools.Method(typeof(TISectorState), "SetFaction");
            MethodInfo activate = AccessTools.Method(typeof(HabSectorProgression),
                nameof(HabSectorProgression.ActivateUpgradeSector));
            MethodInfo update = AccessTools.Method(typeof(TIHabState), "UpdateAllModuleConnectors");
            MethodInfo reconcile = AccessTools.Method(typeof(HabSectorProgression),
                nameof(HabSectorProgression.ReconcileBeforeConnectors));
            int assignments = 0;
            int updates = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(setFaction))
                {
                    codes[i].opcode = OpCodes.Call;
                    codes[i].operand = activate;
                    assignments++;
                }
                if (codes[i].Calls(update))
                {
                    // The hab is already on the stack. Duplicate it for the
                    // reconciliation call; preserve the native rebuild/event.
                    CodeInstruction duplicate = new CodeInstruction(OpCodes.Dup);
                    duplicate.labels.AddRange(codes[i].labels);
                    duplicate.blocks.AddRange(codes[i].blocks);
                    codes[i].labels.Clear();
                    codes[i].blocks.Clear();
                    codes.Insert(i++, duplicate);
                    codes.Insert(i++, new CodeInstruction(OpCodes.Call, reconcile));
                    updates++;
                }
            }
            if (assignments != 8 || updates != 1)
            {
                throw new InvalidOperationException("Hab upgrade expected eight ownership assignments and one connector rebuild.");
            }
            return codes;
        }
    }

    [HarmonyPatch(typeof(HabListItem), nameof(HabListItem.UpdateItem))]
    internal static class TierOneStationListIconPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(instructions);
            FieldInfo habStateField =
                AccessTools.Field(typeof(HabListItem), "habState");
            FieldInfo sectorsField =
                AccessTools.Field(typeof(TIHabState), "sectors");
            MethodInfo sectorItemGetter =
                AccessTools.PropertyGetter(
                    typeof(List<TISectorState>),
                    "Item");
            MethodInfo activeGetter =
                AccessTools.PropertyGetter(typeof(TISectorState), "active");
            MethodInfo tierGetter =
                AccessTools.PropertyGetter(typeof(TIHabState), "tier");
            int replacements = 0;

            for (int index = 5; index + 1 < codes.Count; index++)
            {
                if (!codes[index].Calls(activeGetter) ||
                    (codes[index + 1].opcode != OpCodes.Brfalse &&
                     codes[index + 1].opcode != OpCodes.Brfalse_S) ||
                    !codes[index - 1].Calls(sectorItemGetter) ||
                    codes[index - 2].opcode != OpCodes.Ldloc_1 ||
                    !codes[index - 3].LoadsField(sectorsField) ||
                    !codes[index - 4].LoadsField(habStateField) ||
                    codes[index - 5].opcode != OpCodes.Ldarg_0)
                {
                    continue;
                }

                codes.InsertRange(
                    index + 1,
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, habStateField),
                        new CodeInstruction(OpCodes.Callvirt, tierGetter),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Cgt),
                        new CodeInstruction(OpCodes.And)
                    });
                replacements++;
                index += 6;
            }

            if (replacements != 1)
            {
                throw new InvalidOperationException(
                    "Expected exactly one station-sector icon loop in " +
                    "HabListItem.UpdateItem; found " +
                    replacements + ".");
            }

            return codes;
        }
    }

    [HarmonyPatch(typeof(FactionGoal_BuildRefuellingStation), nameof(FactionGoal_BuildRefuellingStation.GoalFulfilled))]
    internal static class HabRefuellingCapacityPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            MethodInfo vanilla = AccessTools.Method(typeof(TIHabState), nameof(TIHabState.maxModules), new[] { typeof(int) });
            MethodInfo capacity = AccessTools.Method(typeof(HabRefuellingCapacityPatch), nameof(ActualCapacity));
            int replacements = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(vanilla))
                    continue;
                // Keep the native AllowsResupply check and replace only maxModules(tier).
                CodeInstruction loadGoal = new CodeInstruction(OpCodes.Ldarg_0);
                loadGoal.labels.AddRange(codes[i].labels);
                loadGoal.blocks.AddRange(codes[i].blocks);
                codes[i].labels.Clear();
                codes[i].blocks.Clear();
                codes.Insert(i++, loadGoal);
                codes[i].opcode = OpCodes.Call;
                codes[i].operand = capacity;
                replacements++;
            }
            if (replacements != 1)
                throw new InvalidOperationException("Refuelling station goal expected one native capacity calculation.");
            return codes;
        }

        private static int ActualCapacity(int tier, FactionGoal_BuildRefuellingStation goal)
        {
            TIHabState hab = goal.hab;
            if (!HabSectorProgression.Eligible(hab))
                return TIHabState.maxModules(tier);
            int capacity = 0;
            foreach (TISectorState sector in hab.sectors)
            {
                if (sector.active)
                {
                    capacity += sector.habModules.Count;
                }
            }
            return capacity;
        }
    }

    [HarmonyPatch(typeof(TIHabState), nameof(TIHabState.OnHabCreated))]
    internal static class InitializeHabSectorProgressionPatch
    {
        [HarmonyPrefix]
        internal static void Prefix(TIHabState __instance)
        {
            HabSectorProgression.ReconcileBeforeConnectors(__instance);
            // Load can restore stale connector flags even when an earlier mod
            // already activated every required sector. Rebuild once at startup,
            // independently of whether ownership reconciliation changed state.
            if (HabSectorProgression.Eligible(__instance))
            {
                __instance.UpdateAllModuleConnectors();
            }
        }
    }

    [HarmonyPatch(typeof(TIHabState), nameof(TIHabState.PostEverythingSaveRepair_8))]
    internal static class RepairExistingHabSectorPatch
    {
        [HarmonyPrefix]
        internal static void Prefix(TIHabState __instance)
        {
            HabSectorProgression.ReconcileAndRefresh(__instance);
        }
    }
}
