namespace Common;

public class Weapon
{
    public static readonly Dictionary<WeaponType, Weapon> Registry = new()
    {
        { WeaponType.None, null }
    };

    static Weapon()
    {
        Register(new Weapon(WeaponType.Dagger, 0.2f, Sprite.Dagger));
    }

    public readonly Sprite Sprite;
    public readonly float AttackCooldown;
    public readonly WeaponType WeaponType;

    private Weapon(WeaponType weaponType, float attackCooldown, Sprite sprite)
    {
        WeaponType = weaponType;
        AttackCooldown = attackCooldown;
        Sprite = sprite;
    }

    private static void Register(Weapon weapon)
    {
        Registry.Add(weapon.WeaponType, weapon);
    }
}