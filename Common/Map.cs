﻿using Microsoft.Xna.Framework;

namespace Common;

public class Map
{
    public class UpdateResults
    {
        public readonly List<Enemy> HitEnemies = new();

        public void Clear()
        {
            HitEnemies.Clear();
        }
    }
    
    public const int Size = 16;
    private const int TileCount = Size * Size;
    public const float TileScale = 1f;
    public const float TileHeight = 2f;
    public const float FloorShade = 1.0f;
    public const float WallShade = 0.8f;

    public readonly Dictionary<int, DroppedWeapon> DroppedWeapons = new();
    public readonly Dictionary<int, Enemy> Enemies = new();
    // Projectiles don't have an id because the clients simulate projectiles
    // locally to reduce the number of packets sent for each projectile.
    public readonly List<Projectile> Projectiles = new();

    private readonly Tile[] _floorTiles = new Tile[TileCount];
    private readonly Tile[] _wallTiles = new Tile[TileCount];

    public readonly EntitiesInTiles<DroppedWeapon> DroppedWeaponsInTiles = new(Size);
    public readonly EntitiesInTiles<Enemy> EnemiesInTiles = new(Size);

    public readonly UpdateResults LastUpdateResults = new();
    
    private Random _random;

    public void Generate(int seed)
    {
        _random = new Random(seed);
        
        Array.Fill(_floorTiles, Tile.Marble);

        for (var z = 0; z < Size; ++z)
        {
            for (var x = 0; x < Size; ++x)
            {
                if (_random.NextSingle() > 0.3f) continue;

                _wallTiles[x + z * Size] = Tile.Grass;
            }
        }
    }

    public Vector3? GetSpawnPosition()
    {
        const int tileCount = Size * Size;
        for (var i = _random.Next(tileCount); i < tileCount; i = (i + 1) % tileCount)
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
            var hadCollision = Projectiles[i].Update(this, deltaTime);

            if (hadCollision)
            {
                Projectiles.RemoveAt(i);
            }
        }
    }

    public Tile GetFloorTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= Size || z >= Size) return Tile.Barrier;

        return _floorTiles[x + z * Size];
    }

    public Tile GetWallTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= Size || z >= Size) return Tile.Barrier;

        return _wallTiles[x + z * Size];
    }
    
    public Tile GetWallTileF(float x, float z)
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

    public bool SpawnEnemy(float x, float z, int id)
    {
        var tileX = (int)x;
        var tileZ = (int)z;

        if (tileX is < 0 or >= Size || tileZ is < 0 or >= Size) return false;
        
        var newEnemy = new Enemy(x, z, id);
        Enemies.Add(id, newEnemy);
        
        // TODO: Update tile positions for enemies when they move.
        EnemiesInTiles.Add(newEnemy, tileX, tileZ);
        
        return true;
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
        var droppedWeapon = new DroppedWeapon(weaponType, x, z, id);
        var tileX = (int)droppedWeapon.Position.X;
        var tileZ = (int)droppedWeapon.Position.Z;

        if (tileX is < 0 or >= Size || tileZ is < 0 or >= Size) return false;
        
        DroppedWeapons.Add(id, droppedWeapon);
        DroppedWeaponsInTiles.Add(droppedWeapon, tileX, tileZ);

        return true;
    }

    public void PickupWeapon(int id)
    {
        if (!DroppedWeapons.TryGetValue(id, out var droppedWeapon))
        {
            return;
        }
        
        var x = (int)droppedWeapon.Position.X;
        var z = (int)droppedWeapon.Position.Z;

        if (x is < 0 or >= Size || z is < 0 or >= Size) return;

        DroppedWeapons.Remove(id);
        DroppedWeaponsInTiles.Remove(droppedWeapon, x, z);
    }
}
