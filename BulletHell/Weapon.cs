using System.Collections.Generic;
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

    public void Attack(Vector3 direction, float x, float z, List<Projectile> projectiles)
    {
        if (_attackTimer > 0f) return;

        _attackTimer = _attackCooldown;
        projectiles.Add(new Projectile(direction, x, z));
    }
}