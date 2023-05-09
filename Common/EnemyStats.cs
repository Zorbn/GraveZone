using System.Collections.ObjectModel;

namespace Common;

public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> EnemyTypes;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.VampireMimic, new[] { Sprite.MimicClosed, Sprite.MimicVampire }));
        Register(new EnemyStats(EnemyType.ShelledMimic, new[] { Sprite.MimicClosed, Sprite.MimicShelled }));

        EnemyTypes = new ReadOnlyCollection<EnemyType>(Registry.Keys.ToList());
    }

    public readonly ReadOnlyCollection<Sprite> Sprites;
    public readonly EnemyType EnemyType;

    private EnemyStats(EnemyType enemyType, Sprite[] sprites)
    {
        EnemyType = enemyType;
        Sprites = new ReadOnlyCollection<Sprite>(sprites);
    }

    private static void Register(EnemyStats enemyStats)
    {
        Registry.Add(enemyStats.EnemyType, enemyStats);
    }
}