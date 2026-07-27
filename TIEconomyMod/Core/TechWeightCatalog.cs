using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TIEconomyMod
{
    public struct TechWeights
    {
        public TechWeights(
            float productivityPercent,
            float laborSubstitution,
            float resourceSubstitution)
        {
            ProductivityPercent = productivityPercent;
            LaborSubstitution = laborSubstitution;
            ResourceSubstitution = resourceSubstitution;
        }

        public float ProductivityPercent { get; private set; }
        public float LaborSubstitution { get; private set; }
        public float ResourceSubstitution { get; private set; }
    }

    public sealed class TechWeightCatalog
    {
        private static readonly HashSet<string> StartingTechnologies =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "MissionToSpace",
                "AdvancedChemicalRocketry"
            };

        private readonly Dictionary<string, TechWeights> weights;

        private TechWeightCatalog(
            Dictionary<string, TechWeights> weights,
            float totalFutureLaborWeight,
            float totalFutureResourceWeight)
        {
            this.weights = weights;
            TotalFutureLaborWeight = totalFutureLaborWeight;
            TotalFutureResourceWeight = totalFutureResourceWeight;
        }

        public int Count
        {
            get { return weights.Count; }
        }

        public IEnumerable<string> TechnologyIds
        {
            get { return weights.Keys; }
        }

        public float TotalFutureLaborWeight { get; private set; }

        public float TotalFutureResourceWeight { get; private set; }

        public bool TryGetWeights(string technologyId, out TechWeights technologyWeights)
        {
            return weights.TryGetValue(technologyId, out technologyWeights);
        }

        public static bool IsStartingTechnology(string technologyId)
        {
            return StartingTechnologies.Contains(technologyId);
        }

        public static TechWeightCatalog Load(
            string path,
            Action<string> log,
            Func<string, bool> isKnownTechnology = null)
        {
            Dictionary<string, TechWeights> loaded =
                new Dictionary<string, TechWeights>(StringComparer.Ordinal);
            if (!File.Exists(path))
            {
                log("Economy technology weight file was not found at " + path + "; technology scaling will remain at 1x.");
                return new TechWeightCatalog(loaded, 0f, 0f);
            }

            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0 ||
                !string.Equals(
                    lines[0].Trim(),
                    "tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale",
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "economy-tech-weights.csv must begin with tech_id,enabled,productivity_percent,labor_substitution,resource_substitution,rationale.");
            }

            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
            float totalFutureLaborWeight = 0f;
            float totalFutureResourceWeight = 0f;
            for (int index = 1; index < lines.Length; index++)
            {
                string line = lines[index].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] columns = line.Split(',');
                if (columns.Length < 6)
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
                float productivityPercent;
                float laborSubstitution;
                float resourceSubstitution;
                if (!bool.TryParse(columns[1].Trim(), out rowEnabled) ||
                    !float.TryParse(columns[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out productivityPercent) ||
                    !float.TryParse(columns[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out laborSubstitution) ||
                    !float.TryParse(columns[4].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out resourceSubstitution) ||
                    !IsPositiveFinite(productivityPercent) ||
                    !IsPositiveFinite(laborSubstitution) ||
                    !IsPositiveFinite(resourceSubstitution))
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
                    loaded.Add(id, new TechWeights(
                        productivityPercent,
                        laborSubstitution,
                        resourceSubstitution));
                    if (!IsStartingTechnology(id))
                    {
                        totalFutureLaborWeight += laborSubstitution;
                        totalFutureResourceWeight += resourceSubstitution;
                    }
                }
            }

            if (loaded.Count > 0 &&
                (!IsPositiveFinite(totalFutureLaborWeight) ||
                 !IsPositiveFinite(totalFutureResourceWeight)))
            {
                throw new InvalidDataException(
                    "Enabled future technologies must have positive total labor and resource substitution weights.");
            }

            log("Loaded " + loaded.Count + " enabled economy technology weights. Changes require a restart.");
            return new TechWeightCatalog(
                loaded,
                totalFutureLaborWeight,
                totalFutureResourceWeight);
        }

        private static bool IsPositiveFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }
}
