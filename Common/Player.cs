using Microsoft.Xna.Framework;

namespace Common;

public class Player
{
    public const float Speed = 2f;
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public readonly Inventory Inventory;

    public Vector3 Position { get; private set; }

    public readonly int Id;

    public Player(Map map, int id, float x, float z)
    {
        Id = id;
        Inventory = new Inventory();
        Position = new Vector3(x, 0f, z);
        map.PlayersInTiles.Add(this, (int)Position.X, (int)Position.Z);
    }

    public void MoveTo(Map map, Vector3 position)
    {
        map.PlayersInTiles.Remove(this, (int)Position.X, (int)Position.Z);
        Position = position;
        map.PlayersInTiles.Add(this, (int)Position.X, (int)Position.Z);
    }

    public void Despawn(Map map)
    {
        map.PlayersInTiles.Remove(this, (int)Position.X, (int)Position.Z);
    }
}