using System;
using System.Linq;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    [Serializable]
    public class Position
    {
        public bool blocked = false;
        public readonly short x;
        public readonly short y;

        public short normalizedX;

        public readonly short gridSize;
        public readonly CubeFace face;
        public readonly Position[] neighbours = new Position[8];

        public readonly Neighbour? missing;

        public Position(short normalizedX, short y, CubeFace face, short gridSize)
        {
            this.gridSize = gridSize;
            this.x = (short)(normalizedX + gridSize * (int)face);
            this.y = y;
            this.face = face;
            this.normalizedX = normalizedX;

            missing = normalizedX == 0 && y == 0 ? Neighbour.LowerLeft
                      : normalizedX == gridSize - 1 && y == 0 ? Neighbour.LowerRight
                      : normalizedX == 0 && y == gridSize - 1 ? Neighbour.UpperLeft
                      : normalizedX == gridSize - 1 && y == gridSize - 1 ? Neighbour.UpperRight
                      : (Neighbour?)null;
        }

        public Position(short x, short y, short gridSize)
        {
            this.gridSize = gridSize;
            this.x = x;
            this.y = y;
            this.face = (CubeFace)(x / gridSize);
            this.normalizedX = (short)(x % gridSize);

            missing = normalizedX == 0 && y == 0 ? Neighbour.LowerLeft
                      : normalizedX == gridSize - 1 && y == 0 ? Neighbour.LowerRight
                      : normalizedX == 0 && y == gridSize - 1 ? Neighbour.UpperLeft
                      : normalizedX == gridSize - 1 && y == gridSize - 1 ? Neighbour.UpperRight
                      : (Neighbour?)null;
        }

        public Position(int xAndyCombined, short gridSize)
            : this(
                (short)(xAndyCombined >> 16),
                (short)(xAndyCombined & 0xFFFF),
                gridSize
            )
        { }

        public Position(int xAndyCombined, short gridSize, CubeFace face)
            : this(
                (short)((xAndyCombined >> 16) % gridSize),
                (short)(xAndyCombined & 0xFFFF),
                face,
                gridSize
            )
        { }

        public Position GetNeighbour(Neighbour neighbour)
        {
            return neighbours[(int)neighbour];
        }

        public void SetNeighbour(Position n, Neighbour dir)
        {
            if (!missing.HasValue || missing.Value != dir)
                neighbours[(int)dir] = n;
            else
                throw new ArgumentException(string.Format("Neighbour {0} is not allowed for position {1}", dir, this));
        }

        public bool HasNeighbour(Neighbour neighbour)
        {
            return neighbours[(int)neighbour] != null;
        }

        public Position this[Neighbour neighbour]
        {
            get
            {
                return GetNeighbour(neighbour);
            }
            set
            {
                neighbours[(int)neighbour] = value;
            }
        }

        public static int Combine(short x, short y)
        {
            return (x << 16) + y;
        }

        public static void Decombine(int xAndyCombined, out short x, out short y)
        {
            x = (short)(xAndyCombined >> 16);
            y = (short)(xAndyCombined & 0xFFFF);
        }
        public int Combined { get { return Combine(x, y); } }
        public bool IsEdgeField { get { return normalizedX == 0 || normalizedX == gridSize - 1 || y == 0 || y == gridSize - 1; } }
        public bool IsCornerField { get { return (normalizedX == 0 || normalizedX == gridSize - 1) && (y == 0 || y == gridSize - 1); } }

        public override int GetHashCode()
        {
            return Combined;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return ((Position)obj).GetHashCode() == this.GetHashCode();
        }

        public override string ToString()
        {
            return "(" +face.ToString().First()+" "+ normalizedX + ", " + y + ")";
        }
    }

    public enum Neighbour
    {
        /// (0, 1)
        Up,

        /// (1, 1)
        UpperRight,

        /// (1, 0)
        Right,

        /// (1, -1)
        LowerRight,

        /// (0, -1)
        Down,

        /// (-1, -1)
        LowerLeft,

        /// (-1, 0)
        Left,

        /// (-1, 1)
        UpperLeft
    }
}