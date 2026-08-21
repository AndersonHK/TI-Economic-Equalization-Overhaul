using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.IO;
using TIEconomyMod.Patches;
using UnityEngine;
using UnityModManagerNet;

namespace TIEconomyMod
{
    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings settings;
        public static TechWeightCatalog techWeights;
        public static HullVolumeCatalog hullVolumes;
        public static HullDriveScaleCatalog hullDriveScales;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry) ?? new Settings();
            settings.ValidateAndRepair(Log);

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            enabled = true;

            try
            {
                string weightPath = Path.Combine(modEntry.Path, "Config", "economy-tech-weights.csv");
                techWeights = TechWeightCatalog.Load(weightPath, Log, IsKnownTechnology);
                string hullVolumePath = Path.Combine(
                    modEntry.Path, "Config", "hull-variant-main-volumes.csv");
                hullVolumes = HullVolumeCatalog.Load(hullVolumePath, Log);
                string hullDriveScalePath = Path.Combine(
                    modEntry.Path, "Config", "hull-variant-drive-scales.csv");
                hullDriveScales = HullDriveScaleCatalog.Load(
                    hullDriveScalePath, Log);
                Harmony harmony = new Harmony(modEntry.Info.Id);
                try
                {
                    harmony.PatchAll();
                }
                catch (Exception patchException)
                {
                    try
                    {
                        harmony.UnpatchAll(modEntry.Info.Id);
                    }
                    catch (Exception rollbackException)
                    {
                        throw new AggregateException(
                            "EEO patching failed and its partial-patch " +
                            "rollback also failed.",
                            patchException,
                            rollbackException);
                    }

                    throw;
                }
                HabLogistics.Clear();
                if (TemplateManager.self != null &&
                    TemplateManager.self.Initialized)
                {
                    GunPowerRegistry.Refresh();
                    ProjectileGeometryRegistry.Refresh();
                    UtilityFootprintRegistry.Refresh();
                    PropellantDensityRegistry.Refresh();
                    ShipPowerRuntime.RefreshTemplateMassCaches();
                }
                CouncilorRuntimeCaps.InitializeOrganizationCap();
                Log("Loaded TI Economic Equalization Overhaul 0.9.3 for the TI 1.0.51 API surface.");
                return true;
            }
            catch (Exception exception)
            {
                enabled = false;
                modEntry.Logger.Error("EEO initialization failed; all EEO features are disabled.\n" + exception);
                return false;
            }
        }

        private static bool IsKnownTechnology(string technologyId)
        {
            try
            {
                return TemplateManager.Find<TITechTemplate>(technologyId, false) != null;
            }
            catch (InvalidOperationException)
            {
                // UMM normally loads us after template initialization. If another loader changes
                // that order, defer the unknown-ID check instead of disabling all tech scaling.
                return true;
            }
            catch (NullReferenceException)
            {
                return true;
            }
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            CouncilorRuntimeCaps.ApplyOrganizationCap();
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
            GUILayout.Space(8f);
            if (GUILayout.Button("Reset all EEO settings to defaults"))
            {
                settings = new Settings();
                settings.ValidateAndRepair(Log);
                CouncilorRuntimeCaps.ApplyOrganizationCap();
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ValidateAndRepair(Log);
            CouncilorRuntimeCaps.ApplyOrganizationCap();
            ShipPowerRuntime.RefreshTemplateMassCaches();
            settings.Save(modEntry);
        }

        public static bool FeatureEnabled(bool featureEnabled)
        {
            return enabled && settings != null && settings.enabled && featureEnabled;
        }

        public static void Log(string message)
        {
            if (mod != null)
            {
                mod.Logger.Log(message);
            }
        }

        public static void Warn(string message)
        {
            if (mod != null)
            {
                mod.Logger.Warning(message);
            }
        }

        public static void Error(string message)
        {
            if (mod != null)
            {
                mod.Logger.Error(message);
            }
        }
    }

    public sealed class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Enable mod")]
        public bool enabled = true;

        [Draw("Investment Points")]
        public InvestmentSettings investment = new InvestmentSettings();

        [Draw("Economy Growth")]
        public EconomySettings economy = new EconomySettings();

        [Draw("Technology Weighting")]
        public TechnologySettings technology = new TechnologySettings();

        [Draw("Resource and Land Curves")]
        public AbundanceSettings abundance = new AbundanceSettings();

        [Draw("Bounded Inequality")]
        public InequalitySettings inequality = new InequalitySettings();

        [Draw("Cohesion Rest State")]
        public CohesionRestSettings cohesionRest = new CohesionRestSettings();

        [Draw("Control Point Cost")]
        public ControlCostSettings controlCost = new ControlCostSettings();

        [Draw("Councilors")]
        public CouncilorSettings councilors = new CouncilorSettings();

        [Draw("Land Armies")]
        public ArmySettings army = new ArmySettings();

        [Draw("Ship Balance")]
        public ShipBalanceSettings shipBalance = new ShipBalanceSettings();

        [Draw("Research")]
        public ResearchSettings research = new ResearchSettings();

        [Draw("Knowledge")]
        public KnowledgeSettings knowledge = new KnowledgeSettings();

        [Draw("Government")]
        public GovernmentSettings government = new GovernmentSettings();

        [Draw("Military")]
        public MilitarySettings military = new MilitarySettings();

        [Draw("National Mergers")]
        public NationalMergerSettings nationalMergers = new NationalMergerSettings();

        [Draw("Claim Harmonization")]
        public ClaimHarmonizationSettings claimHarmonization = new ClaimHarmonizationSettings();

        [Draw("Oppression")]
        public OppressionSettings oppression = new OppressionSettings();

        [Draw("Environment")]
        public EnvironmentSettings environment = new EnvironmentSettings();

        [Draw("Economy Emissions")]
        public EmissionsSettings emissions = new EmissionsSettings();

        [Draw("Unity")]
        public UnitySettings unity = new UnitySettings();

        [Draw("Spoils Effects")]
        public SpoilsSettings spoils = new SpoilsSettings();

        [Draw("Spoils Money")]
        public SpoilsMoneySettings spoilsMoney = new SpoilsMoneySettings();

        [Draw("Region Thresholds")]
        public RegionThresholdSettings regionThresholds = new RegionThresholdSettings();

        [Draw("UI")]
        public UiSettings ui = new UiSettings();

        public void OnChange()
        {
            ValidateAndRepair(Main.Log);
            CouncilorRuntimeCaps.ApplyOrganizationCap();
        }

        public void ValidateAndRepair(Action<string> log)
        {
            SettingsValidator.Validate(this, log);
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class InvestmentSettings
    {
        public bool enabled = true;
        public float gdpPerInvestmentPointBillions = 100f;
        public float lowIncomeMultiplierAtZero = 0.70f;
        public float lowIncomeThreshold = 15000f;
        public float outputMultiplier = 1.05f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class EconomySettings
    {
        public bool enabled = true;
        public float baseGainBillions = 1f;
        public float educationPerLevel = 0.15f;
        public float governmentPerLevel = 0.05f;
        public float cohesionCenter = 5f;
        public float cohesionPeak = 1.20f;
        public float cohesionPenaltyPerPoint = 0.04f;
        public float referenceCoreRegions = 1f;
        public float referenceEducation = 7f;
        public float referenceGovernment = 6f;
        public float referenceCohesion = 5f;
        public float laborKneePcgdp = 37500f;
        public float resourceKneePcgdp = 55000f;
        public float startingLaborReturnFloor = 0.35f;
        public float startingResourceReturnFloor = 0.45f;
        public float technologyReliefLinearShare = 0.10f;
        public float minimumSupport = 0.05f;
        public float laborPressureExponent = 1.40f;
        public float resourcePressureExponent = 1.20f;
        public float resourceDirectLift = 1f;
        public float landDirectLift = 0.25f;
        public float coreRegionMaximumBonus = 1.20f;
        public float coreRegionHalfSaturation = 2f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class TechnologySettings
    {
        public bool enabled = true;
        public float maximumMultiplier = 4f;
        public bool researchCostEnabled = true;
        public float researchCostMultiplier = 2.00f;
        public bool projectResearchCostEnabled = true;
        public float projectResearchCostMultiplier = 1.40f;
        public bool aiSelectionEnabled = true;
        public float aiSelectionCostExponent = 0.75f;
        public float aiSelectionMinimumCostMultiplier = 0.25f;
        public float aiSelectionMaximumCostMultiplier = 4f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class AbundanceSettings
    {
        public bool enabled = true;
        public float referenceGdpPerResourceRegionBillions = 1000f;
        public float minimumGdpBillions = 1f;
        public float resourceMaximumBonus = 1f;
        public float resourceCurveExponent = 0.30f;
        public float referenceDensity = 50f;
        public float minimumDensity = 0.1f;
        public float landMaximumBonus = 0.25f;
        public float landCurveExponent = 1f;
        public float landReferencePcgdp = 30000f;
        public float landMinimumWealthRelevance = 0.25f;
        public float maximumUnrest = 10f;
        public float unrestExponent = 1f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class InequalitySettings
    {
        public bool enabled = true;
        public bool economyEnabled = true;
        public bool welfareEnabled = true;
        public bool spoilsEnabled = true;
        public bool coupEnabled = true;
        public float coupInequalityChange = -0.10f;
        public bool coupResetCohesionToRestState = true;
        public float minimum = 1f;
        public float neutral = 5f;
        public float maximum = 9f;
        public float exponent = 2f;
        public float maximumDirectionalMultiplier = 3f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float economyChangeAtReferenceGdp = 0.00225f;
        public float welfareChangeAtReferenceGdp = -0.01999998f;
        public float spoilsChangeAtReferenceGdp = 0.01000002f;
        public float climateChangeMultiplier = 4f;
        public float economyMaximumResourceMultiplier = 0.60f;
        public float spoilsMaximumResourceMultiplier = 1f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class CohesionRestSettings
    {
        public bool enabled = true;
        public float baseValue = 10.5f;
        public float autocracyAnocracyBoundary = 4f;
        public float autocracyExponent = 1.285f;
        public float anocracyDemocracyBoundary = 6f;
        public float democracyCoefficient = 1f;
        public float inequalityEducationBaseMultiplier = 0.5f;
        public float inequalityEducationDivisor = 20f;
        public float inequalityOffset = 6.75f;
        public float inequalityCoefficient = 2.25f;
        public float publicEliteGovernmentDivisor = 10f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ControlCostSettings
    {
        public bool enabled = true;
        public bool projectBonusesAsPercent = true;
        public float countryCostMultiplier = 1.20f;
        public float arrivalInternationalRelationsReduction = 0.02f;
        public float unityMovementsReduction = 0.03f;
        public float greatNationsReduction = 0.05f;
        public float arrivalGovernanceReduction = 0.05f;
        public float accelerandoReduction = 0.05f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class CouncilorSettings
    {
        public bool enabled = true;
        public float totalAttributeCap = 50f;
        public float maximumOrganizations = 18f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ArmySettings
    {
        public bool enabled = true;
        public float costCoefficient = 2f;
        public float costGrowthBase = 2f;
        public float repairShare = 0.5f;
        public bool repairDebtMergedDisplayEnabled = true;
        public float homeUpkeepDivisor = 10f;
        public float awayUpkeepDivisor = 3f;
        public float combatModifierScale = 0.5f;
        public float maximumStrengthPenalty = 1f;
        public float hitCurveBase = 2f;
        public bool alienFloraDamageScalingEnabled = true;
        public float alienFloraFullDamageLevel = 100f;
        public bool megafaunaEnabled = true;
        public float megafaunaMaximumTechLevel = 5f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ShipBalanceSettings
    {
        public bool enabled = true;
        public bool multiSlotUtilitiesEnabled = true;
        public bool correctPowerPlantWasteHeat = true;
        public bool openCycleResidualHeatEnabled = true;
        public float openCycleDriveHeatFraction = 0.01f;
        public bool crewSupportMassEnabled = true;
        public float crewSupportMass_tons = 3f;
        public bool hullDriveScalingEnabled = true;
        public bool reactorBayCapacityEnabled = true;
        public bool fuelVolumeCapacityEnabled = true;
        public float utilitySlotVolume_m3 = 200f;
        public float hullWeaponSlotVolume_m3 = 250f;
        public float noseWeaponSlotVolume_m3 = 400f;
        public float crewPressurizedVolume_m3 = 50f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ResearchSettings
    {
        public bool enabled = true;
        public bool neutralControlPointResearchEnabled = true;
        public float coefficient = 0.0038f;
        public float referencePcgdp = 20000f;
        public float pcgdpOffset = 12000f;
        public float educationExponent = 2f;
        public float democracyFloor = 0.10f;
        public float democracyExponent = 0.20f;
        public float cohesionCenter = 5f;
        public float cohesionPeak = 1.25f;
        public float cohesionPenaltyPerPoint = 0.10f;
        public float unrestGrace = 2f;
        public float unrestPenaltyDivisor = 10f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class KnowledgeSettings
    {
        public bool enabled = true;
        public float educationPopulationDivisor = 166667f;
        public float educationMaximumGain = 4f;
        public float educationDecay = 0.87f;
        public float cohesionPopulationDivisor = 333333f;
        public float cohesionTarget = 5f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class GovernmentSettings
    {
        public bool enabled = true;
        public float democracyPopulationDivisor = 333333f;
        public float boundaryCurveFactor = 3f;
        public float passiveLowCohesionMultiplier = 0.50f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class MilitarySettings
    {
        public bool enabled = true;
        public float doctrineBaseCostAtTechOne = 500f;
        public float doctrineCostGrowthBase = 2f;
        public float catchupGapCoefficient = 1f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class NationalMergerSettings
    {
        public bool enabled = true;
        public bool militaryEnabled = true;
        public bool inequalityEnabled = true;
        public float inequalityMinimum = 1f;
        public float inequalityMaximum = 9f;
        public float minimumPerCapitaGdp = 1f;
        public float inequalityBoundaryEpsilon = 0.000001f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ClaimHarmonizationSettings
    {
        public bool enabled = true;
        public float ordinaryThreshold = 6f;
        public float historicalThreshold = 3f;
        public float federationThreshold = 12f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class OppressionSettings
    {
        public bool enabled = true;
        public float unrestPopulationDivisor = 2222222f;
        public float fullDemocracy = 10f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class EnvironmentSettings
    {
        public bool enabled = true;
        public float storageRatingOffset = 0.10f;
        public float startingTechnologyCap = 3f;
        public float maximumTechnologyCap = 10f;
        public float advancementBaseIp = 0.125f;
        public float advancementCostGrowthBase = 1.50f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float cleanupCO2_ppm = 0.00008125f;
        public float cleanupCH4_ppm = 0.000000625f;
        public float cleanupN2O_ppm = 0.000000625f;
        public float falloutReferenceAreaKm2 = 100000f;
        public float minimumLandAreaKm2 = 1f;
        public bool climateGdpDamageEnabled = true;
        public float climateGdpDamageMultiplier = 0.90f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class EmissionsSettings
    {
        public bool enabled = true;
        public float neutralRating = 10f;
        public float co2TonsPerGdpBillionAtScoreZero = 2000000f;
        public float co2DecayBase = 0.25f;
        public float maximumResourceIntensityMultiplier = 1f;
        public float methaneTonsPerMillionPeopleAtScoreZero = 51248.90f;
        public float methaneDecayBase = 0.90f;
        public float nitrousOxideTonsPerMillionPeopleAtScoreZero = 1452.04f;
        public float nitrousOxideDecayBase = 0.90f;
        public float monthsPerYear = 12f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class UnitySettings
    {
        public bool enabled = true;
        public float cohesionPopulationDivisor = 3333333f;
        public float educationPopulationDivisor = -33333f;
        public float educationAndGovernmentPenaltyPerLevel = 0.025f;
        public float minimumCohesionMultiplier = 0.50f;
        public float propagandaMultiplier = 0.20f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class SpoilsSettings
    {
        public bool enabled = true;
        public float governmentPopulationDivisor = -66667f;
        public float sustainabilityChangeAtReferenceGdp = 0.05f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float maximumResourceSustainabilityMultiplier = 2f;
        public float propagandaMultiplier = 0.20f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class SpoilsMoneySettings
    {
        public bool enabled = true;
        public float baseMoney = 60f;
        public float maximumResourceMultiplier = 4f;
        public float governmentBaseMultiplier = 1.30f;
        public float governmentPenaltyPerLevel = 0.03f;
        public float fullGovernment = 10f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class RegionThresholdSettings
    {
        public bool enabled = true;
        public float multiplier = 5f;
        public float oilRemovalInvestmentPoints = 500f;
        public float miningRemovalInvestmentPoints = 750f;
        public float economicUpgradeInvestmentPoints = 1200f;
        public float decolonizationInvestmentPoints = 1000f;
        public float falloutCleanupInvestmentPoints = 100f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class UiSettings
    {
        public bool enabled = true;
        public bool expandedTooltips = true;
    }

    internal static class SettingsValidator
    {
        public static void Validate(Settings value, Action<string> log)
        {
            Settings defaults = new Settings();
            value.investment = value.investment ?? defaults.investment;
            value.economy = value.economy ?? defaults.economy;
            value.technology = value.technology ?? defaults.technology;
            value.abundance = value.abundance ?? defaults.abundance;
            value.inequality = value.inequality ?? defaults.inequality;
            value.cohesionRest = value.cohesionRest ?? defaults.cohesionRest;
            value.controlCost = value.controlCost ?? defaults.controlCost;
            value.councilors = value.councilors ?? defaults.councilors;
            value.army = value.army ?? defaults.army;
            value.shipBalance = value.shipBalance ?? defaults.shipBalance;
            value.research = value.research ?? defaults.research;
            value.knowledge = value.knowledge ?? defaults.knowledge;
            value.government = value.government ?? defaults.government;
            value.military = value.military ?? defaults.military;
            value.nationalMergers = value.nationalMergers ?? defaults.nationalMergers;
            value.claimHarmonization = value.claimHarmonization ?? defaults.claimHarmonization;
            value.oppression = value.oppression ?? defaults.oppression;
            value.environment = value.environment ?? defaults.environment;
            value.emissions = value.emissions ?? defaults.emissions;
            value.unity = value.unity ?? defaults.unity;
            value.spoils = value.spoils ?? defaults.spoils;
            value.spoilsMoney = value.spoilsMoney ?? defaults.spoilsMoney;
            value.regionThresholds = value.regionThresholds ?? defaults.regionThresholds;
            value.ui = value.ui ?? defaults.ui;

            RepairPositive(ref value.investment.gdpPerInvestmentPointBillions, defaults.investment.gdpPerInvestmentPointBillions, "investment.gdpPerInvestmentPointBillions", log);
            RepairRange(ref value.investment.lowIncomeMultiplierAtZero, defaults.investment.lowIncomeMultiplierAtZero, 0f, 1f, "investment.lowIncomeMultiplierAtZero", log);
            RepairPositive(ref value.investment.lowIncomeThreshold, defaults.investment.lowIncomeThreshold, "investment.lowIncomeThreshold", log);
            RepairPositive(ref value.investment.outputMultiplier, defaults.investment.outputMultiplier, "investment.outputMultiplier", log);
            RepairPositive(ref value.economy.baseGainBillions, defaults.economy.baseGainBillions, "economy.baseGainBillions", log);
            RepairNonNegative(ref value.economy.educationPerLevel, defaults.economy.educationPerLevel, "economy.educationPerLevel", log);
            RepairNonNegative(ref value.economy.governmentPerLevel, defaults.economy.governmentPerLevel, "economy.governmentPerLevel", log);
            RepairFinite(ref value.economy.cohesionCenter, defaults.economy.cohesionCenter, "economy.cohesionCenter", log);
            RepairPositive(ref value.economy.cohesionPeak, defaults.economy.cohesionPeak, "economy.cohesionPeak", log);
            RepairNonNegative(ref value.economy.cohesionPenaltyPerPoint, defaults.economy.cohesionPenaltyPerPoint, "economy.cohesionPenaltyPerPoint", log);
            RepairNonNegative(ref value.economy.referenceCoreRegions, defaults.economy.referenceCoreRegions, "economy.referenceCoreRegions", log);
            RepairNonNegative(ref value.economy.referenceEducation, defaults.economy.referenceEducation, "economy.referenceEducation", log);
            RepairNonNegative(ref value.economy.referenceGovernment, defaults.economy.referenceGovernment, "economy.referenceGovernment", log);
            RepairFinite(ref value.economy.referenceCohesion, defaults.economy.referenceCohesion, "economy.referenceCohesion", log);
            RepairPositive(ref value.economy.laborKneePcgdp, defaults.economy.laborKneePcgdp, "economy.laborKneePcgdp", log);
            RepairPositive(ref value.economy.resourceKneePcgdp, defaults.economy.resourceKneePcgdp, "economy.resourceKneePcgdp", log);
            RepairRange(ref value.economy.startingLaborReturnFloor, defaults.economy.startingLaborReturnFloor, 0f, 1f, "economy.startingLaborReturnFloor", log);
            RepairRange(ref value.economy.startingResourceReturnFloor, defaults.economy.startingResourceReturnFloor, 0f, 1f, "economy.startingResourceReturnFloor", log);
            RepairRange(ref value.economy.technologyReliefLinearShare, defaults.economy.technologyReliefLinearShare, 0f, 1f, "economy.technologyReliefLinearShare", log);
            RepairPositive(ref value.economy.minimumSupport, defaults.economy.minimumSupport, "economy.minimumSupport", log);
            RepairPositive(ref value.economy.laborPressureExponent, defaults.economy.laborPressureExponent, "economy.laborPressureExponent", log);
            RepairPositive(ref value.economy.resourcePressureExponent, defaults.economy.resourcePressureExponent, "economy.resourcePressureExponent", log);
            RepairNonNegative(ref value.economy.resourceDirectLift, defaults.economy.resourceDirectLift, "economy.resourceDirectLift", log);
            RepairNonNegative(ref value.economy.landDirectLift, defaults.economy.landDirectLift, "economy.landDirectLift", log);
            RepairNonNegative(ref value.economy.coreRegionMaximumBonus, defaults.economy.coreRegionMaximumBonus, "economy.coreRegionMaximumBonus", log);
            RepairPositive(ref value.economy.coreRegionHalfSaturation, defaults.economy.coreRegionHalfSaturation, "economy.coreRegionHalfSaturation", log);
            RepairRange(ref value.technology.maximumMultiplier, defaults.technology.maximumMultiplier, 1f, 100f, "technology.maximumMultiplier", log);
            RepairPositive(ref value.technology.researchCostMultiplier, defaults.technology.researchCostMultiplier, "technology.researchCostMultiplier", log);
            RepairPositive(ref value.technology.projectResearchCostMultiplier, defaults.technology.projectResearchCostMultiplier, "technology.projectResearchCostMultiplier", log);
            RepairRange(ref value.technology.aiSelectionCostExponent, defaults.technology.aiSelectionCostExponent, 0.1f, 2f, "technology.aiSelectionCostExponent", log);
            RepairPositive(ref value.technology.aiSelectionMinimumCostMultiplier, defaults.technology.aiSelectionMinimumCostMultiplier, "technology.aiSelectionMinimumCostMultiplier", log);
            RepairPositive(ref value.technology.aiSelectionMaximumCostMultiplier, defaults.technology.aiSelectionMaximumCostMultiplier, "technology.aiSelectionMaximumCostMultiplier", log);
            if (value.technology.aiSelectionMaximumCostMultiplier <
                value.technology.aiSelectionMinimumCostMultiplier)
            {
                log("Invalid technology AI selection cost-multiplier bounds; restored safe 0.25/4 defaults.");
                value.technology.aiSelectionMinimumCostMultiplier =
                    defaults.technology.aiSelectionMinimumCostMultiplier;
                value.technology.aiSelectionMaximumCostMultiplier =
                    defaults.technology.aiSelectionMaximumCostMultiplier;
            }
            RepairPositive(ref value.abundance.referenceGdpPerResourceRegionBillions, defaults.abundance.referenceGdpPerResourceRegionBillions, "abundance.referenceGdpPerResourceRegionBillions", log);
            RepairPositive(ref value.abundance.minimumGdpBillions, defaults.abundance.minimumGdpBillions, "abundance.minimumGdpBillions", log);
            RepairNonNegative(ref value.abundance.resourceMaximumBonus, defaults.abundance.resourceMaximumBonus, "abundance.resourceMaximumBonus", log);
            RepairPositive(ref value.abundance.minimumDensity, defaults.abundance.minimumDensity, "abundance.minimumDensity", log);
            RepairPositive(ref value.abundance.referenceDensity, defaults.abundance.referenceDensity, "abundance.referenceDensity", log);
            RepairNonNegative(ref value.abundance.landMaximumBonus, defaults.abundance.landMaximumBonus, "abundance.landMaximumBonus", log);
            RepairPositive(ref value.abundance.resourceCurveExponent, defaults.abundance.resourceCurveExponent, "abundance.resourceCurveExponent", log);
            RepairPositive(ref value.abundance.landCurveExponent, defaults.abundance.landCurveExponent, "abundance.landCurveExponent", log);
            RepairPositive(ref value.abundance.landReferencePcgdp, defaults.abundance.landReferencePcgdp, "abundance.landReferencePcgdp", log);
            RepairRange(ref value.abundance.landMinimumWealthRelevance, defaults.abundance.landMinimumWealthRelevance, 0f, 1f, "abundance.landMinimumWealthRelevance", log);
            RepairPositive(ref value.abundance.unrestExponent, defaults.abundance.unrestExponent, "abundance.unrestExponent", log);
            RepairPositive(ref value.abundance.maximumUnrest, defaults.abundance.maximumUnrest, "abundance.maximumUnrest", log);
            RepairRange(ref value.inequality.exponent, defaults.inequality.exponent, 1f, 10f, "inequality.exponent", log);
            RepairRange(ref value.inequality.maximumDirectionalMultiplier, defaults.inequality.maximumDirectionalMultiplier, 1f, 100f, "inequality.maximumDirectionalMultiplier", log);
            RepairNonNegative(ref value.inequality.climateChangeMultiplier, defaults.inequality.climateChangeMultiplier, "inequality.climateChangeMultiplier", log);
            RepairNegative(ref value.inequality.coupInequalityChange, defaults.inequality.coupInequalityChange, "inequality.coupInequalityChange", log);
            if (!IsFinite(value.inequality.minimum) || !IsFinite(value.inequality.neutral) ||
                !IsFinite(value.inequality.maximum) || value.inequality.minimum >= value.inequality.neutral ||
                value.inequality.neutral >= value.inequality.maximum)
            {
                log("Invalid inequality bounds; restored safe 1/5/9 defaults.");
                value.inequality.minimum = defaults.inequality.minimum;
                value.inequality.neutral = defaults.inequality.neutral;
                value.inequality.maximum = defaults.inequality.maximum;
            }
            RepairPositive(ref value.inequality.referenceGdpBillions, defaults.inequality.referenceGdpBillions, "inequality.referenceGdpBillions", log);
            RepairPositive(ref value.inequality.minimumGdpBillions, defaults.inequality.minimumGdpBillions, "inequality.minimumGdpBillions", log);
            RepairPositive(ref value.inequality.economyChangeAtReferenceGdp, defaults.inequality.economyChangeAtReferenceGdp, "inequality.economyChangeAtReferenceGdp", log);
            RepairNegative(ref value.inequality.welfareChangeAtReferenceGdp, defaults.inequality.welfareChangeAtReferenceGdp, "inequality.welfareChangeAtReferenceGdp", log);
            RepairPositive(ref value.inequality.spoilsChangeAtReferenceGdp, defaults.inequality.spoilsChangeAtReferenceGdp, "inequality.spoilsChangeAtReferenceGdp", log);
            RepairNonNegative(ref value.inequality.economyMaximumResourceMultiplier, defaults.inequality.economyMaximumResourceMultiplier, "inequality.economyMaximumResourceMultiplier", log);
            RepairNonNegative(ref value.inequality.spoilsMaximumResourceMultiplier, defaults.inequality.spoilsMaximumResourceMultiplier, "inequality.spoilsMaximumResourceMultiplier", log);
            RepairNonNegative(ref value.cohesionRest.baseValue, defaults.cohesionRest.baseValue, "cohesionRest.baseValue", log);
            RepairRange(ref value.cohesionRest.anocracyDemocracyBoundary, defaults.cohesionRest.anocracyDemocracyBoundary, 0f, 10f, "cohesionRest.anocracyDemocracyBoundary", log);
            RepairRange(ref value.cohesionRest.autocracyAnocracyBoundary, defaults.cohesionRest.autocracyAnocracyBoundary, 0f, value.cohesionRest.anocracyDemocracyBoundary, "cohesionRest.autocracyAnocracyBoundary", log);
            RepairPositive(ref value.cohesionRest.autocracyExponent, defaults.cohesionRest.autocracyExponent, "cohesionRest.autocracyExponent", log);
            RepairNonNegative(ref value.cohesionRest.democracyCoefficient, defaults.cohesionRest.democracyCoefficient, "cohesionRest.democracyCoefficient", log);
            RepairRange(ref value.cohesionRest.inequalityEducationBaseMultiplier, defaults.cohesionRest.inequalityEducationBaseMultiplier, 0f, 1f, "cohesionRest.inequalityEducationBaseMultiplier", log);
            RepairPositive(ref value.cohesionRest.inequalityEducationDivisor, defaults.cohesionRest.inequalityEducationDivisor, "cohesionRest.inequalityEducationDivisor", log);
            RepairFinite(ref value.cohesionRest.inequalityOffset, defaults.cohesionRest.inequalityOffset, "cohesionRest.inequalityOffset", log);
            RepairPositive(ref value.cohesionRest.inequalityCoefficient, defaults.cohesionRest.inequalityCoefficient, "cohesionRest.inequalityCoefficient", log);
            RepairPositive(ref value.cohesionRest.publicEliteGovernmentDivisor, defaults.cohesionRest.publicEliteGovernmentDivisor, "cohesionRest.publicEliteGovernmentDivisor", log);
            RepairPositive(ref value.controlCost.countryCostMultiplier, defaults.controlCost.countryCostMultiplier, "controlCost.countryCostMultiplier", log);
            RepairRange(ref value.controlCost.arrivalInternationalRelationsReduction, defaults.controlCost.arrivalInternationalRelationsReduction, 0f, 0.99f, "controlCost.arrivalInternationalRelationsReduction", log);
            RepairRange(ref value.controlCost.unityMovementsReduction, defaults.controlCost.unityMovementsReduction, 0f, 0.99f, "controlCost.unityMovementsReduction", log);
            RepairRange(ref value.controlCost.greatNationsReduction, defaults.controlCost.greatNationsReduction, 0f, 0.99f, "controlCost.greatNationsReduction", log);
            RepairRange(ref value.controlCost.arrivalGovernanceReduction, defaults.controlCost.arrivalGovernanceReduction, 0f, 0.99f, "controlCost.arrivalGovernanceReduction", log);
            RepairRange(ref value.controlCost.accelerandoReduction, defaults.controlCost.accelerandoReduction, 0f, 0.99f, "controlCost.accelerandoReduction", log);
            RepairRange(ref value.councilors.totalAttributeCap, defaults.councilors.totalAttributeCap, 25f, 100f, "councilors.totalAttributeCap", log);
            RepairRange(ref value.councilors.maximumOrganizations, defaults.councilors.maximumOrganizations, 1f, 100f, "councilors.maximumOrganizations", log);
            RepairPositive(ref value.army.costCoefficient, defaults.army.costCoefficient, "army.costCoefficient", log);
            RepairRange(ref value.army.costGrowthBase, defaults.army.costGrowthBase, 1f, 10f, "army.costGrowthBase", log);
            RepairRange(ref value.army.repairShare, defaults.army.repairShare, 0f, 1f, "army.repairShare", log);
            RepairPositive(ref value.army.homeUpkeepDivisor, defaults.army.homeUpkeepDivisor, "army.homeUpkeepDivisor", log);
            RepairPositive(ref value.army.awayUpkeepDivisor, defaults.army.awayUpkeepDivisor, "army.awayUpkeepDivisor", log);
            RepairRange(ref value.army.combatModifierScale, defaults.army.combatModifierScale, 0f, 1f, "army.combatModifierScale", log);
            RepairNonNegative(ref value.army.maximumStrengthPenalty, defaults.army.maximumStrengthPenalty, "army.maximumStrengthPenalty", log);
            RepairPositive(ref value.army.hitCurveBase, defaults.army.hitCurveBase, "army.hitCurveBase", log);
            if (value.army.hitCurveBase <= 1f)
            {
                log("Invalid army.hitCurveBase; restored safe default.");
                value.army.hitCurveBase = defaults.army.hitCurveBase;
            }
            RepairPositive(ref value.army.alienFloraFullDamageLevel, defaults.army.alienFloraFullDamageLevel, "army.alienFloraFullDamageLevel", log);
            RepairPositive(ref value.army.megafaunaMaximumTechLevel, defaults.army.megafaunaMaximumTechLevel, "army.megafaunaMaximumTechLevel", log);
            RepairRange(ref value.shipBalance.openCycleDriveHeatFraction, defaults.shipBalance.openCycleDriveHeatFraction, 0f, 1f, "shipBalance.openCycleDriveHeatFraction", log);
            RepairPositive(ref value.shipBalance.crewSupportMass_tons, defaults.shipBalance.crewSupportMass_tons, "shipBalance.crewSupportMass_tons", log);
            RepairPositive(ref value.shipBalance.utilitySlotVolume_m3, defaults.shipBalance.utilitySlotVolume_m3, "shipBalance.utilitySlotVolume_m3", log);
            RepairPositive(ref value.shipBalance.hullWeaponSlotVolume_m3, defaults.shipBalance.hullWeaponSlotVolume_m3, "shipBalance.hullWeaponSlotVolume_m3", log);
            RepairPositive(ref value.shipBalance.noseWeaponSlotVolume_m3, defaults.shipBalance.noseWeaponSlotVolume_m3, "shipBalance.noseWeaponSlotVolume_m3", log);
            RepairPositive(ref value.shipBalance.crewPressurizedVolume_m3, defaults.shipBalance.crewPressurizedVolume_m3, "shipBalance.crewPressurizedVolume_m3", log);
            RepairPositive(ref value.research.coefficient, defaults.research.coefficient, "research.coefficient", log);
            RepairPositive(ref value.research.referencePcgdp, defaults.research.referencePcgdp, "research.referencePcgdp", log);
            RepairNonNegative(ref value.research.pcgdpOffset, defaults.research.pcgdpOffset, "research.pcgdpOffset", log);
            RepairPositive(ref value.research.educationExponent, defaults.research.educationExponent, "research.educationExponent", log);
            RepairPositive(ref value.research.democracyFloor, defaults.research.democracyFloor, "research.democracyFloor", log);
            RepairPositive(ref value.research.democracyExponent, defaults.research.democracyExponent, "research.democracyExponent", log);
            RepairFinite(ref value.research.cohesionCenter, defaults.research.cohesionCenter, "research.cohesionCenter", log);
            RepairPositive(ref value.research.cohesionPeak, defaults.research.cohesionPeak, "research.cohesionPeak", log);
            RepairNonNegative(ref value.research.cohesionPenaltyPerPoint, defaults.research.cohesionPenaltyPerPoint, "research.cohesionPenaltyPerPoint", log);
            RepairNonNegative(ref value.research.unrestGrace, defaults.research.unrestGrace, "research.unrestGrace", log);
            RepairPositive(ref value.research.unrestPenaltyDivisor, defaults.research.unrestPenaltyDivisor, "research.unrestPenaltyDivisor", log);
            RepairPositive(ref value.knowledge.educationPopulationDivisor, defaults.knowledge.educationPopulationDivisor, "knowledge.educationPopulationDivisor", log);
            RepairPositive(ref value.knowledge.educationMaximumGain, defaults.knowledge.educationMaximumGain, "knowledge.educationMaximumGain", log);
            RepairRange(ref value.knowledge.educationDecay, defaults.knowledge.educationDecay, 0.0001f, 1f, "knowledge.educationDecay", log);
            RepairPositive(ref value.knowledge.cohesionPopulationDivisor, defaults.knowledge.cohesionPopulationDivisor, "knowledge.cohesionPopulationDivisor", log);
            RepairFinite(ref value.knowledge.cohesionTarget, defaults.knowledge.cohesionTarget, "knowledge.cohesionTarget", log);
            RepairPositive(ref value.government.democracyPopulationDivisor, defaults.government.democracyPopulationDivisor, "government.democracyPopulationDivisor", log);
            RepairRange(ref value.government.boundaryCurveFactor, defaults.government.boundaryCurveFactor, 1f, 10f, "government.boundaryCurveFactor", log);
            RepairRange(ref value.government.passiveLowCohesionMultiplier, defaults.government.passiveLowCohesionMultiplier, 0f, 1f, "government.passiveLowCohesionMultiplier", log);
            RepairPositive(ref value.military.doctrineBaseCostAtTechOne, defaults.military.doctrineBaseCostAtTechOne, "military.doctrineBaseCostAtTechOne", log);
            RepairRange(ref value.military.doctrineCostGrowthBase, defaults.military.doctrineCostGrowthBase, 1f, 10f, "military.doctrineCostGrowthBase", log);
            RepairNonNegative(ref value.military.catchupGapCoefficient, defaults.military.catchupGapCoefficient, "military.catchupGapCoefficient", log);
            if (!IsFinite(value.nationalMergers.inequalityMinimum) ||
                !IsFinite(value.nationalMergers.inequalityMaximum) ||
                value.nationalMergers.inequalityMaximum -
                    value.nationalMergers.inequalityMinimum < 0.001f)
            {
                log("Invalid national-merger Inequality bounds; restored safe 1/9 defaults.");
                value.nationalMergers.inequalityMinimum = defaults.nationalMergers.inequalityMinimum;
                value.nationalMergers.inequalityMaximum = defaults.nationalMergers.inequalityMaximum;
            }
            RepairPositive(ref value.nationalMergers.minimumPerCapitaGdp, defaults.nationalMergers.minimumPerCapitaGdp, "nationalMergers.minimumPerCapitaGdp", log);
            RepairRange(ref value.nationalMergers.inequalityBoundaryEpsilon,
                defaults.nationalMergers.inequalityBoundaryEpsilon, 0.0000001f,
                Math.Min(0.1f, (value.nationalMergers.inequalityMaximum -
                    value.nationalMergers.inequalityMinimum) / 4f),
                "nationalMergers.inequalityBoundaryEpsilon", log);
            RepairNonNegative(ref value.claimHarmonization.ordinaryThreshold,
                defaults.claimHarmonization.ordinaryThreshold,
                "claimHarmonization.ordinaryThreshold", log);
            RepairNonNegative(ref value.claimHarmonization.historicalThreshold,
                defaults.claimHarmonization.historicalThreshold,
                "claimHarmonization.historicalThreshold", log);
            RepairNonNegative(ref value.claimHarmonization.federationThreshold,
                defaults.claimHarmonization.federationThreshold,
                "claimHarmonization.federationThreshold", log);
            if (value.claimHarmonization.historicalThreshold >
                value.claimHarmonization.ordinaryThreshold)
            {
                log("Historical claim threshold exceeded the ordinary threshold; restored 3/6 defaults.");
                value.claimHarmonization.historicalThreshold =
                    defaults.claimHarmonization.historicalThreshold;
                value.claimHarmonization.ordinaryThreshold =
                    defaults.claimHarmonization.ordinaryThreshold;
            }
            if (value.claimHarmonization.federationThreshold <
                Math.Max(value.claimHarmonization.ordinaryThreshold,
                    value.claimHarmonization.historicalThreshold))
            {
                log("Federation threshold was lower than an integration threshold; restored 12 default.");
                value.claimHarmonization.federationThreshold =
                    defaults.claimHarmonization.federationThreshold;
            }
            RepairPositive(ref value.oppression.unrestPopulationDivisor, defaults.oppression.unrestPopulationDivisor, "oppression.unrestPopulationDivisor", log);
            RepairPositive(ref value.oppression.fullDemocracy, defaults.oppression.fullDemocracy, "oppression.fullDemocracy", log);
            RepairPositive(ref value.environment.storageRatingOffset, defaults.environment.storageRatingOffset, "environment.storageRatingOffset", log);
            RepairRange(ref value.environment.startingTechnologyCap, defaults.environment.startingTechnologyCap, 0.1f, 10f, "environment.startingTechnologyCap", log);
            RepairRange(ref value.environment.maximumTechnologyCap, defaults.environment.maximumTechnologyCap, value.environment.startingTechnologyCap, 10f, "environment.maximumTechnologyCap", log);
            RepairPositive(ref value.environment.advancementBaseIp, defaults.environment.advancementBaseIp, "environment.advancementBaseIp", log);
            RepairRange(ref value.environment.advancementCostGrowthBase, defaults.environment.advancementCostGrowthBase, 1f, 10f, "environment.advancementCostGrowthBase", log);
            RepairPositive(ref value.environment.referenceGdpBillions, defaults.environment.referenceGdpBillions, "environment.referenceGdpBillions", log);
            RepairPositive(ref value.environment.minimumGdpBillions, defaults.environment.minimumGdpBillions, "environment.minimumGdpBillions", log);
            RepairNonNegative(ref value.environment.cleanupCO2_ppm, defaults.environment.cleanupCO2_ppm, "environment.cleanupCO2_ppm", log);
            RepairNonNegative(ref value.environment.cleanupCH4_ppm, defaults.environment.cleanupCH4_ppm, "environment.cleanupCH4_ppm", log);
            RepairNonNegative(ref value.environment.cleanupN2O_ppm, defaults.environment.cleanupN2O_ppm, "environment.cleanupN2O_ppm", log);
            RepairPositive(ref value.environment.falloutReferenceAreaKm2, defaults.environment.falloutReferenceAreaKm2, "environment.falloutReferenceAreaKm2", log);
            RepairPositive(ref value.environment.minimumLandAreaKm2, defaults.environment.minimumLandAreaKm2, "environment.minimumLandAreaKm2", log);
            RepairRange(ref value.environment.climateGdpDamageMultiplier, defaults.environment.climateGdpDamageMultiplier, 0f, 1f, "environment.climateGdpDamageMultiplier", log);
            RepairRange(ref value.emissions.neutralRating, defaults.emissions.neutralRating, 0.1f, 10f, "emissions.neutralRating", log);
            RepairPositive(ref value.emissions.co2TonsPerGdpBillionAtScoreZero, defaults.emissions.co2TonsPerGdpBillionAtScoreZero, "emissions.co2TonsPerGdpBillionAtScoreZero", log);
            RepairRange(ref value.emissions.co2DecayBase, defaults.emissions.co2DecayBase, 0.01f, 1f, "emissions.co2DecayBase", log);
            RepairRange(ref value.emissions.maximumResourceIntensityMultiplier, defaults.emissions.maximumResourceIntensityMultiplier, 1f, 100f, "emissions.maximumResourceIntensityMultiplier", log);
            RepairPositive(ref value.emissions.methaneTonsPerMillionPeopleAtScoreZero, defaults.emissions.methaneTonsPerMillionPeopleAtScoreZero, "emissions.methaneTonsPerMillionPeopleAtScoreZero", log);
            RepairRange(ref value.emissions.methaneDecayBase, defaults.emissions.methaneDecayBase, 0.01f, 1f, "emissions.methaneDecayBase", log);
            RepairPositive(ref value.emissions.nitrousOxideTonsPerMillionPeopleAtScoreZero, defaults.emissions.nitrousOxideTonsPerMillionPeopleAtScoreZero, "emissions.nitrousOxideTonsPerMillionPeopleAtScoreZero", log);
            RepairRange(ref value.emissions.nitrousOxideDecayBase, defaults.emissions.nitrousOxideDecayBase, 0.01f, 1f, "emissions.nitrousOxideDecayBase", log);
            RepairPositive(ref value.emissions.monthsPerYear, defaults.emissions.monthsPerYear, "emissions.monthsPerYear", log);
            RepairPositive(ref value.unity.cohesionPopulationDivisor, defaults.unity.cohesionPopulationDivisor, "unity.cohesionPopulationDivisor", log);
            RepairNegative(ref value.unity.educationPopulationDivisor, defaults.unity.educationPopulationDivisor, "unity.educationPopulationDivisor", log);
            RepairNonNegative(ref value.unity.educationAndGovernmentPenaltyPerLevel, defaults.unity.educationAndGovernmentPenaltyPerLevel, "unity.educationAndGovernmentPenaltyPerLevel", log);
            RepairRange(ref value.unity.minimumCohesionMultiplier, defaults.unity.minimumCohesionMultiplier, 0f, 1f, "unity.minimumCohesionMultiplier", log);
            RepairNonNegative(ref value.unity.propagandaMultiplier, defaults.unity.propagandaMultiplier, "unity.propagandaMultiplier", log);
            RepairNegative(ref value.spoils.governmentPopulationDivisor, defaults.spoils.governmentPopulationDivisor, "spoils.governmentPopulationDivisor", log);
            RepairPositive(ref value.spoils.sustainabilityChangeAtReferenceGdp, defaults.spoils.sustainabilityChangeAtReferenceGdp, "spoils.sustainabilityChangeAtReferenceGdp", log);
            RepairPositive(ref value.spoils.referenceGdpBillions, defaults.spoils.referenceGdpBillions, "spoils.referenceGdpBillions", log);
            RepairPositive(ref value.spoils.minimumGdpBillions, defaults.spoils.minimumGdpBillions, "spoils.minimumGdpBillions", log);
            RepairRange(ref value.spoils.maximumResourceSustainabilityMultiplier, defaults.spoils.maximumResourceSustainabilityMultiplier, 1f, 100f, "spoils.maximumResourceSustainabilityMultiplier", log);
            RepairNonNegative(ref value.spoils.propagandaMultiplier, defaults.spoils.propagandaMultiplier, "spoils.propagandaMultiplier", log);
            RepairNonNegative(ref value.spoilsMoney.baseMoney, defaults.spoilsMoney.baseMoney, "spoilsMoney.baseMoney", log);
            RepairRange(ref value.spoilsMoney.maximumResourceMultiplier, defaults.spoilsMoney.maximumResourceMultiplier, 1f, 100f, "spoilsMoney.maximumResourceMultiplier", log);
            RepairPositive(ref value.spoilsMoney.governmentBaseMultiplier, defaults.spoilsMoney.governmentBaseMultiplier, "spoilsMoney.governmentBaseMultiplier", log);
            RepairNonNegative(ref value.spoilsMoney.governmentPenaltyPerLevel, defaults.spoilsMoney.governmentPenaltyPerLevel, "spoilsMoney.governmentPenaltyPerLevel", log);
            RepairPositive(ref value.spoilsMoney.fullGovernment, defaults.spoilsMoney.fullGovernment, "spoilsMoney.fullGovernment", log);
            RepairPositive(ref value.regionThresholds.multiplier, defaults.regionThresholds.multiplier, "regionThresholds.multiplier", log);
            RepairPositive(ref value.regionThresholds.oilRemovalInvestmentPoints, defaults.regionThresholds.oilRemovalInvestmentPoints, "regionThresholds.oilRemovalInvestmentPoints", log);
            RepairPositive(ref value.regionThresholds.miningRemovalInvestmentPoints, defaults.regionThresholds.miningRemovalInvestmentPoints, "regionThresholds.miningRemovalInvestmentPoints", log);
            RepairPositive(ref value.regionThresholds.economicUpgradeInvestmentPoints, defaults.regionThresholds.economicUpgradeInvestmentPoints, "regionThresholds.economicUpgradeInvestmentPoints", log);
            RepairPositive(ref value.regionThresholds.decolonizationInvestmentPoints, defaults.regionThresholds.decolonizationInvestmentPoints, "regionThresholds.decolonizationInvestmentPoints", log);
            RepairPositive(ref value.regionThresholds.falloutCleanupInvestmentPoints, defaults.regionThresholds.falloutCleanupInvestmentPoints, "regionThresholds.falloutCleanupInvestmentPoints", log);
        }

        private static void RepairPositive(ref float value, float fallback, string name, Action<string> log)
        {
            if (!IsFinite(value) || value <= 0f)
            {
                log("Invalid " + name + "; restored default " + fallback + ".");
                value = fallback;
            }
        }

        private static void RepairRange(ref float value, float fallback, float minimum, float maximum, string name, Action<string> log)
        {
            if (!IsFinite(value) || value < minimum || value > maximum)
            {
                log("Invalid " + name + "; restored default " + fallback + ".");
                value = fallback;
            }
        }

        private static void RepairNonNegative(ref float value, float fallback, string name, Action<string> log)
        {
            if (!IsFinite(value) || value < 0f)
            {
                log("Invalid " + name + "; restored default " + fallback + ".");
                value = fallback;
            }
        }

        private static void RepairNegative(ref float value, float fallback, string name, Action<string> log)
        {
            if (!IsFinite(value) || value >= 0f)
            {
                log("Invalid " + name + "; restored default " + fallback + ".");
                value = fallback;
            }
        }

        private static void RepairFinite(ref float value, float fallback, string name, Action<string> log)
        {
            if (!IsFinite(value))
            {
                log("Invalid " + name + "; restored default " + fallback + ".");
                value = fallback;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
