using System;
using System.Collections.Generic;

namespace TIEconomyMod.Core
{
    public enum UtilityFootprintKind
    {
        Single = 0,
        TwoHorizontal = 1,
        TwoVertical = 2,
        Four = 3
    }

    public struct UtilityGridCell : IEquatable<UtilityGridCell>
    {
        public readonly int X;
        public readonly int Y;

        public UtilityGridCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(UtilityGridCell other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object value)
        {
            return value is UtilityGridCell && Equals((UtilityGridCell)value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }
    }

    public static class UtilityFootprintMath
    {
        private static readonly UtilityGridCell[] singleOffsets =
        {
            new UtilityGridCell(0, 0)
        };

        private static readonly UtilityGridCell[] twoHorizontalOffsets =
        {
            new UtilityGridCell(0, 0),
            new UtilityGridCell(1, 0)
        };

        private static readonly UtilityGridCell[] twoVerticalOffsets =
        {
            new UtilityGridCell(0, 0),
            new UtilityGridCell(0, 2)
        };

        private static readonly UtilityGridCell[] fourOffsets =
        {
            new UtilityGridCell(0, 0),
            new UtilityGridCell(1, 0),
            new UtilityGridCell(0, 2),
            new UtilityGridCell(1, 2)
        };

        public static IList<UtilityGridCell> GetOffsets(
            UtilityFootprintKind footprint)
        {
            switch (footprint)
            {
            case UtilityFootprintKind.TwoHorizontal:
                return twoHorizontalOffsets;
            case UtilityFootprintKind.TwoVertical:
                return twoVerticalOffsets;
            case UtilityFootprintKind.Four:
                return fourOffsets;
            default:
                return singleOffsets;
            }
        }

        public static List<UtilityGridCell> GetCells(
            UtilityGridCell anchor,
            UtilityFootprintKind footprint)
        {
            IList<UtilityGridCell> offsets = GetOffsets(footprint);
            List<UtilityGridCell> cells =
                new List<UtilityGridCell>(offsets.Count);
            for (int index = 0; index < offsets.Count; index++)
            {
                UtilityGridCell offset = offsets[index];
                cells.Add(new UtilityGridCell(
                    anchor.X + offset.X,
                    anchor.Y + offset.Y));
            }

            return cells;
        }

        public static bool TryResolveAnchor(
            UtilityGridCell droppedCell,
            UtilityFootprintKind footprint,
            IList<UtilityGridCell> orderedCandidateAnchors,
            ISet<UtilityGridCell> availableCells,
            ISet<UtilityGridCell> occupiedCells,
            bool allowAlternateAnchors,
            out UtilityGridCell anchor)
        {
            int bestDroppedCellPosition = int.MaxValue;
            int bestCandidatePosition = int.MaxValue;
            UtilityGridCell bestAnchor = default(UtilityGridCell);

            for (int candidateIndex = 0;
                candidateIndex < orderedCandidateAnchors.Count;
                candidateIndex++)
            {
                UtilityGridCell candidate =
                    orderedCandidateAnchors[candidateIndex];
                if (!allowAlternateAnchors && !candidate.Equals(droppedCell))
                {
                    continue;
                }

                List<UtilityGridCell> cells = GetCells(candidate, footprint);
                int droppedCellPosition = cells.IndexOf(droppedCell);
                if (droppedCellPosition < 0)
                {
                    continue;
                }

                bool legal = true;
                for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                {
                    UtilityGridCell cell = cells[cellIndex];
                    if (!availableCells.Contains(cell) ||
                        occupiedCells.Contains(cell))
                    {
                        legal = false;
                        break;
                    }
                }

                if (!legal ||
                    droppedCellPosition > bestDroppedCellPosition ||
                    (droppedCellPosition == bestDroppedCellPosition &&
                        candidateIndex >= bestCandidatePosition))
                {
                    continue;
                }

                bestDroppedCellPosition = droppedCellPosition;
                bestCandidatePosition = candidateIndex;
                bestAnchor = candidate;
            }

            if (bestCandidatePosition == int.MaxValue)
            {
                anchor = default(UtilityGridCell);
                return false;
            }

            anchor = bestAnchor;
            return true;
        }

        public static bool HasCompatibleAnchor(
            UtilityFootprintKind footprint,
            IList<UtilityGridCell> orderedCandidateAnchors,
            ISet<UtilityGridCell> availableCells)
        {
            for (int candidateIndex = 0;
                candidateIndex < orderedCandidateAnchors.Count;
                candidateIndex++)
            {
                List<UtilityGridCell> cells = GetCells(
                    orderedCandidateAnchors[candidateIndex], footprint);
                bool compatible = true;
                for (int cellIndex = 0;
                    cellIndex < cells.Count;
                    cellIndex++)
                {
                    if (!availableCells.Contains(cells[cellIndex]))
                    {
                        compatible = false;
                        break;
                    }
                }

                if (compatible)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
