using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.IO;
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
                new Harmony(modEntry.Info.Id).PatchAll();
                Log("Loaded TI Economic Equalization Overhaul 0.6.1 for the TI 1.0.47 API surface.");
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
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.ValidateAndRepair(Log);
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

        [Draw("Control Point Cost")]
        public ControlCostSettings controlCost = new ControlCostSettings();

        [Draw("Army Upkeep")]
        public ArmySettings army = new ArmySettings();

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
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class EconomySettings
    {
        public bool enabled = true;
        public float baseGainBillions = 0.330f;
        public float educationPerLevel = 0.15f;
        public float governmentPerLevel = 0.05f;
        public float cohesionCenter = 5f;
        public float cohesionPeak = 1.20f;
        public float cohesionPenaltyPerPoint = 0.04f;
        public float pcgdpScale = 6f;
        public float pcgdpDecay = 0.96f;
        public float pcgdpDecayInterval = 1000f;
        public float coreRegionMaximumBonus = 0.60f;
        public float coreRegionHalfSaturation = 2f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class TechnologySettings
    {
        public bool enabled = true;
        public float maximumMultiplier = 4f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class AbundanceSettings
    {
        public bool enabled = true;
        public float referenceGdpPerResourceRegionBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float resourceMaximumBonus = 1f;
        public float resourceCurveExponent = 1f;
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
        public float minimum = 1f;
        public float neutral = 5f;
        public float maximum = 9f;
        public float exponent = 2f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float economyChangeAtReferenceGdp = 0.00025f;
        public float welfareChangeAtReferenceGdp = -0.00333333f;
        public float spoilsChangeAtReferenceGdp = 0.00166667f;
        public float economyMaximumResourceMultiplier = 0.60f;
        public float spoilsMaximumResourceMultiplier = 1f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ControlCostSettings
    {
        public bool enabled = true;
        public float exponentOneTech = 0.98f;
        public float exponentTwoTechs = 0.95f;
        public float exponentThreeTechs = 0.90f;
        public float exponentFourTechs = 0.85f;
        public float exponentFiveTechs = 0.80f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ArmySettings
    {
        public bool enabled = true;
        public float homeBaseCost = 0.5f;
        public float awayBaseCost = 1f;
        public float technologyBaseline = 3f;
        public float costPerTechnologyLevel = 2f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class ResearchSettings
    {
        public bool enabled = true;
        public float coefficient = 0.0037f;
        public float referencePcgdp = 20000f;
        public float minimumPcgdpMultiplier = 0.60f;
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
        public float democracyPopulationDivisor = 166667f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class MilitarySettings
    {
        public bool enabled = true;
        public float technologyChangeForOneArmy = 0.00275f;
        public float catchupBonus = 0.5f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class NationalMergerSettings
    {
        public bool enabled = true;
        public bool militaryEnabled = true;
        public float militaryForceShare = 0.5f;
        public float navyArmyEquivalent = 1f;
        public bool inequalityEnabled = true;
        public float inequalityMinimum = 1f;
        public float inequalityMaximum = 9f;
        public float minimumPerCapitaGdp = 1f;
        public float inequalityBoundaryEpsilon = 0.000001f;
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
        public float cleanupAtReferenceGdp = 0.10f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float falloutReferenceAreaKm2 = 100000f;
        public float minimumLandAreaKm2 = 1f;
        public float atmosphericRemovalMultiplier = 1f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public sealed class EmissionsSettings
    {
        public bool enabled = true;
        public float tonsPerGdpBillion = 275000f;
        public float maximumResourceIntensityMultiplier = 1.25f;
        public float co2TonsMultiplier = 0.3292f;
        public float methaneTonsMultiplier = 0.00547619f;
        public float nitrousOxideTonsMultiplier = 0.000214533f;
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
            value.controlCost = value.controlCost ?? defaults.controlCost;
            value.army = value.army ?? defaults.army;
            value.research = value.research ?? defaults.research;
            value.knowledge = value.knowledge ?? defaults.knowledge;
            value.government = value.government ?? defaults.government;
            value.military = value.military ?? defaults.military;
            value.nationalMergers = value.nationalMergers ?? defaults.nationalMergers;
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
            RepairPositive(ref value.economy.baseGainBillions, defaults.economy.baseGainBillions, "economy.baseGainBillions", log);
            RepairNonNegative(ref value.economy.educationPerLevel, defaults.economy.educationPerLevel, "economy.educationPerLevel", log);
            RepairNonNegative(ref value.economy.governmentPerLevel, defaults.economy.governmentPerLevel, "economy.governmentPerLevel", log);
            RepairFinite(ref value.economy.cohesionCenter, defaults.economy.cohesionCenter, "economy.cohesionCenter", log);
            RepairPositive(ref value.economy.cohesionPeak, defaults.economy.cohesionPeak, "economy.cohesionPeak", log);
            RepairNonNegative(ref value.economy.cohesionPenaltyPerPoint, defaults.economy.cohesionPenaltyPerPoint, "economy.cohesionPenaltyPerPoint", log);
            RepairPositive(ref value.economy.pcgdpScale, defaults.economy.pcgdpScale, "economy.pcgdpScale", log);
            RepairRange(ref value.economy.pcgdpDecay, defaults.economy.pcgdpDecay, 0.0001f, 1f, "economy.pcgdpDecay", log);
            RepairPositive(ref value.economy.pcgdpDecayInterval, defaults.economy.pcgdpDecayInterval, "economy.pcgdpDecayInterval", log);
            RepairNonNegative(ref value.economy.coreRegionMaximumBonus, defaults.economy.coreRegionMaximumBonus, "economy.coreRegionMaximumBonus", log);
            RepairPositive(ref value.economy.coreRegionHalfSaturation, defaults.economy.coreRegionHalfSaturation, "economy.coreRegionHalfSaturation", log);
            RepairRange(ref value.technology.maximumMultiplier, defaults.technology.maximumMultiplier, 1f, 100f, "technology.maximumMultiplier", log);
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
            RepairRange(ref value.controlCost.exponentOneTech, defaults.controlCost.exponentOneTech, 0.01f, 1f, "controlCost.exponentOneTech", log);
            RepairRange(ref value.controlCost.exponentTwoTechs, defaults.controlCost.exponentTwoTechs, 0.01f, 1f, "controlCost.exponentTwoTechs", log);
            RepairRange(ref value.controlCost.exponentThreeTechs, defaults.controlCost.exponentThreeTechs, 0.01f, 1f, "controlCost.exponentThreeTechs", log);
            RepairRange(ref value.controlCost.exponentFourTechs, defaults.controlCost.exponentFourTechs, 0.01f, 1f, "controlCost.exponentFourTechs", log);
            RepairRange(ref value.controlCost.exponentFiveTechs, defaults.controlCost.exponentFiveTechs, 0.01f, 1f, "controlCost.exponentFiveTechs", log);
            RepairNonNegative(ref value.army.homeBaseCost, defaults.army.homeBaseCost, "army.homeBaseCost", log);
            RepairNonNegative(ref value.army.awayBaseCost, defaults.army.awayBaseCost, "army.awayBaseCost", log);
            RepairFinite(ref value.army.technologyBaseline, defaults.army.technologyBaseline, "army.technologyBaseline", log);
            RepairNonNegative(ref value.army.costPerTechnologyLevel, defaults.army.costPerTechnologyLevel, "army.costPerTechnologyLevel", log);
            RepairPositive(ref value.research.coefficient, defaults.research.coefficient, "research.coefficient", log);
            RepairPositive(ref value.research.referencePcgdp, defaults.research.referencePcgdp, "research.referencePcgdp", log);
            RepairNonNegative(ref value.research.minimumPcgdpMultiplier, defaults.research.minimumPcgdpMultiplier, "research.minimumPcgdpMultiplier", log);
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
            RepairPositive(ref value.military.technologyChangeForOneArmy, defaults.military.technologyChangeForOneArmy, "military.technologyChangeForOneArmy", log);
            RepairNonNegative(ref value.military.catchupBonus, defaults.military.catchupBonus, "military.catchupBonus", log);
            RepairRange(ref value.nationalMergers.militaryForceShare, defaults.nationalMergers.militaryForceShare, 0f, 1f, "nationalMergers.militaryForceShare", log);
            RepairNonNegative(ref value.nationalMergers.navyArmyEquivalent, defaults.nationalMergers.navyArmyEquivalent, "nationalMergers.navyArmyEquivalent", log);
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
            RepairPositive(ref value.oppression.unrestPopulationDivisor, defaults.oppression.unrestPopulationDivisor, "oppression.unrestPopulationDivisor", log);
            RepairPositive(ref value.oppression.fullDemocracy, defaults.oppression.fullDemocracy, "oppression.fullDemocracy", log);
            RepairPositive(ref value.environment.cleanupAtReferenceGdp, defaults.environment.cleanupAtReferenceGdp, "environment.cleanupAtReferenceGdp", log);
            RepairPositive(ref value.environment.referenceGdpBillions, defaults.environment.referenceGdpBillions, "environment.referenceGdpBillions", log);
            RepairPositive(ref value.environment.minimumGdpBillions, defaults.environment.minimumGdpBillions, "environment.minimumGdpBillions", log);
            RepairPositive(ref value.environment.falloutReferenceAreaKm2, defaults.environment.falloutReferenceAreaKm2, "environment.falloutReferenceAreaKm2", log);
            RepairPositive(ref value.environment.minimumLandAreaKm2, defaults.environment.minimumLandAreaKm2, "environment.minimumLandAreaKm2", log);
            RepairNonNegative(ref value.environment.atmosphericRemovalMultiplier, defaults.environment.atmosphericRemovalMultiplier, "environment.atmosphericRemovalMultiplier", log);
            RepairPositive(ref value.emissions.tonsPerGdpBillion, defaults.emissions.tonsPerGdpBillion, "emissions.tonsPerGdpBillion", log);
            RepairRange(ref value.emissions.maximumResourceIntensityMultiplier, defaults.emissions.maximumResourceIntensityMultiplier, 1f, 100f, "emissions.maximumResourceIntensityMultiplier", log);
            RepairNonNegative(ref value.emissions.co2TonsMultiplier, defaults.emissions.co2TonsMultiplier, "emissions.co2TonsMultiplier", log);
            RepairNonNegative(ref value.emissions.methaneTonsMultiplier, defaults.emissions.methaneTonsMultiplier, "emissions.methaneTonsMultiplier", log);
            RepairNonNegative(ref value.emissions.nitrousOxideTonsMultiplier, defaults.emissions.nitrousOxideTonsMultiplier, "emissions.nitrousOxideTonsMultiplier", log);
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
