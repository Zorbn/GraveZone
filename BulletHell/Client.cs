using Common;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BulletHell;

public class Client
{
    public readonly NetPacketProcessor NetPacketProcessor;
    public int LocalId { get; set; } = -1;

    public delegate void OnConnection();
    public event OnConnection ConnectedEvent;
    
    private readonly EventBasedNetListener _listener;
    private readonly NetManager _manager;
    private readonly NetDataWriter _writer;

    public Client()
    {
        _writer = new NetDataWriter();
        NetPacketProcessor = new NetPacketProcessor();
        _listener = new EventBasedNetListener();
        _manager = new NetManager(_listener);
        _manager.Start();
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            NetPacketProcessor.ReadAllPackets(dataReader, fromPeer);
        };
        
        _listener.PeerConnectedEvent += _ =>
        {
            ConnectedEvent?.Invoke();
        };
    }

    public void Connect(string ip)
    {
        _manager.Connect(ip, Networking.Port, "");
    }

    public void Disconnect()
    {
        _manager.DisconnectAll();
    }

    public void Stop()
    {
        _manager.Stop();
    }

    public void PollEvents()
    {
        _manager.PollEvents();
    }

    public void SendToServer<T>(T packet, DeliveryMethod deliveryMethod)
        where T : class, new()
    {
        _writer.Reset();
        NetPacketProcessor.Write(_writer, packet);
        _manager.FirstPeer.Send(_writer, deliveryMethod);
    }

    public bool IsLocal(int playerId)
    {
        return playerId == LocalId;
    }
}