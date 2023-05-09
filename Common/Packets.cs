using LiteNetLib.Utils;

namespace Common;

public struct SetLocalId : INetSerializable
{
    public int Id { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
    }
}

public struct PlayerSpawn : INetSerializable
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}

public struct PlayerDespawn : INetSerializable
{
    public int Id { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
    }
}

public struct PlayerMove : INetSerializable
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}

// Sent from the client to the server when a player attacks.
public struct PlayerAttack : INetSerializable
{
    public NetVector3 Direction { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Direction);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Direction = reader.Get<NetVector3>();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}

// Sent from the server to the client when a projectile is spawned.
public struct ProjectileSpawn : INetSerializable
{
    public NetVector3 Direction { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Direction);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Direction = reader.Get<NetVector3>();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}

public struct MapGenerate : INetSerializable
{
    public int Seed { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Seed);
    }

    public void Deserialize(NetDataReader reader)
    {
        Seed = reader.GetInt();
    }
}

public struct DroppedWeaponSpawn : INetSerializable
{
    public WeaponType WeaponType { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public int Id { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((int)WeaponType);
        writer.Put(X);
        writer.Put(Z);
        writer.Put(Id);
    }

    public void Deserialize(NetDataReader reader)
    {
        WeaponType = (WeaponType)reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
        Id = reader.GetInt();
    }
}

public struct RequestPickupWeapon : INetSerializable
{
    public int DroppedWeaponId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(DroppedWeaponId);
    }

    public void Deserialize(NetDataReader reader)
    {
        DroppedWeaponId = reader.GetInt();
    }
}

public struct PickupWeapon : INetSerializable
{
    public int PlayerId { get; set; }
    public int DroppedWeaponId { get; set; }
    public WeaponType WeaponType { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerId);
        writer.Put(DroppedWeaponId);
        writer.Put((int)WeaponType);
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerId = reader.GetInt();
        DroppedWeaponId = reader.GetInt();
        WeaponType = (WeaponType)reader.GetInt();
    }
}

public struct RequestGrabSlot : INetSerializable
{
    public int SlotIndex { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(SlotIndex);
    }

    public void Deserialize(NetDataReader reader)
    {
        SlotIndex = reader.GetInt();
    }
}

public struct GrabSlot : INetSerializable
{
    public int PlayerId { get; set; }
    public int SlotIndex { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerId);
        writer.Put(SlotIndex);
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerId = reader.GetInt();
        SlotIndex = reader.GetInt();
    }
}

public struct RequestGrabEquippedSlot : INetSerializable
{
    public void Serialize(NetDataWriter writer)
    {
    }

    public void Deserialize(NetDataReader reader)
    {
    }
}

public struct GrabEquippedSlot : INetSerializable
{
    public int PlayerId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerId);
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerId = reader.GetInt();
    }
}

public struct RequestDropGrabbed : INetSerializable
{
    public void Serialize(NetDataWriter writer)
    {
    }

    public void Deserialize(NetDataReader reader)
    {
    }
}

public struct DropGrabbed : INetSerializable
{
    public float X { get; set; }
    public float Z { get; set; }
    public int DroppedWeaponId { get; set; }
    public int PlayerId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(X);
        writer.Put(Z);
        writer.Put(DroppedWeaponId);
        writer.Put(PlayerId);
    }

    public void Deserialize(NetDataReader reader)
    {
        X = reader.GetFloat();
        Z = reader.GetFloat();
        DroppedWeaponId = reader.GetInt();
        PlayerId = reader.GetInt();
    }
}

public class UpdateInventory
{
    public int PlayerId { get; set; }
    public int[] Weapons { get; set; }
    public int EquippedWeapon { get; set; }
    public int GrabbedWeapon { get; set; }
}

public struct EnemySpawn : INetSerializable
{
    public int Id { get; set; }
    public EnemyType EnemyType { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put((int)EnemyType);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        EnemyType = (EnemyType)reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}

public struct EnemyTakeDamage : INetSerializable
{
    public int Id { get; set; }
    public int Damage { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Damage);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        Damage = reader.GetInt();
    }
}

public struct EnemyMove : INetSerializable
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(X);
        writer.Put(Z);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
    }
}