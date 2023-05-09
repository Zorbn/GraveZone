using Microsoft.Xna.Framework;

namespace Common;

public class Player
{
    public const float Speed = 2f;
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public readonly Inventory Inventory;

    public Vector3 Position { get; set; }

    public readonly int Id;

    public Player(int id, float x, float z)
    {
        Id = id;
        Inventory = new Inventory();
        Position = new Vector3(x, 0f, z);
    }
}