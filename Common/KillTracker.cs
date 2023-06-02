namespace Common;

public class KillTracker
{
    public const int EnemyKillsToComplete = 20;

    public int EnemiesKilled { get; private set; }
    public bool Complete { get; private set; }

    // Returns true if a new boss should be spawned.
    public bool EnemyDied()
    {
        if (Complete) return false;

        ++EnemiesKilled;

        if (EnemiesKilled < EnemyKillsToComplete) return false;

        Complete = true;
        return true;
    }

    public void Reset(int enemiesKilled)
    {
        Complete = false;
        EnemiesKilled = enemiesKilled;
    }
}