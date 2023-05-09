using Microsoft.Xna.Framework;

namespace Common;

public class Enemy
{
    private const int MaxHealth = 20;
    private const float NodeReachedDistance = 0.1f;
    private const float MoveSpeed = 1f;
    public  static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public Vector3 Position => _position;
    public Vector3 SpritePosition => _spritePosition;
    public readonly int Id;
    public readonly EnemyStats Stats;
    
    private Vector3 _position;
    private Vector3 _spritePosition;
    private int _health = MaxHealth;

    private List<Vector3I> _path = new();
    private int _targetPathNodeI;

    public Enemy(EnemyType enemyType, float x, float z, int id)
    {
        Stats = EnemyStats.Registry[enemyType];
        Id = id;
        _position = new Vector3(x, 0f, z);
        _spritePosition = _position;
    }

    // TODO: Store target player instead of passing in nearestPlayerPosition, the target player could also be used for
    // when the enemy needs to begin doing damage. However, the player needs to be unified into Common before this can happen. 
    public void CalculatePath(AStar aStar, Map map, Vector3I nearestPlayerPosition)
    {
        _targetPathNodeI = 0;
        var tilePosition = new Vector3I(_position);
        aStar.GetPath(map, tilePosition, nearestPlayerPosition, _path);
    }

    public void UpdateClient(float deltaTime)
    {
        _spritePosition = Vector3.Lerp(_spritePosition, _position, SpriteInfo.SpriteLerp * deltaTime);
    }

    public void UpdateServer(Map map, float deltaTime)
    {
        if (_targetPathNodeI >= _path.Count) return;

        var targetNode = _path[_targetPathNodeI].ToVector3();
        targetNode.X += 0.5f;
        targetNode.Z += 0.5f;
        var direction = targetNode - _position;
        direction.Normalize();
        var newPosition = _position + MoveSpeed * deltaTime * direction;
        MoveTo(map, newPosition.X, newPosition.Z);
        _spritePosition = _position;

        var distanceToTarget = (targetNode - _position).Length();
        if (distanceToTarget > NodeReachedDistance) return;

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
        _health -= damage;

        return _health <= 0;
    }
}