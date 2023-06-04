using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GraveZone;

public class Camera
{
    private const float ViewScale = 15;
    private static readonly Vector3 CameraLookOffset = new(1f, -3f, 1f);
    private const int EffectAlphaUpdated = 0b000001;
    private const int TextureUpdated = 0b000010;
    private const int WorldMatrixUpdated = 0b000100;
    private const int ViewMatrixUpdated = 0b001000;
    private const int ProjectionMatrixUpdated = 0b010000;

    public EffectPassCollection Passes
    {
        get
        {
            if ((_updateMask & EffectAlphaUpdated) > 0)
                _effect.Parameters["Alpha"].SetValue(EffectAlpha);
            if ((_updateMask & TextureUpdated) > 0)
                _effect.Parameters["ModelTexture"].SetValue(Texture);
            if ((_updateMask & WorldMatrixUpdated) > 0)
                _effect.Parameters["World"].SetValue(WorldMatrix);
            if ((_updateMask & ViewMatrixUpdated) > 0)
                _effect.Parameters["View"].SetValue(ViewMatrix);
            if ((_updateMask & ProjectionMatrixUpdated) > 0)
                _effect.Parameters["Projection"].SetValue(ProjectionMatrix);

            _updateMask = 0;

            return _effect.CurrentTechnique.Passes;
        }
    }

    public Matrix SpriteMatrix { get; private set; }

    public Matrix WorldMatrix
    {
        get => _worldMatrix;
        set
        {
            _updateMask |= WorldMatrixUpdated;
            _worldMatrix = value;
        }
    }

    public Matrix ViewMatrix
    {
        get => _viewMatrix;
        set
        {
            _updateMask |= ViewMatrixUpdated;
            _viewMatrix = value;
        }
    }

    public Matrix ProjectionMatrix
    {
        get => _projectionMatrix;
        set
        {
            _updateMask |= ProjectionMatrixUpdated;
            _projectionMatrix = value;
        }
    }

    public float EffectAlpha
    {
        get => _effectAlpha;
        set
        {
            _updateMask |= EffectAlphaUpdated;
            _effectAlpha = value;
        }
    }

    public Texture2D? Texture
    {
        get => _texture;
        set
        {
            _updateMask |= TextureUpdated;
            _texture = value;
        }
    }

    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly Effect _effect;
    public Vector3 Position { get; set; }

    private int _updateMask = int.MaxValue;
    private float _angle;
    private Matrix _worldMatrix;
    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;
    private float _effectAlpha;
    private Texture2D? _texture;

    public Camera(GraphicsDevice graphicsDevice, ContentManager contentManager)
    {
        _effect = contentManager.Load<Effect>("WorldEffect");

        WorldMatrix = Matrix.Identity;

        Resize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;

        ProjectionMatrix = CalculateProjectionMatrix(width, height);
    }

    public void Rotate(float delta)
    {
        _angle += delta;
    }

    public void ResetAngle()
    {
        _angle = 0f;
    }

    private static Matrix CalculateProjectionMatrix(int width, int height)
    {
        return Matrix.CreateOrthographic(
            width / (float)height * ViewScale, ViewScale, -20f, 20f);
    }

    public void UpdateViewMatrices()
    {
        var cameraRotation = Matrix.CreateRotationY(_angle);
        var lookOffset = Vector3.Transform(CameraLookOffset, cameraRotation);
        var flattenedLookOffset = lookOffset;
        flattenedLookOffset.Y = 0f;

        Forward = flattenedLookOffset;
        Right = Vector3.Transform(Forward, Matrix.CreateRotationY(MathF.PI * -0.5f));

        SpriteMatrix = Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, flattenedLookOffset, Vector3.Up));

        ViewMatrix = Matrix.CreateLookAt(Position, Position + lookOffset, Vector3.Up);
    }
}