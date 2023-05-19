using System.Collections.ObjectModel;

namespace Common;

public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> NormalEnemyTypes;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.VampireMimic, WeaponType.Dagger, 0.2f, 20, 1.5f,
            new[] { Sprite.MimicClosed, Sprite.MimicVampire }));
        Register(new EnemyStats(EnemyType.ShelledMimic, WeaponType.Sword, 0.2f, 30, 1f,
            new[] { Sprite.MimicClosed, Sprite.MimicShelled }));
        Register(new EnemyStats(EnemyType.Solider, WeaponType.Spear, 0.2f, 30, 1f,
            new[] { Sprite.SoliderStepLeft, Sprite.SoliderIdle, Sprite.SoliderStepRight, Sprite.SoliderIdle }));
        Register(new EnemyStats(EnemyType.Dragon, WeaponType.FireWand, 0.1f, 50, 0.5f,
            new[] { Sprite.DragonStepLeft, Sprite.DragonIdle, Sprite.DragonStepRight, Sprite.DragonIdle }));
        Register(new EnemyStats(EnemyType.Ninja, WeaponType.Knife, 0.1f, 10, 2f,
            new[] { Sprite.NinjaStepLeft, Sprite.NinjaIdle, Sprite.NinjaStepRight, Sprite.NinjaIdle }));
        Register(new EnemyStats(EnemyType.ElderDragon, WeaponType.FireCharm, 0.1f, 200, 0f,
            new[] { Sprite.ElderDragon }, true));

        var normalEnemyTypes = new List<EnemyType>();
        foreach (var (enemyType, enemyStats) in Registry)
        {
            if (enemyStats.IsBoss) continue;

            normalEnemyTypes.Add(enemyType);
        }

        NormalEnemyTypes = new ReadOnlyCollection<EnemyType>(normalEnemyTypes);
    }

    public readonly EnemyType EnemyType;
    public readonly WeaponType WeaponType;
    public readonly float WeaponDropRate;
    public readonly int MaxHealth;
    public readonly float Speed;
    public readonly ReadOnlyCollection<Sprite> Sprites;
    public readonly bool IsBoss;

    private EnemyStats(EnemyType enemyType, WeaponType weaponType, float weaponDropRate, int maxHealth,
        float speed, Sprite[] sprites, bool isBoss = false)
    {
        EnemyType = enemyType;
        WeaponType = weaponType;
        WeaponDropRate = weaponDropRate;
        MaxHealth = maxHealth;
        Speed = speed;
        Sprites = new ReadOnlyCollection<Sprite>(sprites);
        IsBoss = isBoss;
    }

    private static void Register(EnemyStats enemyStats)
    {
        Registry.Add(enemyStats.EnemyType, enemyStats);
    }
}