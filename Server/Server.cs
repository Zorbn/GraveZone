﻿using System.Diagnostics;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace Server;

public class Server
{
    private const float TickTime = 0.05f;
    private const int MaxSpawnedEnemies = 20;
    private const int TicksPerRepath = 2;

    public readonly Action<WeaponStats, Vector3, float, float, Map> EnemyAttackAction;

    private readonly NetPacketProcessor _netPacketProcessor;
    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _manager;
    private readonly NetDataWriter _writer = new();

    private readonly Dictionary<int, Player> _players = new();
    private readonly Random _random = new();
    private int _nextDroppedWeaponId;
    private int _nextEnemyId;
    private readonly Map _map = new();
    private readonly AStar _aStar = new();
    private readonly ServerBossStatus _serverBossStatus = new();

    private bool _isRunning;
    private int _tickCount;

    public Server()
    {
        _netPacketProcessor = new NetPacketProcessor();
        _netPacketProcessor.RegisterNestedType<NetVector3>();
        _netPacketProcessor.SubscribeNetSerializable<PlayerMove, NetPeer>(OnPlayerMove);
        _netPacketProcessor.SubscribeNetSerializable<PlayerAttack, NetPeer>(OnPlayerAttack);
        _netPacketProcessor.SubscribeNetSerializable<PlayerTakeDamage, NetPeer>(OnPlayerTakeDamage);
        _netPacketProcessor.SubscribeNetSerializable<PlayerHeal, NetPeer>(OnPlayerHeal);
        _netPacketProcessor.SubscribeNetSerializable<RequestPickupWeapon, NetPeer>(OnRequestPickupWeapon);
        _netPacketProcessor.SubscribeNetSerializable<RequestGrabSlot, NetPeer>(OnRequestGrabSlot);
        _netPacketProcessor.SubscribeNetSerializable<RequestGrabEquippedSlot, NetPeer>(OnRequestGrabEquippedSlot);
        _netPacketProcessor.SubscribeNetSerializable<RequestDropGrabbed, NetPeer>(OnRequestDropGrabbed);
        _manager = new NetManager(_listener)
        {
            AutoRecycle = true
        };

        EnemyAttackAction = (weaponStats, direction, attackX, attackZ, _) =>
        {
            var netDirection = new NetVector3(direction);
            SendToAll(new ProjectileSpawn
            {
                Direction = netDirection,
                X = attackX,
                Z = attackZ,
                WeaponType = weaponStats.WeaponType,
                Team = Team.Enemies
            }, DeliveryMethod.ReliableOrdered);
        };
    }

    public void Run()
    {
        Console.WriteLine("Starting server...");

        _isRunning = true;

        ServerGenerateMap();

        _manager.Start(Networking.Port);

        _listener.ConnectionRequestEvent += request =>
        {
            // TODO: Use AcceptIfKey/Connection keys to make servers password protected.
            request.Accept();

            // Poll again to make sure PeerConnected is handled before the next server tick,
            // otherwise the new client may receive duplicate data, one copy sent after an update
            // during the next tick, and the next copy send during PeerConnected.
            // This could cause the new client to be told to spawn a new enemy twice, for example.
            _manager.PollEvents();
        };

        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Connection from IP: {peer.EndPoint} with ID: {peer.Id}");
            HandlePeerConnection(peer);
        };

        _listener.PeerDisconnectedEvent += (peer, _) =>
        {
            Console.WriteLine($"Peer disconnected with ID: {peer.Id}");
            HandlePeerDisconnection(peer);
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, _, _) =>
        {
            try
            {
                _netPacketProcessor.ReadAllPackets(dataReader, fromPeer);
            }
            catch (ParseException)
            {
                Console.WriteLine($"Received unknown packet from peer: {fromPeer.Id}");
            }
        };

        var stopwatch = new Stopwatch();

        while (_isRunning)
        {
            stopwatch.Start();

            _manager.PollEvents();

            Tick();

            var deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Reset();

            var timeToNextTick = TimeSpan.FromSeconds(MathF.Max(TickTime - deltaTime, 0f));
            Thread.Sleep(timeToNextTick);
        }

