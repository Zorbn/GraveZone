using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class SpriteRenderer
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private VertexPositionColorTexture[] _vertices;
    private ushort[] _indices;
    private int _vertexI;
    private int _indexI;
    private int _primitives;
    private readonly int _maxSprites;

    public SpriteRenderer(int maxSprites, GraphicsDevice graphicsDevice)
    {
        var maxVertices = maxSprites * 4;
        var maxIndices = maxSprites * 6;
        
        _maxSprites = maxSprites;
        _vertices = new VertexPositionColorTexture[maxVertices];
        _indices = new ushort[maxIndices];
        
        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), maxVertices,
            BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), maxIndices, BufferUsage.WriteOnly);
    }

    public void Add(float x, float z, Matrix rotationMatrix)
    {
        var baseVertexCount = _vertexI;
        foreach (var index in SpriteMesh.Indices)
        {
            _indices[_indexI] = (ushort)(index + baseVertexCount);
            ++_indexI;
        }

        foreach (var vertex in SpriteMesh.Vertices)
        {
            var newVertex = vertex;
            newVertex.Position = Vector3.Transform(vertex.Position, rotationMatrix);
            newVertex.Position.X = newVertex.Position.X * 0.8f + x;
            newVertex.Position.Y *= 1.1f;
            newVertex.Position.Z = newVertex.Position.Z * 0.8f + z;
            _vertices[_vertexI] = newVertex;
            ++_vertexI;
        }
        
        _primitives += 2;
    }

    public void Finish()
    {
        _vertexBuffer.SetData(_vertices, 0, _vertexI);
        _indexBuffer.SetData(_indices, 0, _indexI);
    }

    public void Reset()
    {
        _vertexI = 0;
        _indexI = 0;
        _primitives = 0;
    }
    
    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _vertexBuffer is null || _indexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }
}