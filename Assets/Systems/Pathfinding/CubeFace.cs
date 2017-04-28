using System;
using UnityEngine;

public enum CubeFace
{
    Up,
    Right,
    Forward,
    Down,
    Left,
    Back
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
}