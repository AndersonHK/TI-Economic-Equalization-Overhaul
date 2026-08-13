using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using TIEconomyMod.Core;

namespace TIEconomyMod
{
    public static class UtilityFootprintRegistry
    {
        private const string UtilityTemplateFileName =
            "TIUtilityModuleTemplate";
        private const string HeatSinkTemplateFileName =
            "TIHeatSinkTemplate";
        private const string FootprintFieldName = "utilityFootprint";

        private sealed class RegistrySnapshot
        {
            public readonly Dictionary<string, string> ValuesByKey;
            public readonly Dictionary<string, string> HeatSinkValuesByKey;
            public readonly Dictionary<TIUtilityModuleTemplate,
                UtilityFootprintKind> FootprintsByTemplate;
            public readonly Dictionary<TIHeatSinkTemplate,
                UtilityFootprintKind> HeatSinkFootprintsByTemplate;

            public RegistrySnapshot(
                Dictionary<string, string> valuesByKey,
                Dictionary<string, string> heatSinkValuesByKey,
                Dictionary<TIUtilityModuleTemplate,
                    UtilityFootprintKind> footprintsByTemplate,
                Dictionary<TIHeatSinkTemplate,
                    UtilityFootprintKind> heatSinkFootprintsByTemplate)
            {
                ValuesByKey = valuesByKey;
                HeatSinkValuesByKey = heatSinkValuesByKey;
                FootprintsByTemplate = footprintsByTemplate;
                HeatSinkFootprintsByTemplate =
                    heatSinkFootprintsByTemplate;
            }
        }

        private static RegistrySnapshot snapshot = new RegistrySnapshot(
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<TIUtilityModuleTemplate, UtilityFootprintKind>(
                ReferenceIdentityComparer<TIUtilityModuleTemplate>.Instance),
            new Dictionary<TIHeatSinkTemplate, UtilityFootprintKind>(
                ReferenceIdentityComparer<TIHeatSinkTemplate>.Instance));

        public static void Refresh()
        {
            Dictionary<string, string> valuesByKey =
                TemplateStringExtensionReader.Read(
                    UtilityTemplateFileName, FootprintFieldName);
            Dictionary<string, string> heatSinkValuesByKey =
                TemplateStringExtensionReader.Read(
                    HeatSinkTemplateFileName, FootprintFieldName);
            Dictionary<TIUtilityModuleTemplate, UtilityFootprintKind>
                footprintsByTemplate =
                    new Dictionary<TIUtilityModuleTemplate,
                        UtilityFootprintKind>(
                        ReferenceIdentityComparer<TIUtilityModuleTemplate>
                            .Instance);

            foreach (TIUtilityModuleTemplate template in
                TemplateManager.GetAllTemplates<TIUtilityModuleTemplate>())
            {
                string value;
                UtilityFootprintKind footprint;
                if (!TemplateStringExtensionReader.TryGet(
                        valuesByKey,
                        template.dataName,
                        template.scenarioTags,
                        out value) ||
                    !TryParse(value, out footprint))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Main.Warn("Ignored invalid utility footprint '" +
                            value + "' on template '" + template.dataName +
                            "'.");
                    }
                    continue;
                }

                if (footprint != UtilityFootprintKind.Single)
                {
                    footprintsByTemplate[template] = footprint;
                }
            }

            Dictionary<TIHeatSinkTemplate, UtilityFootprintKind>
                heatSinkFootprintsByTemplate =
                    new Dictionary<TIHeatSinkTemplate,
                        UtilityFootprintKind>(
                        ReferenceIdentityComparer<TIHeatSinkTemplate>
                            .Instance);
            foreach (TIHeatSinkTemplate template in
                TemplateManager.GetAllTemplates<TIHeatSinkTemplate>())
            {
                string value;
                UtilityFootprintKind footprint;
                if (!TemplateStringExtensionReader.TryGet(
                        heatSinkValuesByKey,
                        template.dataName,
                        template.scenarioTags,
                        out value) ||
                    !TryParse(value, out footprint))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Main.Warn("Ignored invalid heat-sink footprint '" +
                            value + "' on template '" + template.dataName +
                            "'.");
                    }
                    continue;
                }

                if (footprint != UtilityFootprintKind.Single)
                {
                    heatSinkFootprintsByTemplate[template] = footprint;
                }
            }

            snapshot = new RegistrySnapshot(
                valuesByKey,
                heatSinkValuesByKey,
                footprintsByTemplate,
                heatSinkFootprintsByTemplate);
            Main.Log("Bound multi-slot footprints for " +
                footprintsByTemplate.Count + " utility and " +
                heatSinkFootprintsByTemplate.Count +
                " heat-sink template instance(s).");
        }

        public static UtilityFootprintKind GetFootprint(
            TIHeatSinkTemplate template)
        {
            if (template == null || !Enabled)
            {
                return UtilityFootprintKind.Single;
            }

            RegistrySnapshot current = snapshot;
            UtilityFootprintKind footprint;
            if (current.HeatSinkFootprintsByTemplate.TryGetValue(
                template, out footprint))
            {
                return footprint;
            }

            string value;
            return TemplateStringExtensionReader.TryGet(
                    current.HeatSinkValuesByKey,
                    template.dataName,
                    template.scenarioTags,
                    out value) &&
                TryParse(value, out footprint)
                    ? footprint
                    : UtilityFootprintKind.Single;
        }

        public static UtilityFootprintKind GetFootprint(
            TIUtilityModuleTemplate template)
        {
            if (template == null || !Enabled)
            {
                return UtilityFootprintKind.Single;
            }

            RegistrySnapshot current = snapshot;
            UtilityFootprintKind footprint;
            if (current.FootprintsByTemplate.TryGetValue(
                template, out footprint))
            {
                return footprint;
            }

            string value;
            return TemplateStringExtensionReader.TryGet(
                    current.ValuesByKey,
                    template.dataName,
                    template.scenarioTags,
                    out value) &&
                TryParse(value, out footprint)
                    ? footprint
                    : UtilityFootprintKind.Single;
        }

        public static UtilityFootprintKind GetFootprint(
            TIShipPartTemplate template)
        {
            if (template == null)
            {
                return UtilityFootprintKind.Single;
            }

            TIUtilityModuleTemplate utility = template.ref_utilityModule;
            if (utility != null)
            {
                return GetFootprint(utility);
            }

            return GetFootprint(template.ref_heatSink);
        }

        public static bool Enabled
        {
            get
            {
                return Main.enabled && Main.settings != null &&
                    Main.settings.enabled &&
                    Main.settings.shipBalance != null &&
                    Main.settings.shipBalance.enabled &&
                    Main.settings.shipBalance.multiSlotUtilitiesEnabled;
            }
        }

        private static bool TryParse(
            string value,
            out UtilityFootprintKind footprint)
        {
            return Enum.TryParse(value, true, out footprint) &&
                Enum.IsDefined(typeof(UtilityFootprintKind), footprint);
        }
    }
}
