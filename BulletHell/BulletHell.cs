using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class BulletHell : Game
{
    private const float ViewScale = 15;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private AlphaTestEffect _alphaEffect;

    private Texture2D _mapTexture;
    private Texture2D _spriteTexture;

    private Map _map;
    private SpriteRenderer _spriteRenderer;
    private Player _player;
    private List<Projectile> _projectiles;

    private Vector3 _cameraPosition = Vector3.Zero;
    private Vector3 _cameraForward;
    private Vector3 _cameraRight;
    private Matrix _cameraSpriteMatrix;
    private float _cameraAngle;
    private static readonly Vector3 CameraLookOffset = new(1f, -3f, 1f);

    public BulletHell()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferMultiSampling = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;
    }

    private static void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
    {
        args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
    }

    private void OnResize(object sender, EventArgs eventArgs)
    {
        _alphaEffect.Projection = CalculateProjectionMatrix();
    }

    private Matrix CalculateProjectionMatrix()
    {
        return Matrix.CreateOrthographic(
            GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height * ViewScale, ViewScale, -20f, 20f);
    }

    private void UpdateViewMatrices()
    {
        var cameraRotation = Matrix.CreateRotationY(_cameraAngle);
        var lookOffset = Vector3.Transform(CameraLookOffset, cameraRotation);
        _cameraForward = lookOffset;
        _cameraForward.Y = 0f;
        _cameraRight = Vector3.Transform(_cameraForward, Matrix.CreateRotationY(MathF.PI * -0.5f));

        var spriteLookOffset = lookOffset;
        spriteLookOffset.Y = 0f;
        _cameraSpriteMatrix = Matrix.Invert(Matrix.CreateLookAt(Vector3.Zero, spriteLookOffset, Vector3.Up));

        _alphaEffect.View = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + lookOffset, Vector3.Up);
    }

    protected override void Initialize()
    {
        _mapTexture = Texture2D.FromFile(GraphicsDevice, "Content/tiles.png");
        _spriteTexture = Texture2D.FromFile(GraphicsDevice, "Content/sprites.png");

        _alphaEffect = new AlphaTestEffect(GraphicsDevice);

        _alphaEffect.World = Matrix.Identity;
        _alphaEffect.Projection = CalculateProjectionMatrix();

        _alphaEffect.Texture = _mapTexture;
        _alphaEffect.VertexColorEnabled = true;

        _map = new Map(16, GraphicsDevice);
        _map.Mesh(GraphicsDevice);

        _spriteRenderer = new SpriteRenderer(500, GraphicsDevice);

        _player = new Player(-1, -1);

        _projectiles = new List<Projectile>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        var cameraAngleMovement = 0f;

        if (keyboardState.IsKeyDown(Keys.Q))
        {
            cameraAngleMovement -= 1f;
        }

        if (keyboardState.IsKeyDown(Keys.E))
        {
            cameraAngleMovement += 1f;
        }

        _cameraAngle += cameraAngleMovement * deltaTime;

        if (keyboardState.IsKeyDown(Keys.Z))
        {
            _cameraAngle = 0f;
        }

        _player.Update(keyboardState, mouseState, _map, _projectiles, _cameraForward, _cameraRight, _alphaEffect,
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, deltaTime);
        _cameraPosition = _player.Position;
        
        for (var i = _projectiles.Count - 1; i >= 0; i--)
        {
            var hadCollision = _projectiles[i].Update(_map, deltaTime);
            
            if (hadCollision)
            {
                _projectiles.RemoveAt(i);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        _spriteBatch.Begin();
        _spriteBatch.Draw(_spriteTexture, new Vector2(0f, 0f), Color.White);
        _spriteBatch.End();
        
        // Set graphics state to be suitable for 3D models.
        // Using the sprite batch modifies these to different values.
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        UpdateViewMatrices();

        _spriteRenderer.Begin(_cameraSpriteMatrix);
        _spriteRenderer.Add(0, 0);
        _spriteRenderer.Add(1, 0);
        _spriteRenderer.Add(0, 1);
        _player.Draw(_spriteRenderer);
        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteRenderer);
        }
        _spriteRenderer.End();

        _alphaEffect.World = Matrix.Identity;

        _alphaEffect.Texture = _mapTexture;
        foreach (var currentPass in _alphaEffect.CurrentTechnique.Passes)
        {
            currentPass.Apply();
            _map.Draw(GraphicsDevice);
        }

        _alphaEffect.Texture = _spriteTexture;
        foreach (var currentPass in _alphaEffect.CurrentTechnique.Passes)
        {
            currentPass.Apply();
            _spriteRenderer.Draw(GraphicsDevice);
        }
        
        base.Draw(gameTime);
    }
}