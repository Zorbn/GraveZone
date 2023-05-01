using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class SpriteRenderer
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private List<VertexPositionColorTexture> _vertices;
    private List<ushort> _indices;
    private int _primitives;

    public SpriteRenderer()
    {
        _vertices = new List<VertexPositionColorTexture>();
        _indices = new List<ushort>();
    }

    public void Mesh(GraphicsDevice graphicsDevice, Matrix rotationMatrix, Vector3 cameraPosition)
    {
        if (_vertexBuffer is not null) _vertexBuffer.Dispose();
        if (_indexBuffer is not null) _indexBuffer.Dispose();

        _vertices.Clear();
        _indices.Clear();

        var sideIndices = CubeMesh.Indices[Direction.Forward];
        var baseVertexCount = _vertices.Count;
        foreach (var index in sideIndices)
        {
            _indices.Add((ushort)(index + baseVertexCount));
        }

        var sideVertices = CubeMesh.Vertices[Direction.Forward];
        foreach (var vertex in sideVertices)
        {
            var newVertex = vertex;
            newVertex.Position = Vector3.Transform(vertex.Position, rotationMatrix);
            newVertex.Position.X *= 0.8f;
            newVertex.Position.Y *= 1.1f;
            newVertex.Position.Z *= 0.8f;
            _vertices.Add(newVertex);
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