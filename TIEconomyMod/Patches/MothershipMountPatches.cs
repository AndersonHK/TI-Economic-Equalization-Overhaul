using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(ShipModelController), nameof(ShipModelController.BuildShip))]
    public static class CapitalMothershipGeometryPatch
    {
        private sealed class WarningState { internal bool Reported; }
        private static readonly ConditionalWeakTable<ShipModelController, WarningState> Warnings =
            new ConditionalWeakTable<ShipModelController, WarningState>();

        [HarmonyPrefix]
        public static void Prefix(ShipModelController __instance)
        {
            if (!Main.enabled || !(__instance is AlienMothershipController)) return;
            // Independent of the turret guard: append the paired nose controllers
            // before BuildWeapons resolves the appended serialized nose slots.
            EnsureNoseAnchors(__instance);
            try
            {
                Transform root = __instance.transform;
                var unique = new HashSet<Transform>();
                Transform[] top;
                Transform[] bottom;
                string reason;
                if (!TryAnchors(root, __instance.dorsalHullWeaponControllers, false, unique, out top, out reason) ||
                    !TryAnchors(root, __instance.ventralHullWeaponControllers, true, unique, out bottom, out reason))
                {
                    WarnOnce(__instance, reason);
                    return;
                }
                var topPositions = new Vector3[MothershipMountGeometry.Count];
                var bottomPositions = new Vector3[MothershipMountGeometry.Count];
                for (int i = 0; i < MothershipMountGeometry.Count; ++i)
                {
                    topPositions[i] = top[i].localPosition;
                    bottomPositions[i] = bottom[i].localPosition;
                }
                if (!MothershipMountGeometry.ValidatePositions(topPositions, bottomPositions, out reason))
                {
                    WarnOnce(__instance, reason);
                    return;
                }
                // Preflight every controller before moving any. Preserve all index,
                // rotation, scale and weapon/fire-point relationships. This runs
                // before native BuildWeapons creates orientation parents.
                for (int i = 0; i < MothershipMountGeometry.Count; ++i)
                {
                    top[i].localPosition = MothershipMountGeometry.Target(i, false);
                    bottom[i].localPosition = MothershipMountGeometry.Target(i, true);
                }
            }
            catch (Exception exception)
            {
                WarnOnce(__instance, exception.GetType().Name + ": " + exception.Message);
            }
        }

        private static void EnsureNoseAnchors(ShipModelController model)
        {
            var controllers = model.noseWeaponControllers;
            string[] names = { "Nose Base Center", "Nose Base Left", "Nose Base Dorsal", "Nose Base Right", "Nose Base Ventral" };
            if (controllers == null || (controllers.Count != 5 && controllers.Count != 7 && controllers.Count != 9))
                throw new InvalidOperationException("Mothership nose hierarchy changed: expected five authored controllers plus paired extensions.");
            for (int i = 5; i < controllers.Count; ++i)
                if (controllers[i] == null || controllers[i].baseObject == null || controllers[i].baseObject.name != MothershipMountGeometry.AddedNoseName(i - 5))
                    throw new InvalidOperationException("Mothership prepared nose controller changed at " + i);
            if (controllers.Count == 9) return;
            for (int i = 0; i < names.Length; ++i)
            {
                var controller = controllers[i];
                if (controller == null || controller.baseObject != controller.gameObject ||
                    controller.baseObject.name != names[i] || controller.weaponObject == null || controller.firePoint == null)
                    throw new InvalidOperationException("Mothership nose controller changed at " + i);
                if (!controller.baseObject.transform.IsChildOf(model.transform))
                    throw new InvalidOperationException("Mothership nose anchor was initialized before its geometry extension.");
            }
            var created = new List<GameObject>();
            try
            {
                for (int i = controllers.Count - 5; i < 4; ++i)
                {
                    // Clone the complete authored base/barrel/fire-point hierarchy;
                    // Unity remaps all serialized internal component references.
                    GameObject clone = UnityEngine.Object.Instantiate(controllers[1].baseObject, model.transform, false);
                    created.Add(clone);
                    clone.name = MothershipMountGeometry.AddedNoseName(i);
                    clone.transform.localPosition = MothershipMountGeometry.AddedNosePosition(i);
                    var controller = clone.GetComponent<ShipWeaponVisController>();
                    if (controller == null || controller.baseObject != clone || controller.weaponObject == null || controller.firePoint == null ||
                        !controller.weaponObject.transform.IsChildOf(clone.transform) ||
                        !controller.firePoint.transform.IsChildOf(controller.weaponObject.transform) ||
                        controller.weaponObject == controllers[1].weaponObject || controller.firePoint == controllers[1].firePoint)
                        throw new InvalidOperationException("Mothership nose clone did not remap its weapon hierarchy.");
                }
                foreach (GameObject clone in created) controllers.Add(clone.GetComponent<ShipWeaponVisController>());
            }
            catch
            {
                foreach (GameObject clone in created) UnityEngine.Object.Destroy(clone);
                throw;
            }
        }

        private static bool TryAnchors(Transform root, List<ShipWeaponVisController> controllers,
            bool ventral, HashSet<Transform> unique, out Transform[] anchors, out string reason)
        {
            anchors = null;
            reason = "unexpected Mothership controller hierarchy";
            if (controllers == null || controllers.Count != MothershipMountGeometry.Count) return false;
            anchors = new Transform[MothershipMountGeometry.Count];
            for (int i = 0; i < controllers.Count; ++i)
            {
                ShipWeaponVisController controller = controllers[i];
                if (controller == null || controller.baseObject == null || controller.weaponObject == null ||
                    controller.firePoint == null || controller.baseObject.name != MothershipMountGeometry.Name(i, ventral))
                    return false;
                Transform anchor = controller.baseObject.transform;
                // A reused model can already have one or more native orientation
                // parents. Move the root-relative anchor, never the aimed barrel
                // or the base's now-local-to-orientation position.
                int depth = 0;
                while (anchor.parent != root && anchor.parent != null &&
                    anchor.parent.name == "Base Orientation" && ++depth <= 8)
                    anchor = anchor.parent;
                if (anchor.parent != root || !unique.Add(anchor)) return false;
                anchors[i] = anchor;
            }
            reason = null;
            return true;
        }

        private static void WarnOnce(ShipModelController model, string reason)
        {
            WarningState state = Warnings.GetOrCreateValue(model);
            if (state.Reported) return;
            state.Reported = true;
            Main.Warn("Mothership mount geometry guard: " + reason + ". Authored layout retained where not corrected.");
        }
    }
}
