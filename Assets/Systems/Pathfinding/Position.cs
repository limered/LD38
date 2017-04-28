using System;
using System.Linq;

[Serializable]
public class Position
{
    public bool blocked = false;
    public readonly short x;
    public readonly short y;
    public readonly CubeFace face;

    public Position[] neighbours;
    public void AddNeighbour(Position n)
    {
        if (neighbours == null) neighbours = new[] { n };
        else if (!neighbours.Contains(n))
        {
            var l = neighbours.ToList();
            l.Add(n);
            neighbours = l.ToArray();
        }
    }

    public Position(short x, short y, CubeFace face)
    {
        this.x = x;
        this.y = y;
        this.face = face;
    }

    public Position(int xAndyCombined, CubeFace face)
    {
        this.x = (short)(xAndyCombined >> 16);
        this.y = (short)(xAndyCombined & 0xFFFF);
        this.face = face;
    }

    public int Combined { get { return (x << 16) + y; } }

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
        return "(" + x + ", " + y + ")";
    }
}