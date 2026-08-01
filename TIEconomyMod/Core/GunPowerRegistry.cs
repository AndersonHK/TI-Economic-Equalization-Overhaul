using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class GunPowerRegistry
    {
        private const string TemplateFileName = "TIGunTemplate";
        private const string PowerFieldName = "powerUse_MJ";

        private static Dictionary<string, float> powerByTemplate =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public static void Refresh()
        {
            Dictionary<string, float> next =
                TemplateFloatExtensionReader.Read(
                    TemplateFileName, PowerFieldName);

            powerByTemplate = next;
            Main.Log("Bound generic power data for " + next.Count +
                " gun template record(s).");
        }

        public static bool TryGetPowerUse_MJ(
            TIGunTemplate template, out float powerUse_MJ)
        {
            Dictionary<string, float> snapshot = powerByTemplate;
            return TemplateFloatExtensionReader.TryGet(
                snapshot,
                template.dataName,
                template.scenarioTags,
                out powerUse_MJ);
        }
    }
}
