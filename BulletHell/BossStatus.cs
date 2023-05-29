using Common;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class BossStatus
{
    private const int X = Ui.CenterX;
    private const int Y = TextRenderer.BackgroundOffset;

    private readonly KillTracker _killTracker = new();
    private Enemy? _currentBoss;

    public void SetEnemiesKilled(int enemiesKilled)
    {
        _killTracker.EnemiesKilled = enemiesKilled;
    }

    public void Draw(BulletHell game)
    {
        if (_currentBoss is null || _currentBoss.Health <= 0)
            DrawEnemiesKilled(game);
        else
            DrawBossHealth(game);
    }

    private void DrawEnemiesKilled(BulletHell game)
    {
        TextRenderer.Draw($"{_killTracker.EnemiesKilled}/{KillTracker.EnemyKillsToSpawnBoss} kills", X, Y,
            game, Color.White, centered: true, uiAnchor: UiAnchor.Top);
    }

    private void DrawBossHealth(BulletHell game)
    {
        if (_currentBoss is null) return;

        TextRenderer.Draw($"boss health: {_currentBoss.Health}/{_currentBoss.Stats.MaxHealth}", X, Y, game, Color.White,
            centered: true, uiAnchor: UiAnchor.Top);
    }

    public void EnemySpawned(Enemy? enemy)
    {
        if (enemy is null || !enemy.Stats.IsBoss) return;

        _currentBoss = enemy;
    }

    public void EnemyDied(Enemy? enemy)
    {
        _killTracker.EnemyDied(_currentBoss);

        if (_currentBoss is not null && enemy == _currentBoss) _currentBoss = null;
    }
}