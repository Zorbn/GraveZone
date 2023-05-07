using Microsoft.Xna.Framework;

namespace Common;

public class Weapon
{
    public static readonly Dictionary<WeaponType, Weapon> Registry = new()
    {
        { WeaponType.None, null }
    };

    static Weapon()
    {
        Register(new Weapon(WeaponType.Dagger, 0.2f,
            new Rectangle(1, 31 * Resources.TileSize, Resources.TileSize, Resources.TileSize)));
    }

    public readonly Rectangle SourceRectangle;
    public readonly float AttackCooldown;
    public readonly WeaponType WeaponType;

    private Weapon(WeaponType weaponType, float attackCooldown, Rectangle sourceRectangle)
    {
        WeaponType = weaponType;
        AttackCooldown = attackCooldown;
        SourceRectangle = sourceRectangle;
    }

    private static void Register(Weapon weapon)
    {
        Registry.Add(weapon.WeaponType, weapon);
    }
}