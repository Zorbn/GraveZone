using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace Common;

public class Map
{
    public class UpdateResults
    {
        public readonly List<EntityHit<Enemy>> EnemyHits = new();
        public readonly List<EntityHit<Player>> PlayerHits = new();
        public readonly List<int> WeaponsToDespawn = new();

        public void Clear()
        {
            EnemyHits.Clear();
            PlayerHits.Clear();
            WeaponsToDespawn.Clear();
        }
    }

    protected struct DecorationSprite
    {
        public Sprite Sprite;
        public Vector3 Position;
    }

    public static readonly int Size = (int)Math.Pow(2, Exponent) + 1;

    private const int Exponent = 6;
    private static readonly int BossArenaRadius = (int)Math.Ceiling(Size * 0.125f);
    private static readonly int TileCount = Size * Size;
    protected const float TileScale = 1f;
    protected const float TileHeight = 2f;
    protected const float FloorShade = 1.0f;
    protected const float WallShade = 0.8f;

    private static readonly Dictionary<MapZone, Tile> FloorTilesPerZone = new()
    {
        { MapZone.Beach, Tile.DryGrass },
        { MapZone.Grasslands, Tile.Grass },
        { MapZone.Roads, Tile.Path },
        { MapZone.Ruins, Tile.Planks },
        { MapZone.BossArena, Tile.Embers }
    };

    private static readonly Dictionary<MapZone, Tile> WallTilesPerZone = new()
    {
        { MapZone.Beach, Tile.Air },
        { MapZone.Grasslands, Tile.Flower },
        { MapZone.Roads, Tile.Air },
        { MapZone.Ruins, Tile.Brick },
        { MapZone.BossArena, Tile.Air }
    };

    private static readonly Dictionary<MapZone, Sprite> SpritesPerZone = new()
    {
        { MapZone.Beach, Sprite.PalmTree },
        { MapZone.Grasslands, Sprite.None },
        { MapZone.Roads, Sprite.None },
        { MapZone.Ruins, Sprite.None },
        { MapZone.BossArena, Sprite.Grave }
    };

    public readonly Dictionary<int, Weapon> DroppedWeapons = new();

    public readonly Dictionary<int, Enemy> Enemies = new();

    // Projectiles don't have an id because the clients simulate projectiles
    // locally to reduce the number of packets sent for each projectile.
    public readonly List<Projectile> Projectiles = new();

    private readonly Tile[] _floorTiles = new Tile[TileCount];
    private readonly Tile[] _wallTiles = new Tile[TileCount];
    protected readonly List<DecorationSprite> DecorationSprites = new();

    public readonly EntitiesInTiles<Weapon> DroppedWeaponsInTiles = new(Size);
    public readonly EntitiesInTiles<Enemy> EnemiesInTiles = new(Size);
    public readonly EntitiesInTiles<Player> PlayersInTiles = new(Size);

    public readonly UpdateResults LastUpdateResults = new();

    private readonly MidpointDisplacement _midpointDisplacement = new(Exponent);
    private Random? _random;

    public void Generate(int seed)
    {
        _random = new Random(seed);
        _midpointDisplacement.Generate(_random);

        for (var z = 0; z < Size; ++z)
        for (var x = 0; x < Size; ++x)
        {
            var i = x + z * Size;

            var centeredX = x - Size * 0.5f;
            var centeredZ = z - Size * 0.5f;

            MapZone zone;

            if (centeredX * centeredX + centeredZ * centeredZ < BossArenaRadius * BossArenaRadius)
                zone = MapZone.BossArena;
            else
                zone = _midpointDisplacement.Heightmap[i] switch
                {
                    < 0.3f => MapZone.Beach,
                    < 0.6f => MapZone.Grasslands,
                    < 0.7f => MapZone.Roads,
                    _ => MapZone.Ruins
                };

            _floorTiles[i] = FloorTilesPerZone[zone];

            var wallTile = WallTilesPerZone[zone];
            if (wallTile != Tile.Air && _random.NextSingle() < 0.1f)
            {
                _wallTiles[i] = WallTilesPerZone[zone];
                continue;
            }

            var sprite = SpritesPerZone[zone];
            if (sprite != Sprite.None && _random.NextSingle() < 0.1f)
                DecorationSprites.Add(new DecorationSprite
                {
                    Sprite = sprite,
                    Position = new Vector3(x + 0.5f, 0f, z + 0.5f)
                });
        }
    }

    public Vector3? GetSpawnPosition()
    {
        if (_random is null) return null;

        for (var i = _random.Next(TileCount); i < TileCount; i = (i + 1) % TileCount)
        {
            var x = i % Size;
            var z = i / Size;

            if (GetWallTile(x, z) != Tile.Air) continue;

            return new Vector3(x + 0.5f, 0f, z + 0.5f);
        }

        return null;
    }

