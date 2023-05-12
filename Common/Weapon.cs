using Microsoft.Xna.Framework;

namespace Common;

// TODO: The server should despawn dropped weapons after a certain amount of time.
public class Weapon
{
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public readonly WeaponStats Stats;
    public readonly Vector3 Position;
    public readonly int Id;

    public Weapon(WeaponType weaponType, float x, float z, int id)
    {
        Stats = WeaponStats.Registry[weaponType];
        Position = new Vector3(x, 0f, z);
        Id = id;
    }
}