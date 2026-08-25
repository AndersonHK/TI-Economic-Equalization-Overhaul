using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class PowerPlantScalingRegistry
    {
        private const string TemplateFileName = "TIPowerPlantTemplate";
        private const string MultiplierFieldName =
            "openCycleThermalMassMultiplier";

        private static Dictionary<string, float> multiplierByKey =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public static void Refresh()
        {
            multiplierByKey = TemplateFloatExtensionReader.Read(
                TemplateFileName, MultiplierFieldName);
            Main.Log("Bound " + multiplierByKey.Count +
                " explicit open-cycle thermal mass multiplier(s). " +
                "Other plants use technology-class defaults.");
        }

        public static float OpenCycleThermalMassMultiplier(
            TIPowerPlantTemplate powerPlant)
        {
            if (powerPlant == null)
            {
                return 1f;
            }

            float multiplier;
            if (TemplateFloatExtensionReader.TryGet(
                multiplierByKey,
                powerPlant.dataName,
                powerPlant.scenarioTags,
                out multiplier))
            {
                return Math.Max(0.001f, Math.Min(1f, multiplier));
            }

            return DefaultMultiplier(powerPlant.powerPlantClass);
        }

        public static float DefaultMultiplier(
            PowerPlantRequirement powerPlantClass)
        {
            return PowerPlantScalingMath
                .DefaultOpenCycleThermalMassMultiplier(powerPlantClass);
        }
    }
}
