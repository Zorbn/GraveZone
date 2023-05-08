using BulletHell;
using Microsoft.Xna.Framework;

namespace Common;

public class Projectile
{
    public static readonly Point ProjectileSpriteCoords = new(8, 0);
    private const float Speed = 8f;
    private static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public Vector3 Position => _position;
    public readonly Vector3 Direction;

    private Vector3 _position;

    public Projectile(Vector3 direction, float x, float z)
    {
        Direction = direction;
        _position = new Vector3(x, 0f, z);
    }

    // Returns true if the projectile collided with something.
    private bool Move(Vector3 movement, Map map, float deltaTime)
    {
        if (movement.Length() == 0f) return false;

        var hasCollision = false;

        movement.Normalize();

        var newPosition = _position;
        newPosition.X += movement.X * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.X = _position.X;
            hasCollision = true;
        }

        _position.X = newPosition.X;

        newPosition.Z += movement.Z * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.Z = _position.Z;
            hasCollision = true;
        }

        _position.Z = newPosition.Z;

        var nearbyEnemies = map.GetNearbyEnemies((int)_position.X, (int)_position.Z);
        foreach (var nearbyEnemy in nearbyEnemies)
        {
            if (!Collision.HasCollision(_position, Size, nearbyEnemy.Position, Enemy.Size)) continue;

            hasCollision = true;
            map.LastUpdateResults.HitEnemies.Add(nearbyEnemy);

            break;
        }

        return hasCollision;
    }

    public bool Update(Map map, float deltaTime)
    {
        return Move(Direction, map, deltaTime);
    }
}