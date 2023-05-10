using Microsoft.Xna.Framework;

namespace Common;

public class Attacker
{
    private float _attackTimer;
    private readonly Team _team;
    private readonly Action<WeaponStats, Vector3, float, float, Map> _action;
    
    public Attacker(Team team, Action<WeaponStats, Vector3, float, float, Map> action)
    {
        _team = team;
        _action = action;
    }

    public void Update(float deltaTime)
    {
        _attackTimer -= deltaTime;
    }

    public void Attack(WeaponStats weaponStats, Vector3 direction, float x, float z, Map map)
    {
        if (_attackTimer > 0f) return;

        _attackTimer = weaponStats.AttackCooldown;
        map.AddAttackProjectiles(weaponStats.WeaponType, _team, direction, x, z);

        _action?.Invoke(weaponStats, direction, x, z, map);
    }
}