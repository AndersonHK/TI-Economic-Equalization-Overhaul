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

        internal static float MandatoryBoost(
            TIHabModuleTemplate template,
            TISpaceBodyState spaceBody,
            TIFactionState faction,
            TIGameState destination,
            float rateMultiplier)
        {
            float earthMass = MandatoryEarthMass(
                template,
                spaceBody,
                faction,
                destination,
                rateMultiplier);
            return HabRebalanceMath.RoundCost(
                (float)TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                    faction,
                    destination,
                    earthMass));
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
            float transferDays =
                TISpaceObjectState.GenericTransferTimeFromEarthsSurface_d(
                    faction,
                    destination);
            transferDays += TIEffectsState.SumEffectsModifiers(
                Context.GenericModuleTransferTime,
                faction,
                transferDays,
                null);
            return transferDays;
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
                HabRebalanceMath.OrdinaryMaterialMass(
                    nominalMass,
                    materialWeightSum,
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
            float mandatoryMass =
                HabConstructionCostRewrite.MandatoryEarthMass(
                    __instance,
                    spaceBody,
                    faction,
                    destination,
                    rateMultiplier);

            __result = HabRebalanceMath.RoundCost(
                (float)TISpaceObjectState.GenericTransferBoostFromEarthSurface(
                    faction,
                    destination,
                    ordinaryMass + mandatoryMass));
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
            TIResourcesCost cost = materials.ToResourcesCost(1f);
            float mandatoryBoost =
                HabConstructionCostRewrite.MandatoryBoost(
                    __instance,
                    spaceBody,
                    faction,
                    destinationState,
                    rateMultiplier);
            cost.AddCost(FactionResource.Boost, mandatoryBoost, false);

            if (substituteBoost &&
                !cost.CanAfford(faction, 1f, null, float.PositiveInfinity) &&
                (faction.IsActiveHumanFaction ||
                 GameStateManager.AlienNation().extant))
            {
                cost = cost.GetBoostSubstitutedCost(
                    faction,
                    destinationState,
                    true,
                    null);
            }

            float transferTime = HabRebalanceMath.HasEarthDelivery(
                cost.GetSingleCostValue(FactionResource.Boost))
                ? HabConstructionCostRewrite.EarthTransferTime(
                    faction,
                    destinationState)
                : 0f;
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

    internal static class HabStationSectorRebalance
    {
        internal static void ReconcileTierOneStation(TIHabState hab)
        {
            if (hab == null ||
                !hab.IsStation ||
                hab.tier != 1 ||
                hab.IsAlien() ||
                hab.sectors == null ||
                hab.sectors.Count <= 4)
            {
                return;
            }

            TISectorState coreSector = hab.sectors[0];
            if (coreSector.faction == null)
            {
                return;
            }

            ActivateSector(hab.sectors[2], coreSector.faction);
            ActivateSector(hab.sectors[4], coreSector.faction);
        }

        private static void ActivateSector(
            TISectorState sector,
            TIFactionState faction)
        {
            if (!sector.active)
            {
                sector.SetFaction(faction);
            }
        }

        internal static int ConnectorTierRequirement(
            TIHabState hab,
            int sectorIndex)
        {
            bool validSector =
                (sectorIndex == 2 || sectorIndex == 4) &&
                hab != null &&
                hab.sectors != null &&
                hab.sectors.Count > sectorIndex;
            return HabRebalanceMath.ConnectorTierRequirement(
                hab == null ? 0 : hab.tier,
                hab != null && hab.IsStation,
                hab != null && hab.IsAlien(),
                validSector && hab.sectors[sectorIndex].active);
        }
    }

    [HarmonyPatch(
        typeof(TISectorState),
        nameof(TISectorState.UpdateModuleConnectorMap),
        new Type[] { typeof(TIHabState), typeof(TIHabModuleState) })]
    internal static class TierOneStationConnectorMapPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes =
                new List<CodeInstruction>(instructions);
            MethodInfo tierGetter =
                AccessTools.PropertyGetter(typeof(TIHabState), "tier");
            FieldInfo sectorsField =
                AccessTools.Field(typeof(TIHabState), "sectors");
            MethodInfo hasAnyModules =
                AccessTools.Method(typeof(TISectorState), "HasAnyModules");
            MethodInfo requirement =
                AccessTools.Method(
                    typeof(HabStationSectorRebalance),
                    nameof(HabStationSectorRebalance.ConnectorTierRequirement));
            int firstSectorSwitch = codes.FindIndex(
                instruction => instruction.opcode == OpCodes.Switch);
            if (firstSectorSwitch < 0)
            {
                throw new InvalidOperationException(
                    "Could not find the station-sector switch in " +
                    "TISectorState.UpdateModuleConnectorMap.");
            }

            int replacements = 0;

            for (int index = 1;
                index + 6 < firstSectorSwitch;
                index++)
            {
                if (!codes[index - 1].Calls(tierGetter) ||
                    !LoadsInt(codes[index], 2) ||
                    (codes[index + 1].opcode != OpCodes.Blt &&
                     codes[index + 1].opcode != OpCodes.Blt_S) ||
                    !codes[index + 3].LoadsField(sectorsField) ||
                    !TryGetTargetSector(codes[index + 4], out int sectorIndex) ||
                    !codes[index + 6].Calls(hasAnyModules))
                {
                    continue;
                }

                CodeInstruction originalThreshold = codes[index];
                CodeInstruction loadHab =
                    new CodeInstruction(OpCodes.Ldarg_0);
                loadHab.labels.AddRange(originalThreshold.labels);
                loadHab.blocks.AddRange(originalThreshold.blocks);
                codes[index] = loadHab;
                codes.Insert(
                    index + 1,
                    new CodeInstruction(OpCodes.Ldc_I4, sectorIndex));
                codes.Insert(
                    index + 2,
                    new CodeInstruction(OpCodes.Call, requirement));
                replacements++;
                index += 2;
            }

            if (replacements != 2)
            {
                throw new InvalidOperationException(
                    "Expected exactly two T1 station connector tier gates in " +
                    "TISectorState.UpdateModuleConnectorMap; found " +
                    replacements + ".");
            }

            return codes;
        }

        private static bool TryGetTargetSector(
            CodeInstruction instruction,
            out int sectorIndex)
        {
            if (LoadsInt(instruction, 2))
            {
                sectorIndex = 2;
                return true;
            }

            if (LoadsInt(instruction, 4))
            {
                sectorIndex = 4;
                return true;
            }

            sectorIndex = -1;
            return false;
        }

        private static bool LoadsInt(
            CodeInstruction instruction,
            int value)
        {
            if (instruction.opcode == OpCodes.Ldc_I4)
            {
                return instruction.operand is int &&
                    (int)instruction.operand == value;
            }

            if (instruction.opcode == OpCodes.Ldc_I4_S)
            {
                return instruction.operand is sbyte &&
                    (sbyte)instruction.operand == value;
            }

            return value == 2 && instruction.opcode == OpCodes.Ldc_I4_2 ||
                value == 4 && instruction.opcode == OpCodes.Ldc_I4_4;
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
