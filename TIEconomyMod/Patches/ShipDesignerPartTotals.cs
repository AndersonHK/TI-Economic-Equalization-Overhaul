using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    internal sealed class DesignerPartTotals
    {
        private static ConditionalWeakTable<TISpaceShipTemplate, DesignerPartTotals> designs =
            new ConditionalWeakTable<TISpaceShipTemplate, DesignerPartTotals>();

        private readonly TIShipHullTemplate hull;
        private readonly IList<ModuleDataTemplateEntry> utilities;
        private readonly IList<ModuleDataTemplateEntry> nose;
        private readonly IList<ModuleDataTemplateEntry> hullWeapons;
        private readonly Dictionary<TIShipWeaponTemplate, int> weapons =
            new Dictionary<TIShipWeaponTemplate, int>();
        private readonly Dictionary<FactionResource, double> costs =
            new Dictionary<FactionResource, double>();
        private int utilityCount;
        private int noseCount;
        private int hullWeaponCount;
        private int crew;
        private double mass;
        private double heatCapacity;
        private double batteryCapacity;
        private double utilityPower;
        private double magazine;
        private double laserBonus;
        private double particleBonus;
        private double weaponPower;
        private double weaponHeat;
        private float appliedMagazine;
        private float appliedLaser;
        private float appliedParticle;

        internal int UtilityCells { get; private set; }
        internal int NoseCells { get; private set; }
        internal int HullCells { get; private set; }
        internal float PartMass => (float)mass;
        internal float HeatCapacity => (float)heatCapacity;
        internal float BatteryCapacity => (float)batteryCapacity;
        internal float Magazine => (float)magazine;
        internal float LaserBonus => (float)laserBonus;
        internal float ParticleBonus => (float)particleBonus;
        internal float WeaponPower => (float)weaponPower;
        internal float WeaponHeat => (float)weaponHeat;

        private DesignerPartTotals(TISpaceShipTemplate ship)
        {
            hull = ship.hullTemplate;
            utilities = ship.moduleTemplateEntries;
            nose = ship.noseWeaponTemplateEntries;
            hullWeapons = ship.hullWeaponTemplateEntries;
            utilityCount = utilities.Count;
            noseCount = nose.Count;
            hullWeaponCount = hullWeapons.Count;
            foreach (ModuleDataEntry entry in ship.utilityModules)
                ChangeUtility(entry.moduleTemplate, 1);
            appliedMagazine = Magazine;
            appliedLaser = LaserBonus;
            appliedParticle = ParticleBonus;
            foreach (ModuleDataEntry entry in ship.noseWeapons)
                ChangeWeapon(entry.moduleTemplate.ref_weapon, true, 1);
            foreach (ModuleDataEntry entry in ship.hullWeapons)
                ChangeWeapon(entry.moduleTemplate.ref_weapon, false, 1);
        }

        internal static DesignerPartTotals ForEdit(TISpaceShipTemplate ship)
        {
            var totals = designs.GetValue(ship, design => new DesignerPartTotals(design));
            if (totals.Matches(ship))
                return totals;

            Invalidate(ship);
            return designs.GetValue(ship, design => new DesignerPartTotals(design));
        }

        internal static void Invalidate(TISpaceShipTemplate ship)
        {
            if (ship != null)
                designs.Remove(ship);
        }

        internal static void InvalidateAll()
        {
            designs = new ConditionalWeakTable<TISpaceShipTemplate, DesignerPartTotals>();
        }

        private bool Matches(TISpaceShipTemplate ship)
        {
            return hull == ship.hullTemplate && utilities == ship.moduleTemplateEntries &&
                nose == ship.noseWeaponTemplateEntries && hullWeapons == ship.hullWeaponTemplateEntries &&
                utilityCount == utilities.Count && noseCount == nose.Count && hullWeaponCount == hullWeapons.Count;
        }

        internal static void AddEntry(List<ModuleDataTemplateEntry> entries, ModuleDataTemplateEntry entry)
        {
            entries.Add(entry);
            DesignerPartEdit.RecordEntry(entries, entry, 1);
        }

        internal static bool RemoveEntry(List<ModuleDataTemplateEntry> entries, ModuleDataTemplateEntry entry)
        {
            bool removed = entries.Remove(entry);
            if (removed)
                DesignerPartEdit.RecordEntry(entries, entry, -1);
            return removed;
        }

        internal static void TrackMutations(List<CodeInstruction> code, bool installing)
        {
            Type entries = typeof(List<ModuleDataTemplateEntry>);
            var native = AccessTools.Method(entries, installing ? "Add" : "Remove");
            var tracked = AccessTools.Method(typeof(DesignerPartTotals),
                installing ? nameof(AddEntry) : nameof(RemoveEntry));
            int count = 0;
            foreach (CodeInstruction instruction in code)
            {
                if (!instruction.Calls(native))
                    continue;
                instruction.opcode = OpCodes.Call;
                instruction.operand = tracked;
                count++;
            }
            if (count != 3)
                throw new InvalidOperationException("Designer module list mutation sites changed.");
        }

        internal void Apply(TISpaceShipTemplate ship, List<ModuleDataTemplateEntry> entries,
            ModuleDataTemplateEntry entry, int count)
        {
            bool utility = ReferenceEquals(entries, utilities);
            bool noseMount = ReferenceEquals(entries, nose);
            if (!utility && !noseMount && !ReferenceEquals(entries, hullWeapons))
                return;

            utilityCount = utilities.Count;
            noseCount = nose.Count;
            hullWeaponCount = hullWeapons.Count;
            var part = entry.moduleTemplate;
            if (part == null || !ship.ValidAssignedSlotForLocation(part, entry.slot))
                return;

            if (utility)
                ChangeUtility(part, count);
            else
                ChangeWeapon(part.ref_weapon, noseMount, count);
        }

        private void ChangeUtility(TIShipPartTemplate part, int count)
        {
            UtilityCells += count * UtilityFootprintMath.GetOffsets(UtilityFootprintRegistry.GetFootprint(part)).Count;
            crew += count * part.crew;
            mass += count * (double)part.buildMass_tons();
            ChangeCost(part.buildCost(), count);
            heatCapacity += count * (double)Math.Max(0f, part.ref_heatSink?.heatCapacity_GJ ?? 0f);
            batteryCapacity += count * (double)(part.ref_battery?.GetCapacity(false) ?? 0f);
            TIUtilityModuleTemplate utility = part.ref_utilityModule;
            if (utility == null)
                return;

            utilityPower += count * (double)(utility.powerRequirement_MW / 1000f);
            if (utility.specialModuleRules.Contains(SpecialModuleRule.Magazine))
                magazine += count * (double)utility.specialModuleValue;
            laserBonus += count * (double)Math.Max(0f, utility.laserPowerBonus_MW);
            particleBonus += count * (double)Math.Max(0f, utility.particleBeamPowerBonus_MW);
        }

        private void ChangeWeapon(TIShipWeaponTemplate weapon, bool noseMount, int count)
        {
            if (noseMount)
                NoseCells += count * CapitalMountPolicy.CapacitySize(weapon.dataName, weapon.internalSize);
            else
                HullCells += count * Math.Max(1, weapon.internalSize);
            crew += count * weapon.crew;
            weapons.TryGetValue(weapon, out int existing);
            if (existing + count == 0)
                weapons.Remove(weapon);
            else
                weapons[weapon] = existing + count;
            ChangeWeaponMassAndCost(weapon, appliedMagazine, count);
            ChangeWeaponPower(weapon, appliedLaser, appliedParticle, count);
        }

        internal void ApplyModifiers()
        {
            float nextMagazine = Magazine;
            float nextLaser = LaserBonus;
            float nextParticle = ParticleBonus;
            if (nextMagazine == appliedMagazine && nextLaser == appliedLaser && nextParticle == appliedParticle)
                return;

            foreach (var installed in weapons)
            {
                var weapon = installed.Key;
                int count = installed.Value;
                if (nextMagazine != appliedMagazine)
                {
                    ChangeWeaponMassAndCost(weapon, appliedMagazine, -count);
                    ChangeWeaponMassAndCost(weapon, nextMagazine, count);
                }
                if ((weapon.isLaserWeapon && nextLaser != appliedLaser) ||
                    (weapon.isParticleWeapon && nextParticle != appliedParticle))
                {
                    ChangeWeaponPower(weapon, appliedLaser, appliedParticle, -count);
                    ChangeWeaponPower(weapon, nextLaser, nextParticle, count);
                }
            }
            appliedMagazine = nextMagazine;
            appliedLaser = nextLaser;
            appliedParticle = nextParticle;
        }

        private void ChangeWeaponMassAndCost(TIShipWeaponTemplate weapon, float magazineMultiplier, int count)
        {
            mass += count * (double)weapon.buildMass_tons(magazineMultiplier);
            ChangeCost(weapon.buildCost(magazineMultiplier), count);
        }

        private void ChangeWeaponPower(TIShipWeaponTemplate weapon, float laser, float particle, int count)
        {
            if (weapon.selfPowered)
                return;
            float bonus = weapon.isLaserWeapon ? laser : (weapon.isParticleWeapon ? particle : 0f);
            bonus = bonus * (weapon.attackMode ? 1f : 0.5f) / 1000f;
            float interval = weapon.salvo_shots == 1 ? weapon.cooldown_s : weapon.intraSalvoCooldown_s;
            weaponPower += count * (double)(weapon.EnergyUsage_GJ(bonus) / interval);
            weaponHeat += count * (double)PoweredWeaponRadiatorHeatPatch.WeaponHeat_GW(weapon, bonus);
        }

        private void ChangeCost(TIResourcesCost cost, int count)
        {
            if (cost?.resourceCosts == null)
                return;
            foreach (var resource in cost.resourceCosts)
            {
                costs.TryGetValue(resource.resource, out double value);
                costs[resource.resource] = value + count * (double)resource.value;
            }
        }

        internal void AppendCost(TIResourcesCost total)
        {
            foreach (var item in costs)
                total.AddCost(item.Key, (float)item.Value);
        }

        internal int Crew(TISpaceShipTemplate ship)
        {
            return hull.simpleHull ? hull.crew : hull.crew + crew + (ship.driveTemplate?.crew ?? 0) +
                (ship.powerPlantTemplate?.crew ?? 0) + (ship.radiatorTemplate?.crew ?? 0);
        }

        internal float SystemsPower(TISpaceShipTemplate ship)
        {
            return (Crew(ship) * 5e-6f + hull.consTier * 0.005f + (float)utilityPower) * 1.1f;
        }

    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.dryMass_tons))]
    internal static class DesignerPartMassDeltaPatch
    {
        private static readonly AccessTools.FieldRef<TISpaceShipTemplate, float> cachedMass =
            AccessTools.FieldRefAccess<TISpaceShipTemplate, float>("cachedDryMass_tons");

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var code = new List<CodeInstruction>(instructions);
            var magazine = AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.magazineModuleMultiplier));
            var noseArmor = AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.noseArmorMass_tons));
            int start = code.FindIndex(i => i.Calls(magazine)) - 1;
            int end = code.FindIndex(i => i.Calls(noseArmor)) - 4;
            var apply = AccessTools.Method(typeof(DesignerPartMassDeltaPatch), nameof(AppendPartMass));
            return DesignerPartCostDeltaPatch.InsertAggregateBranch(code, start, end, generator,
                new CodeInstruction(OpCodes.Call, apply));
        }

        private static bool AppendPartMass(TISpaceShipTemplate ship)
        {
            var edit = DesignerPartEdit.ForPublication(ship);
            if (edit == null)
                return false;
            cachedMass(ship) += edit.Totals.PartMass;
            return true;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.spaceResourceConstructionCost))]
    internal static class DesignerPartCostDeltaPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator, MethodBase original)
        {
            var code = new List<CodeInstruction>(instructions);
            var weapons = AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.allWeaponTemplates));
            var noseArmor = AccessTools.Field(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.noseArmor));
            if (original.GetMethodBody().LocalVariables[0].LocalType != typeof(TIResourcesCost))
                throw new InvalidOperationException("Designer cost aggregate local changed.");
            int start = code.FindIndex(i => i.Calls(weapons)) - 1;
            int end = code.FindIndex(i => i.opcode == OpCodes.Ldflda && Equals(i.operand, noseArmor)) - 1;
            var apply = AccessTools.Method(typeof(DesignerPartCostDeltaPatch), nameof(AppendPartCost));
            return InsertAggregateBranch(code, start, end, generator,
                new CodeInstruction(OpCodes.Ldloc_0), new CodeInstruction(OpCodes.Call, apply));
        }

        internal static IEnumerable<CodeInstruction> InsertAggregateBranch(List<CodeInstruction> code,
            int start, int end, ILGenerator generator, params CodeInstruction[] append)
        {
            if (start < 0 || end <= start || code[start].opcode != OpCodes.Ldarg_0 ||
                code[end].opcode != OpCodes.Ldarg_0)
                throw new InvalidOperationException("Designer contribution loop boundaries changed.");
            Label skip = generator.DefineLabel();
            code[end].labels.Add(skip);
            var load = new CodeInstruction(OpCodes.Ldarg_0);
            load.labels.AddRange(code[start].labels);
            code[start].labels.Clear();
            code.Insert(start++, load);
            code.InsertRange(start, append);
            code.Insert(start + append.Length, new CodeInstruction(OpCodes.Brtrue, skip));
            return code;
        }

        private static bool AppendPartCost(TISpaceShipTemplate ship, TIResourcesCost cost)
        {
            var edit = DesignerPartEdit.ForPublication(ship);
            if (edit == null)
                return false;
            edit.Totals.AppendCost(cost);
            return true;
        }
    }
}
