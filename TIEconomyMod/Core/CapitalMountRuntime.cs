using FullSerializer;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TIEconomyMod.Patches;

namespace TIEconomyMod.Core
{
    internal sealed class CapitalLayoutState
    {
        internal int Schema;
        internal bool LegacyCapacity;
    }

    internal sealed class CapitalConversion
    {
        internal TISpaceShipTemplate Design;
        internal List<ModuleDataTemplateEntry> Weapons;
        internal List<FireModeDataTemplateEntry> Modes;
        internal readonly Dictionary<int, int> SourceSlots = new Dictionary<int, int>();
        internal string CapacityWarning;
    }

    internal sealed class CapitalShipConversion
    {
        internal TISpaceShipState Ship;
        internal List<ModuleDataEntry> Weapons;
        internal Dictionary<ModuleDataEntry, int> Ammo;
        internal List<DamagedShipPartData> Damage;
        internal List<DamagedShipPartData> Repair;
        internal List<DamagedShipPartData> PreviousRepair;
        internal Dictionary<ModuleDataEntry, int> Reload;
    }

    internal static class CapitalMountRuntime
    {
        private static readonly ConditionalWeakTable<object, CapitalLayoutState> States = new ConditionalWeakTable<object, CapitalLayoutState>();
        private static readonly HashSet<TIFactionState> InitializedFactions = new HashSet<TIFactionState>();
        private static bool processorInstalled;
        private static string campaignSource;
        private static string migrationFailure;
        [ThreadStatic] internal static bool Planning;

        internal static CapitalLayoutState State(object instance) { return States.GetOrCreateValue(instance); }

        internal static void InstallSerializer()
        {
            if (processorInstalled) return;
            ((fsSerializer)AccessTools.Field(typeof(StringSerializationAPI), "_serializer").GetValue(null))
                .AddProcessor(new CapitalLayoutProcessor());
            processorInstalled = true;
        }

        internal static void BeginDeserialize(Type type, string source)
        {
            if (type.Name != "SaveStructure") return;
            campaignSource = source;
            migrationFailure = null;
            InitializedFactions.Clear();
        }

        internal static void FactionInitialized(TIFactionState faction)
        {
            InitializedFactions.Add(faction);
            if (GameStateManager.AllFactions().All(InitializedFactions.Contains)) MigrateCampaign();
        }

        internal static void AssertCanSave(Type type)
        {
            if (type.Name == "SaveStructure" && migrationFailure != null)
                throw new InvalidOperationException("EEO blocked saving after a failed capital migration: " + migrationFailure);
        }

        internal static bool ValidLayout(TISpaceShipTemplate design)
        {
            if (design.hullWeaponTemplateEntries == null) return true;
            var used = new HashSet<int>();
            return design.hullWeaponTemplateEntries.All(entry => entry.moduleTemplate != null &&
                entry.moduleTemplate.ref_weapon != null && entry.moduleTemplate.ref_weapon.mount == Mount.FourHull &&
                entry.slot >= 0 && entry.slot < design.hullTemplate.shipModuleSlots.Count &&
                design.hullTemplate.shipModuleSlots[entry.slot].moduleSlotType == ShipModuleSlotType.HullHardPoint && used.Add(entry.slot));
        }

        internal static void Invalidate(TISpaceShipTemplate design)
        {
            foreach (string name in new[] { "cachedHullWeapons", "cachedHullWeaponTemplates", "_spaceResourceConstructionCost" })
                AccessTools.Field(typeof(TISpaceShipTemplate), name).SetValue(design, null);
            foreach (string name in new[] { "_unnormalizedCombatValue", "_combatValue", "_baseCruiseDeltaV_kps", "_baseCruiseAcceleration_mps2", "_requiredExotics", "_requiredAntimatter", "_heatCapacity_GJ", "_batteryCapacity_GJ" })
                AccessTools.Field(typeof(TISpaceShipTemplate), name).SetValue(design, -1f);
            AccessTools.Field(typeof(TISpaceShipTemplate), "cachedDryMass_tons").SetValue(design, 0f);
        }

        internal static void EnsureDesign(TISpaceShipTemplate design)
        {
            EnsureDesignWithBackup(design, false);
        }

        internal static void EnsureImportedDesign(TISpaceShipTemplate design)
        {
            EnsureDesignWithBackup(design, true);
        }

