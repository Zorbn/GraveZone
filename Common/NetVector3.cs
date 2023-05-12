using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace Common;

public struct NetVector3 : INetSerializable
{
    public float X;
    public float Y;
    public float Z;

    public NetVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public NetVector3(float x, float z) : this(x, 0f, z)
    {
    }

    public NetVector3(Vector3 vector) : this(vector.X, vector.Y, vector.Z)
    {
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(X);
        writer.Put(Y);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        X = reader.GetFloat();
        Y = reader.GetFloat();
        Z = reader.GetFloat();
    }
}