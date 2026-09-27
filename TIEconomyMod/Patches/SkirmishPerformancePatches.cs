using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using TIEconomyMod.Core;
using TMPro;

namespace TIEconomyMod.Patches
{
    internal static class SkirmishDropdownCacheRuntime
    {
        private sealed class CachedCatalog
        {
            public readonly List<TMP_Dropdown.OptionData> Options;
            public readonly Dictionary<string, TMP_Dropdown.OptionData>
                OptionByClassName;
            public readonly TMP_Dropdown.OptionData LastImportedOption;

            public CachedCatalog(
                List<TMP_Dropdown.OptionData> options,
                Dictionary<string, TMP_Dropdown.OptionData>
                    optionByClassName,
                TMP_Dropdown.OptionData lastImportedOption)
            {
                Options = options;
                OptionByClassName = optionByClassName;
                LastImportedOption = lastImportedOption;
            }
        }

        private sealed class RowState
        {
            public CachedCatalog Catalog;
        }

        private static readonly ConditionalWeakTable<
            StartMenuController,
            ReferenceContextVariantCache<CachedCatalog>> controllerCaches =
                new ConditionalWeakTable<
                    StartMenuController,
                    ReferenceContextVariantCache<CachedCatalog>>();

        private static readonly ConditionalWeakTable<
            SkirmishShipListItemController,
            RowState> rowStates =
                new ConditionalWeakTable<
                    SkirmishShipListItemController,
                    RowState>();

        private static readonly FieldInfo FactionsField = AccessTools.Field(
            typeof(SkirmishShipListItemController), "factions");

        private static readonly AccessTools.FieldRef<TISpaceShipTemplate, TIFactionState>
            DesigningFaction = AccessTools.FieldRefAccess<TISpaceShipTemplate, TIFactionState>("_designingFaction");

        internal static void PrepareMenuDesigns(IEnumerable<TISpaceShipTemplate> designs)
        {
            // Imported templates survive a completed skirmish, unlike its game
            // states. Their private faction cache must not outlive that scene.
            if (GameStateManager.HasGamestates || designs == null) return;
            foreach (TISpaceShipTemplate design in designs)
            {
                if (design == null || DesigningFaction(design) == null) continue;
                DesigningFaction(design) = null;
                CapitalMountRuntime.Invalidate(design);
            }
        }

        private static readonly Action<SkirmishShipListItemController>
            SetTooltipDelegate = AccessTools.MethodDelegate<
                Action<SkirmishShipListItemController>>(
                AccessTools.Method(
                    typeof(SkirmishShipListItemController),
                    "SetTooltipDelegate"),
                null,
                false);

        private static readonly Action<SkirmishShipListItemController>
            SetShipDamageImages = AccessTools.MethodDelegate<
                Action<SkirmishShipListItemController>>(
                AccessTools.Method(
                    typeof(SkirmishShipListItemController),
                    "SetShipDamageImages"),
                null,
                false);

        public static bool Enabled
        {
            get
            {
                return Main.FeatureEnabled(Main.settings.shipBalance.enabled);
            }
        }

        public static void Invalidate(StartMenuController controller)
        {
            if (controller == null)
            {
                return;
            }

            ReferenceContextVariantCache<CachedCatalog> cache;
            if (controllerCaches.TryGetValue(controller, out cache))
            {
                cache.Invalidate();
            }
        }

        public static void Populate(
            SkirmishShipListItemController row,
            StartMenuController controller,
            TISpaceFleetTemplate fleetTemplate,
            int shipIndex,
            int fleetIndex,
            ref bool selectImportedDesign)
        {
            List<TISpaceShipTemplate> ships = controller.ships;
            List<TISpaceShipTemplate> imported =
                controller.ImportedShipTemplates;
            bool allowAlien = fleetIndex == 1;

            ReferenceContextVariantCache<CachedCatalog> cache =
                controllerCaches.GetValue(
                    controller,
                    ignored =>
                        new ReferenceContextVariantCache<CachedCatalog>());
            CachedCatalog catalog = cache.GetOrCreate(
                ships,
                ships.Count,
                imported,
                imported.Count,
                Loc.CurrentLanguage,
                allowAlien,
                () => BuildCatalog(controller, allowAlien));

            RowState rowState = rowStates.GetValue(
                row, ignored => new RowState());
            if (!ReferenceEquals(rowState.Catalog, catalog) ||
                row.shipDropdown.options.Count != catalog.Options.Count)
            {
                row.shipDropdown.ClearOptions();
                row.shipDropdown.options.AddRange(catalog.Options);
                rowState.Catalog = catalog;
            }

            bool isAddShipButton = shipIndex == -1;
            TMP_Dropdown.OptionData selectedOption = null;
            if (selectImportedDesign)
            {
                selectedOption = catalog.LastImportedOption;
            }
            else if (!isAddShipButton)
            {
                TISpaceShipTemplate selectedTemplate =
                    TemplateManager.Find<TISpaceShipTemplate>(
                        fleetTemplate.shipsInFleet[shipIndex].shipTemplateName);
                catalog.OptionByClassName.TryGetValue(
                    selectedTemplate.fullClassName, out selectedOption);
            }

            selectImportedDesign = false;
            int selectedIndex = selectedOption == null
                ? -1
                : row.shipDropdown.options.IndexOf(selectedOption);
            if (isAddShipButton)
            {
                row.shipDropdown.SetValueWithoutNotify(selectedIndex);
            }
            else
            {
                row.shipDropdown.value = selectedIndex;
            }

            row.shipDropdown.RefreshShownValue();
            SetTooltipDelegate(row);
            SetShipDamageImages(row);
        }