        private static void EnsureDesignWithBackup(TISpaceShipTemplate design, bool imported)
        {
            if (Planning || design == null || !CapitalMountPolicy.Applies(design.hullName) || State(design).Schema == CapitalMountPolicy.Schema) return;
            CapitalConversion conversion = Prepare(design);
            // Stock skirmish/benchmark templates are initialized at the main
            // menu, before campaign factions exist. They contain no save data.
            // Imported save designs still need a backup when no live campaign
            // faction exists in the main-menu GameStateManager.
            if (imported || design.designingFaction != null) WriteReport(new[] { conversion }, false);
            Commit(conversion);
        }

        internal static void NormalizeStockDesigns()
        {
            foreach (TISpaceShipTemplate design in TemplateManager.GetAllTemplates<TISpaceShipTemplate>())
                if (CapitalMountPolicy.Applies(design.hullName) && design.designingFaction == null)
                    EnsureDesign(design);
        }

        private static List<TIShipWeaponTemplate> Eligible(TISpaceShipTemplate design)
        {
            TIFactionState faction = design.designingFaction;
            if (faction == null) return InferredStockWeapons(design);
            return TemplateManager.IterateByClass<TIShipWeaponTemplate>()
                .Where(w => w.mount == Mount.FourHull && w.isAlien == design.isAlien && w.FactionCanBuild(faction))
                .OrderByDescending(w => CapitalMountPolicy.FallbackRank(w.dataName))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).ToList();
        }

        private static List<TIShipWeaponTemplate> InferredStockWeapons(TISpaceShipTemplate design)
        {
            // TemplateManager is available at the main menu even though faction
            // research states are not. Infer a conservative tech floor from the
            // authored hull and weapons, without modifying any research state.
            var registry = TemplateManager.IterateByClass<TIGenericTechTemplate>()
                .ToDictionary(t => t.dataName, StringComparer.Ordinal);
            var known = new HashSet<string>(StringComparer.Ordinal);
            InferPrerequisites(design.hullTemplate.requiredProjectName, registry, known);
            foreach (ModuleDataTemplateEntry entry in (design.hullWeaponTemplateEntries ?? new List<ModuleDataTemplateEntry>())
                .Concat(design.noseWeaponTemplateEntries ?? new List<ModuleDataTemplateEntry>()))
                if (entry.moduleName != "Empty" && entry.moduleTemplate != null)
                    InferPrerequisites(entry.moduleTemplate.requiredProjectName, registry, known);
            return TemplateManager.IterateByClass<TIShipWeaponTemplate>()
                .Where(w => w.mount == Mount.FourHull && w.isAlien == design.isAlien &&
                    ProjectSupported(w.requiredProjectName, registry, known, new HashSet<string>()))
                .OrderByDescending(w => CapitalMountPolicy.FallbackRank(w.dataName))
                .ThenBy(w => w.dataName, StringComparer.Ordinal).ToList();
        }

        private static void InferPrerequisites(string name, Dictionary<string, TIGenericTechTemplate> registry, HashSet<string> known)
        {
            if (string.IsNullOrEmpty(name) || !known.Add(name)) return;
            TIGenericTechTemplate node;
            if (!registry.TryGetValue(name, out node)) return;
            // An installed project does not reveal which alternative route was
            // taken. Do not infer an entire branch from that ambiguous edge.
            for (int i = 0; i < node.prereqs.Count; ++i)
            {
                string alternative = i == 0 ? node.altPrereq0 : i == 1 ? node.altPrereq1 : null;
                if (string.IsNullOrEmpty(alternative)) InferPrerequisites(node.prereqs[i], registry, known);
            }
        }

        private static bool ProjectSupported(string name, Dictionary<string, TIGenericTechTemplate> registry,
            HashSet<string> known, HashSet<string> visiting)
        {
            if (string.IsNullOrEmpty(name) || known.Contains(name)) return true;
            TIGenericTechTemplate node;
            if (!registry.TryGetValue(name, out node) || node is TITechTemplate || !visiting.Add(name)) return false;
            // Other engineering projects at the inferred global-tech level are
            // acceptable substitutes. Unknown root projects (notably alien
            // master projects) must never become inferred unlocks for free.
            bool supported = node.prereqs.Count > 0;
            for (int i = 0; supported && i < node.prereqs.Count; ++i)
            {
                string alternative = i == 0 ? node.altPrereq0 : i == 1 ? node.altPrereq1 : null;
                supported = ProjectSupported(node.prereqs[i], registry, known, visiting) ||
                    (!string.IsNullOrEmpty(alternative) && ProjectSupported(alternative, registry, known, visiting));
            }
            visiting.Remove(name);
            return supported;
        }

