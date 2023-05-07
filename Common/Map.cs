using Microsoft.Xna.Framework;

namespace Common;

public class Map
{
    public const int Size = 16;
    public const float TileScale = 1f;
    public const float TileHeight = 2f;
    public const float FloorShade = 1.0f;
    public const float WallShade = 0.8f;

    public readonly Dictionary<int, DroppedWeapon> DroppedWeapons;
    public readonly Dictionary<int, Enemy> Enemies;
    
    private Tile[] _floorTiles;
    private Tile[] _wallTiles;
    private List<DroppedWeapon>[] _droppedWeaponsInTiles;
    private HashSet<DroppedWeapon> _droppedWeaponQueryResults;

    private Random _random;

    public Map()
    {
        var tileCount = Size * Size;
        _floorTiles = new Tile[tileCount];
        _wallTiles = new Tile[tileCount];
        
        _droppedWeaponsInTiles = new List<DroppedWeapon>[tileCount];
        for (var i = 0; i < tileCount; i++)
        {
            _droppedWeaponsInTiles[i] = new List<DroppedWeapon>();
        }
        
        _droppedWeaponQueryResults = new HashSet<DroppedWeapon>();
        
        DroppedWeapons = new Dictionary<int, DroppedWeapon>();
        Enemies = new Dictionary<int, Enemy>();
    }

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
        
        var newEnemy = new Enemy(x, z);
        Enemies.Add(id, newEnemy);
        
        return true;
    }

    public void DespawnEnemy(int id)
    {
        Enemies.Remove(id);
    }
    
    public bool DropWeapon(WeaponType weaponType, float x, float z, int id)
    {
        var droppedWeapon = new DroppedWeapon(weaponType, x, z, id);
        var tileX = (int)droppedWeapon.Position.X;
        var tileZ = (int)droppedWeapon.Position.Z;

        if (tileX is < 0 or >= Size || tileZ is < 0 or >= Size) return false;
        
        DroppedWeapons.Add(id, droppedWeapon);
        _droppedWeaponsInTiles[tileX + tileZ * Size].Add(droppedWeapon);

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
        _droppedWeaponsInTiles[x + z * Size].Remove(droppedWeapon);
    }

    public IEnumerable<DroppedWeapon> GetNearbyDroppedWeapons(float x, float z)
    {
        _droppedWeaponQueryResults.Clear();
        
        var tileX = (int)x;
        var tileZ = (int)z;

        for (var zi = -1; zi <= 1; zi++)
        {
            var targetZ = tileZ + zi;
            
            for (var xi = -1; xi <= 1; xi++)
            {
                var targetX = tileX + xi;
                
                if (targetX is < 0 or >= Size || targetZ is < 0 or >= Size) continue;

                var droppedWeaponsInTile = _droppedWeaponsInTiles[targetX + targetZ * Size];
                foreach (var droppedWeapon in droppedWeaponsInTile)
                {
                    _droppedWeaponQueryResults.Add(droppedWeapon);
                }
            }
        }

        foreach (var droppedWeapon in _droppedWeaponQueryResults)
        {
            yield return droppedWeapon;
        }
    }
}
