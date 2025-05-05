using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIEconomyMod.EconomyInvestmentPatches
{
    [HarmonyPatch(typeof(TIGlobalValuesState), "AddEconomyPriorityEnvEffect")]
    public static class EconomyEnvironmentEffectPatch
    {
        [HarmonyPrefix]
        public static bool AddEconomyPriorityEnvEffectOverwrite(TINationState nation, TIGlobalValuesState __instance)
        {
            /*
            //Overwrites the vanilla greenhouse gas emissions values for completing an economy investment
            //Applies a diminishing returns effect on the emissions from resource regions

            //CO2
            float baseCO2 = TemplateManager.global.SpoCO2_ppm; //Flat per investment completion, changed from EcoCO2_ppm to SpoCO2_ppm

            float resRegionCO2Mult = 1.0f;
            if (nation.currentResourceRegions >= 1) resRegionCO2Mult += 0.3f;
            if (nation.currentResourceRegions >= 2) resRegionCO2Mult += 0.3f / 2f;
            if (nation.currentResourceRegions >= 3) resRegionCO2Mult += 0.3f / 4f;
            if (nation.currentResourceRegions >= 4) resRegionCO2Mult += (0.3f / 8f) * (nation.currentResourceRegions - 3);
            //Emissions mult is >2x higher than the gdp mult

            __instance.AddCO2_ppm(baseCO2 * resRegionCO2Mult, GHGSources.SpoilsPriority); //Added GHGSources.SpoilsPriority


            //CH4
            float baseCH4 = TemplateManager.global.SpoCH4_ppm; //Changed from EcoCH4_ppm to SpoCH4_ppm

            float resRegionCH4Mult = 1.0f;
            if (nation.currentResourceRegions >= 1) resRegionCH4Mult += 0.2f;
            if (nation.currentResourceRegions >= 2) resRegionCH4Mult += 0.2f / 2f;
            if (nation.currentResourceRegions >= 3) resRegionCH4Mult += 0.2f / 4f;
            if (nation.currentResourceRegions >= 4) resRegionCH4Mult += (0.2f / 8f) * (nation.currentResourceRegions - 3);
            //Emissions mult is slightly higher than the gdp mult

            __instance.AddCH4_ppm(baseCH4 * resRegionCH4Mult, GHGSources.SpoilsPriority); //Added GHGSources.SpoilsPriority


            //N2O
            float baseN2O = TemplateManager.global.SpoN2O_ppm; //Changed from EcoN2O_ppm to SpoN2O_ppm

            float resRegionN2OMult = 1.0f;
            if (nation.currentResourceRegions >= 1) resRegionN2OMult += 0.05f;
            if (nation.currentResourceRegions >= 2) resRegionN2OMult += 0.05f / 2f;
            if (nation.currentResourceRegions >= 3) resRegionN2OMult += 0.05f / 4f;
            if (nation.currentResourceRegions >= 4) resRegionN2OMult += (0.05f / 8f) * (nation.currentResourceRegions - 3);
            //Emissions mult is significantly lower than the gdp mult

            __instance.AddN2O_ppm(baseN2O * resRegionN2OMult, GHGSources.SpoilsPriority); //Added GHGSources.SpoilsPriority



            */
            return true; //Skip the original method Changed to true
        }
    }
}
