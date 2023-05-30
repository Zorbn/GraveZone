using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;

namespace Server;

public class ServerBossSpawner
{
    private static readonly Vector3 BossSpawnPosition = new Vector3(Map.Size, 0f, Map.Size) * 0.5f;

    public int EnemiesKilled => _killTracker.EnemiesKilled;

    private readonly KillTracker _killTracker = new();
    private Enemy? _currentBoss;

    public void EnemyDied(Server server, Map map, ref int nextEnemyId)
    {
        var shouldSpawnBoss = _killTracker.EnemyDied(_currentBoss);

        if (!shouldSpawnBoss) return;

        ServerSpawnBoss(server, map, ref nextEnemyId);
    }

    private void ServerSpawnBoss(Server server, Map map, ref int nextEnemyId)
    {
        var enemyId = nextEnemyId++;
        var enemy = map.SpawnRandomBossEnemy(BossSpawnPosition.X, BossSpawnPosition.Z, enemyId,
            new Attacker(Team.Enemies, server.EnemyAttackAction));

        if (enemy is null) return;

        _currentBoss = enemy;

        server.SendToAll(new EnemySpawn
        {
            Id = enemyId,
            EnemyType = enemy.Stats.EnemyType,
            X = enemy.Position.X,
            Z = enemy.Position.Z,
            Health = enemy.Health
        }, DeliveryMethod.ReliableOrdered);
    }
}