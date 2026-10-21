using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod.Patches
{
    public static class GunpowderArtRuntime
    {
        private sealed class State
        {
            internal readonly GunpowderScaleSnapshot Scale = new GunpowderScaleSnapshot();
            internal Transform Base;
            internal Transform Child;
            internal Transform FirePoint;
            internal Mesh NativeBaseMesh, NativeWeaponMesh, BakedBaseMesh, BakedWeaponMesh;
            internal Vector3 NativeChildPosition, NativeFirePosition, NativeFireScale;
            internal bool Baked;
            internal string Profile;
            internal string Warning;
        }
        private static readonly ConditionalWeakTable<ShipWeaponVisController, State> States =
            new ConditionalWeakTable<ShipWeaponVisController, State>();
        private static readonly List<WeakReference> Tracked = new List<WeakReference>();
        private static readonly HashSet<string> Reported = new HashSet<string>(StringComparer.Ordinal);
        private static readonly Dictionary<Material, Material> NonInstancedMaterials = new Dictionary<Material, Material>();
        public static bool BakeSizing = true;
        public static bool DisableInstancing;
        private static readonly System.Reflection.FieldInfo OriginalBaseMaterial =
            AccessTools.Field(typeof(ShipWeaponVisController), "_originalBaseMaterial");
        private static readonly System.Reflection.FieldInfo OriginalWeaponMaterial =
            AccessTools.Field(typeof(ShipWeaponVisController), "_originalWeaponMaterial");
        private static bool Enabled
        {
            get { return Main.settings != null && Main.FeatureEnabled(Main.settings.shipBalance.enabled) && Main.gunpowderArt != null; }
        }

        public static void Restore(ShipWeaponVisController target)
        {
            if (target == null) return;
            State state;
            if (!States.TryGetValue(target, out state) || !state.Scale.Applied) return;
            if (state.Base != null && state.Child != null && target.baseObject != null && target.weaponObject != null &&
                state.Base == target.baseObject.transform && state.Child == target.weaponObject.transform)
            {
                if (state.Baked)
                {
                    MeshFilter baseFilter = target.baseObject.GetComponent<MeshFilter>();
                    MeshFilter weaponFilter = target.weaponObject.GetComponent<MeshFilter>();
                    if (baseFilter != null && baseFilter.sharedMesh == state.BakedBaseMesh) baseFilter.sharedMesh = state.NativeBaseMesh;
                    if (weaponFilter != null && weaponFilter.sharedMesh == state.BakedWeaponMesh) weaponFilter.sharedMesh = state.NativeWeaponMesh;
                    state.Child.localPosition = state.NativeChildPosition;
                    if (state.FirePoint != null && target.firePoint != null && state.FirePoint == target.firePoint.transform)
                    {
                        state.FirePoint.localPosition = state.NativeFirePosition;
                        state.FirePoint.localScale = state.NativeFireScale;
                    }
                    state.Baked = false;
                }
                if (state.Scale.Release(state.Base.localScale, state.Child.localScale))
                {
                    state.Base.localScale = state.Scale.NativeBase;
                    state.Child.localScale = state.Scale.NativeChild;
                }
            }
            else { state.Scale.Release(Vector3.zero, Vector3.zero); state.Baked = false; }
        }

        public static void Apply(string resource, ShipWeaponVisController target)
        {
            if (!Enabled || target == null) return;
            TIGunTemplate gun = target.weaponTemplate as TIGunTemplate;
            GunpowderArtProfile profile;
            if (gun == null || !Main.gunpowderArt.TryGet(gun.dataName, out profile)) return;
            State state;
            if (!States.TryGetValue(target, out state))
            {
                state = new State(); States.Add(target, state);
                Tracked.RemoveAll(reference => !reference.IsAlive || reference.Target as ShipWeaponVisController == null);
                Tracked.Add(new WeakReference(target));
            }
            try
            {
                if (state.Scale.Applied) Restore(target);
                float caliber;
                if (resource != profile.Resource || gun.modelResource != profile.Resource ||
                    !ProjectileGeometryRegistry.TryGetDiameter_mm(gun, out caliber) || Math.Abs(caliber - profile.CaliberMm) > .001)
                { Warn(state, gun.dataName, "resource or scenario caliber differs from the calibrated profile"); return; }
                if (target.baseObject == null || target.weaponObject == null || target.firePoint == null)
                { Warn(state, gun.dataName, "missing weapon transform"); return; }
                Transform baseT = target.baseObject.transform, childT = target.weaponObject.transform;
                Transform fireT = target.firePoint.transform;
                MeshFilter baseMesh = target.baseObject.GetComponent<MeshFilter>();
                MeshFilter movingMesh = target.weaponObject.GetComponent<MeshFilter>();
                if (childT.parent != baseT || fireT.parent != childT || baseMesh == null || movingMesh == null ||
                    !profile.BaseMesh.Matches(baseMesh.sharedMesh) || !profile.MovingMesh.Matches(movingMesh.sharedMesh) ||
                    !GunpowderArtCatalog.Near(childT.localPosition, GunpowderArtCatalog.Vector(profile.ChildPosition)) ||
                    !GunpowderArtCatalog.Near(fireT.localPosition, GunpowderArtCatalog.Vector(profile.FirePointPosition)))
                { Warn(state, gun.dataName, "mesh, pivot or fire-point fingerprint mismatch"); return; }

                // Both representations have identical world geometry: either
                // multiply the parent scale, or bake vertices/pivot/fire point.
                state.Base = baseT; state.Child = childT; state.FirePoint = fireT; state.Profile = profile.Weapon;
                bool bake = BakeSizing;
                Mesh bakedBase = null, bakedWeapon = null;
                if (bake)
                {
                    try { GunpowderArtGeometry.Get(profile, baseMesh.sharedMesh, movingMesh.sharedMesh, out bakedBase, out bakedWeapon); }
                    catch (Exception bakeException)
                    {
                        bake = false;
                        string warning = gun.dataName + ": mesh baking unavailable; canonical transform scaling retained: " + bakeException.Message;
                        if (state.Warning != warning) { state.Warning = warning; Main.Warn(warning); }
                    }
                }
                Vector3 finalBaseScale = bake ? GunpowderArtCatalog.Vector(profile.RootScale) : profile.FinalBaseScale;
                state.Scale.Capture(baseT.localScale, childT.localScale, finalBaseScale, profile.FinalChildScale);
                if (bake)
                {
                    state.NativeBaseMesh = baseMesh.sharedMesh; state.NativeWeaponMesh = movingMesh.sharedMesh;
                    state.BakedBaseMesh = bakedBase; state.BakedWeaponMesh = bakedWeapon;
                    state.NativeChildPosition = childT.localPosition;
                    state.NativeFirePosition = fireT.localPosition; state.NativeFireScale = fireT.localScale;
                    state.Baked = true;
                    baseMesh.sharedMesh = bakedBase; movingMesh.sharedMesh = bakedWeapon;
                    childT.localPosition = GunpowderArtGeometry.ScalePoint(state.NativeChildPosition, profile.Factor);
                    fireT.localPosition = GunpowderArtGeometry.ScalePoint(state.NativeFirePosition, profile.Factor);
                    fireT.localScale = GunpowderArtGeometry.ScalePoint(state.NativeFireScale, profile.Factor);
                }
                baseT.localScale = finalBaseScale;
                childT.localScale = profile.FinalChildScale;
                try
                {
                    RestoreSourceMaterials(target, profile);
                    GunpowderMaterialProbe probe = target.GetComponent<GunpowderMaterialProbe>();
                    if (probe == null) probe = target.gameObject.AddComponent<GunpowderMaterialProbe>();
                    probe.Arm(target, gun.dataName);
                }
                catch (Exception materialException)
                {
                    // A diagnostic/material failure must not undo accepted art dimensions.
                    string warning = gun.dataName + ": material pass skipped; canonical scale retained: " + materialException.Message;
                    if (state.Warning != warning) { state.Warning = warning; Main.Warn(warning); }
                }
                string context = target.shipVisController != null && target.shipVisController.shipState != null ? "ship" : "preview";
                if (Reported.Add(gun.dataName + ":" + context + ":" + bake + ":" + DisableInstancing))
                    Main.Log("Gunpowder art schema 1: " + gun.dataName + " / " + context + " factor=" +
                        profile.Factor.ToString("F6", System.Globalization.CultureInfo.InvariantCulture) +
                        " baked=" + bake + " nonInstanced=" + DisableInstancing);
            }
            catch (Exception ex)
            {
                Restore(target);
                Warn(state, gun.dataName, ex.GetType().Name + ": " + ex.Message);
            }
        }

        public static void RefreshVisible()
        {
            for (int i = Tracked.Count - 1; i >= 0; --i)
            {
                var target = Tracked[i].Target as ShipWeaponVisController;
                if (target == null) { Tracked.RemoveAt(i); continue; }
                Restore(target);
                if (target.weaponTemplate != null) Apply(target.weaponTemplate.modelResource, target);
            }
        }

        public static void OnRepaired(ShipWeaponVisController target)
        {
            if (!Enabled || target == null || target.weaponTemplate == null) return;
            State state;
            GunpowderArtProfile profile;
            if (!States.TryGetValue(target, out state) || !state.Scale.Applied ||
                !Main.gunpowderArt.TryGet(target.weaponTemplate.dataName, out profile)) return;
            try { RestoreSourceMaterials(target, profile); }
            catch (Exception ex) { Warn(state, profile.Weapon, "material repair: " + ex.Message); }
        }

        private static void RestoreSourceMaterials(ShipWeaponVisController target, GunpowderArtProfile profile)
        {
            GameObject source = GameControl.assetLoader.LoadAsset<GameObject>(profile.Resource);
            if (source == null || source.transform.childCount == 0 || OriginalBaseMaterial == null || OriginalWeaponMaterial == null)
                throw new InvalidOperationException("Gunpowder material source/cache fields unavailable.");
            MeshRenderer sourceBase = source.GetComponent<MeshRenderer>();
            MeshRenderer sourceWeapon = source.transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer targetBase = target.baseObject.GetComponent<MeshRenderer>();
            MeshRenderer targetWeapon = target.weaponObject.GetComponent<MeshRenderer>();
            if (sourceBase == null || sourceWeapon == null || targetBase == null || targetWeapon == null)
                throw new InvalidOperationException("Gunpowder material renderer unavailable.");
            Mesh currentBase = target.baseObject.GetComponent<MeshFilter>().sharedMesh;
            Mesh currentWeapon = target.weaponObject.GetComponent<MeshFilter>().sharedMesh;
            State owner;
            bool ownedBake = States.TryGetValue(target, out owner) && owner.Baked && owner.Profile == profile.Weapon &&
                currentBase == owner.BakedBaseMesh && currentWeapon == owner.BakedWeaponMesh;
            if (!ownedBake && (!profile.BaseMesh.Matches(currentBase) || !profile.MovingMesh.Matches(currentWeapon)))
                throw new InvalidOperationException("Gunpowder repair target mesh changed.");
            Material[] baseMaterials = sourceBase.sharedMaterials, weaponMaterials = sourceWeapon.sharedMaterials;
            // All approved gunpowder resources have exactly one submesh/material.
            // Refuse a changed source instead of silently dropping material slots.
            if (!GunpowderArtCatalog.MaterialLayoutMatches(target.baseObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount,
                    target.weaponObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount, baseMaterials.Length, weaponMaterials.Length) ||
                baseMaterials[0] == null || weaponMaterials[0] == null)
                throw new InvalidOperationException("Unexpected gunpowder submesh/material layout.");
            if (DisableInstancing)
            {
                baseMaterials = new[] { NonInstanced(baseMaterials[0]) };
                weaponMaterials = new[] { NonInstanced(weaponMaterials[0]) };
            }
            targetBase.sharedMaterials = baseMaterials;
            targetWeapon.sharedMaterials = weaponMaterials;
            OriginalBaseMaterial.SetValue(target, baseMaterials[0]);
            OriginalWeaponMaterial.SetValue(target, weaponMaterials[0]);
        }

        private static Material NonInstanced(Material source)
        {
            Material material;
            if (!NonInstancedMaterials.TryGetValue(source, out material) || material == null)
            {
                material = new Material(source) { name = source.name + " [EEO non-instanced]", enableInstancing = false };
                NonInstancedMaterials[source] = material;
            }
            return material;
        }

        public static void DrawRenderComparisonControls()
        {
            GUILayout.Space(8f);
            GUILayout.Label("Gunpowder render comparison (same dimensions; session only)");
            bool bake = GUILayout.Toggle(BakeSizing, "Bake sizing into mesh copies");
            bool nonInstanced = GUILayout.Toggle(DisableInstancing, "Disable GPU instancing for gunpowder materials");
            if (bake != BakeSizing || nonInstanced != DisableInstancing)
            {
                BakeSizing = bake; DisableInstancing = nonInstanced;
                Main.Log("Gunpowder render comparison: baked=" + bake + " nonInstanced=" + nonInstanced);
                RefreshVisible();
            }
        }

        private static void Warn(State state, string weapon, string reason)
        {
            string message = weapon + ": " + reason;
            if (state.Warning == message) return;
            state.Warning = message;
            Main.Warn("Gunpowder art guard retained native art for " + message);
        }
    }

    // Read-only, bounded runtime evidence. No material writes from Update, so
    // damaged-state effects remain visible and later overrides can be diagnosed.
    public sealed class GunpowderMaterialProbe : MonoBehaviour
    {
        private static readonly HashSet<string> CameraReports = new HashSet<string>(StringComparer.Ordinal);
        private ShipWeaponVisController target;
        private string weapon;
        private int frame;
        public void Arm(ShipWeaponVisController controller, string name)
        {
            target = controller; weapon = name; frame = 0; enabled = true;
        }
        private void Update()
        {
            if (target == null || target.weaponTemplate == null || target.weaponTemplate.dataName != weapon)
            { enabled = false; return; }
            frame++;
            if (frame == 1 || frame == 30 || frame == 120)
            {
                Trace("base", target.baseObject); Trace("moving", target.weaponObject);
                if (frame == 1)
                    foreach (Camera camera in Camera.allCameras)
                    {
                        string info = camera.name + " path=" + camera.actualRenderingPath + " depth=" + camera.depth +
                            " clear=" + camera.clearFlags + " mask=" + camera.cullingMask +
                            " clip=" + camera.nearClipPlane + "/" + camera.farClipPlane;
                        if (CameraReports.Add(info)) Main.Log("Gunpowder camera probe: " + info);
                    }
            }
            if (frame >= 120) enabled = false;
        }
        private void Trace(string part, GameObject go)
        {
            if (go == null) return;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            MeshFilter filter = go.GetComponent<MeshFilter>();
            if (renderer == null) return;
            var text = new System.Text.StringBuilder();
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat == null) { text.Append(" [null]"); continue; }
                Texture tex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                text.Append(" [").Append(mat.name).Append(" shader=").Append(mat.shader == null ? "null" : mat.shader.name)
                    .Append(" albedo=").Append(tex == null ? "null" : tex.name + " " + tex.width + "x" + tex.height);
                text.Append(" queue=").Append(mat.renderQueue).Append(" instancing=").Append(mat.enableInstancing)
                    .Append(" supported=").Append(mat.shader != null && mat.shader.isSupported)
                    .Append(" forward=").Append(mat.GetShaderPassEnabled("FORWARD"))
                    .Append(" deferred=").Append(mat.GetShaderPassEnabled("DEFERRED"))
                    .Append(" keywords=").Append(String.Join(",", mat.shaderKeywords));
                foreach (string property in new[] { "_Mode", "_SrcBlend", "_DstBlend", "_ZWrite" })
                    if (mat.HasProperty(property)) text.Append(" ").Append(property).Append("=").Append(mat.GetFloat(property));
                if (mat.HasProperty("_Color")) text.Append(" color=").Append(mat.GetColor("_Color"));
                if (mat.HasProperty("_MainTex")) text.Append(" UVscale=").Append(mat.GetTextureScale("_MainTex"));
                text.Append("]");
            }
            if (renderer.HasPropertyBlock())
            {
                var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
                Texture overrideTexture = block.GetTexture("_MainTex");
                text.Append(" blockAlbedo=").Append(overrideTexture == null ? "none" : overrideTexture.name);
            }
            Main.Log("Gunpowder material probe: " + weapon + " / " + part + " frame=" + frame +
                " node=" + go.name + " submeshes=" + (filter == null || filter.sharedMesh == null ? -1 : filter.sharedMesh.subMeshCount) +
                " mesh=" + (filter == null || filter.sharedMesh == null ? "null" : filter.sharedMesh.name) +
                " scale=" + go.transform.lossyScale + " layer=" + go.layer +
                " enabled=" + renderer.enabled + " staticBatch=" + renderer.isPartOfStaticBatch +
                " additionalStreams=" + (renderer.additionalVertexStreams == null ? "none" : renderer.additionalVertexStreams.name) +
                " propertyBlock=" + renderer.HasPropertyBlock() + text);
        }
    }

    [HarmonyPatch(typeof(ShipWeaponVisController), nameof(ShipWeaponVisController.OnWeaponRepaired))]
    public static class GunpowderArtRepairMaterialPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipWeaponVisController __instance) { GunpowderArtRuntime.OnRepaired(__instance); }
    }

    [HarmonyPatch(typeof(ShipModelController), nameof(ShipModelController.SetWeapon))]
    public static class GunpowderArtSetWeaponPatch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        public static void Prefix(ShipWeaponVisController targetController)
        {
            // Runs for all weapon types, even disabled: a reused gun controller
            // must not lend its scale to a newly selected laser or magnetic gun.
            GunpowderArtRuntime.Restore(targetController);
        }
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(string resource, ShipWeaponVisController targetController)
        {
            GunpowderArtRuntime.Apply(resource, targetController);
        }
    }
}
