using System;
using System.Collections.Generic;
using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class BulletHell : Game
{
    private const float TickTime = 0.05f;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _mapTexture;
    private Texture2D _spriteTexture;
    private Texture2D _uiTexture;

    private Map _map;
    private SpriteRenderer _spriteRenderer;
    private Dictionary<int, Player> _players;
    private List<Projectile> _projectiles;
    private float _tickTimer;
    private Camera _camera;
    
    private Client _client;
    
    private static readonly Matrix UiMatrix = Matrix.CreateScale(4f);
    
    public BulletHell()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferMultiSampling = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
        Content.RootDirectory = "Content";
        InactiveSleepTime = TimeSpan.Zero;
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
        _camera.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
    }
    
    protected override void Initialize()
    {
        _mapTexture = Texture2D.FromFile(GraphicsDevice, "Content/tiles.png");
        _spriteTexture = Texture2D.FromFile(GraphicsDevice, "Content/sprites.png");
        _uiTexture = Texture2D.FromFile(GraphicsDevice, "Content/ui.png");

        _camera = new Camera(GraphicsDevice);

        _map = new Map(Common.Map.Size);

        _spriteRenderer = new SpriteRenderer(500, GraphicsDevice);

        _players = new Dictionary<int, Player>();

        _projectiles = new List<Projectile>();

        _client = new Client();
        
        _client.NetPacketProcessor.RegisterNestedType<NetVector3>();
        _client.NetPacketProcessor.SubscribeReusable<SetLocalId, NetPeer>(OnSetLocalId);
        _client.NetPacketProcessor.SubscribeReusable<PlayerSpawn, NetPeer>(OnPlayerSpawn);
        _client.NetPacketProcessor.SubscribeReusable<PlayerDespawn, NetPeer>(OnPlayerDespawn);
        _client.NetPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        _client.NetPacketProcessor.SubscribeReusable<ProjectileSpawn, NetPeer>(OnProjectileSpawn);
        _client.NetPacketProcessor.SubscribeReusable<MapGenerate>(OnMapGenerate);
        
        // TODO: Make a menu for joining a server before the game starts.
        // TODO: Close client with UI and when game is closed.
        _client.Connect("localhost");
        
        Console.WriteLine("Starting client...");
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    private void UpdateLocal(Player localPlayer)
    {
        _camera.SetPosition(localPlayer.Position);
    }

    protected override void Update(GameTime gameTime)
    {
        _client.PollEvents();
        
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var keyboardState = Keyboard.GetState();
        // Don't allow mouse inputs when the window isn't focused.
        var mouseState = IsActive ? Mouse.GetState() : new MouseState();
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        var cameraAngleMovement = 0f;

        if (keyboardState.IsKeyDown(Keys.Q))
        {
            cameraAngleMovement += 1f;
        }

        if (keyboardState.IsKeyDown(Keys.E))
        {
            cameraAngleMovement -= 1f;
        }

        _camera.Rotate(cameraAngleMovement * deltaTime);

        if (keyboardState.IsKeyDown(Keys.Z))
        {
            _camera.ResetAngle();
        }

        foreach (var (_, player) in _players)
        {
            player.Update(keyboardState, mouseState, _map, _projectiles, _client, _camera, deltaTime);
        }

        if (_players.TryGetValue(_client.LocalId, out var localPlayer))
        {
            UpdateLocal(localPlayer);
        }
        
        for (var i = _projectiles.Count - 1; i >= 0; i--)
        {
            var hadCollision = _projectiles[i].Update(_map, deltaTime);
            
            if (hadCollision)
            {
                _projectiles.RemoveAt(i);
            }
        }
        
        Tick(deltaTime);
        
        base.Update(gameTime);
    }

    private void Tick(float deltaTime)
    {
        _tickTimer -= deltaTime;

        if (_tickTimer > 0f) return;

        _tickTimer = TickTime;
        
        foreach (var (_, player) in _players)
        {
            player.Tick(_client, TickTime);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Set graphics state to be suitable for 3D models.
        // Using the sprite batch modifies these to different values.
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        
        _camera.UpdateViewMatrices();

        _spriteRenderer.Begin(_camera.SpriteMatrix);
        foreach (var pair in _players)
        {
            pair.Value.Draw(_spriteRenderer);
        }
        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteRenderer);
        }
        _spriteRenderer.End();

        _camera.SetTexture(_mapTexture);
        foreach (var currentPass in _camera.Passes)
        {
            currentPass.Apply();
            _map.Draw(GraphicsDevice);
        }

        _camera.SetTexture(_spriteTexture);
        foreach (var currentPass in _camera.Passes)
        {
            currentPass.Apply();
            _spriteRenderer.Draw(GraphicsDevice);
        }
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: UiMatrix);
        _spriteBatch.Draw(_uiTexture, new Vector2(0f, 0f), Color.White);
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    private void OnPlayerSpawn(PlayerSpawn playerSpawn, NetPeer peer)
    {
        Console.WriteLine($"Player spawned with id: {playerSpawn.Id}");
        _players.Add(playerSpawn.Id, new Player(playerSpawn.Id, playerSpawn.X, playerSpawn.Z));
    }
    
    private void OnPlayerDespawn(PlayerDespawn playerDespawn, NetPeer peer)
    {
        Console.WriteLine($"Player de-spawned with id: {playerDespawn.Id}");
        _players.Remove(playerDespawn.Id);
    }
    
    private void OnPlayerMove(PlayerMove playerMove, NetPeer peer)
    {
        if (_client.IsLocal(playerMove.Id)) return;
        if (!_players.TryGetValue(playerMove.Id, out var player)) return;
        
        var newPosition = player.Position;
        newPosition.X = playerMove.X;
        newPosition.Z = playerMove.Z;
        player.Position = newPosition;        
    }
    
    private void OnSetLocalId(SetLocalId setLocalId, NetPeer peer)
    {
        Console.WriteLine($"Updating local ID: {setLocalId.Id}");
        _client.LocalId = setLocalId.Id;
    }

    private void OnProjectileSpawn(ProjectileSpawn projectileSpawn, NetPeer netPeer)
    {
        var direction = new Vector3(projectileSpawn.Direction.X, projectileSpawn.Direction.Y,
            projectileSpawn.Direction.Z);
        _projectiles.Add(new Projectile(direction, projectileSpawn.X, projectileSpawn.Z));
    }
    
    private void OnMapGenerate(MapGenerate mapGenerate)
    {
        _map.Generate(mapGenerate.Seed);
        _map.Mesh(GraphicsDevice);
    }
}