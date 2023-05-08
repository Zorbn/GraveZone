using Microsoft.Xna.Framework;

namespace Common;

public class Enemy
{
    private const int MaxHealth = 20;
    public  static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public Vector3 Position => _position;
    public readonly int Id;
    
    private Vector3 _position;
    private int _health = MaxHealth;
    
    public Enemy(float x, float z, int id)
    {
        Id = id;
        _position = new Vector3(x, 0f, z);
    }

    public bool TakeDamage(int damage)
    {
        _health -= damage;

        return _health <= 0;
    }
}