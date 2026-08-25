using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace TIEconomyMod.Patches
{
    public static class HabUpgradeCostResolver
    {
        public static TIResourcesCost SelectSpaceCost(
            TIHabModuleTemplate template,
            TIFactionState faction,
            TIGameState destination,
            bool isUpgrade,
            int maxDaysToSave,
            bool dontRecalculateIncome)
        {
            TIResourcesCost nativeCost = template.CostFromSpace(
                faction,
                destination,
                isUpgrade,
                false,
                maxDaysToSave,
                dontRecalculateIncome);
            if (nativeCost.CanAfford(faction))
            {
                return nativeCost;
            }

            TIResourcesCost substitutedCost = template.CostFromSpace(
                faction,
                destination,
                isUpgrade,
                true,
                maxDaysToSave,
                dontRecalculateIncome);
            return substitutedCost.CanAfford(faction)
                ? substitutedCost
                : nativeCost;
        }

        // Matches the instance CostFromSpace stack shape so guarded
        // transpilers can replace callvirt with call without moving arguments.
        public static TIResourcesCost SelectSpaceCostCall(
            TIHabModuleTemplate template,
            TIFactionState faction,
            TIGameState destination,
            bool isUpgrade,
            bool substituteBoost,
            int maxDaysToSave,
            bool dontRecalculateIncome)
        {
            return SelectSpaceCost(
                template,
                faction,
                destination,
                isUpgrade,
                maxDaysToSave,
                dontRecalculateIncome);
        }
    }

    internal static class HabUpgradeCostTranspiler
    {
        private static readonly MethodInfo CostFromSpace = AccessTools.Method(
            typeof(TIHabModuleTemplate),
            nameof(TIHabModuleTemplate.CostFromSpace),
            new Type[]
            {
                typeof(TIFactionState),
                typeof(TIGameState),
                typeof(bool),
                typeof(bool),
                typeof(int),
                typeof(bool)
            });

        private static readonly MethodInfo SharedResolver = AccessTools.Method(
            typeof(HabUpgradeCostResolver),
            nameof(HabUpgradeCostResolver.SelectSpaceCostCall));

        internal static IEnumerable<CodeInstruction> ReplaceUpgradeQuotes(
            IEnumerable<CodeInstruction> instructions,
            int expectedReplacements,
            string targetName)
        {
            List<CodeInstruction> patched = new List<CodeInstruction>(instructions);
            int replacements = 0;
            for (int index = 4; index < patched.Count; index++)
            {
                if (!patched[index].Calls(CostFromSpace) ||
                    !LoadsIntegerOne(patched[index - 4]))
                {
                    continue;
                }

                patched[index].opcode = OpCodes.Call;
                patched[index].operand = SharedResolver;
                replacements++;
            }

            if (replacements != expectedReplacements)
            {
                throw new InvalidOperationException(
                    targetName + " expected " + expectedReplacements +
                    " upgrade CostFromSpace calls, found " + replacements + ".");
            }

            return patched;
        }

        private static bool LoadsIntegerOne(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Ldc_I4_1 ||
                (instruction.opcode == OpCodes.Ldc_I4 &&
                 instruction.operand is int &&
                 (int)instruction.operand == 1) ||
                (instruction.opcode == OpCodes.Ldc_I4_S &&
                 instruction.operand is sbyte &&
                 (sbyte)instruction.operand == 1);
        }
    }

    [HarmonyPatch]
    public static class HabUpgradePreviewCostPatch
    {
        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(HabitatsScreenController),
                "UpdateModulePreviewText",
                new Type[] { typeof(bool), typeof(bool) });
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return HabUpgradeCostTranspiler.ReplaceUpgradeQuotes(
                instructions,
                2,
                "HabitatsScreenController.UpdateModulePreviewText");
        }
    }

    [HarmonyPatch]
    public static class HabUpgradePlacementCostPatch
    {
        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(HabitatsScreenController),
                "GetSpaceCost",
                new Type[] { typeof(bool), typeof(TIHabModuleState) });
        }

        [HarmonyPrefix]
        internal static bool Prefix(
            bool moduleToPlaceIsUpgrade,
            TIHabModuleTemplate ___proposedModuleTemplate,
            TIHabState ___habToDisplay,
            ref TIResourcesCost __result)
        {
            __result = HabUpgradeCostResolver.SelectSpaceCost(
                ___proposedModuleTemplate,
                ___habToDisplay.faction,
                ___habToDisplay,
                moduleToPlaceIsUpgrade,
                0,
                false);
            return false;
        }
    }

    [HarmonyPatch]
    public static class HabUpgradeBulkExecutionCostPatch
    {
        [HarmonyTargetMethod]
        internal static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(HabitatsScreenController),
                "UpgradeAllModulesSelected",
                new Type[] { typeof(List<TIHabModuleState>) });
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return HabUpgradeCostTranspiler.ReplaceUpgradeQuotes(
                instructions,
                2,
                "HabitatsScreenController.UpgradeAllModulesSelected");
        }
    }
}
