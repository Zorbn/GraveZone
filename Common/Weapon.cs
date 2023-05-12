using Microsoft.Xna.Framework;

namespace Common;

public class Weapon
{
    public const float DespawnTime = 30f;
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public readonly WeaponStats Stats;
    public readonly Vector3 Position;
    public readonly int Id;

    private float _despawnTimer;

    public Weapon(WeaponType weaponType, float x, float z, int id)
    {
        Stats = WeaponStats.Registry[weaponType];
        Position = new Vector3(x, 0f, z);
        Id = id;
    }

    public bool Update(float deltaTime)
    {
        _despawnTimer += deltaTime;

        return _despawnTimer > DespawnTime;
    }
}