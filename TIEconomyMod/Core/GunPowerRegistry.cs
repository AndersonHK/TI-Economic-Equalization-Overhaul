using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class GunPowerRegistry
    {
        private const string TemplateFileName = "TIGunTemplate";
        private const string PowerFieldName = "powerUse_MJ";

        private sealed class RegistrySnapshot
        {
            public readonly Dictionary<string, float> PowerByKey;
            public readonly Dictionary<TIGunTemplate, float> PowerByTemplate;

            public RegistrySnapshot(
                Dictionary<string, float> powerByKey,
                Dictionary<TIGunTemplate, float> powerByTemplate)
            {
                PowerByKey = powerByKey;
                PowerByTemplate = powerByTemplate;
            }
        }

        private static RegistrySnapshot snapshot = new RegistrySnapshot(
            new Dictionary<string, float>(StringComparer.Ordinal),
            new Dictionary<TIGunTemplate, float>(
                ReferenceIdentityComparer<TIGunTemplate>.Instance));

        public static void Refresh()
        {
            Dictionary<string, float> powerByKey =
                TemplateFloatExtensionReader.Read(
                    TemplateFileName, PowerFieldName);

            Dictionary<TIGunTemplate, float> powerByTemplate =
                new Dictionary<TIGunTemplate, float>(
                    ReferenceIdentityComparer<TIGunTemplate>.Instance);
            foreach (TIGunTemplate template in
                TemplateManager.GetAllTemplates<TIGunTemplate>())
            {
                float powerUse_MJ;
                if (TemplateFloatExtensionReader.TryGet(
                    powerByKey,
                    template.dataName,
                    template.scenarioTags,
                    out powerUse_MJ))
                {
                    powerByTemplate[template] = powerUse_MJ;
                }
            }

            snapshot = new RegistrySnapshot(powerByKey, powerByTemplate);
            Main.Log("Bound generic power data for " + powerByTemplate.Count +
                " gun template instance(s) from " + powerByKey.Count +
                " record(s).");
        }

        public static bool TryGetPowerUse_MJ(
            TIGunTemplate template, out float powerUse_MJ)
        {
            if (template == null)
            {
                powerUse_MJ = 0f;
                return false;
            }

            RegistrySnapshot current = snapshot;
            if (current.PowerByTemplate.TryGetValue(
                template, out powerUse_MJ))
            {
                return powerUse_MJ > 0f;
            }

            // Templates are normally bound once by reference during Refresh.
            // Retain the scenario-aware key path for an unexpected dynamically
            // constructed template without charging allocations to hot getters.
            return TemplateFloatExtensionReader.TryGet(
                current.PowerByKey,
                template.dataName,
                template.scenarioTags,
                out powerUse_MJ);
        }
    }
}