        private static TIShipWeaponTemplate Replacement(TIShipWeaponTemplate old, List<TIShipWeaponTemplate> eligible, bool owned)
        {
            // Already-installed heavy equipment is retained even if its unlock is no longer available.
            if (old.mount == Mount.FourHull) return old;
            string equivalent = CapitalMountPolicy.HeavyEquivalent(old.dataName);
            if (!owned && equivalent != null)
            {
                TIShipWeaponTemplate family = TemplateManager.Find<TIShipWeaponTemplate>(equivalent, true);
                if (family != null && family.mount == Mount.FourHull) return family;
            }
            TIShipWeaponTemplate result = eligible.FirstOrDefault(w => w.dataName == equivalent);
            if (result != null) return result;
            result = eligible.FirstOrDefault(w => w.attackMode) ?? eligible.FirstOrDefault();
            if (result == null)
            {
                // Legacy equipment is an explicit exception, never a research unlock.
                // Prefer the old weapon's heavy relative; otherwise use a baseline mag.
                result = equivalent == null ? null : TemplateManager.Find<TIShipWeaponTemplate>(equivalent, true);
                if (result == null || result.mount != Mount.FourHull)
                    result = TemplateManager.Find<TIShipWeaponTemplate>(old.isAlien ? "AlienHeavyMagBattery" : "HeavyRailgunBatteryMk1", true);
                if (owned) Main.Warn("No researched capital replacement for " + old.dataName + "; retaining legacy equipment access through " + result.dataName + ". No project unlocked.");
            }
            return result;
        }

        private static CapitalConversion Prepare(TISpaceShipTemplate design)
        {
            var result = new CapitalConversion { Design = design, Weapons = new List<ModuleDataTemplateEntry>(),
                Modes = new List<FireModeDataTemplateEntry>(design.fireModeTemplateEntries ?? new List<FireModeDataTemplateEntry>()) };
            var eligible = Eligible(design);
            foreach (ModuleDataTemplateEntry entry in design.hullWeaponTemplateEntries ?? new List<ModuleDataTemplateEntry>())
            {
                // Authored skirmish rosters retain explicit vacant-cell entries.
                if (entry.moduleName == "Empty") continue;
                TIShipWeaponTemplate old = entry.moduleTemplate?.ref_weapon;
                if (old == null || entry.slot < 0 || entry.slot >= design.hullTemplate.shipModuleSlots.Count)
                    throw new InvalidOperationException("Missing weapon or invalid slot in " + design.dataName);
                TIShipWeaponTemplate weapon = Replacement(old, eligible, design.designingFaction != null);
                TIShipHullTemplate.ShipModuleSlot anchor = design.hullTemplate.shipModuleSlots[entry.slot];
                FireMode mode = design.GetFireModeDataEntryFromSlot(entry.slot).fireMode;
                if (!weapon.GetActualFireModes().Contains(mode)) mode = weapon.DefaultFireMode;
                foreach (UtilityGridCell offset in CapitalMountPolicy.LegacyOffsets(old.mount.ToString()))
                {
                    int slot = design.hullTemplate.shipModuleSlots.FindIndex(s => s.x == anchor.x + offset.X && s.y == anchor.y + offset.Y && s.moduleSlotType == ShipModuleSlotType.HullHardPoint);
                    if (slot < 0 || result.SourceSlots.ContainsKey(slot))
                        throw new InvalidOperationException("Invalid or overlapping legacy footprint in " + design.dataName + " at " + entry.slot);
                    result.SourceSlots.Add(slot, entry.slot);
                    result.Weapons.Add(new ModuleDataTemplateEntry(weapon, slot));
                    result.Modes.RemoveAll(m => m.slot == slot);
                    result.Modes.Add(new FireModeDataTemplateEntry(slot, mode));
                }
            }
            Planning = true;
            try
            {
                TISpaceShipTemplate trial = design.Clone(design.dataName, design.factionName);
                trial.hullWeaponTemplateEntries = result.Weapons;
                trial.fireModeTemplateEntries = result.Modes;
                State(trial).Schema = CapitalMountPolicy.Schema;
                Invalidate(trial);
                if (!ValidLayout(trial)) throw new InvalidOperationException("Invalid converted layout in " + design.dataName);
                var issues = new List<string>();
                FuelCapacitySnapshot fuel;
                if (HullFuelCapacityFeature.TryGetSnapshot(trial, out fuel) && trial.propellantTanks > fuel.MaximumTanks)
                    issues.Add("fuel tanks " + trial.propellantTanks + " exceed capacity " + fuel.MaximumTanks);
                if (trial.driveTemplate != null && trial.powerPlantTemplate != null && !HullFuelCapacityFeature.IsPropulsionSpaceLegal(trial))
                    issues.Add("reactor/drive capacity exceeded");
                if (trial.radiatorTemplate != null && trial.powerPlantTemplate != null && trial.driveTemplate != null)
                {
                    float mass = trial.dryMass_tons(true);
                    if (float.IsNaN(mass) || float.IsInfinity(mass) || mass <= 0)
                        throw new InvalidOperationException("Non-finite converted mass in " + design.dataName);
                }
                result.CapacityWarning = string.Join("; ", issues);
            }
            finally { Planning = false; }
            return result;
        }

