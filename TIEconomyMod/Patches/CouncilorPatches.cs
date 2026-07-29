using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    public static class CouncilorRuntimeCaps
    {
        private static bool organizationCapInitialized;
        private static int loadedOrganizationCap;

        public static void InitializeOrganizationCap()
        {
            if (!organizationCapInitialized)
            {
                loadedOrganizationCap = TemplateManager.global.councilorMaxOrgs;
                organizationCapInitialized = true;
            }

            ApplyOrganizationCap();
        }

        public static void ApplyOrganizationCap()
        {
            if (!organizationCapInitialized)
            {
                return;
            }

            int organizationCap = loadedOrganizationCap;
            if (Main.settings != null &&
                Main.settings.councilors != null &&
                Main.FeatureEnabled(Main.settings.councilors.enabled))
            {
                organizationCap = (int)Math.Round(
                    Main.settings.councilors.maximumOrganizations,
                    MidpointRounding.AwayFromZero);
            }

            TemplateManager.global.councilorMaxOrgs = organizationCap;
            TIGlobalConfig.globalConfig.councilorMaxOrgs = organizationCap;
        }

        public static int GetConfiguredOrganizationCap()
        {
            if (Main.settings != null &&
                Main.settings.councilors != null &&
                Main.FeatureEnabled(Main.settings.councilors.enabled))
            {
                return (int)Math.Round(
                    Main.settings.councilors.maximumOrganizations,
                    MidpointRounding.AwayFromZero);
            }

            return TemplateManager.global.councilorMaxOrgs;
        }
    }

    [HarmonyPatch(typeof(TICouncilorState), nameof(TICouncilorState.GetAttribute))]
    public static class CouncilorTotalAttributeCapPatch
    {
        private static readonly MethodInfo VanillaCapGetter =
            AccessTools.PropertyGetter(typeof(TICouncilorState), "maxCouncilorAttribute");
        private static readonly MethodInfo ConfiguredCapGetter =
            AccessTools.Method(
                typeof(CouncilorTotalAttributeCapPatch),
                nameof(GetConfiguredCap));

        public static int GetConfiguredCap(TICouncilorState councilor)
        {
            CouncilorSettings config = Main.settings.councilors;
            if (!Main.FeatureEnabled(config.enabled))
            {
                return TemplateManager.global.maxCouncilorAttribute;
            }

            return (int)Math.Round(
                config.totalAttributeCap,
                MidpointRounding.AwayFromZero);
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> result = new List<CodeInstruction>(instructions);
            int replacementCount = 0;

            for (int index = 0; index < result.Count; index++)
            {
                if (!result[index].Calls(VanillaCapGetter))
                {
                    continue;
                }

                CodeInstruction replacement =
                    new CodeInstruction(OpCodes.Call, ConfiguredCapGetter);
                replacement.labels.AddRange(result[index].labels);
                replacement.blocks.AddRange(result[index].blocks);
                result[index] = replacement;
                replacementCount++;
            }

            if (replacementCount != 1)
            {
                string message =
                    "Councilor total-cap transpiler expected one final-cap getter " +
                    "in TICouncilorState.GetAttribute, found " + replacementCount + ".";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }

            return result;
        }
    }

    [HarmonyPatch(
        typeof(TICouncilorState),
        "availableAdministration",
        MethodType.Getter)]
    public static class CouncilorAvailableAdministrationCapPatch
    {
        private static readonly MethodInfo VanillaCapGetter =
            AccessTools.PropertyGetter(typeof(TICouncilorState), "maxCouncilorAttribute");
        private static readonly MethodInfo ConfiguredCapGetter =
            AccessTools.Method(
                typeof(CouncilorTotalAttributeCapPatch),
                nameof(CouncilorTotalAttributeCapPatch.GetConfiguredCap));

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> result = new List<CodeInstruction>(instructions);
            int replacementCount = 0;

            for (int index = 0; index < result.Count; index++)
            {
                if (!result[index].Calls(VanillaCapGetter))
                {
                    continue;
                }

                CodeInstruction replacement =
                    new CodeInstruction(OpCodes.Call, ConfiguredCapGetter);
                replacement.labels.AddRange(result[index].labels);
                replacement.blocks.AddRange(result[index].blocks);
                result[index] = replacement;
                replacementCount++;
            }

            if (replacementCount != 1)
            {
                string message =
                    "Councilor Administration-cap transpiler expected one " +
                    "base-cap getter in availableAdministration, found " +
                    replacementCount + ".";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }

            return result;
        }
    }

    [HarmonyPatch(
        typeof(TICouncilorState),
        nameof(TICouncilorState.SufficientCapacityForOrg))]
    public static class CouncilorOrganizationWeightCapPatch
    {
        private static readonly MethodInfo VanillaOrgCapacityMaximum =
            AccessTools.Method(
                typeof(TICouncilorState),
                nameof(TICouncilorState.GetClampedMaxStatValue));
        private static readonly MethodInfo ConfiguredOrgCapacityMaximum =
            AccessTools.Method(
                typeof(CouncilorOrganizationWeightCapPatch),
                nameof(GetConfiguredOrgCapacityMaximum));

        public static int GetConfiguredOrgCapacityMaximum(
            TICouncilorState councilor,
            CouncilorAttribute attribute)
        {
            CouncilorSettings config = Main.settings.councilors;
            if (!Main.FeatureEnabled(config.enabled))
            {
                return councilor.GetClampedMaxStatValue(attribute);
            }

            return CouncilorTotalAttributeCapPatch.GetConfiguredCap(councilor);
        }

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> result = new List<CodeInstruction>(instructions);
            int replacementCount = 0;

            for (int index = 0; index < result.Count; index++)
            {
                if (!result[index].Calls(VanillaOrgCapacityMaximum))
                {
                    continue;
                }

                CodeInstruction replacement =
                    new CodeInstruction(OpCodes.Call, ConfiguredOrgCapacityMaximum);
                replacement.labels.AddRange(result[index].labels);
                replacement.blocks.AddRange(result[index].blocks);
                result[index] = replacement;
                replacementCount++;
            }

            if (replacementCount != 1)
            {
                string message =
                    "Councilor org-weight transpiler expected one base-cap " +
                    "check in SufficientCapacityForOrg, found " +
                    replacementCount + ".";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(CouncilGridController), nameof(CouncilGridController.StatDetail))]
    public static class CouncilorAttributeCapTooltipPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            TICouncilorState councilor,
            CouncilorAttribute attribute,
            ref string __result)
        {
            CouncilorSettings config = Main.settings.councilors;
            if (!Main.FeatureEnabled(config.enabled))
            {
                return;
            }

            int baseMaximum = councilor.GetClampedMaxStatValue(attribute);
            string vanillaMaximum = Loc.T(
                "UI.Councilor.MaxStat",
                baseMaximum,
                TemplateManager.global.maxCouncilorAttribute.ToString("N0"));
            string configuredMaximum = Loc.T(
                "UI.Councilor.MaxStat",
                baseMaximum,
                CouncilorTotalAttributeCapPatch.GetConfiguredCap(councilor).ToString("N0"));

            if (__result.Contains(vanillaMaximum))
            {
                __result = __result.Replace(vanillaMaximum, configuredMaximum);
            }
        }
    }
}
