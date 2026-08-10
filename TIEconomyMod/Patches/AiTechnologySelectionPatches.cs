using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod.Patches
{
    internal static class GlobalTechnologySelectionRuntime
    {
        public static bool TryScoreCandidates(
            TIFactionState faction,
            IList<TITechTemplate> candidates,
            out Dictionary<TITechTemplate, float> scores)
        {
            scores = null;
            if (faction == null || candidates == null || candidates.Count == 0)
            {
                return false;
            }

            TechnologySettings settings = Main.settings != null
                ? Main.settings.technology
                : null;
            if (settings == null)
            {
                return false;
            }

            Dictionary<TITechTemplate, double> costs =
                new Dictionary<TITechTemplate, double>();
            List<double> costValues = new List<double>();
            foreach (TITechTemplate candidate in candidates)
            {
                if (candidate == null)
                {
                    return false;
                }

                double cost = candidate.GetResearchCost(faction);
                if (double.IsNaN(cost) || double.IsInfinity(cost) || cost <= 0d)
                {
                    return false;
                }
                costs.Add(candidate, cost);
                costValues.Add(cost);
            }

            double medianCost = GlobalTechnologySelectionMath.Median(costValues);
            if (medianCost <= 0d)
            {
                return false;
            }

            bool shipBuilding = faction.shipBuilding;
            bool hasObjectiveProject = faction.HasObjectiveProjectAvailable();
            bool hasDominateMission = faction.GetAllPossibleMissions().Any(
                mission => mission.targetEffects.Any(
                    effect => effect is TIMissionEffect_Dominate));
            Dictionary<TITechTemplate, float> calculated =
                new Dictionary<TITechTemplate, float>();

            foreach (TITechTemplate candidate in candidates)
            {
                int effectiveTier = AIEvaluators.GetTechTier(candidate, faction);
                if (!hasObjectiveProject &&
                    candidate.LeadsToObjectiveProjects(faction))
                {
                    effectiveTier = Math.Max(effectiveTier, 5);
                }

                double costMultiplier =
                    GlobalTechnologySelectionMath.CostMultiplier(
                        costs[candidate],
                        medianCost,
                        settings.aiSelectionCostExponent,
                        settings.aiSelectionMinimumCostMultiplier,
                        settings.aiSelectionMaximumCostMultiplier);
                if (costMultiplier <= 0d)
                {
                    return false;
                }

                double contextMultiplier = 1d;
                if (candidate.AI_techRole == TechRole.SpaceWar && !shipBuilding)
                {
                    contextMultiplier *= 0.05d;
                }

                double controlPointValue = Math.Abs(candidate.Effects
                    .Where(effect => effect.GetContexts().Contains(
                        Context.ControlPointMaintenance))
                    .Sum(effect => (double)effect.value));
                if (controlPointValue > 0d)
                {
                    double capacityMultiplier = 1d + controlPointValue / 10d;
                    contextMultiplier *= capacityMultiplier;
                    if (hasDominateMission)
                    {
                        contextMultiplier *= 2d * capacityMultiplier;
                    }
                }

                double weight = GlobalTechnologySelectionMath.SelectionWeight(
                    faction.TechCategoryValuation(candidate.techCategory),
                    faction.TechRoleValuation(candidate.AI_techRole),
                    effectiveTier,
                    costMultiplier,
                    contextMultiplier);
                if (weight <= 0d || weight > float.MaxValue)
                {
                    return false;
                }
                calculated.Add(candidate, (float)weight);
            }

            scores = calculated;
            return true;
        }
    }

    [HarmonyPatch(
        typeof(AIEvaluators),
        nameof(AIEvaluators.SelectTech),
        new Type[]
        {
            typeof(TIFactionState),
            typeof(List<TITechTemplate>),
            typeof(bool)
        })]
    public static class GlobalTechnologySoftSelectionPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            TIFactionState faction,
            List<TITechTemplate> candidates,
            bool randomize,
            ref TITechTemplate __result)
        {
            TechnologySettings settings = Main.settings != null
                ? Main.settings.technology
                : null;
            if (settings == null ||
                !Main.FeatureEnabled(settings.aiSelectionEnabled))
            {
                return true;
            }

            Dictionary<TITechTemplate, float> scores;
            if (!GlobalTechnologySelectionRuntime.TryScoreCandidates(
                faction, candidates, out scores))
            {
                Main.Warn("Soft global-technology selection received invalid inputs; retaining vanilla selection.");
                return true;
            }

            if (randomize)
            {
                __result = scores.SelectRandomWeightedItem(
                    pair => pair.Value).Key;
            }
            else
            {
                TITechTemplate best = null;
                float bestScore = float.MinValue;
                foreach (TITechTemplate candidate in candidates)
                {
                    float score = scores[candidate];
                    if (best == null || score > bestScore)
                    {
                        best = candidate;
                        bestScore = score;
                    }
                }
                __result = best;
            }
            return false;
        }
    }
}
