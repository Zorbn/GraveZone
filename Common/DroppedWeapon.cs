using Microsoft.Xna.Framework;

namespace Common;

public class DroppedWeapon
{
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);
    
    public readonly Weapon Weapon;
    public readonly Vector3 Position;
    public readonly int Id;

    public DroppedWeapon(WeaponType weaponType, float x, float z, int id)
    {
        Weapon = Weapon.Registry[weaponType];
        Position = new Vector3(x, 0f, z);
        Id = id;
    }
}