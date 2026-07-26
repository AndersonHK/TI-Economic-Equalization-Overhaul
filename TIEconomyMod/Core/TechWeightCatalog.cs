using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TIEconomyMod
{
    public sealed class TechWeightCatalog
    {
        private readonly Dictionary<string, float> percentages;

        private TechWeightCatalog(Dictionary<string, float> percentages)
        {
            this.percentages = percentages;
        }

        public int Count
        {
            get { return percentages.Count; }
        }

        public bool TryGetPercent(string technologyId, out float percent)
        {
            return percentages.TryGetValue(technologyId, out percent);
        }

        public static TechWeightCatalog Load(
            string path,
            Action<string> log,
            Func<string, bool> isKnownTechnology = null)
        {
            Dictionary<string, float> loaded = new Dictionary<string, float>(StringComparer.Ordinal);
            if (!File.Exists(path))
            {
                log("Economy technology weight file was not found at " + path + "; technology scaling will remain at 1x.");
                return new TechWeightCatalog(loaded);
            }

            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0 ||
                !string.Equals(lines[0].Trim(), "tech_id,enabled,percent,rationale", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "economy-tech-weights.csv must begin with tech_id,enabled,percent,rationale.");
            }

            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] columns = line.Split(',');
                if (columns.Length < 4)
                {
                    log("Skipping malformed economy technology row " + (index + 1) + ".");
                    continue;
                }

                string id = columns[0].Trim();
                if (!seenIds.Add(id))
                {
                    throw new InvalidDataException("Duplicate technology ID in economy-tech-weights.csv: " + id);
                }

                bool rowEnabled;
                float percent;
                if (!bool.TryParse(columns[1].Trim(), out rowEnabled) ||
                    !float.TryParse(columns[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out percent) ||
                    float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0f)
                {
                    log("Skipping invalid economy technology row " + (index + 1) + " (" + id + ").");
                    continue;
                }

                if (isKnownTechnology != null && !isKnownTechnology(id))
                {
                    log("Skipping unknown economy technology ID " + id + " on row " + (index + 1) + ".");
                    continue;
                }

                if (rowEnabled)
                {
                    loaded.Add(id, percent);
                }
            }

            log("Loaded " + loaded.Count + " enabled economy technology weights. Changes require a restart.");
            return new TechWeightCatalog(loaded);
        }
    }
}
