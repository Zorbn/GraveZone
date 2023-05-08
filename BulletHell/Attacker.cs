using System.Collections.Generic;
using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class Attacker
{
    private float _attackTimer;

    public void Update(float deltaTime)
    {
        _attackTimer -= deltaTime;
    }

    public void Attack(Weapon weapon, Vector3 direction, float x, float z, List<Projectile> projectiles, Client client)
    {
        if (_attackTimer > 0f) return;

        _attackTimer = weapon.AttackCooldown;
        projectiles.Add(new Projectile(direction, x, z));
        
        var netDirectionToMouse = new NetVector3(direction) ;
        client.SendToServer(new PlayerAttack { Direction = netDirectionToMouse, X = x, Z = z }, DeliveryMethod.ReliableOrdered);
    }
}