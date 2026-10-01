using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.Tasks;
using TIEconomyMod;
using Tuple = System.Tuple;

public static class MissionProbabilityValidation
{
    private static readonly Assembly Mod = typeof(Main).Assembly;
    private static readonly Type Policy = Mod.GetType("TIEconomyMod.Patches.MissionProbability", true);
    private static float rawChance, roll;
    private static bool nativeValid = true;
    private static int maxSteps;

    private static object Call(string method, params object[] args)
    {
        return Policy.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    // Isolate game services while exercising the actual patched methods and postfix order.
    public static bool Chance(TIMissionTemplate mission, TICouncilorState councilor,
        TIGameState target, float resourcesSpent, bool reValidateTarget, ref float __result)
    {
        __result = reValidateTarget && !mission.target.ValidTarget(
            mission.target.ValidateSingleTarget(mission, councilor, target))
            ? 0f : rawChance + resourcesSpent * 0.0001f;
        return false;
    }
    public static bool Random(ref float __result) { __result = roll; return false; }
    public static bool MaxSteps(ref int __result) { __result = maxSteps; return false; }
    public static bool Valid(ref List<string> __result)
    {
        __result = new List<string> { nativeValid ? "_Pass" : "_Fail" };
        return false;
    }
    public static bool NoSabotage(ref bool __result) { __result = false; return false; }
    public static bool SabotageRoll(ref float __result) { __result = 0.95f; return false; }

    public sealed class BonusCost : TIMissionCost_Bonus
    {
        public override float GetCost(float bonus, TICouncilorState councilor = null, TIGameState scalingState = null)
        { return bonus * 4f; }
    }

    private static T State<T>(int id) where T : TIGameState
    {
        T state = (T)FormatterServices.GetUninitializedObject(typeof(T));
        typeof(TIGameState).GetProperty("ID").SetValue(state, new GameStateID(id), null);
        return state;
    }

