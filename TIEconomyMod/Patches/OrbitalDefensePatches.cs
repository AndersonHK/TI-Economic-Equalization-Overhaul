using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    [HarmonyPatch(typeof(TIHabModuleTemplate), nameof(TIHabModuleTemplate.NotionalWeaponsList))]
    internal static class OrbitalDefensePreviewPatch
    {
        private static bool Prefix(TIHabModuleTemplate __instance, TIFactionState faction, bool isBase,
            TISpaceBodyState spacebody, bool includePD, ref List<TIShipWeaponTemplate> __result)
        {
            if (faction == null || !OrbitalDefense.TryGet(__instance, isBase, out OrbitalDefenseLayout layout))
                return true;
            __result = OrbitalDefense.Weapons(layout, faction, spacebody, includePD);
            return false;
        }
    }

    internal static class OrbitalDefenseSummary
    {
        internal static void Group(TIHabModuleState module, ref string summary)
        {
            if (module?.ref_faction == null || string.IsNullOrEmpty(summary) ||
                !OrbitalDefense.TryGet(module.moduleTemplate, module.hab.IsBase, out OrbitalDefenseLayout layout))
                return;
            var counts = OrbitalDefense.Weapons(layout, module.ref_faction, module.hab.ref_spaceBody)
                .GroupBy(weapon => weapon.displayName).ToDictionary(group => group.Key, group => group.Count());
            var rendered = new HashSet<string>(StringComparer.Ordinal);
            var output = new StringBuilder(summary.Length);
            using (var lines = new System.IO.StringReader(summary))
            {
                string line;
                while ((line = lines.ReadLine()) != null)
                {
                    if (!counts.TryGetValue(line, out int count) || count == 1)
                        output.AppendLine(line);
                    else if (rendered.Add(line))
                        output.Append(count).Append(" × ").AppendLine(line);
                }
            }
            summary = output.ToString();
        }
    }

    [HarmonyPatch(typeof(TIHabModuleState), nameof(TIHabModuleState.GetCombatSummary))]
    internal static class OrbitalDefenseCombatSummaryPatch
    {
        private static void Postfix(TIHabModuleState __instance, ref string __result) =>
            OrbitalDefenseSummary.Group(__instance, ref __result);
    }

    [HarmonyPatch(typeof(TIHabModuleState), nameof(TIHabModuleState.FullSummary))]
    internal static class OrbitalDefenseFullSummaryPatch
    {
        private static void Postfix(TIHabModuleState habModule, ref string __result) =>
            OrbitalDefenseSummary.Group(habModule, ref __result);
    }

    [HarmonyPatch(typeof(TIProjectTemplate), nameof(TIProjectTemplate.BenefitsDescription))]
    internal static class OrbitalDefenseResearchPatch
    {
        private static void Postfix(TIProjectTemplate __instance, TIFactionState faction,
            TechBenefitsContext benefitsContext, ref string __result)
        {
            if (faction == null || benefitsContext != TechBenefitsContext.JustCompleted)
                return;
            var additions = __instance.ShipPartUnlocks.OfType<TIShipWeaponTemplate>().ToList();
            if (additions.Count == 0)
                return;
            var notices = new Dictionary<string, bool>();
            foreach (TIShipWeaponTemplate weapon in additions)
            {
                for (int tier = 1; tier <= 3; tier++)
                    notices[Loc.T("UI.Science.StationWeapon", weapon.displayName, tier.ToString())] =
                        OrbitalDefense.IsResearchUpgrade(faction, tier, weapon, additions);
            }
            var lines = (__result ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Where(line => !notices.ContainsKey(line)).ToList();
            lines.AddRange(notices.Where(notice => notice.Value).Select(notice => notice.Key));
            __result = string.Join(Environment.NewLine, lines);
        }
    }

    [HarmonyPatch(typeof(SimulatedCombat.SimulatedCombatHabModule), MethodType.Constructor,
        new[] { typeof(SimulatedCombat), typeof(TIHabModuleState) })]
    internal static class OrbitalDefenseAutoresolvePatch
    {
        private static void Postfix(SimulatedCombat.SimulatedCombatHabModule __instance, TIHabModuleState __1,
            ref List<SimulatedCombat.SimulatedHabWeapon> ___weapons)
        {
            if (!OrbitalDefense.TryGet(__1.moduleTemplate, __1.hab.IsBase, out OrbitalDefenseLayout layout))
                return;
            List<TIShipWeaponTemplate> weapons = OrbitalDefense.Weapons(layout, __1.ref_faction, __1.hab.ref_spaceBody);
            ___weapons = weapons.Select((weapon, index) => new SimulatedCombat.SimulatedHabWeapon(
                __instance, weapon, layout.Mounts[index].IsPointDefense ? FireMode.Defense : FireMode.Offense)).ToList();
        }
    }

    [HarmonyPatch(typeof(CombatHabModuleController), nameof(CombatHabModuleController.InitializeForCombat))]
    internal static class OrbitalDefenseInitializePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> source, ILGenerator generator)
        {
            var instructions = source.ToList();
            FieldInfo armor = AccessTools.Field(typeof(CombatHabModuleController), nameof(CombatHabModuleController.armor));
            int at = instructions.FindIndex(instruction => instruction.StoresField(armor));
            if (at < 0 || at + 1 >= instructions.Count)
                throw new InvalidOperationException("Could not locate the orbital defense initializer after armor setup.");
            Label original = generator.DefineLabel();
            instructions[at + 1].labels.Add(original);
            instructions.InsertRange(at + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbitalDefenseCombat), nameof(OrbitalDefenseCombat.Build))),
                new CodeInstruction(OpCodes.Brfalse, original),
                new CodeInstruction(OpCodes.Ret)
            });
            return instructions;
        }
    }

    internal static class OrbitalDefenseCombat
    {
        private sealed class ModuleModels
        {
            private struct CachedSegment
            {
                internal Vector3 Start, End;
                internal bool Valid, Blocked;
            }

            private static readonly ConditionalWeakTable<Mesh, RenderedMeshRaycast.ShellQuery> shells =
                new ConditionalWeakTable<Mesh, RenderedMeshRaycast.ShellQuery>();
            internal readonly List<GameObject> Objects = new List<GameObject>();
            internal MeshFilter Hull;
            private Mesh rayMesh;
            private RenderedMeshRaycast.ShellQuery shell;
            private readonly CachedSegment[] segments = new CachedSegment[2048];

            internal void PrepareShell(Mesh mesh)
            {
                shell = shells.GetValue(mesh, PrepareGeometry);
                rayMesh = mesh;
                Array.Clear(segments, 0, segments.Length);
            }

            private static RenderedMeshRaycast.ShellQuery PrepareGeometry(Mesh mesh)
            {
                Mesh readable = mesh.isReadable ? mesh : CombatUnreadableMeshes.Copy(mesh);
                return new RenderedMeshRaycast.ShellQuery(RenderedMeshRaycast.For(readable));
            }

            internal bool Blocks(Vector3 start, Vector3 end)
            {
                Mesh mesh = Hull.sharedMesh;
                if (rayMesh != mesh) PrepareShell(mesh);
                Transform hull = Hull.transform;
                Vector3 a = hull.InverseTransformPoint(start);
                Vector3 b = hull.InverseTransformPoint(end);

                // Equality checks both actual local endpoints. Hash collisions only evict
                // entries. Rotation, target motion and aligned muzzle changes stay visible.
                uint hash = unchecked((uint)(a.GetHashCode() * 397 ^ b.GetHashCode()));
                int slot = (int)(unchecked(hash * 2654435761u) >> 21);
                ref CachedSegment cached = ref segments[slot];
                if (!cached.Valid || !cached.Start.Equals(a) || !cached.End.Equals(b))
                {
                    cached = new CachedSegment { Start = a, End = b, Blocked = shell.Blocked(a, b), Valid = true };
                }
                return cached.Blocked;
            }
        }

        private static readonly ConditionalWeakTable<CombatHabModuleController, ModuleModels> models =
            new ConditionalWeakTable<CombatHabModuleController, ModuleModels>();
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<SphereCollider>> combatColliders =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<SphereCollider>>("<combatHitColliders>k__BackingField");
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<Collider>> genericColliders =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<Collider>>("genericColliders");
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<ModuleDataEntry>> entries =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<ModuleDataEntry>>("<weaponDataEntries>k__BackingField");
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<IWeapon>> weapons =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<IWeapon>>("<weapons>k__BackingField");
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<ShipWeaponVisController>> dorsal =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<ShipWeaponVisController>>("<dorsalWeaponControllers>k__BackingField");
        private static readonly AccessTools.FieldRef<CombatHabModuleController, List<ShipWeaponVisController>> ventral =
            AccessTools.FieldRefAccess<CombatHabModuleController, List<ShipWeaponVisController>>("<ventralWeaponControllers>k__BackingField");
        private static Mesh collarMesh;

        internal static bool Build(CombatHabModuleController controller)
        {
            TIHabModuleState module = controller.habModule;
            if (!OrbitalDefense.TryGet(module.moduleTemplate, module.hab.IsBase, out OrbitalDefenseLayout layout))
                return false;

            List<TIShipWeaponTemplate> loadout = OrbitalDefense.Weapons(layout, module.ref_faction, module.hab.ref_spaceBody);
            entries(controller) = loadout.Select((weapon, index) => new ModuleDataEntry(weapon, index)).ToList();
            if (GameControl.spaceCombat.combatState.autoresolve)
                return true;

            RemoveModels(controller);
            controller.transform.SetLayer(LayerMask.NameToLayer("HurtBox"), includeChildren: false);
            SphereCollider sphere = controller.gameObject.AddComponent<SphereCollider>();
            sphere.radius = module.moduleTemplate.tier * 480f * GameControl.spaceCombat.modelScalingFactor;
            combatColliders(controller) = new List<SphereCollider> { sphere };
            genericColliders(controller) = controller.GetComponents<Collider>().ToList();
            controller.alliedCombatants = new List<CombatantController>();
            controller.enemyCombatants = new List<CombatantController>();
            dorsal(controller) = new List<ShipWeaponVisController>(loadout.Count);
            ventral(controller) = new List<ShipWeaponVisController>(loadout.Count);
            weapons(controller) = new List<IWeapon>(loadout.Count);

            var state = new ModuleModels { Hull = controller.GetComponent<MeshFilter>() };
            if (state.Hull == null || state.Hull.sharedMesh == null)
                throw new InvalidOperationException("Missing defense shell on " + module.moduleTemplate.dataName);
            state.PrepareShell(state.Hull.sharedMesh);
            models.Add(controller, state);
            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            for (int i = 0; i < loadout.Count; i++)
            {
                TIShipWeaponTemplate weapon = loadout[i];
                OrbitalMount mount = layout.Mounts[i];
                if (!prefabs.TryGetValue(weapon.modelResource, out GameObject prefab))
                {
                    prefab = GameControl.assetLoader.LoadAsset<GameObject>(weapon.modelResource);
                    prefabs.Add(weapon.modelResource, prefab);
                }
                float scale = layout.ScaleFor(mount, weapon.modelResource);
                dorsal(controller).Add(Place(state, controller, prefab, weapon, mount.Top, i, scale, mount.IsPointDefense));
                ventral(controller).Add(Place(state, controller, prefab, weapon, mount.Bottom, i, scale, mount.IsPointDefense));
                ModuleDataEntry entry = entries(controller)[i];
                Weapon instance = weapon is TIBeamWeaponTemplate
                    ? (Weapon)new BeamWeapon(controller, entry, i)
                    : new ProjectileWeapon(controller, entry, i);
                FireMode mode = mount.IsPointDefense ? FireMode.Defense : FireMode.Offense;
                instance.currentFireMode = instance.fireModes.FirstOrDefault(fireMode => fireMode.mode == mode)
                    ?? instance.currentFireMode;
                weapons(controller).Add(instance);
            }
            return true;
        }

        private static ShipWeaponVisController Place(ModuleModels state, CombatHabModuleController controller,
            GameObject prefab, TIShipWeaponTemplate weapon, OrbitalMountPose pose, int index, float scale, bool pd)
        {
            GameObject model = UnityEngine.Object.Instantiate(prefab, controller.transform);
            Transform root = model.transform;
            root.localPosition = pose.Position;
            root.localRotation = pose.Rotation;
            root.localScale *= scale;
            var visual = model.AddComponent<ShipWeaponVisController>();
            visual.Initialize(controller, model, weapon, index);
            model.SetActive(true);
            state.Objects.Add(root.parent.gameObject); // Initialize creates the aiming parent.
            AddCollar(state, controller.transform, pose, model.GetComponent<MeshRenderer>().sharedMaterial, pd);
            return visual;
        }

        private static void AddCollar(ModuleModels state, Transform parent, OrbitalMountPose pose, Material material, bool pd)
        {
            Vector3 lift = pose.Position - pose.Surface;
            if (lift.sqrMagnitude < 1e-6f)
                return;
            var collar = new GameObject("Orbital turret collar");
            collar.transform.SetParent(parent, false);
            collar.transform.localPosition = (pose.Position + pose.Surface) * 0.5f;
            collar.transform.localRotation = pose.Rotation;
            float radius = pd ? 0.20f : 0.34f;
            collar.transform.localScale = new Vector3(radius, lift.magnitude, radius);
            collar.AddComponent<MeshFilter>().sharedMesh = GetCollarMesh();
            collar.AddComponent<MeshRenderer>().sharedMaterial = material;
            state.Objects.Add(collar);
        }

        private static Mesh GetCollarMesh()
        {
            if (collarMesh != null)
                return collarMesh;
            const int sides = 12;
            var vertices = new Vector3[2 * sides];
            var indices = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float angle = i * 2f * Mathf.PI / sides;
                vertices[i] = new Vector3(Mathf.Sin(angle), -0.5f, Mathf.Cos(angle));
                vertices[i + sides] = vertices[i] + Vector3.up;
                int next = (i + 1) % sides;
                indices.AddRange(new[] { i, next, i + sides, next, next + sides, i + sides });
            }
            collarMesh = new Mesh { name = "Orbital turret mounting collar", vertices = vertices, triangles = indices.ToArray() };
            collarMesh.RecalculateNormals();
            return collarMesh;
        }

        internal static void RemoveModels(CombatHabModuleController controller)
        {
            if (!models.TryGetValue(controller, out ModuleModels state))
                return;
            foreach (GameObject instance in state.Objects)
            {
                if (instance != null)
                {
                    instance.SetActive(false);
                    UnityEngine.Object.Destroy(instance);
                }
            }
            models.Remove(controller);
        }

        internal static bool Managed(Weapon weapon)
        {
            return weapon.combatant is CombatHabModuleController controller && models.TryGetValue(controller, out _);
        }

        internal static ShipWeaponVisController SelectFace(Weapon weapon, Vector3 target, out bool possible)
        {
            ShipWeaponVisController a = weapon.weaponVisualization;
            ShipWeaponVisController b = weapon.altWeaponVisualization;
            float elevationA = Elevation(a, target), elevationB = Elevation(b, target);
            if (elevationB > elevationA)
            {
                a = b;
                b = weapon.weaponVisualization;
                float swap = elevationA;
                elevationA = elevationB;
                elevationB = swap;
            }
            possible = CanAim(a, target, false, elevationA);
            if (possible) return a;
            possible = CanAim(b, target, false, elevationB);
            return possible ? b : a;
        }

        private static float Elevation(ShipWeaponVisController visual, Vector3 target)
        {
            return Vector3.Dot(visual.transform.up, (target - visual.weaponObjectTransform.position).normalized);
        }

        internal static bool AlignedMuzzleClear(ShipWeaponVisController visual, Vector3 target)
        {
            return CanAim(visual, target, true, Elevation(visual, target));
        }

        private static bool CanAim(ShipWeaponVisController visual, Vector3 target, bool aligned, float elevation)
        {
            if (elevation < 0f)
                return false;
            Transform head = visual.weaponObjectTransform;
            Vector3 muzzle = visual.firePointTransform.position;
            if (!aligned)
            {
                Quaternion rotation = Quaternion.FromToRotation(head.forward, target - head.position);
                muzzle = head.position + rotation * (muzzle - head.position);
            }
            return models.TryGetValue(visual.combatHabModuleController, out ModuleModels module) &&
                !module.Blocks(muzzle, target);
        }
    }

    [HarmonyPatch(typeof(Weapon), nameof(Weapon.SelectWeaponVisualization))]
    internal static class OrbitalDefenseFacePatch
    {
        private static bool Prefix(Weapon __instance, Vector3 targetedPosition, ref ShipWeaponVisController __result)
        {
            if (!OrbitalDefenseCombat.Managed(__instance))
                return true;
            __result = OrbitalDefenseCombat.SelectFace(__instance, targetedPosition, out _);
            return false;
        }
    }

    [HarmonyPatch]
    internal static class OrbitalDefenseAcquirePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(Weapon), nameof(Weapon.GetPositionToTarget));
            yield return AccessTools.DeclaredMethod(typeof(ProjectileWeapon), nameof(Weapon.GetPositionToTarget));
        }

        private static void Postfix(Weapon __instance, ref bool impossible, Vector3 __result)
        {
            if (!impossible && OrbitalDefenseCombat.Managed(__instance))
            {
                OrbitalDefenseCombat.SelectFace(__instance, __result, out bool possible);
                impossible = !possible;
            }
        }
    }

    [HarmonyPatch(typeof(Weapon), nameof(Weapon.TryFireCommon))]
    internal static class OrbitalDefenseMuzzlePatch
    {
        private static void Postfix(Weapon __instance, ref bool __result)
        {
            if (__result && OrbitalDefenseCombat.Managed(__instance))
                __result = OrbitalDefenseCombat.AlignedMuzzleClear(
                    OrbitalDefenseCombat.SelectFace(__instance, __instance.targetedPosition, out _), __instance.targetedPosition);
        }
    }

    [HarmonyPatch(typeof(SpaceCombatManager), "ReturnHabToStrategyLayer")]
    internal static class OrbitalDefenseEndPatch
    {
        private static void Postfix(SpaceCombatManager __instance)
        {
            foreach (CombatHabModuleController module in __instance.combatHabModuleControllers)
                OrbitalDefenseCombat.RemoveModels(module);
        }
    }
}
