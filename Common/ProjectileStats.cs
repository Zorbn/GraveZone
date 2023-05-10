namespace Common;

public class ProjectileStats
{
    public static readonly Dictionary<ProjectileType, ProjectileStats> Registry = new();

    static ProjectileStats()
    {
        Register(new ProjectileStats(ProjectileType.ThrownDagger, Sprite.Dagger));
        Register(new ProjectileStats(ProjectileType.ThrownSword, Sprite.Sword));
    }

    public readonly Sprite Sprite;
    public readonly ProjectileType ProjectileType;

    private ProjectileStats(ProjectileType projectileType, Sprite sprite)
    {
        ProjectileType = projectileType;
        Sprite = sprite;
    }

    private static void Register(ProjectileStats projectileStats)
    {
        Registry.Add(projectileStats.ProjectileType, projectileStats);
    }
}