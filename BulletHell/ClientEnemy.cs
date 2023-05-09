using Common;

namespace BulletHell;

public static class ClientEnemy
{
    public static void Draw(this Enemy enemy, SpriteRenderer spriteRenderer, int animationFrame)
    {
        var spriteI = animationFrame % enemy.Stats.Sprites.Count;
        spriteRenderer.Add(enemy.SpritePosition.X, enemy.SpritePosition.Z, enemy.Stats.Sprites[spriteI]);
    }
}