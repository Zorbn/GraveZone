using Common;

namespace BulletHell;

public static class ClientEnemy
{
    public static void AddSprite(this Enemy enemy, SpriteRenderer spriteRenderer, int animationFrame)
    {
        var spriteI = animationFrame % enemy.Stats.Sprites.Count;
        var size = enemy.Stats.IsBoss ? SpriteSize.Large : SpriteSize.Medium;
        spriteRenderer.Add(enemy.SpritePosition, enemy.Stats.Sprites[spriteI], size);
    }
}