    public static string Run()
    {
        string id = "eeo.validate.mission-probability." + Guid.NewGuid().ToString("N");
        Harmony harmony = new Harmony(id);
        bool oldEnabled = Main.enabled;
        Settings oldSettings = Main.settings;
        CultureInfo oldCulture = CultureInfo.CurrentCulture;
        FieldInfo registryField = AccessTools.Field(typeof(GameStateManager), "gamestates");
        object oldRegistry = registryField.GetValue(null);
        try
        {
            Main.enabled = true;
            Main.settings = new Settings();
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Type[] patches = Mod.GetTypes().Where(t => t.Namespace == "TIEconomyMod.Patches" &&
                t.Name.StartsWith("Mission", StringComparison.Ordinal) &&
                t.Name != "MissionControlUsageColorPatch" &&
                t.GetCustomAttributes(typeof(HarmonyPatch), false).Length != 0).ToArray();
            Require(patches.Length == 12, "Unexpected mission patch coverage: " + patches.Length);
            int bindings = 0;
            List<string> unityOnly = new List<string>();
            foreach (Type patch in patches)
            {
                List<MethodInfo> methods;
                try { methods = harmony.CreateClassProcessor(patch).Patch(); }
                catch (Exception exception)
                {
                    Exception cause = exception.GetBaseException();
                    if (!(cause is System.Security.SecurityException) || !cause.Message.StartsWith("ECall methods") ||
                        !(patch.Name == "MissionConfirmProbabilityPatch" || patch.Name == "MissionConfirmZeroGuardPatch" ||
                          patch.Name == "MissionDirectChanceTextPatch")) throw;
                    unityOnly.Add(patch.Name);
                    // The methods resolve, but this standalone CLR cannot JIT Unity's native calls.
                    if (patch.Name == "MissionDirectChanceTextPatch")
                    {
                        foreach (MethodBase original in (IEnumerable<MethodBase>)AccessTools.Method(patch, "TargetMethods").Invoke(null, null))
                        {
                            var instructions = PatchProcessor.GetOriginalInstructions(original);
                            var rewritten = (IEnumerable<CodeInstruction>)AccessTools.Method(patch, "Transpiler").Invoke(null,
                                new object[] { instructions, original });
                            Require(rewritten.ToList().Count > 0, "UI formatter transpiler produced no IL");
                        }
                    }
                    continue;
                }
                Require(methods != null && methods.Count > 0, "No target for " + patch.Name);
                Require(Harmony.GetAllPatchedMethods().Any(method => {
                    Patches info = Harmony.GetPatchInfo(method);
                    return info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers)
                        .Any(p => p.owner == id && p.PatchMethod.DeclaringType == patch);
                }), "Missing binding: " + patch.Name);
                bindings += methods.Count;
            }

            // Adjacent representable floats detect an epsilon or multiply-rounding leak.
            for (int tick = 1; tick < 1000; tick++)
            {
                float boundary = tick / 1000f;
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(boundary), 0);
                float below = BitConverter.ToSingle(BitConverter.GetBytes(bits - 1), 0);
                float above = BitConverter.ToSingle(BitConverter.GetBytes(bits + 1), 0);
                Require((int)Call("Ticks", boundary) == tick, "Boundary floor " + tick);
                Require((int)Call("Ticks", below) == tick - 1, "Floor admitted sub-boundary " + tick);
                Require((int)Call("Ticks", above) == tick, "Floor above boundary " + tick);
                Require((float)Call("Floor", (float)Call("Floor", boundary)) == boundary, "Non-idempotent floor");
            }
            Require((float)Call("Floor", 0.00305f) == 0.003f, "0.305% must become 0.3%");
            Require((float)Call("Floor", 0.000999f) == 0f, "Sub-0.1% must become zero");
            int certaintyBits = BitConverter.ToInt32(BitConverter.GetBytes(0.9995f), 0);
            float belowCertainty = BitConverter.ToSingle(BitConverter.GetBytes(certaintyBits - 1), 0);
            float aboveCertainty = BitConverter.ToSingle(BitConverter.GetBytes(certaintyBits + 1), 0);
            Require((float)Call("Floor", belowCertainty) == 0.999f, "Below 99.95% promoted");
            Require((float)Call("Floor", 0.9995f) == 0.999f, "Exactly 99.95% promoted");
            Require((float)Call("Floor", aboveCertainty) == 1f, "Above 99.95% not promoted");
            Require((float)Call("Floor", 0.99999f) == 1f, "Near certainty must become 100%");
            Require((float)Call("Floor", (float)Call("Floor", aboveCertainty)) == 1f, "Certainty not idempotent");
            Require((float)Call("Floor", 1f) == 1f, "Certainty changed");
            foreach (float p in new[] { 0f, 0.001f, 0.003f, 0.5f, 0.999f, 1f })
            {
                int successes = 0;
                for (int sample = 0; sample < 10000; sample++)
                {
                    TIMissionOutcome result = (TIMissionOutcome)Call("Outcome", p, sample / 10000f);
                    if (result == TIMissionOutcome.Success || result == TIMissionOutcome.CriticalSuccess) successes++;
                }
                Require(successes == (int)Math.Round(p * 10000), "Effective probability differs at " + p);
            }
            Require((TIMissionOutcome)Call("Outcome", 0f, 0f) == TIMissionOutcome.Failure, "Zero roll succeeded at zero chance");
            Require((TIMissionOutcome)Call("Outcome", 1f, 1f) == TIMissionOutcome.Success, "RNG endpoint failed at certainty");
            Require((TIMissionOutcome)Call("Outcome", 1f, 1.01f) == TIMissionOutcome.CriticalFailure, "Sabotage bypassed at certainty");
            foreach (var sample in new[] {
                Tuple.Create(0.003f, "0.3%"), Tuple.Create(0.009f, "0.9%"),
                Tuple.Create(0.01f, "1%"), Tuple.Create(0.578f, "57%"),
                Tuple.Create(0.99f, "99%"), Tuple.Create(0.991f, "99.1%"),
                Tuple.Create(0.999f, "99.9%"), Tuple.Create(0f, "0%"), Tuple.Create(1f, "100%") })
                Require(((string)Call("Format", sample.Item1, 0)).Replace(" ", "") == sample.Item2, "Formatting " + sample.Item1);
            Require((string)Call("Format", 0.003f, 4) != (string)Call("Format", 0.00304f, 4), "Result precision lost");
            Require(((string)Call("Format", 0f, 4)).Replace(" ", "") == "0%" &&
                ((string)Call("Format", 1f, 4)).Replace(" ", "") == "100%", "Endpoints gained decimals");

