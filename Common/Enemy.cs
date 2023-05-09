using Microsoft.Xna.Framework;

namespace Common;

// TODO: Make enemies attack players by spawning projectiles (ie: ShelledMimic spawns 360 degree burst, VampireMimic fires single shot towards player.)
// Each enemy attacks based on what weapon it has equipped (these are the same weapons that players can equip) and enemies have
// a small chance to drop their weapon for the player when they die.
public class Enemy
{
    private const float NodeReachedDistance = 0.1f;
    private const float MoveSpeed = 1f;
    public  static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    public Vector3 Position => _position;
    public Vector3 SpritePosition { get; private set; }
    public int Health => _health;
    
    public readonly int Id;
    public readonly EnemyStats Stats;
    
    private Vector3 _position;
    private int _health;

    private List<Vector3I> _path = new();
    private int _targetPathNodeI;

    private Player _targetPlayer;

    public Enemy(EnemyType enemyType, float x, float z, int id, int? health = null)
    {
        Stats = EnemyStats.Registry[enemyType];
        health ??= Stats.MaxHealth;
        Id = id;
        _position = new Vector3(x, 0f, z);
        _health = health.Value;
        SpritePosition = _position;
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
        if (_targetPathNodeI >= _path.Count) return;

        var targetNode = _path[_targetPathNodeI].ToVector3();
        targetNode.X += 0.5f;
        targetNode.Z += 0.5f;
        var direction = targetNode - _position;
        direction.Normalize();
        var newPosition = _position + MoveSpeed * deltaTime * direction;
        MoveTo(map, newPosition.X, newPosition.Z);
        SpritePosition = _position;

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