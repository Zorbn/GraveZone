using System.Diagnostics;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace Server;

public class Server
{
    private const float TickTime = 0.05f;
    private const int MaxSpawnedEnemies = 3;
    
    private NetPacketProcessor _netPacketProcessor;
    private EventBasedNetListener _listener;
    private NetManager _manager;
    private NetDataWriter _writer;

    private Dictionary<int, ServerPlayer> _players;
    private Random _random;
    private int _mapSeed;
    private int _nextDroppedWeaponId;
    private int _nextEnemyId;
    private Map _map;

    private float _tickTimer;
    
    public Server()
    {
        _netPacketProcessor = new NetPacketProcessor();
        _netPacketProcessor.RegisterNestedType<NetVector3>();
        _netPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        _netPacketProcessor.SubscribeReusable<PlayerAttack, NetPeer>(OnPlayerAttack);
        _netPacketProcessor.SubscribeReusable<RequestPickupWeapon, NetPeer>(OnRequestPickupWeapon);
        _netPacketProcessor.SubscribeReusable<RequestGrabSlot, NetPeer>(OnRequestGrabSlot);
        _netPacketProcessor.SubscribeReusable<RequestGrabEquippedSlot, NetPeer>(OnRequestGrabEquippedSlot);
        _netPacketProcessor.SubscribeReusable<RequestDropGrabbed, NetPeer>(OnRequestDropGrabbed);
        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        _writer = new NetDataWriter();

        _players = new Dictionary<int, ServerPlayer>();
        _map = new Map();
        _random = new Random();
    }
    
