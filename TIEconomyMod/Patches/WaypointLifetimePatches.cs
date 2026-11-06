using System.Collections.Generic;
using System;
using System.Reflection;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(WaypointNavigationController),MethodType.Constructor,new Type[]{typeof(string),typeof(int),typeof(Vector3),typeof(Vector3),typeof(TIDateTime),typeof(WaypointSharedData),typeof(TISpaceShipState),typeof(Vector3),typeof(Camera),typeof(CombatShipController)})]
    public static class WaypointLifetimeRegistrationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(WaypointNavigationController __instance){WaypointLifetime.Register(__instance);}
    }
    [HarmonyPatch(typeof(WaypointNavigationController),"CleanUpWaypoints")]
    public static class WaypointLifetimeCleanupPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(WaypointNavigationController __instance){WaypointLifetime.Retire(__instance);return false;}
    }
    [HarmonyPatch(typeof(WaypointController),"Destroy")]
    public static class WaypointControllerDisposePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(WaypointController __instance){WaypointLifetime.DestroyController(__instance);return false;}
    }
    [HarmonyPatch(typeof(WaypointNavigationController),"EndInputHandling")]
    public static class WaypointStaleInputPatch
    {
        [HarmonyPrefix]
        public static void Prefix(){WaypointLifetime.DropStaleInput();}
    }
    [HarmonyPatch]
    public static class WaypointBattleBoundaryPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(SpaceCombatManager),"Initialize");
            yield return AccessTools.Method(typeof(SpaceCombatManager),"EndCombat");
            yield return AccessTools.Method(typeof(SpaceCombatManager),"OnDisable");
        }
        [HarmonyPrefix,HarmonyPriority(Priority.First)]
        public static void Prefix(SpaceCombatManager __instance,MethodBase __originalMethod)
        {
            if(__originalMethod.Name=="Initialize"&&!__instance.initialized)WaypointLifetime.ResetBattle(__instance);
            else WaypointLifetime.DropStaleInput();
        }
        [HarmonyPostfix]
        public static void Postfix(SpaceCombatManager __instance,MethodBase __originalMethod)
        {if(__originalMethod.Name!="Initialize")WaypointLifetime.ResetBattle(__instance);}
    }
}