        private static void Commit(CapitalConversion conversion)
        {
            conversion.Design.hullWeaponTemplateEntries = conversion.Weapons;
            conversion.Design.fireModeTemplateEntries = conversion.Modes;
            State(conversion.Design).Schema = CapitalMountPolicy.Schema;
            State(conversion.Design).LegacyCapacity = !string.IsNullOrEmpty(conversion.CapacityWarning);
            Invalidate(conversion.Design);
            if (!string.IsNullOrEmpty(conversion.CapacityWarning))
                Main.Warn("Capital migration " + conversion.Design.dataName + ": " + conversion.CapacityWarning);
        }

        private static CapitalShipConversion PrepareShip(TISpaceShipState ship, CapitalConversion design)
        {
            var result = new CapitalShipConversion { Ship = ship, Weapons = new List<ModuleDataEntry>(),
                Ammo = new Dictionary<ModuleDataEntry, int>(ship.ammo),
                Damage = new List<DamagedShipPartData>(ship.damagedParts),
                Repair = new List<DamagedShipPartData>(ship.plannedResupplyAndRepair.modulesToRepair),
                PreviousRepair = new List<DamagedShipPartData>(ship.prevPartsBeingRepaired),
                Reload = new Dictionary<ModuleDataEntry, int>(ship.plannedResupplyAndRepair.ammoToReload) };
            foreach (ModuleDataEntry old in ship.hullWeapons)
            {
                result.Ammo.Remove(old);
                result.Reload.Remove(old);
                result.Damage.RemoveAll(d => d.module.Equals(old));
                result.Repair.RemoveAll(d => d.module.Equals(old));
                result.PreviousRepair.RemoveAll(d => d.module.Equals(old));
            }
            foreach (ModuleDataTemplateEntry entry in design.Weapons)
            {
                int sourceSlot = design.SourceSlots[entry.slot];
                ModuleDataEntry old = ship.hullWeapons.SingleOrDefault(w => w.slotIndex == sourceSlot);
                if (old == null) throw new InvalidOperationException("Built ship " + ship.ID + " lacks legacy weapon at slot " + sourceSlot);
                var replacement = new ModuleDataEntry(entry.moduleTemplate, entry.slot);
                result.Weapons.Add(replacement);
                CopyDamage(ship.damagedParts, result.Damage, old, replacement);
                CopyDamage(ship.plannedResupplyAndRepair.modulesToRepair, result.Repair, old, replacement);
                CopyDamage(ship.prevPartsBeingRepaired, result.PreviousRepair, old, replacement);
                TIProjectileWeaponTemplate projectile = replacement.weaponTemplate.ref_projectileWeapon;
                if (projectile != null && projectile.hasMagazine())
                {
                    int max = projectile.FullAmmoCount_Current(ship);
                    TIProjectileWeaponTemplate oldProjectile = old.weaponTemplate.ref_projectileWeapon;
                    int oldMax = oldProjectile == null ? 0 : oldProjectile.FullAmmoCount_Current(ship);
                    int remaining;
                    // Energy-to-projectile fallback has no depletion history; start loaded.
                    result.Ammo[replacement] = oldProjectile == null ? max : CapitalMountPolicy.ConvertedAmmo(
                        ship.ammo.TryGetValue(old, out remaining) ? remaining : 0, oldMax, max);
                    int reload;
                    if (ship.plannedResupplyAndRepair.ammoToReload.TryGetValue(old, out reload))
                        result.Reload[replacement] = CapitalMountPolicy.ConvertedAmmo(reload, oldMax, max);
                }
            }
            return result;
        }

