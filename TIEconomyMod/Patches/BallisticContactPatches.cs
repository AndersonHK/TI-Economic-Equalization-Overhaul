using HarmonyLib;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using TIEconomyMod.Core;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(BallisticProjectileController),"Fire")]
    public static class BallisticCompletedStepRegistrationPatch
    {
        [HarmonyPostfix,HarmonyPriority(Priority.Last)]
        public static void Postfix(BallisticProjectileController __instance,ProjectileJobContainer ____container)
        {if(BattleScaleRuntime.Enabled)BallisticContactRuntime.Fired(__instance,____container);}
    }
    [HarmonyPatch(typeof(BallisticProjectileController),"UpdateController")]
    public static class BallisticCompletedStepPhasePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(){return !BattleScaleRuntime.Enabled||BallisticContactRuntime.Resolving;}
    }
    [HarmonyPatch(typeof(ProjectileJobContainer),"LateUpdate")]
    public static class BallisticCompletedStepResolvePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ProjectileJobContainer __instance){BallisticContactRuntime.AfterMovement(__instance);}
    }
    [HarmonyPatch(typeof(ProjectileController),"FilterRaycastsForHits")]
    public static class ProjectileRegisteredGeometryFilterPatch
    {
        [HarmonyPrefix]
        public static void Prefix(List<RaycastHit> raycastHitsList)
        {if(BattleScaleRuntime.Enabled)raycastHitsList.RemoveAll(hit=>!BallisticContactRuntime.RegisteredContact(hit));}
    }
}
