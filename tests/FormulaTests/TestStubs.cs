using System;
using System.Collections.Generic;

namespace HarmonyLib
{
    public enum MethodType { Getter }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type, string methodName, MethodType methodType) { }
        public HarmonyPatch(Type type, string methodName) { }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HarmonyPrefix : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HarmonyPostfix : Attribute { }
}

namespace PavonisInteractive.TerraInvicta
{
    public sealed class TIMegafaunaArmyState
    {
        public float bonusTechLevel;
    }

    public sealed class TIPowerPlantTemplate
    {
        public float efficiency;
    }

    public sealed class TISpaceShipTemplate
    {
        public int crewBillets;
    }

    public sealed class GlobalResearchState
    {
        public readonly HashSet<string> finishedTechsNames = new HashSet<string>(StringComparer.Ordinal);
    }

    public static class GameStateManager
    {
        public static readonly GlobalResearchState Research = new GlobalResearchState();
        public static readonly TITimeState TimeState = new TITimeState();
        public static GlobalResearchState GlobalResearch() { return Research; }
        public static TITimeState Time() { return TimeState; }
    }

    public sealed class TITimeState
    {
        public TIStartTimeTemplate template = new TIStartTimeTemplate();
    }

    public sealed class TIStartTimeTemplate
    {
        public float CPMaintenanceModifier = 1f;
    }

    public sealed class TINationState
    {
        public double GDP;
        public float perCapitaGDP;
        public float economyScore;
        public int numControlPoints;
        public float population_Millions;
        public float population;
        public float economyPriorityPerCapitaIncomeChange;
        public float education;
        public float democracy;
        public float cohesion;
        public float unrest;
        public float adviserScienceBonus;
        public float inequality;
        public int numCoreEconomicRegions_dailyCache;
        public int currentResourceRegions;
        public float populationDesnity_pop_km2;
        public float militaryTechLevel;
        public float maxMilitaryTechLevel;
        public int numNavies;
        public float sustainability;
        public readonly List<TIArmyState> armies = new List<TIArmyState>();
        public readonly List<TIRegionState> regions = new List<TIRegionState>();

        public enum InequalityChangeReason
        {
            InqReason_ClimateChange,
            InqReason_Annexation
        }

        public enum GDPChangeReason
        {
            GDPReason_EconomyPriority
        }

        public void ModifyGDP(double value, GDPChangeReason reason)
        {
            GDP += value;
        }

        public void AddToMilitaryTechLevel(float value)
        {
            militaryTechLevel += value;
        }

        public void AddToInequality(float value, InequalityChangeReason reason)
        {
            inequality = Math.Max(1f, Math.Min(9f, inequality + value));
        }
    }

    public sealed class TIRegionState
    {
        public float area_km2;
        public int nuclearDetonations;
    }

    public sealed class TIArmyState
    {
        public bool useHomeInvestmentFactor;
        public TINationState homeNation;
    }

    public sealed class TIFactionState
    {
        public bool IsAlienFaction;
    }

    public enum Context
    {
        ControlPointMaintenance,
        Environment_SustainabilityChange,
        Welfare_CO2_ppm,
        Welfare_CH4_ppm,
        Welfare_N2O_ppm
    }

    public sealed class TIGlobalConfig
    {
        public float WelCO2_ppm = -0.001f;
        public float WelCH4_ppm = -0.002f;
        public float WelN2O_ppm = -0.003f;
    }

    public static class TemplateManager
    {
        public static readonly TIGlobalConfig global = new TIGlobalConfig();
    }

    public static class TIEffectsState
    {
        public static readonly List<TIEffectTemplate> FactionEffects =
            new List<TIEffectTemplate>();

        public static List<TIEffectTemplate> GetFactionEffectsForContext(
            Context context, TIFactionState faction)
        {
            return new List<TIEffectTemplate>(FactionEffects);
        }

        public static float SumEffectsModifiers(
            Context context, TINationState nation, float baseValue, object target)
        {
            return 0f;
        }
    }
}

public sealed class TIEffectTemplate
{
    public string dataName;
    public float value;
}

namespace TIEconomyMod
{
    public static class Main
    {
        public static bool enabled = true;
        public static Settings settings = new Settings();
        public static TechWeightCatalog techWeights;
        public static readonly List<string> Warnings = new List<string>();

        public static bool FeatureEnabled(bool featureEnabled)
        {
            return enabled && settings.enabled && featureEnabled;
        }

        public static void Warn(string value) { Warnings.Add(value); }
    }

