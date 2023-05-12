using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class SpriteRenderer
{
    private const int ShadowResolution = 10;
    private const int ShadowRadius = 30;
    private const int ShadowDiameter = 2 * ShadowRadius;

    private readonly VertexBuffer _spriteVertexBuffer;

    private readonly IndexBuffer _spriteIndexBuffer;
    private readonly VertexBuffer _shadowVertexBuffer;
    private readonly IndexBuffer _shadowIndexBuffer;

    private readonly VertexPositionColorTexture[] _spriteVertices;

    private readonly ushort[] _spriteIndices;

    private static readonly VertexPositionColorTexture[] ShadowMapVertices =
    {
        new(new Vector3(0, 0.01f, 0), Color.White, new Vector2(0, 0)),
        new(new Vector3(0, 0.01f, ShadowDiameter), Color.White, new Vector2(0, 1)),
        new(new Vector3(ShadowDiameter, 0.01f, ShadowDiameter), Color.White, new Vector2(1, 1)),
        new(new Vector3(ShadowDiameter, 0.01f, 0), Color.White, new Vector2(1, 0))
    };

    private static readonly ushort[] ShadowMapIndices = { 0, 2, 1, 0, 3, 2 };

    private readonly Vector2[] _shadowPositions;
    private readonly VertexPositionColorTexture[] _shadowMapVertices;

    private Matrix _rotationMatrix;

    private int _spriteVertexI;

    private int _spriteIndexI;

    private int _primitives;
    private int _sprites;
    private readonly int _maxSprites;

    public readonly RenderTarget2D ShadowTarget;

    public SpriteRenderer(int maxSprites, GraphicsDevice graphicsDevice)
    {
        var maxVertices = maxSprites * 4;
        var maxIndices = maxSprites * 6;

        _maxSprites = maxSprites;
        _spriteVertices = new VertexPositionColorTexture[maxVertices];
        _spriteIndices = new ushort[maxIndices];
        _shadowPositions = new Vector2[maxSprites];

        _spriteVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), maxVertices,
            BufferUsage.WriteOnly);
        _spriteIndexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), maxIndices, BufferUsage.WriteOnly);

        _shadowMapVertices = new VertexPositionColorTexture[ShadowMapVertices.Length];
        _shadowVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture),
            ShadowMapVertices.Length,
            BufferUsage.WriteOnly);
        _shadowIndexBuffer =
            new IndexBuffer(graphicsDevice, typeof(ushort), ShadowMapIndices.Length, BufferUsage.WriteOnly);
        _shadowIndexBuffer.SetData(ShadowMapIndices, 0, ShadowMapIndices.Length);

        const int targetSize = ShadowResolution * Resources.TileSize * ShadowDiameter;
        ShadowTarget = new RenderTarget2D(graphicsDevice, targetSize, targetSize, false,
            graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
    }

    public void Add(float x, float z, Sprite sprite)
    {
        if (_sprites >= _maxSprites) return;

        _shadowPositions[_sprites] = new Vector2(x - 0.5f, z - 0.5f);
        AddSprite(x, z, sprite);

        ++_sprites;
        _primitives += 2;
    }

    private void AddSprite(float x, float z, Sprite sprite)
    {
        var baseVertexCount = _spriteVertexI;
        foreach (var index in SpriteMesh.Indices)
        {
            _spriteIndices[_spriteIndexI] = (ushort)(index + baseVertexCount);
            ++_spriteIndexI;
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
            _spriteVertices[_spriteVertexI] = newVertex;
            ++_spriteVertexI;
        }
    }

    public void End()
    {
        if (_spriteVertexI < 1 || _spriteIndexI < 1) return;

        _spriteVertexBuffer.SetData(_spriteVertices, 0, _spriteVertexI);
        _spriteIndexBuffer.SetData(_spriteIndices, 0, _spriteIndexI);
    }

    public void Begin(Matrix rotationMatrix)
    {
        _spriteVertexI = 0;
        _spriteIndexI = 0;
        _primitives = 0;
        _sprites = 0;
        _rotationMatrix = rotationMatrix;
    }

    public void DrawSprites(GraphicsDevice graphicsDevice)
    {
        if (_primitives == 0 || _spriteVertexBuffer is null || _spriteIndexBuffer is null) return;

        graphicsDevice.SetVertexBuffer(_spriteVertexBuffer);
        graphicsDevice.Indices = _spriteIndexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
    }

    public void DrawShadowsToTexture(Vector3 viewPosition, GraphicsDevice graphicsDevice, Resources resources, SpriteBatch spriteBatch)
    {
        graphicsDevice.SetRenderTarget(ShadowTarget);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        const int positionMultiplier = ShadowResolution * Resources.TileSize;
        var scale = new Vector2(ShadowResolution);
        var offset = new Vector2(viewPosition.X - ShadowRadius, viewPosition.Z - ShadowRadius);

        for (var i = 0; i < _sprites; i++)
        {
            var position =  (_shadowPositions[i] - offset) * positionMultiplier;
            spriteBatch.Draw(resources.SpriteTexture, position, Resources.ShadowRectangle, Color.White, 0f,
                Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();

        graphicsDevice.SetRenderTarget(null);
    }

    public void DrawShadows(Vector3 viewPosition, GraphicsDevice graphicsDevice)
    {
        var offset = new Vector3(viewPosition.X - ShadowRadius, 0f, viewPosition.Z - ShadowRadius);

        
        for (var i = 0; i < ShadowMapVertices.Length; i++)
        {
            _shadowMapVertices[i] = ShadowMapVertices[i];
            _shadowMapVertices[i].Position += offset;
        }
        
        _shadowVertexBuffer.SetData(_shadowMapVertices, 0, ShadowMapVertices.Length);
        
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.SetVertexBuffer(_shadowVertexBuffer);
        graphicsDevice.Indices = _shadowIndexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        graphicsDevice.BlendState = BlendState.Opaque;
    }
}