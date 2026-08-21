using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    public sealed class ClaimHarmonizationEvaluation
    {
        public bool hasClaim;
        public bool historical;
        public bool hostile;
        public double threshold;
        public NationalHarmonizationResult harmonization;
    }

    public static class HistoricalClaimRegistry
    {
        private const string DarkSkies = "DarkSkies";
        private static readonly HashSet<string> HistoricalClaims =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> ExpansionProjects =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Project_RestoredWarsawPact",
                "Project_ForwardRussia",
                "Project_LiberatingMainlandChina"
            };
        private static bool initialized;
        private static bool darkSkiesActiveAtRefresh;

        public static bool IsHistorical(TINationState claimant,
            TIRegionState region)
        {
            EnsureCurrent();
            return claimant != null && region != null &&
                HistoricalClaims.Contains(Key(claimant.templateName,
                    region.templateName));
        }

        public static void RefreshAndApply()
        {
            Refresh();
            foreach (TINationState nation in
                GameStateManager.IterateByClass<TINationState>())
            {
                if (nation == null || nation.alienNation ||
                    nation.claims == null || nation.hostileClaims == null)
                {
                    continue;
                }

                foreach (TIRegionState region in nation.claims)
                {
                    if (region != null && region.nation != nation &&
                        HistoricalClaims.Contains(Key(nation.templateName,
                            region.templateName)) &&
                        !nation.hostileClaims.Contains(region))
                    {
                        nation.hostileClaims.Add(region);
                    }
                }
            }
        }

        private static void EnsureCurrent()
        {
            bool darkSkiesActive = IsDarkSkies2003Active();
            if (!initialized || darkSkiesActive != darkSkiesActiveAtRefresh)
            {
                Refresh();
            }
        }

        private static void Refresh()
        {
            HistoricalClaims.Clear();
            bool darkSkiesActive = IsDarkSkies2003Active();
            foreach (TIBilateralTemplate bilateral in
                TemplateManager.IterateByClass<TIBilateralTemplate>())
            {
                if (bilateral == null ||
                    bilateral.relationType != BilateralRelationType.Claim ||
                    string.IsNullOrEmpty(bilateral.nation1) ||
                    string.IsNullOrEmpty(bilateral.region1))
                {
                    continue;
                }

                bool is2003 = Is2003Identifier(bilateral.nation1) ||
                    Is2003Identifier(bilateral.region1);
                // Never resolve or query Dark Skies templates merely because
                // the DLC is installed. They are registered only for its active
                // scenario after TI has validated ownership.
                if (is2003 && !darkSkiesActive)
                {
                    continue;
                }
                if (!bilateral.BilateralIsInScenario())
                {
                    continue;
                }

                if (bilateral.hostileClaim || IsApprovedAddition(bilateral,
                    is2003))
                {
                    HistoricalClaims.Add(Key(bilateral.nation1,
                        bilateral.region1));
                }
            }
            initialized = true;
            darkSkiesActiveAtRefresh = darkSkiesActive;
            Main.Log("Registered " + HistoricalClaims.Count +
                " historical claim classifications for the active scenario.");
        }

        private static bool IsApprovedAddition(TIBilateralTemplate bilateral,
            bool is2003)
        {
            if (ExpansionProjects.Contains(bilateral.projectUnlockName))
            {
                return true;
            }

            string nation = RemoveScenarioPrefix(bilateral.nation1);
            string region = RemoveScenarioPrefix(bilateral.region1);
            switch (nation)
            {
            case "RUS":
                if (region == "Georgia" || region == "Moldova" ||
                    region == "Estonia" || region == "Latvia" ||
                    region == "Lithuania")
                {
                    return true;
                }
                return !is2003 && (region == "Donetsk" ||
                    region == "Kharkiv" || region == "Kiev" ||
                    region == "Odesa");
            case "CHN":
                return region == "Taiwan" ||
                    region == "ArunchalPradesh";
            case "PAK":
                return region == "JammuandKashmir";
            case "PRK":
                return region == "SouthKorea";
            case "KOR":
                return region == "NorthKorea";
            case "VEN":
                return region == "Guyana";
            case "JPN":
                return region == "SakhalinKurils";
            case "SYR":
                return region == "Lebanon";
            case "ERI":
                return region == "Mekelle";
            case "GTM":
                return region == "Belize";
            default:
                return false;
            }
        }

        private static bool IsDarkSkies2003Active()
        {
            if (!GameControl.DLCValidated || ModManager.dlcNames == null ||
                !ModManager.dlcNames.Contains(DarkSkies) ||
                GameControl.control == null ||
                GameControl.control.scenarioTemplate == null ||
                GameControl.control.scenarioTemplate.requiredDLC == null)
            {
                return false;
            }
            return GameControl.control.scenarioTemplate.requiredDLC.Contains(
                DarkSkies);
        }

        private static bool Is2003Identifier(string value)
        {
            return value != null && value.StartsWith("2003_",
                StringComparison.Ordinal);
        }

        private static string RemoveScenarioPrefix(string value)
        {
            string[] prefixes = { "2003_", "2026_", "2070_" };
            foreach (string prefix in prefixes)
            {
                if (value.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return value.Substring(prefix.Length);
                }
            }
            return value;
        }

        private static string Key(string nation, string region)
        {
            return nation + "|" + region;
        }
    }

    public static class ClaimHarmonizationEvaluator
    {
        public static ClaimHarmonizationEvaluation Evaluate(
            TINationState claimant, TIRegionState region)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            ClaimHarmonizationEvaluation evaluation =
                new ClaimHarmonizationEvaluation
                {
                    hasClaim = claimant != null && region != null &&
                        claimant.claims != null &&
                        claimant.claims.Contains(region),
                    historical = claimant != null && region != null &&
                        HistoricalClaimRegistry.IsHistorical(claimant, region),
                    hostile = true
                };
            evaluation.threshold = evaluation.historical
                ? settings.historicalThreshold
                : settings.ordinaryThreshold;

            if (claimant == null || region == null || !evaluation.hasClaim)
            {
                return evaluation;
            }
            TINationState target = region.nation;
            if (target == claimant)
            {
                evaluation.hostile = false;
                return evaluation;
            }
            if (target == null)
            {
                return evaluation;
            }

            evaluation.harmonization = NationalHarmonizationMath.Calculate(
                claimant.democracy, claimant.inequality, claimant.education,
                claimant.perCapitaGDP, claimant.cohesion, target.democracy,
                target.inequality, target.education, target.perCapitaGDP,
                target.unrest);
            evaluation.hostile = !NationalHarmonizationMath.Passes(
                evaluation.harmonization, evaluation.threshold);
            return evaluation;
        }

        public static bool IsHostile(TINationState claimant,
            TIRegionState region)
        {
            return Evaluate(claimant, region).hostile;
        }

        public static string ClaimExplanation(TINationState claimant,
            TIRegionState region)
        {
            ClaimHarmonizationEvaluation evaluation = Evaluate(claimant,
                region);
            if (!evaluation.hasClaim)
            {
                return Loc.T("UI.Nation.HarmonizationMissingClaim");
            }
            if (!evaluation.harmonization.valid)
            {
                return Loc.T("UI.Nation.HarmonizationInvalid");
            }
            string claimType = evaluation.historical
                ? Loc.T("UI.Nation.HarmonizationHistorical")
                : Loc.T("UI.Nation.HarmonizationOrdinary");
            NationalHarmonizationResult score = evaluation.harmonization;
            return Loc.T("UI.Nation.HarmonizationClaimScore",
                score.score.ToString("N2"),
                evaluation.threshold.ToString("N2"), claimType,
                score.governmentDifference.ToString("N2"),
                score.inequalityDifference.ToString("N2"),
                score.knowledgeDifference.ToString("N2"),
                score.perCapitaGdpRatio.ToString("N2"),
                score.modifier.ToString("N2"));
        }
    }

    public sealed class FederationHarmonizationEvaluation
    {
        public bool found;
        public bool valid;
        public double score = double.PositiveInfinity;
        public TINationState claimant;
        public TIRegionState region;
    }

    public static class FederationHarmonizationEvaluator
    {
        public static FederationHarmonizationEvaluation Between(
            TINationState first, TINationState second)
        {
            return Across(new[] { first }, second);
        }

        public static FederationHarmonizationEvaluation Across(
            IEnumerable<TINationState> members, TINationState prospective)
        {
            FederationHarmonizationEvaluation best =
                new FederationHarmonizationEvaluation();
            if (members == null || prospective == null)
            {
                return best;
            }
            List<TINationState> memberList = members.Where(x => x != null)
                .Distinct().ToList();
            foreach (TINationState member in memberList)
            {
                ConsiderClaims(best, member, prospective);
                ConsiderClaims(best, prospective, member);
            }
            return best;
        }

        public static bool Passes(FederationHarmonizationEvaluation evaluation)
        {
            return evaluation != null && evaluation.found && evaluation.valid &&
                evaluation.score <=
                    Main.settings.claimHarmonization.federationThreshold;
        }

        public static string Explanation(
            FederationHarmonizationEvaluation evaluation)
        {
            double threshold =
                Main.settings.claimHarmonization.federationThreshold;
            if (evaluation == null || !evaluation.found)
            {
                return Loc.T("UI.Nation.HarmonizationFederationNoLink",
                    threshold.ToString("N2"));
            }
            if (!evaluation.valid)
            {
                return Loc.T("UI.Nation.HarmonizationFederationInvalid");
            }
            return Loc.T("UI.Nation.HarmonizationFederationScore",
                evaluation.score.ToString("N2"), threshold.ToString("N2"),
                evaluation.claimant.displayName,
                evaluation.region.nation.displayName);
        }

        private static void ConsiderClaims(
            FederationHarmonizationEvaluation best,
            TINationState claimant, TINationState target)
        {
            if (claimant.claims == null)
            {
                return;
            }
            foreach (TIRegionState region in claimant.claims)
            {
                if (region == null || region.nation != target)
                {
                    continue;
                }
                ClaimHarmonizationEvaluation claim =
                    ClaimHarmonizationEvaluator.Evaluate(claimant, region);
                best.found = true;
                if (!claim.harmonization.valid)
                {
                    continue;
                }
                if (!best.valid || claim.harmonization.score < best.score)
                {
                    best.valid = true;
                    best.score = claim.harmonization.score;
                    best.claimant = claimant;
                    best.region = region;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "SetAllBilaterals")]
    public static class HistoricalClaimRegistrationPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            HistoricalClaimRegistry.RefreshAndApply();
        }
    }

    [HarmonyPatch(typeof(TINationState), "SetClaim")]
    public static class HistoricalClaimAcquisitionPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TINationState __instance,
            TIRegionState region, ref bool forceFromSeizure)
        {
            if (__instance != null && region != null &&
                region.nation != __instance &&
                HistoricalClaimRegistry.IsHistorical(__instance, region))
            {
                forceFromSeizure = true;
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "ClaimWillBeHostile")]
    public static class ClaimWillBeHostilePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TINationState __instance,
            TIRegionState region, ref bool __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (!Main.FeatureEnabled(settings.enabled) ||
                __instance.alienNation)
            {
                return true;
            }
            __result = ClaimHarmonizationEvaluator.IsHostile(__instance,
                region);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "HostileClaimDueToDemocracy")]
    public static class HostileClaimCompatibilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TINationState __instance,
            TIRegionState region, ref bool __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (!Main.FeatureEnabled(settings.enabled) ||
                __instance.alienNation)
            {
                return true;
            }
            __result = ClaimHarmonizationEvaluator.IsHostile(__instance,
                region);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "WillBeHostileExplanation")]
    public static class WillBeHostileExplanationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TINationState __instance,
            TIRegionState region, ref string __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (!Main.FeatureEnabled(settings.enabled) ||
                __instance.alienNation)
            {
                return true;
            }
            __result = ClaimHarmonizationEvaluator.ClaimExplanation(
                __instance, region);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "CanFormFederation")]
    public static class CanFormFederationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance,
            TINationState nation, ref bool __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (__result && Main.FeatureEnabled(settings.enabled))
            {
                __result = FederationHarmonizationEvaluator.Passes(
                    FederationHarmonizationEvaluator.Between(__instance,
                        nation));
            }
        }
    }

    [HarmonyPatch(typeof(TIFederationState), "CanAddNation")]
    public static class CanAddNationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TIFederationState __instance,
            TINationState prospectiveNation, ref bool __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (__result && Main.FeatureEnabled(settings.enabled))
            {
                __result = FederationHarmonizationEvaluator.Passes(
                    FederationHarmonizationEvaluator.Across(
                        __instance.members, prospectiveNation));
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "FormFederation")]
    public static class FormFederationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(TINationState __instance,
            TINationState nation)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            return !Main.FeatureEnabled(settings.enabled) ||
                __instance.CanFormFederation(nation);
        }
    }

    [HarmonyPatch(typeof(TINationState), "CanFormFederationFeedback")]
    public static class CanFormFederationFeedbackPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance,
            TINationState nation, ref string __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (Main.FeatureEnabled(settings.enabled))
            {
                __result += Environment.NewLine +
                    FederationHarmonizationEvaluator.Explanation(
                        FederationHarmonizationEvaluator.Between(__instance,
                            nation));
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "CanJoinFederationFeedback")]
    public static class CanJoinFederationFeedbackPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TIFederationState federation,
            TINationState prospectiveNation, ref string __result)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (Main.FeatureEnabled(settings.enabled))
            {
                __result += Environment.NewLine +
                    FederationHarmonizationEvaluator.Explanation(
                        FederationHarmonizationEvaluator.Across(
                            federation.members, prospectiveNation));
            }
        }
    }

    [HarmonyPatch(typeof(TINationState), "TransferRegionsControlTo")]
    public static class HarmonizedRegionTransferPatch
    {
        public sealed class TransferState
        {
            public TINationState receivingNation;
            public List<TIRegionState> peacefulClaims;
        }

        [HarmonyPrefix]
        public static void Prefix(List<TIRegionState> regions,
            TINationState newNation, ref TransferState __state)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (!Main.FeatureEnabled(settings.enabled) || regions == null ||
                newNation == null || newNation.alienNation)
            {
                return;
            }
            __state = new TransferState
            {
                receivingNation = newNation,
                peacefulClaims = regions.Where(region =>
                    newNation.claims.Contains(region) &&
                    !ClaimHarmonizationEvaluator.IsHostile(newNation, region))
                    .ToList()
            };
        }

        [HarmonyPostfix]
        public static void Postfix(TransferState __state)
        {
            if (__state == null || __state.receivingNation.hostileClaims == null)
            {
                return;
            }
            foreach (TIRegionState region in __state.peacefulClaims)
            {
                __state.receivingNation.hostileClaims.Remove(region);
            }
        }
    }

    public static class ExternalClaimPresentation
    {
        private static readonly MethodInfo ListContains = AccessTools.Method(
            typeof(List<TIRegionState>), nameof(List<TIRegionState>.Contains));
        private static readonly MethodInfo ResolvedContainsMethod =
            AccessTools.Method(typeof(ExternalClaimPresentation),
                nameof(ResolvedContains));

        public static bool ResolvedContains(List<TIRegionState> list,
            TIRegionState region)
        {
            if (list == null)
            {
                return false;
            }
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            if (!Main.FeatureEnabled(settings.enabled) || region == null)
            {
                return list.Contains(region);
            }
            foreach (TINationState nation in
                GameStateManager.IterateByClass<TINationState>())
            {
                if (ReferenceEquals(nation.hostileClaims, list) &&
                    region.nation != nation && nation.claims.Contains(region))
                {
                    return ClaimHarmonizationEvaluator.IsHostile(nation,
                        region);
                }
            }
            return list.Contains(region);
        }

        public static IEnumerable<CodeInstruction> ReplaceContains(
            IEnumerable<CodeInstruction> instructions, int expected,
            string target)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            int replacements = 0;
            foreach (CodeInstruction instruction in patched)
            {
                if (instruction.Calls(ListContains))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = ResolvedContainsMethod;
                    replacements++;
                }
            }
            if (replacements != expected)
            {
                string message = "Claim-presentation IL changed in " + target +
                    ": expected " + expected + " region-list Contains call(s), found " +
                    replacements + ". Refusing a partial patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }
    }

    [HarmonyPatch(typeof(TINationState), "CanUnifyFeedback")]
    public static class CanUnifyFeedbackClaimPresentationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ExternalClaimPresentation.ReplaceContains(instructions, 2,
                "TINationState.CanUnifyFeedback");
        }
    }

    [HarmonyPatch(typeof(RegionController), "GetRegionFillColor")]
    public static class RegionClaimPresentationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ExternalClaimPresentation.ReplaceContains(instructions, 3,
                "RegionController.GetRegionFillColor");
        }
    }

    [HarmonyPatch(typeof(ClaimListItemController), "UpdateListItem")]
    public static class ClaimListPresentationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ExternalClaimPresentation.ReplaceContains(instructions, 2,
                "ClaimListItemController.UpdateListItem");
        }
    }

    [HarmonyPatch(typeof(NationInfoController), "UpdateRegionList")]
    public static class NationRegionListPresentationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ExternalClaimPresentation.ReplaceContains(instructions, 3,
                "NationInfoController.UpdateRegionList");
        }
    }

    [HarmonyPatch(typeof(PolicyTargetGridItemController), "UpdateListItem")]
    public static class PolicyTargetClaimPresentationPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ExternalClaimPresentation.ReplaceContains(instructions, 1,
                "PolicyTargetGridItemController.UpdateListItem");
        }

        [HarmonyPostfix]
        public static void Postfix(PolicyTargetGridItemController __instance,
            TIGameState target, TIPolicyOption policyOption,
            TINationState proposingNation)
        {
            ClaimHarmonizationSettings settings =
                Main.settings.claimHarmonization;
            TINationState targetNation = target == null ? null :
                target.ref_nation;
            if (!Main.FeatureEnabled(settings.enabled) ||
                policyOption.GetPolicyType() != PolicyType.UnificationOption ||
                proposingNation == null || targetNation == null)
            {
                return;
            }
            bool hostile = targetNation.regions.Any(region =>
                proposingNation.claims.Contains(region) &&
                ClaimHarmonizationEvaluator.IsHostile(proposingNation,
                    region));
            __instance.secondaryIcon.gameObject.SetActive(hostile);
            if (hostile)
            {
                GameControl.assetLoader.LoadAssetForImageAssignment(
                    TemplateManager.global.pathUnrestIcon,
                    __instance.secondaryIcon);
            }
        }
    }
}
