using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class BulletHell : Game
{
    private const float ViewScale = 15;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private BasicEffect _basicEffect;

    private Map _map;
    private SpriteRenderer _spriteRenderer;
    private Player _player;
    
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
        _basicEffect.Projection = CalculateProjectionMatrix();
    }

    private Matrix CalculateProjectionMatrix()
    {
        return Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height * ViewScale, ViewScale, -20f, 20f);
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
        
        _basicEffect.View = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + lookOffset, Vector3.Up);
    }

    protected override void Initialize()
    {
        GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        
        _basicEffect = new BasicEffect(GraphicsDevice);
        
        _basicEffect.World = Matrix.Identity;
        _basicEffect.Projection = CalculateProjectionMatrix();

        _basicEffect.Texture = Texture2D.FromFile(GraphicsDevice, "Content/tiles.png");
        _basicEffect.TextureEnabled = true;
        _basicEffect.VertexColorEnabled = true;

        _map = new Map(16, GraphicsDevice);
        _map.Mesh(GraphicsDevice);
        
        _spriteRenderer = new SpriteRenderer(500, GraphicsDevice);

        _player = new Player(0, 0);
        
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
        
        _player.Update(keyboardState, _map, _cameraForward, _cameraRight, deltaTime);
        _cameraPosition = _player.Position;
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        UpdateViewMatrices();
        
        _spriteRenderer.Begin(_cameraSpriteMatrix);
        _spriteRenderer.Add(0, 0);
        _spriteRenderer.Add(1, 0);
        _spriteRenderer.Add(0, 1);
        _player.Draw(_spriteRenderer);
        _spriteRenderer.End();

        _basicEffect.World = Matrix.Identity;
        foreach (var currentPass in _basicEffect.CurrentTechnique.Passes)
        {
            currentPass.Apply();
            _map.Draw(GraphicsDevice);
            _spriteRenderer.Draw(GraphicsDevice);            
        }
        
        base.Draw(gameTime);
    }
}