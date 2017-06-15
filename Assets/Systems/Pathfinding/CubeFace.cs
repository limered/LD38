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
        public static Vector3 Up(this CubeFace cubeFace)
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

        public static Vector3 Down(this CubeFace face)
        {
            return -face.Up();
        }

        public static Vector3 Forward(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return Vector3.forward;
                case CubeFace.Down: return Vector3.back;
                case CubeFace.Right: return Vector3.down;
                case CubeFace.Left: return Vector3.up;
                case CubeFace.Forward: return Vector3.left;
                case CubeFace.Back: return Vector3.right;

                default: throw new NotImplementedException("wtf òÓ");
            }
        }

        public static Vector3 Back(this CubeFace face)
        {
            return -face.Forward();
        }

        public static Vector3 Right(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return Vector3.right;
                case CubeFace.Down: return Vector3.left;
                case CubeFace.Right: return Vector3.back;
                case CubeFace.Left: return Vector3.forward;
                case CubeFace.Forward: return Vector3.down;
                case CubeFace.Back: return Vector3.up;

                default: throw new NotImplementedException("wtf òÓ");
            }
        }

        public static Vector3 Left(this CubeFace face)
        {
            return -face.Right();
        }

        public static float SumComponents(this Vector3 v)
        {
            return v.x + v.y + v.z;
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

    

        public static CubeFace XOverflow(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Right;
                case CubeFace.Right: return CubeFace.Back;
                case CubeFace.Forward: return CubeFace.Down;
                case CubeFace.Down: return CubeFace.Left;
                case CubeFace.Left: return CubeFace.Forward;
                case CubeFace.Back: return CubeFace.Up;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace XBelowZero(this CubeFace face)
        {
            return face.XOverflow().Opposite();
        }

        public static CubeFace YOverflow(this CubeFace face)
        {
            switch (face)
            {
                case CubeFace.Up: return CubeFace.Forward;
                case CubeFace.Right: return CubeFace.Down;
                case CubeFace.Forward: return CubeFace.Left;
                case CubeFace.Down: return CubeFace.Back;
                case CubeFace.Left: return CubeFace.Up;
                case CubeFace.Back: return CubeFace.Right;
            }

            throw new Exception("wtf oO");
        }

        public static CubeFace YBelowZero(this CubeFace face)
        {
            return face.YOverflow().Opposite();
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

        public static CubeFace Add(this CubeFace face, OutOfBounds oob)
        {
            var result = face;
            if((oob & OutOfBounds.X_Below_0) != OutOfBounds.Nope) result = result.XBelowZero();
            if((oob & OutOfBounds.X_Over_Max) != OutOfBounds.Nope) result = result.XOverflow();
            if((oob & OutOfBounds.Y_Below_0) != OutOfBounds.Nope) result = result.YBelowZero();
            if((oob & OutOfBounds.Y_Over_Max) != OutOfBounds.Nope) result = result.YOverflow();

            return result;
        }
    }
}