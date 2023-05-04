using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class Camera
{
    private const float ViewScale = 15;
    private static readonly Vector3 CameraLookOffset = new(1f, -3f, 1f);

    public EffectPassCollection Passes => _effect.CurrentTechnique.Passes;
    public Matrix SpriteMatrix { get; private set; }
    public Matrix WorldMatrix => _effect.World;
    public Matrix ViewMatrix => _effect.View;
    public Matrix ProjectionMatrix => _effect.Projection;
    
    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    private AlphaTestEffect _effect;
    private Vector3 _position = Vector3.Zero;
    private float _angle;

    public Camera(GraphicsDevice graphicsDevice)
    {
        _effect = new AlphaTestEffect(graphicsDevice);

        _effect.World = Matrix.Identity;

        _effect.VertexColorEnabled = true;
        
        Resize(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
    }

    public void SetTexture(Texture2D texture)
    {
        _effect.Texture = texture;
    }

    public void SetPosition(Vector3 position)
    {
        _position = position;
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        
        _effect.Projection = CalculateProjectionMatrix(width, height);
    }

    public void Rotate(float delta)
    {
        _angle += delta;
    }
    
    public void ResetAngle()
    {
        _angle = 0f;
    }

    private Matrix CalculateProjectionMatrix(int width, int height)
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

        _effect.View = Matrix.CreateLookAt(_position, _position + lookOffset, Vector3.Up);
    }
}