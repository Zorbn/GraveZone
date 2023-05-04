using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

// TODO: Sync map, destroy projectiles that go outside the map.
public class Map
{
    private const float TileScale = 1f;
    private const float TileHeight = 2f;
    private const float FloorShade = 1.0f;
    private const float WallShade = 0.8f;

    private Tile[] _floorTiles;
    private Tile[] _wallTiles;

    private int _size;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private List<VertexPositionColorTexture> _vertices;
    private List<ushort> _indices;
    private int _primitives;

    private Random _random;

    public Map(int size, GraphicsDevice graphicsDevice)
    {
        _size = size;
        _random = new Random();

        _floorTiles = new Tile[size * size];
        _wallTiles = new Tile[size * size];
        
        Array.Fill(_floorTiles, Tile.Marble);

        for (var z = 0; z < size; ++z)
        {
            for (var x = 0; x < size; ++x)
            {
                if (_random.NextSingle() > 0.3f) continue;

                _wallTiles[x + z * size] = Tile.Marble;
            }
        }

        _vertices = new List<VertexPositionColorTexture>();
        _indices = new List<ushort>();
    }

    private Tile GetFloorTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= _size || z >= _size) return Tile.Air;

        return _floorTiles[x + z * _size];
    }

    public Tile GetWallTile(int x, int z)
    {
        if (x < 0 || z < 0 || x >= _size || z >= _size) return Tile.Air;

        return _wallTiles[x + z * _size];
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

    public void Mesh(GraphicsDevice graphicsDevice)
    {
        if (_vertexBuffer is not null) _vertexBuffer.Dispose();
        if (_indexBuffer is not null) _indexBuffer.Dispose();

        _vertices.Clear();
        _indices.Clear();

        // Mesh the walls, including all of their visible sides.
        for (var z = 0; z < _size; ++z)
        {
            for (var x = 0; x < _size; ++x)
            {
                var tile = GetWallTile(x, z);

                if (tile == Tile.Air) continue;

                for (var i = 0; i < 6; i++)
                {
                    var direction = (Direction)i;

                    var sideIndices = CubeMesh.Indices[direction];
                    var baseVertexCount = _vertices.Count;
                    foreach (var index in sideIndices)
                    {
                        _indices.Add((ushort)(index + baseVertexCount));
                    }

                    var sideVertices = CubeMesh.Vertices[direction];
                    foreach (var vertex in sideVertices)
                    {
                        var newVertex = vertex;
                        newVertex.Position.X = (x + vertex.Position.X) * TileScale;
                        newVertex.Position.Y = vertex.Position.Y * TileHeight;
                        newVertex.Position.Z = (z + vertex.Position.Z) * TileScale;
                        newVertex.Color.R = (byte)(vertex.Color.R * WallShade);
                        newVertex.Color.G = (byte)(vertex.Color.G * WallShade);
                        newVertex.Color.B = (byte)(vertex.Color.B * WallShade);
                        _vertices.Add(newVertex);
                    }
                }
            }
        }
        
        // Mesh floor tiles, which are only visible from the top.
        for (var z = 0; z < _size; ++z)
        {
            for (var x = 0; x < _size; ++x)
            {
                var tile = GetFloorTile(x, z);

                if (tile == Tile.Air) continue;

                var sideIndices = CubeMesh.Indices[Direction.Up];
                var baseVertexCount = _vertices.Count;
                foreach (var index in sideIndices)
                {
                    _indices.Add((ushort)(index + baseVertexCount));
                }

                var sideVertices = CubeMesh.Vertices[Direction.Up];
                foreach (var vertex in sideVertices)
                {
                    var newVertex = vertex;
                    newVertex.Position.X = (x + vertex.Position.X) * TileScale;
                    newVertex.Position.Y = 0f;
                    newVertex.Position.Z = (z + vertex.Position.Z) * TileScale;
                    newVertex.Color.R = (byte)(vertex.Color.R * FloorShade);
                    newVertex.Color.G = (byte)(vertex.Color.G * FloorShade);
                    newVertex.Color.B = (byte)(vertex.Color.B * FloorShade);
                    _vertices.Add(newVertex);
                }
            }
        }

        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Count,
            BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), _indices.Count, BufferUsage.WriteOnly);

        _vertexBuffer.SetData(_vertices.ToArray());
        _indexBuffer.SetData(_indices.ToArray());

        _primitives = _indices.Count / 3;
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _vertexBuffer is null || _indexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }
}