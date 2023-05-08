using Common;
using Microsoft.Xna.Framework;

namespace BulletHell;

public static class ClientDroppedWeapon
{
    public static void Draw(this DroppedWeapon droppedWeapon, SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(droppedWeapon.Position.X, droppedWeapon.Position.Z, droppedWeapon.Weapon.Sprite);
    }
}