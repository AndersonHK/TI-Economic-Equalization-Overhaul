using System;
using UnityEngine;

namespace TIEconomyMod.Core
{
    // Source geometry and surface probes: docs/ship-balance-research/turret-clearance/mothership-layout-2026-10-21.json
    // All twenty authored pairs are spaced; four remain unavailable in the native UI.
    public static class MothershipMountGeometry
    {
        public const int Count = 20;
        private static readonly string[] Suffixes = { "70", "60", "50", "40", "72", "7C", "74", "62", "6C", "64", "52", "5C", "54", "42", "4C", "44", "76", "66", "56", "46" };
        private static readonly Vector3[] original_dorsal =
        {
            new Vector3(-108.5f, 25.0f, 249.0f), // 0
            new Vector3(-196.5f, 25.1000004f, 96.0f), // 1
            new Vector3(-196.5f, 25.3500004f, -161.399994f), // 2
            new Vector3(-88.4000015f, 25.3500004f, -273.75f), // 3
            new Vector3(-4.0f, 88.8899994f, 3.0f), // 4
            new Vector3(0.0f, 88.8899994f, 3.0f), // 5
            new Vector3(4.0f, 88.8899994f, 3.0f), // 6
            new Vector3(-4.0f, 88.909996f, -7.0f), // 7
            new Vector3(0.0f, 88.909996f, -7.0f), // 8
            new Vector3(4.0f, 88.909996f, -7.0f), // 9
            new Vector3(-4.0f, 88.909996f, -17.0f), // 10
            new Vector3(0.0f, 88.909996f, -17.0f), // 11
            new Vector3(4.0f, 88.909996f, -17.0f), // 12
            new Vector3(-4.0f, 88.909996f, -27.0f), // 13
            new Vector3(0.0f, 88.909996f, -27.0f), // 14
            new Vector3(4.0f, 88.909996f, -27.0f), // 15
            new Vector3(108.5f, 25.0f, 249.0f), // 16
            new Vector3(196.5f, 25.1000004f, 96.0f), // 17
            new Vector3(196.5f, 25.3500004f, -161.399994f), // 18
            new Vector3(88.4000168f, 25.3500004f, -273.750031f), // 19
        };
        private static readonly Vector3[] original_ventral =
        {
            new Vector3(-108.5f, -22.9659996f, 249.0f), // 0
            new Vector3(-196.5f, -23.1800003f, 96.0f), // 1
            new Vector3(-196.5f, -23.5900002f, -161.399994f), // 2
            new Vector3(-88.4000015f, -23.5900002f, -273.75f), // 3
            new Vector3(-4.0f, -88.8199997f, 3.0f), // 4
            new Vector3(0.0f, -88.8199997f, 3.0f), // 5
            new Vector3(4.0f, -88.8199997f, 3.0f), // 6
            new Vector3(-4.0f, -88.8199997f, -7.0f), // 7
            new Vector3(0.0f, -88.8199997f, -7.0f), // 8
            new Vector3(4.0f, -88.8199997f, -7.0f), // 9
            new Vector3(-4.0f, -88.8199997f, -17.0f), // 10
            new Vector3(0.0f, -88.8199997f, -17.0f), // 11
            new Vector3(4.0f, -88.8199997f, -17.0f), // 12
            new Vector3(-4.0f, -88.8199997f, -27.0f), // 13
            new Vector3(0.0f, -88.8199997f, -27.0f), // 14
            new Vector3(4.0f, -88.8199997f, -27.0f), // 15
            new Vector3(108.5f, -22.9659996f, 249.0f), // 16
            new Vector3(196.5f, -23.1800003f, 96.0f), // 17
            new Vector3(-196.5f, -23.1800003f, 96.0f), // 18
            new Vector3(88.4000168f, -23.5900002f, -273.750031f), // 19
        };
        private static readonly Vector3[] dorsal =
        {
            new Vector3(-108.5f, 25.0f, 249.0f), // 0
            new Vector3(-196.5f, 25.1f, 96.0f), // 1
            new Vector3(-196.5f, 25.35f, -161.399994f), // 2
            new Vector3(-88.400002f, 25.35f, -273.75f), // 3
            new Vector3(-196.5f, 25.135874f, 61.8f), // 4
            new Vector3(0.0f, 88.91f, 7.063f), // 5
            new Vector3(196.5f, 25.135874f, 61.8f), // 6
            new Vector3(-196.5f, 25.19645f, -6.6f), // 7
            new Vector3(-196.5f, 25.165888f, 27.6f), // 8
            new Vector3(196.5f, 25.19645f, -6.6f), // 9
            new Vector3(-196.5f, 25.227309f, -40.8f), // 10
            new Vector3(196.5f, 25.165888f, 27.6f), // 11
            new Vector3(196.5f, 25.227309f, -40.8f), // 12
            new Vector3(-196.5f, 25.322998f, -120.0f), // 13
            new Vector3(0.0f, 88.91f, -27.337f), // 14
            new Vector3(196.5f, 25.322998f, -120.0f), // 15
            new Vector3(108.5f, 25.0f, 249.0f), // 16
            new Vector3(196.5f, 25.1f, 96.0f), // 17
            new Vector3(196.5f, 25.35f, -161.399994f), // 18
            new Vector3(88.400002f, 25.35f, -273.75f), // 19
        };
        private static readonly Vector3[] ventral =
        {
            new Vector3(-108.5f, -22.966f, 249.0f), // 0
            new Vector3(-196.5f, -23.18f, 96.0f), // 1
            new Vector3(-196.5f, -23.59f, -161.399994f), // 2
            new Vector3(-88.400002f, -23.59f, -273.75f), // 3
            new Vector3(-196.5f, -23.252537f, 61.8f), // 4
            new Vector3(0.0f, -88.91f, 7.063f), // 5
            new Vector3(196.5f, -23.252537f, 61.8f), // 6
            new Vector3(-196.5f, -23.34972f, -6.6f), // 7
            new Vector3(-196.5f, -23.300595f, 27.6f), // 8
            new Vector3(196.5f, -23.34972f, -6.6f), // 9
            new Vector3(-196.5f, -23.399424f, -40.8f), // 10
            new Vector3(196.5f, -23.300595f, 27.6f), // 11
            new Vector3(196.5f, -23.399424f, -40.8f), // 12
            new Vector3(-196.5f, -23.552573f, -120.0f), // 13
            new Vector3(0.0f, -88.91f, -27.337f), // 14
            new Vector3(196.5f, -23.552573f, -120.0f), // 15
            new Vector3(108.5f, -22.966f, 249.0f), // 16
            new Vector3(196.5f, -23.18f, 96.0f), // 17
            new Vector3(196.5f, -23.59f, -161.399994f), // 18
            new Vector3(88.400002f, -23.59f, -273.75f), // 19
        };

