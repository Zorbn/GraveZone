using Microsoft.Xna.Framework;

namespace Common;

public struct Projectile
{
    private static readonly Vector3 Size = new(0.6f, 1.0f, 0.6f);

    public Vector3 Position => _position;
    public readonly Vector3 Direction;
    public readonly ProjectileStats Stats;
    public readonly WeaponType WeaponType;
    public readonly Team Team;

    private Vector3 _position;
    private readonly Vector3 _startPosition;

    public Projectile(ProjectileType projectileType, WeaponType weaponType, Team team, Vector3 direction, float x,
        float z)
    {
        Stats = ProjectileStats.Registry[projectileType];
        WeaponType = weaponType;
        Team = team;
        Direction = direction;
        _position = new Vector3(x, 0f, z);
        _startPosition = _position;
    }

    // Returns true if the projectile collided with something.
    private bool Move(Vector3 movement, Map map, float deltaTime)
    {
        if (movement.Length() == 0f) return false;

        var hasCollision = false;

        movement.Normalize();

        var newPosition = _position;
        newPosition.X += movement.X * Stats.Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.X = _position.X;
            hasCollision = true;
        }

        _position.X = newPosition.X;

        newPosition.Z += movement.Z * Stats.Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.Z = _position.Z;
            hasCollision = true;
        }

        _position.Z = newPosition.Z;

        var distanceTravelled = (_position - _startPosition).Length();
        if (distanceTravelled > Stats.Range) hasCollision = true;

        switch (Team)
        {
            case Team.Players:
                CheckEnemyCollisions(map, ref hasCollision);
                break;
            case Team.Enemies:
                CheckPlayerCollisions(map, ref hasCollision);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return hasCollision;
    }

    private void CheckEnemyCollisions(Map map, ref bool hasCollision)
    {
        var nearbyEnemies = map.EnemiesInTiles.GetNearby((int)_position.X, (int)_position.Z);
        foreach (var nearbyEnemy in nearbyEnemies)
        {
            if (!Collision.HasCollision(_position, Size, nearbyEnemy.Position, Enemy.Size)) continue;

            hasCollision = true;
            map.LastUpdateResults.EnemyHits.Add(new EntityHit<Enemy>
            {
                Entity = nearbyEnemy,
                Damage = Stats.Damage
            });

            break;
        }
    }

    private void CheckPlayerCollisions(Map map, ref bool hasCollision)
    {
        var nearbyPlayers = map.PlayersInTiles.GetNearby((int)_position.X, (int)_position.Z);
        foreach (var nearbyPlayer in nearbyPlayers)
        {
            if (!Collision.HasCollision(_position, Size, nearbyPlayer.Position, Player.Size)) continue;

            hasCollision = true;
            map.LastUpdateResults.PlayerHits.Add(new EntityHit<Player>
            {
                Entity = nearbyPlayer,
                Damage = Stats.Damage
            });

            break;
        }
    }

    public bool Update(Map map, float deltaTime)
    {
        return Move(Direction, map, deltaTime);
    }
}