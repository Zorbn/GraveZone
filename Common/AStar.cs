namespace Common;

public class AStar
{
    private readonly List<Vector3I> _neighbors = new();
    private readonly Dictionary<Vector3I, Vector3I> _cameFrom = new();
    private readonly PriorityQueue<Vector3I, int> _frontier = new();

    public void GetPath(Map map, Vector3I start, Vector3I goal, List<Vector3I> path)
    {
        Search(map, start, goal);
        ReconstructPath(start, goal, path);
    }

    private static int Heuristic(Vector3I a, Vector3I b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Z - b.Z);
    }

    private void GetNeighbors(Vector3I position, Map map)
    {
        _neighbors.Clear();
        
        if (map.GetWallTile(position.X + 1, position.Z) == Tile.Air) _neighbors.Add(new Vector3I(position.X + 1, 0, position.Z));
        if (map.GetWallTile(position.X - 1, position.Z) == Tile.Air) _neighbors.Add(new Vector3I(position.X - 1, 0, position.Z));
        if (map.GetWallTile(position.X, position.Z + 1) == Tile.Air) _neighbors.Add(new Vector3I(position.X, 0, position.Z + 1));
        if (map.GetWallTile(position.X, position.Z - 1) == Tile.Air) _neighbors.Add(new Vector3I(position.X, 0, position.Z - 1));
    }

    private void Search(Map map, Vector3I start, Vector3I goal)
    {
        _cameFrom.Clear();

        if (start.Y != goal.Y) return;

        _neighbors.Clear();
        _frontier.Clear();
        
        _frontier.Enqueue(start, 0);
        _cameFrom.Add(start, start);

        while (_frontier.TryDequeue(out var current, out _))
        {
            if (current == goal) break;
            
            GetNeighbors(current, map);
            foreach (var neighbor in _neighbors)
            {
                if (_cameFrom.ContainsKey(neighbor)) continue;

                var priority = Heuristic(neighbor, goal);
                _frontier.Enqueue(neighbor, priority);
                _cameFrom.Add(neighbor, current);
            }
        }
        
        Console.WriteLine(_cameFrom.Count);
    }

    private void ReconstructPath(Vector3I start, Vector3I goal, List<Vector3I> path)
    {
        path.Clear();

        var current = goal;

        if (!_cameFrom.ContainsKey(goal))
        {
            return;
        }

        while (current != start)
        {
            path.Add(current);
            current = _cameFrom[current];
        }

        path.Reverse();
    }
}