using System.Collections.ObjectModel;
using System.Diagnostics;
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
        { MapZone.Forest, Tile.DryGrass },
        { MapZone.Factory, Tile.Slime },
        { MapZone.Grasslands, Tile.Grass },
        { MapZone.PumpkinPatch, Tile.GardenDirt },
        { MapZone.Roads, Tile.Path },
        { MapZone.WheatField, Tile.Field },
        { MapZone.Ruins, Tile.Planks },
        { MapZone.BossArena, Tile.Embers }
    };

    private static readonly Dictionary<MapZone, Tile> WallTilesPerZone = new()
    {
        { MapZone.Forest, Tile.Air },
        { MapZone.Factory, Tile.FactoryWall },
        { MapZone.Grasslands, Tile.Flower },
        { MapZone.PumpkinPatch, Tile.Air },
        { MapZone.Roads, Tile.Air },
        { MapZone.WheatField, Tile.Air },
        { MapZone.Ruins, Tile.Brick },
        { MapZone.BossArena, Tile.Air }
    };

    private static readonly Dictionary<MapZone, Sprite> SpritesPerZone = new()
    {
        { MapZone.Forest, Sprite.Tree },
        { MapZone.Factory, Sprite.None },
        { MapZone.Grasslands, Sprite.None },
        { MapZone.PumpkinPatch, Sprite.Pumpkin },
        { MapZone.Roads, Sprite.None },
        { MapZone.WheatField, Sprite.Wheat },
        { MapZone.Ruins, Sprite.None },
        { MapZone.BossArena, Sprite.Grave }
    };

    private static readonly MapZone[] LowlandsZones = { MapZone.Forest, MapZone.WheatField };
    private static readonly MapZone[] MidlandsZones = { MapZone.Grasslands, MapZone.PumpkinPatch };
    private static readonly MapZone[] BorderZones = { MapZone.Roads };
    private static readonly MapZone[] HighlandsZones = { MapZone.Ruins, MapZone.Factory };

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
    public readonly KillTracker KillTracker = new();

    public int LastSeed { get; private set; }

    private readonly MidpointDisplacement _midpointDisplacement = new(Exponent);
    // Net Random is used for randomness that is synchronized between the client and the server,
    // all uses of Net Random on the server should correspond to a use on the client and vise versa.
    private Random? _netRandom;
    // Local Random is used for randomness that doesn't need to be synchronized between the client and
    // the server, calls to Local Random can happen independently on either the client or the server.
    private Random? _localRandom;

    public void Generate(int seed, int enemiesKilled)
    {
        LastSeed = seed;

        // The map may be generated multiple times, so it needs to be reset.
        KillTracker.Reset(enemiesKilled);
        Clear();

        _localRandom = new Random(seed);
        _netRandom = new Random(seed);
        _midpointDisplacement.Generate(_netRandom);

        var lowlandsZone = LowlandsZones.Choose(_netRandom);
        var midlandsZone = MidlandsZones.Choose(_netRandom);
        var borderZone = BorderZones.Choose(_netRandom);
        var highlandsZone = HighlandsZones.Choose(_netRandom);

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
                    < 0.4f => lowlandsZone,
                    < 0.6f => midlandsZone,
                    < 0.7f => borderZone,
                    _ => highlandsZone
                };

            _floorTiles[i] = FloorTilesPerZone[zone];

            var wallTile = WallTilesPerZone[zone];
            if (wallTile != Tile.Air && _netRandom.NextSingle() < 0.1f)
            {
                _wallTiles[i] = WallTilesPerZone[zone];
                continue;
            }

            var sprite = SpritesPerZone[zone];
            if (sprite != Sprite.None && _netRandom.NextSingle() < 0.1f)
                DecorationSprites.Add(new DecorationSprite
                {
                    Sprite = sprite,
                    Position = new Vector3(x + 0.5f, 0f, z + 0.5f)
                });
        }
    }

    private void Clear()
    {
        DroppedWeapons.Clear();
        Enemies.Clear();
        Projectiles.Clear();

        Array.Fill(_floorTiles, Tile.Air);
        Array.Fill(_wallTiles, Tile.Air);
        DecorationSprites.Clear();

        DroppedWeaponsInTiles.Clear();
        EnemiesInTiles.Clear();
        PlayersInTiles.Clear();
    }

    public Vector3? GetSpawnPosition()
    {
        if (_localRandom is null) return null;

        for (var i = _localRandom.Next(TileCount); i < TileCount; i = (i + 1) % TileCount)
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
        if (_localRandom is null) return null;

        var enemyType = possibleEnemyTypes.Choose(_localRandom);
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

    // Returns true if a new boss should be spawned.
    public bool DespawnEnemy(int id, bool wasKilled = true)
    {
        if (!Enemies.TryGetValue(id, out var enemy)) return false;

        var enemyTileX = (int)enemy.Position.X;
        var enemyTileZ = (int)enemy.Position.Z;

        EnemiesInTiles.Remove(enemy, enemyTileX, enemyTileZ);

        Enemies.Remove(id);

        return wasKilled && KillTracker.EnemyDied();
    }

    // Empties an inventory onto the ground randomly near the specified location.
    // Calls to this method must be synchronized between the client and the server.
    public void DropInventory(Inventory inventory, float x, float z, int baseId)
    {
        Debug.Assert(_netRandom is not null);

        var tileX = MathF.Floor(x);
        var tileZ = MathF.Floor(z);

        for (var i = 0; i < Inventory.SlotCount; i++)
        {
            var removedWeapon = inventory.RemoveWeapon(i);

            if (removedWeapon.IsNone) continue;

            var dropX = tileX + _netRandom.NextSingle();
            var dropZ = tileZ + _netRandom.NextSingle();

            DropWeapon(removedWeapon.WeaponType, dropX, dropZ, baseId + i);
        }
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
        var weaponStats = WeaponStats.Registry[weaponType];
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