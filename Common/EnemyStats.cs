using System.Collections.ObjectModel;

namespace Common;

public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> EnemyTypes;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.VampireMimic, WeaponType.Dagger, 0.2f, 20, 1.5f,
            new[] { Sprite.MimicClosed, Sprite.MimicVampire }));
        Register(new EnemyStats(EnemyType.ShelledMimic, WeaponType.Sword, 0.2f, 30, 1f,
            new[] { Sprite.MimicClosed, Sprite.MimicShelled }));
        Register(new EnemyStats(EnemyType.Solider, WeaponType.Spear, 0.2f, 30, 1f,
            new[] { Sprite.SoliderStepLeft, Sprite.SoliderIdle, Sprite.SoliderStepRight, Sprite.SoliderIdle }));
        Register(new EnemyStats(EnemyType.Dragon, WeaponType.FireWand, 0.02f, 50, 0.5f,
            new[] { Sprite.DragonStepLeft, Sprite.DragonIdle, Sprite.DragonStepRight, Sprite.DragonIdle }));

        EnemyTypes = new ReadOnlyCollection<EnemyType>(Registry.Keys.ToList());
    }

    public readonly EnemyType EnemyType;
    public readonly WeaponType WeaponType;
    public readonly float WeaponDropRate;
    public readonly int MaxHealth;
    public readonly float Speed;
    public readonly ReadOnlyCollection<Sprite> Sprites;

    private EnemyStats(EnemyType enemyType, WeaponType weaponType, float weaponDropRate, int maxHealth,
        float speed, Sprite[] sprites)
    {
        EnemyType = enemyType;
        WeaponType = weaponType;
        WeaponDropRate = weaponDropRate;
        MaxHealth = maxHealth;
        Speed = speed;
        Sprites = new ReadOnlyCollection<Sprite>(sprites);
    }

    private static void Register(EnemyStats enemyStats)
    {
        Registry.Add(enemyStats.EnemyType, enemyStats);
    }
}