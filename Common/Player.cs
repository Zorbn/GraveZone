using Microsoft.Xna.Framework;

namespace Common;

public class Player
{
    protected const int MaxHealth = 100;
    protected const float Speed = 2.5f;
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public readonly Inventory Inventory;

    public int Health { get; private set; }
    public Vector3 Position { get; private set; }
    protected Vector3 SpritePosition;

    public readonly int Id;
    protected bool IsDead { get; private set; }

    public Player(Map map, int id, float x, float z, int? health = null)
    {
        Id = id;
        Inventory = new Inventory();
        Position = new Vector3(x, 0f, z);
        SpritePosition = Position;
        Health = health ?? MaxHealth;
        map.PlayersInTiles.Add(this, (int)Position.X, (int)Position.Z);
    }

    // Move without updating the sprite position, so that it can be interpolated.
    public void MoveTo(Map map, Vector3 position)
    {
        map.PlayersInTiles.Remove(this, (int)Position.X, (int)Position.Z);
        Position = position;
        map.PlayersInTiles.Add(this, (int)Position.X, (int)Position.Z);
    }

    // Move and also update the sprite position.
    protected void Teleport(Map map, Vector3 position)
    {
        MoveTo(map, position);
        SpritePosition = Position;
    }

    public void Respawn(Map map, Vector3 position)
    {
        Health = MaxHealth;
        Teleport(map, position);
        IsDead = false;
    }

    public void Despawn(Map map)
    {
        map.PlayersInTiles.Remove(this, (int)Position.X, (int)Position.Z);
    }

    public virtual bool TakeDamage(int damage)
    {
        Health -= damage;

        if (Health > 0) return false;

        IsDead = true;
        return true;
    }

    public void Heal(int amount)
    {
        Health += amount;

        if (Health <= MaxHealth) return;

        Health = MaxHealth;
    }
}