    public sealed class Settings
    {
        public bool enabled = true;
        public InvestmentSettings investment = new InvestmentSettings();
        public EconomySettings economy = new EconomySettings();
        public TechnologySettings technology = new TechnologySettings();
        public AbundanceSettings abundance = new AbundanceSettings();
        public InequalitySettings inequality = new InequalitySettings();
        public ControlCostSettings controlCost = new ControlCostSettings();
        public ArmySettings army = new ArmySettings();
        public ShipBalanceSettings shipBalance = new ShipBalanceSettings();
        public ResearchSettings research = new ResearchSettings();
        public KnowledgeSettings knowledge = new KnowledgeSettings();
        public GovernmentSettings government = new GovernmentSettings();
        public MilitarySettings military = new MilitarySettings();
        public NationalMergerSettings nationalMergers = new NationalMergerSettings();
        public OppressionSettings oppression = new OppressionSettings();
        public EnvironmentSettings environment = new EnvironmentSettings();
        public EmissionsSettings emissions = new EmissionsSettings();
        public UnitySettings unity = new UnitySettings();
        public SpoilsSettings spoils = new SpoilsSettings();
        public SpoilsMoneySettings spoilsMoney = new SpoilsMoneySettings();
    }

    public sealed class InvestmentSettings
    {
        public bool enabled = true;
        public float gdpPerInvestmentPointBillions = 100f;
        public float lowIncomeMultiplierAtZero = 0.70f;
        public float lowIncomeThreshold = 15000f;
        public float outputMultiplier = 1.05f;
    }

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

    public sealed class TechnologySettings
    {
        public bool enabled = true;
        public float maximumMultiplier = 4f;
        public bool researchCostEnabled = true;
        public float researchCostMultiplier = 1.40f;
    }

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
        public float economyChangeAtReferenceGdp = 0.0005f;
        public float welfareChangeAtReferenceGdp = -0.00666666f;
        public float spoilsChangeAtReferenceGdp = 0.00333334f;
        public float climateChangeMultiplier = 2f;
        public float economyMaximumResourceMultiplier = 0.60f;
        public float spoilsMaximumResourceMultiplier = 1f;
    }

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

    public sealed class ArmySettings
    {
        public bool enabled = true;
        public float homeBaseCost = 0.5f;
        public float awayBaseCost = 1f;
        public float technologyBaseline = 3f;
        public float costPerTechnologyLevel = 2f;
        public bool megafaunaEnabled = true;
        public float megafaunaMaximumTechLevel = 5f;
    }

    public sealed class ShipBalanceSettings
    {
        public bool enabled = true;
        public bool correctPowerPlantWasteHeat = true;
        public bool openCycleResidualHeatEnabled = true;
        public float openCycleDriveHeatFraction = 0.01f;
        public bool crewSupportMassEnabled = true;
        public float crewSupportMass_tons = 3f;
    }

    public sealed class ResearchSettings
    {
        public bool enabled = true;
        public bool neutralControlPointResearchEnabled = true;
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

    public sealed class KnowledgeSettings
    {
        public bool enabled = true;
        public float educationPopulationDivisor = 166667f;
        public float educationMaximumGain = 4f;
        public float educationDecay = 0.87f;
        public float cohesionPopulationDivisor = 333333f;
        public float cohesionTarget = 5f;
    }

    public sealed class GovernmentSettings
    {
        public bool enabled = true;
        public float democracyPopulationDivisor = 166667f;
    }

    public sealed class MilitarySettings
    {
        public bool enabled = true;
        public float technologyChangeForOneArmy = 0.00275f;
        public float catchupBonus = 0.5f;
    }

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

    public sealed class OppressionSettings
    {
        public bool enabled = true;
        public float unrestPopulationDivisor = 2222222f;
        public float fullDemocracy = 10f;
    }

    public sealed class EnvironmentSettings
    {
        public bool enabled = true;
        public float cleanupAtReferenceGdp = 0.10f;
        public float referenceGdpBillions = 100f;
        public float minimumGdpBillions = 1f;
        public float falloutReferenceAreaKm2 = 100000f;
        public float minimumLandAreaKm2 = 1f;
        public float atmosphericRemovalMultiplier = 1f;
        public bool climateGdpDamageEnabled = true;
        public float climateGdpDamageMultiplier = 0.90f;
    }

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

    public sealed class UnitySettings
    {
        public bool enabled = true;
        public float cohesionPopulationDivisor = 3333333f;
        public float educationPopulationDivisor = -33333f;
        public float educationAndGovernmentPenaltyPerLevel = 0.025f;
        public float minimumCohesionMultiplier = 0.50f;
        public float propagandaMultiplier = 0.20f;
    }

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

    public sealed class SpoilsMoneySettings
    {
        public bool enabled = true;
        public float baseMoney = 60f;
        public float maximumResourceMultiplier = 4f;
        public float governmentBaseMultiplier = 1.30f;
        public float governmentPenaltyPerLevel = 0.03f;
        public float fullGovernment = 10f;
    }
}

public sealed class TITechTemplate
{
}
