using System;
using UnityEngine;

namespace Assets.Systems.Pathfinding
{
    public enum CubeFace
    {
        Up = 0,
        Right = 1,
        Forward = 2,
        Down = 3,
        Left = 4,
        Back = 5
    }

    public static class CubeFaceExtensions
    {
        public static Vector3 ToUnitVector(this CubeFace cubeFace)
        {
            switch (cubeFace)
            {
                case CubeFace.Up: return Vector3.up;
                case CubeFace.Down: return Vector3.down;
                case CubeFace.Left: return Vector3.left;
                case CubeFace.Right: return Vector3.right;
                case CubeFace.Forward: return Vector3.forward;
                case CubeFace.Back: return Vector3.back;

                default: throw new NotImplementedException("wtf òÓ");
            }
        }

        public static Vector3 ToUpperLeftUnitVector(this CubeFace cubeFace)
        {
            var one = new Vector3(1, 1, 1);
            switch (cubeFace)
            {
                case CubeFace.Up: return (cubeFace.ToUnitVector() - one).normalized;
                case CubeFace.Down: return -CubeFace.Up.ToUpperLeftUnitVector();
                case CubeFace.Right: return (cubeFace.ToUnitVector() - one).normalized;
                case CubeFace.Left: return -CubeFace.Right.ToUpperLeftUnitVector();
                case CubeFace.Forward: return (cubeFace.ToUnitVector() - one).normalized;
                case CubeFace.Back: return -CubeFace.Forward.ToUpperLeftUnitVector();

                default: throw new NotImplementedException("wtf òÓ");
            }
        }

        public static CubeFace ToCubeFace(this Vector3 vector)
        {
            var v = vector.normalized;
            var e = Mathf.Epsilon;

            return
                Mathf.Approximately(v.x, 0f) && v.y > e ? CubeFace.Up :
                Mathf.Approximately(v.x, 0f) && v.y < e ? CubeFace.Down :
                v.x < e && Mathf.Approximately(v.y, 0f) ? CubeFace.Left :
                v.x > e && Mathf.Approximately(v.y, 0f) ? CubeFace.Right :
                Mathf.Approximately(v.x, 0f) && Mathf.Approximately(v.y, 0f) && v.z > e ? CubeFace.Forward :
                CubeFace.Back;
        }

        // public static CubeFace[] NeighbourFaces(this CubeFace face)
        // {
        //     switch (face)
        //     {
        //         case CubeFace.Up: return new []{ CubeFace.Forward, CubeFace.Right, CubeFace.Back, CubeFace.Left };
        //         case CubeFace.Right: return new []{ CubeFace.Up, CubeFace.Forward, CubeFace.Down, CubeFace.Back };
        //         case CubeFace.Forward: return new []{ CubeFace.Up, CubeFace.Left, CubeFace.Down, CubeFace.Right };
        //         case CubeFace.Down: return new []{ CubeFace., CubeFace., CubeFace., CubeFace. };
        //         // case CubeFace.Left: return new []{ CubeFace., CubeFace., CubeFace., CubeFace. };
        //         // case CubeFace.Back: return new []{ CubeFace., CubeFace., CubeFace., CubeFace. };
        //     }

        //     throw new Exception("wtf oO");
        // }

        public static CubeFace XOverflow(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Right;
                case CubeFace.Right: return CubeFace.Up;
                case CubeFace.Forward: return CubeFace.Right;
                case CubeFace.Down: return CubeFace.Left;
                case CubeFace.Left: return CubeFace.Down;
                case CubeFace.Back: return CubeFace.Left;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace XBelowZero(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Left;
                case CubeFace.Right: return CubeFace.Down;
                case CubeFace.Forward: return CubeFace.Left;
                case CubeFace.Down: return CubeFace.Right;
                case CubeFace.Left: return CubeFace.Up;
                case CubeFace.Back: return CubeFace.Right;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace YOverflow(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Forward;
                case CubeFace.Right: return CubeFace.Forward;
                case CubeFace.Forward: return CubeFace.Up;
                case CubeFace.Down: return CubeFace.Back;
                case CubeFace.Left: return CubeFace.Back;
                case CubeFace.Back: return CubeFace.Down;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace YBelowZero(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Back;
                case CubeFace.Right: return CubeFace.Back;
                case CubeFace.Forward: return CubeFace.Down;
                case CubeFace.Down: return CubeFace.Forward;
                case CubeFace.Left: return CubeFace.Forward;
                case CubeFace.Back: return CubeFace.Up;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace Opposite(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Down;
                case CubeFace.Right: return CubeFace.Left;
                case CubeFace.Forward: return CubeFace.Back;
                case CubeFace.Down: return CubeFace.Up;
                case CubeFace.Left: return CubeFace.Right;
                case CubeFace.Back: return CubeFace.Forward;
            }

            throw new Exception("wtf oO");
        }

        public enum Direction
        {
            Up,
            Right,
            Down,
            Left
        }
    }
}