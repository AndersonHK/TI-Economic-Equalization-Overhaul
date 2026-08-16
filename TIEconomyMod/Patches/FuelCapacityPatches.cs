using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TIEconomyMod.Core;
using TMPro;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    public struct FuelCapacitySnapshot
    {
        public float HullVolume_m3;
        public float ModuleVolume_m3;
        public float CrewVolume_m3;
        public float FuelVolume_m3;
        public float PropellantDensity_kgm3;
        public float TankVolume_m3;
        public int MaximumTanks;
        public bool UsedHullFallback;
        public string MaterialLocalizationKey;
    }

    internal static class HullFuelCapacityFeature
    {
        private static readonly MethodInfo ValidPowerPlantForShipsDriveMethod =
            ResolveValidPowerPlantForShipsDriveMethod();
        private static readonly object diagnosticLock = new object();
        private static readonly HashSet<string> reportedDiagnostics =
            new HashSet<string>(StringComparer.Ordinal);

        public static bool Enabled
        {
            get
            {
                ShipBalanceSettings settings = Main.settings.shipBalance;
                return Main.FeatureEnabled(
                    settings.enabled && settings.fuelVolumeCapacityEnabled);
            }
        }

        public static bool TryGetSnapshot(
            TISpaceShipTemplate ship, out FuelCapacitySnapshot snapshot)
        {
            snapshot = default(FuelCapacitySnapshot);
            if (!Enabled || ship == null || ship.hullTemplate == null ||
                ship.driveTemplate == null)
            {
                return false;
            }

            TIShipHullTemplate hull = ship.hullTemplate;
            float hullVolume_m3 = 0f;
            bool usedFallback = Main.hullVolumes == null ||
                !Main.hullVolumes.TryGetVolume_m3(
                    hull.dataName,
                    ship.GetHullAppearanceIndex,
                    out hullVolume_m3);
            if (usedFallback)
            {
                hullVolume_m3 = Math.Max(0f, hull.volume_m3);
                ReportDiagnosticOnce(
                    "No measured main-hull volume is configured for '" +
                    hull.dataName + "' appearance " +
                    ship.GetHullAppearanceIndex +
                    "; using runtime cylinder " +
                    hullVolume_m3.ToString("0.###") + " m3.");
            }

            ShipBalanceSettings settings = Main.settings.shipBalance;
            float moduleVolume_m3 = ModuleVolume_m3(ship, settings);
            float crewVolume_m3 = Math.Max(0, ship.crewBillets) *
                Math.Max(0f, settings.crewPressurizedVolume_m3);
            float fuelVolume_m3 = ShipBalanceMath.FuelVolume_m3(
                hullVolume_m3,
                moduleVolume_m3,
                ship.crewBillets,
                settings.crewPressurizedVolume_m3);
            float density = PropellantDensityRegistry.Density_kgm3(
                ship.driveTemplate);
            float tankVolume_m3 = ShipBalanceMath.PropellantTankVolume_m3(
                TISpaceShipTemplate.propellantTankMass_tons, density);

            snapshot.HullVolume_m3 = hullVolume_m3;
            snapshot.ModuleVolume_m3 = moduleVolume_m3;
            snapshot.CrewVolume_m3 = crewVolume_m3;
            snapshot.FuelVolume_m3 = fuelVolume_m3;
            snapshot.PropellantDensity_kgm3 = density;
            snapshot.TankVolume_m3 = tankVolume_m3;
            snapshot.MaximumTanks = ShipBalanceMath.MaximumPropellantTanks(
                fuelVolume_m3, tankVolume_m3);
            snapshot.UsedHullFallback = usedFallback;
            snapshot.MaterialLocalizationKey =
                PropellantDensityRegistry.MaterialLocalizationKey(
                    ship.driveTemplate);
            return true;
        }

        public static bool Enforce(TISpaceShipTemplate ship)
        {
            FuelCapacitySnapshot snapshot;
            if (!TryGetSnapshot(ship, out snapshot) ||
                ship.propellantTanks <= snapshot.MaximumTanks)
            {
                return false;
            }

            ship.propellantTanks = snapshot.MaximumTanks;
            return true;
        }

        public static float DeltaVAtMaximumCapacity_kps(
            TISpaceShipTemplate ship)
        {
            FuelCapacitySnapshot snapshot;
            if (ship == null ||
                !TryGetSnapshot(ship, out snapshot))
            {
                return float.PositiveInfinity;
            }

            return ShipBalanceMath.DeltaVForPropellantTanks_kps(
                ship.modifiedEV_kps,
                ship.dryMass_tons(forceUpdate: false),
                TISpaceShipTemplate.propellantTankMass_tons,
                snapshot.MaximumTanks);
        }

        public static int ClampTankCount(
            TISpaceShipTemplate ship, int requestedTanks)
        {
            FuelCapacitySnapshot snapshot;
            if (!TryGetSnapshot(ship, out snapshot))
            {
                return Math.Max(0, requestedTanks);
            }
            return Math.Max(0, Math.Min(requestedTanks, snapshot.MaximumTanks));
        }

        public static bool IsPropulsionSpaceLegal(TISpaceShipTemplate ship)
        {
            if (ship == null || ship.driveTemplate == null ||
                ship.powerPlantTemplate == null)
            {
                return false;
            }

            return ship.validDriveForShipsPowerPlant(ship.driveTemplate) &&
                (bool)ValidPowerPlantForShipsDriveMethod.Invoke(
                    ship,
                    new object[] { ship.powerPlantTemplate });
        }

        private static MethodInfo ResolveValidPowerPlantForShipsDriveMethod()
        {
            MethodInfo method = AccessTools.Method(
                typeof(TISpaceShipTemplate),
                "ValidPowerPlantForShipsDrive",
                new[] { typeof(TIPowerPlantTemplate) });
            if (method == null || method.ReturnType != typeof(bool))
            {
                throw new MissingMethodException(
                    typeof(TISpaceShipTemplate).FullName,
                    "ValidPowerPlantForShipsDrive(TIPowerPlantTemplate)");
            }

            return method;
        }

        public static bool PrepareCompletedDesign(TISpaceShipTemplate ship)
        {
            if (ship == null)
            {
                return false;
            }

            Enforce(ship);
            FuelCapacitySnapshot snapshot;
            if (TryGetSnapshot(ship, out snapshot) &&
                (snapshot.MaximumTanks < 1 ||
                    ship.propellantTanks > snapshot.MaximumTanks))
            {
                return false;
            }

            return ship.propellantTanks > 0 &&
                IsPropulsionSpaceLegal(ship);
        }

        public static float ClampDeltaVTargetToCapacity(
            TISpaceShipTemplate ship, float requestedDeltaV_kps)
        {
            float maximumDeltaV_kps = DeltaVAtMaximumCapacity_kps(ship);
            return float.IsInfinity(maximumDeltaV_kps)
                ? requestedDeltaV_kps
                : Math.Min(requestedDeltaV_kps, maximumDeltaV_kps);
        }

        public static float DeltaVTargetFloorForCapacity(
            TISpaceShipTemplate ship)
        {
            return ClampDeltaVTargetToCapacity(ship, 250f);
        }

        public static void SetTankCountWithinCapacity(
            TISpaceShipTemplate ship, int requestedTanks)
        {
            if (ship != null)
            {
                ship.propellantTanks = ClampTankCount(ship, requestedTanks);
            }
        }

        public static bool TryRepairPropulsionSpace(
            TIFactionState designFaction, TISpaceShipTemplate ship)
        {
            if (IsPropulsionSpaceLegal(ship))
            {
                return true;
            }
            if (designFaction == null || ship == null ||
                ship.driveTemplate == null ||
                designFaction.allowedPowerPlants == null)
            {
                return false;
            }

            string originalDrive = ship.driveName;
            string originalPowerPlant = ship.powerPlantName;
            TIDriveTemplate startingDrive = ship.driveTemplate;
            for (int thrusters = Math.Max(1, startingDrive.thrusters);
                thrusters >= 1;
                thrusters--)
            {
                TIDriveTemplate variation = thrusters == startingDrive.thrusters
                    ? startingDrive
                    : startingDrive.GetVariation(thrusters);
                if (variation == null)
                {
                    continue;
                }

                ship.SetDriveTemplate(variation.dataName);
                TIPowerPlantTemplate best = null;
                foreach (TIPowerPlantTemplate candidate in
                    designFaction.allowedPowerPlants)
                {
                    if (candidate == null || !variation.IsCompatible(candidate))
                    {
                        continue;
                    }

                    ship.SetPowerPlantTemplate(candidate.dataName);
                    if (IsPropulsionSpaceLegal(ship) &&
                        (best == null || candidate.specificPower_tGW <
                            best.specificPower_tGW))
                    {
                        best = candidate;
                    }
                }

                if (best != null)
                {
                    ship.SetPowerPlantTemplate(best.dataName);
                    Enforce(ship);
                    return true;
                }
            }

            ship.SetDriveTemplate(originalDrive);
            ship.SetPowerPlantTemplate(originalPowerPlant);
            return false;
        }

        private static float ModuleVolume_m3(
            TISpaceShipTemplate ship, ShipBalanceSettings settings)
        {
            int utilityCells = 0;
            foreach (ModuleDataEntry module in ship.utilityModules)
            {
                UtilityFootprintKind footprint =
                    UtilityFootprintRegistry.GetFootprint(
                        module.moduleTemplate);
                utilityCells += UtilityFootprintMath.GetOffsets(
                    footprint).Count;
            }

            int hullWeaponCells = 0;
            foreach (TIShipWeaponTemplate weapon in ship.hullWeaponTemplates)
            {
                hullWeaponCells += Math.Max(1, weapon.internalSize);
            }

            int noseWeaponCells = 0;
            foreach (TIShipWeaponTemplate weapon in ship.noseWeaponTemplates)
            {
                noseWeaponCells += Math.Max(1, weapon.internalSize);
            }

            return utilityCells *
                    Math.Max(0f, settings.utilitySlotVolume_m3) +
                hullWeaponCells *
                    Math.Max(0f, settings.hullWeaponSlotVolume_m3) +
                noseWeaponCells *
                    Math.Max(0f, settings.noseWeaponSlotVolume_m3);
        }

        private static void ReportDiagnosticOnce(string diagnostic)
        {
            lock (diagnosticLock)
            {
                if (!reportedDiagnostics.Add(diagnostic))
                {
                    return;
                }
            }
            Main.Warn("Fuel-volume capacity: " + diagnostic);
        }
    }

    public struct AiShipAppearanceContextState
    {
        public bool Active;
        public TIFactionState Faction;
        public int ForcedAppearanceIndex;
    }

    internal static class AiShipAppearanceContext
    {
        [ThreadStatic]
        private static bool active;

        [ThreadStatic]
        private static TIFactionState faction;

        [ThreadStatic]
        private static int forcedAppearanceIndex;

        public static AiShipAppearanceContextState Begin(
            TIFactionState designFaction, int forcedIndex = -1)
        {
            AiShipAppearanceContextState previous =
                new AiShipAppearanceContextState
                {
                    Active = active,
                    Faction = faction,
                    ForcedAppearanceIndex = forcedAppearanceIndex
                };
            active = designFaction != null && designFaction.player != null &&
                designFaction.player.isAI;
            faction = designFaction;
            forcedAppearanceIndex = forcedIndex;
            return previous;
        }

        public static void Restore(AiShipAppearanceContextState previous)
        {
            active = previous.Active;
            faction = previous.Faction;
            forcedAppearanceIndex = previous.ForcedAppearanceIndex;
        }

        public static void Apply(TISpaceShipTemplate ship)
        {
            if (!active || ship == null || ship.driveTemplate == null ||
                faction == null)
            {
                return;
            }

            ship.hullAppearanceIndex = forcedAppearanceIndex >= 0
                ? forcedAppearanceIndex
                : ResolveVanillaAppearance(faction, ship.driveTemplate);
        }

        private static int ResolveVanillaAppearance(
            TIFactionState designFaction, TIDriveTemplate drive)
        {
            TIFactionTemplate template = designFaction.template;
            if (template == null || designFaction.IsAlienFaction)
            {
                return template == null ? 0 : template.hullIndex_default;
            }

            switch (drive.driveClassification)
            {
                case DriveClassification.Chemical:
                    return TIUtilities.GetHullAppearanceIndex(
                        template.hullIndex_chem);
                case DriveClassification.Electrothermal:
                case DriveClassification.Electromagnetic:
                case DriveClassification.Electrostatic:
                    return TIUtilities.GetHullAppearanceIndex(
                        template.hullIndex_electric);
                case DriveClassification.Fission_Thermal:
                case DriveClassification.Fission_Pulse:
                case DriveClassification.NuclearSaltWater:
                    return TIUtilities.GetHullAppearanceIndex(
                        template.hullIndex_fission);
                case DriveClassification.Fusion_Thermal:
                case DriveClassification.Fusion_Pulse:
                    return TIUtilities.GetHullAppearanceIndex(
                        drive.powerRequirement_GW <= 100f
                            ? template.hullIndex_fusion
                            : template.hullIndex_fusion_adv);
                case DriveClassification.Antimatter:
                    return TIUtilities.GetHullAppearanceIndex(
                        template.hullIndex_amat);
                default:
                    return template.hullIndex_default;
            }
        }
    }

    internal static class FuelCapacityDesignerUi
    {
        private const string OverlayName = "EEO_HullFuelCapacityOverlay";
        private static readonly FieldInfo slotDictionary = AccessTools.Field(
            typeof(FleetsScreenController), "shipModuleSlotDictionary");

        public static void EnforceAndRefreshSpinner(
            FleetsScreenController controller)
        {
            if (controller == null || controller.newShipTemplate == null)
            {
                return;
            }

            bool changed = HullFuelCapacityFeature.Enforce(
                controller.newShipTemplate);
            if (changed)
            {
                controller.changesMadeToExistingClass = true;
            }
            RefreshSpinner(controller);
        }

        public static void RefreshSpinner(FleetsScreenController controller)
        {
            if (controller == null || controller.newShipTemplate == null ||
                controller.newShipTemplate.hullTemplate == null ||
                slotDictionary == null)
            {
                return;
            }

            Dictionary<Vector2Int, ShipModuleDragDestination> destinations =
                slotDictionary.GetValue(controller) as
                    Dictionary<Vector2Int, ShipModuleDragDestination>;
            if (destinations == null)
            {
                return;
            }

            Vector2Int coordinates = controller.newShipTemplate.hullTemplate
                .GetUniqueSlotCoordinates(ShipModuleSlotType.Propellant);
            ShipModuleDragDestination destination;
            if (destinations.TryGetValue(coordinates, out destination) &&
                destination != null)
            {
                destination.UpdateSpinnerValue(
                    controller.newShipTemplate.propellantTanks);
            }
        }

        public static void RefreshOverlay(FleetsScreenController controller)
        {
            if (controller == null || controller.newShipTemplate == null ||
                controller.newShipTemplate.hullTemplate == null ||
                controller.shipImageSpaceBackground == null)
            {
                return;
            }

            TMP_Text label = GetOrCreateOverlay(controller);
            if (label == null)
            {
                return;
            }

            TISpaceShipTemplate ship = controller.newShipTemplate;
            TIShipHullTemplate hull = ship.hullTemplate;
            int appearanceIndex = ship.GetHullAppearanceIndex;
            float deLavalScale;
            float magneticScale;
            GetGraphicalDriveScales(
                hull, appearanceIndex, out deLavalScale, out magneticScale);
            bool usedBayFallback;
            string ignoredSizeBand;
            float engineBayVolume_m3 = ShipBalanceMath.ReactorBayVolume_m3(
                hull.dataName,
                appearanceIndex,
                hull.smallHull,
                hull.mediumHull,
                hull.largeHull,
                hull.hugeHull,
                out usedBayFallback,
                out ignoredSizeBand);
            string firstLine = Loc.T(
                "UI.Fleets.HullModelStats",
                deLavalScale.ToString("0.###"),
                magneticScale.ToString("0.###"),
                engineBayVolume_m3.ToString("N1"),
                HullVariantEmptyMassFeature.EmptyHullMass_tons(ship)
                    .ToString("N0"),
                hull.crew.ToString("N0"));

            FuelCapacitySnapshot snapshot;
            string secondLine;
            if (HullFuelCapacityFeature.TryGetSnapshot(ship, out snapshot))
            {
                secondLine = Loc.T(
                    "UI.Fleets.HullFuelStats",
                    Loc.T(snapshot.MaterialLocalizationKey),
                    ship.propellantTanks.ToString("N0"),
                    snapshot.MaximumTanks.ToString("N0"),
                    snapshot.FuelVolume_m3.ToString("N0"));
            }
            else
            {
                secondLine = Loc.T("UI.Fleets.HullFuelStatsNoDrive");
            }
            label.SetText(firstLine + "\n" + secondLine);
            label.gameObject.SetActive(HullFuelCapacityFeature.Enabled);
            label.transform.SetAsLastSibling();
        }

        private static void GetGraphicalDriveScales(
            TIShipHullTemplate hull,
            int appearanceIndex,
            out float deLaval,
            out float magnetic)
        {
            deLaval = HullDriveScalingFeature.GraphicalMultiplier(
                hull.dataName, hull.alien, appearanceIndex, "DeLaval");
            magnetic = HullDriveScalingFeature.GraphicalMultiplier(
                hull.dataName, hull.alien, appearanceIndex, "Magnetic");
        }

        public static void RefreshSpinnerLabel(
            ShipModuleDragDestination destination)
        {
            if (destination == null ||
                destination.shipModuleSlotType !=
                    ShipModuleSlotType.Propellant ||
                destination.spinnerValueText == null)
            {
                return;
            }

            FleetsScreenController controller =
                destination.FleetsScreenController;
            FuelCapacitySnapshot snapshot;
            if (controller == null ||
                !HullFuelCapacityFeature.TryGetSnapshot(
                    controller.newShipTemplate, out snapshot))
            {
                return;
            }

            destination.spinnerValueText.SetText(Loc.T(
                "UI.Fleets.PropellantTanksWithCap",
                controller.newShipTemplate.propellantTanks.ToString("N0"),
                snapshot.MaximumTanks.ToString("N0")));
        }

        private static TMP_Text GetOrCreateOverlay(
            FleetsScreenController controller)
        {
            Transform parent = controller.shipImageSpaceBackground.parent;
            if (parent == null)
            {
                return null;
            }

            Transform existing = parent.Find(OverlayName);
            if (existing != null)
            {
                return existing.GetComponent<TMP_Text>();
            }

            GameObject overlayObject = new GameObject(
                OverlayName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            overlayObject.transform.SetParent(parent, false);
            TextMeshProUGUI label =
                overlayObject.GetComponent<TextMeshProUGUI>();
            TMP_Text source = controller.designerCrewText;
            if (source != null)
            {
                label.font = source.font;
                label.fontSharedMaterial = source.fontSharedMaterial;
                label.color = source.color;
                label.fontStyle = source.fontStyle;
                label.fontSize = source.fontSize;
            }
            label.alignment = TextAlignmentOptions.TopLeft;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12f;
            label.fontSizeMax = source == null
                ? 18f
                : Math.Max(12f, source.fontSize);

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -8f);
            rect.sizeDelta = new Vector2(-120f, 58f);
            return label;
        }
    }

    [HarmonyPatch(typeof(TemplateManager), "Initialize")]
    public static class PropellantDensityTemplateInitializationPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            PropellantDensityRegistry.Refresh();
        }
    }

    [HarmonyPatch(
        typeof(FleetsScreenController),
        nameof(FleetsScreenController.UpdateShipDesignDataPanelAndImage),
        new[] { typeof(bool), typeof(bool), typeof(bool) })]
    public static class FuelCapacityDesignerRefreshPatch
    {
        [HarmonyPrefix]
        public static void Prefix(FleetsScreenController __instance)
        {
            FuelCapacityDesignerUi.EnforceAndRefreshSpinner(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(FleetsScreenController __instance)
        {
            FuelCapacityDesignerUi.RefreshOverlay(__instance);
        }
    }

    [HarmonyPatch(
        typeof(ShipModuleDragDestination),
        nameof(ShipModuleDragDestination.UpdateSpinnerValue),
        new[] { typeof(int) })]
    public static class FuelCapacitySpinnerLabelPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipModuleDragDestination __instance)
        {
            FuelCapacityDesignerUi.RefreshSpinnerLabel(__instance);
        }
    }

    [HarmonyPatch(typeof(FleetsScreenController), "SaveDesign")]
    public static class FuelCapacitySaveGuardPatch
    {
        [HarmonyPrefix]
        public static void Prefix(FleetsScreenController __instance)
        {
            FuelCapacityDesignerUi.EnforceAndRefreshSpinner(__instance);
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.DesignShip))]
    public static class AiShipDesignCapacityBoundaryPatch
    {
        [HarmonyPrefix]
        private static void Prefix(
            TIFactionState __instance,
            out AiShipAppearanceContextState __state)
        {
            __state = AiShipAppearanceContext.Begin(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(
            ref TIFactionState.ShipDesignerOutcome __result,
            ref TISpaceShipTemplate design)
        {
            if (__result == TIFactionState.ShipDesignerOutcome.Success &&
                !HullFuelCapacityFeature.PrepareCompletedDesign(design))
            {
                design = null;
                __result =
                    TIFactionState.ShipDesignerOutcome.NoScoredDesigns;
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            AiShipAppearanceContextState __state)
        {
            AiShipAppearanceContext.Restore(__state);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.DesignRefit))]
    public static class AiShipRefitCapacityBoundaryPatch
    {
        [HarmonyPrefix]
        private static void Prefix(
            TIFactionState __instance,
            TISpaceShipTemplate original,
            out AiShipAppearanceContextState __state)
        {
            __state = AiShipAppearanceContext.Begin(
                __instance,
                original == null ? -1 : original.GetHullAppearanceIndex);
        }

        [HarmonyPostfix]
        public static void Postfix(ref TISpaceShipTemplate __result)
        {
            if (__result != null &&
                !HullFuelCapacityFeature.PrepareCompletedDesign(__result))
            {
                __result = null;
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(
            Exception __exception,
            AiShipAppearanceContextState __state)
        {
            AiShipAppearanceContext.Restore(__state);
            return __exception;
        }
    }

    [HarmonyPatch(
        typeof(TISpaceShipTemplate),
        nameof(TISpaceShipTemplate.SetDriveTemplate),
        new[] { typeof(string) })]
    public static class AiShipEarlyAppearanceSelectionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(TISpaceShipTemplate __instance)
        {
            AiShipAppearanceContext.Apply(__instance);
        }
    }

    [HarmonyPatch]
    public static class FuelCapacityIdealTankCountPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(TISpaceShipTemplate),
                nameof(TISpaceShipTemplate.GetIdealPropellentTankCount),
                new[]
                {
                    typeof(float),
                    typeof(float).MakeByRefType(),
                    typeof(float),
                    typeof(float)
                });
        }

        [HarmonyPostfix]
        public static void Postfix(
            TISpaceShipTemplate __instance,
            ref int __result,
            ref float actualDV)
        {
            int clamped = HullFuelCapacityFeature.ClampTankCount(
                __instance, __result);
            if (clamped == __result)
            {
                return;
            }

            __result = clamped;
            actualDV = ShipBalanceMath.DeltaVForPropellantTanks_kps(
                __instance.modifiedEV_kps,
                __instance.dryMass_tons(forceUpdate: false),
                TISpaceShipTemplate.propellantTankMass_tons,
                clamped);
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.DesignAlienShip))]
    public static class AlienShipFuelCapacityPatch
    {
        private static readonly FieldInfo PropellantTanksField =
            AccessTools.Field(
                typeof(TISpaceShipTemplate),
                nameof(TISpaceShipTemplate.propellantTanks));
        private static readonly MethodInfo ClampTargetMethod =
            AccessTools.Method(
                typeof(HullFuelCapacityFeature),
                nameof(HullFuelCapacityFeature.ClampDeltaVTargetToCapacity));
        private static readonly MethodInfo SetTankCountMethod =
            AccessTools.Method(
                typeof(HullFuelCapacityFeature),
                nameof(HullFuelCapacityFeature.SetTankCountWithinCapacity));
        private static readonly MethodInfo TargetFloorMethod =
            AccessTools.Method(
                typeof(HullFuelCapacityFeature),
                nameof(HullFuelCapacityFeature.DeltaVTargetFloorForCapacity));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            CodeInstruction targetLoad;
            CodeInstruction targetStore;
            int targetFloorIndex;
            FindDeltaVTargetLocal(
                patched,
                out targetLoad,
                out targetStore,
                out targetFloorIndex);

            CodeInstruction loadDesign =
                new CodeInstruction(OpCodes.Ldarg_2);
            loadDesign.labels.AddRange(patched[targetFloorIndex].labels);
            loadDesign.blocks.AddRange(patched[targetFloorIndex].blocks);
            patched[targetFloorIndex] = loadDesign;
            patched.Insert(targetFloorIndex + 1,
                new CodeInstruction(OpCodes.Ldind_Ref));
            patched.Insert(targetFloorIndex + 2,
                new CodeInstruction(OpCodes.Call, TargetFloorMethod));

            int tankStores = 0;
            int targetInsertions = 0;
            for (int index = 0; index < patched.Count; index++)
            {
                if (patched[index].opcode != OpCodes.Stfld ||
                    !Equals(patched[index].operand, PropellantTanksField))
                {
                    continue;
                }

                bool initialTankAssignment = index > 0 &&
                    LoadsIntegerOne(patched[index - 1]);
                CodeInstruction replacement = new CodeInstruction(
                    OpCodes.Call, SetTankCountMethod);
                replacement.labels.AddRange(patched[index].labels);
                replacement.blocks.AddRange(patched[index].blocks);
                patched[index] = replacement;
                tankStores++;

                if (!initialTankAssignment)
                {
                    continue;
                }

                patched.Insert(index + 1,
                    new CodeInstruction(OpCodes.Ldarg_2));
                patched.Insert(index + 2,
                    new CodeInstruction(OpCodes.Ldind_Ref));
                patched.Insert(index + 3,
                    new CodeInstruction(targetLoad.opcode, targetLoad.operand));
                patched.Insert(index + 4,
                    new CodeInstruction(OpCodes.Call, ClampTargetMethod));
                patched.Insert(index + 5,
                    new CodeInstruction(targetStore.opcode, targetStore.operand));
                index += 5;
                targetInsertions++;
            }

            if (tankStores != 3 || targetInsertions != 1)
            {
                string message =
                    "Alien ship-design IL changed: expected three tank " +
                    "assignments and one initial target clamp. Refusing a " +
                    "partial fuel-capacity patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }

        private static void FindDeltaVTargetLocal(
            IList<CodeInstruction> instructions,
            out CodeInstruction targetLoad,
            out CodeInstruction targetStore,
            out int targetFloorIndex)
        {
            for (int index = 0; index < instructions.Count; index++)
            {
                CodeInstruction instruction = instructions[index];
                if (instruction.opcode != OpCodes.Ldc_R4 ||
                    !(instruction.operand is float) ||
                    (float)instruction.operand != 250f)
                {
                    continue;
                }

                CodeInstruction load = null;
                CodeInstruction store = null;
                int end = Math.Min(instructions.Count, index + 14);
                for (int scan = index + 1; scan < end; scan++)
                {
                    if (load == null && IsLoadLocal(instructions[scan]))
                    {
                        load = instructions[scan];
                    }
                    else if (load != null &&
                        IsMatchingStoreLocal(load, instructions[scan]))
                    {
                        store = instructions[scan];
                    }
                }

                if (load != null && store != null)
                {
                    targetLoad = load;
                    targetStore = store;
                    targetFloorIndex = index;
                    return;
                }
            }

            throw new InvalidOperationException(
                "Alien ship-design IL changed: could not identify the " +
                "delta-v target local near its 250 kps floor.");
        }

        private static bool IsLoadLocal(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Ldloc ||
                instruction.opcode == OpCodes.Ldloc_S ||
                instruction.opcode == OpCodes.Ldloc_0 ||
                instruction.opcode == OpCodes.Ldloc_1 ||
                instruction.opcode == OpCodes.Ldloc_2 ||
                instruction.opcode == OpCodes.Ldloc_3;
        }

        private static bool IsMatchingStoreLocal(
            CodeInstruction load, CodeInstruction store)
        {
            if (load.opcode == OpCodes.Ldloc_0)
            {
                return store.opcode == OpCodes.Stloc_0;
            }
            if (load.opcode == OpCodes.Ldloc_1)
            {
                return store.opcode == OpCodes.Stloc_1;
            }
            if (load.opcode == OpCodes.Ldloc_2)
            {
                return store.opcode == OpCodes.Stloc_2;
            }
            if (load.opcode == OpCodes.Ldloc_3)
            {
                return store.opcode == OpCodes.Stloc_3;
            }
            return (store.opcode == OpCodes.Stloc ||
                    store.opcode == OpCodes.Stloc_S) &&
                Equals(load.operand, store.operand);
        }

        private static bool LoadsIntegerOne(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Ldc_I4_1 ||
                (instruction.opcode == OpCodes.Ldc_I4 &&
                    Equals(instruction.operand, 1)) ||
                (instruction.opcode == OpCodes.Ldc_I4_S &&
                    Convert.ToInt32(instruction.operand) == 1);
        }
    }

    [HarmonyPatch(typeof(TIFactionState), nameof(TIFactionState.DesignSTOFighter))]
    public static class StoFighterFuelCapacityPatch
    {
        [HarmonyPrefix]
        public static void Prefix(
            TIFactionState __instance,
            out AiShipAppearanceContextState __state)
        {
            __state = AiShipAppearanceContext.Begin(__instance);
        }

        [HarmonyPostfix]
        public static void Postfix(
            TIFactionState __instance,
            TISpaceShipTemplate __result)
        {
            if (__result == null)
            {
                return;
            }

            HullFuelCapacityFeature.Enforce(__result);
            if (!HullFuelCapacityFeature.TryRepairPropulsionSpace(
                __instance, __result))
            {
                Main.Warn(
                    "STO fighter designer could not find a drive/reactor " +
                    "combination that fits the selected engine bay.");
            }
        }

        [HarmonyFinalizer]
        public static Exception Finalizer(
            Exception __exception,
            AiShipAppearanceContextState __state)
        {
            AiShipAppearanceContext.Restore(__state);
            return __exception;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo propellantTanks = AccessTools.Field(
                typeof(TISpaceShipTemplate),
                nameof(TISpaceShipTemplate.propellantTanks));
            MethodInfo setter = AccessTools.Method(
                typeof(HullFuelCapacityFeature),
                nameof(HullFuelCapacityFeature.SetTankCountWithinCapacity));
            List<CodeInstruction> patched =
                new List<CodeInstruction>(instructions);
            int replacements = 0;
            for (int index = 0; index < patched.Count; index++)
            {
                CodeInstruction instruction = patched[index];
                if (instruction.opcode != OpCodes.Stfld ||
                    !Equals(instruction.operand, propellantTanks))
                {
                    continue;
                }

                CodeInstruction replacement =
                    new CodeInstruction(OpCodes.Call, setter);
                replacement.labels.AddRange(instruction.labels);
                replacement.blocks.AddRange(instruction.blocks);
                patched[index] = replacement;
                replacements++;
            }

            if (replacements != 1)
            {
                string message =
                    "STO fighter-design IL changed: expected one tank " +
                    "increment. Refusing a partial fuel-capacity patch.";
                Main.Warn(message);
                throw new InvalidOperationException(message);
            }
            return patched;
        }
    }

    [HarmonyPatch(typeof(SaveShipDesignAction), nameof(SaveShipDesignAction.Execute))]
    public static class SavedShipCapacityInvariantPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(SaveShipDesignAction __instance)
        {
            TISpaceShipTemplate ship = __instance.shipDesign;
            if (!HullFuelCapacityFeature.PrepareCompletedDesign(ship))
            {
                Main.Warn(
                    "Rejected a generated ship design that exceeded its " +
                    "fuel, reactor, or engine-bay capacity.");
                return false;
            }

            ship.CacheTemplateValues();
            return true;
        }
    }
}
