using HarmonyLib;
using FullSerializer;
using PavonisInteractive.TerraInvicta;
using System;
using TIEconomyMod.Core;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIGameStateConverter), nameof(TIGameStateConverter.TryDeserialize))]
    public static class CapitalResolvedShipSchemaPatch
    {
        [HarmonyPostfix]
        public static void Postfix(fsData data, object instance)
        {
            // The native converter replaces a temporary object with its earlier
            // ID-reference placeholder. Apply metadata to that resolved object.
            if (instance is TISpaceShipState) CapitalLayoutProcessor.ReadMarker(instance, data);
        }
    }

    [HarmonyPatch(typeof(StringSerializationAPI), nameof(StringSerializationAPI.Deserialize))]
    public static class CapitalMigrationLoadSourcePatch
    {
        [HarmonyPrefix]
        public static void Prefix(Type type, string serializedState) { CapitalMountRuntime.BeginDeserialize(type, serializedState); }
    }

    [HarmonyPatch(typeof(StringSerializationAPI), nameof(StringSerializationAPI.Serialize))]
    public static class CapitalMigrationSaveGuardPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Type type) { CapitalMountRuntime.AssertCanSave(type); }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.PostGlobalGameStateCreateInit_2))]
    public static class CapitalCampaignMigrationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TIFactionState __instance) { CapitalMountRuntime.FactionInitialized(__instance); }
    }

    [HarmonyPatch(typeof(TISpaceShipState), nameof(TISpaceShipState.InitWithTemplate))]
    public static class CapitalNewShipMigrationPatch
    {
        [HarmonyPrefix]
        public static void Prefix(TIDataTemplate rawTemplate) { CapitalMountRuntime.EnsureDesign(rawTemplate as TISpaceShipTemplate); }
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipState __instance, TIDataTemplate rawTemplate)
        {
            TISpaceShipTemplate design = rawTemplate as TISpaceShipTemplate;
            if (design != null) CapitalMountRuntime.State(__instance).Schema = CapitalMountRuntime.State(design).Schema;
        }
    }

    [HarmonyPatch(typeof(TISpaceShipState), nameof(TISpaceShipState.BecomeCopyOf))]
    public static class CapitalCopiedShipSchemaPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipState __instance, TISpaceShipState shipToCopy)
        {
            CapitalMountRuntime.State(__instance).Schema = CapitalMountRuntime.State(shipToCopy).Schema;
        }
    }
}
