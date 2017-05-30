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
        public readonly OutOfBounds outOfBounds;
        public short normalizedX;
        public readonly short gridSize;
        public readonly CubeFace face;
        public readonly Position[] neighbours = new Position[8];

        public readonly Neighbour? missing;

        public Position(short normalizedX, short y, CubeFace face, OutOfBounds outOfBounds, short gridSize)
        {
            this.gridSize = gridSize;
            this.normalizedX = (short)(normalizedX % gridSize);
            this.x = (short)(this.normalizedX + gridSize * (int)face);
            this.y = y;
            this.outOfBounds = outOfBounds;
            this.face = face;

            missing = normalizedX == 0 && y == 0 ? Neighbour.LowerLeft
                      : normalizedX == gridSize - 1 && y == 0 ? Neighbour.LowerRight
                      : normalizedX == 0 && y == gridSize - 1 ? Neighbour.UpperLeft
                      : normalizedX == gridSize - 1 && y == gridSize - 1 ? Neighbour.UpperRight
                      : (Neighbour?)null;
        }

        public Position(short x, short y, OutOfBounds outOfBounds, short gridSize)
        {
            this.gridSize = gridSize;
            this.x = x;
            this.y = y;
            this.outOfBounds = outOfBounds;
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
                OutOfBounds.Nope,
                gridSize
            )
        { }

        public Position(int xAndyCombined, short gridSize, CubeFace face)
            : this(
                (short)((xAndyCombined >> 16) % gridSize),
                (short)(xAndyCombined & 0xFFFF),
                face,
                OutOfBounds.Nope,
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
            return "(" + face.ToString().First() + " " + normalizedX + ", " + y + ")";
        }

        public SimplePosition Simple
        {
            get
            {
                return new SimplePosition(normalizedX, y, outOfBounds, face);
            }
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

    [Serializable]
    public struct SimplePosition
    {
        public short x;
        public short y;
        public OutOfBounds outOfBounds;
        public CubeFace face;

        public SimplePosition(short normalizedX, short y, OutOfBounds oob, CubeFace face)
        {
            this.x = normalizedX;
            this.y = y;
            this.outOfBounds = oob;
            this.face = face;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || (obj.GetType() != this.GetType() && obj.GetType() != typeof(Position)))
            {
                return false;
            }

            var simpleOther = obj as SimplePosition?;
            if (simpleOther.HasValue)
            {
                return simpleOther.Value.x == x
                        && simpleOther.Value.y == y
                        && simpleOther.Value.face == face;
            }

            var posOther = obj as Position;
            if (posOther != null)
            {
                return posOther.Combined == Combine(posOther.gridSize);
            }

            return false;
        }

        public int Combine(int gridSize)
        {
            return Position.Combine((short)(x + gridSize * (int)face), y);
        }

        public override string ToString()
        {
            return "(x="+x+" y="+y+" face="+face+" out-of-bounds="+outOfBounds+")";
        }
    }

    [Flags]
    public enum OutOfBounds
    {
        Nope = 0,
        X_Below_0 = 1,
        X_Over_Max = 2,
        Y_Below_0 = 4,
        Y_Over_Max = 8
    }
}