using Common;

namespace Server;

// TODO: Unify server player and client player?
public class ServerPlayer
{
    public float X;
    public float Z;

    public readonly Inventory Inventory;

    public ServerPlayer()
    {
        Inventory = new Inventory();
    }
}