using Microsoft.Xna.Framework;

namespace BulletHell;

public class DroppedWeapon
{
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);
    
    // TODO: Add id that will let player request DroppedWeapon with given id from server.
    public readonly Weapon Weapon;
    public readonly Vector3 Position;

    public DroppedWeapon(Weapon weapon, float x, float z)
    {
        Weapon = weapon;
        Position = new Vector3(x, 0f, z);
    }

    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(Position.X, Position.Z, Weapon.SourceRectangle.Location);
    }
}