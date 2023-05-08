using Common;
using Microsoft.Xna.Framework;

namespace Server;

// TODO: Unify server player and client player?
public class ServerPlayer
{
    public Vector3 Position;
    public readonly Inventory Inventory;

    public ServerPlayer()
    {
        Inventory = new Inventory();
    }
}