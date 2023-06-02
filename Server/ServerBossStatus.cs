using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;

namespace Server;

public class ServerBossStatus
{
    private const float TransitionTime = 10f;
    private static readonly Vector3 BossSpawnPosition = new Vector3(Map.Size, 0f, Map.Size) * 0.5f;

    private bool _inTransition;
    private float _transitionTimer;

    public void EnemyDied(Server server, Map map, Enemy enemy, bool shouldSpawnBoss, ref int nextEnemyId)
    {
        if (map.KillTracker.Complete)
        {
            if (enemy.Stats.IsBoss)
            {
                BeginTransition();
            }
        }

        if (!shouldSpawnBoss) return;

        ServerSpawnBoss(server, map, ref nextEnemyId);
    }

    public void Update(Server server, float deltaTime)
    {
        if (!_inTransition) return;

        _transitionTimer += deltaTime;

        if (_transitionTimer < TransitionTime) return;

        EndTransition(server);
    }

    private void BeginTransition()
    {
        _inTransition = true;
        _transitionTimer = 0;
    }

    private void EndTransition(Server server)
    {
        _inTransition = false;
        server.ServerGenerateMap();
    }

    private void ServerSpawnBoss(Server server, Map map, ref int nextEnemyId)
    {
        var enemyId = nextEnemyId++;
        var enemy = map.SpawnRandomBossEnemy(BossSpawnPosition.X, BossSpawnPosition.Z, enemyId,
            new Attacker(Team.Enemies, server.EnemyAttackAction));

        if (enemy is null) return;

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