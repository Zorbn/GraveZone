using System.Collections.ObjectModel;

namespace Common;

public class EnemyStats
{
    public static readonly Dictionary<EnemyType, EnemyStats> Registry = new();
    public static readonly ReadOnlyCollection<EnemyType> NormalEnemyTypes;

    private const float MediumDropRate = 0.2f;

    private const float MediumSpeed = 1.2f;

    private const int LowMaxHealth = 20;
    private const int MediumMaxHealth = 40;
    private const int HighMaxHealth = 60;

    private const int BossMediumMaxHealth = 200;

    static EnemyStats()
    {
        Register(new EnemyStats(EnemyType.Zombie, WeaponType.Sword, MediumDropRate, MediumMaxHealth, MediumSpeed,
            new[] { Sprite.ZombieIdle, Sprite.ZombieStepLeft, Sprite.ZombieIdle, Sprite.ZombieStepRight }));

        Register(new EnemyStats(EnemyType.ChestMimicVampire, WeaponType.Dagger, MediumDropRate, MediumMaxHealth,
            MediumSpeed,
            new[] { Sprite.ChestMimicClosed, Sprite.ChestMimicVampire }));
        Register(new EnemyStats(EnemyType.ChestMimicShelled, WeaponType.Mace, MediumDropRate, HighMaxHealth,
            MediumSpeed,
            new[] { Sprite.ChestMimicClosed, Sprite.ChestMimicShelled }));

        Register(new EnemyStats(EnemyType.Ghost, WeaponType.Knife, MediumDropRate, LowMaxHealth, MediumSpeed,
            new[] { Sprite.GhostIdle, Sprite.GhostStepLeft, Sprite.GhostIdle, Sprite.GhostStepRight }));

        Register(new EnemyStats(EnemyType.Skeleton, WeaponType.Spear, MediumDropRate, MediumMaxHealth, MediumSpeed,
            new[] { Sprite.SkeletonIdle, Sprite.SkeletonStepLeft, Sprite.SkeletonIdle, Sprite.SkeletonStepRight }));

        Register(new EnemyStats(EnemyType.HauntedLantern, WeaponType.FireWand, MediumDropRate, LowMaxHealth,
            MediumSpeed,
            new[] { Sprite.HauntedLantern, Sprite.HauntedLanternHop }));

        Register(new EnemyStats(EnemyType.GraveMimic, WeaponType.Scythe, MediumDropRate, HighMaxHealth, 0,
            new[] { Sprite.GraveMimicClosed, Sprite.GraveMimicOpen }));

        Register(new EnemyStats(EnemyType.GrayMushroom, WeaponType.SporeBlaster, MediumDropRate, HighMaxHealth, 0,
            new[]
            {
                Sprite.GrayMushroomLookForward, Sprite.GrayMushroomLookDown, Sprite.GrayMushroomLookForward,
                Sprite.GrayMushroomLookUp
            }));
        Register(new EnemyStats(EnemyType.RedMushroom, WeaponType.SporeBlaster, MediumDropRate, HighMaxHealth, 0,
            new[]
            {
                Sprite.RedMushroomLookForward, Sprite.RedMushroomLookDown, Sprite.RedMushroomLookForward,
                Sprite.RedMushroomLookUp
            }));

        Register(new EnemyStats(EnemyType.GhostArcher, WeaponType.Crossbow, MediumDropRate, MediumMaxHealth,
            MediumSpeed,
            new[]
            {
                Sprite.GhostArcherIdle, Sprite.GhostArcherStepLeft, Sprite.GhostArcherIdle, Sprite.GhostArcherStepRight
            }));
        Register(new EnemyStats(EnemyType.SkeletonArcher, WeaponType.Shortbow, MediumDropRate, MediumMaxHealth,
            MediumSpeed,
            new[]
            {
                Sprite.SkeletonArcherIdle, Sprite.SkeletonArcherStepLeft, Sprite.SkeletonArcherIdle,
                Sprite.SkeletonArcherStepRight
            }));

        // TODO: Make more bosses that favor different weapon types, the bonfire is easiest with long range.
        Register(new EnemyStats(EnemyType.HauntedBonfire, WeaponType.FireCharm, MediumDropRate, BossMediumMaxHealth, 0,
            new[] { Sprite.HauntedBonfire1, Sprite.HauntedBonfire2, Sprite.HauntedBonfire3 }, true));

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