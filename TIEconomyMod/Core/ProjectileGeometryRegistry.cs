using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;

namespace TIEconomyMod
{
    public static class ProjectileGeometryRegistry
    {
        private const string DiameterFieldName = "projectileDiameter_mm";

        private static Dictionary<string, float> gunDiameters =
            new Dictionary<string, float>(StringComparer.Ordinal);
        private static Dictionary<string, float> magneticDiameters =
            new Dictionary<string, float>(StringComparer.Ordinal);

        public static void Refresh()
        {
            Dictionary<string, float> nextGunDiameters =
                TemplateFloatExtensionReader.Read(
                    "TIGunTemplate", DiameterFieldName);
            Dictionary<string, float> nextMagneticDiameters =
                TemplateFloatExtensionReader.Read(
                    "TIMagneticGunTemplate", DiameterFieldName);

            gunDiameters = nextGunDiameters;
            magneticDiameters = nextMagneticDiameters;
            Main.Log("Bound generic projectile diameter data for " +
                (nextGunDiameters.Count + nextMagneticDiameters.Count) +
                " weapon template record(s).");
        }

        public static bool TryGetDiameter_mm(
            TIProjectileWeaponTemplate template, out float diameter_mm)
        {
            Dictionary<string, float> values;
            switch (template.weaponClass)
            {
            case WeaponClass.NavalGun:
                values = gunDiameters;
                break;
            case WeaponClass.Magnetic:
                values = magneticDiameters;
                break;
            default:
                diameter_mm = 0f;
                return false;
            }

            return TemplateFloatExtensionReader.TryGet(
                values,
                template.dataName,
                template.scenarioTags,
                out diameter_mm);
        }
    }
}