    public void Update(float deltaTime)
    {
        LastUpdateResults.Clear();

        for (var i = Projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = Projectiles[i];
            var hadCollision = projectile.Update(this, deltaTime);
            Projectiles[i] = projectile;

            if (hadCollision) Projectiles.RemoveAt(i);
        }

        foreach (var (droppedWeaponId, droppedWeapon) in DroppedWeapons)
        {
            var shouldDespawn = droppedWeapon.Update(deltaTime);

            if (!shouldDespawn) continue;

            LastUpdateResults.WeaponsToDespawn.Add(droppedWeaponId);
        }
    }

    protected Tile GetFloorTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= Size || z >= Size) return Tile.Barrier;

        return _floorTiles[x + z * Size];
    }

    public Tile GetWallTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= Size || z >= Size) return Tile.Barrier;

        return _wallTiles[x + z * Size];
    }

    private Tile GetWallTileF(float x, float z)
    {
        var ix = (int)MathF.Floor(x);
        var iz = (int)MathF.Floor(z);

        return GetWallTile(ix, iz);
    }

    public bool IsCollidingWithBox(Vector3 at, Vector3 size)
    {
        return GetWallTileF(at.X - size.X * 0.5f, at.Z - size.Z * 0.5f) != Tile.Air ||
               GetWallTileF(at.X + size.X * 0.5f, at.Z - size.Z * 0.5f) != Tile.Air ||
               GetWallTileF(at.X - size.X * 0.5f, at.Z + size.Z * 0.5f) != Tile.Air ||
               GetWallTileF(at.X + size.X * 0.5f, at.Z + size.Z * 0.5f) != Tile.Air;
    }

    public Enemy? SpawnRandomNormalEnemy(float x, float z, int id, Attacker attacker)
    {
        return SpawnRandomEnemy(x, z, id, attacker, EnemyStats.NormalEnemyTypes);
    }

    public Enemy? SpawnRandomBossEnemy(float x, float z, int id, Attacker attacker)
    {
        return SpawnRandomEnemy(x, z, id, attacker, EnemyStats.BossEnemyTypes);
    }

    private Enemy? SpawnRandomEnemy(float x, float z, int id, Attacker attacker, ReadOnlyCollection<EnemyType> possibleEnemyTypes)
    {
        if (_random is null) return null;

        var enemyType = possibleEnemyTypes.Choose(_random);
        return SpawnEnemy(enemyType, x, z, id, attacker);
    }

    public Enemy? SpawnEnemy(EnemyType enemyType, float x, float z, int id, Attacker? attacker, int? health = null)
    {
        if (Enemies.ContainsKey(id)) return null;

        var tileX = (int)x;
        var tileZ = (int)z;

        if (tileX < 0 || tileX >= Size || tileZ < 0 || tileZ >= Size) return null;

        var newEnemy = new Enemy(enemyType, x, z, id, attacker, health);
        Enemies.Add(id, newEnemy);

        EnemiesInTiles.Add(newEnemy, tileX, tileZ);

        return newEnemy;
    }

    public void DespawnEnemy(int id)
    {
        if (!Enemies.TryGetValue(id, out var enemy)) return;

        var enemyTileX = (int)enemy.Position.X;
        var enemyTileZ = (int)enemy.Position.Z;

        EnemiesInTiles.Remove(enemy, enemyTileX, enemyTileZ);

        Enemies.Remove(id);
    }

    public bool DropWeapon(WeaponType weaponType, float x, float z, int id)
    {
        if (DroppedWeapons.ContainsKey(id)) return false;

        var droppedWeapon = new Weapon(weaponType, x, z, id);
        var tileX = (int)droppedWeapon.Position.X;
        var tileZ = (int)droppedWeapon.Position.Z;

        if (tileX < 0 || tileX >= Size || tileZ < 0 || tileZ >= Size) return false;

        DroppedWeapons.Add(id, droppedWeapon);
        DroppedWeaponsInTiles.Add(droppedWeapon, tileX, tileZ);

        return true;
    }

    public void PickupWeapon(int id)
    {
        if (!DroppedWeapons.TryGetValue(id, out var droppedWeapon)) return;

        var x = (int)droppedWeapon.Position.X;
        var z = (int)droppedWeapon.Position.Z;

        if (x < 0 || x >= Size || z < 0 || z >= Size) return;

        DroppedWeapons.Remove(id);
        DroppedWeaponsInTiles.Remove(droppedWeapon, x, z);
    }

    public void AddAttackProjectiles(WeaponType weaponType, Team team, Vector3 direction, float x, float z)
    {
        var weaponStats = WeaponStats.Registry[weaponType]!;
        foreach (var projectileSpawn in weaponStats.ProjectileSpawns)
        {
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(projectileSpawn.Angle));
            var forward = projectileSpawn.RelativeToForward ? direction : Vector3.Forward;
            var projectileDirection = Vector3.Transform(forward, rotation);
            Projectiles.Add(new Projectile(projectileSpawn.ProjectileType, weaponType, team, projectileDirection, x,
                z));
        }
    }
}