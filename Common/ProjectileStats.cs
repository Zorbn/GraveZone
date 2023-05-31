namespace Common;

public class ProjectileStats
{
    public static readonly Dictionary<ProjectileType, ProjectileStats> Registry = new();

    private const int LowDamageTier1 = 5;
    private const int MediumDamageTier1 = 10;
    private const int HighDamageTier1 = 15;
    private const int LowDamageTier2 = 10;
    private const int MediumDamageTier2 = 15;
    private const int HighDamageTier2 = 20;
    private const int LowDamageTier3 = 15;
    private const int MediumDamageTier3 = 20;
    private const int HighDamageTier3 = 25;

    private const float LowRange = 1.5f;
    private const float MediumRange = 3f;
    private const float HighRange = 7.5f;

    private const float LowSpeed = 6f;
    private const float MediumSpeed = 7f;
    private const float HighSpeed = 8f;

    static ProjectileStats()
    {
        Register(new ProjectileStats(ProjectileType.ThrownDagger, LowDamageTier1, MediumRange, MediumSpeed, Sprite.Dagger));
        Register(new ProjectileStats(ProjectileType.ThrownSword, HighDamageTier1, LowRange, MediumSpeed, Sprite.Sword));
        Register(new ProjectileStats(ProjectileType.ThrownLongsword, HighDamageTier2, LowRange, MediumSpeed, Sprite.Longsword));
        Register(new ProjectileStats(ProjectileType.ThrownWarHammer, HighDamageTier3, LowRange, MediumSpeed, Sprite.WarHammer));
        Register(new ProjectileStats(ProjectileType.Fireball, HighDamageTier1, HighRange, LowSpeed, Sprite.Fireball));
        Register(new ProjectileStats(ProjectileType.FireNova, HighDamageTier2, HighRange, LowSpeed, Sprite.FireNova));
        Register(new ProjectileStats(ProjectileType.ThrownSpear, MediumDamageTier1, MediumRange, MediumSpeed, Sprite.Spear));
        Register(new ProjectileStats(ProjectileType.ThrownDoubleSpear, MediumDamageTier2, MediumRange, MediumSpeed, Sprite.DoubleSpear));
        Register(new ProjectileStats(ProjectileType.ThrownQuadSpear, MediumDamageTier3, MediumRange, MediumSpeed, Sprite.QuadSpear));
        Register(new ProjectileStats(ProjectileType.ThrownKnife, HighDamageTier1, LowRange, MediumSpeed, Sprite.Knife));
        Register(new ProjectileStats(ProjectileType.ThrownMachete, HighDamageTier2, LowRange, MediumSpeed, Sprite.Machete));
        Register(new ProjectileStats(ProjectileType.ThrownScimitar, HighDamageTier3, LowRange, MediumSpeed, Sprite.Scimitar));
        Register(new ProjectileStats(ProjectileType.CrossbowBolt, MediumDamageTier1, MediumRange, HighSpeed, Sprite.CrossbowBolt));
        Register(new ProjectileStats(ProjectileType.Arrow, HighDamageTier1, HighRange, HighSpeed, Sprite.Arrow));
        Register(new ProjectileStats(ProjectileType.ScytheBlades, HighDamageTier1, MediumRange, MediumSpeed, Sprite.ScytheBlades));
        Register(new ProjectileStats(ProjectileType.DoubleScytheBlades, HighDamageTier2, MediumRange, MediumSpeed, Sprite.DoubleScytheBlades));
        Register(new ProjectileStats(ProjectileType.EnchantedScytheBlades, HighDamageTier3, MediumRange, MediumSpeed, Sprite.EnchantedScytheBlades));
        Register(new ProjectileStats(ProjectileType.Spores, LowDamageTier1, MediumRange, LowSpeed, Sprite.Spores));
        Register(new ProjectileStats(ProjectileType.AdvancedSpores, LowDamageTier2, MediumRange, LowSpeed, Sprite.AdvancedSpores));
        Register(new ProjectileStats(ProjectileType.EvolvedSpores, LowDamageTier3, MediumRange, LowSpeed, Sprite.EvolvedSpores));
        Register(new ProjectileStats(ProjectileType.SpikeBall, HighDamageTier1, LowRange, MediumSpeed, Sprite.SpikeBall));
    }

    public readonly Sprite Sprite;
    public readonly int Damage;
    public readonly float Range;
    public readonly float Speed;
    public readonly ProjectileType ProjectileType;

    private ProjectileStats(ProjectileType projectileType, int damage, float range, float speed, Sprite sprite)
    {
        ProjectileType = projectileType;
        Damage = damage;
        Range = range;
        Speed = speed;
        Sprite = sprite;
    }

    private static void Register(ProjectileStats projectileStats)
    {
        Registry.Add(projectileStats.ProjectileType, projectileStats);
    }
}