        private static void CopyDamage(List<DamagedShipPartData> source, List<DamagedShipPartData> destination, ModuleDataEntry old, ModuleDataEntry replacement)
        {
            DamagedShipPartData damage = source.FirstOrDefault(d => d.module.Equals(old));
            if (damage != null) destination.Add(new DamagedShipPartData(replacement, damage.damage));
        }

        private static void CommitShip(CapitalShipConversion conversion)
        {
            TISpaceShipState ship = conversion.Ship;
            ship.hullWeapons = conversion.Weapons;
            ship.ammo.Clear();
            foreach (var pair in conversion.Ammo) ship.ammo.Add(pair.Key, pair.Value);
            ship.damagedParts = conversion.Damage;
            ship.prevPartsBeingRepaired = conversion.PreviousRepair;
            ship.plannedResupplyAndRepair.modulesToRepair = conversion.Repair;
            ship.plannedResupplyAndRepair.ammoToReload = conversion.Reload;
            var damageCache = (Dictionary<ModuleDataEntry, DamagedShipPartData>)AccessTools.Field(typeof(TISpaceShipState), "damagedPartsCache").GetValue(ship);
            damageCache.Clear();
            foreach (DamagedShipPartData damage in conversion.Damage) damageCache[damage.module] = damage;
            ship.spaceCombatValueDataDirty = true;
            ship.effectiveBeamWeaponRange_km = null;
            State(ship).Schema = CapitalMountPolicy.Schema;
        }

        private static void MigrateCampaign()
        {
            try
            {
                var designs = new HashSet<TISpaceShipTemplate>();
                var ships = GameStateManager.IterateByClass<TISpaceShipState>().Where(s => !s.deleted).ToList();
                foreach (TIFactionState faction in GameStateManager.AllFactions())
                {
                    if (faction.shipDesigns != null) designs.UnionWith(faction.shipDesigns);
                    // Native phase 2 folds the obsolete shipRefitDesigns field into names.
                    foreach (string name in faction.shipRefitDesignNames ?? new List<string>())
                        designs.Add(TemplateManager.Find<TISpaceShipTemplate>(name));
                    if (faction.nShipyardQueues != null)
                        foreach (var queue in faction.nShipyardQueues.Values)
                            foreach (ShipConstructionQueueItem item in queue)
                            {
                                designs.Add(item.shipDesign);
                                designs.Add(item.refit_originalShipDesign);
                            }
                }
                designs.UnionWith(ships.Select(s => s.template));
                List<CapitalConversion> plans = designs.Where(d => d != null && CapitalMountPolicy.Applies(d.hullName) && State(d).Schema == 0)
                    .OrderBy(d => d.dataName, StringComparer.Ordinal).Select(Prepare).ToList();
                var shipPlans = new List<CapitalShipConversion>();
                foreach (TISpaceShipState ship in ships.Where(s => CapitalMountPolicy.Applies(s.template.hullName) && State(s).Schema == 0))
                {
                    CapitalConversion plan = plans.SingleOrDefault(p => ReferenceEquals(p.Design, ship.template));
                    if (plan == null)
                        throw new InvalidOperationException("Mismatched design/ship migration schema for ship " + ship.ID);
                    shipPlans.Add(PrepareShip(ship, plan));
                }
                if (plans.Count > 0)
                {
                    WriteReport(plans, true);
                    foreach (CapitalConversion plan in plans) Commit(plan);
                    foreach (CapitalShipConversion plan in shipPlans) CommitShip(plan);
                    Main.Log("Capital migration committed: " + plans.Count + " designs, " + shipPlans.Count + " built ships.");
                }
                // Native phase 2/3 initialization refreshes built-ship power, propulsion,
                // heat and combat systems after this transaction. Also clear rounded-mass caches.
                foreach (TISpaceShipTemplate design in designs.Where(d => d != null)) Invalidate(design);
                foreach (TISpaceShipState ship in ships)
                {
                    float mass = ship.template.dryMass_kg + ship.propellant_tons * 1000f;
                    foreach (ModuleDataEntry weapon in ship.hullWeapons.Concat(ship.noseWeapons))
                    {
                        TIProjectileWeaponTemplate projectile = weapon.weaponTemplate.ref_projectileWeapon;
                        int remaining;
                        if (projectile != null && projectile.hasMagazine() && ship.ammo.TryGetValue(weapon, out remaining))
                            mass -= Math.Max(0, projectile.FullAmmoCount_Current(ship) - remaining) * projectile.ammoMass_kg;
                    }
                    AccessTools.Property(typeof(TISpaceShipState), "currentMass_kg").SetValue(ship, mass, null);
                }
                campaignSource = null;
            }
            catch (Exception exception)
            {
                migrationFailure = exception.Message;
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TIEconomyMod", "CapitalMigrationBackups");
                Directory.CreateDirectory(directory);
                string stem = Path.Combine(directory, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + "-failed");
                File.WriteAllText(stem + ".txt", exception.ToString());
                if (campaignSource != null) File.WriteAllText(stem + ".json", campaignSource);
                Main.Error("Capital migration failed before campaign handoff. Saving is blocked. " + exception);
                throw;
            }
        }

