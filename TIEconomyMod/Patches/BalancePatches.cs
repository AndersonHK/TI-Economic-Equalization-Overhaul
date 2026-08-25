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

            return GraphicalMultiplier(
                ship.hullTemplate.dataName,
                ship.hullTemplate.alien,
                ship.GetHullAppearanceIndex,
                drive.nozzle.ToString());
        }

        public static float GraphicalMultiplier(
            string hullDataName,
            bool alien,
            int appearanceIndex,
            string nozzleFamily)
        {
            if (string.Equals(
                    nozzleFamily, "Pulsed", StringComparison.Ordinal))
            {
                return 1f;
            }

            string diagnostic = null;
            float scale;
            if (!alien)
            {
                if (Main.hullDriveScales == null)
                {
                    diagnostic = "Measured human drive-art catalog is not " +
                        "loaded.";
                    scale = 1f;
                }
                else if (!Main.hullDriveScales.TryGetScale(
                    hullDataName,
                    appearanceIndex,
                    nozzleFamily,
                    out scale))
                {
                    diagnostic = "No measured human drive-art scale is " +
                        "configured for hull '" + hullDataName +
                        "' appearance " + appearanceIndex + " and nozzle '" +
                        nozzleFamily + "'.";
                    scale = 1f;
                }
            }
            else
            {
                scale = ShipBalanceMath.DriveScale(
                    hullDataName,
                    true,
                    appearanceIndex,
                    nozzleFamily,
                    out diagnostic);
            }

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

    internal static class OpenCycleReactorDemandFeature
    {
        public static bool Enabled
        {
            get
            {
                ShipBalanceSettings settings = Main.settings.shipBalance;
                return Main.FeatureEnabled(
                    settings.enabled &&
                    settings.correctPowerPlantWasteHeat &&
                    (settings.openCycleResidualHeatEnabled ||
                        settings.openCycleThermalMassScalingEnabled));
            }
        }

        public static float RequiredCapRatedDriveOutput_GW(
            float usefulDrivePower_GW,
            TIDriveTemplate drive,
            TIPowerPlantTemplate powerPlant)
        {
            if (!Enabled || drive == null || powerPlant == null ||
                !drive.openCycleCooling)
            {
                return usefulDrivePower_GW;
            }

            ShipBalanceSettings settings = Main.settings.shipBalance;
            return PowerPlantThermalMath.CapRatedDriveDemand_GW(
                true,
                usefulDrivePower_GW,
                powerPlant.efficiency,
                settings.openCycleResidualHeatEnabled
                    ? settings.openCycleDriveHeatFraction
                    : 0f,
                settings.openCycleThermalMassScalingEnabled
                    ? PowerPlantScalingRegistry
                        .OpenCycleThermalMassMultiplier(powerPlant)
                    : 1f);
        }
    }

    internal static class DrivePowerPlantCompatibilityFeature
    {
        public static bool ClassCompatible(
            TIDriveTemplate drive, TIPowerPlantTemplate powerPlant)
        {
            if (drive == null || powerPlant == null)
            {
                return false;
            }

            PowerPlantRequirement required = drive.requiredPowerPlant;
            return required == PowerPlantRequirement.Any_General ||
                required == powerPlant.powerPlantClass ||
                (required ==
                    PowerPlantRequirement.Any_Magnetic_Confinement_Fusion &&
                    powerPlant.magneticFusionPlant) ||
                (powerPlant.powerPlantClass ==
                    PowerPlantRequirement.Molten_Salt_Core_Fission &&
                    (required == PowerPlantRequirement.Solid_Core_Fission ||
                        required ==
                            PowerPlantRequirement.Liquid_Core_Fission));
        }

        public static bool FitsRatedOutput(
            float usefulDrivePower_GW,
            TIDriveTemplate drive,
            TIPowerPlantTemplate powerPlant,
            float ratedOutput_GW)
        {
            return ClassCompatible(drive, powerPlant) &&
                OpenCycleReactorDemandFeature
                    .RequiredCapRatedDriveOutput_GW(
                        usefulDrivePower_GW, drive, powerPlant) <=
                    Math.Max(0f, ratedOutput_GW) + 0.0001f;
        }
    }

    internal static class ShipPowerDemandFeature
    {
        public static bool Enabled
        {
            get
            {
                ShipBalanceSettings settings = Main.settings.shipBalance;
                return Main.FeatureEnabled(
                    settings.enabled && settings.correctPowerPlantWasteHeat);
            }
        }

        public static ShipPowerDemandSnapshot Snapshot(
            TISpaceShipTemplate ship,
            TIDriveTemplate drive = null,
            TIPowerPlantTemplate powerPlant = null)
        {
            drive = drive ?? (ship == null ? null : ship.driveTemplate);
            powerPlant = powerPlant ??
                (ship == null ? null : ship.powerPlantTemplate);
            if (powerPlant == null)
            {
                return default(ShipPowerDemandSnapshot);
            }

            float driveDemand_GW = drive == null
                ? 0f
                : ShipBalanceMath.ScaledDriveValue(
                    drive.powerRequirement_GW,
                    HullDriveScalingFeature.Multiplier(ship, drive));
            float auxiliaryElectricalDemand_GW = ship == null
                ? 0f
                : Math.Max(0f, ship.requiredSystemsPower_GW) +
                    Math.Max(0f, ship.requiredWeaponsPowerGeneration_GW);
            ShipBalanceSettings settings = Main.settings.shipBalance;
            float multiplier = settings.openCycleThermalMassScalingEnabled
                ? PowerPlantScalingRegistry
                    .OpenCycleThermalMassMultiplier(powerPlant)
                : 1f;
            return PowerPlantThermalMath.CalculateShipDemand(
                drive != null && drive.openCycleCooling,
                driveDemand_GW,
                auxiliaryElectricalDemand_GW,
                powerPlant.efficiency,
                settings.openCycleResidualHeatEnabled
                    ? settings.openCycleDriveHeatFraction
                    : 0f,
                multiplier);
        }
    }

    internal static class HullVariantEmptyMassFeature
    {
        private static readonly object diagnosticLock = new object();
        private static readonly HashSet<string> reportedDiagnostics =
            new HashSet<string>(StringComparer.Ordinal);

        public static float EmptyHullMass_tons(TISpaceShipTemplate ship)
        {
            if (ship == null || ship.hullTemplate == null)
            {
                return 0f;
            }

            float baseMass_tons = ship.hullTemplate.buildMass_tons();
            return baseMass_tons + AdditionalMass_tons(ship);
        }

        public static float AdditionalMass_tons(TISpaceShipTemplate ship)
        {
            if (ship == null || ship.hullTemplate == null ||
                ship.hullTemplate.alien ||
                !Main.FeatureEnabled(
                    Main.settings.shipBalance.enabled &&
                    Main.settings.shipBalance.hullDriveScalingEnabled))
            {
                return 0f;
            }

            TIShipHullTemplate hull = ship.hullTemplate;
            int appearanceIndex = ship.GetHullAppearanceIndex;
            float mass_tons;
            if (!ShipBalanceMath.TryGetVariantEmptyHullMass_tons(
                    hull.dataName, appearanceIndex, out mass_tons))
            {
                ReportDiagnosticOnce(
                    "No flat empty-hull mass is configured for hull '" +
                    hull.dataName +
                    "' appearance " + appearanceIndex +
                    "; vanilla empty hull mass is being used.");
                return 0f;
            }

            float baseMass_tons = hull.buildMass_tons();
            return mass_tons - baseMass_tons;
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

            Main.Error("Hull variant mass configuration error: " +
                diagnostic);
        }
    }

    public struct ReactorBayCapacitySnapshot
    {
        public float BayVolume_m3;
        public float BayVolumeUsed_m3;
        public float BayMassAllowance_tons;
        public float BayOutputLimit_GW;
        public float EffectiveOutput_GW;
        public float TotalReactorOutput_GWth;
        public float MassRatedOutput_GW;
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
            ShipPowerDemandSnapshot demand =
                ShipPowerDemandFeature.Snapshot(
                    ship, ship.driveTemplate, powerPlant);
            float bayVolumeUsed_m3 =
                ShipBalanceMath.ReactorBayVolumeUsed_m3(
                    demand.MassRatedOutput_GW,
                    plantClass,
                    powerPlant.specificPower_tGW);

            snapshot.BayVolume_m3 = bayVolume_m3;
            snapshot.BayVolumeUsed_m3 = bayVolumeUsed_m3;
            snapshot.BayMassAllowance_tons = massAllowance_tons;
            snapshot.BayOutputLimit_GW = bayOutputLimit_GW;
            snapshot.EffectiveOutput_GW = effectiveOutput_GW;
            snapshot.TotalReactorOutput_GWth =
                demand.TotalReactorOutput_GWth;
            snapshot.MassRatedOutput_GW = demand.MassRatedOutput_GW;
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

            // This method exists only on global technologies. The default x2.20 changes
            // a 1,000 research technology to 2,200 after vanilla applies its modifiers.
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

            // Closed-cycle loads reject input minus delivered power. For an
            // open-cycle drive, the drive argument is its useful thermal demand;
            // only the configured share of the implied loss remains on the ship.
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

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "shipPowerProductionRequirement_GW",
        MethodType.Getter)]
    public static class SeparatedShipPowerProductionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            if (!ShipPowerDemandFeature.Enabled || __instance == null ||
                __instance.powerPlantTemplate == null)
            {
                return true;
            }

            __result = ShipPowerDemandFeature.Snapshot(__instance)
                .TotalReactorOutput_GWth;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "powerPlantMass_tons",
        MethodType.Getter)]
    public static class OpenCyclePowerPlantMassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            if (!ShipPowerDemandFeature.Enabled || __instance == null ||
                __instance.powerPlantTemplate == null)
            {
                return true;
            }

            ShipPowerDemandSnapshot demand =
                ShipPowerDemandFeature.Snapshot(__instance);
            __result = __instance.powerPlantTemplate.buildMass_tons(
                demand.MassRatedOutput_GW);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "powerPlantBuildCost",
        MethodType.Getter)]
    public static class OpenCyclePowerPlantCostPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref TIResourcesCost __result, TISpaceShipTemplate __instance)
        {
            if (!ShipPowerDemandFeature.Enabled || __instance == null ||
                __instance.powerPlantTemplate == null)
            {
                return true;
            }

            ShipPowerDemandSnapshot demand =
                ShipPowerDemandFeature.Snapshot(__instance);
            __result = __instance.powerPlantTemplate.buildCost(
                demand.MassRatedOutput_GW);
            return false;
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

    [HarmonyPatch(typeof(TISpaceShipTemplate), "dryMass_tons")]
    public static class HullVariantEmptyMassPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref float __result, TISpaceShipTemplate __instance)
        {
            __result += HullVariantEmptyMassFeature.AdditionalMass_tons(
                __instance);
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
            if (Math.Abs(extraFactor) <= 0.0001f)
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
        typeof(TIDriveTemplate), "IsCompatible")]
    public static class OpenCycleDrivePowerPlantCompatibilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref bool __result,
            TIDriveTemplate __instance,
            TIPowerPlantTemplate powerPlant)
        {
            __result = DrivePowerPlantCompatibilityFeature.FitsRatedOutput(
                __instance == null ? 0f : __instance.powerRequirement_GW,
                __instance,
                powerPlant,
                powerPlant == null ? 0f : powerPlant.maxOutput_GW);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "ValidDrivesForPowerPlants")]
    public static class OpenCycleValidDrivesForPowerPlantsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref List<TIDriveTemplate> __result,
            List<TIDriveTemplate> candidateDrives,
            IEnumerable<TIPowerPlantTemplate> availablePowerPlants)
        {
            List<TIPowerPlantTemplate> plants = availablePowerPlants == null
                ? new List<TIPowerPlantTemplate>()
                : new List<TIPowerPlantTemplate>(availablePowerPlants);
            __result = new List<TIDriveTemplate>();
            if (candidateDrives == null)
            {
                return false;
            }

            foreach (TIDriveTemplate drive in candidateDrives)
            {
                foreach (TIPowerPlantTemplate plant in plants)
                {
                    if (DrivePowerPlantCompatibilityFeature.FitsRatedOutput(
                            drive == null ? 0f : drive.powerRequirement_GW,
                            drive,
                            plant,
                            plant == null ? 0f : plant.maxOutput_GW))
                    {
                        __result.Add(drive);
                        break;
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "validDriveForShipsPowerPlant")]
    public static class HullScaledDriveCompatibilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref bool __result,
            TISpaceShipTemplate __instance,
            TIDriveTemplate driveToCheck)
        {
            if (__instance == null || __instance.powerPlantTemplate == null)
            {
                __result = true;
                return false;
            }

            __result = DrivePowerPlantCompatibilityFeature.FitsRatedOutput(
                ShipBalanceMath.ScaledDriveValue(
                    driveToCheck == null
                        ? 0f
                        : driveToCheck.powerRequirement_GW,
                    HullDriveScalingFeature.Multiplier(
                        __instance, driveToCheck)),
                driveToCheck,
                __instance.powerPlantTemplate,
                ReactorBayCapacityFeature.EffectiveOutput_GW(
                    __instance, __instance.powerPlantTemplate));
            return false;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate), "ValidPowerPlantForShipsDrive")]
    public static class HullScaledPowerPlantCompatibilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref bool __result,
            TISpaceShipTemplate __instance,
            TIPowerPlantTemplate powerPlantToCheck)
        {
            if (__instance == null || __instance.driveTemplate == null)
            {
                __result = true;
                return false;
            }

            __result = DrivePowerPlantCompatibilityFeature.FitsRatedOutput(
                ShipBalanceMath.ScaledDriveValue(
                    __instance.driveTemplate.powerRequirement_GW,
                    HullDriveScalingFeature.Multiplier(__instance)),
                __instance.driveTemplate,
                powerPlantToCheck,
                ReactorBayCapacityFeature.EffectiveOutput_GW(
                    __instance, powerPlantToCheck));
            return false;
        }
    }
}
