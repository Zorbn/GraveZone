using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class BossStatus
{
    private const int X = BulletHell.UiCenterX;
    private const int Y = TextRenderer.BackgroundOffset;

    private readonly KillTracker _killTracker = new();
    private Enemy? _currentBoss;

    public void SetEnemiesKilled(int enemiesKilled)
    {
        _killTracker.EnemiesKilled = enemiesKilled;
    }

    public void Draw(Resources resources, SpriteBatch spriteBatch)
    {
        if (_currentBoss is null || _currentBoss.Health <= 0)
        {
            DrawEnemiesKilled(resources, spriteBatch);
        }
        else
        {
            DrawBossHealth(resources, spriteBatch);
        }
    }

    private void DrawEnemiesKilled(Resources resources, SpriteBatch spriteBatch)
    {
        TextRenderer.Draw($"{_killTracker.EnemiesKilled}/{KillTracker.EnemyKillsToSpawnBoss} kills", X, Y, resources, spriteBatch,
            Color.White, centered: true);
    }

    private void DrawBossHealth(Resources resources, SpriteBatch spriteBatch)
    {
        if (_currentBoss is null) return;

        TextRenderer.Draw($"boss health: {_currentBoss.Health}/{_currentBoss.Stats.MaxHealth}", X, Y, resources, spriteBatch,
            Color.White, centered: true);
    }

    public void EnemySpawned(Enemy? enemy)
    {
        if (enemy is null || !enemy.Stats.IsBoss) return;

        _currentBoss = enemy;
    }

    public void EnemyDied(Enemy? enemy)
    {
        _killTracker.EnemyDied(_currentBoss);

        if (_currentBoss is not null && enemy == _currentBoss)
        {
            _currentBoss = null;
        }
    }
}