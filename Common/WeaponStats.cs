using System.Collections.ObjectModel;

namespace Common;

// TODO: Consider adding a stat to slow player & enemy move speed when attacking with each weapon.
public class WeaponStats
{
    public static readonly Dictionary<WeaponType, WeaponStats> Registry = new()
    {
        { WeaponType.None, null }
    };

    static WeaponStats()
    {
        Register(new WeaponStats(WeaponType.Dagger, 0.2f, Sprite.Dagger, new[]
        {
            new ProjectileSpawnData
            {
                ProjectileType = ProjectileType.ThrownDagger, Angle = -10f, RelativeToForward = true
            },
            new ProjectileSpawnData
            {
                ProjectileType = ProjectileType.ThrownDagger, Angle = 10f, RelativeToForward = true
            }
        }));
    }

    public readonly Sprite Sprite;
    public readonly float AttackCooldown;
    public readonly ReadOnlyCollection<ProjectileSpawnData> ProjectileSpawns;
    public readonly WeaponType WeaponType;

    private WeaponStats(WeaponType weaponType, float attackCooldown, Sprite sprite,
        ProjectileSpawnData[] projectileSpawns)
    {
        WeaponType = weaponType;
        AttackCooldown = attackCooldown;
        Sprite = sprite;
        ProjectileSpawns = new ReadOnlyCollection<ProjectileSpawnData>(projectileSpawns);
    }

    private static void Register(WeaponStats weaponStats)
    {
        Registry.Add(weaponStats.WeaponType, weaponStats);
    }
}