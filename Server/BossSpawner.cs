using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;

namespace Server;

public class BossSpawner
{
    private const int EnemyKillsToSpawnBoss = 20;
    private static readonly Vector3 BossSpawnPosition = new Vector3(Map.Size, 0f, Map.Size) * 0.5f;

    private int _enemiesKilled;
    private Enemy? _currentBoss;

    public void EnemyDied(Server server, Map map, ref int nextEnemyId)
    {
        // Don't progress towards spawning a new boss if the last one is still alive.
        if (_currentBoss is not null && _currentBoss.Health > 0) return;

        ++_enemiesKilled;

        if (_enemiesKilled < EnemyKillsToSpawnBoss) return;

        _enemiesKilled = 0;
        ServerSpawnBoss(server, map, ref nextEnemyId);
    }

    private void ServerSpawnBoss(Server server, Map map, ref int nextEnemyId)
    {
        var enemyId = nextEnemyId++;
        var enemy = map.SpawnEnemy(EnemyType.HauntedBonfire, BossSpawnPosition.X, BossSpawnPosition.Z, enemyId,
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