using System;
using System.Collections.Generic;

namespace Assets.Systems.Pathfinding
{
    public static class GridCalculations
    {
        private static readonly Dictionary<CubeFaceNeighbours, Func<short, short, int, int>> neighbourConverters = new Dictionary<CubeFaceNeighbours, Func<short, short, int, int>>(){
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Right), (x, y, s) => C(x, y, s)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Forward), (x, y, s) => C(x, y, s)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Left), (x, y, s) => C(0, y.I(s), s)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Back), (x, y, s) => C(x.I(s), y, s)},

            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Right), (x, y, s) => C(0, y.I(s), s)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Forward), (x, y, s) => C(x.I(s), y, s)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Left), (x, y, s) => C(x, y, s)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Back), (x, y, s) => C(x, y, s)},
            
            {new CubeFaceNeighbours(CubeFace.Forward, CubeFace.Left), (x, y, s) => C(y.I(s), x, s)},
            {new CubeFaceNeighbours(CubeFace.Left, CubeFace.Forward), (x, y, s) => C(y.I(s), x, s)},
        };

        ///invert
        private static int I(this short xOrY, int s)
        {
            return (s - 1) - xOrY;
        }
        private static int C(int x, int y, int s)
        {
            return C((short)x, (short)y, s);
        }

        ///combines x and y to a position by applying s
        private static int C(short x, short y, int s)
        {
            return Position.Combine((short)(x * s), y);
        }

        ///returns position as combined-int
        public static int CalcNeighbourField(Position pos, CubeFace neighbourFace, int gridSize)
        {
            var n = new CubeFaceNeighbours(pos.face, neighbourFace);
            // var reversed = n.IsReversed;
            return neighbourConverters[n](
                pos.y, pos.x,
                // reversed ? pos.x : pos.y,
                gridSize);
        }

        private struct CubeFaceNeighbours
        {
            public readonly CubeFace faceA;
            public readonly CubeFace faceB;

            public bool IsReversed { get { return GetHashCode() != ((int)faceA << 16) + (int)faceB; } }

            public CubeFaceNeighbours(CubeFace a, CubeFace b)
            {
                faceA = a;
                faceB = b;
            }

            public bool Has(CubeFace face)
            {
                return face == faceA || face == faceB;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return obj.GetHashCode() == this.GetHashCode();
            }

            public override int GetHashCode()
            {
                var a = (int)faceA;
                var b = (int)faceB;
                return Math.Min(a, b) << 16 + Math.Max(a, b);
            }
        }
    }
}