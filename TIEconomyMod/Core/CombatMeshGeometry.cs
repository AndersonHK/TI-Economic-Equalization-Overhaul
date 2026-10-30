using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Ship;
using PavonisInteractive.TerraInvicta.SpaceCombat;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Boxes follow individual solid render transforms, including animated parts.
    // No triangle-mesh collision cooking and no mutation of shared prefab assets.
    public sealed class CombatMeshGeometry : MonoBehaviour
    {
        private sealed class Part
        {
            internal Renderer Renderer;
            internal MeshFilter Filter;
            internal SkinnedMeshRenderer Skin;
            internal BoxCollider Box;
            internal Mesh AppliedMesh;
            internal bool Configured;
            internal RenderedMeshRaycast Tree;
            internal PreparedSkinnedContact SkinTree;
            internal PreparedSkinnedContact.QueryScratch SkinScratch;
            internal Transform[] Bones;
            internal Matrix4x4[] ToWorld, ToBone;
            internal int SnapshotFrame=-1;
            internal Matrix4x4 Inverse;
            internal Bounds WorldBounds;
            internal bool Visible;
            internal readonly List<BoxCollider> Boxes = new List<BoxCollider>();
        }
        private sealed class Original { internal Collider Collider; internal bool Enabled; }
        private readonly List<Part> parts = new List<Part>();
        private readonly List<Original> originals = new List<Original>();
        private readonly List<Collider> priorHits = new List<Collider>();
        private List<Collider> hits;
        private Collider[] priorPhysics;
        private ShipModelController model;
        private CombatantController combatant;
        private ProjectileController projectile;
        private bool restored;
        private bool destroying;
        private int frame;
        private static readonly HashSet<string> Reported = new HashSet<string>();
        private static readonly Dictionary<Collider, CombatMeshGeometry> Owners = new Dictionary<Collider, CombatMeshGeometry>();
        private readonly Dictionary<Collider, Part> byCollider = new Dictionary<Collider, Part>();
        private static readonly Dictionary<Mesh,RenderedMeshRaycast> StaticTrees=new Dictionary<Mesh,RenderedMeshRaycast>();
        private int snapshotFrame=-1;
        private Bounds contactBounds;
        private int visualBoundsFrame=-1;
        private Matrix4x4 visualBoundsRoot;
        private Bounds visualBounds;
        private bool hasVisualBounds;
        private static readonly Dictionary<CombatantController,CombatMeshGeometry> Combatants=new Dictionary<CombatantController,CombatMeshGeometry>();
        public static CombatMeshGeometry ForCombatant(CombatantController value)
        {CombatMeshGeometry geometry;return Combatants.TryGetValue(value,out geometry)?geometry:null;}
        public IDamageable Damageable { get { return combatant != null ? (IDamageable)combatant : projectile; } }

        public static CombatMeshGeometry ForShip(CombatShipController ship)
        {
            if (ship == null || ship.ModelController == null || ship.hitColliders == null) return null;
            CombatMeshGeometry existing = ship.GetComponent<CombatMeshGeometry>();
            if (existing != null && !existing.restored) return existing;
            CombatMeshGeometry value = ship.gameObject.AddComponent<CombatMeshGeometry>();
            value.model = ship.ModelController;
            value.combatant = ship;Combatants[ship]=value;
            value.priorPhysics = value.model._shipModalPhysicsColliders;
            CombatRenderGeometry.Apply(value.model.transform);
            value.Bind(value.model.transform, ship.hitColliders);
            PreparedContactWorld.Register(value);
            if (value.parts.Count > 0)
            {
                var colliders = new List<Collider>();
                foreach (Part part in value.parts) foreach(var box in part.Boxes) colliders.Add(box);
                value.model._shipModalPhysicsColliders = colliders.ToArray();
            }
            return value;
        }

        public static void ForHab(CombatHabModuleController hab)
        {
            CombatMeshGeometry previous = hab.GetComponent<CombatMeshGeometry>();
            if (previous != null) previous.Restore();
            CombatMeshGeometry value = hab.gameObject.AddComponent<CombatMeshGeometry>();
            value.combatant = hab;Combatants[hab]=value;
            value.Bind(hab.transform, hab.hitColliders);
            PreparedContactWorld.Register(value);
        }

        public static void ForMissile(MissileController missile)
        {
            CombatMeshGeometry value = missile.GetComponent<CombatMeshGeometry>();
            if (value == null)
            {
                value = missile.gameObject.AddComponent<CombatMeshGeometry>();
                value.projectile = missile;
                value.Bind(missile.transform, missile.hitColliders);
                PreparedContactWorld.Register(value);
            }
            value.Refresh();
        }

        private void Bind(Transform root, List<Collider> targetHits)
        {
            hits = targetHits;
            priorHits.AddRange(hits);
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || filter.sharedMesh == null || filter.sharedMesh.vertexCount < 3 || IsEffect(renderer.transform, root)) continue;
                // Ignore sub-0.1mm authored helper geometry rather than
                // inflating it into a minimum-size physical box.
                if(renderer.bounds.size.magnitude <= Patches.BattleScaleRuntime.ModelScale*0.0001f) continue;
                Add(renderer, filter, null);
            }
            foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer.sharedMesh != null && renderer.sharedMesh.vertexCount > 0 && !IsEffect(renderer.transform, root))
                    Add(renderer, null, renderer);
            }
            if (parts.Count == 0)
            {
                Main.Warn("Combat geometry found no solid meshes for " + root.name + "; native geometry retained.");
                return;
            }
            int hurtLayer = LayerMask.NameToLayer("HurtBox");
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider.gameObject.name == "EEO Mesh Hitbox") continue;
                // Selection/agent triggers are not physical damage targets.
                if (collider.gameObject.layer != hurtLayer && !priorHits.Contains(collider)) continue;
                if (collider.isTrigger || collider.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) continue;
                originals.Add(new Original { Collider = collider, Enabled = collider.enabled });
                collider.enabled = false;
            }
            Refresh();
            PrimeProjectiles(root);
            hits.Clear();
            foreach (Part part in parts) foreach(var box in part.Boxes) hits.Add(box);
        }

        private static bool IsEffect(Transform node, Transform root)
        {
            if (node.GetComponentInParent<ParticleSystem>() != null) return true;
            for (Transform item = node; item != null; item = item.parent)
            {
                string name = item.name;
                foreach (string token in new[] { "Selection", "TrackingUI", "Waypoint", "Marker", "ThrusterFX", "MuzzleFlash", "Exhaust" })
                    if (name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (item == root) break;
            }
            return false;
        }

        private void Add(Renderer renderer, MeshFilter filter, SkinnedMeshRenderer skin)
        {
            var part = new Part { Renderer = renderer, Filter = filter, Skin = skin };
            if(skin==null)part.Box=AddBox(part,renderer.transform);
            parts.Add(part);
        }
        private BoxCollider AddBox(Part part,Transform parent)
        {
            var go=new GameObject("EEO Mesh Hitbox");go.layer=LayerMask.NameToLayer("HurtBox");
            go.transform.SetParent(parent,false);var box=go.AddComponent<BoxCollider>();
            part.Boxes.Add(box);byCollider.Add(box,part);Owners.Add(box,this);return box;
        }
        private static void Fit(BoxCollider box,Vector3 min,Vector3 max)
        {
            box.center=(min+max)*.5f;Vector3 size=max-min;
            box.size=new Vector3(Math.Max(1e-8f,size.x),Math.Max(1e-8f,size.y),Math.Max(1e-8f,size.z));
            CombatContactMetrics.ColliderConfigurations++;
        }
        private void Refresh()
        {
            SuppressNativeDamageColliders();
            foreach(Part part in parts)
            {
                if(part.Skin!=null)
                {
                    if(part.Configured)continue;
                    int quality=part.Skin.quality==SkinQuality.Auto?(int)QualitySettings.skinWeights:(int)part.Skin.quality;
                    quality=quality==1?1:quality==2?2:4;
                    part.SkinTree=PreparedSkinnedContact.Prepare(part.Skin.sharedMesh,quality);
                    part.SkinScratch=part.SkinTree.CreateQueryScratch();
                    Transform[] bones=part.Skin.bones;
                    int size=Math.Max(bones.Length,part.SkinTree.VirtualBoneIndex+1);
                    part.Bones=new Transform[size];Array.Copy(bones,part.Bones,bones.Length);
                    if(part.SkinTree.VirtualBoneIndex>=0)part.Bones[part.SkinTree.VirtualBoneIndex]=part.Skin.transform;
                    part.ToWorld=new Matrix4x4[size];part.ToBone=new Matrix4x4[size];
                    for(int i=0;i<part.SkinTree.EnvelopeCount;i++)
                    {
                        var envelope=part.SkinTree.Envelope(i);
                        if(envelope.Bone>=part.Bones.Length||part.Bones[envelope.Bone]==null)
                            throw new InvalidOperationException("Prepared skin has a missing bone: "+part.Skin.name);
                        var box=AddBox(part,part.Bones[envelope.Bone]);Fit(box,envelope.Min,envelope.Max);
                        if(part.Box==null)part.Box=box;
                    }
                    part.Configured=true;
                    continue;
                }
                Mesh mesh=part.Filter==null?null:part.Filter.sharedMesh;
                if(mesh==null||part.Configured&&part.AppliedMesh==mesh)continue;
                part.AppliedMesh=mesh;part.Configured=true;
                Bounds bounds=mesh.bounds;Fit(part.Box,bounds.min,bounds.max);
                part.Tree=PrepareStatic(mesh);
            }
        }
        private static RenderedMeshRaycast PrepareStatic(Mesh mesh)
        {
            RenderedMeshRaycast tree;
            if(!StaticTrees.TryGetValue(mesh,out tree))
            {
                Mesh readable=mesh.isReadable?mesh:CombatRenderGeometry.BakedMesh(mesh,1f);
                tree=RenderedMeshRaycast.For(readable);StaticTrees.Add(mesh,tree);
            }
            return tree;
        }
        public static void PrimeProjectiles(Transform root)
        {
            foreach(var weapon in root.GetComponentsInChildren<ShipWeaponVisController>(true))
            {
                if(weapon.projectilePrefab==null||weapon.projectilePrefab.GetComponent<MissileController>()==null)continue;
                foreach(var filter in weapon.projectilePrefab.GetComponentsInChildren<MeshFilter>(true))
                    if(filter.sharedMesh!=null&&filter.sharedMesh.vertexCount>=3&&filter.GetComponent<MeshRenderer>()!=null&&!IsEffect(filter.transform,weapon.projectilePrefab.transform))PrepareStatic(filter.sharedMesh);
                foreach(var skin in weapon.projectilePrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    int quality=skin.quality==SkinQuality.Auto?(int)QualitySettings.skinWeights:(int)skin.quality;
                    PreparedSkinnedContact.Prepare(skin.sharedMesh,quality==1?1:quality==2?2:4);
                }
            }
        }
        public void RefreshAfterRenderBake() { Refresh(); }
        public static bool TryOwner(Collider collider, out CombatMeshGeometry owner) { return Owners.TryGetValue(collider, out owner); }
        public bool VisibleDamagePart(Collider collider)
        {
            Part part;
            return !restored && byCollider.TryGetValue(collider, out part) && part.Renderer != null &&
                part.Renderer.enabled && part.Renderer.gameObject.activeInHierarchy &&
                (combatant == null || !combatant.destructionTriggered) && (projectile == null || !projectile.beenDestroyed);
        }
        public bool RaycastVisual(Collider collider, Vector3 origin, Vector3 direction, float length, out float distance, out Vector3 normal)
        {
            distance = 0f; normal = Vector3.zero;
            Part part;
            if (!VisibleDamagePart(collider) || !byCollider.TryGetValue(collider, out part)) return false;
            Snapshot(part);
            if(!part.Visible)return false;
            if(part.SkinTree!=null)return part.SkinTree.Intersect(origin,direction,length,part.ToWorld,part.ToBone,part.SkinScratch,out distance,out normal);
            if(part.Tree==null)return false;
            Matrix4x4 inverse=part.Inverse;
            Vector3 localNormal;
            bool result = part.Tree.Intersect(inverse.MultiplyPoint3x4(origin),
                inverse.MultiplyVector(direction), length, out distance, out localNormal);
            if (result) normal = RenderedMeshRaycast.TransformNormal(inverse,localNormal);
            return result;
        }

        private static Bounds Transformed(Vector3 min,Vector3 max,Matrix4x4 matrix)
        {
            Vector3 e=(max-min)*.5f;
            return new Bounds(matrix.MultiplyPoint3x4((min+max)*.5f),new Vector3(
                Math.Abs(matrix.m00)*e.x+Math.Abs(matrix.m01)*e.y+Math.Abs(matrix.m02)*e.z,
                Math.Abs(matrix.m10)*e.x+Math.Abs(matrix.m11)*e.y+Math.Abs(matrix.m12)*e.z,
                Math.Abs(matrix.m20)*e.x+Math.Abs(matrix.m21)*e.y+Math.Abs(matrix.m22)*e.z)*2f);
        }
        private static void Snapshot(Part part)
        {
            if(part.SnapshotFrame==Time.frameCount)return;
            part.SnapshotFrame=Time.frameCount;
            part.Visible=part.Renderer!=null&&part.Renderer.enabled&&part.Renderer.gameObject.activeInHierarchy;
            if(!part.Visible)return;
            long timer=CombatContactMetrics.Begin();
            if(part.SkinTree!=null)
            {
                part.SkinScratch.BeginPose();
                for(int i=0;i<part.SkinTree.EnvelopeCount;i++)
                {
                    var envelope=part.SkinTree.Envelope(i);int bone=envelope.Bone;
                    part.ToWorld[bone]=part.Bones[bone].localToWorldMatrix;
                    part.ToBone[bone]=part.Bones[bone].worldToLocalMatrix;CombatContactMetrics.BonePoseReads++;
                    Bounds b=Transformed(envelope.Min,envelope.Max,part.ToWorld[bone]);
                    if(i==0)part.WorldBounds=b;else part.WorldBounds.Encapsulate(b);
                }
            }
            else
            {
                part.Inverse=part.Renderer.worldToLocalMatrix;Bounds b=part.AppliedMesh.bounds;
                part.WorldBounds=Transformed(b.min,b.max,part.Renderer.localToWorldMatrix);
            }
            CombatContactMetrics.End(3,timer);
        }
        public bool ContactLive
        {get{return !restored&&gameObject.activeInHierarchy&&(combatant==null||!combatant.destructionTriggered&&!combatant.isDestroyed)&&
            (projectile==null||!projectile.beenDestroyed&&!projectile.hasHit);}}
        public bool TryContact(Vector3 origin,Vector3 direction,float maximum,int mask,out RaycastHit hit)
        {
            hit=new RaycastHit();if(!ContactLive)return false;
            if(snapshotFrame!=Time.frameCount)
            {
                snapshotFrame=Time.frameCount;bool first=true;
                foreach(var part in parts)
                {Snapshot(part);if(!part.Visible)continue;if(first){contactBounds=part.WorldBounds;first=false;}else contactBounds.Encapsulate(part.WorldBounds);}
                if(first)contactBounds=new Bounds(transform.position,Vector3.zero);
            }
            if(!RenderedMeshRaycast.Box(origin,direction,contactBounds.min,contactBounds.max,maximum))return false;
            CombatContactMetrics.BodyCandidates++;
            bool found=false;
            foreach(var part in parts)
            {
                if(!part.Visible||!VisibleDamagePart(part.Box)||(mask&(1<<part.Box.gameObject.layer))==0||
                    !RenderedMeshRaycast.Box(origin,direction,part.WorldBounds.min,part.WorldBounds.max,maximum))continue;
                CombatContactMetrics.Candidates++;float distance;Vector3 normal;
                if(!RaycastVisual(part.Box,origin,direction,maximum,out distance,out normal))continue;
                hit=PreparedContactWorld.Hit(part.Box,origin+direction*distance,normal,distance);maximum=distance;found=true;
            }
            return found;
        }

        public void SuppressNativeDamageColliders()
        {
            if (restored) return;
            foreach (Original item in originals)
                if (item.Collider != null && item.Collider.enabled) item.Collider.enabled = false;
        }

        public bool TryBounds(out Bounds bounds)
        {
            Matrix4x4 root=transform.localToWorldMatrix;
            if(visualBoundsFrame!=Time.frameCount||root!=visualBoundsRoot)
            {
                visualBoundsFrame=Time.frameCount;visualBoundsRoot=root;hasVisualBounds=false;
                foreach(Part part in parts)
                {
                    if(part.Renderer==null||!part.Renderer.enabled||!part.Renderer.gameObject.activeInHierarchy)continue;
                    if(!hasVisualBounds){visualBounds=part.Renderer.bounds;hasVisualBounds=true;}
                    else visualBounds.Encapsulate(part.Renderer.bounds);
                }
                CombatContactMetrics.VisualBoundsScans++;
            }
            bounds=visualBounds;return hasVisualBounds;
        }

        private void LateUpdate()
        {
            if (restored) return;
            frame++;
            if (frame != 2) return;
            enabled = false; // Probe only; no recurring dimension/visibility work.
            Readback(false);
        }
        private void Readback(bool synchronized)
        {
            string key = (projectile != null ? "missile:" : "model:") +
                (model != null ? model.name : gameObject.name)+(synchronized?":synced":":initial");
            if (!Reported.Add(key)) return;
            float maximumError = 0f;
            float absoluteError = 0f; Part worst=null;
            int live = 0;
            foreach (Part part in parts)
            {
                if (part.Box == null || !part.Box.enabled) continue;
                Bounds visual = part.Renderer.bounds;
                Bounds physical = part.Box.bounds;
                foreach(var box in part.Boxes)if(box!=part.Box&&box.gameObject.activeInHierarchy)physical.Encapsulate(box.bounds);
                float scale = Math.Max(0.000000001f, visual.size.magnitude);
                maximumError = Math.Max(maximumError, (visual.center - physical.center).magnitude / scale);
                maximumError = Math.Max(maximumError, (visual.size - physical.size).magnitude / scale);
                float error=(visual.center-physical.center).magnitude+(visual.size-physical.size).magnitude;
                if(error>absoluteError){absoluteError=error;worst=part;}
                live++;
            }
            Main.Log("Combat geometry readback: " + key + " cinematic=" + Patches.BattleScaleRuntime.Cinematic +
                " boxes=" + live + " replaced=" + originals.Count + " maxRelativeBoundsError=" + maximumError.ToString("G6")+
                " maxAbsoluteErrorUnits="+absoluteError.ToString("G7")+" configurations="+CombatContactMetrics.ColliderConfigurations);
            if(worst!=null)Main.Log("Combat geometry worst part: "+worst.Renderer.name+
                " rendererBounds="+worst.Renderer.bounds+" physicsBounds="+worst.Box.bounds+
                " boxSize="+worst.Box.size.ToString("G7")+" childScale="+worst.Box.transform.localScale.ToString("G7")+
                " rendererScale="+worst.Renderer.transform.lossyScale.ToString("G7")+
                " meshBounds="+(worst.Filter==null?worst.Skin.localBounds.ToString():worst.Filter.sharedMesh.bounds.ToString()));
        }

        public void Restore()
        {
            if (restored) return;
            restored = true;
            PreparedContactWorld.Unregister(this);
            if(!ReferenceEquals(combatant,null))Combatants.Remove(combatant);
            foreach (Part part in parts)
            {
                foreach(var box in part.Boxes)
                {
                    if(ReferenceEquals(box,null))continue;
                    Owners.Remove(box);
                    if(box!=null){box.enabled=false;Destroy(box.gameObject);}
                }
            }
            foreach (Original item in originals)
                if (item.Collider != null) item.Collider.enabled = item.Enabled;
            if (model != null && priorPhysics != null) model._shipModalPhysicsColliders = priorPhysics;
            if (hits != null && parts.Count > 0)
            {
                hits.Clear();
                foreach (Collider collider in priorHits) if (collider != null) hits.Add(collider);
            }
            enabled = false;
            if (!destroying) Destroy(this);
        }
        private void OnDestroy() { destroying = true; Restore(); }
    }
}
