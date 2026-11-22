using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using PavonisInteractive.TerraInvicta.SpaceCombat.UI;
using PavonisInteractive.TerraInvicta.Systems.GameTime;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Diagnostic patches exist only in a batch process explicitly launched for this replay.
    internal sealed class StationBattleReplay : MonoBehaviour
    {
        private sealed class Frame
        {
            public double combatSeconds, wallMs, shellMs, jobCompleteMs;
            public int missiles, shellQueries;
        }

        private static long shellTicks, waitTicks;
        private static int queries;
        private static long verifiedQueries;
        private static readonly Dictionary<Mesh, RenderedMeshRaycast> referenceGeometry = new Dictionary<Mesh, RenderedMeshRaycast>();
        private static TIStandaloneInputModule batchInput;
        private readonly List<Frame> frames = new List<Frame>(16384);
        private string save, output;
        private float speed;
        private bool loaded, playing, startedEncounter;
        private bool throughSalvo;
        private float nextPoll, deadline;
        private float nextProgress;
        private double firstSalvo = -1;
        private long lastFrame;

        internal static void InstallIfRequested()
        {
            if (!Application.isBatchMode) return;
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--eeo-station-replay");
            if (index < 0) return;
            var root = new GameObject("EEO station battle replay");
            DontDestroyOnLoad(root);
            var replay = root.AddComponent<StationBattleReplay>();
            replay.save = Path.GetFullPath(args[index + 1]);
            replay.output = Path.GetFullPath(args[index + 2]);
            replay.speed = float.Parse(args[index + 3], System.Globalization.CultureInfo.InvariantCulture);
            replay.throughSalvo = Array.IndexOf(args, "--eeo-through-salvo") >= 0;
            replay.deadline = Time.realtimeSinceStartup + Math.Max(420f, 150f + 1000f / replay.speed);
            var harmony = new Harmony("eeo.batch.station-replay");
            var harness = typeof(StationBattleReplay);
            // FMOD drives the game's audio independently of the Unity -noaudio switch.
            harmony.Patch(AccessTools.Method(typeof(FMOD.Studio.System), "initialize"),
                prefix: new HarmonyMethod(harness, nameof(UseSilentOutput)));
            var core = FMODUnity.RuntimeManager.CoreSystem;
            bool silentDriver = core.getOutput(out var driver) == FMOD.RESULT.OK && driver == FMOD.OUTPUTTYPE.NOSOUND;
            // An already initialized runtime can keep its device. Mute its process-local master
            // channel group before entering the menu, including all banks and music buses.
            if (!silentDriver && (core.getMasterChannelGroup(out var master) != FMOD.RESULT.OK ||
                master.setMute(true) != FMOD.RESULT.OK || master.getMute(out var muted) != FMOD.RESULT.OK || !muted))
            {
                Main.Warn("Cannot verify silent FMOD output. Stopping replay.");
                replay.enabled = false;
                Application.Quit(2);
                return;
            }
            Main.Log("Station replay verified silent FMOD output: " + (silentDriver ? "NOSOUND driver" : "master channel group muted"));
            harmony.Patch(AccessTools.Method(typeof(GameStateManager), "SaveAllGameStates"),
                prefix: new HarmonyMethod(harness, nameof(SuppressSave)));
            harmony.Patch(AccessTools.PropertyGetter(typeof(TIStandaloneInputModule), "current"),
                postfix: new HarmonyMethod(harness, nameof(SupplyBatchInput)));
            harmony.Patch(AccessTools.Method(AccessTools.Inner(typeof(Patches.OrbitalDefenseCombat), "ModuleModels"), "Blocks"),
                prefix: new HarmonyMethod(harness, nameof(BeginShell)), postfix: new HarmonyMethod(harness, nameof(EndShell)));
            if (Array.IndexOf(args, "--eeo-verify-shell") >= 0)
            {
                harmony.Patch(AccessTools.Method(AccessTools.Inner(typeof(Patches.OrbitalDefenseCombat), "ModuleModels"), "Blocks"),
                    postfix: new HarmonyMethod(harness, nameof(VerifyShell)));
            }
            foreach (var method in new[] { AccessTools.Method(typeof(ProjectileJobContainer), "LateUpdate"),
                AccessTools.Method(typeof(ContactJobRuntime), "CaptureCompatibilityPose"),
                AccessTools.Method(typeof(ContactJobRuntime), "IndexCompatibility"),
                AccessTools.Method(typeof(ContactJobRuntime), "Complete") })
                harmony.Patch(method, transpiler: new HarmonyMethod(harness, nameof(MeasureCompletions)));
            Main.Log("Station replay armed: " + replay.save);
        }

        private static void UseSilentOutput(FMOD.Studio.System __instance)
        {
            if (__instance.getCoreSystem(out var core) != FMOD.RESULT.OK ||
                core.setOutput(FMOD.OUTPUTTYPE.NOSOUND) != FMOD.RESULT.OK)
                throw new InvalidOperationException("Cannot initialize silent FMOD benchmark.");
        }
        private static bool SuppressSave(ref bool __result) { __result = true; return false; }
        private static void SupplyBatchInput(ref TIStandaloneInputModule __result)
        {
            if (__result != null) return;
            if (batchInput == null)
            {
                var events = UnityEngine.EventSystems.EventSystem.current;
                if (events == null) return;
                batchInput = events.GetComponent<TIStandaloneInputModule>();
                if (batchInput == null) batchInput = events.gameObject.AddComponent<TIStandaloneInputModule>();
            }
            __result = batchInput;
        }
        private static void BeginShell(out long __state) { __state = Stopwatch.GetTimestamp(); }
        private static void EndShell(long __state) { shellTicks += Stopwatch.GetTimestamp() - __state; queries++; }
        private static void VerifyShell(MeshFilter ___Hull, Vector3 start, Vector3 end, bool __result)
        {
            Mesh mesh = ___Hull.sharedMesh;
            if (!referenceGeometry.TryGetValue(mesh, out var tree))
            {
                tree = RenderedMeshRaycast.For(mesh.isReadable ? mesh : CombatUnreadableMeshes.Copy(mesh));
                referenceGeometry.Add(mesh, tree);
            }
            Vector3 origin = ___Hull.transform.InverseTransformPoint(start);
            Vector3 delta = ___Hull.transform.InverseTransformPoint(end) - origin;
            float length = delta.magnitude;
            bool expected = length > 0f && tree.Intersect(origin, delta / length, length, out _, out _);
            if (__result != expected)
            {
                Main.Warn("Station clearance parity failure at query " + verifiedQueries);
                Application.Quit(3);
                throw new InvalidOperationException("Station clearance differs from the reference mesh.");
            }
            verifiedQueries++;
        }
        private static IEnumerable<CodeInstruction> MeasureCompletions(IEnumerable<CodeInstruction> source)
        {
            var complete = AccessTools.Method(typeof(JobHandle), nameof(JobHandle.Complete));
            var timed = AccessTools.Method(typeof(StationBattleReplay), nameof(TimedComplete));
            foreach (var instruction in source)
            {
                if (instruction.Calls(complete)) { instruction.opcode = OpCodes.Call; instruction.operand = timed; }
                yield return instruction;
            }
        }
        private static void TimedComplete(ref JobHandle handle)
        {
            long start = Stopwatch.GetTimestamp();
            handle.Complete();
            waitTicks += Stopwatch.GetTimestamp() - start;
        }

        private void Update()
        {
            try
            {
                if (Time.realtimeSinceStartup > deadline) throw new TimeoutException("Station replay timed out.");
                if (playing) { Sample(); return; }
                if (Time.realtimeSinceStartup < nextPoll) return;
                nextPoll = Time.realtimeSinceStartup + 1;
                if (!loaded)
                {
                    var menu = FindObjectOfType<StartMenuController>();
                    if (menu == null || menu.loadMenuController == null || Time.realtimeSinceStartup < 10) return;
                    loaded = true;
                    Main.Log("Station replay loading save");
                    menu.loadMenuController.LoadSaveFilePath(save);
                    return;
                }
                if (!GameControl.initialized) return;
                var manager = GameControl.spaceCombat;
                if (manager != null && manager.initialized && manager.combatHUD != null)
                {
                    if (manager.IsInFormationSelectionMode) manager.combatHUD.ConfirmFormation();
                    var clock = FindObjectOfType<SpaceCombatSpeedController>();
                    if (clock == null) return;
                    var time = World.Active.GetExistingManager<GameTimeManager>();
                    int setting = time.currentSpeeds.FindIndex(s => s.multiplier == speed);
                    if (setting < 0) throw new InvalidOperationException("Missing combat speed " + speed);
                    clock.Play();
                    clock.SetSpeed(setting);
                    playing = true;
                    Main.Log("Station replay combat speed " + time.currentSpeed);
                    return;
                }
                var pre = FindObjectOfType<PrecombatController>();
                if (Time.realtimeSinceStartup >= nextProgress)
                {
                    nextProgress = Time.realtimeSinceStartup + 15;
                    Main.Log("Station replay waiting: precombat=" + (pre != null) + ", combat manager=" + (manager != null));
                }
                if (pre == null || pre.combat == null)
                {
                    if (!startedEncounter)
                    {
                        foreach (var encounter in GameStateManager.IterateByClass<TISpaceCombatState>())
                        {
                            if (encounter.initialized || encounter.cachedFleet1 == null ||
                                (encounter.cachedFleet2 == null && encounter.cachedHab == null)) continue;
                            startedEncounter = true;
                            Main.Log("Station replay starting saved encounter");
                            encounter.StartCombatFromStrategyLayer();
                            break;
                        }
                    }
                    return;
                }
                if (pre.stanceSelectionObject.activeInHierarchy)
                    pre.StanceSubmit((int)(pre.engageButton.interactable ? CombatStance.Pursue : CombatStance.Defend));
                else if (pre.bidSelectionObject.activeInHierarchy) pre.BidSubmit();
                else if (pre.resolutionSelectionObject.activeInHierarchy) pre.LiveResolveSelected();
            }
            catch (Exception error) { Main.Warn("Station replay failed: " + error); enabled = false; Application.Quit(1); }
        }

        private void Sample()
        {
            var manager = GameControl.spaceCombat;
            long now = Stopwatch.GetTimestamp();
            int missiles = 0;
            foreach (int count in manager.liveMissiles.Values) missiles += count;
            double seconds = manager.combatDuration_s;
            if (missiles > 0 && firstSalvo < 0) { firstSalvo = seconds; Main.Log("Station replay salvo at " + seconds); }
            double unit = 1000.0 / Stopwatch.Frequency;
            if (lastFrame != 0) frames.Add(new Frame { combatSeconds = seconds, missiles = missiles,
                wallMs = (now - lastFrame) * unit, shellMs = shellTicks * unit,
                shellQueries = queries, jobCompleteMs = waitTicks * unit });
            lastFrame = now; shellTicks = waitTicks = 0; queries = 0;
            if (firstSalvo < 0 || seconds - firstSalvo < 180) return;
            if (throughSalvo && !manager.combatEnded)
            {
                if (missiles != 0) return;
                foreach (var projectile in manager._projectiles.Values)
                    if (projectile is MissileController && !projectile.hasHit && !projectile.beenDestroyed) return;
            }
            File.WriteAllText(output, JsonConvert.SerializeObject(new { save = Path.GetFileName(save), speed, firstSalvo, throughSalvo, verifiedQueries, frames }));
            enabled = false;
            Main.Log("Station replay completed: " + frames.Count + " frames");
            Application.Quit(0);
        }
    }
}
