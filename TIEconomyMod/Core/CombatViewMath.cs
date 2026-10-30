using System;
using UnityEngine;

namespace TIEconomyMod.Core
{
    public static class CombatViewMath
    {
        public static float RelativeModelScale(bool cinematic, float multiplier)
        {
            return BattleScaleMath.ModelScale(multiplier) / (cinematic ? 0.01f : 0.001f);
        }
        public static float RelativeProjectileScale(float multiplier)
        {
            // Native tracking's 2-unit envelope is mode-independent. Anchor it
            // to the former 50x projectile presentation, not the cinematic flag.
            return BattleScaleMath.ProjectileScale(multiplier) / 0.0025f;
        }

        // Stop at the first intersection, even if a large step would end on the
        // far side of the sphere. Moving away from an inside start is allowed.
        public static Vector3 SafeDollyPoint(Vector3 start, Vector3 proposed, Vector3 center, float radius)
        {
            Vector3 offset = start - center, delta = proposed - start;
            double a = delta.sqrMagnitude;
            if (a <= 0.0) return proposed;
            double b = Vector3.Dot(offset, delta);
            double c = offset.sqrMagnitude - (double)radius * radius;
            if (c <= 0.0) return b < 0.0 ? start : proposed;
            double discriminant = b * b - a * c;
            if (b >= 0.0 || discriminant < 0.0) return proposed;
            double entry = (-b - Math.Sqrt(discriminant)) / a;
            if (entry < 0.0 || entry > 1.0) return proposed;
            return start + delta * (float)Math.Max(0.0, entry - 0.00001);
        }
    }
}
