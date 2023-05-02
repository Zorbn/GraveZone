using Common;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

public class Server
{
    private EventBasedNetListener _listener;
    private NetManager _server;
    
    public Server()
    {
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
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
            Console.WriteLine($"Connection from IP: {peer.EndPoint}");
            var writer = new NetDataWriter();
            writer.Put("Hello client!");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        while (!Console.KeyAvailable)
        {
            _server.PollEvents();
            Thread.Sleep(15);
        }
        
        _server.Stop();
    }
}