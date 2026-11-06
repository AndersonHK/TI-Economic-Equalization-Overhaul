using HarmonyLib;
using PavonisInteractive.TerraInvicta.Jobs;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using TIEconomyMod.Core;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(ProjectileJobContainer),"RemoveProjectile")]
    public static class BallisticContainerRemovalPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ProjectileJobContainer __instance,Transform projectile){BallisticContactRuntime.RemovedTransform(__instance,projectile);}
    }
    [HarmonyPatch(typeof(ProjectileJobContainer),"UpdateControllers")]
    public static class BallisticContainerDispatchPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ProjectileJobContainer __instance,List<ProjectileReferences> ____projectileReferences)
        {
            CompatibilityContactWorld.Invalidate();
            // A purely ballistic container has no pre-movement controller work.
            // Mixed containers retain the native missile update phase.
            return !BattleScaleRuntime.Enabled||BallisticContactRuntime.Resolving||
                !BallisticContactRuntime.OwnsAllControllers(__instance,____projectileReferences.Count);
        }
    }
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
        [HarmonyPrefix]
        public static void Prefix(ProjectileJobContainer __instance,JobHandle ____jobHandle){BallisticContactRuntime.BeforeMovementComplete(__instance,____jobHandle);}
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
