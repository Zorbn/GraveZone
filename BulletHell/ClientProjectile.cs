using Common;

namespace BulletHell;

public static class ClientProjectile
{
    public static void Draw(this Projectile projectile, SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(projectile.Position.X, projectile.Position.Z, projectile.Stats.Sprite);
    }
}