using System.Collections.ObjectModel;

namespace Common;

public class WeaponStats
{
    public static readonly Dictionary<WeaponType, WeaponStats?> Registry = new()
    {
        { WeaponType.None, null }
    };

    static WeaponStats()
    {
        Register(new WeaponStats(WeaponType.Dagger, WeaponType.None, 0.2f, 1.5f, Sprite.Dagger, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownDagger, Angle = -10f, RelativeToForward = true },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownDagger, Angle = 10f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.Sword, WeaponType.None, 0.5f, 1f, Sprite.Sword, new[]
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

        Register(new WeaponStats(WeaponType.FireWand, WeaponType.FireStaff, 0.5f, 1.0f, Sprite.FireWand, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.Fireball, Angle = 0f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.FireStaff, WeaponType.FireCharm, 0.5f, 1.0f, Sprite.FireStaff, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.FireNova, Angle = 0f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.FireCharm, WeaponType.None, 0.25f, 1.0f, Sprite.FireCharm, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.FireNova, Angle = -10f, RelativeToForward = true },
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.FireNova, Angle = 10f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.Spear, WeaponType.Dagger, 0.5f, 1f, Sprite.Spear, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownSpear, Angle = 0f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.Knife, WeaponType.Machete, 0.5f, 1.5f, Sprite.Knife, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownKnife, Angle = 0f, RelativeToForward = true }
        }));

        Register(new WeaponStats(WeaponType.Machete, WeaponType.None, 0.5f, 2f, Sprite.Machete, new[]
        {
            new ProjectileSpawnData
                { ProjectileType = ProjectileType.ThrownMachete, Angle = 0f, RelativeToForward = true }
        }));
    }

    public readonly WeaponType WeaponType;
    public readonly WeaponType Evolution;
    public readonly float AttackCooldown;
    public readonly float SpeedMultiplier;
    public readonly Sprite Sprite;
    public readonly ReadOnlyCollection<ProjectileSpawnData> ProjectileSpawns;
    public readonly float AttackRange;

    private WeaponStats(WeaponType weaponType, WeaponType evolution, float attackCooldown, float speedMultiplier,
        Sprite sprite, ProjectileSpawnData[] projectileSpawns)
    {
        WeaponType = weaponType;
        Evolution = evolution;
        AttackCooldown = attackCooldown;
        SpeedMultiplier = speedMultiplier;
        Sprite = sprite;
        ProjectileSpawns = new ReadOnlyCollection<ProjectileSpawnData>(projectileSpawns);

        foreach (var projectileSpawn in ProjectileSpawns)
            AttackRange = Math.Max(AttackRange, ProjectileStats.Registry[projectileSpawn.ProjectileType].Range);
    }

    private static void Register(WeaponStats weaponStats)
    {
        Registry.Add(weaponStats.WeaponType, weaponStats);
    }
}