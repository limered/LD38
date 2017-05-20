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

        public static Vector3 ToDirectionVector(this CubeFace face, float angle, bool degrees=true)
        {
            var forward = face.ToUpperLeftUnitVector().RemoveComponents(true, false).normalized;
            return Quaternion.AngleAxis(degrees ? angle * Mathf.Rad2Deg : angle, face.ToUnitVector()) * forward;
        }

        public static Vector3 RemoveComponents(this Vector3 v, bool removeX, bool removeY)
        {
            if(Mathf.Approximately(v.x, 0))
            {
                return new Vector3(0f, removeX ? 0f : v.y, removeY ? 0f : v.z);
            }
            if(Mathf.Approximately(v.y, 0))
            {
                return new Vector3(removeX ? 0f : v.x, 0, removeY ? 0f : v.z);
            }
            if(Mathf.Approximately(v.z, 0))
            {
                return new Vector3(removeX ? 0f : v.x, removeY ? 0f : v.y, 0f);
            }

            throw new Exception("wtf oO");
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

        public static Vector3 MapToFacePlane(this CubeFace face, float x, float y)
        {
            var fieldDirection = -face.ToUpperLeftUnitVector();

            if (Mathf.Approximately(fieldDirection.y, 0f))
                return new Vector3(x, 0f, y);
            if (Mathf.Approximately(fieldDirection.x, 0f))
                return new Vector3(0f, x, y);
            if (Mathf.Approximately(fieldDirection.z, 0f))
                return new Vector3(x, y, 0f);

            throw new Exception("should never happen :)");
        }

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