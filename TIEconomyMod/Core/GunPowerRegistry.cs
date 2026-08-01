using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TIEconomyMod
{
    public static class GunPowerRegistry
    {
        private const string TemplateFileName = "TIGunTemplate";
        private const string PowerFieldName = "powerUse_MJ";
        private const char KeySeparator = '\u001f';

        private static Dictionary<string, float> powerByTemplate =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public static void Refresh()
        {
            Dictionary<string, float> next =
                new Dictionary<string, float>(StringComparer.Ordinal);

            foreach (JsonMod jsonMod in
                ModTemplateManager.GetModsForTemplate(TemplateFileName))
            {
                if (jsonMod.TemplatesToReplace != null &&
                    jsonMod.TemplatesToReplace.Contains(
                        TemplateFileName + ".json"))
                {
                    next.Clear();
                }

                foreach (JObject record in jsonMod.FileContents)
                {
                    JToken dataNameToken = record["dataName"];
                    if (dataNameToken == null)
                    {
                        continue;
                    }

                    JToken powerToken;
                    if (!record.TryGetValue(PowerFieldName, out powerToken))
                    {
                        continue;
                    }

                    string key = BuildKey(
                        dataNameToken.Value<string>(), ReadScenarioTags(record));
                    if (powerToken.Type == JTokenType.Null)
                    {
                        next.Remove(key);
                        continue;
                    }

                    float powerUse_MJ;
                    if (!float.TryParse(
                        powerToken.ToString(),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out powerUse_MJ) ||
                        float.IsNaN(powerUse_MJ) ||
                        float.IsInfinity(powerUse_MJ))
                    {
                        Main.Warn("Ignored invalid " + PowerFieldName +
                            " value on gun template '" + dataNameToken + "'.");
                        continue;
                    }

                    next[key] = Math.Max(0f, powerUse_MJ);
                }
            }

            powerByTemplate = next;
            Main.Log("Bound generic power data for " + next.Count +
                " gun template record(s).");
        }

        public static bool TryGetPowerUse_MJ(
            TIGunTemplate template, out float powerUse_MJ)
        {
            Dictionary<string, float> snapshot = powerByTemplate;
            if (snapshot.TryGetValue(
                BuildKey(template.dataName, template.scenarioTags),
                out powerUse_MJ))
            {
                return powerUse_MJ > 0f;
            }

            // An untagged mod row follows the game's ordinary merge behavior and
            // applies to the active scenario variant unless a tagged row overrides it.
            return snapshot.TryGetValue(
                BuildKey(template.dataName, null), out powerUse_MJ) &&
                powerUse_MJ > 0f;
        }

        private static IEnumerable<string> ReadScenarioTags(JObject record)
        {
            JToken tags = record["scenarioTags"];
            return tags == null || tags.Type == JTokenType.Null
                ? Enumerable.Empty<string>()
                : tags.Values<string>();
        }

        private static string BuildKey(
            string dataName, IEnumerable<string> scenarioTags)
        {
            IEnumerable<string> orderedTags = (scenarioTags ??
                Enumerable.Empty<string>()).OrderBy(
                    value => value, StringComparer.Ordinal);
            return (dataName ?? string.Empty) + KeySeparator +
                string.Join(KeySeparator.ToString(), orderedTags);
        }
    }
}
