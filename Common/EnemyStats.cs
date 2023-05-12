using System.Collections.ObjectModel;

namespace Common;

// TODO: Add speed stat.
public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> EnemyTypes;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.VampireMimic, WeaponType.Dagger, 0.2f, 20,
            new[] { Sprite.MimicClosed, Sprite.MimicVampire }));
        Register(new EnemyStats(EnemyType.ShelledMimic, WeaponType.Sword, 0.2f, 30,
            new[] { Sprite.MimicClosed, Sprite.MimicShelled }));

        EnemyTypes = new ReadOnlyCollection<EnemyType>(Registry.Keys.ToList());
    }

    public readonly EnemyType EnemyType;
    public readonly WeaponType WeaponType;
    public readonly float WeaponDropRate;
    public readonly int MaxHealth;
    public readonly ReadOnlyCollection<Sprite> Sprites;

    private EnemyStats(EnemyType enemyType, WeaponType weaponType, float weaponDropRate, int maxHealth,
        Sprite[] sprites)
    {
        EnemyType = enemyType;
        WeaponType = weaponType;
        WeaponDropRate = weaponDropRate;
        MaxHealth = maxHealth;
        Sprites = new ReadOnlyCollection<Sprite>(sprites);
    }

    private static void Register(EnemyStats enemyStats)
    {
        Registry.Add(enemyStats.EnemyType, enemyStats);
    }
}