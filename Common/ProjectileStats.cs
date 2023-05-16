namespace Common;

public class ProjectileStats
{
    public static readonly Dictionary<ProjectileType, ProjectileStats> Registry = new();

    static ProjectileStats()
    {
        Register(new ProjectileStats(ProjectileType.ThrownDagger, 5, 3f, Sprite.Dagger));
        Register(new ProjectileStats(ProjectileType.ThrownSword, 10, 1.5f, Sprite.Sword));
        Register(new ProjectileStats(ProjectileType.Fireball, 25, 5f, Sprite.Fireball));
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