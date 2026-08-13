using Newtonsoft.Json.Linq;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Modding;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    internal static class TemplateStringExtensionReader
    {
        public static Dictionary<string, string> Read(
            string templateFileName,
            string fieldName)
        {
            Dictionary<string, string> values =
                new Dictionary<string, string>(StringComparer.Ordinal);

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

                    string key = TemplateFloatExtensionReader.BuildKey(
                        dataNameToken.Value<string>(),
                        TemplateFloatExtensionReader.ReadScenarioTags(record));
                    if (valueToken.Type == JTokenType.Null)
                    {
                        values.Remove(key);
                        continue;
                    }

                    string value = valueToken.Value<string>();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Main.Warn("Ignored empty " + fieldName +
                            " value on template '" + dataNameToken + "'.");
                        continue;
                    }

                    values[key] = value.Trim();
                }
            }

            return values;
        }

        public static bool TryGet(
            Dictionary<string, string> values,
            string dataName,
            IEnumerable<string> scenarioTags,
            out string value)
        {
            if (values.TryGetValue(
                TemplateFloatExtensionReader.BuildKey(
                    dataName, scenarioTags), out value))
            {
                return true;
            }

            return values.TryGetValue(
                TemplateFloatExtensionReader.BuildKey(dataName, null),
                out value);
        }
    }
}
