using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Text;

namespace TIEconomyMod.Patches
{
    internal static class MilitaryRuntime
    {
        public static int EligibleArmyCount(TINationState nation)
        {
            if (nation == null || nation.armies == null)
            {
                return 0;
            }

            int count = 0;
            foreach (TIArmyState army in nation.armies)
            {
                if (army != null && TIGameState.Valid(army) && army.HumanArmy && !army.destroyed)
                {
                    count++;
                }
            }
            return count;
        }

        public static double Cost(TINationState nation, double fromTechnology, double toTechnology)
        {
            ArmySettings army = Main.settings.army;
            MilitarySettings military = Main.settings.military;
            return MilitaryMath.MiltechCost(
                fromTechnology,
                toTechnology,
                EligibleArmyCount(nation),
                nation.maxMilitaryTechLevel,
                army.costCoefficient,
                    army.costGrowthBase,
                military.doctrineBaseCostAtTechOne,
                military.doctrineCostGrowthBase,
                military.catchupGapCoefficient);
        }

        public static bool TryTechnologyAfter(TINationState nation, double investment, out double technology)
        {
            ArmySettings army = Main.settings.army;
            MilitarySettings military = Main.settings.military;
            return MilitaryMath.TrySolveTechAfterInvestment(
                nation.militaryTechLevel,
                nation.maxMilitaryTechLevel,
                EligibleArmyCount(nation),
                investment,
                army.costCoefficient,
                    army.costGrowthBase,
                military.doctrineBaseCostAtTechOne,
                military.doctrineCostGrowthBase,
                military.catchupGapCoefficient,
                out technology);
        }
    }

