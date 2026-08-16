using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TIEconomyMod
{
    public sealed class HullVolumeCatalog
    {
        private readonly Dictionary<string, float[]> volumesByHull;

        private HullVolumeCatalog(Dictionary<string, float[]> volumesByHull)
        {
            this.volumesByHull = volumesByHull;
        }

        public int HullCount
        {
            get { return volumesByHull.Count; }
        }

        public static HullVolumeCatalog Load(
            string path, Action<string> log)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Hull-volume capacity catalog was not found.", path);
            }

            Dictionary<string, float[]> result =
                new Dictionary<string, float[]>(StringComparer.Ordinal);
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

                int separator = line.IndexOf(',');
                if (separator <= 0 || separator >= line.Length - 1)
                {
                    throw new FormatException(
                        "Invalid hull-volume catalog row " + lineNumber +
                        ".");
                }

                string dataName = line.Substring(0, separator).Trim();
                string[] fields = line.Substring(separator + 1).Split('|');
                if (result.ContainsKey(dataName) || fields.Length == 0)
                {
                    throw new FormatException(
                        "Duplicate or empty hull-volume row for '" +
                        dataName + "'.");
                }

                float[] volumes = new float[fields.Length];
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
                            "Invalid volume for '" + dataName +
                            "' appearance " + index + ".");
                    }
                    volumes[index] = value;
                }
                result.Add(dataName, volumes);
            }

            if (result.Count == 0)
            {
                throw new FormatException(
                    "Hull-volume capacity catalog is empty.");
            }
            if (log != null)
            {
                log("Loaded measured main-hull volumes for " +
                    result.Count + " hull templates.");
            }
            return new HullVolumeCatalog(result);
        }

        public bool TryGetVolume_m3(
            string hullDataName, int appearanceIndex, out float volume_m3)
        {
            volume_m3 = 0f;
            float[] appearances;
            if (string.IsNullOrEmpty(hullDataName) || appearanceIndex < 0 ||
                !volumesByHull.TryGetValue(hullDataName, out appearances) ||
                appearanceIndex >= appearances.Length)
            {
                return false;
            }

            volume_m3 = appearances[appearanceIndex];
            return volume_m3 > 0f;
        }
    }
}
