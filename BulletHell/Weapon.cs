using System.Collections.Generic;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class Weapon
{
    public static readonly Weapon Dagger = new(0.2f,
        new Rectangle(1, 28 * Resources.TileSize + 4, Resources.TileSize, Resources.TileSize));
    
    public readonly Rectangle SourceRectangle;
    
    private readonly float _attackCooldown;
    private float _attackTimer;

    public Weapon(float attackCooldown, Rectangle sourceRectangle)
    {
        _attackCooldown = attackCooldown;
        SourceRectangle = sourceRectangle;
    }

    public void Update(float deltaTime)
    {
        _attackTimer -= deltaTime;
    }

    public void Attack(Vector3 direction, float x, float z, List<Projectile> projectiles, Client client)
    {
        if (_attackTimer > 0f) return;

        _attackTimer = _attackCooldown;
        projectiles.Add(new Projectile(direction, x, z));
        
        var netDirectionToMouse = new NetVector3 { X = direction.X, Y = direction.Y, Z = direction.Z } ;
        client.SendToServer(new PlayerAttack { Direction = netDirectionToMouse, X = x, Z = z }, DeliveryMethod.ReliableUnordered);
    }
}