﻿namespace Common;

public class EntitiesInTiles<T>
{
    private readonly int _size;
    private readonly int _tileCount;

    private readonly List<T>[] _entitiesInTiles;
    private readonly HashSet<T> _entityQueryResults;

    public EntitiesInTiles(int size)
    {
        _size = size;
        _tileCount = size * size;

        _entitiesInTiles = new List<T>[_tileCount];
        _entityQueryResults = new HashSet<T>();

        for (var i = 0; i < _tileCount; i++) _entitiesInTiles[i] = new List<T>();
    }

    public void Add(T entity, int x, int z)
    {
        _entitiesInTiles[x + z * _size].Add(entity);
    }

    public void Remove(T entity, int x, int z)
    {
        _entitiesInTiles[x + z * _size].Remove(entity);
    }

    public HashSet<T> GetNearby(float x, float z)
    {
        _entityQueryResults.Clear();

        var tileX = (int)x;
        var tileZ = (int)z;

        for (var zi = -1; zi <= 1; zi++)
        {
            var targetZ = tileZ + zi;

            for (var xi = -1; xi <= 1; xi++)
            {
                var targetX = tileX + xi;

                if (targetX < 0 || targetX >= _size || targetZ < 0 || targetZ >= _size) continue;

                var entitiesInTile = _entitiesInTiles[targetX + targetZ * _size];
                foreach (var entity in entitiesInTile) _entityQueryResults.Add(entity);
            }
        }

        return _entityQueryResults;
    }

    public void Clear()
    {
        for (var i = 0; i < _tileCount; i++) _entitiesInTiles[i].Clear();
        _entityQueryResults.Clear();
    }
}