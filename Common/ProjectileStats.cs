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

    static ProjectileStats()
    {
        Register(new ProjectileStats(ProjectileType.ThrownDagger, LowDamageTier1, MediumRange, Sprite.Dagger));
        Register(new ProjectileStats(ProjectileType.ThrownSword, HighDamageTier1, LowRange, Sprite.Sword));
        Register(new ProjectileStats(ProjectileType.Fireball, HighDamageTier1, HighRange, Sprite.Fireball));
        Register(new ProjectileStats(ProjectileType.FireNova, HighDamageTier2, HighRange, Sprite.FireNova));
        Register(new ProjectileStats(ProjectileType.ThrownSpear, MediumDamageTier1, MediumRange, Sprite.Spear));
        Register(new ProjectileStats(ProjectileType.ThrownKnife, HighDamageTier1, LowRange, Sprite.Knife));
        Register(new ProjectileStats(ProjectileType.ThrownMachete, HighDamageTier2, LowRange, Sprite.Machete));
        Register(new ProjectileStats(ProjectileType.CrossbowBolt, MediumDamageTier1, MediumRange, Sprite.CrossbowBolt));
        Register(new ProjectileStats(ProjectileType.Arrow, HighDamageTier1, HighRange, Sprite.Arrow));
        Register(new ProjectileStats(ProjectileType.ScytheBlades, HighDamageTier1, MediumRange, Sprite.ScytheBlades));
        Register(new ProjectileStats(ProjectileType.Spores, LowDamageTier1, MediumRange, Sprite.Spores));
        Register(new ProjectileStats(ProjectileType.SpikeBall, HighDamageTier1, LowRange, Sprite.SpikeBall));
    }

    public readonly Sprite Sprite;
    public readonly int Damage;
    public readonly float Range;
    public readonly ProjectileType ProjectileType;

    private ProjectileStats(ProjectileType projectileType, int damage, float range, Sprite sprite)
    {
        ProjectileType = projectileType;
        Damage = damage;
        Range = range;
        Sprite = sprite;
    }

    private static void Register(ProjectileStats projectileStats)
    {
        Registry.Add(projectileStats.ProjectileType, projectileStats);
    }
}