using Common;
using Microsoft.Xna.Framework;

namespace GraveZone;

public class BossStatus
{
    private const int X = Ui.CenterX;
    private const int Y = TextRenderer.BackgroundOffset;

    private Enemy? _currentBoss;

    public void Draw(GraveZone game, Map map)
    {
        if (_currentBoss is not null && _currentBoss.Health > 0)
            DrawBossHealth(game);
        else if (_currentBoss is not null && map.KillTracker.Complete)
            DrawTransitionTimer(game);
        else
            DrawEnemiesKilled(game, map);
    }

    private void DrawBossHealth(GraveZone game)
    {
        if (_currentBoss is null) return;

        TextRenderer.Draw($"boss health: {_currentBoss.Health}/{_currentBoss.Stats.MaxHealth}", X, Y, game, Color.White,
            centered: true, uiAnchor: UiAnchor.Top);
    }

    private void DrawTransitionTimer(GraveZone game)
    {
        TextRenderer.Draw($"get ready...", X, Y,
            game, Color.White, centered: true, uiAnchor: UiAnchor.Top);
    }

    private void DrawEnemiesKilled(GraveZone game, Map map)
    {
        TextRenderer.Draw($"{map.KillTracker.EnemiesKilled}/{KillTracker.EnemyKillsToComplete} kills", X, Y,
            game, Color.White, centered: true, uiAnchor: UiAnchor.Top);
    }

    public void EnemySpawned(Enemy? enemy)
    {
        if (enemy is null || !enemy.Stats.IsBoss) return;

        _currentBoss = enemy;
    }
}