            harmony.Patch(AccessTools.Method(typeof(TIMissionResolution_Contested), "GetSuccessChance"),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "Chance"));
            harmony.Patch(AccessTools.Method(typeof(TIUtilities), "RandomFloatValue"),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "Random"));
            harmony.Patch(AccessTools.Method(typeof(TICouncilorState), "CurrentMaxSliderSteps"),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "MaxSteps"));
            harmony.Patch(AccessTools.Method(typeof(TIMissionTarget_Councilor), "ValidateSingleTarget"),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "Valid"));
            TICouncilorState actor = State<TICouncilorState>(900001);
            TICouncilorState target = State<TICouncilorState>(900002);
            TIMissionResolution_Contested resolver = new TIMissionResolution_Contested();
            TIMissionTemplate mission = (TIMissionTemplate)FormatterServices.GetUninitializedObject(typeof(TIMissionTemplate));
            mission.resolutionMethod = resolver;
            mission.target = new TIMissionTarget_Councilor();
            rawChance = 0.00305f;
            Require(resolver.GetSuccessChance(mission, actor, target) == 0.003f, "Native chance postfix missing");
            float displayed;
            Require(resolver.GetSuccessChanceString(mission, out displayed, actor, target).Replace(" ", "") == "0.3%" &&
                displayed == 0.003f, "Out-parameter formatter mismatch");
            Require(resolver.GetSuccessChanceString(mission, actor, target).Replace(" ", "") == "0.3%", "Formatter overload mismatch");
            roll = 0.00304f;
            TIMissionResult outcome = resolver.GetMissionOutcome(mission, actor, target);
            Require(outcome.outcome == TIMissionOutcome.Failure && outcome.roll == roll, "Native roll truncation survived");
            roll = 0.00299f;
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.Success, "Rare valid success lost");
            rawChance = 0.9995f;
            Require(resolver.GetSuccessChanceString(mission, actor, target).Replace(" ", "") == "99.9%",
                "Exactly 99.95% display promoted");
            rawChance = aboveCertainty;
            Require(resolver.GetSuccessChanceString(mission, actor, target).Replace(" ", "") == "100%",
                "Promoted certainty formatter mismatch");
            Require(resolver.GetSuccessChanceString(mission, out displayed, actor, target).Replace(" ", "") == "100%" &&
                displayed == 1f, "Promoted certainty out-parameter formatter mismatch");
            int criticals = 0, ordinarySuccesses = 0;
            for (int sample = 0; sample < 10000; sample++)
            {
                roll = sample / 10000f;
                outcome = resolver.GetMissionOutcome(mission, actor, target);
                Require(outcome.roll == roll, "Certainty changed random roll precision");
                if (outcome.outcome == TIMissionOutcome.CriticalSuccess) criticals++;
                else if (outcome.outcome == TIMissionOutcome.Success) ordinarySuccesses++;
                else throw new InvalidOperationException("Promoted certainty failed");
            }
            Require(criticals == 1000 && ordinarySuccesses == 9000, "Certainty changed critical-success proportion");
            int criticalBits = BitConverter.ToInt32(BitConverter.GetBytes(0.1f), 0);
            roll = BitConverter.ToSingle(BitConverter.GetBytes(criticalBits - 1), 0);
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.CriticalSuccess,
                "Favorable adjacent critical roll lost");
            roll = 0.1f;
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.Success,
                "Critical boundary must remain strict");
            roll = BitConverter.ToSingle(BitConverter.GetBytes(criticalBits + 1), 0);
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.Success,
                "Unfavorable adjacent roll became critical");
            roll = 1f;
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.Success,
                "Promoted certainty failed at RNG endpoint");
            rawChance = 0.00099f; roll = 0f;
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.Failure, "Zero-chance native outcome succeeded");
            Require(!mission.target.ValidTarget(mission.target.ValidateSingleTarget(mission, actor, target)), "Impossible target allowed");
            Require(resolver.GetSuccessChance(mission, actor, target, 0f, true) == 0f, "Recursive revalidation failed");
            mission.cost = new BonusCost(); maxSteps = 3;
            Require(mission.target.ValidTarget(mission.target.ValidateSingleTarget(mission, actor, target)), "Affordable boost target lost");
            Require((bool)Call("Impossible", mission, actor, target, 0f), "Zero slider allowed");
            Require(!(bool)Call("Impossible", mission, actor, target, 12f), "Positive slider rejected");
            nativeValid = false;
            Require(!mission.target.ValidTarget(mission.target.ValidateSingleTarget(mission, actor, target)), "Native invalidity overwritten");
            nativeValid = true; maxSteps = 0;
            TIMissionState queued = State<TIMissionState>(900003);
            queued.SetTemplate<TIMissionTemplate>(mission);
            queued.target = target; queued.councilor = actor; queued.resources = 12f;
            AccessTools.Property(typeof(TICouncilorState), "activeMission").SetValue(actor, queued, null);
            Require((float)Call("AvailableSpend", mission, actor, target) == 12f, "Paid resources ignored");
            Require(mission.target.ValidTarget(mission.target.ValidateSingleTarget(mission, actor, target)), "Paid order invalidated");
            queued.resources = 0f;
            Require(AIEvaluators.AI_ShouldAbortBadMission(queued), "Queued impossible AI order not aborted");
            AIMissionEntry entry = new AIMissionEntry { mission = mission, successChanceHigh = 0f, acceptableMinimumSuccess = 0f };
            Require(entry.isTooRisky, "Zero-risk AI objective allowed");

            registryField.SetValue(null, new Dictionary<Type, Dictionary<GameStateID, TIGameState>> {
                { typeof(TICouncilorState), new Dictionary<GameStateID, TIGameState> { { actor.ID, actor }, { target.ID, target } } } });
            // Full action must return before accessing faction resources, creating state, or moving.
            new AssignCouncilorToMission(actor, mission, target, 0f).Execute();
            Require(actor.activeMission == queued, "Rejected assignment changed active order");

            harmony.Patch(AccessTools.Method(typeof(TICouncilorState), "AutofailTurnedCouncilor"),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "NoSabotage"));
            harmony.Patch(AccessTools.Method(typeof(TIUtilities), "RandomRange", new[] { typeof(float), typeof(float) }),
                prefix: new HarmonyMethod(typeof(MissionProbabilityValidation), "SabotageRoll"));
            AccessTools.Property(typeof(TICouncilorState), "agentForFaction").SetValue(actor, State<TIFactionState>(900004), null);
            AccessTools.Property(typeof(TICouncilorState), "autofailMissionsValue").SetValue(actor, 1f, null);
            rawChance = 0.5f; roll = 0.1f;
            Require(resolver.GetMissionOutcome(mission, actor, target).outcome == TIMissionOutcome.CriticalFailure,
                "Turned-agent sabotage lost");
            AccessTools.Property(typeof(TICouncilorState), "agentForFaction").SetValue(actor, null, null);

            Main.enabled = false; rawChance = 0.00099f;
            Require(resolver.GetSuccessChance(mission, actor, target) == rawChance, "Disabled-mod chance changed");
            Require(mission.target.ValidTarget(mission.target.ValidateSingleTarget(mission, actor, target)), "Disabled-mod target changed");
            Require(!entry.isTooRisky, "Disabled-mod AI rule changed");
            return "PASS: " + bindings + " Harmony bindings; 999 float boundaries and neighbors; exact effective probabilities; " +
                "strict >99.95% certainty and normal critical rolls, unrounded rolls, zero/100% endpoints, " +
                "both text overloads, boosted/paid/native-invalid targets, " +
                "AI risk and abort, rejected assignment without side effects, turned agents, and disabled-mod behavior. " +
                "Unity-host UI validation remains: " + string.Join(", ", unityOnly);
        }
        finally
        {
            harmony.UnpatchAll(id);
            registryField.SetValue(null, oldRegistry);
            Main.enabled = oldEnabled; Main.settings = oldSettings;
            CultureInfo.CurrentCulture = oldCulture;
        }
    }
}
