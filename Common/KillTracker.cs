namespace Common;

public class KillTracker
{
    public const int EnemyKillsToSpawnBoss = 20;

    public int EnemiesKilled;

    // Returns true if a new boss should be spawned.
    public bool EnemyDied(Enemy? currentBoss)
    {
        // Don't progress towards spawning a new boss if the last one is still alive.
        if (currentBoss is not null && currentBoss.Health > 0) return false;

        ++EnemiesKilled;

        if (EnemiesKilled < EnemyKillsToSpawnBoss) return false;

        EnemiesKilled = 0;
        return true;
    }
}