using PavonisInteractive.TerraInvicta;
using System.Collections.Generic;
using TIEconomyMod.Core;
using UnityEngine;

namespace TIEconomyMod
{
    public static class UtilityFootprintRuntime
    {
        public static List<int> GetFootprintSlotIndices(
            TIShipHullTemplate hull,
            int anchorSlotIndex,
            UtilityFootprintKind footprint)
        {
            List<int> indices = new List<int>();
            if (hull == null || anchorSlotIndex < 0 ||
                anchorSlotIndex >= hull.shipModuleSlots.Count)
            {
                return indices;
            }

            TIShipHullTemplate.ShipModuleSlot anchor =
                hull.shipModuleSlots[anchorSlotIndex];
            ShipModuleSlotType slotType = anchor.moduleSlotType;

            List<UtilityGridCell> cells = UtilityFootprintMath.GetCells(
                new UtilityGridCell(anchor.x, anchor.y), footprint);
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                int slotIndex = FindSlotIndex(
                    hull, cells[cellIndex], slotType);
                if (slotIndex < 0)
                {
                    indices.Clear();
                    return indices;
                }

                indices.Add(slotIndex);
            }

            return indices;
        }

        public static bool TryResolvePlacement(
            TISpaceShipTemplate design,
            TIShipPartTemplate module,
            Vector2Int droppedCoordinates,
            bool allowAlternateAnchors,
            out Vector2Int anchorCoordinates)
        {
            anchorCoordinates = droppedCoordinates;
            if (design == null || design.hullTemplate == null || module == null)
            {
                return false;
            }

            UtilityFootprintKind footprint =
                UtilityFootprintRegistry.GetFootprint(module);
            TIShipHullTemplate hull = design.hullTemplate;
            int droppedSlotIndex = FindAllowedSlotIndex(
                hull,
                new UtilityGridCell(
                    droppedCoordinates.x, droppedCoordinates.y),
                module.allowedSlots);
            if (droppedSlotIndex < 0)
            {
                return false;
            }

            ShipModuleSlotType slotType =
                hull.shipModuleSlots[droppedSlotIndex].moduleSlotType;
            if (footprint == UtilityFootprintKind.Single)
            {
                return design.GetPartInHullSlotIndex(
                    droppedSlotIndex, true) == null;
            }

            List<UtilityGridCell> orderedAnchors =
                new List<UtilityGridCell>();
            HashSet<UtilityGridCell> available =
                new HashSet<UtilityGridCell>();
            HashSet<UtilityGridCell> occupied =
                new HashSet<UtilityGridCell>();
            for (int index = 0; index < hull.shipModuleSlots.Count; index++)
            {
                TIShipHullTemplate.ShipModuleSlot slot =
                    hull.shipModuleSlots[index];
                if (slot.moduleSlotType != slotType)
                {
                    continue;
                }

                UtilityGridCell cell = new UtilityGridCell(slot.x, slot.y);
                orderedAnchors.Add(cell);
                available.Add(cell);
                if (design.GetPartInHullSlotIndex(index, true) != null)
                {
                    occupied.Add(cell);
                }
            }

            UtilityGridCell resolvedAnchor;
            if (!UtilityFootprintMath.TryResolveAnchor(
                new UtilityGridCell(
                    droppedCoordinates.x, droppedCoordinates.y),
                footprint,
                orderedAnchors,
                available,
                occupied,
                allowAlternateAnchors,
                out resolvedAnchor))
            {
                return false;
            }

            anchorCoordinates = new Vector2Int(
                resolvedAnchor.X, resolvedAnchor.Y);
            return true;
        }

