using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Core
{
    public sealed class BattleProjectileGeometry : MonoBehaviour
    {
        private static readonly AccessTools.FieldRef<ProjectileController, ParticleSystem> FlightParticles =
            AccessTools.FieldRefAccess<ProjectileController, ParticleSystem>("projectileParticleSystem");
        private static readonly Dictionary<WeaponClass, Material> Materials = new Dictionary<WeaponClass, Material>();
        private static readonly HashSet<string> Reported = new HashSet<string>();
        private GameObject body;
        private Collider bodyCollider;
        private Renderer bodyRenderer;
        private ProjectileController owner;
        private float particleDiameter;
        private ParticleSystemRenderer[] tacticalRenderers;
        private float[] tacticalMinimumSizes;
        private bool? tacticalVisible;
        private int tacticalHeight;

        public static void Apply(BallisticProjectileController projectile)
        {
            BattleProjectileGeometry value = projectile.GetComponent<BattleProjectileGeometry>();
            if (value == null)
            {
                value = projectile.gameObject.AddComponent<BattleProjectileGeometry>();
                value.owner = projectile;
                ParticleSystem particles = FlightParticles(projectile);
                if (particles != null)
                {
                    float parentScale = Math.Max(0.000000001f, Math.Abs(projectile.transform.lossyScale.x));
                    Vector3 scale = particles.transform.lossyScale;
                    value.particleDiameter = particles.main.startSize.constantMax *
                        Math.Max(Math.Abs(scale.x), Math.Max(Math.Abs(scale.y), Math.Abs(scale.z))) / parentScale;
                }
                foreach (Collider collider in projectile.hitColliders) if (collider != null) collider.enabled = false;
                if (particles != null)
                {
                    value.tacticalRenderers = particles.GetComponentsInChildren<ParticleSystemRenderer>(true);
                    value.tacticalMinimumSizes = new float[value.tacticalRenderers.Length];
                    for (int i = 0; i < value.tacticalRenderers.Length; i++)
                    {
                        value.tacticalMinimumSizes[i] = value.tacticalRenderers[i].minParticleSize;
                        value.tacticalRenderers[i].enabled = false;
                    }
                }
                value.body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                value.body.name = "EEO Projectile Body";
                value.body.layer = LayerMask.NameToLayer("HurtBox");
                value.body.transform.SetParent(projectile.transform, false);
                // Remove the primitive's default sphere: an elongated sphere's
                // Unity collider would use the largest axis and become enormous.
                Collider sphere = value.body.GetComponent<Collider>();
                sphere.enabled = false;
                Destroy(sphere);
                value.bodyRenderer = value.body.GetComponent<Renderer>();
                value.bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                value.bodyRenderer.receiveShadows = false;
            }
            TIProjectileWeaponTemplate weapon = projectile.projectileState.originWeapon;
            float diameter_mm;
            float diameter = ProjectileGeometryRegistry.TryGetDiameter_mm(weapon, out diameter_mm)
                ? diameter_mm / 1000f : Math.Max(0.000001f, value.particleDiameter);
            float length = weapon.weaponClass == WeaponClass.Magnetic
                ? diameter * ProjectileCollisionMath.MagneticProjectileLengthToDiameter : diameter;
            // Eliminate the logarithmic magnetic visibility multiplier. The body
            // now derives from caliber/complete-round geometry, in actual metres.
            projectile.transform.localScale = Vector3.one * Patches.BattleScaleRuntime.ProjectileScale;
            value.body.transform.localPosition = Vector3.zero;
            value.body.transform.localRotation = Quaternion.identity;
            value.body.transform.localScale = new Vector3(diameter, diameter, length);
            value.bodyRenderer.sharedMaterial = MaterialFor(weapon.weaponClass);
            if (value.bodyCollider == null)
            {
                if (weapon.weaponClass == WeaponClass.Magnetic)
                {
                    BoxCollider box = value.body.AddComponent<BoxCollider>();
                    box.size = Vector3.one;
                    value.bodyCollider = box;
                }
                else
                {
                    SphereCollider sphere = value.body.AddComponent<SphereCollider>();
                    sphere.radius = 0.5f;
                    value.bodyCollider = sphere;
                }
            }
            projectile.projectileCollider = value.bodyCollider;
            projectile.hitColliders.Clear();
            projectile.hitColliders.Add(value.bodyCollider);
            PreparedContactWorld.RegisterRound(projectile,value.bodyCollider);
            value.body.SetActive(true);
            value.bodyCollider.enabled = true;
            value.enabled = true;
            value.tacticalVisible = null;
            if (Reported.Add(weapon.dataName + ":" + Patches.BattleScaleRuntime.Cinematic))
                Main.Log("Combat projectile readback: " + weapon.dataName + " cinematic=" + Patches.BattleScaleRuntime.Cinematic +
                    " bodyMetres=" + diameter.ToString("G6") + "x" + length.ToString("G6") +
                    " unitsPerMetre=" + Patches.BattleScaleRuntime.ProjectileScale.ToString("G6") +
                    " collider=" + value.bodyCollider.GetType().Name);
        }

        private static Material MaterialFor(WeaponClass family)
        {
            Material material;
            if (Materials.TryGetValue(family, out material)) return material;
            Shader shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("No supported projectile body shader is available.");
            material = new Material(shader);
            material.name = "EEO Physical Projectile " + family;
            material.color = family == WeaponClass.Plasma ? new Color(0.4f, 0.8f, 1f, 1f) : new Color(1f, 0.85f, 0.5f, 1f);
            Materials.Add(family, material);
            return material;
        }
        public void Hide()
        {
            if (body != null && body.activeSelf) body.SetActive(false);
            if (tacticalVisible != false && tacticalRenderers != null) foreach (ParticleSystemRenderer renderer in tacticalRenderers)
                if (renderer != null) renderer.enabled = false;
            tacticalVisible = false;
            enabled = false;
        }
        private void LateUpdate()
        {
            if (owner == null || owner.beenDestroyed || owner.hasHit) { Hide(); return; }
            if (tacticalRenderers == null || GameControl.spaceCombat == null || GameControl.spaceCombat.mainCameraTransform == null) return;
            float threshold = 2f * Patches.BattleScaleRuntime.ProjectileRatio;
            bool show = (GameControl.spaceCombat.mainCameraTransform.position - owner.position).sqrMagnitude >= threshold * threshold &&
                GameControl.spaceCombat.combatHUD != null && !GameControl.spaceCombat.combatHUD.debugHideUI;
            bool changeVisibility = tacticalVisible != show;
            bool changeSize = tacticalHeight != Screen.height;
            if (!changeVisibility && !changeSize) return;
            tacticalVisible = show; tacticalHeight = Screen.height;
            for (int i = 0; i < tacticalRenderers.Length; i++)
            {
                if (tacticalRenderers[i] == null) continue;
                if (changeVisibility) tacticalRenderers[i].enabled = show;
                // A tactical overlay is not extra physical diameter/hit volume.
                if (changeSize) tacticalRenderers[i].minParticleSize = Math.Max(tacticalMinimumSizes[i], 2f / Math.Max(1, Screen.height));
            }
        }
    }
}
