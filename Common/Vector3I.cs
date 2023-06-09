﻿using Microsoft.Xna.Framework;

namespace Common;

public struct Vector3I : IEquatable<Vector3I>
{
    public int X;
    public int Y;
    public int Z;

    public Vector3I(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3I(Vector3 vector) : this((int)vector.X, (int)vector.Y, (int)vector.Z)
    {
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public static bool operator ==(Vector3I a, Vector3I b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Vector3I a, Vector3I b)
    {
        return !(a == b);
    }

    public bool Equals(Vector3I other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3I other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}