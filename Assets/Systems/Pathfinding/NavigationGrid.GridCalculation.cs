using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public static class GridCalculations
    {
        private static readonly Dictionary<CubeFaceNeighbours, Func<short, short, int, CubeFace, int>> neighbourConverters = new Dictionary<CubeFaceNeighbours, Func<short, short, int, CubeFace, int>>(){
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Right, true), (x, y, s, f) => C(x, y, s, f)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Forward, true), (x, y, s, f) => C(x, y, s, f)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Left, true), (x, y, s, f) => C(0, y.I(s), s, f)},
            {new CubeFaceNeighbours(CubeFace.Up, CubeFace.Back, true), (x, y, s, f) => C(x.I(s), y, s, f)},

            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Right, true), (x, y, s, f) => C(0, y.I(s), s, f)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Forward, true), (x, y, s, f) => C(x.I(s), y, s, f)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Left, true), (x, y, s, f) => C(x, y, s, f)},
            {new CubeFaceNeighbours(CubeFace.Down, CubeFace.Back, true), (x, y, s, f) => C(x, y, s, f)},

            {new CubeFaceNeighbours(CubeFace.Forward, CubeFace.Right, true), (x, y, s, f) => C(y, x, s, f)},
            {new CubeFaceNeighbours(CubeFace.Forward, CubeFace.Left, false), (x, y, s, f) => C(y.I(s), 0, s, f)},
            {new CubeFaceNeighbours(CubeFace.Left, CubeFace.Forward, false), (x, y, s, f) => C(0, x.I(s), s, f)},

            {new CubeFaceNeighbours(CubeFace.Back, CubeFace.Left, true), (x, y, s, f) => C(y, x, s, f)},
            {new CubeFaceNeighbours(CubeFace.Back, CubeFace.Right, false), (x, y, s, f) => C(y.I(s), 0, s, f)},
            {new CubeFaceNeighbours(CubeFace.Right, CubeFace.Back, false), (x, y, s, f) => C(0, x.I(s), s, f)},
        };

        ///invert
        private static int I(this short xOrY, int s)
        {
            return (s - 1) - (xOrY % s);
        }
        private static int C(int x, int y, int s, CubeFace f)
        {
            return C((short)x, (short)y, s, f);
        }

        ///combines x and y to a position by applying s
        private static int C(short x, short y, int s, CubeFace f)
        {
            return Position.Combine((short)((x % s) + (int)f * s), y);
        }

        ///returns position as combined-int
        public static int CalcNeighbourField(short normalizedX, short y, CubeFace face, CubeFace neighbourFace, int gridSize)
        {

            try
            {
                var n = new CubeFaceNeighbours(face, neighbourFace, null);
                var fun = neighbourConverters.ContainsKey(n) ? neighbourConverters[n] : neighbourConverters[new CubeFaceNeighbours(neighbourFace, face, null)];
                var neighbourField = fun(
                    normalizedX, y,
                    gridSize,
                    neighbourFace);

                Position.Decombine(neighbourField, out normalizedX, out y);
                // Debug.Log("CalcNeighbourField " + pos + " <-> " + new Position(x, y, neighbourFace, (short)gridSize));

                return neighbourField;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return 0;
            }
        }

        private struct CubeFaceNeighbours
        {
            public readonly CubeFace faceA;
            public readonly CubeFace faceB;

            public readonly bool? bidirectional;

            public bool IsReversed { get { return GetHashCode() != ((int)faceA << 16) + (int)faceB; } }

            public CubeFaceNeighbours(CubeFace a, CubeFace b, bool? bidirectional)
            {
                // Debug.Log(a + (bidirectional.HasValue && bidirectional.Value ? "<->" : "-->") + b);
                faceA = a;
                faceB = b;
                this.bidirectional = bidirectional;
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

                var other = (CubeFaceNeighbours)obj;

                if (
                    (!bidirectional.HasValue || bidirectional.HasValue && bidirectional.Value)
                    && (!other.bidirectional.HasValue || other.bidirectional.HasValue && other.bidirectional.Value)
                    && other.Has(faceA) && other.Has(faceB)
                )
                {
                    return true;
                }

                if (obj.GetHashCode() != this.GetHashCode()) Debug.Log("(" + this + ") != (" + other + ")");
                return obj.GetHashCode() == this.GetHashCode();
            }

            public override int GetHashCode()
            {
                var a = (int)faceA;
                var b = (int)faceB;
                if (bidirectional.HasValue && bidirectional.Value)
                    return ((Math.Min(a, b) + 1) << 16) + Math.Max(a, b);
                return ((a + 1) << 16) + b;
            }

            public override string ToString()
            {
                return faceA + "<->" + faceB;
            }
        }


        private static Neighbour[] allNeighbourDirections; 
        public static Neighbour[] AllNeighbourDirections {get{
            if(allNeighbourDirections == null) {
                allNeighbourDirections = new Neighbour[]{
                    Neighbour.Up,
                    Neighbour.UpperRight,
                    Neighbour.Right,
                    Neighbour.LowerRight,
                    Neighbour.Down,
                    Neighbour.LowerLeft,
                    Neighbour.Left,
                    Neighbour.UpperLeft
                };
            }

            return allNeighbourDirections;
        }}
    }
}