        private static void WriteReport(IEnumerable<CapitalConversion> plans, bool campaign)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TIEconomyMod", "CapitalMigrationBackups");
            Directory.CreateDirectory(directory);
            string stem = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff") + (campaign ? "-campaign" : "-design");
            var report = new StringBuilder("EEO capital mount schema 1 preflight\n");
            foreach (CapitalConversion plan in plans)
            {
                report.AppendLine(plan.Design.dataName + " (" + plan.Design.hullName + ") " + plan.CapacityWarning);
                foreach (ModuleDataTemplateEntry weapon in plan.Weapons)
                    report.AppendLine("  old slot " + plan.SourceSlots[weapon.slot] + " -> " + weapon.slot + ": " + weapon.moduleName);
                if (!campaign)
                    File.WriteAllText(Path.Combine(directory, stem + ".json"), StringSerializationAPI.SerializePretty(typeof(TISpaceShipTemplate), plan.Design));
            }
            if (campaign && campaignSource != null) File.WriteAllText(Path.Combine(directory, stem + ".json"), campaignSource);
            File.WriteAllText(Path.Combine(directory, stem + ".txt"), report.ToString());
            Main.Log("Capital migration preflight archived at " + Path.Combine(directory, stem + ".txt"));
        }
    }

    internal sealed class CapitalLayoutProcessor : fsObjectProcessor
    {
        public override bool CanProcess(Type type) { return type == typeof(TISpaceShipTemplate) || type == typeof(TISpaceShipState); }

        public override void OnBeforeDeserializeAfterInstanceCreation(Type storageType, object instance, ref fsData data)
        {
            ReadMarker(instance, data);
        }

        internal static void ReadMarker(object instance, fsData data)
        {
            if (instance == null || !data.IsDictionary) return;
            if (instance is TISpaceShipState && !data.AsDictionary.ContainsKey("ID")) return;
            fsData marker;
            int schema = data.AsDictionary.TryGetValue("eeoCapitalLayout", out marker) && marker.IsInt64 ? (int)marker.AsInt64 : 0;
            if (schema < 0 || schema > CapitalMountPolicy.Schema) throw new InvalidOperationException("Unsupported EEO capital layout schema " + schema);
            CapitalMountRuntime.State(instance).Schema = schema;
            if (data.AsDictionary.TryGetValue("eeoLegacyCapacity", out marker) && marker.IsBool)
                CapitalMountRuntime.State(instance).LegacyCapacity = marker.AsBool;
        }

        public override void OnAfterSerialize(Type storageType, object instance, ref fsData data)
        {
            if (instance == null || !data.IsDictionary) return;
            if (instance is TISpaceShipState && !data.AsDictionary.ContainsKey("ID")) return;
            CapitalLayoutState state = CapitalMountRuntime.State(instance);
            data.AsDictionary["eeoCapitalLayout"] = new fsData((long)state.Schema);
            if (state.LegacyCapacity) data.AsDictionary["eeoLegacyCapacity"] = new fsData(true);
        }
    }
}