        public static string Name(int index, bool ventralSide)
        {
            return (ventralSide ? "Ventral" : "Dorsal") + " Turret Mount Base " + Suffixes[index];
        }

        public static Vector3 Original(int index, bool ventralSide)
        {
            return (ventralSide ? original_ventral : original_dorsal)[index];
        }

        public static Vector3 Target(int index, bool ventralSide)
        {
            return (ventralSide ? ventral : dorsal)[index];
        }

        public static bool Matches(Vector3 actual, Vector3 expected)
        {
            // Component comparisons also reject NaN, without Unity native calls.
            return Math.Abs(actual.x - expected.x) <= 0.0025f &&
                Math.Abs(actual.y - expected.y) <= 0.0025f && Math.Abs(actual.z - expected.z) <= 0.0025f;
        }

        public static bool ValidatePositions(Vector3[] top, Vector3[] bottom, out string reason)
        {
            reason = null;
            if (top == null || bottom == null || top.Length != Count || bottom.Length != Count)
            {
                reason = "expected twenty dorsal and twenty ventral authored controllers";
                return false;
            }
            for (int side = 0; side < 2; ++side)
            {
                Vector3[] values = side == 0 ? top : bottom;
                for (int index = 0; index < Count; ++index)
                    if (!Matches(values[index], Original(index, side == 1)) &&
                        !Matches(values[index], Target(index, side == 1)))
                    {
                        reason = "unexpected " + Name(index, side == 1) + " position";
                        return false;
                    }
            }
            return true;
        }
    }
}
