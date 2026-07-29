using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    internal static class IndependentResearchRuntime
    {
        private const float VisibleResearchEpsilon = 0.0001f;

        public static bool Enabled
        {
            get
            {
                ResearchSettings settings = Main.settings != null
                    ? Main.settings.research
                    : null;
                return settings != null &&
                    Main.FeatureEnabled(settings.enabled) &&
                    settings.neutralControlPointResearchEnabled;
            }
        }

        public static float MonthlyResearch()
        {
            double total = 0d;
            foreach (TINationState nation in GameStateManager.AllExtantHumanNations())
            {
                if (nation == null || nation.controlPoints == null)
                {
                    continue;
                }

                int totalControlPoints = nation.numControlPoints;
                // Vanilla factions receive research only from owned Control Points
                // whose benefits are active. Count that set, then route its exact
                // complement to neutral research so no base share can enter both.
                int factionAllocatableControlPoints = nation.controlPoints.Count(
                    controlPoint => controlPoint != null &&
                        !IndependentResearchMath.IsNeutralResearchControlPoint(
                            controlPoint.owned,
                            controlPoint.benefitsDisabled));
                int neutralControlPoints =
                    IndependentResearchMath.NeutralControlPointCount(
                        totalControlPoints,
                        factionAllocatableControlPoints);
                total += IndependentResearchMath.MonthlyNeutralShare(
                    nation.research_month,
                    neutralControlPoints,
                    totalControlPoints);
            }

            if (double.IsNaN(total) || double.IsInfinity(total) || total <= 0d)
            {
                return 0f;
            }
            if (total > float.MaxValue)
            {
                Main.Warn("Independent national research exceeded the supported range; no independent research was added.");
                return 0f;
            }
            return (float)total;
        }

        public static float DailyPerTechnology()
        {
            return Enabled
                ? IndependentResearchMath.DailyPerGlobalTechnology(MonthlyResearch())
                : 0f;
        }

        public static float AttributedResearch(TechProgress progress)
        {
            if (progress == null || progress.factionContributions == null)
            {
                return 0f;
            }
            return progress.factionContributions.Values.Sum();
        }

        public static bool HasFactionContribution(TechProgress progress)
        {
            return progress != null &&
                progress.factionContributions != null &&
                progress.factionContributions.Values.Any(value => value > 0f);
        }

        public static float IndependentProgress(TechProgress progress)
        {
            return progress == null
                ? 0f
                : IndependentResearchMath.IndependentProgress(
                    progress.accumulatedResearch,
                    AttributedResearch(progress));
        }

        public static bool HasVisibleIndependentProgress(TechProgress progress)
        {
            return IndependentProgress(progress) > VisibleResearchEpsilon;
        }

        public static bool WaitingForFactionSponsor(TechProgress progress)
        {
            if (progress == null ||
                progress.techTemplate == null ||
                HasFactionContribution(progress) ||
                !HasVisibleIndependentProgress(progress))
            {
                return false;
            }

            float cost = progress.techTemplate.GetResearchCost(null);
            return progress.accumulatedResearch >=
                cost - IndependentResearchMath.CompletionEpsilon * 1.01f;
        }
    }

    [HarmonyPatch(typeof(JointResearchDailyUpdate), "Execute")]
    public static class IndependentGlobalResearchDailyPatch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            TIGlobalResearchState globalResearch =
                GameStateManager.GlobalResearch();
            float dailyPerTechnology = IndependentResearchRuntime.DailyPerTechnology();
            for (int slot = 0;
                slot < IndependentResearchMath.GlobalTechnologySlots;
                slot++)
            {
                TechProgress progress = globalResearch.GetTechProgress(slot);
                if (progress == null || progress.techTemplate == null)
                {
                    continue;
                }

                if (dailyPerTechnology > 0f &&
                    !TIGlobalResearchState.TechFinished(progress.techTemplate))
                {
                    progress.accumulatedResearch += dailyPerTechnology;
                }

                float cost = progress.techTemplate.GetResearchCost(null);
                progress.accumulatedResearch =
                    IndependentResearchMath.GuardUnattributedCompletion(
                        progress.accumulatedResearch,
                        cost,
                        IndependentResearchRuntime.HasFactionContribution(progress));
            }
        }
    }

    [HarmonyPatch(typeof(TIGlobalResearchState), "Leader")]
    public static class IndependentResearchLeaderPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TIGlobalResearchState __instance,
            int slot,
            ref TIFactionState __result)
        {
            TechProgress progress = __instance.GetTechProgress(slot);
            if (IndependentResearchRuntime.HasVisibleIndependentProgress(progress) &&
                !IndependentResearchRuntime.HasFactionContribution(progress))
            {
                __result = null;
            }
        }
    }

    [HarmonyPatch(typeof(TechProgress), "GetExpectedWinner")]
    public static class IndependentResearchExpectedWinnerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TechProgress __instance, ref TIFactionState __result)
        {
            if (!IndependentResearchRuntime.HasVisibleIndependentProgress(__instance) ||
                IndependentResearchRuntime.HasFactionContribution(__instance))
            {
                return true;
            }

            // Vanilla filters its final MaxBy call to factions with positive prior
            // contributions. An independently progressing technology may have none,
            // so show no projected winner until a faction actually joins the race.
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIGlobalResearchState), "TechCompletionDate")]
    public static class IndependentResearchCompletionDatePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            TIGlobalResearchState __instance,
            int slot,
            ref string __result)
        {
            if (!IndependentResearchRuntime.Enabled)
            {
                return true;
            }

            TIDateTime completionDate = TITimeState.Now();
            TechProgress progress = __instance.GetTechProgress(slot);
            float remaining = progress.techTemplate.GetResearchCost(null) -
                progress.accumulatedResearch;
            if (remaining > 0f)
            {
                float factionDaily = 0f;
                foreach (TIFactionState faction in GameStateManager.AllHumanFactions())
                {
                    factionDaily += faction.PointsToSlot(
                        slot,
                        faction.GetDailyIncome(FactionResource.Research),
                        faction.TotalResearchWeights(
                            faction.OrgProjectAllowed(),
                            faction.HabProjectAllowed()));
                }

                // Independent research cannot cross the finish line without at
                // least one real faction contribution. Do not promise a date when
                // no faction is currently scheduled to provide one.
                if (!IndependentResearchRuntime.HasFactionContribution(progress) &&
                    factionDaily <= 0f)
                {
                    __result = string.Empty;
                    return false;
                }

                float dailyRate = factionDaily +
                    IndependentResearchRuntime.DailyPerTechnology();
                if (dailyRate <= 0f ||
                    !completionDate.TryAddDays(remaining / dailyRate))
                {
                    __result = string.Empty;
                    return false;
                }
            }

            __result = completionDate.ToCustomDateString();
            return false;
        }
    }

    [HarmonyPatch(typeof(ResearchPanelController), "UpdatePanel")]
    public static class IndependentResearchPanelPatch
    {
        private static readonly Color32 IndependentResearchColor =
            new Color32(128, 128, 128, byte.MaxValue);

        [HarmonyPostfix]
        public static void Postfix(
            ResearchPanelController __instance,
            TIFactionState faction)
        {
            if (__instance.slot > 2 ||
                __instance.forceSelectTechOverlay.enabled)
            {
                return;
            }

            TechProgress progress = GameStateManager.GlobalResearch()
                .GetTechProgress(__instance.slot);
            float independentProgress =
                IndependentResearchRuntime.IndependentProgress(progress);
            float dailyPerTechnology =
                IndependentResearchRuntime.DailyPerTechnology();
            if (independentProgress <= 0.0001f && dailyPerTechnology <= 0f)
            {
                return;
            }

            __instance.progressFraction.text += Loc.T(
                "UI.Science.Panel.EEOIndependentResearch",
                dailyPerTechnology.ToString("N2"));
            if (IndependentResearchRuntime.WaitingForFactionSponsor(progress))
            {
                __instance.progressFraction.text += Loc.T(
                    "UI.Science.Panel.EEOIndependentResearchAwaitingFaction");
            }

            if (independentProgress <= 0.0001f)
            {
                return;
            }

            List<TIFactionState> factions = progress.factionContributions.Keys
                .OrderBy(item => item.displayName)
                .ToList();
            int barCount = factions.Count + 1;
            __instance.factionContributionBar
                .SetListSize<FactionContributionBarListItemController>(barCount);

            int index = 0;
            foreach (object item in __instance.factionContributionBar)
            {
                FactionContributionBarListItemController bar =
                    item as FactionContributionBarListItemController;
                if (bar == null)
                {
                    continue;
                }

                if (index < factions.Count)
                {
                    bar.UpdateListItem(factions[index], progress, barCount);
                }
                else
                {
                    float cost = progress.techTemplate.GetResearchCost(faction);
                    float denominator = Math.Max(cost, progress.accumulatedResearch);
                    float fraction = denominator > 0f
                        ? independentProgress / denominator
                        : 0f;
                    float availableWidth = 540f - barCount * 2f;
                    bar.factionColor.color = IndependentResearchColor;
                    bar.thisRT.sizeDelta = new Vector2(
                        (int)Mathf.Clamp(
                            fraction * availableWidth,
                            1f,
                            540f),
                        bar.thisRT.sizeDelta.y);
                }
                index++;
            }
        }
    }
}
