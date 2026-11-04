using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace TIEconomyMod.Core
{
    // JSON is private UI metadata, not a native game-template array.
    public sealed class ShipDesignerLayout
    {
        [JsonProperty(Required = Required.Always)] public int Schema;
        [JsonProperty(Required = Required.Always)] public int Columns;
        [JsonProperty(Required = Required.Always)] public int Rows;
        [JsonProperty(Required = Required.Always)] public int WindowRows;
        [JsonProperty(Required = Required.Always)] public float ColumnPitch;
        [JsonProperty(Required = Required.Always)] public float HalfRowPitch;
        [JsonProperty(Required = Required.Always)] public float OriginX;
        [JsonProperty(Required = Required.Always)] public float OriginY;
        [JsonProperty(Required = Required.Always)] public string CenterPanelPath;
        [JsonProperty(Required = Required.Always)] public string[] FixedHeightPanels;
        [JsonProperty(Required = Required.Always)] public ShipDesignerLabelLayout PropellantLabel;
        [JsonProperty(Required = Required.Always)] public ShipDesignerLegacySlots[] LegacyWeaponSlots;
        [JsonProperty(Required = Required.Always)] public ShipDesignerColumnRule[] ColumnRestrictions;
        [JsonProperty(Required = Required.Always)] public ShipDesignerReservedCell[] ReservedCells;
        [JsonProperty(Required = Required.Always)] public ShipDesignerViewportSettings Viewport;

        public static ShipDesignerLayout Load(string path) { return Parse(File.ReadAllText(path)); }
        public static ShipDesignerLayout Parse(string json)
        {
            var data = JsonConvert.DeserializeObject<ShipDesignerLayout>(json,
                new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            if (data == null || data.Schema != 1 || data.Columns != 10 || data.Rows < 7 || data.Rows > 32 ||
                data.WindowRows < 7 || data.WindowRows > data.Rows ||
                !Finite(data.ColumnPitch) || data.ColumnPitch <= 0 || !Finite(data.HalfRowPitch) || data.HalfRowPitch <= 0 ||
                !Finite(data.OriginX) || !Finite(data.OriginY) || string.IsNullOrWhiteSpace(data.CenterPanelPath) ||
                data.FixedHeightPanels == null || Array.Exists(data.FixedHeightPanels, string.IsNullOrWhiteSpace) ||
                data.PropellantLabel == null || !Finite(data.PropellantLabel.Width) || data.PropellantLabel.Width <= 0 ||
                !Finite(data.PropellantLabel.Height) || data.PropellantLabel.Height <= 0 ||
                !Finite(data.PropellantLabel.OffsetY) || !Finite(data.PropellantLabel.MaximumFontSize) || data.PropellantLabel.MaximumFontSize <= 0)
                throw new InvalidDataException("Invalid ship designer layout schema or dimensions.");
            if (data.LegacyWeaponSlots == null) throw new InvalidDataException("Missing historical weapon slot layouts.");
            foreach (var history in data.LegacyWeaponSlots)
            {
                if (history == null || string.IsNullOrWhiteSpace(history.Hull) || history.MaximumSchema < 0 || history.Positions == null)
                    throw new InvalidDataException("Invalid historical weapon layout.");
                var indices = new HashSet<int>();
                foreach (var p in history.Positions)
                    if (p == null || p.Slot < 0 || !indices.Add(p.Slot) || p.X < 0 || p.X >= data.Columns || p.Y < 0 || p.Y >= data.Rows)
                        throw new InvalidDataException("Invalid historical weapon coordinates.");
            }
            if (data.ColumnRestrictions == null || data.ReservedCells == null) throw new InvalidDataException("Missing designer membership rules.");
            foreach (var rule in data.ColumnRestrictions)
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.Hull) || rule.Columns == null || rule.Columns.Length == 0 ||
                    rule.AllowedTypes == null || rule.AllowedTypes.Length == 0 || Array.Exists(rule.Columns, x => x < 0 || x >= data.Columns) ||
                    Array.Exists(rule.AllowedTypes, t => !Enum.IsDefined(typeof(ShipModuleSlotType), t)))
                    throw new InvalidDataException("Invalid designer column membership rule.");
            }
            foreach (var cell in data.ReservedCells)
                if (cell == null || string.IsNullOrWhiteSpace(cell.Hull) || cell.X < 0 || cell.X >= data.Columns || cell.Y < 0 || cell.Y >= data.Rows)
                    throw new InvalidDataException("Invalid reserved designer cell.");
            if (data.Viewport == null || string.IsNullOrWhiteSpace(data.Viewport.RegionPath) || data.Viewport.HeaderPaths == null ||
                Array.Exists(data.Viewport.HeaderPaths, string.IsNullOrWhiteSpace) ||
                !Finite(data.Viewport.PaddingTop) || data.Viewport.PaddingTop < 0 || !Finite(data.Viewport.PaddingBottom) || data.Viewport.PaddingBottom < 0 ||
                !Finite(data.Viewport.PaddingLeft) || data.Viewport.PaddingLeft < 0 || !Finite(data.Viewport.PaddingRight) || data.Viewport.PaddingRight < 0 ||
                !Finite(data.Viewport.ScreenMargin) || data.Viewport.ScreenMargin < 0)
                throw new InvalidDataException("Invalid designer viewport settings.");
            return data;
        }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        public int ResolveRows(int nativeRows, int maximumDeclaredY)
        {
            int rows = Math.Max(Rows, Math.Max(nativeRows, maximumDeclaredY + 1));
            if (maximumDeclaredY < 0 || rows > 32) throw new InvalidDataException("Hull grid exceeds supported designer rows.");
            return rows;
        }
        public float HeightGrowth(int nativeRows, int rows) { return Math.Max(0, Math.Min(rows, WindowRows) - nativeRows) * HalfRowPitch; }
        public float X(int column) { return OriginX + column * ColumnPitch; }
        public float Y(int row) { return OriginY - row * HalfRowPitch; }
        public bool AcceptsSlot(string hull, int x, int y, string type)
        {
            if (type == "None") return true;
            if (x < 0 || x >= Columns || y < 0 || y >= Rows) return false;
            foreach (var rule in ColumnRestrictions)
                if ((rule.Hull == "*" || rule.Hull == hull) && Array.IndexOf(rule.Columns, x) >= 0 && Array.IndexOf(rule.AllowedTypes, type) < 0) return false;
            foreach (var cell in ReservedCells)
                if ((cell.Hull == "*" || cell.Hull == hull) && cell.X == x && cell.Y == y) return false;
            return true;
        }
        public DesignerRect ValidateHull(TIShipHullTemplate hull)
        {
            if (hull.shipModuleSlots == null) return new DesignerRect();
            var tiles = new List<System.Tuple<int, DesignerRect>>();
            var controls = new List<System.Tuple<int, DesignerRect>>();
            var tail = hull.GetUniqueSlotCoordinates(ShipModuleSlotType.TailArmor);
            var nose = hull.GetUniqueSlotCoordinates(ShipModuleSlotType.NoseArmor);
            int index = 0;
            foreach (var s in hull.shipModuleSlots)
            {
                if (!AcceptsSlot(hull.dataName, s.x, s.y, s.moduleSlotType.ToString()))
                    throw new InvalidDataException("Invalid designer member " + s.moduleSlotType + " at " + hull.dataName + " (" + s.x + "," + s.y + ").");
                if (s.moduleSlotType == ShipModuleSlotType.None) { index++; continue; }
                float x = s.x * ColumnPitch, y = s.y * HalfRowPitch;
                if (s.armorSlot)
                {
                    y += 36f;
                    if (s.moduleSlotType == ShipModuleSlotType.LateralArmor && (tail.x + nose.x) % 2 != 0) x += 36f;
                }
                if (s.moduleSlotType != ShipModuleSlotType.Propellant)
                    tiles.Add(System.Tuple.Create(index, new DesignerRect(x - 36, y - 36, x + 36, y + 36)));
                if (s.moduleSlotType == ShipModuleSlotType.Drive)
                    controls.Add(System.Tuple.Create(index, new DesignerRect(x - 36, y + 38.5f, x + 36, y + 58.5f)));
                else if (s.armorSlot)
                    controls.Add(System.Tuple.Create(index, new DesignerRect(x - 36, y + 38.5f, x + 36, y + 80.5f)));
                else if (s.moduleSlotType == ShipModuleSlotType.Propellant)
                {
                    float half = Math.Max(36f, PropellantLabel.Width / 2);
                    controls.Add(System.Tuple.Create(index, new DesignerRect(x - half, y + 46 - PropellantLabel.OffsetY - PropellantLabel.Height / 2,
                        x + half, y + Math.Max(56, 46 - PropellantLabel.OffsetY + PropellantLabel.Height / 2))));
                }
                index++;
            }
            for (int i = 0; i < tiles.Count; ++i)
                for (int j = i + 1; j < tiles.Count; ++j)
                    if (tiles[i].Item2.Overlaps(tiles[j].Item2)) throw new InvalidDataException("Designer icons overlap in " + hull.dataName + ".");
            foreach (var control in controls)
                foreach (var tile in tiles)
                    if (control.Item1 != tile.Item1 && control.Item2.Overlaps(tile.Item2))
                        throw new InvalidDataException("Designer control overlaps another tile in " + hull.dataName + ".");
            bool any = false; DesignerRect result = new DesignerRect();
            foreach (var tile in tiles) { result = any ? DesignerRect.Union(result, tile.Item2) : tile.Item2; any = true; }
            foreach (var control in controls) { result = any ? DesignerRect.Union(result, control.Item2) : control.Item2; any = true; }
            return result;
        }
    }

    public sealed class ShipDesignerLabelLayout
    {
        [JsonProperty(Required = Required.Always)] public float Width;
        [JsonProperty(Required = Required.Always)] public float Height;
        [JsonProperty(Required = Required.Always)] public float OffsetY;
        [JsonProperty(Required = Required.Always)] public float MaximumFontSize;
    }

    public sealed class ShipDesignerLegacySlots
    {
        [JsonProperty(Required = Required.Always)] public string Hull;
        [JsonProperty(Required = Required.Always)] public int MaximumSchema;
        [JsonProperty(Required = Required.Always)] public ShipDesignerSlotPosition[] Positions;
    }
    public sealed class ShipDesignerSlotPosition
    {
        [JsonProperty(Required = Required.Always)] public int Slot;
        [JsonProperty(Required = Required.Always)] public int X;
        [JsonProperty(Required = Required.Always)] public int Y;
    }
    public sealed class ShipDesignerColumnRule
    {
        [JsonProperty(Required = Required.Always)] public string Hull;
        [JsonProperty(Required = Required.Always)] public int[] Columns;
        [JsonProperty(Required = Required.Always)] public string[] AllowedTypes;
    }
    public sealed class ShipDesignerReservedCell
    {
        [JsonProperty(Required = Required.Always)] public string Hull;
        [JsonProperty(Required = Required.Always)] public int X;
        [JsonProperty(Required = Required.Always)] public int Y;
    }
    public sealed class ShipDesignerViewportSettings
    {
        [JsonProperty(Required = Required.Always)] public string RegionPath;
        [JsonProperty(Required = Required.Always)] public string[] HeaderPaths;
        [JsonProperty(Required = Required.Always)] public float PaddingTop;
        [JsonProperty(Required = Required.Always)] public float PaddingBottom;
        [JsonProperty(Required = Required.Always)] public float PaddingLeft;
        [JsonProperty(Required = Required.Always)] public float PaddingRight;
        [JsonProperty(Required = Required.Always)] public float ScreenMargin;
    }
}
