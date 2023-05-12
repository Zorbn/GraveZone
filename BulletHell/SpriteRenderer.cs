using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class SpriteRenderer
{
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private VertexPositionColorTexture[] _vertices;
    private Matrix _rotationMatrix;
    private ushort[] _indices;
    private int _vertexI;
    private int _indexI;
    private int _primitives;
    private int _sprites;
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

    public void Add(float x, float z, Sprite sprite)
    {
        if (_sprites >= _maxSprites) return;
        ++_sprites;
        
        var baseVertexCount = _vertexI;
        foreach (var index in SpriteMesh.Indices)
        {
            _indices[_indexI] = (ushort)(index + baseVertexCount);
            ++_indexI;
        }

        var texCoords = SpriteMesh.GetTexCoord(sprite);

        foreach (var vertex in SpriteMesh.Vertices)
        {
            var newVertex = vertex;
            newVertex.Position = Vector3.Transform(vertex.Position, _rotationMatrix);
            newVertex.Position.X = newVertex.Position.X * 0.8f + x;
            newVertex.Position.Y *= 1.75f;
            newVertex.Position.Z = newVertex.Position.Z * 0.8f + z;
            newVertex.TextureCoordinate += new Vector2(texCoords.X, texCoords.Y);
            _vertices[_vertexI] = newVertex;
            ++_vertexI;
        }
        
        _primitives += 2;
    }

    public void End()
    {
        if (_vertexI < 1 || _indexI < 1) return;
        
        _vertexBuffer.SetData(_vertices, 0, _vertexI);
        _indexBuffer.SetData(_indices, 0, _indexI);
    }

    public void Begin(Matrix rotationMatrix)
    {
        _vertexI = 0;
        _indexI = 0;
        _primitives = 0;
        _sprites = 0;
        _rotationMatrix = rotationMatrix;
    }
    
    public void Draw(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _vertexBuffer is null || _indexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }
}