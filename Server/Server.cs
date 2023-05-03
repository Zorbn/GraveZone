using Common;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

public class Server
{
    private NetPacketProcessor _netPacketProcessor;
    private EventBasedNetListener _listener;
    private NetManager _server;
    private NetDataWriter _writer;

    private Dictionary<int, Player> _players;

    public Server()
    {
        _netPacketProcessor = new NetPacketProcessor();
        _netPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
        _writer = new NetDataWriter();

        _players = new Dictionary<int, Player>();
    }
    
    public void Run()
    {
        Console.WriteLine("Starting server...");
        
        _server.Start(Networking.Port);

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
            _writer.Reset();
            _netPacketProcessor.Write(_writer, new SetLocalId { Id = newPlayerId });
            peer.Send(_writer, DeliveryMethod.ReliableOrdered);
            
            // Notify new player of old players.
            foreach (var (playerId, player) in _players)
            {
                _writer.Reset();
                _netPacketProcessor.Write(_writer, new PlayerSpawn { X = player.X, Z = player.Z, Id = playerId });
                peer.Send(_writer, DeliveryMethod.ReliableOrdered);
            }
            
            // Notify all players of new player.
            _players[newPlayerId] = newPlayer;
            _writer.Reset();
            _netPacketProcessor.Write(_writer, new PlayerSpawn { X = newPlayer.X, Z = newPlayer.Z, Id = newPlayerId });
            _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
        };
        
        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Console.WriteLine($"Peer disconnected with ID: {peer.Id}");
            _players.Remove(peer.Id);
            _writer.Reset();
            _netPacketProcessor.Write(_writer, new PlayerDespawn { Id = peer.Id });
            _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
        };
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            _netPacketProcessor.ReadAllPackets(dataReader, fromPeer);
        };

        while (!Console.KeyAvailable)
        {
            _server.PollEvents();
            Thread.Sleep(15);
        }
        
        _server.Stop();
    }
    
    private void OnPlayerMove(PlayerMove playerMove, NetPeer peer)
    {
        if (!_players.TryGetValue(peer.Id, out var player)) return;

        player.X = playerMove.X;
        player.Z = playerMove.Z;
        
        _writer.Reset();
        _netPacketProcessor.Write(_writer, new PlayerMove { Id = peer.Id, X = playerMove.X, Z = playerMove.Z });
        _server.SendToAll(_writer, DeliveryMethod.Unreliable);
    }
}