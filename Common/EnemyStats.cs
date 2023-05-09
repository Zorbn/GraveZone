using System.Collections.ObjectModel;

namespace Common;

public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> EnemyTypes;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.VampireMimic, 20, new[] { Sprite.MimicClosed, Sprite.MimicVampire }));
        Register(new EnemyStats(EnemyType.ShelledMimic, 30, new[] { Sprite.MimicClosed, Sprite.MimicShelled }));

        EnemyTypes = new ReadOnlyCollection<EnemyType>(Registry.Keys.ToList());
    }

    public readonly ReadOnlyCollection<Sprite> Sprites;
    public readonly int MaxHealth;
    public readonly EnemyType EnemyType;

    private EnemyStats(EnemyType enemyType, int maxHealth, Sprite[] sprites)
    {
        EnemyType = enemyType;
        MaxHealth = maxHealth;
        Sprites = new ReadOnlyCollection<Sprite>(sprites);
        
    }

    private static void Register(EnemyStats enemyStats)
    {
        Registry.Add(enemyStats.EnemyType, enemyStats);
    }
}