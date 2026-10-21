using System;
using System.Collections.Generic;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Preserve world geometry while avoiding unusually small renderer transforms.
    // Owned copies retain the source normals, tangents, UVs and topology.
    public static class GunpowderArtGeometry
    {
        private sealed class Pair { internal Mesh SourceBase, SourceWeapon, Base, Weapon; }
        private static readonly Dictionary<string, Pair> Cache = new Dictionary<string, Pair>(StringComparer.Ordinal);
        public static Vector3 ScalePoint(Vector3 source, float factor) { return source * factor; }
        public static void Get(GunpowderArtProfile profile, Mesh sourceBase, Mesh sourceWeapon,
            out Mesh bakedBase, out Mesh bakedWeapon)
        {
            Pair pair;
            if (!Cache.TryGetValue(profile.Weapon, out pair) || pair.SourceBase != sourceBase ||
                pair.SourceWeapon != sourceWeapon || pair.Base == null || pair.Weapon == null)
            {
                if (!sourceBase.isReadable || !sourceWeapon.isReadable)
                    throw new InvalidOperationException("Gunpowder source mesh is not readable; cannot bake art dimensions.");
                pair = new Pair { SourceBase = sourceBase, SourceWeapon = sourceWeapon };
                pair.Base = Bake(sourceBase, profile.Factor, profile.Weapon);
                try { pair.Weapon = Bake(sourceWeapon, profile.Factor, profile.Weapon); }
                catch { UnityEngine.Object.Destroy(pair.Base); throw; }
                Cache[profile.Weapon] = pair;
            }
            bakedBase = pair.Base; bakedWeapon = pair.Weapon;
        }
        private static Mesh Bake(Mesh source, float factor, string weapon)
        {
            Mesh mesh = UnityEngine.Object.Instantiate(source);
            mesh.name = source.name + " [EEO baked " + weapon + "]";
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; ++i) vertices[i] = ScalePoint(vertices[i], factor);
            mesh.vertices = vertices; mesh.RecalculateBounds();
            return mesh;
        }
    }
}
