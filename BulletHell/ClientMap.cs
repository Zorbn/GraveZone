using System.Collections.Generic;
using Common;
using LiteNetLib;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class ClientMap : Map
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private List<VertexPositionColorTexture> _vertices;
    private List<ushort> _indices;
    private int _primitives;

    public ClientMap()
    {
        _vertices = new List<VertexPositionColorTexture>();
        _indices = new List<ushort>();
    }

    public void Mesh(GraphicsDevice graphicsDevice)
    {
        if (_vertexBuffer is not null) _vertexBuffer.Dispose();
        if (_indexBuffer is not null) _indexBuffer.Dispose();

        _vertices.Clear();
        _indices.Clear();

        // Mesh the walls, including all of their visible sides.
        for (var z = 0; z < Size; ++z)
        {
            for (var x = 0; x < Size; ++x)
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
                        var texCoord = CubeMesh.GetTexCoord(tile);
                        newVertex.TextureCoordinate.X += texCoord.X;
                        newVertex.TextureCoordinate.Y += texCoord.Y;
                        _vertices.Add(newVertex);
                    }
                }
            }
        }
        
        // Mesh floor tiles, which are only visible from the top.
        for (var z = 0; z < Size; ++z)
        {
            for (var x = 0; x < Size; ++x)
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
                    var texCoord = CubeMesh.GetTexCoord(tile);
                    newVertex.TextureCoordinate.X += texCoord.X;
                    newVertex.TextureCoordinate.Y += texCoord.Y;
                    _vertices.Add(newVertex);
                }
            }
        }

        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Count,
            BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), _indices.Count, BufferUsage.WriteOnly);

        if (_vertices.Count == 0 || _indices.Count == 0) return;
        
        _vertexBuffer.SetData(_vertices.ToArray());
        _indexBuffer.SetData(_indices.ToArray());
        
        _primitives = _indices.Count / 3;
    }

    public void RequestPickupWeapon(Client client, int id)
    {
        client.SendToServer(new RequestPickupWeapon { DroppedWeaponId = id }, DeliveryMethod.ReliableOrdered);
    }
    
    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _vertexBuffer is null || _indexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }
}