using System.Collections.Generic;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class Weapon
{
    private readonly float _attackCooldown;
    private float _attackTimer;
    
    public Weapon(float attackCooldown)
    {
        _attackCooldown = attackCooldown;
    }

    public void Update(float deltaTime)
    {
        _attackTimer -= deltaTime;
    }

    public void Attack(Vector3 direction, float x, float z, List<Projectile> projectiles,
        NetPacketProcessor netPacketProcessor, NetDataWriter writer, NetManager client)
    {
        if (_attackTimer > 0f) return;

        _attackTimer = _attackCooldown;
        projectiles.Add(new Projectile(direction, x, z));
        
        var netDirectionToMouse = new NetVector3 { X = direction.X, Y = direction.Y, Z = direction.Z } ;
        writer.Reset();
        netPacketProcessor.Write(writer, new PlayerAttack { Direction = netDirectionToMouse, X = x, Z = z });
        client.FirstPeer.Send(writer, DeliveryMethod.ReliableUnordered);
    }
}