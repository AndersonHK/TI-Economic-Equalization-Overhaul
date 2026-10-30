using System;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Never patch DestroyHabModule: Mono prepares its AssetCacheManager fields
    // during early Harmony installation, before the native asset loader exists.
    public sealed class CombatStationExplosionObserver : MonoBehaviour
    {
        private readonly HashSet<Transform> known = new HashSet<Transform>();
        private CombatHabModuleController owner;
        public void Arm(CombatHabModuleController module)
        {
            owner = module;
            known.Clear();
            foreach (Transform child in transform) known.Add(child);
            enabled = true;
        }
        public void Disarm() { enabled = false; known.Clear(); owner = null; }
        private void LateUpdate()
        {
            if (owner == null || !TIGlobalValuesState.isSpaceCombatEnabled ||
                !Patches.BattleScaleRuntime.Enabled || !owner.destructionTriggered) return;
            foreach (Transform child in transform)
                if (known.Add(child)) CombatExplosionGeometry.MatchSpawn(child, transform, owner);
        }
    }

    public sealed class CombatExplosionGeometry : MonoBehaviour
    {
        public static float WorldScale(float diameter, float reference)
        { return reference > 0f ? Math.Max(0f, diameter) / reference : 0f; }
        public static Vector3 LocalScale(Vector3 parentScale, float worldScale)
        { return new Vector3(worldScale / Safe(parentScale.x), worldScale / Safe(parentScale.y), worldScale / Safe(parentScale.z)); }
        private readonly Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
        private readonly Dictionary<ParticleSystem, ParticleSystemScalingMode> originalModes = new Dictionary<ParticleSystem, ParticleSystemScalingMode>();
        private bool restored;

        public static float VisualDiameter(Transform root)
        {
            if (root == null) return 0f;
            Bounds bounds = new Bounds(); bool found = false;
            foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy || renderer.GetComponentInParent<ParticleSystem>() != null) continue;
                if (renderer.gameObject.layer == LayerMask.NameToLayer("Space Combat UI") || renderer.gameObject.layer == LayerMask.NameToLayer("Selection")) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return found ? Math.Max(bounds.size.x, Math.Max(bounds.size.y, bounds.size.z)) : 0f;
        }

        public static void BindShip(CombatShipController ship)
        {
            if (ship == null || ship.ModelController == null) return;
            CombatExplosionGeometry state = ship.GetComponent<CombatExplosionGeometry>();
            if (state == null) state = ship.gameObject.AddComponent<CombatExplosionGeometry>();
            float diameter = VisualDiameter(ship.ModelController.transform);
            if (ship.ModelController.smallExplosionParticleSystems != null)
                foreach (ParticleSystem particle in ship.ModelController.smallExplosionParticleSystems)
                {
                    if (particle == null) continue;
                    float part = VisualDiameter(particle.transform.parent);
                    state.Match(particle.transform, part > 0f ? part : diameter);
                }
            if (ship.ModelController.destructionExplosionParticleSystem != null)
                state.Match(ship.ModelController.destructionExplosionParticleSystem.transform, diameter);
        }

        public void Match(Transform root, float worldDiameter)
        {
            if (root == null || worldDiameter <= 0f) return;
            ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
            if (particles.Length == 0) return;
            float reference = 0f;
            Vector3 rootScale = root.lossyScale;
            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule main = particle.main;
                float size = main.startSize3D
                    ? Math.Max(main.startSizeX.constantMax, Math.Max(main.startSizeY.constantMax, main.startSizeZ.constantMax))
                    : main.startSize.constantMax;
                Vector3 scale = particle.transform.lossyScale;
                float relative = Math.Max(Math.Abs(scale.x) / Safe(rootScale.x),
                    Math.Max(Math.Abs(scale.y) / Safe(rootScale.y), Math.Abs(scale.z) / Safe(rootScale.z)));
                reference = Math.Max(reference, size * relative);
            }
            if (reference <= 0f) return;
            if (!originalScales.ContainsKey(root)) originalScales.Add(root, root.localScale);
            foreach (ParticleSystem particle in particles)
            {
                if (!originalModes.ContainsKey(particle)) originalModes.Add(particle, particle.main.scalingMode);
                var main = particle.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            float scaleWorld = WorldScale(worldDiameter, reference);
            Vector3 parent = root.parent == null ? Vector3.one : root.parent.lossyScale;
            root.localScale = LocalScale(parent, scaleWorld);
            // Newly activated pooled/instantiated effects may already have born
            // world-space particles at the old size; discard those, not timing.
            foreach (ParticleSystem particle in particles) particle.Clear(false);
        }

        public static void MatchSpawn(Transform root, Transform visualRoot, Component owner)
        {
            if (root == null || owner == null) return;
            float diameter = VisualDiameter(visualRoot);
            if (diameter <= 0f) return;
            CombatExplosionGeometry state = root.GetComponent<CombatExplosionGeometry>();
            if (state == null) state = root.gameObject.AddComponent<CombatExplosionGeometry>();
            state.Match(root, diameter);
        }
        private static float Safe(float value) { return Math.Max(0.000000001f, Math.Abs(value)); }
        public void Restore()
        {
            if (restored) return;
            restored = true;
            foreach (var item in originalScales) if (item.Key != null) item.Key.localScale = item.Value;
            foreach (var item in originalModes) if (item.Key != null) { var main = item.Key.main; main.scalingMode = item.Value; }
        }
        private void OnDestroy() { Restore(); }
    }
}
