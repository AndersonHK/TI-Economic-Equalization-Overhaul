using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TIEconomyMod
{
    internal static class TemplateFloatExtensionReader
    {
        private const char KeySeparator = '\u001f';

        public static Dictionary<string, float> Read(
            string templateFileName, string fieldName)
        {
            Dictionary<string, float> values =
                new Dictionary<string, float>(StringComparer.Ordinal);

            foreach (JsonMod jsonMod in
                ModTemplateManager.GetModsForTemplate(templateFileName))
            {
                if (jsonMod.TemplatesToReplace != null &&
                    jsonMod.TemplatesToReplace.Contains(
                        templateFileName + ".json"))
                {
                    values.Clear();
                }

                foreach (JObject record in jsonMod.FileContents)
                {
                    JToken dataNameToken = record["dataName"];
                    if (dataNameToken == null)
                    {
                        continue;
                    }

                    JToken valueToken;
                    if (!record.TryGetValue(fieldName, out valueToken))
                    {
                        continue;
                    }

                    string key = BuildKey(
                        dataNameToken.Value<string>(), ReadScenarioTags(record));
                    if (valueToken.Type == JTokenType.Null)
                    {
                        values.Remove(key);
                        continue;
                    }

                    float value;
                    if (!float.TryParse(
                        valueToken.ToString(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out value) ||
                        float.IsNaN(value) ||
                        float.IsInfinity(value))
                    {
                        Main.Warn("Ignored invalid " + fieldName +
                            " value on template '" + dataNameToken + "'.");
                        continue;
                    }

                    values[key] = Math.Max(0f, value);
                }
            }

            return values;
        }

        public static bool TryGet(
            Dictionary<string, float> values,
            string dataName,
            IEnumerable<string> scenarioTags,
            out float value)
        {
            if (values.TryGetValue(
                BuildKey(dataName, scenarioTags), out value))
            {
                return value > 0f;
            }

            return values.TryGetValue(
                BuildKey(dataName, null), out value) && value > 0f;
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
