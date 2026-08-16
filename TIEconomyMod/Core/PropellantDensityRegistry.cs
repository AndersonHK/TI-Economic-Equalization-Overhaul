using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class PropellantDensityRegistry
    {
        private const string TemplateFileName = "TIDriveTemplate";
        private const string DensityFieldName = "propellantDensity_kgm3";

        private sealed class RegistrySnapshot
        {
            public readonly Dictionary<string, float> DensityByKey;
            public readonly Dictionary<TIDriveTemplate, float>
                DensityByTemplate;

            public RegistrySnapshot(
                Dictionary<string, float> densityByKey,
                Dictionary<TIDriveTemplate, float> densityByTemplate)
            {
                DensityByKey = densityByKey;
                DensityByTemplate = densityByTemplate;
            }
        }

        private static RegistrySnapshot snapshot = new RegistrySnapshot(
            new Dictionary<string, float>(StringComparer.Ordinal),
            new Dictionary<TIDriveTemplate, float>(
                ReferenceIdentityComparer<TIDriveTemplate>.Instance));

        public static void Refresh()
        {
            Dictionary<string, float> densityByKey =
                TemplateFloatExtensionReader.Read(
                    TemplateFileName, DensityFieldName);
            Dictionary<TIDriveTemplate, float> densityByTemplate =
                new Dictionary<TIDriveTemplate, float>(
                    ReferenceIdentityComparer<TIDriveTemplate>.Instance);
            foreach (TIDriveTemplate template in
                TemplateManager.GetAllTemplates<TIDriveTemplate>())
            {
                float density;
                if (TemplateFloatExtensionReader.TryGet(
                    densityByKey,
                    template.dataName,
                    template.scenarioTags,
                    out density))
                {
                    densityByTemplate[template] = density;
                }
            }

            snapshot = new RegistrySnapshot(
                densityByKey, densityByTemplate);
            Main.Log("Bound " + densityByTemplate.Count +
                " drive-specific propellant-density override(s).");
        }

        public static float Density_kgm3(TIDriveTemplate drive)
        {
            if (drive == null)
            {
                return 0f;
            }

            RegistrySnapshot current = snapshot;
            float density;
            if (current.DensityByTemplate.TryGetValue(drive, out density) ||
                TemplateFloatExtensionReader.TryGet(
                    current.DensityByKey,
                    drive.dataName,
                    drive.scenarioTags,
                    out density))
            {
                return density;
            }

            switch (drive.propellant)
            {
            case Propellant.Hydrogen:
                return 70.85f;
            case Propellant.Water:
                return 997f;
            case Propellant.NobleGases:
                return 2942f;
            case Propellant.Volatiles:
                return 422.6f;
            case Propellant.Metals:
                return 534f;
            case Propellant.ReactionProducts:
            case Propellant.Anything:
            default:
                return 1000f;
            }
        }

        public static string MaterialLocalizationKey(TIDriveTemplate drive)
        {
            if (drive == null)
            {
                return "UI.Fleets.FuelMaterial.Unknown";
            }

            switch (drive.propellant)
            {
            case Propellant.Hydrogen:
                return "UI.Fleets.FuelMaterial.Hydrogen";
            case Propellant.Water:
                return "UI.Fleets.FuelMaterial.Water";
            case Propellant.NobleGases:
                return "UI.Fleets.FuelMaterial.Xenon";
            case Propellant.Volatiles:
                return "UI.Fleets.FuelMaterial.Methane";
            case Propellant.Metals:
                return "UI.Fleets.FuelMaterial.Lithium";
            case Propellant.ReactionProducts:
                return "UI.Fleets.FuelMaterial.ReactionProducts";
            case Propellant.Anything:
            default:
                return "UI.Fleets.FuelMaterial.Anything";
            }
        }
    }
}
