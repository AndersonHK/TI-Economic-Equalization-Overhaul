using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.UI;
using PavonisInteractive.TerraInvicta.UI.Canvas_Prefabs.FleetsScreen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    internal sealed class DesignerPartEdit
    {
        [ThreadStatic]
        private static DesignerPartEdit active;

        private static readonly Action<FleetsScreenController> filterCatalog =
            AccessTools.MethodDelegate<Action<FleetsScreenController>>(
                AccessTools.Method(typeof(FleetsScreenController), "FilterAvailableShipModules"));
        private static readonly AccessTools.FieldRef<TISpaceShipTemplate, float> heatCapacity =
            AccessTools.FieldRefAccess<TISpaceShipTemplate, float>("_heatCapacity_GJ");
        private static readonly AccessTools.FieldRef<TISpaceShipTemplate, float> batteryCapacity =
            AccessTools.FieldRefAccess<TISpaceShipTemplate, float>("_batteryCapacity_GJ");

        private readonly FleetsScreenController controller;
        private readonly TISpaceShipTemplate design;
        private List<int> groupingChanges;
        private bool fullCatalog;
        private bool batteriesChanged;
        private bool lasersChanged;
        private bool particleBeamsChanged;
        private bool imageChanged;
        private readonly bool removalOnly;
        private bool publishing;
        private bool published;

        internal DesignerPartTotals Totals { get; }
        internal bool FuelComputed;
        internal bool HasFuelCapacity;
        internal FuelCapacitySnapshot FuelCapacity;
        internal bool MassComputed;
        internal readonly Dictionary<MethodBase, object> Values =
            new Dictionary<MethodBase, object>(16);

        private DesignerPartEdit(FleetsScreenController controller, bool removalOnly)
        {
            this.controller = controller;
            this.removalOnly = removalOnly;
            design = controller.newShipTemplate;
            Totals = DesignerPartTotals.ForEdit(design);
        }

        internal static bool Enabled => Main.settings != null &&
            Main.FeatureEnabled(Main.settings.shipBalance.enabled);

        internal static DesignerPartEdit Begin(FleetsScreenController controller, bool removalOnly)
        {
            if (active != null || !Enabled || controller.newShipTemplate == null)
                return null;

            active = new DesignerPartEdit(controller, removalOnly);
            return active;
        }

        internal static DesignerPartEdit ForPublication(TISpaceShipTemplate ship)
        {
            DesignerPartEdit edit = active;
            return edit != null && edit.publishing && ReferenceEquals(edit.design, ship)
                ? edit : null;
        }

        // The verified branch points are after all native writes to the template and slot.
        // The outer postfix publishes the finished design, including footprint postfixes.
        internal static bool OwnsPublication(FleetsScreenController controller)
        {
            return active != null && ReferenceEquals(active.controller, controller);
        }

        internal static void RecordEntry(List<ModuleDataTemplateEntry> entries, ModuleDataTemplateEntry entry, int count)
        {
            if (active != null && !active.publishing)
                active.Totals.Apply(active.design, entries, entry, count);
        }

        internal static void RecordPart(FleetsScreenController controller, TIShipPartTemplate part, bool removing)
        {
            if (!OwnsPublication(controller) || part == null)
                return;

            DesignerPartEdit edit = active;
            edit.imageChanged |= part.hasModel || part.isDrive || part.isPowerPlant ||
                (removing && part.isWeapon);
            edit.batteriesChanged |= part.isBattery;
            edit.fullCatalog |= part.isDrive || part.isPowerPlant;
            TIShipWeaponTemplate weapon = part.ref_weapon;
            if (weapon != null)
            {
                edit.lasersChanged |= weapon.isLaserWeapon;
                edit.particleBeamsChanged |= weapon.isParticleWeapon;
            }
            TIUtilityModuleTemplate utility = part.ref_utilityModule;
            if (utility == null)
                return;

            if (utility.grouping != -1)
            {
                if (edit.groupingChanges == null)
                    edit.groupingChanges = new List<int>(2);
                if (!edit.groupingChanges.Contains(utility.grouping))
                    edit.groupingChanges.Add(utility.grouping);
            }
            edit.fullCatalog |= utility.requiresFusionDrive || utility.requiresFissionDrive ||
                utility.requiresNuclearDrive || utility.requiresHydrogenPropellant ||
                utility.requiresNonISRUDrive ||
                utility.specialModuleRules.Contains(SpecialModuleRule.RefuelFromUnimprovedSites);
        }

        internal void Publish()
        {
            publishing = true;
            try
            {
                Totals.ApplyModifiers();
                // Fuel must be final before updating any cached cost or performance value.
                if (HullFuelCapacityFeature.Enforce(design))
                    controller.changesMadeToExistingClass = true;
                design.dryMass_tons(true);
                design.spaceResourceConstructionCost(true, null);
                heatCapacity(design) = Totals.HeatCapacity;
                batteryCapacity(design) = Totals.BatteryCapacity;
                if (removalOnly)
                    controller.UpdateModuleDataPanel(false, null, true);
                if (fullCatalog)
                    filterCatalog(controller);
                else if (groupingChanges != null || batteriesChanged || lasersChanged || particleBeamsChanged)
                    DesignerCatalogRows.Refresh(controller, groupingChanges,
                        batteriesChanged, lasersChanged, particleBeamsChanged);

                controller.UpdateShipDesignDataPanelAndImage(imageChanged, false);
                controller.designerSaveDesignButton.interactable = controller.CanSaveCurrentDesign;
                controller.UpdateTransferInfo();
                published = true;
            }
            finally
            {
                Dispose();
            }
        }

        internal void Dispose()
        {
            if (!ReferenceEquals(active, this))
                return;
            if (!published)
                DesignerPartTotals.Invalidate(design);
            active = null;
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), nameof(FleetsScreenController.SetModuleInSlot))]
    internal static class DesignerInstallPartPatch
    {
        [HarmonyPrefix]
        private static void Prefix(FleetsScreenController __instance, TIShipPartTemplate module,
            ShipModuleDragDestination dropDestination, bool updateModelAndDropdowns,
            bool ___loadingExistingTemplate, out DesignerPartEdit __state)
        {
            __state = null;
            if (module == null || dropDestination == null || dropDestination.currentPart == module)
                return;
            if (___loadingExistingTemplate || !updateModelAndDropdowns || !DesignerPartEdit.Enabled)
            {
                DesignerPartTotals.Invalidate(__instance.newShipTemplate);
                return;
            }

            __state = DesignerPartEdit.Begin(__instance, removalOnly: false);
            DesignerPartEdit.RecordPart(__instance, module, removing: false);
        }

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(DesignerPartEdit __state)
        {
            __state?.Publish();
        }

        [HarmonyFinalizer]
        private static void Finalizer(DesignerPartEdit __state)
        {
            __state?.Dispose();
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var code = new List<CodeInstruction>(instructions);
            DesignerPartTotals.TrackMutations(code, installing: true);
            var saveButton = AccessTools.Field(typeof(FleetsScreenController), "designerSaveDesignButton");
            int index = code.FindIndex(instruction => instruction.LoadsField(saveButton));
            if (index < 1 || code[index - 1].opcode != OpCodes.Ldarg_0)
                throw new InvalidOperationException("Designer install publication boundary changed.");
            return DesignerRemovalPartPatch.SkipNativePublication(code, index - 1, generator);
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), nameof(FleetsScreenController.RemoveModuleFromSlot))]
    internal static class DesignerRemovalPartPatch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        private static bool Prefix(FleetsScreenController __instance, Vector2Int coordinates,
            bool suppressSCVUpdate, bool ___loadingExistingTemplate,
            Dictionary<Vector2Int, ShipModuleDragDestination> ___shipModuleSlotDictionary,
            out DesignerPartEdit __state)
        {
            __state = null;
            if (___loadingExistingTemplate || !DesignerPartEdit.Enabled ||
                (suppressSCVUpdate && !DesignerPartEdit.OwnsPublication(__instance)) ||
                ___shipModuleSlotDictionary == null || __instance.newShipTemplate == null ||
                !___shipModuleSlotDictionary.TryGetValue(coordinates, out var slot))
            {
                if (!DesignerPartEdit.OwnsPublication(__instance))
                    DesignerPartTotals.Invalidate(__instance.newShipTemplate);
                return true;
            }

            if (slot.currentPart == null && !slot.blocked)
                return false;

            __state = DesignerPartEdit.Begin(__instance, removalOnly: true);
            DesignerPartEdit.RecordPart(__instance, slot.currentPart, removing: true);
            return true;
        }

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(DesignerPartEdit __state)
        {
            __state?.Publish();
        }

        [HarmonyFinalizer]
        private static void Finalizer(DesignerPartEdit __state)
        {
            __state?.Dispose();
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var code = new List<CodeInstruction>(instructions);
            DesignerPartTotals.TrackMutations(code, installing: false);
            var setEmpty = AccessTools.Method(typeof(ShipModuleDragDestination), nameof(ShipModuleDragDestination.SetEmpty));
            int index = code.FindLastIndex(instruction => instruction.Calls(setEmpty));
            if (index < 0 || index + 1 >= code.Count || code[index + 1].opcode != OpCodes.Ldarg_0)
                throw new InvalidOperationException("Designer removal publication boundary changed.");
            return SkipNativePublication(code, index + 1, generator);
        }

        internal static IEnumerable<CodeInstruction> SkipNativePublication(List<CodeInstruction> code,
            int index, ILGenerator generator)
        {
            var check = new CodeInstruction(OpCodes.Ldarg_0);
            check.labels.AddRange(code[index].labels);
            code[index].labels.Clear();
            Label native = generator.DefineLabel();
            code[index].labels.Add(native);
            code.InsertRange(index, new[]
            {
                check,
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DesignerPartEdit),
                    nameof(DesignerPartEdit.OwnsPublication))),
                new CodeInstruction(OpCodes.Brfalse, native),
                new CodeInstruction(OpCodes.Ret)
            });
            return code;
        }
    }

    internal sealed class DesignerCatalogRows
    {
        private static readonly ConditionalWeakTable<FleetsScreenController, DesignerCatalogRows> byController =
            new ConditionalWeakTable<FleetsScreenController, DesignerCatalogRows>();
        private static readonly AccessTools.FieldRef<FleetsScreenController, List<ShipModuleListItem>> icons =
            AccessTools.FieldRefAccess<FleetsScreenController, List<ShipModuleListItem>>("shipModuleListItems");
        private static readonly AccessTools.FieldRef<FleetsScreenController, List<ShipModuleListItem>> comparisons =
            AccessTools.FieldRefAccess<FleetsScreenController, List<ShipModuleListItem>>("shipModuleListItemsB");

        private struct Row
        {
            internal ShipModuleListItem Item;
            internal TIShipPartTemplate Part;
            internal bool Comparison;
        }

        private readonly Dictionary<int, List<Row>> byGroup = new Dictionary<int, List<Row>>();
        private readonly List<Row> batteries = new List<Row>();
        private readonly List<Row> lasers = new List<Row>();
        private readonly List<Row> particleBeams = new List<Row>();
        private List<ShipModuleListItem> iconItems;
        private List<ShipModuleListItem> comparisonItems;
        private int iconCount;
        private int comparisonCount;

        internal static void Refresh(FleetsScreenController controller, List<int> groups,
            bool batteriesChanged, bool lasersChanged, bool particlesChanged)
        {
            var rows = GetIndex(controller);
            var ship = controller.newShipTemplate;
            if (groups != null)
            {
                foreach (int group in groups)
                    if (rows.byGroup.TryGetValue(group, out var matching))
                        RefreshRows(ship, matching);
            }
            if (batteriesChanged) RefreshRows(ship, rows.batteries);
            if (lasersChanged) RefreshRows(ship, rows.lasers);
            if (particlesChanged) RefreshRows(ship, rows.particleBeams);
        }

        internal static void Prime(FleetsScreenController controller)
        {
            GetIndex(controller);
        }

        private static DesignerCatalogRows GetIndex(FleetsScreenController controller)
        {
            var rows = byController.GetValue(controller, BuildIndex);
            if (rows.IsCurrent(controller))
                return rows;

            byController.Remove(controller);
            return byController.GetValue(controller, BuildIndex);
        }

        private bool IsCurrent(FleetsScreenController owner)
        {
            return iconItems == icons(owner) && comparisonItems == comparisons(owner) &&
                iconCount == iconItems.Count && comparisonCount == comparisonItems.Count;
        }

        private static DesignerCatalogRows BuildIndex(FleetsScreenController owner)
        {
            var index = new DesignerCatalogRows();
            index.iconItems = icons(owner);
            index.comparisonItems = comparisons(owner);
            index.iconCount = index.iconItems.Count;
            index.comparisonCount = index.comparisonItems.Count;
            index.Add(index.iconItems, comparison: false);
            index.Add(index.comparisonItems, comparison: true);
            return index;
        }

        private void Add(List<ShipModuleListItem> items, bool comparison)
        {
            foreach (ShipModuleListItem item in items)
            {
                TIShipPartTemplate part = item.GetModuleTemplate();
                if (part == null)
                    continue;
                var row = new Row { Item = item, Part = part, Comparison = comparison };
                if (part.isBattery) batteries.Add(row);
                TIUtilityModuleTemplate utility = part.ref_utilityModule;
                if (utility == null)
                    continue;

                if (utility.laserPowerBonus_MW > 0f) lasers.Add(row);
                if (utility.particleBeamPowerBonus_MW > 0f) particleBeams.Add(row);
                if (utility.grouping == -1)
                    continue;
                if (!byGroup.TryGetValue(utility.grouping, out var rows))
                {
                    rows = new List<Row>();
                    byGroup.Add(utility.grouping, rows);
                }
                rows.Add(row);
            }
        }

        private static void RefreshRows(TISpaceShipTemplate ship, List<Row> rows)
        {
            foreach (Row row in rows)
            {
                if (!row.Item.gameObject.activeSelf)
                    continue;
                bool legal = ship.ValidPartForDesign(row.Part);
                if (row.Comparison)
                {
                    if (row.Item.addModuleButton.interactable == legal)
                        continue;
                    row.Item.addModuleButton.interactable = legal;
                }
                else
                {
                    if (row.Item.draggable == legal)
                        continue;
                    row.Item.draggable = legal;
                }
                row.Item.SetAlpha(legal);
            }
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), "FilterAvailableShipModules")]
    internal static class DesignerCatalogPass
    {
        [ThreadStatic]
        private static DesignerCatalogPassState active;

        internal sealed class DesignerCatalogPassState
        {
            internal TISpaceShipTemplate Ship;
            internal List<TIDriveTemplate> Drives;
            internal List<TIPowerPlantTemplate> Plants;
        }

        [HarmonyPrefix]
        private static void Prefix(FleetsScreenController __instance, bool ___loadingExistingTemplate,
            out DesignerCatalogPassState __state)
        {
            __state = active;
            if (DesignerPartEdit.Enabled && !___loadingExistingTemplate)
                active = new DesignerCatalogPassState { Ship = __instance.newShipTemplate };
        }

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(FleetsScreenController __instance, bool ___loadingExistingTemplate)
        {
            if (DesignerPartEdit.Enabled && !___loadingExistingTemplate && __instance.newShipTemplate != null)
                DesignerCatalogRows.Prime(__instance);
        }

        [HarmonyFinalizer]
        private static void Finalizer(DesignerCatalogPassState __state)
        {
            active = __state;
        }

        internal static DesignerCatalogPassState ForDesign(TISpaceShipTemplate ship)
        {
            return active != null && ReferenceEquals(active.Ship, ship) ? active : null;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.validDrivesForPowerPlant), MethodType.Getter)]
    internal static class DesignerCatalogDriveCompatibility
    {
        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, ref List<TIDriveTemplate> __result,
            out DesignerCatalogPass.DesignerCatalogPassState __state)
        {
            __state = DesignerCatalogPass.ForDesign(__instance);
            __result = __state?.Drives;
            return __result == null;
        }

        [HarmonyPostfix]
        private static void Postfix(List<TIDriveTemplate> __result,
            DesignerCatalogPass.DesignerCatalogPassState __state)
        {
            if (__state != null)
                __state.Drives = __result;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.validPowerPlantsForDrive), MethodType.Getter)]
    internal static class DesignerCatalogReactorCompatibility
    {
        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, ref List<TIPowerPlantTemplate> __result,
            out DesignerCatalogPass.DesignerCatalogPassState __state)
        {
            __state = DesignerCatalogPass.ForDesign(__instance);
            __result = __state?.Plants;
            return __result == null;
        }

        [HarmonyPostfix]
        private static void Postfix(List<TIPowerPlantTemplate> __result,
            DesignerCatalogPass.DesignerCatalogPassState __state)
        {
            if (__state != null)
                __state.Plants = __result;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.dryMass_tons))]
    internal static class DesignerDryMassReuse
    {
        [HarmonyPrefix]
        private static void Prefix(TISpaceShipTemplate __instance, ref bool forceUpdate,
            out DesignerPartEdit __state)
        {
            __state = DesignerPartEdit.ForPublication(__instance);
            if (__state != null && __state.MassComputed)
                forceUpdate = false;
        }

        [HarmonyPostfix]
        private static void Postfix(DesignerPartEdit __state)
        {
            if (__state != null)
                __state.MassComputed = true;
        }
    }

    internal static class DesignerPublicationValues
    {
        internal static bool Read<T>(TISpaceShipTemplate ship, MethodBase method, ref T result,
            out DesignerPartEdit edit)
        {
            edit = DesignerPartEdit.ForPublication(ship);
            if (edit == null || !edit.Values.TryGetValue(method, out object cached))
                return true;

            result = (T)cached;
            edit = null;
            return false;
        }

        internal static void Record<T>(DesignerPartEdit edit, MethodBase method, T result)
        {
            if (edit != null)
                edit.Values[method] = result;
        }
    }

    [HarmonyPatch]
    internal static class DesignerPartEntryReuse
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.utilityModules));
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.noseWeapons));
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.hullWeapons));
        }

        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, MethodBase __originalMethod,
            ref IEnumerable<ModuleDataEntry> __result, out DesignerPartEdit __state)
        {
            return DesignerPublicationValues.Read(__instance, __originalMethod, ref __result, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(MethodBase __originalMethod, IEnumerable<ModuleDataEntry> __result,
            DesignerPartEdit __state)
        {
            DesignerPublicationValues.Record(__state, __originalMethod, __result);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.utilitySlotModuleTemplates), MethodType.Getter)]
    internal static class DesignerUtilityListReuse
    {
        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, MethodBase __originalMethod,
            ref IEnumerable<TIShipModuleTemplate> __result, out DesignerPartEdit __state)
        {
            return DesignerPublicationValues.Read(__instance, __originalMethod, ref __result, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(MethodBase __originalMethod, IEnumerable<TIShipModuleTemplate> __result,
            DesignerPartEdit __state)
        {
            DesignerPublicationValues.Record(__state, __originalMethod, __result);
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.allWeaponTemplates), MethodType.Getter)]
    internal static class DesignerWeaponListReuse
    {
        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, MethodBase __originalMethod,
            ref List<TIShipWeaponTemplate> __result, out DesignerPartEdit __state)
        {
            return DesignerPublicationValues.Read(__instance, __originalMethod, ref __result, out __state);
        }

        [HarmonyPostfix]
        private static void Postfix(MethodBase __originalMethod, List<TIShipWeaponTemplate> __result,
            DesignerPartEdit __state)
        {
            DesignerPublicationValues.Record(__state, __originalMethod, __result);
        }
    }

    [HarmonyPatch]
    internal static class DesignerPowerTotalsReuse
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.magazineModuleMultiplier));
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.requiredSystemsPower_GW));
            yield return AccessTools.PropertyGetter(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.requiredWeaponsPowerGeneration_GW));
        }

        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, MethodBase __originalMethod,
            ref float __result)
        {
            var edit = DesignerPartEdit.ForPublication(__instance);
            if (edit == null)
                return true;
            if (__originalMethod.Name == "get_requiredSystemsPower_GW")
                __result = edit.Totals.SystemsPower(__instance);
            else if (__originalMethod.Name == "get_requiredWeaponsPowerGeneration_GW")
                __result = edit.Totals.WeaponPower;
            else
                __result = edit.Totals.Magazine;
            return false;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.crewBillets), MethodType.Getter)]
    internal static class DesignerCrewDeltaPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, ref int __result)
        {
            var edit = DesignerPartEdit.ForPublication(__instance);
            if (edit == null)
                return true;
            __result = edit.Totals.Crew(__instance);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class DesignerBeamBonusReuse
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.GetLaserBonusPower_MJ));
            yield return AccessTools.Method(typeof(TISpaceShipTemplate), nameof(TISpaceShipTemplate.GetParticleBonusPower_MJ));
        }

        [HarmonyPrefix]
        private static bool Prefix(TISpaceShipTemplate __instance, MethodBase __originalMethod,
            Func<ModuleDataEntry, float> GetPartFunction, ref float __result)
        {
            var edit = DesignerPartEdit.ForPublication(__instance);
            if (edit == null || GetPartFunction != null)
                return true;
            __result = __originalMethod.Name == nameof(TISpaceShipTemplate.GetLaserBonusPower_MJ)
                ? edit.Totals.LaserBonus : edit.Totals.ParticleBonus;
            return false;
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), nameof(FleetsScreenController.UpdateTransferPlannerParameters))]
    internal static class DesignerTransferParametersPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(FleetsScreenController __instance)
        {
            if (!DesignerPartEdit.Enabled || __instance.newShipTemplate == null)
                return true;

            TISpaceShipTemplate ship = __instance.newShipTemplate;
            var planner = IntelScreenController.Singleton.TransferPlanner;
            string acceleration = (ship.baseCruiseAcceleration_gs(true) * 1000f).ToString("N1");
            string deltaV = ship.baseCruiseDeltaV_kps(true).ToString("N1");
            bool changed = acceleration != planner.accelerationInputField.text ||
                deltaV != planner.dvInputField.text;
            if (changed)
            {
                planner.accelerationInputField.SetTextWithoutNotify(acceleration);
                planner.dvInputField.SetTextWithoutNotify(deltaV);
                planner.UpdateThrustProfile();
            }
            return false;
        }
    }

}