        private static CachedCatalog BuildCatalog(
            StartMenuController controller, bool allowAlien)
        {
            // Clear the entire roster first: score normalization may consult
            // another template before its own catalog entry is visited.
            PrepareMenuDesigns(controller.ships);
            List<TMP_Dropdown.OptionData> options =
                new List<TMP_Dropdown.OptionData>();
            Dictionary<string, TMP_Dropdown.OptionData> optionByClassName =
                new Dictionary<string, TMP_Dropdown.OptionData>(
                    StringComparer.Ordinal);
            TMP_Dropdown.OptionData lastImportedOption = null;
            List<TISpaceShipTemplate> imported =
                controller.ImportedShipTemplates;
            Dictionary<string, TIFactionState> factions =
                (Dictionary<string, TIFactionState>)
                    FactionsField.GetValue(null);

            foreach (TISpaceShipTemplate ship in controller.ships)
            {
                if (ship.hideInSkirmish ||
                    (!allowAlien && ship.factionName == "AlienCouncil"))
                {
                    continue;
                }

                // Normalize before reading a saved positive combat score can
                // bypass the dummy-ship initialization guard.
                CapitalMountRuntime.EnsureDesign(ship);
                string text = Loc.T(
                    "UI.StartScreen.SkirmishShipDropdownLineItem",
                    ship.fullClassName,
                    TemplateManager.global.spaceCombatScoreInlineSpritePath,
                    ship.TemplateSpaceCombatValue().ToString("N0"));
                bool isImported = imported.Contains(ship);
                if (ship.isAlien)
                {
                    text = TIUtilities.PurpleLine(text);
                }
                else if (isImported)
                {
                    text = TIUtilities.FactionLine(
                        text, factions[ship.factionName]);
                }

                TMP_Dropdown.OptionData option =
                    new TMP_Dropdown.OptionData(text);
                options.Add(option);
                optionByClassName[ship.fullClassName] = option;
                if (isImported)
                {
                    lastImportedOption = option;
                }
            }

            options.Add(new TMP_Dropdown.OptionData(
                Loc.T("UI.StartScreen.Skirmish.Import")));
            return new CachedCatalog(
                options, optionByClassName, lastImportedOption);
        }
    }

    [HarmonyPatch(
        typeof(SkirmishShipListItemController), "PopulateShipDropdown")]
    public static class SkirmishShipDropdownCachePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            SkirmishShipListItemController __instance,
            StartMenuController ___masterController,
            TISpaceFleetTemplate ___fleetTemplate,
            int ___shipIndex,
            int ___fleetIdx,
            ref bool ___selectImportedDesign)
        {
            if (!SkirmishDropdownCacheRuntime.Enabled)
            {
                return true;
            }

            SkirmishDropdownCacheRuntime.Populate(
                __instance,
                ___masterController,
                ___fleetTemplate,
                ___shipIndex,
                ___fleetIdx,
                ref ___selectImportedDesign);
            return false;
        }
    }

    [HarmonyPatch(
        typeof(StartMenuController),
        "ImportedShipTemplates",
        MethodType.Setter)]
    public static class SkirmishImportedShipCacheInvalidationPatch
    {
        [HarmonyPrefix]
        public static void Prefix(List<TISpaceShipTemplate> value)
        {
            // Save import bypasses campaign and designer initialization. Do
            // this before registration/preview, not during phase-1 fleet
            // creation where effects are not ready to recalculate the score.
            if (value == null) return;
            SkirmishDropdownCacheRuntime.PrepareMenuDesigns(value);
            foreach (TISpaceShipTemplate design in value)
                CapitalMountRuntime.EnsureImportedDesign(design);
        }

        [HarmonyPostfix]
        public static void Postfix(StartMenuController __instance)
        {
            SkirmishDropdownCacheRuntime.Invalidate(__instance);
        }
    }
}
