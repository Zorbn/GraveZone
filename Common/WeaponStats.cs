using System.Collections.ObjectModel;

namespace Common;

// TODO: Add speed stat (entity's speed multiplier while the weapon is equipped).
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
                { ProjectileType = ProjectileType.ThrownDagger, Angle = -10f, RelativeToForward = true },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownDagger, Angle = 10f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.Sword, 0.5f, Sprite.Sword, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 0f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 45f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 90f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 135f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 180f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 225f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 270f, RelativeToForward = false },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSword, Angle = 315f, RelativeToForward = false }
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