        _manager.Stop();
    }

    public void Exit()
    {
        _isRunning = false;
    }

    private void Tick()
    {
        // Spawn a new enemy each tick that we aren't at the maximum.
        TrySpawnEnemy();

        _map.Update(TickTime);

        foreach (var enemyHit in _map.LastUpdateResults.EnemyHits) ServerDamageEnemy(enemyHit.Entity, enemyHit.Damage);

        foreach (var weaponToDespawn in _map.LastUpdateResults.WeaponsToDespawn) ServerDespawnWeapon(weaponToDespawn);

        ServerMapUpdate();
        _serverBossStatus.Update(this, TickTime);

        ++_tickCount;
    }

    // Alternative to Map.Update for server-only actions.
    private void ServerMapUpdate()
    {
        var shouldRepath = _tickCount % TicksPerRepath == 0;

        foreach (var (enemyId, enemy) in _map.Enemies)
        {
            if (shouldRepath)
            {
                Player? nearestPlayer = null;
                var nearestPlayerDistance = float.PositiveInfinity;
                foreach (var (_, player) in _players)
                {
                    var distance = (player.Position - enemy.Position).Length();

                    if (distance >= nearestPlayerDistance) continue;

                    nearestPlayerDistance = distance;
                    nearestPlayer = player;
                }

                enemy.CalculatePath(_aStar, _map, nearestPlayer);
            }

            var enemyMoved = enemy.UpdateServer(_map, TickTime);

            if (!enemyMoved) continue;

            SendToAll(new EnemyMove
            {
                Id = enemyId,
                X = enemy.Position.X,
                Z = enemy.Position.Z
            }, DeliveryMethod.Unreliable);
        }
    }

    private void HandlePeerConnection(NetPeer peer)
    {
        var playerSpawnPosition = _map.GetSpawnPosition() ?? Vector3.Zero;
        var newPlayerId = peer.Id;
        var newPlayer = new Player(_map, newPlayerId, playerSpawnPosition.X, playerSpawnPosition.Z);
        newPlayer.Inventory.AddWeapon(WeaponType.Dagger);

        // Tell the new player their id.
        SendToPeer(peer, new SetLocalId { Id = newPlayerId }, DeliveryMethod.ReliableOrdered);

        SendMapStateToPeer(peer);

        // Notify new player of old players.
        foreach (var (playerId, player) in _players)
        {
            SendToPeer(peer, new PlayerSpawn
            {
                Id = playerId,
                X = player.Position.X,
                Z = player.Position.Z,
                Health = player.Health
            }, DeliveryMethod.ReliableOrdered);
            SendRefToPeer(peer, CreateUpdateInventoryPacket(playerId, player), DeliveryMethod.ReliableOrdered);
        }

        // Notify all players of new player.
        _players[newPlayerId] = newPlayer;
        SendToAll(new PlayerSpawn
        {
            Id = newPlayerId,
            X = newPlayer.Position.X,
            Z = newPlayer.Position.Z,
            Health = newPlayer.Health
        }, DeliveryMethod.ReliableOrdered);
        SendRefToAll(CreateUpdateInventoryPacket(newPlayerId, newPlayer), DeliveryMethod.ReliableOrdered);
    }

    private void HandlePeerDisconnection(NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.Despawn(_map);
        _players.Remove(peer.Id);
        SendToAll(new PlayerDespawn { Id = peer.Id }, DeliveryMethod.ReliableOrdered);
    }

    public void ServerGenerateMap()
    {
        var nextSeed = _random.Next();
        _map.Generate(nextSeed, 0);

        SendToAll(new MapGenerate
        {
            Seed = _map.LastSeed,
            EnemiesKilled = _map.KillTracker.EnemiesKilled
        }, DeliveryMethod.ReliableOrdered);

        foreach (var (playerId, player) in _players)
        {
            var spawnPosition = _map.GetSpawnPosition() ?? Vector3.Zero;
            player.Respawn(_map, spawnPosition);

            SendToAll(new PlayerRespawn
            {
                Id = playerId,
                X = spawnPosition.X,
                Z = spawnPosition.Z
            }, DeliveryMethod.ReliableOrdered);
        }
    }

    private void SendMapStateToPeer(NetPeer peer)
    {
        // Tell the new player to generate the map.
        SendToPeer(peer, new MapGenerate
        {
            Seed = _map.LastSeed,
            EnemiesKilled = _map.KillTracker.EnemiesKilled
        }, DeliveryMethod.ReliableOrdered);

        // Populate the map for the new player.
        foreach (var (droppedWeaponId, droppedWeapon) in _map.DroppedWeapons)
            SendToPeer(peer, new DroppedWeaponSpawn
            {
                WeaponType = droppedWeapon.Stats.WeaponType,
                X = droppedWeapon.Position.X,
                Z = droppedWeapon.Position.Z,
                Id = droppedWeaponId
            }, DeliveryMethod.ReliableOrdered);

        foreach (var (enemyId, enemy) in _map.Enemies)
            SendToPeer(peer, new EnemySpawn
            {
                Id = enemyId,
                EnemyType = enemy.Stats.EnemyType,
                X = enemy.Position.X,
                Z = enemy.Position.Z,
                Health = enemy.Health
            }, DeliveryMethod.ReliableOrdered);

        foreach (var projectile in _map.Projectiles)
            SendToPeer(peer, new ProjectileSpawn
            {
                Direction = new NetVector3(projectile.Direction),
                X = projectile.Position.X,
                Z = projectile.Position.Z,
                WeaponType = projectile.WeaponType,
                Team = projectile.Team
            }, DeliveryMethod.ReliableOrdered);
    }

    private void TrySpawnEnemy()
    {
        if (_map.Enemies.Count >= MaxSpawnedEnemies) return;

        var enemySpawnPosition = _map.GetSpawnPosition();

        if (enemySpawnPosition is null) return;

        ServerSpawnEnemy(enemySpawnPosition.Value);
    }

    private void ServerSpawnEnemy(Vector3 position)
    {
        var enemyId = _nextEnemyId++;
        var enemy = _map.SpawnRandomNormalEnemy(position.X, position.Z, enemyId,
            new Attacker(Team.Enemies, EnemyAttackAction));

        if (enemy is null) return;

        SendToAll(new EnemySpawn
        {
            Id = enemyId,
            EnemyType = enemy.Stats.EnemyType,
            X = enemy.Position.X,
            Z = enemy.Position.Z,
            Health = enemy.Health
        }, DeliveryMethod.ReliableOrdered);
    }

    private void ServerDamageEnemy(Enemy enemy, int damage)
    {
        var enemyDied = enemy.TakeDamage(damage);

        if (enemyDied)
        {
            if (_random.NextSingle() < enemy.Stats.WeaponDropRate)
                ServerDropWeapon(enemy.Stats.WeaponType, enemy.Position.X, enemy.Position.Z);

            var shouldSpawnBoss = _map.DespawnEnemy(enemy.Id);
            _serverBossStatus.EnemyDied(this, _map, enemy, shouldSpawnBoss, ref _nextEnemyId);
        }

        SendToAll(new EnemyTakeDamage { Id = enemy.Id, Damage = damage }, DeliveryMethod.ReliableOrdered);
    }

    private UpdateInventory CreateUpdateInventoryPacket(int playerId, Player player)
    {
        var updateInventory = new UpdateInventory
        {
            PlayerId = playerId,
            Weapons = new int[Inventory.SlotCount]
        };

        for (var i = 0; i < Inventory.SlotCount; i++)
            updateInventory.Weapons[i] = (int)player.Inventory.Weapons[i].WeaponType;

        updateInventory.EquippedWeapon = (int)player.Inventory.EquippedWeaponStats.WeaponType;
        updateInventory.GrabbedWeapon = (int)player.Inventory.GrabbedWeaponStats.WeaponType;

        return updateInventory;
    }

    private void ServerDropWeapon(WeaponType weaponType, float x, float z)
    {
        var droppedWeaponId = _nextDroppedWeaponId++;
        var successfullyDropped = _map.DropWeapon(weaponType, x, z, droppedWeaponId);

        if (!successfullyDropped) return;

        SendToAll(new DroppedWeaponSpawn
        {
            WeaponType = weaponType,
            X = x,
            Z = z,
            Id = droppedWeaponId
        }, DeliveryMethod.ReliableOrdered);
    }

    private void ServerPickupWeapon(int playerId, int droppedWeaponId, WeaponType weaponType)
    {
        _map.PickupWeapon(droppedWeaponId);

        SendToAll(new PickupWeapon
        {
            PlayerId = playerId,
            DroppedWeaponId = droppedWeaponId,
            WeaponType = weaponType
        }, DeliveryMethod.ReliableOrdered);
    }

    private void ServerDespawnWeapon(int droppedWeaponId)
    {
        _map.PickupWeapon(droppedWeaponId);

        SendToAll(new DespawnWeapon
        {
            DroppedWeaponId = droppedWeaponId
        }, DeliveryMethod.ReliableOrdered);
    }

    private void SendToPeer<T>(NetPeer peer, T packet, DeliveryMethod deliveryMethod)
        where T : INetSerializable
    {
        _writer.Reset();
        _netPacketProcessor.WriteNetSerializable(_writer, ref packet);
        peer.Send(_writer, deliveryMethod);
    }

    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod)
        where T : INetSerializable
    {
        _writer.Reset();
        _netPacketProcessor.WriteNetSerializable(_writer, ref packet);
        _manager.SendToAll(_writer, deliveryMethod);
    }

    private void SendToAll<T>(T packet, DeliveryMethod deliveryMethod, NetPeer excludePeer)
        where T : INetSerializable
    {
        _writer.Reset();
        _netPacketProcessor.WriteNetSerializable(_writer, ref packet);
        _manager.SendToAll(_writer, deliveryMethod, excludePeer);
    }

    private void SendRefToPeer<T>(NetPeer peer, T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        peer.Send(_writer, deliveryMethod);
    }

    private void SendRefToAll<T>(T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        _manager.SendToAll(_writer, deliveryMethod);
    }

    public void SendRefToAll<T>(T packet, DeliveryMethod deliveryMethod, NetPeer excludePeer)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        _manager.SendToAll(_writer, deliveryMethod, excludePeer);
    }

    private void OnPlayerMove(PlayerMove playerMove, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.MoveTo(_map, player.Position with { X = playerMove.X, Z = playerMove.Z });

        SendToAll(playerMove with { Id = peer.Id }, DeliveryMethod.Unreliable);
    }

    private void OnPlayerAttack(PlayerAttack playerAttack, NetPeer peer)
    {
        _map.AddAttackProjectiles(playerAttack.WeaponType, Team.Players, playerAttack.Direction.ToVector3(),
            playerAttack.X,
            playerAttack.Z);

        // Send the new projectile to all players except the player who created the projectile.
        // That player will have already spawned its own local copy.
        SendToAll(new ProjectileSpawn
            {
                Direction = playerAttack.Direction,
                X = playerAttack.X,
                Z = playerAttack.Z,
                WeaponType = playerAttack.WeaponType,
                Team = Team.Players
            },
            DeliveryMethod.ReliableOrdered, peer);
    }

    private void OnPlayerTakeDamage(PlayerTakeDamage playerTakeDamage, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        var playerDied = player.TakeDamage(playerTakeDamage.Damage);

        SendToAll(playerTakeDamage with { Id = peer.Id }, DeliveryMethod.ReliableOrdered, peer);

        if (!playerDied) return;

        ServerKillPlayer(peer.Id, player);
    }

    private void ServerKillPlayer(int playerId, Player player)
    {
        var baseDroppedWeaponId = _nextDroppedWeaponId;
        _nextDroppedWeaponId += Inventory.SlotCount;
        _map.DropInventory(player.Inventory, player.Position.X, player.Position.Z, baseDroppedWeaponId);
        SendToAll(new PlayerDropInventory
        {
            PlayerId = playerId,
            BaseDroppedWeaponId = baseDroppedWeaponId
        }, DeliveryMethod.ReliableOrdered);


        var spawnPosition = _map.GetSpawnPosition() ?? Vector3.Zero;
        player.Respawn(_map, spawnPosition);

        SendToAll(new PlayerRespawn
        {
            Id = playerId,
            X = spawnPosition.X,
            Z = spawnPosition.Z
        }, DeliveryMethod.ReliableOrdered);
    }

    private void OnPlayerHeal(PlayerHeal playerHeal, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.Heal(playerHeal.Amount);
        SendToAll(playerHeal with { Id = peer.Id }, DeliveryMethod.ReliableOrdered, peer);
    }

    private void OnRequestPickupWeapon(RequestPickupWeapon requestPickupWeapon, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;
        if (!_map.DroppedWeapons.TryGetValue(requestPickupWeapon.DroppedWeaponId, out var droppedWeapon)) return;

        if (player.Inventory.AddWeapon(droppedWeapon.Stats.WeaponType))
            ServerPickupWeapon(peer.Id, droppedWeapon.Id, droppedWeapon.Stats.WeaponType);
    }

    private void OnRequestGrabSlot(RequestGrabSlot requestGrabSlot, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.Inventory.GrabSlot(requestGrabSlot.SlotIndex);
        SendToAll(new GrabSlot
        {
            PlayerId = peer.Id,
            SlotIndex = requestGrabSlot.SlotIndex
        }, DeliveryMethod.ReliableOrdered);
    }

    private void OnRequestGrabEquippedSlot(RequestGrabEquippedSlot requestGrabEquippedSlot, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.Inventory.GrabEquippedSlot();
        SendToAll(new GrabEquippedSlot
        {
            PlayerId = peer.Id
        }, DeliveryMethod.ReliableOrdered);
    }

    private void OnRequestDropGrabbed(RequestDropGrabbed requestDropGrabbed, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        var droppedWeaponId = _nextDroppedWeaponId++;

        // The item should be dropped right behind the player so that it will appear slightly
        // in-front of the player sprite from the player's camera's perspective.
        var dropOffset = requestDropGrabbed.PlayerForward.ToVector3() * 0.01f;
        var dropPosition = player.Position - dropOffset;

        player.Inventory.DropGrabbed(_map, dropPosition.X, dropPosition.Z, droppedWeaponId);
        SendToAll(new DropGrabbed
        {
            X = dropPosition.X,
            Z = dropPosition.Z,
            DroppedWeaponId = droppedWeaponId,
            PlayerId = peer.Id
        }, DeliveryMethod.ReliableOrdered);
    }
}