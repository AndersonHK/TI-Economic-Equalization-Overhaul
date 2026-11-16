using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

// Runs in an isolated verification process, not in Unity. Only ownership's
// engine/event side effects and faction-template lookup are stubbed. Connector
// routing, topology guards, reconciliation and transformed native IL are real.
public static class HabSectorProgressionTests
{
    private static readonly List<TIFactionState> Aliens = new List<TIFactionState>();
    private static int assignments;
    private static int checks;
    private static Type policy;

    private static T Raw<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
    private static void Set(object obj, string name, object value)
    {
        PropertyInfo property = AccessTools.Property(obj.GetType(), name);
        if (property != null) property.SetValue(obj, value, null);
        else AccessTools.Field(obj.GetType(), name).SetValue(obj, value);
    }
    private static object Call(string name, params object[] args)
    {
        return AccessTools.Method(policy, name).Invoke(null, args);
    }
    private static void Assert(bool okay, string message)
    {
        checks++;
        if (!okay) throw new Exception("Hab progression: " + message);
    }
    public static bool Assign(TISectorState __instance, TIFactionState newFaction)
    {
        Set(__instance, "faction", newFaction);
        assignments++;
        return false;
    }
    public static bool Alien(TIFactionState __instance, ref bool __result)
    {
        __result = Aliens.Any(f => Object.ReferenceEquals(f, __instance));
        return false;
    }
    public static bool Resupply(ref bool __result)
    {
        __result = true;
        return false;
    }
    private static int Unlock(bool station, int sector)
    {
        return station ? (sector == 2 || sector == 4 ? 1 : 2) : (sector <= 2 ? 1 : 2);
    }
    private static TIHabState Hab(bool station, int tier, bool alien, bool fixedHab)
    {
        TIHabState h = Raw<TIHabState>();
        Set(h, "habType", station ? HabType.Station : HabType.Base);
        Set(h, "tier", tier);
        Set(h, "staticHab", fixedHab);
        h.sectors = new List<TISectorState>();
        TIFactionState owner = Raw<TIFactionState>();
        if (alien) Aliens.Add(owner);
        for (int s = 0; s < 5; s++)
        {
            TISectorState sector = Raw<TISectorState>();
            sector.hab = h;
            sector.sectorNum = s;
            sector.slots = s == 0 ? 5 : 4;
            sector.habModules = new List<TIHabModuleState>();
            Set(sector, "faction", s == 0 || tier >= Unlock(station, s) + 1 ? owner : null);
            for (int m = 0; m < sector.slots; m++)
            {
                TIHabModuleState module = Raw<TIHabModuleState>();
                Set(module, "sector", sector);
                Set(module, "slot", m);
                Set(module, "templateName", "");
                sector.habModules.Add(module);
            }
            h.sectors.Add(sector);
        }
        Occupy(h.sectors[0].habModules[0], tier);
        return h;
    }
    private static void Occupy(TIHabModuleState m, int tier)
    {
        TIHabModuleTemplate t = Raw<TIHabModuleTemplate>();
        t.tier = tier;
        t.coreModule = m.sectorNum == 0 && m.slot == 0;
        Set(m, "templateName", "HeadlessFixture");
        Set(m, "moduleTemplate", t);
    }
    private static string Snapshot(TIHabState h)
    {
        h.UpdateAllModuleConnectors();
        return Flags(h);
    }
    private static string Flags(TIHabState h)
    {
        return String.Join("", h.sectors.SelectMany(s => s.habModules).Select(m =>
            String.Join("", new[] { m.C0, m.N1, m.N2, m.E1, m.E2, m.W1, m.W2, m.S1, m.S2 }
                .Select(b => b ? "1" : "0")))) + ":" +
            h.ringStruct.NE + h.ringStruct.SE + h.ringStruct.SW + h.ringStruct.NW + ":" +
            h.connStruct.C42 + h.connStruct.C16 + h.connStruct.C36 + h.connStruct.C46 +
            h.connStruct.C56 + h.connStruct.C76;
    }
    private sealed class Fixture
    {
        public TIHabState Hab;
        public string Expected;
        public string Name;
    }
    public static string Run(Assembly mod)
    {
        policy = mod.GetType("TIEconomyMod.Patches.HabSectorProgression", true);
        Harmony harmony = new Harmony("ti-eeo.hab-execution-validation");
        harmony.Patch(AccessTools.Method(typeof(TISectorState), "SetFaction"),
            prefix: new HarmonyMethod(typeof(HabSectorProgressionTests), "Assign"));
        harmony.Patch(AccessTools.PropertyGetter(typeof(TIFactionState), "IsAlienFaction"),
            prefix: new HarmonyMethod(typeof(HabSectorProgressionTests), "Alien"));
        List<Fixture> fixtures = new List<Fixture>();
        // All 16 outer coordinates singly occupied, plus empty, dense, and
        // mixed-size/damaged/building states. Compare the entire connector map
        // to unmodified native routing with the equivalent sector tier.
        for (int kind = 0; kind < 2; kind++)
        for (int tier = 1; tier <= 3; tier++)
        for (int control = 0; control < 3; control++)
        for (int pattern = 0; pattern < 19; pattern++)
        {
            bool station = kind == 0, alien = control == 1, fixedHab = control == 2;
            TIHabState h = Hab(station, tier, alien, fixedHab);
            if (!alien && !fixedHab)
            {
                foreach (TISectorState sector in h.sectors.Skip(1))
                    if (tier >= Unlock(station, sector.sectorNum)) Set(sector, "faction", h.sectors[0].faction);
            }
            if (pattern >= 1 && pattern <= 16)
            {
                int s = 1 + (pattern - 1) / 4, slot = (pattern - 1) % 4;
                if (h.sectors[s].active) Occupy(h.sectors[s].habModules[slot], tier);
            }
            if (pattern >= 17)
                foreach (TIHabModuleState m in h.sectors.Where(s => s.active).SelectMany(s => s.habModules).Skip(1))
                {
                    Occupy(m, pattern == 18 ? 1 + m.slot % tier : tier);
                    Set(m, "constructionCompleted", pattern != 18 || m.slot % 2 == 0);
                    Set(m, "destroyed", pattern == 18 && m.slot == 2);
                }
            int originalTier = h.tier;
            if (!alien && !fixedHab) Set(h, "tier", Math.Min(3, tier + 1));
            string expected = Snapshot(h);
            Set(h, "tier", originalTier);
            fixtures.Add(new Fixture { Hab = h, Expected = expected,
                Name = kind + "/" + tier + "/" + control + "/" + pattern });
        }
        Type connector = mod.GetType("TIEconomyMod.Patches.HabSectorConnectorMapPatch", true);
        harmony.Patch(AccessTools.Method(typeof(TISectorState), "UpdateModuleConnectorMap",
            new[] { typeof(TIHabState), typeof(TIHabModuleState) }),
            transpiler: new HarmonyMethod(AccessTools.Method(connector, "Transpiler")));
        foreach (Fixture f in fixtures)
            Assert(Snapshot(f.Hab) == f.Expected, "connector/native routing equivalence " + f.Name);

        for (int kind = 0; kind < 2; kind++)
        for (int tier = 1; tier <= 3; tier++)
        for (int control = 0; control < 3; control++)
        {
            TIHabState h = Hab(kind == 0, tier, control == 1, control == 2);
            List<TIHabModuleState> identities = h.sectors.SelectMany(s => s.habModules).ToList();
            DateTime queued = new DateTime(2040, 1, 1);
            Set(h.sectors[2].habModules[3], "completionDate", queued);
            Call("ReconcileAndRefresh", h);
            Assert(h.sectors.SelectMany(s => s.habModules).SequenceEqual(identities), "module identities preserved");
            Assert(h.sectors[2].habModules[3].completionDate == queued, "queued job preserved");
            if (control == 0)
                Assert(h.sectors.Count(s => s.active) == (tier == 1 ? 3 : 5), "active sector count");
            else
                Assert(h.sectors.Count(s => s.active) == (tier == 1 ? 1 : tier == 2 ? 3 : 5), "alien/static native activation");
            int before = assignments;
            Assert(!(bool)Call("Reconcile", h) && assignments == before, "idempotent reconciliation");
            // A T1 queued upgrade leaves the tier untouched; completion uses
            // the native rebuild's injected reconciliation after changing tier.
            if (control == 0 && tier == 1)
            {
                TIFactionState foreign = Raw<TIFactionState>();
                TISectorState occupied = h.sectors[kind == 0 ? 2 : 1];
                Set(occupied, "faction", foreign);
                Set(h, "tier", 2);
                Call("ReconcileBeforeConnectors", h);
                Assert(h.sectors.All(s => s.active), "completed T1 to T2 activation");
                Call("ActivateUpgradeSector", occupied, h.sectors[0].faction);
                Assert(Object.ReferenceEquals(occupied.faction, foreign), "foreign upgrade sector preserved");
                Set(h, "tier", 3);
                Call("ReconcileAndRefresh", h);
                Assert(h.sectors.SelectMany(s => s.habModules).SequenceEqual(identities), "T3 transition preserves IDs");
            }
        }
        Assert(!(bool)Call("Reconcile", new object[] { null }), "null topology rejected");
        TIHabState malformed = Hab(true, 1, false, false);
        malformed.sectors[4].habModules.RemoveAt(0);
        Assert(!(bool)Call("Reconcile", malformed), "malformed topology rejected");
        TIHabState inactive = Hab(true, 1, false, false);
        Assert((int)Call("ConnectorTierRequirement", inactive, 2, 2) == 2, "inactive connector stays gated");
        Assert((int)Call("ConnectorTierRequirement", inactive, 5, 3) == 3, "invalid sector native fallback");
        TIHabState ownerless = Hab(true, 1, false, false);
        Set(ownerless.sectors[0], "faction", null);
        Assert(!(bool)Call("Reconcile", ownerless), "missing core owner rejected");
        TIHabState stale = Hab(true, 1, false, false);
        Call("Reconcile", stale);
        Occupy(stale.sectors[2].habModules[3], 1);
        string expectedStartup = Snapshot(stale);
        foreach (TIHabModuleState m in stale.sectors.SelectMany(s => s.habModules))
            m.N1 = !m.N1;
        Type startup = mod.GetType("TIEconomyMod.Patches.InitializeHabSectorProgressionPatch", true);
        AccessTools.Method(startup, "Prefix").Invoke(null, new object[] { stale });
        Assert(Flags(stale) == expectedStartup, "startup refreshes stale flags even without newly activated sectors");

        harmony.Patch(AccessTools.Method(typeof(TIHabState), "AllowsResupply"),
            prefix: new HarmonyMethod(typeof(HabSectorProgressionTests), "Resupply"));
        Type refuel = mod.GetType("TIEconomyMod.Patches.HabRefuellingCapacityPatch", true);
        harmony.Patch(AccessTools.Method(typeof(FactionGoal_BuildRefuellingStation), "GoalFulfilled"),
            prefix: new HarmonyMethod(AccessTools.Method(refuel, "Prefix")));
        for (int tier = 1; tier <= 3; tier++)
        {
            TIHabState h = Hab(true, tier, false, false);
            Call("Reconcile", h);
            FactionGoal_BuildRefuellingStation goal = Raw<FactionGoal_BuildRefuellingStation>();
            Set(goal, "hab", h);
            goal.faction = h.sectors[0].faction;
            List<TIHabModuleState> slots = h.sectors.Where(s => s.active).SelectMany(s => s.habModules).ToList();
            for (int i = 0; i < slots.Count; i++)
            {
                Occupy(slots[i], tier);
                Set(slots[i], "constructionCompleted", true);
                Assert(goal.GoalFulfilled() == (i == slots.Count - 1), "AI uses actual expanded capacity");
            }
        }

        Type upgrade = mod.GetType("TIEconomyMod.Patches.HabUpgradeSectorProgressionPatch", true);
        MethodInfo target = AccessTools.Method(typeof(TIHabState), "CompleteModuleConstruction");
        List<CodeInstruction> original = PatchProcessor.GetOriginalInstructions(target);
        var transformed = ((IEnumerable<CodeInstruction>)AccessTools.Method(upgrade, "Transpiler")
            .Invoke(null, new object[] { original })).ToList();
        MethodInfo rebuild = AccessTools.Method(typeof(TIHabState), "UpdateAllModuleConnectors");
        MethodInfo reconcile = AccessTools.Method(policy, "ReconcileBeforeConnectors");
        int rebuildAt = transformed.FindIndex(c => c.Calls(rebuild));
        Assert(rebuildAt > 1 && transformed[rebuildAt - 1].Calls(reconcile) &&
            transformed[rebuildAt - 2].opcode == System.Reflection.Emit.OpCodes.Dup,
            "completion reconciles before native rebuild and final event");
        Assert(harmony.Patch(target, transpiler: new HarmonyMethod(AccessTools.Method(upgrade, "Transpiler"))) != null,
            "upgrade IL emits");
        foreach (string patchName in new[] { "InitializeHabSectorProgressionPatch", "RepairExistingHabSectorPatch" })
        {
            Type patch = mod.GetType("TIEconomyMod.Patches." + patchName, true);
            MethodInfo hook = AccessTools.Method(patch, "Prefix");
            string native = patchName == "InitializeHabSectorProgressionPatch" ? "OnHabCreated" : "PostEverythingSaveRepair_8";
            Assert(harmony.Patch(AccessTools.Method(typeof(TIHabState), native), prefix: new HarmonyMethod(hook)) != null,
                native + " prefix emits");
        }
        harmony.UnpatchAll(harmony.Id);
        return fixtures.Count + " executed connector fixtures, " + checks + " assertions. Ownership event side effects stubbed; live rendering remains manual.";
    }
}
