using System;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraveZone;

public class SpriteRenderer
{
    private struct SpriteShadow
    {
        public Vector2 Position;
        public SpriteSize Size;
    }

    private const int ShadowResolution = 10;
    private const int ShadowRadius = 17;
    private const int ShadowDiameter = 2 * ShadowRadius;

    public readonly RenderTarget2D ShadowTarget;
    public readonly Effect ScreenEffect;

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

    private readonly SpriteShadow[] _spriteShadows;
    private readonly VertexPositionColorTexture[] _shadowMapVertices;

    private Matrix _rotationMatrix;

    private int _spriteVertexI;

    private int _spriteIndexI;

    private int _primitives;
    private int _sprites;
    private readonly int _maxSprites;

    private RenderTarget2D? _spriteTarget;
    private readonly VertexBuffer _screenVertexBuffer;
    private readonly IndexBuffer _screenIndexBuffer;

    public SpriteRenderer(int maxSprites, GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        var maxVertices = maxSprites * 4;
        var maxIndices = maxSprites * 6;

        _maxSprites = maxSprites;
        _spriteVertices = new VertexPositionColorTexture[maxVertices];
        _spriteIndices = new ushort[maxIndices];
        _spriteShadows = new SpriteShadow[maxSprites];

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

        ScreenEffect = contentManager.Load<Effect>("ScreenEffect");
        Resize(graphicsDevice);

        _screenVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture),
            ScreenMesh.Vertices.Length,
            BufferUsage.WriteOnly);
        _screenIndexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), ScreenMesh.Indices.Length,
            BufferUsage.WriteOnly);
        _screenVertexBuffer.SetData(ScreenMesh.Vertices, 0, ScreenMesh.Vertices.Length);
        _screenIndexBuffer.SetData(ScreenMesh.Indices, 0, ScreenMesh.Indices.Length);
    }

    public void Resize(GraphicsDevice graphicsDevice)
    {
        var width = graphicsDevice.Viewport.Width;
        var height = graphicsDevice.Viewport.Height;

        _spriteTarget = new RenderTarget2D(graphicsDevice, width, height, false,
            graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        ScreenEffect.Parameters["ModelTexture"].SetValue(_spriteTarget);
        ScreenEffect.Parameters["OutlineWidth"].SetValue(new Vector2(1f / width, 1f / height));
    }

    public void Add(Vector3 position, Sprite sprite, SpriteSize size = SpriteSize.Medium)
    {
        if (_sprites >= _maxSprites) return;

        var scale = size switch
        {
            SpriteSize.Large => 2f,
            SpriteSize.Medium => 1f,
            SpriteSize.Small => 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };

        _spriteShadows[_sprites] = new SpriteShadow
        {
            Position = new Vector2(position.X - scale * 0.5f, position.Z - scale * 0.5f),
            Size = size
        };
        AddSprite(position, sprite, scale);

        ++_sprites;
        _primitives += 2;
    }

    private void AddSprite(Vector3 position, Sprite sprite, float scale)
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
            newVertex.Position.X *= scale * 0.8f;
            newVertex.Position.Y *= scale * 1.75f;
            newVertex.Position.Z *= scale * 0.8f;
            newVertex.Position += position;
            newVertex.TextureCoordinate *= scale;
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

    public void DrawSpritesToTexture(GraphicsDevice graphicsDevice, Resources resources, Camera camera, ClientMap map)
    {
        if (_primitives == 0) return;

        graphicsDevice.SetRenderTarget(_spriteTarget);

        graphicsDevice.Clear(Color.Transparent);
        graphicsDevice.ClearState();

        camera.Texture = resources.MapTexture;
        foreach (var currentPass in camera.Passes)
        {
            currentPass.Apply();
            map.Draw(graphicsDevice);
        }

        graphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0f, 0);

        camera.Texture = resources.SpriteTexture;
        foreach (var currentPass in camera.Passes)
        {
            currentPass.Apply();
            graphicsDevice.SetVertexBuffer(_spriteVertexBuffer);
            graphicsDevice.Indices = _spriteIndexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitives);
        }

        graphicsDevice.SetRenderTarget(null);
    }

    public void DrawSprites(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.SetVertexBuffer(_screenVertexBuffer);
        graphicsDevice.Indices = _screenIndexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        graphicsDevice.BlendState = BlendState.Opaque;
    }

    public void DrawShadowsToTexture(Vector3 viewPosition, GraphicsDevice graphicsDevice, Resources resources,
        SpriteBatch spriteBatch)
    {
        graphicsDevice.SetRenderTarget(ShadowTarget);
        graphicsDevice.Clear(Color.Transparent);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        const int positionMultiplier = ShadowResolution * Resources.TileSize;
        var scale = new Vector2(ShadowResolution);
        var offset = new Vector2(viewPosition.X - ShadowRadius, viewPosition.Z - ShadowRadius);

        for (var i = 0; i < _sprites; i++)
        {
            var position = (_spriteShadows[i].Position - offset) * positionMultiplier;
            var shadowRectangle = _spriteShadows[i].Size switch
            {
                SpriteSize.Large => Resources.ShadowLargeRectangle,
                SpriteSize.Medium => Resources.ShadowMediumRectangle,
                SpriteSize.Small => Resources.ShadowSmallRectangle,
                _ => throw new ArgumentOutOfRangeException()
            };

            spriteBatch.Draw(resources.SpriteTexture, position, shadowRectangle, Color.White, 0f,
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