using Microsoft.Xna.Framework;

namespace Common;

public class Enemy
{
    private const float NodeReachedDistance = 0.1f;
    private const float AttackDistance = 3f;
    private const float MoveSpeed = 1f;
    public static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public Vector3 Position => _position;
    public Vector3 SpritePosition { get; private set; }
    public int Health { get; private set; }

    public readonly int Id;
    public readonly EnemyStats Stats;

    private Vector3 _position;

    private readonly List<Vector3I> _path = new();
    private int _targetPathNodeI;

    private Player _targetPlayer;
    private readonly Attacker _attacker;
    private readonly WeaponStats _weaponStats;

    public Enemy(EnemyType enemyType, float x, float z, int id,
        Attacker attacker, int? health = null)
    {
        Stats = EnemyStats.Registry[enemyType];
        _weaponStats = WeaponStats.Registry[Stats.WeaponType];
        Id = id;
        _position = new Vector3(x, 0f, z);
        SpritePosition = _position;
        Health = health ?? Stats.MaxHealth;
        _attacker = attacker;
    }

    public void CalculatePath(AStar aStar, Map map, Player targetPlayer)
    {
        if (targetPlayer is null) return;

        _targetPlayer = targetPlayer;
        _targetPathNodeI = 0;
        var startTilePosition = new Vector3I(_position);
        var goalTilePosition = new Vector3I(targetPlayer.Position);
        aStar.GetPath(map, startTilePosition, goalTilePosition, _path);
    }

    public void UpdateClient(float deltaTime)
    {
        SpritePosition = Vector3.Lerp(SpritePosition, _position, SpriteInfo.SpriteLerp * deltaTime);
    }

    public void UpdateServer(Map map, float deltaTime)
    {
        _attacker.Update(deltaTime);

        if (_targetPlayer is null) return;

        var directionToPlayer = _targetPlayer.Position - _position;
        directionToPlayer.Normalize();
        var distanceToPlayer = (_targetPlayer.Position - _position).Length();

        if (distanceToPlayer <= AttackDistance)
            _attacker.Attack(_weaponStats, directionToPlayer, Position.X, Position.Z, map);

        if (_targetPathNodeI >= _path.Count) return;

        var targetNode = _path[_targetPathNodeI].ToVector3();
        targetNode.X += 0.5f;
        targetNode.Z += 0.5f;
        var directionToNode = targetNode - _position;
        directionToNode.Normalize();
        var newPosition = _position + MoveSpeed * deltaTime * directionToNode;
        MoveTo(map, newPosition.X, newPosition.Z);
        SpritePosition = _position;

        var distanceToNode = (targetNode - _position).Length();
        if (distanceToNode > NodeReachedDistance) return;

        ++_targetPathNodeI;
    }

    public void MoveTo(Map map, float x, float z)
    {
        map.EnemiesInTiles.Remove(this, (int)_position.X, (int)_position.Z);
        _position.X = x;
        _position.Z = z;
        map.EnemiesInTiles.Add(this, (int)_position.X, (int)_position.Z);
    }

    public bool TakeDamage(int damage)
    {
        Health -= damage;

        return Health <= 0;
    }
}