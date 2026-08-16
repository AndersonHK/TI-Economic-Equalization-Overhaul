using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TIEconomyMod
{
    public sealed class HullDriveScaleCatalog
    {
        private sealed class AppearanceScales
        {
            public float[] DeLaval;
            public float[] Magnetic;
        }

        private readonly Dictionary<string, AppearanceScales> scalesByHull;

        private HullDriveScaleCatalog(
            Dictionary<string, AppearanceScales> scalesByHull)
        {
            this.scalesByHull = scalesByHull;
        }

        public int HullCount
        {
            get { return scalesByHull.Count; }
        }

        public static HullDriveScaleCatalog Load(
            string path, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Hull drive-art scale catalog was not found.", path);
            }

            Dictionary<string, AppearanceScales> result =
                new Dictionary<string, AppearanceScales>(
                    StringComparer.Ordinal);
            int lineNumber = 0;
            foreach (string rawLine in File.ReadAllLines(path))
            {
                lineNumber++;
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#") ||
                    line.StartsWith("dataName,"))
                {
                    continue;
                }

                string[] columns = line.Split(',');
                if (columns.Length != 3)
                {
                    throw new FormatException(
                        "Invalid hull drive-art scale catalog row " +
                        lineNumber + ".");
                }

                string dataName = columns[0].Trim();
                if (dataName.Length == 0 || result.ContainsKey(dataName))
                {
                    throw new FormatException(
                        "Duplicate or empty hull drive-art scale row for '" +
                        dataName + "'.");
                }

                float[] deLaval = ParseScales(
                    dataName, "De Laval", columns[1]);
                float[] magnetic = ParseScales(
                    dataName, "Magnetic", columns[2]);
                if (deLaval.Length != magnetic.Length)
                {
                    throw new FormatException(
                        "Drive-art scale appearance counts differ for '" +
                        dataName + "'.");
                }

                result.Add(dataName, new AppearanceScales
                {
                    DeLaval = deLaval,
                    Magnetic = magnetic,
                });
            }

            if (result.Count == 0)
            {
                throw new FormatException(
                    "Hull drive-art scale catalog is empty.");
            }
            if (log != null)
            {
                log("Loaded measured De Laval/Magnetic drive-art scales for " +
                    result.Count + " human hull templates.");
            }
            return new HullDriveScaleCatalog(result);
        }

        public bool TryGetScales(
            string hullDataName,
            int appearanceIndex,
            out float deLaval,
            out float magnetic)
        {
            deLaval = 0f;
            magnetic = 0f;
            AppearanceScales appearances;
            if (string.IsNullOrEmpty(hullDataName) || appearanceIndex < 0 ||
                !scalesByHull.TryGetValue(hullDataName, out appearances) ||
                appearanceIndex >= appearances.DeLaval.Length)
            {
                return false;
            }

            deLaval = appearances.DeLaval[appearanceIndex];
            magnetic = appearances.Magnetic[appearanceIndex];
            return deLaval > 0f && magnetic > 0f;
        }

        public bool TryGetScale(
            string hullDataName,
            int appearanceIndex,
            string nozzleFamily,
            out float scale)
        {
            scale = 0f;
            float deLaval;
            float magnetic;
            if (!TryGetScales(
                    hullDataName,
                    appearanceIndex,
                    out deLaval,
                    out magnetic))
            {
                return false;
            }

            if (string.Equals(
                    nozzleFamily, "DeLaval", StringComparison.Ordinal))
            {
                scale = deLaval;
                return true;
            }
            if (string.Equals(
                    nozzleFamily, "Magnetic", StringComparison.Ordinal))
            {
                scale = magnetic;
                return true;
            }

            return false;
        }

        private static float[] ParseScales(
            string dataName, string family, string values)
        {
            string[] fields = values.Split('|');
            if (fields.Length == 0)
            {
                throw new FormatException(
                    "No " + family + " drive-art scales for '" +
                    dataName + "'.");
            }

            float[] result = new float[fields.Length];
            for (int index = 0; index < fields.Length; index++)
            {
                float value;
                if (!float.TryParse(
                        fields[index],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out value) ||
                    value <= 0f || float.IsNaN(value) ||
                    float.IsInfinity(value))
                {
                    throw new FormatException(
                        "Invalid " + family + " drive-art scale for '" +
                        dataName + "' appearance " + index + ".");
                }
                result[index] = value;
            }
            return result;
        }
    }
}
