using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System.Collections.Generic;
using System.Reflection;

namespace TIEconomyMod.Patches
{
    internal static class NationalSpaceAssets
    {
        internal static bool UpkeepEnabled(TINationState nation)
        {
            return nation != null && !nation.alienNation &&
                Main.FeatureEnabled(Main.settings.investment.enabled) &&
                Main.settings.investment.spaceAssetUpkeepEnabled;
        }

        internal static bool CapEnabled(TINationState nation)
        {
            return nation != null && !nation.alienNation &&
                Main.FeatureEnabled(Main.settings.investment.enabled) &&
                Main.settings.investment.boostCapEnabled;
        }

        internal static double Upkeep(TINationState nation)
        {
            InvestmentSettings s = Main.settings.investment;
            return NationalSpaceAssetMath.Upkeep(nation.missionControl,
                nation.rawBoostPerMonth_dekatons, nation.spaceFunding_month,
                s.missionControlUpkeep, s.boostUpkeep, s.fundingUpkeep);
        }

        internal static double CapMonth(TINationState nation)
        {
            return NationalSpaceAssetMath.BoostCapMonth(nation.GDP / 1000000000d,
                nation.education, Main.settings.investment.boostCapFactor);
        }

        internal static double HeadroomYear(TINationState nation)
        {
            return NationalSpaceAssetMath.BoostHeadroomYear(nation.rawBoostPerYear_dekatons,
                CapMonth(nation));
        }
    }

    [HarmonyPatch(typeof(TINationState), "SetBaseInvestmentPoints_month")]
    public static class NationalSpaceAssetUpkeepPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, ref float ___baseInvestmentPoints_month)
        {
            if (NationalSpaceAssets.UpkeepEnabled(__instance))
                ___baseInvestmentPoints_month = (float)NationalSpaceAssetMath.Available(
                    ___baseInvestmentPoints_month, NationalSpaceAssets.Upkeep(__instance));
        }
    }

    [HarmonyPatch(typeof(TINationState), nameof(TINationState.ValidPriority))]
    public static class BoostCapPriorityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, PriorityType priority, ref bool __result)
        {
            if (__result && priority == PriorityType.LaunchFacilities &&
                NationalSpaceAssets.CapEnabled(__instance))
                __result = NationalSpaceAssets.HeadroomYear(__instance) > 0d;
        }
    }

    [HarmonyPatch(typeof(TIRegionState), nameof(TIRegionState.ChangeSpaceFacilityValue))]
    public static class BoostCapFacilityPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TIRegionState __instance, SpaceFacilityType facilityType,
            ref float fValue)
        {
            if (facilityType == SpaceFacilityType.launchFacility && fValue > 0f &&
                NationalSpaceAssets.CapEnabled(__instance.nation))
                fValue = (float)NationalSpaceAssetMath.ClampBoostChange(fValue,
                    __instance.nation.rawBoostPerYear_dekatons,
                    NationalSpaceAssets.CapMonth(__instance.nation));
        }

        [HarmonyPostfix]
        public static void Postfix(TIRegionState __instance, SpaceFacilityType facilityType)
        {
            if (facilityType == SpaceFacilityType.launchFacility &&
                NationalSpaceAssets.CapEnabled(__instance.nation))
                __instance.nation.PossiblePriorityValidationChange();
        }
    }

    [HarmonyPatch(typeof(TINationState), nameof(TINationState.CanDirectInvest))]
    public static class BoostCapDirectInvestmentPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, PriorityType priority,
            ref int maxAllowed, ref bool __result)
        {
            if (priority != PriorityType.LaunchFacilities ||
                !NationalSpaceAssets.CapEnabled(__instance)) return;
            if (!__result)
            {
                maxAllowed = 0;
                return;
            }
            maxAllowed = NationalSpaceAssetMath.DirectInvestmentLimit(
                NationalSpaceAssets.HeadroomYear(__instance), __instance.BoostGainHigh(),
                __instance.GetRequiredInvestmentPointsForPriority(priority),
                __instance.GetAccumulatedInvestmentPoints(priority), maxAllowed);
            __result = maxAllowed > 0;
        }
    }

    [HarmonyPatch]
    public static class BoostCapEconomicChangePatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(TINationState), nameof(TINationState.ModifyGDP));
            yield return AccessTools.Method(typeof(TINationState), nameof(TINationState.AddToEducation));
        }

        [HarmonyPrefix]
        public static void Prefix(TINationState __instance, out bool __state)
        {
            __state = NationalSpaceAssets.CapEnabled(__instance) &&
                NationalSpaceAssets.HeadroomYear(__instance) > 0d;
        }

        [HarmonyPostfix]
        public static void Postfix(TINationState __instance, bool __state)
        {
            if (NationalSpaceAssets.CapEnabled(__instance) &&
                __state != (NationalSpaceAssets.HeadroomYear(__instance) > 0d))
                __instance.PossiblePriorityValidationChange();
        }
    }
}
