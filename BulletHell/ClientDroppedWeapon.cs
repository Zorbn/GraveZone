using Common;
using Microsoft.Xna.Framework;

namespace BulletHell;

public static class ClientDroppedWeapon
{
    public static void Draw(this Weapon weapon, SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(weapon.Position.X, weapon.Position.Z, weapon.Stats.Sprite);
    }
}