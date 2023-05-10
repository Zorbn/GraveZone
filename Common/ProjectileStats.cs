namespace Common;

public class ProjectileStats
{
    public static readonly Dictionary<ProjectileType, ProjectileStats> Registry = new();

    static ProjectileStats()
    {
        Register(new ProjectileStats(ProjectileType.ThrownDagger, 5, Sprite.Dagger));
        Register(new ProjectileStats(ProjectileType.ThrownSword, 10, Sprite.Sword));
    }

    public readonly Sprite Sprite;
    public readonly int Damage;
    public readonly ProjectileType ProjectileType;

    private ProjectileStats(ProjectileType projectileType, int damage, Sprite sprite)
    {
        ProjectileType = projectileType;
        Damage = damage;
        Sprite = sprite;
    }

    private static void Register(ProjectileStats projectileStats)
    {
        Registry.Add(projectileStats.ProjectileType, projectileStats);
    }
}