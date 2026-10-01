using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.Tasks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    internal static class MissionProbability
    {
        internal static bool Enabled => Main.enabled && (Main.settings == null || Main.settings.enabled);
        internal const string ImpossibleReason = "EEO.Mission.ZeroSuccessChance";

        internal static int Ticks(float chance)
        {
            if (float.IsNaN(chance) || chance <= 0f) return 0;
            // Promote near certainty before flooring, but leave the RNG/critical roll untouched.
            // Exactly 99.95% is not promoted; use the game's representable float boundary.
            if (chance > 0.9995f) return 1000;
            int ticks = (int)((double)chance * 1000d);
            // Compare float boundaries, without an epsilon that admits values below 0.1%.
            // This also keeps flooring idempotent for e.g. the float representation of 0.009.
            if ((ticks + 1) / 1000f <= chance) ticks++;
            return ticks;
        }

        internal static float Floor(float chance) => Ticks(chance) / 1000f;

        internal static TIMissionOutcome Outcome(float chance, float roll)
        {
            if (chance > 0f && roll < chance / 10f) return TIMissionOutcome.CriticalSuccess;
            if (chance > 0f && (roll < chance || (chance >= 1f && roll <= 1f)))
                return TIMissionOutcome.Success;
            return roll >= 1f - (1f - chance) / 10f
                ? TIMissionOutcome.CriticalFailure : TIMissionOutcome.Failure;
        }

        internal static string Format(float value, int decimals = 0)
        {
            if (value == 0f || value == 1f)
                return value.ToString("P0", CultureInfo.CurrentCulture);
            decimals = Math.Max(0, Math.Min(9, decimals));
            if (value > 0f && value < 1f && (value < 0.01f || value > 0.99f))
                decimals = Math.Max(1, decimals);
            // Decimal conversion removes float representation noise at decimal boundaries.
            decimal scale = (decimal)Math.Pow(10, decimals + 2);
            decimal truncated = Math.Floor((decimal)value * scale) / scale;
            return truncated.ToString("P" + decimals, CultureInfo.CurrentCulture);
        }

        internal static bool Applies(TIMissionTemplate mission) =>
            Enabled && mission?.resolutionMethod is TIMissionResolution_Contested;

        internal static bool Impossible(TIMissionTemplate mission, TICouncilorState councilor,
            TIGameState target, float spend)
        {
            return Applies(mission) && councilor != null && target != null &&
                mission.resolutionMethod.GetSuccessChance(mission, councilor, target, spend, false) <= 0f;
        }

        internal static float AvailableSpend(TIMissionTemplate mission, TICouncilorState councilor,
            TIGameState target)
        {
            float spend = 0f;
            if (mission.cost is TIMissionCost_Bonus)
                spend = mission.cost.GetCost(councilor.CurrentMaxSliderSteps(mission), councilor, target);
            // Assigned resources have already been deducted from the faction balance.
            TIMissionState active = councilor.activeMission;
            if (active != null && active.missionTemplate == mission && active.target == target)
                spend = Math.Max(spend, active.resources);
            return spend;
        }
    }

    [HarmonyPatch(typeof(TIMissionResolution_Contested), nameof(TIMissionResolution_Contested.GetSuccessChance))]
    internal static class MissionChanceFloorPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(ref float __result)
        {
            if (MissionProbability.Enabled) __result = MissionProbability.Floor(__result);
        }
    }

    [HarmonyPatch(typeof(TIMissionResolution_Contested), nameof(TIMissionResolution_Contested.GetMissionOutcome))]
    internal static class MissionOutcomeProbabilityPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(TIMissionResolution_Contested __instance, TIMissionTemplate mission,
            TICouncilorState councilor, TIGameState target, float resourcesSpent, ref TIMissionResult __result)
        {
            if (!MissionProbability.Enabled) return true;
            float chance = __instance.GetSuccessChance(mission, councilor, target, resourcesSpent);
            float roll = TIUtilities.RandomFloatValue();
            // Retain the native turned-agent sabotage rule, but do not truncate the roll.
            if (councilor.turned && ((chance <= councilor.autofailMissionsValue && roll < chance) ||
                councilor.AutofailTurnedCouncilor(mission, target)))
                roll = TIUtilities.RandomRange(chance + 0.01f, chance + 0.9f * (1f - chance));
            __result = new TIMissionResult { roll = roll, outcome = MissionProbability.Outcome(chance, roll) };
            return false;
        }
    }

    [HarmonyPatch]
    internal static class MissionChanceTextPatch
    {
        internal static MethodBase TargetMethod() => typeof(TIMissionResolution).GetMethods()
            .Single(m => m.Name == "GetSuccessChanceString" && !m.GetParameters()[1].ParameterType.IsByRef);

        [HarmonyPrefix]
        internal static bool Prefix(TIMissionResolution __instance, TIMissionTemplate mission,
            TICouncilorState councilor, TIGameState target, float resourcesSpent, bool reValidateTarget,
            int digits, ref string __result)
        {
            if (!MissionProbability.Applies(mission)) return true;
            __result = MissionProbability.Format(__instance.GetSuccessChance(
                mission, councilor, target, resourcesSpent, reValidateTarget), digits - 2);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class MissionChanceTextWithValuePatch
    {
        internal static MethodBase TargetMethod() => typeof(TIMissionResolution).GetMethods()
            .Single(m => m.Name == "GetSuccessChanceString" && m.GetParameters()[1].ParameterType.IsByRef);

        [HarmonyPrefix]
        internal static bool Prefix(TIMissionResolution __instance, TIMissionTemplate mission,
            ref float successChance, TICouncilorState councilor, TIGameState target, float resourcesSpent,
            bool reValidateTarget, int digits, ref string __result)
        {
            if (!MissionProbability.Applies(mission)) return true;
            successChance = __instance.GetSuccessChance(mission, councilor, target, resourcesSpent, reValidateTarget);
            __result = MissionProbability.Format(successChance, digits - 2);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class MissionPossibleTargetPatch
    {
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            // Explicit TI 1.0.53b coverage avoids loading unrelated game/Unity types.
            foreach (Type type in new[] {
                typeof(TIMissionTarget_AlienActivity), typeof(TIMissionTarget_AlienAsset),
                typeof(TIMissionTarget_Army), typeof(TIMissionTarget_Councilor),
                typeof(TIMissionTarget_EnemyHabModule), typeof(TIMissionTarget_EnemyProjectLocation),
                typeof(TIMissionTarget_Nation), typeof(TIMissionTarget_NationFleetHab),
                typeof(TIMissionTarget_NationHab), typeof(TIMissionTarget_Org),
                typeof(TIMissionTarget_OwnedControlPoint), typeof(TIMissionTarget_Region),
                typeof(TIMissionTarget_RegionBase), typeof(TIMissionTarget_RegionCouncilorHab),
                typeof(TIMissionTarget_RegionFleetHab), typeof(TIMissionTarget_ShipHab),
                typeof(TIMissionTarget_SpaceAsset), typeof(TIMissionTarget_SpaceFacility),
                typeof(TIMissionTarget_VictoryMissionTarget) })
                yield return AccessTools.DeclaredMethod(type, "ValidateSingleTarget");
        }

        [HarmonyPostfix]
        internal static void Postfix(TIMissionTemplate mission, TICouncilorState councilor,
            TIGameState target, List<string> __result)
        {
            if (!MissionProbability.Applies(mission) || councilor == null || target == null ||
                __result == null || __result.Any(reason => reason != "_Pass")) return;
            if (MissionProbability.Impossible(mission, councilor, target,
                MissionProbability.AvailableSpend(mission, councilor, target)))
                __result.Add(MissionProbability.ImpossibleReason);
        }
    }

    [HarmonyPatch(typeof(MarkerController), nameof(MarkerController.BuildInvalidTargetTooltip))]
    internal static class MissionImpossibleTooltipPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(ref string __result)
        {
            if (MissionProbability.Enabled && __result != null)
                __result = __result.Replace(MissionProbability.ImpossibleReason,
                    "Success chance is 0% even with available mission resources.");
        }
    }

    [HarmonyPatch(typeof(AssignCouncilorToMission), nameof(AssignCouncilorToMission.Execute))]
    internal static class MissionImpossibleAssignmentPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(GameStateID ___councilorID, GameStateID ___targetID,
            TIMissionTemplate ___missionTemplate, float ___resourcesSpend)
        {
            if (!MissionProbability.Applies(___missionTemplate)) return true;
            return !MissionProbability.Impossible(___missionTemplate,
                ___councilorID.GetState<TICouncilorState>(), ___targetID.GetState(), ___resourcesSpend);
        }
    }

    [HarmonyPatch(typeof(AIMissionEntry), "isTooRisky", MethodType.Getter)]
    internal static class MissionImpossibleAiRiskPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(AIMissionEntry __instance, ref bool __result)
        {
            if (MissionProbability.Applies(__instance.mission) && __instance.successChanceHigh <= 0f)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(AIEvaluators), nameof(AIEvaluators.AI_ShouldAbortBadMission))]
    internal static class MissionImpossibleAiAbortPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(TIMissionState mission, ref bool __result)
        {
            if (!MissionProbability.Impossible(mission.missionTemplate, mission.councilor,
                mission.target, mission.resources)) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class MissionConfirmProbabilityPatch
    {
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "UpdateMissionModifiers", "UpdateAllMissionData", "UpdateResourcePanel" })
                yield return AccessTools.Method(typeof(CouncilorMissionCanvasController), name);
        }

        [HarmonyPostfix]
        internal static void Postfix(CouncilorMissionCanvasController __instance,
            TICouncilorState ___myCouncilor, TIGameState ___currentTarget, float ___resourcesSpend,
            CouncilorMissionButtonController ___activeButton)
        {
            if (!MissionProbability.Enabled || __instance.confirmButton == null) return;
            __instance.confirmButton.interactable = !MissionProbability.Impossible(
                ___activeButton == null ? null : ___activeButton.missionType,
                ___myCouncilor, ___currentTarget, ___resourcesSpend);
        }
    }

    [HarmonyPatch(typeof(CouncilorMissionCanvasController), nameof(CouncilorMissionCanvasController.OnConfirmMissionClick))]
    internal static class MissionConfirmZeroGuardPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(CouncilorMissionCanvasController __instance,
            TICouncilorState ___myCouncilor, TIGameState ___currentTarget, float ___resourcesSpend,
            CouncilorMissionButtonController ___activeButton) =>
            !MissionProbability.Impossible(___activeButton == null ? null : ___activeButton.missionType,
                ___myCouncilor, ___currentTarget, ___resourcesSpend);
    }

    // These UI routes bypass GetSuccessChanceString. Restrict formatting substitution
    // to these methods rather than changing the game's general percentage formatter.
    [HarmonyPatch]
    internal static class MissionDirectChanceTextPatch
    {
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(SpaceCouncilorController), "UpdateController");
            yield return AccessTools.Method(typeof(TINotificationQueueState), "LogMissionOutcome");
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase __originalMethod)
        {
            int count = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                MethodInfo method = instruction.operand as MethodInfo;
                if (method != null && method.Name == "ToPercent" && method.IsStatic &&
                    method.ReturnType == typeof(string) && method.GetParameters().Select(p => p.ParameterType)
                        .SequenceEqual(new[] { typeof(float), typeof(string) }))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(MissionDirectChanceTextPatch), nameof(Format));
                    count++;
                }
                yield return instruction;
            }
            int expected = __originalMethod.DeclaringType == typeof(SpaceCouncilorController) ? 1 : 4;
            if (count != expected) throw new InvalidOperationException(
                "Mission chance formatting IL changed: " + __originalMethod.Name + " matched " + count);
        }

        internal static string Format(float value, string format)
        {
            if (!MissionProbability.Enabled) return value.ToPercent(format);
            int decimals;
            if (format == null || !format.StartsWith("P", StringComparison.Ordinal) ||
                !int.TryParse(format.Substring(1), out decimals)) return value.ToPercent(format);
            return MissionProbability.Format(value, decimals);
        }
    }
}
