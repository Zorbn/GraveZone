using Common;

namespace BulletHell;

public static class ClientEnemy
{
    public static void AddSprite(this Enemy enemy, SpriteRenderer spriteRenderer, int animationFrame)
    {
        var spriteI = animationFrame % enemy.Stats.Sprites.Count;
        spriteRenderer.Add(enemy.SpritePosition, enemy.Stats.Sprites[spriteI]);
    }
}