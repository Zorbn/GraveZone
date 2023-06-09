﻿using LiteNetLib.Utils;

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
    public int Health { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(X);
        writer.Put(Z);
        writer.Put(Health);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
        Health = reader.GetInt();
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
    public WeaponType WeaponType { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Direction);
        writer.Put(X);
        writer.Put(Z);
        writer.Put((int)WeaponType);
    }

    public void Deserialize(NetDataReader reader)
    {
        Direction = reader.Get<NetVector3>();
        X = reader.GetFloat();
        Z = reader.GetFloat();
        WeaponType = (WeaponType)reader.GetInt();
    }
}

// When the client sends this packet to the server, the server looks at the sender's Id
// rather than using the Id in the packet to prevent cheating. So for client -> server Id
// doesn't need to be specified. For server -> client, Id is used and should be specified. 
public struct PlayerTakeDamage : INetSerializable
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

// Same rules apply for Id as with PlayerTakeDamage.
public struct PlayerHeal : INetSerializable
{
    public int Id { get; set; }
    public int Amount { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Amount);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        Amount = reader.GetInt();
    }
}

public struct PlayerRespawn : INetSerializable
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

// Sent from the server to the client when a projectile is spawned.
public struct ProjectileSpawn : INetSerializable
{
    public NetVector3 Direction { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public WeaponType WeaponType { get; set; }
    public Team Team { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Direction);
        writer.Put(X);
        writer.Put(Z);
        writer.Put((int)WeaponType);
        writer.Put((int)Team);
    }

    public void Deserialize(NetDataReader reader)
    {
        Direction = reader.Get<NetVector3>();
        X = reader.GetFloat();
        Z = reader.GetFloat();
        WeaponType = (WeaponType)reader.GetInt();
        Team = (Team)reader.GetInt();
    }
}

public struct MapGenerate : INetSerializable
{
    public int Seed { get; set; }
    public int EnemiesKilled { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Seed);
        writer.Put(EnemiesKilled);
    }

    public void Deserialize(NetDataReader reader)
    {
        Seed = reader.GetInt();
        EnemiesKilled = reader.GetInt();
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

public struct DespawnWeapon : INetSerializable
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
    public NetVector3 PlayerForward;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerForward);
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerForward = reader.Get<NetVector3>();
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

public struct PlayerDropInventory : INetSerializable
{
    public int PlayerId { get; set; }
    public int BaseDroppedWeaponId { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PlayerId);
        writer.Put(BaseDroppedWeaponId);
    }

    public void Deserialize(NetDataReader reader)
    {
        PlayerId = reader.GetInt();
        BaseDroppedWeaponId = reader.GetInt();
    }
}

public class UpdateInventory
{
    public int PlayerId { get; set; }
    public int[]? Weapons { get; set; }
    public int EquippedWeapon { get; set; }
    public int GrabbedWeapon { get; set; }
}

public struct EnemySpawn : INetSerializable
{
    public int Id { get; set; }
    public EnemyType EnemyType { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public int Health { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put((int)EnemyType);
        writer.Put(X);
        writer.Put(Z);
        writer.Put(Health);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        EnemyType = (EnemyType)reader.GetInt();
        X = reader.GetFloat();
        Z = reader.GetFloat();
        Health = reader.GetInt();
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