using System;
using System.Collections.Generic;
using System.Data;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class BulletHell : Game
{
    private const float ViewScale = 15;
    private const float TickTime = 0.05f;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _mapTexture;
    private Texture2D _spriteTexture;
    private Texture2D _uiTexture;

    private Map _map;
    private SpriteRenderer _spriteRenderer;
    private Dictionary<int, Player> _players;
    private int _localId = -1;
    private List<Projectile> _projectiles;
    private float _tickTimer;

    private AlphaTestEffect _cameraEffect;
    private Vector3 _cameraPosition = Vector3.Zero;
    private Vector3 _cameraForward;
    private Vector3 _cameraRight;
    private Matrix _cameraSpriteMatrix;
    private float _cameraAngle;
    private static readonly Vector3 CameraLookOffset = new(1f, -3f, 1f);
    
    private static readonly Matrix UiMatrix = Matrix.CreateScale(4f);

    private NetPacketProcessor _netPacketProcessor;
    private EventBasedNetListener _listener;
    private NetManager _client;
    private NetDataWriter _writer;

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
        _cameraEffect.Projection = CalculateProjectionMatrix();
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

        _cameraEffect.View = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + lookOffset, Vector3.Up);
    }

    protected override void Initialize()
    {
        _mapTexture = Texture2D.FromFile(GraphicsDevice, "Content/tiles.png");
        _spriteTexture = Texture2D.FromFile(GraphicsDevice, "Content/sprites.png");
        _uiTexture = Texture2D.FromFile(GraphicsDevice, "Content/ui.png");

        _cameraEffect = new AlphaTestEffect(GraphicsDevice);

        _cameraEffect.World = Matrix.Identity;
        _cameraEffect.Projection = CalculateProjectionMatrix();

        _cameraEffect.Texture = _mapTexture;
        _cameraEffect.VertexColorEnabled = true;

        _map = new Map(16, GraphicsDevice);
        _map.Mesh(GraphicsDevice);

        _spriteRenderer = new SpriteRenderer(500, GraphicsDevice);

        _players = new Dictionary<int, Player>();

        _projectiles = new List<Projectile>();
        
        _writer = new NetDataWriter();
        
        // TODO: Make a menu for joining a server before the game starts.
        _netPacketProcessor = new NetPacketProcessor();
        _netPacketProcessor.SubscribeReusable<SetLocalId, NetPeer>(OnSetLocalId);
        _netPacketProcessor.SubscribeReusable<PlayerSpawn, NetPeer>(OnPlayerSpawn);
        _netPacketProcessor.SubscribeReusable<PlayerDespawn, NetPeer>(OnPlayerDespawn);
        _netPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
        // TODO: Close client with UI and when game is closed.
        _client.Start();
        _client.Connect("localhost", Networking.Port, "");
        
        Console.WriteLine($"Starting client...");
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) =>
        {
            _netPacketProcessor.ReadAllPackets(dataReader, fromPeer);
        };
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    private void UpdateLocal(Player localPlayer)
    {
        _cameraPosition = localPlayer.Position;
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

        _cameraAngle += cameraAngleMovement * deltaTime;

        if (keyboardState.IsKeyDown(Keys.Z))
        {
            _cameraAngle = 0f;
        }

        foreach (var (playerId, player) in _players)
        {
            player.Update(playerId == _localId, keyboardState, mouseState, _map, _projectiles, _cameraForward,
                _cameraRight, _cameraEffect, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, deltaTime);
        }

        if (_players.TryGetValue(_localId, out var localPlayer))
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
        
        foreach (var (playerId, player) in _players)
        {
            player.Tick(playerId == _localId, _localId, _netPacketProcessor, _writer, _client, TickTime);
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
        
        UpdateViewMatrices();

        _spriteRenderer.Begin(_cameraSpriteMatrix);
        foreach (var pair in _players)
        {
            pair.Value.Draw(_spriteRenderer);
        }
        foreach (var projectile in _projectiles)
        {
            projectile.Draw(_spriteRenderer);
        }
        _spriteRenderer.End();

        _cameraEffect.World = Matrix.Identity;

        _cameraEffect.Texture = _mapTexture;
        foreach (var currentPass in _cameraEffect.CurrentTechnique.Passes)
        {
            currentPass.Apply();
            _map.Draw(GraphicsDevice);
        }

        _cameraEffect.Texture = _spriteTexture;
        foreach (var currentPass in _cameraEffect.CurrentTechnique.Passes)
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
        _players.Add(playerSpawn.Id, new Player(playerSpawn.X, playerSpawn.Z));
    }
    
    private void OnPlayerDespawn(PlayerDespawn playerDespawn, NetPeer peer)
    {
        Console.WriteLine($"Player de-spawned with id: {playerDespawn.Id}");
        _players.Remove(playerDespawn.Id);
    }
    
    private void OnPlayerMove(PlayerMove playerMove, NetPeer peer)
    {
        if (playerMove.Id == _localId) return;
        if (!_players.TryGetValue(playerMove.Id, out var player)) return;
        
        var newPosition = player.Position;
        newPosition.X = playerMove.X;
        newPosition.Z = playerMove.Z;
        player.Position = newPosition;        
    }
    
    private void OnSetLocalId(SetLocalId setLocalId, NetPeer peer)
    {
        Console.WriteLine($"Updating local ID: {setLocalId.Id}");
        _localId = setLocalId.Id;
    }
}