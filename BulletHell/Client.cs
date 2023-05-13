using Common;
using LiteNetLib;
using LiteNetLib.Utils;

namespace BulletHell;

public class Client
{
    public readonly NetPacketProcessor NetPacketProcessor;
    public int LocalId { get; set; } = -1;

    public delegate void OnConnection();

    public delegate void OnDisconnection();

    public event OnConnection? ConnectedEvent;
    public event OnDisconnection? DisconnectedEvent;

    private readonly NetManager _manager;
    private readonly NetDataWriter _writer;

    public Client()
    {
        _writer = new NetDataWriter();
        NetPacketProcessor = new NetPacketProcessor();
        var listener = new EventBasedNetListener();
        _manager = new NetManager(listener)
        {
            AutoRecycle = true
        };
        _manager.Start();

        listener.NetworkReceiveEvent += (fromPeer, dataReader, _, _) =>
        {
            NetPacketProcessor.ReadAllPackets(dataReader, fromPeer);
        };

        listener.PeerConnectedEvent += _ => { ConnectedEvent?.Invoke(); };

        listener.PeerDisconnectedEvent += (_, _) => { DisconnectedEvent?.Invoke(); };
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
        where T : INetSerializable
    {
        _writer.Reset();
        NetPacketProcessor.WriteNetSerializable(_writer, ref packet);
        _manager.FirstPeer?.Send(_writer, deliveryMethod);
    }

    public bool IsLocal(int playerId)
    {
        return playerId == LocalId;
    }
}