        public static bool HasCompatiblePlacement(
            TISpaceShipTemplate design,
            TIShipPartTemplate module)
        {
            if (design == null || design.hullTemplate == null ||
                module == null)
            {
                return false;
            }

            TIShipHullTemplate hull = design.hullTemplate;
            for (int index = 0; index < hull.shipModuleSlots.Count; index++)
            {
                TIShipHullTemplate.ShipModuleSlot slot =
                    hull.shipModuleSlots[index];
                if (!module.allowedSlots.Contains(slot.moduleSlotType))
                {
                    continue;
                }

                List<int> cells = GetFootprintSlotIndices(
                    hull,
                    index,
                    UtilityFootprintRegistry.GetFootprint(module));
                if (cells.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryFindFootprintOccupant(
            TISpaceShipTemplate design,
            int queriedSlotIndex,
            out TIShipPartTemplate module,
            out int anchorSlotIndex,
            out List<int> footprintSlotIndices)
        {
            module = null;
            anchorSlotIndex = -1;
            footprintSlotIndices = null;
            if (design == null || design.hullTemplate == null ||
                queriedSlotIndex < 0 ||
                queriedSlotIndex >= design.hullTemplate.shipModuleSlots.Count)
            {
                return false;
            }

            for (int index = 0;
                index < design.moduleTemplateEntries.Count;
                index++)
            {
                ModuleDataTemplateEntry entry =
                    design.moduleTemplateEntries[index];
                TIShipPartTemplate candidate = FindPart(entry.moduleName);
                if (candidate != null && entry.slot == queriedSlotIndex)
                {
                    module = candidate;
                    anchorSlotIndex = entry.slot;
                    footprintSlotIndices = GetEffectiveFootprintSlotIndices(
                        design, entry, candidate);
                    return true;
                }
            }

            if (IsLegacyLayout(design))
            {
                return false;
            }

            for (int index = 0;
                index < design.moduleTemplateEntries.Count;
                index++)
            {
                ModuleDataTemplateEntry entry =
                    design.moduleTemplateEntries[index];
                TIShipPartTemplate candidate = FindPart(entry.moduleName);
                UtilityFootprintKind footprint =
                    UtilityFootprintRegistry.GetFootprint(candidate);
                if (candidate == null ||
                    footprint == UtilityFootprintKind.Single)
                {
                    continue;
                }

                List<int> indices = GetFootprintSlotIndices(
                    design.hullTemplate, entry.slot, footprint);
                if (indices.Contains(queriedSlotIndex))
                {
                    module = candidate;
                    anchorSlotIndex = entry.slot;
                    footprintSlotIndices = indices;
                    return true;
                }
            }

            return false;
        }

        public static bool IsLegacyLayout(TISpaceShipTemplate design)
        {
            if (design == null || design.hullTemplate == null ||
                !UtilityFootprintRegistry.Enabled)
            {
                return false;
            }

            HashSet<int> claimedSlots = new HashSet<int>();
            for (int index = 0;
                index < design.moduleTemplateEntries.Count;
                index++)
            {
                ModuleDataTemplateEntry entry =
                    design.moduleTemplateEntries[index];
                if (entry.slot < 0 ||
                    entry.slot >= design.hullTemplate.shipModuleSlots.Count)
                {
                    continue;
                }

                TIShipPartTemplate part = FindPart(entry.moduleName);
                if (part == null)
                {
                    continue;
                }
                UtilityFootprintKind footprint =
                    UtilityFootprintRegistry.GetFootprint(part);
                List<int> cells = GetFootprintSlotIndices(
                    design.hullTemplate, entry.slot, footprint);
                if (cells.Count == 0)
                {
                    return true;
                }

                for (int cellIndex = 0;
                    cellIndex < cells.Count;
                    cellIndex++)
                {
                    if (!claimedSlots.Add(cells[cellIndex]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<ModuleDataTemplateEntry> PackUtilityModules(
            TISpaceShipTemplate design,
            IList<ModuleDataTemplateEntry> modules)
        {
            List<ModuleDataTemplateEntry> packed =
                new List<ModuleDataTemplateEntry>();
            if (design == null || design.hullTemplate == null ||
                modules == null)
            {
                return packed;
            }

            TIShipHullTemplate hull = design.hullTemplate;
            List<int> utilitySlots = new List<int>();
            for (int index = 0; index < hull.shipModuleSlots.Count; index++)
            {
                if (hull.shipModuleSlots[index].moduleSlotType ==
                    ShipModuleSlotType.Utility)
                {
                    utilitySlots.Add(index);
                }
            }

            HashSet<int> occupied = new HashSet<int>();
            for (int moduleIndex = 0;
                moduleIndex < modules.Count;
                moduleIndex++)
            {
                ModuleDataTemplateEntry source = modules[moduleIndex];
                TIUtilityModuleTemplate utility = FindUtility(source.moduleName);
                UtilityFootprintKind footprint =
                    UtilityFootprintRegistry.GetFootprint(utility);
                for (int anchorIndex = 0;
                    anchorIndex < utilitySlots.Count;
                    anchorIndex++)
                {
                    int anchorSlot = utilitySlots[anchorIndex];
                    List<int> cells = GetFootprintSlotIndices(
                        hull, anchorSlot, footprint);
                    if (cells.Count == 0 || Overlaps(cells, occupied))
                    {
                        continue;
                    }

                    ModuleDataTemplateEntry placed =
                        new ModuleDataTemplateEntry();
                    placed.moduleName = source.moduleName;
                    placed.slot = anchorSlot;
                    packed.Add(placed);
                    for (int cellIndex = 0;
                        cellIndex < cells.Count;
                        cellIndex++)
                    {
                        occupied.Add(cells[cellIndex]);
                    }
                    break;
                }
            }

            return packed;
        }

        public static Mount GetDisplayMount(UtilityFootprintKind footprint)
        {
            switch (footprint)
            {
            case UtilityFootprintKind.TwoHorizontal:
                return Mount.TwoHullHoriz;
            case UtilityFootprintKind.TwoVertical:
                return Mount.TwoHullVert;
            case UtilityFootprintKind.Four:
                return Mount.FourHull;
            default:
                return Mount.Standard;
            }
        }

        private static List<int> GetEffectiveFootprintSlotIndices(
            TISpaceShipTemplate design,
            ModuleDataTemplateEntry entry,
            TIShipPartTemplate part)
        {
            if (IsLegacyLayout(design))
            {
                return new List<int> { entry.slot };
            }

            return GetFootprintSlotIndices(
                design.hullTemplate,
                entry.slot,
                UtilityFootprintRegistry.GetFootprint(part));
        }

        private static TIShipPartTemplate FindPart(string dataName)
        {
            TIUtilityModuleTemplate utility = FindUtility(dataName);
            if (utility != null)
            {
                return utility;
            }

            return string.IsNullOrEmpty(dataName)
                ? null
                : TemplateManager.Find<TIHeatSinkTemplate>(dataName, false);
        }

        private static TIUtilityModuleTemplate FindUtility(string dataName)
        {
            if (string.IsNullOrEmpty(dataName))
            {
                return null;
            }

            return TemplateManager.Find<TIUtilityModuleTemplate>(
                dataName, false);
        }

        private static int FindSlotIndex(
            TIShipHullTemplate hull,
            UtilityGridCell cell,
            ShipModuleSlotType slotType)
        {
            for (int index = 0; index < hull.shipModuleSlots.Count; index++)
            {
                TIShipHullTemplate.ShipModuleSlot slot =
                    hull.shipModuleSlots[index];
                if (slot.moduleSlotType == slotType &&
                    slot.x == cell.X && slot.y == cell.Y)
                {
                    return index;
                }
            }

            return -1;
        }

        private static int FindAllowedSlotIndex(
            TIShipHullTemplate hull,
            UtilityGridCell cell,
            IList<ShipModuleSlotType> allowedSlotTypes)
        {
            for (int index = 0; index < hull.shipModuleSlots.Count; index++)
            {
                TIShipHullTemplate.ShipModuleSlot slot =
                    hull.shipModuleSlots[index];
                if (allowedSlotTypes.Contains(slot.moduleSlotType) &&
                    slot.x == cell.X && slot.y == cell.Y)
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool Overlaps(
            IList<int> cells,
            ISet<int> occupied)
        {
            for (int index = 0; index < cells.Count; index++)
            {
                if (occupied.Contains(cells[index]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
