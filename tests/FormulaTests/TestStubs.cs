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
    public sealed class GlobalResearchState
    {
        public readonly HashSet<string> finishedTechsNames = new HashSet<string>(StringComparer.Ordinal);
    }

    public static class GameStateManager
    {
        public static readonly GlobalResearchState Research = new GlobalResearchState();
        public static GlobalResearchState GlobalResearch() { return Research; }
    }

    public sealed class TINationState
    {
        public double GDP;
        public float perCapitaGDP;
        public float economyScore;
        public int numControlPoints;
        public float population_Millions;
        public float population;
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
            InqReason_Annexation
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

    public enum Context
    {
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
        public static float SumEffectsModifiers(
            Context context, TINationState nation, float baseValue, object target)
        {
            return 0f;
        }
    }
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
    }

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

    public sealed class TechnologySettings
    {
        public bool enabled = true;
        public float maximumMultiplier = 4f;
    }

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

    public sealed class ControlCostSettings
    {
        public bool enabled = true;
        public float exponentOneTech = 0.98f;
        public float exponentTwoTechs = 0.95f;
        public float exponentThreeTechs = 0.90f;
        public float exponentFourTechs = 0.85f;
        public float exponentFiveTechs = 0.80f;
    }

    public sealed class ArmySettings
    {
        public bool enabled = true;
        public float homeBaseCost = 0.5f;
        public float awayBaseCost = 1f;
        public float technologyBaseline = 3f;
        public float costPerTechnologyLevel = 2f;
    }

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