    public void Run()
    {
        Console.WriteLine("Starting server...");

        _mapSeed = _random.Next();
        _map.Generate(_mapSeed);
        
        _manager.Start(Networking.Port);

        _listener.ConnectionRequestEvent += request =>
        {
            // TODO: Use AcceptIfKey/Connection keys to make servers password protected.
            request.Accept();
        };
        
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Connection from IP: {peer.EndPoint} with ID: {peer.Id}");

            var playerSpawnPosition = _map.GetSpawnPosition() ?? Vector3.Zero;
            var newPlayer = new ServerPlayer { X = playerSpawnPosition.X, Z = playerSpawnPosition.Z };
            var newPlayerId = peer.Id;
            
            // Tell the new player their id.
            SendToPeer(peer, new SetLocalId { Id = newPlayerId }, DeliveryMethod.ReliableOrdered);
            
            // Tell the new player to generate the map.
            SendToPeer(peer, new MapGenerate { Seed = _mapSeed }, DeliveryMethod.ReliableOrdered);
            
            // Populate the map for the new player.
            foreach (var (droppedWeaponId, droppedWeapon) in _map.DroppedWeapons)
            {
                SendToPeer(peer, new DroppedWeaponSpawn
                {
                    WeaponType = droppedWeapon.Weapon.WeaponType,
                    X = droppedWeapon.Position.X,
                    Z = droppedWeapon.Position.Z,
                    Id = droppedWeaponId
                }, DeliveryMethod.ReliableOrdered);
            }
            
            foreach (var (enemyId, enemy) in _map.Enemies)
            {
                SendToPeer(peer, new EnemySpawn
                {
                    Id = enemyId,
                    X = enemy.Position.X,
                    Z = enemy.Position.Z
                }, DeliveryMethod.ReliableOrdered);
            }
            
            // Notify new player of old players.
            foreach (var (playerId, player) in _players)
            {
                SendToPeer(peer, new PlayerSpawn { X = player.X, Z = player.Z, Id = playerId }, DeliveryMethod.ReliableOrdered);
                SendToPeer(peer, CreateUpdateInventoryPacket(playerId, player), DeliveryMethod.ReliableOrdered);
            }
            
            // Notify all players of new player.
            _players[newPlayerId] = newPlayer;
            SendToAll(new PlayerSpawn { X = newPlayer.X, Z = newPlayer.Z, Id = newPlayerId }, DeliveryMethod.ReliableOrdered);
            SendToAll(CreateUpdateInventoryPacket(newPlayerId, newPlayer), DeliveryMethod.ReliableOrdered);
        };
        
        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Peer disconnected with ID: {peer.Id}");
            _players.Remove(peer.Id);
            SendToAll(new PlayerDespawn { Id = peer.Id }, DeliveryMethod.ReliableOrdered);
        };
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
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
        
        var weaponSpawnPosition = _map.GetSpawnPosition();

        if (weaponSpawnPosition is not null)
        {
            ServerDropWeapon(WeaponType.Dagger, weaponSpawnPosition.Value.X, weaponSpawnPosition.Value.Z);
        }
        
        var stopwatch = new Stopwatch();
        
        while (true)
        {
            stopwatch.Start();

            _manager.PollEvents();
            
            Tick();

            var deltaTime = (float)stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Reset();

            var timeToNextTick = TimeSpan.FromMilliseconds(MathF.Max(TickTime - deltaTime, 0f));
            Thread.Sleep(timeToNextTick);
        }
        
        _manager.Stop();
    }

    private void Tick()
    {
        // Spawn a new enemy each tick that we aren't at the maximum.
        TrySpawnEnemy();
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
        _map.SpawnEnemy(position.X, position.Z, enemyId);
        SendToAll(new EnemySpawn { Id = enemyId, X = position.X, Z = position.Z }, DeliveryMethod.ReliableOrdered);
    }
    
    private UpdateInventory CreateUpdateInventoryPacket(int playerId, ServerPlayer player)
    {
        var updateInventory = new UpdateInventory
        {
            PlayerId = playerId,
            Weapons = new int[Inventory.SlotCount],
        };
        
        for (var i = 0; i < Inventory.SlotCount; i++)
        {
            updateInventory.Weapons[i] = (int)(player.Inventory.Weapons[i]?.WeaponType ?? WeaponType.None);
        }

        updateInventory.EquippedWeapon = (int)(player.Inventory.EquippedWeapon?.WeaponType ?? WeaponType.None);
        updateInventory.GrabbedWeapon = (int)(player.Inventory.GrabbedWeapon?.WeaponType ?? WeaponType.None);

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
    
    public void SendToPeer<T>(NetPeer peer, T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        peer.Send(_writer, deliveryMethod);
    }
    
    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        _manager.SendToAll(_writer, deliveryMethod);
    }
    
    public void SendToAll<T>(T packet, DeliveryMethod deliveryMethod, NetPeer excludePeer)
        where T : class, new()
    {
        _writer.Reset();
        _netPacketProcessor.Write(_writer, packet);
        _manager.SendToAll(_writer, deliveryMethod, excludePeer);
    }
    
    private void OnPlayerMove(PlayerMove playerMove, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.X = playerMove.X;
        player.Z = playerMove.Z;
        
        SendToAll(new PlayerMove { Id = peer.Id, X = playerMove.X, Z = playerMove.Z }, DeliveryMethod.Unreliable);
    }
    
    private void OnPlayerAttack(PlayerAttack playerAttack, NetPeer peer)
    {
        // Send the new projectile to all players except the player who created the projectile.
        // That player will have already spawned its own local copy.
        SendToAll(new ProjectileSpawn { Direction = playerAttack.Direction, X = playerAttack.X, Z = playerAttack.Z }, DeliveryMethod.ReliableOrdered, peer);
    }
    
    private void OnRequestPickupWeapon(RequestPickupWeapon requestPickupWeapon, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;
        if (!_map.DroppedWeapons.TryGetValue(requestPickupWeapon.DroppedWeaponId, out var droppedWeapon)) return;

        if (player.Inventory.AddWeapon(droppedWeapon.Weapon.WeaponType))
        {
            ServerPickupWeapon(peer.Id, droppedWeapon.Id, droppedWeapon.Weapon.WeaponType);
        }
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
            PlayerId = peer.Id,
        }, DeliveryMethod.ReliableOrdered);
    }
    
    private void OnRequestDropGrabbed(RequestDropGrabbed requestDropGrabbed, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;
        
        var droppedWeaponId = _nextDroppedWeaponId++;
        player.Inventory.DropGrabbed(_map, player.X, player.Z, droppedWeaponId);
        SendToAll(new DropGrabbed
        {
            X = player.X,
            Z = player.Z,
            DroppedWeaponId = droppedWeaponId,
            PlayerId = peer.Id
        }, DeliveryMethod.ReliableOrdered);
    }
}