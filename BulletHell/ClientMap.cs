using System;
using Common;
using LiteNetLib;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class ClientMap : Map
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private readonly ArrayList<VertexPositionColorTexture> _vertices;
    private readonly ArrayList<ushort> _indices;
    private int _primitives;

    public ClientMap()
    {
        _vertices = new ArrayList<VertexPositionColorTexture>(10240);
        _indices = new ArrayList<ushort>(10240);
    }

    public void Mesh(GraphicsDevice graphicsDevice)
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        _vertices.Clear();
        _indices.Clear();

        // Mesh the walls, including all of their visible sides.
        for (var z = 0; z < Size; ++z)
        for (var x = 0; x < Size; ++x)
        {
            var tile = GetWallTile(x, z);

            if (tile == Tile.Air) continue;

            for (var i = 0; i < 6; i++)
            {
                var direction = (Direction)i;

                var sideIndices = CubeMesh.Indices[direction];
                var baseVertexCount = _vertices.Count;
                foreach (var index in sideIndices) _indices.Add((ushort)(index + baseVertexCount));

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

        // Mesh floor tiles, which are only visible from the top.
        for (var z = 0; z < Size; ++z)
        for (var x = 0; x < Size; ++x)
        {
            var tile = GetFloorTile(x, z);

            if (tile == Tile.Air) continue;

            var sideIndices = CubeMesh.Indices[Direction.Up];
            var baseVertexCount = _vertices.Count;
            foreach (var index in sideIndices) _indices.Add((ushort)(index + baseVertexCount));

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

        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), _vertices.Count,
            BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), _indices.Count, BufferUsage.WriteOnly);

        if (_vertices.Count == 0 || _indices.Count == 0) return;

        _vertexBuffer.SetData(_vertices.Array, 0, _vertices.Count);
        _indexBuffer.SetData(_indices.Array, 0, _indices.Count);

        _primitives = _indices.Count / 3;
    }

    public void RequestPickupWeapon(Client client, int id)
    {
        client.SendToServer(new RequestPickupWeapon { DroppedWeaponId = id }, DeliveryMethod.ReliableOrdered);
    }

    public void UpdateClient(float deltaTime)
    {
        foreach (var (_, enemy) in Enemies) enemy.UpdateClient(deltaTime);
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _vertexBuffer is null || _indexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }

    public void DrawSprites(SpriteRenderer spriteRenderer, int animationFrame)
    {
        foreach (var decorationSprite in DecorationSprites)
            spriteRenderer.Add(decorationSprite.X, decorationSprite.Z, decorationSprite.Sprite);

        foreach (var projectile in Projectiles) projectile.Draw(spriteRenderer);

        foreach (var (_, droppedWeapon) in DroppedWeapons) droppedWeapon.Draw(spriteRenderer);

        foreach (var (_, enemy) in Enemies) enemy.Draw(spriteRenderer, animationFrame);
    }
}