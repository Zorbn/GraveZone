using Common;

namespace GraveZone;

public static class ClientDroppedWeapon
{
    public static void AddSprite(this Weapon weapon, SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(weapon.Position, weapon.Stats.Sprite);
    }
}