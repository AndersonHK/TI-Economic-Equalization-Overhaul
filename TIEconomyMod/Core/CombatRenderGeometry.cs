using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using PavonisInteractive.TerraInvicta;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Equivalent world geometry with larger renderer matrices, as in the
    // accepted gunpowder fix. Only owned meshes and live instance transforms.
    public sealed class CombatRenderGeometry : MonoBehaviour
    {
        private sealed class Child
        {
            internal Transform Transform;
            internal Vector3 Position, Scale;
        }
        private sealed class Part
        {
            internal MeshFilter Filter;
            internal Mesh Source, Baked;
            internal Vector3 Scale;
            internal List<Child> Children = new List<Child>();
            internal List<Shape> Shapes = new List<Shape>();
        }
        private sealed class Shape
        {
            internal Collider Collider;
            internal Vector3 Center, Size;
            internal float Radius, Height;
            internal Mesh Mesh;
            internal void Apply(float factor)
            {
                BoxCollider box = Collider as BoxCollider;
                if (box != null) { box.center = Center * factor; box.size = Size * factor; }
                SphereCollider sphere = Collider as SphereCollider;
                if (sphere != null) { sphere.center = Center * factor; sphere.radius = Radius * factor; }
                CapsuleCollider capsule = Collider as CapsuleCollider;
                if (capsule != null) { capsule.center = Center * factor; capsule.radius = Radius * factor; capsule.height = Height * factor; }
                MeshCollider mesh = Collider as MeshCollider;
                if (mesh != null && Mesh != null) mesh.sharedMesh = factor == 1f ? Mesh : BakedMesh(Mesh, factor);
            }
        }
        private readonly List<Part> parts = new List<Part>();
        private static readonly Dictionary<Mesh, Dictionary<float, Mesh>> Cache = new Dictionary<Mesh, Dictionary<float, Mesh>>();
        private float factor;
        private bool restored, destroying;
        private int frame;

        public static void Apply(Transform root)
        {
            if (root == null || root.GetComponent<CombatRenderGeometry>() != null) return;
            var value = root.gameObject.AddComponent<CombatRenderGeometry>();
            value.factor = Patches.BattleScaleRuntime.ModelRatio;
            var filters = new List<MeshFilter>(root.GetComponentsInChildren<MeshFilter>(true));
            filters.Sort((a, b) => Depth(a.transform).CompareTo(Depth(b.transform)));
            foreach (MeshFilter filter in filters)
            {
                if (filter.sharedMesh == null || filter.sharedMesh.vertexCount == 0 ||
                    filter.GetComponent<MeshRenderer>() == null || filter.GetComponentInParent<ParticleSystem>() != null) continue;
                if (filter.gameObject.layer == LayerMask.NameToLayer("Space Combat UI") ||
                    filter.gameObject.layer == LayerMask.NameToLayer("Selection")) continue;
                try { value.Add(filter); }
                catch (Exception ex) { Main.Warn("Combat mesh bake skipped " + filter.sharedMesh.name + ": " + ex.Message); }
            }
            Main.Log("Combat render bake: " + root.name + " parts=" + value.parts.Count +
                " vertexFactor=" + value.factor.ToString("G6") + " rendererScaleCompensation=" + (1f / value.factor).ToString("G6"));
        }

        private static int Depth(Transform node)
        {
            int result = 0;
            while (node.parent != null) { result++; node = node.parent; }
            return result;
        }

        private void Add(MeshFilter filter)
        {
            Mesh baked = BakedMesh(filter.sharedMesh, factor);
            var part = new Part { Filter = filter, Source = filter.sharedMesh, Baked = baked, Scale = filter.transform.localScale };
            foreach (Transform child in filter.transform)
            {
                // Fitted damage boxes always use the renderer's local frame.
                if (child.name == "EEO Mesh Hitbox") continue;
                part.Children.Add(new Child { Transform = child, Position = child.localPosition, Scale = child.localScale });
            }
            foreach (Collider collider in filter.GetComponents<Collider>())
            {
                var shape = new Shape { Collider = collider };
                BoxCollider box = collider as BoxCollider;
                if (box != null) { shape.Center = box.center; shape.Size = box.size; }
                SphereCollider sphere = collider as SphereCollider;
                if (sphere != null) { shape.Center = sphere.center; shape.Radius = sphere.radius; }
                CapsuleCollider capsule = collider as CapsuleCollider;
                if (capsule != null) { shape.Center = capsule.center; shape.Radius = capsule.radius; shape.Height = capsule.height; }
                MeshCollider mesh = collider as MeshCollider;
                if (mesh != null) shape.Mesh = mesh.sharedMesh;
                part.Shapes.Add(shape);
            }
            parts.Add(part);
            filter.sharedMesh = baked;
            filter.transform.localScale = part.Scale / factor;
            foreach (Child child in part.Children)
            {
                child.Transform.localPosition *= factor;
                child.Transform.localScale *= factor;
            }
            foreach (Shape shape in part.Shapes) shape.Apply(factor);
        }

        public static Mesh BakedMesh(Mesh source, float factor)
        {
            Dictionary<float, Mesh> variants;
            if (!Cache.TryGetValue(source, out variants)) { variants = new Dictionary<float, Mesh>(); Cache.Add(source, variants); }
            Mesh result;
            if (variants.TryGetValue(factor, out result) && result != null) return result;
            result = source.isReadable ? UnityEngine.Object.Instantiate(source) : CombatUnreadableMeshes.Copy(source);
            try
            {
                Vector3[] vertices = result.vertices;
                for (int i = 0; i < vertices.Length; i++) vertices[i] *= factor;
                result.vertices = vertices;
                result.RecalculateBounds();
                result.name = source.name + " [EEO combat baked]";
                variants[factor] = result;
                return result;
            }
            catch { UnityEngine.Object.Destroy(result); throw; }
        }

        private void LateUpdate()
        {
            if (restored) return;
            if (++frame == 2)
            {
                enabled = false; // One readback; scale baking is initialization-only.
                float maxError = 0f;
                foreach (Part part in parts)
                {
                    if (part.Filter == null) continue;
                    Vector3 expected = part.Source.bounds.size * factor;
                    maxError = Math.Max(maxError, (part.Baked.bounds.size - expected).magnitude / Math.Max(0.000000001f, expected.magnitude));
                }
                Main.Log("Combat render bake readback: " + name + " parts=" + parts.Count + " maxLocalBoundsError=" + maxError.ToString("G6"));
            }
        }

        public static void RestoreOn(Transform root)
        {
            if (root == null) return;
            CombatRenderGeometry value = root.GetComponent<CombatRenderGeometry>();
            if (value != null) value.Restore();
        }
        public void Restore()
        {
            if (restored) return;
            restored = true;
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                Part part = parts[i];
                if (part.Filter == null) continue;
                if (part.Filter.sharedMesh == part.Baked) part.Filter.sharedMesh = part.Source;
                part.Filter.transform.localScale = part.Scale;
                foreach (Shape shape in part.Shapes) if (shape.Collider != null) shape.Apply(1f);
                foreach (Child child in part.Children)
                {
                    if (child.Transform == null) continue;
                    child.Transform.localPosition = child.Position;
                    child.Transform.localScale = child.Scale;
                }
            }
            enabled = false;
            if (!destroying) Destroy(this);
        }
        private void OnDestroy() { destroying = true; Restore(); }
    }

    public static class CombatUnreadableMeshes
    {
        private sealed class Record
        {
            internal int Count;
            internal string Entry;
            internal string DataFile;
            internal float[][] Channels;
            internal int[] Widths;
            internal int[][] Triangles;
        }
        private static Dictionary<string, Record> records;
        private static string dataPath;
        public static int Load(string path, string assemblyPath)
        {
            var next = new Dictionary<string, Record>(StringComparer.Ordinal);
            using (var archive = ZipFile.OpenRead(path))
            using (var reader = new BinaryReader(archive.GetEntry("header").Open()))
            {
                if (Text(reader) != "EEO-COMBAT-MESH-3") throw new InvalidDataException("Unsupported combat mesh schema.");
                string expected = Text(reader);
                using (var hash = SHA256.Create())
                using (var stream = File.OpenRead(assemblyPath))
                    if (BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "") != expected)
                        throw new InvalidDataException("Combat mesh fallback game assembly changed.");
                int count = Count(reader, 2000);
                for (int i = 0; i < count; i++)
                {
                    string name = Text(reader);
                    var record = new Record { Count = Count(reader, 1000000), Entry = Text(reader), DataFile = Text(reader) };
                    if (record.Entry.Length != 64 || (record.DataFile != "combat-mesh-data-base.eeo" && record.DataFile != "combat-mesh-data-dlc.eeo") ||
                        !File.Exists(Path.Combine(Path.GetDirectoryName(path), record.DataFile)))
                        throw new InvalidDataException("Missing guarded combat mesh payload.");
                    string key = name + ":" + record.Count;
                    if (next.ContainsKey(key))
                    {
                        if (next[key].Entry != record.Entry) throw new InvalidDataException("Ambiguous combat mesh fingerprint: " + key);
                        continue;
                    }
                    next.Add(key, record);
                }
                if (reader.BaseStream.ReadByte() != -1) throw new InvalidDataException("Trailing combat mesh header bytes.");
            }
            records = next;
            dataPath = path;
            return next.Count;
        }
        private static void Read(Record record)
        {
            if (record.Channels != null) return;
            byte[] payload;
            using (var archive = ZipFile.OpenRead(Path.Combine(Path.GetDirectoryName(dataPath), record.DataFile)))
            using (var stream = archive.GetEntry(record.Entry).Open())
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                payload = memory.ToArray();
            }
            using (var hash = SHA256.Create())
                if (BitConverter.ToString(hash.ComputeHash(payload)).Replace("-", "") != record.Entry)
                    throw new InvalidDataException("Combat mesh payload checksum mismatch.");
            using (var reader = new BinaryReader(new MemoryStream(payload)))
            {
                if (Count(reader, 1000000) != record.Count) throw new InvalidDataException("Combat mesh vertex fingerprint mismatch.");
                record.Channels = new float[12][]; record.Widths = new int[12];
                for (int channel = 0; channel < 12; channel++)
                {
                    int width = Count(reader, 4);
                    record.Widths[channel] = width;
                    float[] data = record.Channels[channel] = new float[record.Count * width];
                    for (int v = 0; v < data.Length; v++)
                    { data[v] = reader.ReadSingle(); if (float.IsNaN(data[v]) || float.IsInfinity(data[v])) throw new InvalidDataException("Nonfinite mesh data."); }
                }
                if (record.Widths[0] != 3) throw new InvalidDataException("Combat mesh has no 3D vertices.");
                record.Triangles = new int[Count(reader, 128)][];
                for (int sub = 0; sub < record.Triangles.Length; sub++)
                {
                    int[] indices = record.Triangles[sub] = new int[Count(reader, 10000000)];
                    if (indices.Length % 3 != 0) throw new InvalidDataException("Mesh triangle count invalid.");
                    for (int v = 0; v < indices.Length; v++)
                    { indices[v] = reader.ReadInt32(); if (indices[v] < 0 || indices[v] >= record.Count) throw new InvalidDataException("Mesh index out of bounds."); }
                }
                if (reader.BaseStream.Position != reader.BaseStream.Length) throw new InvalidDataException("Trailing combat mesh payload bytes.");
            }
        }
        public static int VerifyPayloads()
        {
            if (records == null) throw new InvalidOperationException("Load combat mesh data before validation.");
            var checkedEntries = new Dictionary<string, int>();
            foreach (Record record in records.Values)
            {
                int vertices;
                if (checkedEntries.TryGetValue(record.Entry, out vertices))
                { if (vertices != record.Count) throw new InvalidDataException("Aliased mesh vertex count mismatch."); continue; }
                Read(record);
                checkedEntries.Add(record.Entry, record.Count);
                // The standalone validator need not retain every decoded mesh.
                record.Channels = null; record.Widths = null; record.Triangles = null;
            }
            return checkedEntries.Count;
        }
        private static int Count(BinaryReader reader, int maximum)
        { int value = reader.ReadInt32(); if (value < 0 || value > maximum) throw new InvalidDataException("Invalid combat mesh size."); return value; }
        private static string Text(BinaryReader reader) { return System.Text.Encoding.UTF8.GetString(reader.ReadBytes(Count(reader, 4096))); }
        private static Vector3[] Vectors(Record r, int channel)
        {
            var result = new Vector3[r.Count];
            int w = r.Widths[channel];
            for (int i = 0; i < result.Length; i++) result[i] = new Vector3(r.Channels[channel][i*w], r.Channels[channel][i*w+1], r.Channels[channel][i*w+2]);
            return result;
        }
        public static Mesh Copy(Mesh source)
        {
            if (records == null)
                Load(Path.Combine(Main.mod.Path, "Config", "combat-unreadable-meshes.eeo"), typeof(ShipModelController).Assembly.Location);
            string name = source.name.Replace(" Instance", "");
            Record r;
            if (!records.TryGetValue(name + ":" + source.vertexCount, out r))
                throw new InvalidDataException("No guarded CPU mesh copy for " + source.name);
            Read(r);
            var mesh = new Mesh();
            if (r.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = Vectors(r, 0);
            if (r.Widths[1] >= 3) mesh.normals = Vectors(r, 1);
            if (r.Widths[2] == 4)
            {
                var array = new Vector4[r.Count];
                for (int i = 0; i < array.Length; i++) array[i] = new Vector4(r.Channels[2][4*i],r.Channels[2][4*i+1],r.Channels[2][4*i+2],r.Channels[2][4*i+3]);
                mesh.tangents = array;
            }
            if (r.Widths[3] == 4)
            {
                var array = new Color[r.Count];
                for (int i = 0; i < array.Length; i++) array[i] = new Color(r.Channels[3][4*i],r.Channels[3][4*i+1],r.Channels[3][4*i+2],r.Channels[3][4*i+3]);
                mesh.colors = array;
            }
            for (int channel = 0; channel < 8; channel++)
            {
                int w = r.Widths[channel+4]; float[] data = r.Channels[channel+4];
                if (w == 0) continue;
                var uv = new List<Vector4>(r.Count);
                for (int i = 0; i < r.Count; i++) uv.Add(new Vector4(data[i*w], w>1?data[i*w+1]:0f,w>2?data[i*w+2]:0f,w>3?data[i*w+3]:0f));
                mesh.SetUVs(channel, uv);
            }
            mesh.subMeshCount = r.Triangles.Length;
            for (int i = 0; i < r.Triangles.Length; i++) mesh.SetTriangles(r.Triangles[i], i, false);
            mesh.RecalculateBounds();
            // Retain the compact metadata only; meshes themselves are cached.
            r.Channels = null; r.Widths = null; r.Triangles = null;
            if ((mesh.bounds.center - source.bounds.center).magnitude > 0.05f || (mesh.bounds.size - source.bounds.size).magnitude > 0.1f)
            { UnityEngine.Object.Destroy(mesh); throw new InvalidDataException("Combat mesh bounds fingerprint mismatch: " + source.name); }
            return mesh;
        }
    }
}
