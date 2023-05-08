using Common;
using Microsoft.Xna.Framework;

namespace BulletHell;

public static class ClientEnemy
{
    // TODO: Figure out how to smooth enemy motion on the client.
    public static void Draw(this Enemy enemy, SpriteRenderer spriteRenderer)
    {
        // TODO: Give enemies sprites so they don't look like projectiles.
        spriteRenderer.Add(enemy.Position.X, enemy.Position.Z, Sprite.Projectile);
    }
}