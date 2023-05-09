namespace Common;

public class WeaponStats
{
    public static readonly Dictionary<WeaponType, WeaponStats> Registry = new()
    {
        { WeaponType.None, null }
    };

    static WeaponStats()
    {
        Register(new WeaponStats(WeaponType.Dagger, 0.2f, Sprite.Dagger));
    }

    public readonly Sprite Sprite;
    public readonly float AttackCooldown;
    public readonly WeaponType WeaponType;

    private WeaponStats(WeaponType weaponType, float attackCooldown, Sprite sprite)
    {
        WeaponType = weaponType;
        AttackCooldown = attackCooldown;
        Sprite = sprite;
    }

    private static void Register(WeaponStats weaponStats)
    {
        Registry.Add(weaponStats.WeaponType, weaponStats);
    }
}