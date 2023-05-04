using Common;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

public class Server
{
    private NetPacketProcessor _netPacketProcessor;
    private EventBasedNetListener _listener;
    private NetManager _manager;
    private NetDataWriter _writer;

    private Dictionary<int, Player> _players;
    private Random _random;
    private int _mapSeed;

    public Server()
    {
        _netPacketProcessor = new NetPacketProcessor();
        _netPacketProcessor.RegisterNestedType<NetVector3>();
        _netPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        _netPacketProcessor.SubscribeReusable<PlayerAttack, NetPeer>(OnPlayerAttack);
        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        _writer = new NetDataWriter();

        _players = new Dictionary<int, Player>();
        _random = new Random();
    }
    
    public void Run()
    {
        Console.WriteLine("Starting server...");

        _mapSeed = _random.Next();
        
        _manager.Start(Networking.Port);

        _listener.ConnectionRequestEvent += request =>
        {
            // TODO: Use AcceptIfKey/Connection keys to make servers password protected.
            request.Accept();
        };
        
        _listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Connection from IP: {peer.EndPoint} with ID: {peer.Id}");
            
            var newPlayer = new Player { X = -1, Z = -1 };
            var newPlayerId = peer.Id;
            
            // Tell the new player their id.
            SendToPeer(peer, new SetLocalId { Id = newPlayerId }, DeliveryMethod.ReliableOrdered);
            
            // Tell the new player to generate the map.
            SendToPeer(peer, new MapGenerate { Seed = _mapSeed }, DeliveryMethod.ReliableOrdered);
            
            // Notify new player of old players.
            foreach (var (playerId, player) in _players)
            {
                SendToPeer(peer, new PlayerSpawn { X = player.X, Z = player.Z, Id = playerId }, DeliveryMethod.ReliableOrdered);
            }
            
            // Notify all players of new player.
            _players[newPlayerId] = newPlayer;
            SendToAll(new PlayerSpawn { X = newPlayer.X, Z = newPlayer.Z, Id = newPlayerId }, DeliveryMethod.ReliableOrdered);
        };
        
        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Peer disconnected with ID: {peer.Id}");
            _players.Remove(peer.Id);
            SendToAll(new PlayerDespawn { Id = peer.Id }, DeliveryMethod.ReliableOrdered);
        };
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            _netPacketProcessor.ReadAllPackets(dataReader, fromPeer);
        };

        while (!Console.KeyAvailable)
        {
            _manager.PollEvents();
            Thread.Sleep(15);
        }
        
        _manager.Stop();
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
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        // Send the new projectile to all players except the player who created the projectile.
        // That player will have already spawned its own local copy.
        SendToAll(new ProjectileSpawn { Direction = playerAttack.Direction, X = playerAttack.X, Z = playerAttack.Z }, DeliveryMethod.ReliableUnordered, peer);
    }
}