    [HarmonyPatch(typeof(TINationState), "GetRequiredInvestmentPointsForPriority")]
    public static class MilitaryPriorityCostPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance, PriorityType priority)
        {
            if (priority == PriorityType.Military_BuildArmy &&
                Main.FeatureEnabled(Main.settings.army.enabled))
            {
                double cost = MilitaryMath.ArmyCost(
                    __instance.militaryTechLevel,
                    Main.settings.army.costCoefficient,
                Main.settings.army.costGrowthBase);
                if (!MilitaryMath.IsFinite(cost) || cost <= 0d || cost > float.MaxValue)
                {
                    Main.Warn("Army construction cost was invalid; retaining vanilla.");
                    return true;
                }

                __result = (float)cost;
                return false;
            }

            if (priority == PriorityType.Military &&
                Main.FeatureEnabled(Main.settings.military.enabled))
            {
                double remaining = MilitaryRuntime.Cost(
                    __instance,
                    __instance.militaryTechLevel,
                    __instance.maxMilitaryTechLevel);
                if (!MilitaryMath.IsFinite(remaining) || remaining < 0d)
                {
                    Main.Warn("Military priority threshold was invalid; retaining vanilla.");
                    return true;
                }

                // Keep the ordinary unit at 1 IP, but make the last completion consume
                // exactly the integrated remainder. At the cap, return a harmless positive
                // value; ValidPriority prevents another completion.
                __result = (float)(remaining > 0d ? Math.Min(1d, remaining) : 1d);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TINationState), "militaryPriorityTechLevelChange", MethodType.Getter)]
    public static class MilitaryTechnologyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TINationState __instance)
        {
            if (!Main.FeatureEnabled(Main.settings.military.enabled))
            {
                return true;
            }

            double remaining = MilitaryRuntime.Cost(
                __instance,
                __instance.militaryTechLevel,
                __instance.maxMilitaryTechLevel);
            double investment = MilitaryMath.IsFinite(remaining)
                ? Math.Min(1d, Math.Max(0d, remaining))
                : double.NaN;
            double technology;
            if (!MilitaryMath.IsFinite(investment) ||
                !MilitaryRuntime.TryTechnologyAfter(__instance, investment, out technology))
            {
                Main.Warn("Military technology inversion failed; retaining vanilla.");
                return true;
            }

            __result = (float)Math.Max(0d, technology - __instance.militaryTechLevel);
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "CanDirectInvest")]
    public static class MilitaryDirectInvestmentPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            ref bool __result,
            TINationState __instance,
            TIFactionState faction,
            PriorityType priority,
            ref int maxAllowed)
        {
            if (priority != PriorityType.Military ||
                !Main.FeatureEnabled(Main.settings.military.enabled))
            {
                return;
            }

            double remaining = MilitaryRuntime.Cost(
                __instance,
                __instance.militaryTechLevel,
                __instance.maxMilitaryTechLevel);
            if (!MilitaryMath.IsFinite(remaining) || remaining < 0d)
            {
                return;
            }

            // Direct investment is integer-valued in TI. Truncation prevents charging
            // more than the exact integrated remainder; ordinary national IP can supply
            // any final sub-IP completion.
            int exactLimit = remaining >= int.MaxValue
                ? int.MaxValue
                : Math.Max(0, (int)Math.Floor(remaining + 1e-7d));
            maxAllowed = Math.Min(__instance.MaxDirectInvestIPsRemainingThisYear(), exactLimit);
            __result = maxAllowed > 0 && __instance.ValidPriority(priority) &&
                (!__instance.policy_closedBorders || __instance.FactionHasControlPoint(faction));
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "investmentArmyFactor", MethodType.Getter)]
    public static class ArmyUpkeepPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TIArmyState __instance)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return true;
            }

            TINationState nation = __instance.homeNation;
            double upkeep = nation == null
                ? double.NaN
                : MilitaryMath.Upkeep(
                    nation.militaryTechLevel,
                    __instance.useHomeInvestmentFactor,
                    settings.homeUpkeepDivisor,
                    settings.awayUpkeepDivisor);
            if (!MilitaryMath.IsFinite(upkeep) || upkeep < 0d || upkeep > float.MaxValue)
            {
                Main.Warn("Army upkeep was invalid; retaining vanilla.");
                return true;
            }

            __result = (float)upkeep;
            return false;
        }
    }

    [HarmonyPatch(typeof(TINationState), "ModifyAccumulatedInvestment")]
    public static class ArmyRepairDebtAccumulationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            TINationState __instance,
            PriorityType priority,
            float by,
            bool multiply,
            bool triggerUpdate)
        {
            if (priority != PriorityType.Military_BuildArmy ||
                !Main.FeatureEnabled(Main.settings.army.enabled))
            {
                return true;
            }

            float current = __instance.GetAccumulatedInvestmentPoints(priority);
            double calculated;
            if (!MilitaryMath.TryApplyBuildArmyProgress(
                current, by, multiply, out calculated) ||
                calculated < -float.MaxValue || calculated > float.MaxValue)
            {
                Main.Warn("Build Army accumulation was invalid; retaining its previous value.");
                calculated = current;
            }
            float value = (float)calculated;
            if (!__instance.ValidPriority(priority))
            {
                value = Math.Min(
                    value,
                    __instance.GetRequiredInvestmentPointsForPriority(priority) - 1f);
            }

            // Deliberately no lower clamp: negative accumulation is persistent repair debt.
            __instance.SetAccumulatedInvestmentPoints(priority, value, triggerUpdate);
            return false;
        }
    }

    [HarmonyPatch(typeof(PriorityListItemController), "priorityAccumulationStr")]
    public static class ArmyRepairDebtDisplayPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref string __result,
            TINationState nation,
            PriorityType priority)
        {
            if (priority != PriorityType.Military_BuildArmy ||
                !Main.FeatureEnabled(Main.settings.army.enabled) ||
                nation == null)
            {
                return true;
            }

            float progress = nation.GetAccumulatedInvestmentPoints(priority);
            if (progress >= 0f)
            {
                return true;
            }

            __result = "Repair debt " + (-progress).ToString("0.##") + " IP";
            return false;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "HealDamage")]
    public static class ArmyRepairChargePatch
    {
        [HarmonyPrefix]
        public static void Prefix(TIArmyState __instance, ref float __state)
        {
            __state = __instance == null ? 0f : __instance.strength;
        }

        [HarmonyPostfix]
        public static void Postfix(TIArmyState __instance, float __state)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.enabled) ||
                __instance == null ||
                !__instance.HumanArmy ||
                __instance.destroyed ||
                __instance.homeNation == null)
            {
                return;
            }

            double healing = Math.Max(0d, __instance.strength - __state);
            if (healing <= 0d)
            {
                return;
            }

            double charge = MilitaryMath.RepairCharge(
                __instance.homeNation.militaryTechLevel,
                healing,
                settings.repairShare,
                settings.costCoefficient,
                settings.costGrowthBase);
            if (!MilitaryMath.IsFinite(charge) || charge < 0d || charge > float.MaxValue)
            {
                Main.Warn("Army repair charge was invalid; no repair debt was recorded.");
                return;
            }

            __instance.homeNation.ModifyAccumulatedInvestment(
                PriorityType.Military_BuildArmy,
                -(float)charge,
                multiply: false,
                triggerUpdate: true);
        }
    }

    internal static class ArmyCombatRuntime
    {
        public static bool TryRating(
            TIArmyState army,
            TIRegionState battlefield,
            bool includeFriendlyCohesion,
            out float rating)
        {
            rating = 0f;
            if (army == null || battlefield == null || army.homeNation == null)
            {
                return false;
            }

            ArmySettings settings = Main.settings.army;
            double modifierScale = settings.combatModifierScale;
            double vanillaValue = army.techLevel +
                army.homeNation.adviserCommandBonus + army.LEOHabBonus;
            double value = army.techLevel +
                LandCombatMath.ScaleContribution(
                    army.homeNation.adviserCommandBonus, modifierScale) +
                LandCombatMath.ScaleContribution(army.LEOHabBonus, modifierScale);

            if (army.homeNation.regions.Contains(battlefield))
            {
                vanillaValue += TemplateManager.global.armyRegionDefenseBonus;
                value += LandCombatMath.ScaleContribution(
                    TemplateManager.global.armyRegionDefenseBonus, modifierScale);
                if (battlefield.terrain == TerrainType.Rugged)
                {
                    vanillaValue += TemplateManager.global.ruggedTerrainDefenseBonus;
                    value += LandCombatMath.ScaleContribution(
                        TemplateManager.global.ruggedTerrainDefenseBonus, modifierScale);
                }
                if (battlefield.coreEconomicRegion)
                {
                    vanillaValue += TemplateManager.global.coreEconomicRegionDefenseBonus;
                    value += LandCombatMath.ScaleContribution(
                        TemplateManager.global.coreEconomicRegionDefenseBonus, modifierScale);
                }
            }

            TIControlPoint controlPoint = army.ref_controlPoint;
            if (controlPoint != null && controlPoint.benefitsDisabled)
            {
                vanillaValue -= TemplateManager.global.armyCrackdownMalus;
                value += LandCombatMath.ScaleContribution(
                    -TemplateManager.global.armyCrackdownMalus, modifierScale);
            }
            if (battlefield.terrain == TerrainType.Rugged)
            {
                float realized = TIEffectsState.SumEffectsModifiers(
                    Context.ArmyRuggedWarfare, army.faction, (float)vanillaValue);
                vanillaValue += realized;
                value += LandCombatMath.ScaleContribution(realized, modifierScale);
            }
            if (battlefield.coreEconomicRegion)
            {
                float realized = TIEffectsState.SumEffectsModifiers(
                    Context.ArmyUrbanWarfare, army.faction, (float)vanillaValue);
                value += LandCombatMath.ScaleContribution(realized, modifierScale);
            }
            if (includeFriendlyCohesion)
            {
                value += LandCombatMath.ScaleContribution(
                    army.FightingInFriendlyRegionBonus, modifierScale);
            }

            value = LandCombatMath.RatingAfterStrength(
                value, army.strength, settings.maximumStrengthPenalty);
            if (!MilitaryMath.IsFinite(value) || value < -float.MaxValue || value > float.MaxValue)
            {
                return false;
            }

            rating = (float)value;
            return true;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "GetAttackValue")]
    public static class ArmyAttackRatingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TIArmyState __instance)
        {
            if (!Main.FeatureEnabled(Main.settings.army.enabled))
            {
                return true;
            }

            float rating;
            if (!ArmyCombatRuntime.TryRating(
                __instance, __instance.currentRegion, true, out rating))
            {
                Main.Warn("Army attack rating was invalid; retaining vanilla.");
                return true;
            }
            __result = rating;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "GetEnemyDefendValue")]
    public static class ArmyDefenseRatingPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result,
            TIArmyState __instance,
            TIArmyState defendingArmy)
        {
            if (!Main.FeatureEnabled(Main.settings.army.enabled))
            {
                return true;
            }

            float rating;
            if (!ArmyCombatRuntime.TryRating(
                defendingArmy, __instance.currentRegion, false, out rating))
            {
                Main.Warn("Army defense rating was invalid; retaining vanilla.");
                return true;
            }
            __result = rating;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "GetEffectiveCombatStrength")]
    public static class ArmyEffectiveStrengthPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result, TIArmyState __instance)
        {
            if (!Main.FeatureEnabled(Main.settings.army.enabled))
            {
                return true;
            }

            double uninjured = __instance.techLevel +
                LandCombatMath.ScaleContribution(
                    __instance.homeNation.adviserCommandBonus,
                    Main.settings.army.combatModifierScale) +
                LandCombatMath.ScaleContribution(
                    __instance.LEOHabBonus,
                    Main.settings.army.combatModifierScale);
            double result = LandCombatMath.RatingAfterStrength(
                uninjured,
                __instance.strength,
                Main.settings.army.maximumStrengthPenalty);
            if (!MilitaryMath.IsFinite(result))
            {
                return true;
            }
            __result = (float)result;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "GetCombatSuccessChance")]
    public static class ArmyHitChancePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            ref float __result,
            float attackValue,
            float enemyValue)
        {
            if (!Main.FeatureEnabled(Main.settings.army.enabled))
            {
                return true;
            }

            double chance = LandCombatMath.HitChance(
                attackValue, enemyValue, Main.settings.army.hitCurveBase);
            if (!MilitaryMath.IsFinite(chance))
            {
                Main.Warn("Army hit chance was invalid; retaining vanilla.");
                return true;
            }
            __result = (float)chance;
            return false;
        }
    }

    [HarmonyPatch(typeof(TIArmyState), "CombatBreakdown_Army")]
    public static class ArmyCombatBreakdownPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result)
        {
            ArmySettings settings = Main.settings.army;
            if (!Main.FeatureEnabled(settings.enabled))
            {
                return;
            }

            StringBuilder note = new StringBuilder()
                .AppendLine()
                .Append("EEO: listed adviser, LEO, region, cohesion, crackdown, and project ")
                .Append("modifiers apply at x")
                .Append(settings.combatModifierScale.ToString("0.##"))
                .Append("; damage subtracts up to ")
                .Append(settings.maximumStrengthPenalty.ToString("0.##"))
                .Append(" rating; hit odds use base ")
                .Append(settings.hitCurveBase.ToString("0.##"))
                .Append(".");
            __result = (__result ?? string.Empty).TrimEnd() + "\n" + note;
        }
    }
}
