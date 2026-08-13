using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod.Patches
{
    internal static class HullDriveScalingFeature
    {
        private static readonly object diagnosticLock = new object();
        private static readonly HashSet<string> reportedDiagnostics =
            new HashSet<string>(StringComparer.Ordinal);

        public static float Multiplier(
            TISpaceShipTemplate ship, TIDriveTemplate driveToCheck = null)
        {
            ShipBalanceSettings settings = Main.settings.shipBalance;
            if (!Main.FeatureEnabled(
                settings.enabled && settings.hullDriveScalingEnabled) ||
                ship == null || ship.hullTemplate == null)
            {
                return 1f;
            }

            TIDriveTemplate drive = driveToCheck ?? ship.driveTemplate;
            if (drive == null)
            {
                return 1f;
            }

            string diagnostic;
            float scale = ShipBalanceMath.DriveScale(
                ship.hullTemplate.dataName,
                ship.hullTemplate.alien,
                ship.GetHullAppearanceIndex,
                drive.nozzle.ToString(),
                out diagnostic);
            ReportDiagnosticOnce(diagnostic, scale);
            return scale;
        }

        private static void ReportDiagnosticOnce(
            string diagnostic, float fallbackScale)
        {
            if (string.IsNullOrEmpty(diagnostic))
            {
                return;
            }

            lock (diagnosticLock)
            {
                if (!reportedDiagnostics.Add(diagnostic))
                {
                    return;
                }
            }

            Main.Error(
                "Drive scaling configuration error: " + diagnostic +
                " Safe fallback scale " + fallbackScale.ToString("0.###") +
                " is being used.");
        }
    }

    public struct ReactorBayCapacitySnapshot
    {
        public float BayVolume_m3;
        public float BayVolumeUsed_m3;
        public float BayMassAllowance_tons;
        public float BayOutputLimit_GW;
        public float EffectiveOutput_GW;
        public bool BayLimited;
        public bool UsedFallback;
        public int AppearanceIndex;
        public string SizeBand;
    }

    internal static class ReactorBayCapacityFeature
    {
        private static readonly object diagnosticLock = new object();
        private static readonly HashSet<string> reportedDiagnostics =
            new HashSet<string>(StringComparer.Ordinal);

        public static bool Enabled
        {
            get
            {
                ShipBalanceSettings settings = Main.settings.shipBalance;
                return Main.FeatureEnabled(
                    settings.enabled && settings.reactorBayCapacityEnabled);
            }
        }

        public static bool TryGetSnapshot(
            TISpaceShipTemplate ship,
            TIPowerPlantTemplate powerPlant,
            out ReactorBayCapacitySnapshot snapshot)
        {
            snapshot = default(ReactorBayCapacitySnapshot);
            if (!Enabled || ship == null || ship.hullTemplate == null ||
                powerPlant == null)
            {
                return false;
            }

            TIShipHullTemplate hull = ship.hullTemplate;
            int appearanceIndex = ship.GetHullAppearanceIndex;
            bool usedFallback;
            string sizeBand;
            float bayVolume_m3 = ShipBalanceMath.ReactorBayVolume_m3(
                hull.dataName,
                appearanceIndex,
                hull.smallHull,
                hull.mediumHull,
                hull.largeHull,
                hull.hugeHull,
                out usedFallback,
                out sizeBand);
            string plantClass = powerPlant.powerPlantClass.ToString();
            float massAllowance_tons =
                ShipBalanceMath.ReactorBayMassAllowance_tons(
                    bayVolume_m3, plantClass);
            float bayOutputLimit_GW =
                ShipBalanceMath.ReactorBayOutputLimit_GW(
                    bayVolume_m3,
                    plantClass,
                    powerPlant.specificPower_tGW,
                    powerPlant.maxOutput_GW);
            float effectiveOutput_GW =
                ShipBalanceMath.EffectiveReactorOutput_GW(
                    powerPlant.maxOutput_GW, bayOutputLimit_GW);
            TIDriveTemplate drive = ship.driveTemplate;
            float requiredPower_GW = drive == null
                ? 0f
                : ShipBalanceMath.ScaledDriveValue(
                    drive.powerRequirement_GW,
                    HullDriveScalingFeature.Multiplier(ship, drive));
            float bayVolumeUsed_m3 =
                ShipBalanceMath.ReactorBayVolumeUsed_m3(
                    requiredPower_GW,
                    plantClass,
                    powerPlant.specificPower_tGW);

            snapshot.BayVolume_m3 = bayVolume_m3;
            snapshot.BayVolumeUsed_m3 = bayVolumeUsed_m3;
            snapshot.BayMassAllowance_tons = massAllowance_tons;
            snapshot.BayOutputLimit_GW = bayOutputLimit_GW;
            snapshot.EffectiveOutput_GW = effectiveOutput_GW;
            snapshot.BayLimited = bayOutputLimit_GW + 0.0001f <
                Math.Max(0f, powerPlant.maxOutput_GW);
            snapshot.UsedFallback = usedFallback;
            snapshot.AppearanceIndex = appearanceIndex;
            snapshot.SizeBand = sizeBand;

            if (usedFallback)
            {
                ReportDiagnosticOnce(
                    "No measured reactor bay is configured for hull '" +
                    hull.dataName + "' appearance " + appearanceIndex +
                    ". Using the " + sizeBand + " class-maximum fallback " +
                    bayVolume_m3.ToString("0.###") + " m3.");
            }
            if (powerPlant.specificPower_tGW <= 0f ||
                float.IsNaN(powerPlant.specificPower_tGW) ||
                float.IsInfinity(powerPlant.specificPower_tGW))
            {
                ReportDiagnosticOnce(
                    "Power plant '" + powerPlant.dataName +
                    "' has invalid specificPower_tGW " +
                    powerPlant.specificPower_tGW +
                    "; its theoretical maximum remains in use.");
            }

            return true;
        }

        public static float EffectiveOutput_GW(
            TISpaceShipTemplate ship, TIPowerPlantTemplate powerPlant)
        {
            ReactorBayCapacitySnapshot snapshot;
            return TryGetSnapshot(ship, powerPlant, out snapshot)
                ? snapshot.EffectiveOutput_GW
                : Math.Max(0f, powerPlant == null
                    ? 0f
                    : powerPlant.maxOutput_GW);
        }

        private static void ReportDiagnosticOnce(string diagnostic)
        {
            lock (diagnosticLock)
            {
                if (!reportedDiagnostics.Add(diagnostic))
                {
                    return;
                }
            }
            Main.Error("Reactor-bay capacity configuration: " + diagnostic);
        }
    }

    [HarmonyPatch(typeof(TIMegafaunaArmyState), "techLevel", MethodType.Getter)]
    public static class XenofaunaStrengthPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result, TIMegafaunaArmyState __instance)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.megafaunaEnabled))
            {
                return;
            }

            // Vanilla xenofauna begins at 2, gains 1 tech per 100 abductions, and
            // stops at 6 plus any bonusTechLevel earned after faction control. This
            // changes only that natural ceiling to 5: a vanilla 6.0 becomes 5.0,
            // while a controlled army with +0.4 bonus may still reach 5.4.
            __result = Math.Min(__result,
                settings.megafaunaMaximumTechLevel + __instance.bonusTechLevel);
        }
    }

    [HarmonyPatch(typeof(TITechTemplate), "GetResearchCost")]
    public static class GlobalTechnologyResearchCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            TechnologySettings settings = Main.settings.technology;
            if (!Main.FeatureEnabled(settings.researchCostEnabled))
            {
                return;
            }

            // This method exists only on global technologies. The default x2.00 changes
            // a 1,000 research technology to 2,000 after vanilla applies its modifiers.
            __result *= settings.researchCostMultiplier;
        }
    }

    [HarmonyPatch(typeof(TIProjectTemplate), "GetResearchCost")]
    public static class FactionProjectResearchCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            TechnologySettings settings = Main.settings.technology;
            if (!Main.FeatureEnabled(settings.projectResearchCostEnabled))
            {
                return;
            }

            // Applied after vanilla accounts for repeatables and faction research speed.
            // The default x1.40 turns a resulting 1,000-point project into 1,400 points.
            __result *= settings.projectResearchCostMultiplier;
        }
    }

    [HarmonyPatch(typeof(TIPowerPlantTemplate), "WasteHeat_GW")]
    public static class PowerPlantWasteHeatPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result,
            TIPowerPlantTemplate __instance,
            bool openCycleDriveCooling,
            float drivePowerRequirement_GW,
            float systemsAndWeaponsRequirement_GW)
        {
            ShipBalanceSettings settings = Main.settings.shipBalance;
            if (!Main.FeatureEnabled(
                settings.enabled && settings.correctPowerPlantWasteHeat))
            {
                return true;
            }

            // Vanilla multiplies delivered power by (1 - efficiency). Because the
            // plant requirement is deliveredPower / efficiency, the rejected heat
            // is input minus output: deliveredPower * (1 / efficiency - 1).
            __result = PowerPlantThermalMath.PlantWasteHeat_GW(
                openCycleDriveCooling,
                drivePowerRequirement_GW,
                systemsAndWeaponsRequirement_GW,
                __instance.efficiency,
                settings.openCycleResidualHeatEnabled
                    ? settings.openCycleDriveHeatFraction
                    : 0f);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "crewMass_tons", MethodType.Getter)]
    public static class ShipCrewSupportMassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            ShipBalanceSettings settings = Main.settings.shipBalance;
            if (!Main.FeatureEnabled(
                settings.enabled && settings.crewSupportMassEnabled))
            {
                return true;
            }

            __result = ShipBalanceMath.CrewMass_tons(
                __instance.crewBillets, settings.crewSupportMass_tons);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "modifiedThrust_N", MethodType.Getter)]
    public static class HullScaledDriveThrustPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            __result = ShipBalanceMath.ScaledDriveValue(
                __result, HullDriveScalingFeature.Multiplier(__instance));
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipState), "currentThrust_N", MethodType.Getter)]
    public static class HullScaledLiveShipThrustPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipState __instance)
        {
            if (__instance == null)
            {
                return;
            }

            __result = ShipBalanceMath.ScaledDriveValue(
                __result, HullDriveScalingFeature.Multiplier(__instance.template));
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "drivePowerRequirement_GW",
        MethodType.Getter)]
    public static class HullScaledDrivePowerPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            __result = ShipBalanceMath.ScaledDriveValue(
                __result, HullDriveScalingFeature.Multiplier(__instance));
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), "dryMass_tons")]
    public static class HullScaledDriveMassPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            if (__instance.driveTemplate == null)
            {
                return;
            }

            __result += ShipBalanceMath.AdditionalScaledDriveValue(
                __instance.driveTemplate.buildMass_tons(),
                HullDriveScalingFeature.Multiplier(__instance));
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "spaceResourceConstructionCost")]
    public static class HullScaledDriveConstructionCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref TIResourcesCost __result,
            TISpaceShipTemplate __instance,
            TIHabModuleState shipyard)
        {
            if (__result == null || __instance.driveTemplate == null)
            {
                return;
            }

            float extraFactor =
                HullDriveScalingFeature.Multiplier(__instance) - 1f;
            if (extraFactor <= 0f)
            {
                return;
            }

            if (shipyard != null)
            {
                extraFactor *= TemplateManager.global
                    .GetAIShipbuildingCostDifficultyScaling(
                        __instance.designingFaction);
            }

            __result.SumCosts_NoDuration(
                __instance.driveTemplate.buildCost()
                    .MultiplyCost(extraFactor));
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), "RefitResourceCost")]
    public static class HullScaledDriveRefitCostPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref TIResourcesCost __result,
            TISpaceShipTemplate __instance,
            TISpaceShipTemplate originalDesign)
        {
            if (__result == null || originalDesign == null ||
                (__instance.driveTemplate == originalDesign.driveTemplate &&
                 __instance.powerPlantTemplate ==
                    originalDesign.powerPlantTemplate &&
                 __instance.radiatorTemplate ==
                    originalDesign.radiatorTemplate))
            {
                return;
            }

            if (__instance.driveTemplate != null)
            {
                float newExtra =
                    HullDriveScalingFeature.Multiplier(__instance) - 1f;
                __result.SumCosts_NoDuration(
                    __instance.driveTemplate.buildCost()
                        .MultiplyCost(newExtra));
            }

            if (originalDesign.driveTemplate != null)
            {
                float oldExtra =
                    HullDriveScalingFeature.Multiplier(originalDesign) - 1f;
                __result.SubtractRefitDiscountCost(
                    originalDesign.driveTemplate.buildCost()
                        .MultiplyCost(oldExtra));
            }
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "validDriveForShipsPowerPlant")]
    public static class HullScaledDriveCompatibilityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref bool __result,
            TISpaceShipTemplate __instance,
            TIDriveTemplate driveToCheck)
        {
            if (!__result || __instance.powerPlantTemplate == null ||
                driveToCheck == null)
            {
                return;
            }

            float requiredPower = ShipBalanceMath.ScaledDriveValue(
                driveToCheck.powerRequirement_GW,
                HullDriveScalingFeature.Multiplier(__instance, driveToCheck));
            __result = requiredPower <=
                ReactorBayCapacityFeature.EffectiveOutput_GW(
                    __instance, __instance.powerPlantTemplate);
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "ValidPowerPlantForShipsDrive")]
    public static class HullScaledPowerPlantCompatibilityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref bool __result,
            TISpaceShipTemplate __instance,
            TIPowerPlantTemplate powerPlantToCheck)
        {
            if (!__result || powerPlantToCheck == null ||
                __instance.driveTemplate == null)
            {
                return;
            }

            float requiredPower = ShipBalanceMath.ScaledDriveValue(
                __instance.driveTemplate.powerRequirement_GW,
                HullDriveScalingFeature.Multiplier(__instance));
            __result = requiredPower <=
                ReactorBayCapacityFeature.EffectiveOutput_GW(
                    __instance, powerPlantToCheck);
        }
    }
}
