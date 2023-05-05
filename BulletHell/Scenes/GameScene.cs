using System;
using System.Collections.Generic;
using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell.Scenes;

public class GameScene : IScene
{
    private const float TickTime = 0.05f;
    private static readonly Matrix UiMatrix = Matrix.CreateScale(4f);

    private BulletHell _game;
    
    private Map _map;
    private SpriteRenderer _spriteRenderer;
    private Dictionary<int, Player> _players;
    private List<Projectile> _projectiles;
    private float _tickTimer;
    private Camera _camera;
    
    private Client _client;
    
    public GameScene(BulletHell game)
    {
        _game = game;
        
        _camera = new Camera(game.GraphicsDevice);

        _map = new Map(Common.Map.Size);

        _spriteRenderer = new SpriteRenderer(500, game.GraphicsDevice);

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
    }

    ~GameScene()
    {
        
    }
    
    public void Update(KeyboardState keyboardState, MouseState mouseState, float deltaTime)
    {
        _client.PollEvents();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            _game.Exit();

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
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.CornflowerBlue);
        
        // Set graphics state to be suitable for 3D models.
        // Using the sprite batch modifies these to different values.
        _game.GraphicsDevice.BlendState = BlendState.Opaque;
        _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        _game.GraphicsDevice.RasterizerState = new RasterizerState { MultiSampleAntiAlias = true };
        _game.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        
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

        _camera.SetTexture(_game.Resources.MapTexture);
        foreach (var currentPass in _camera.Passes)
        {
            currentPass.Apply();
            _map.Draw(_game.GraphicsDevice);
        }

        _camera.SetTexture(_game.Resources.SpriteTexture);
        foreach (var currentPass in _camera.Passes)
        {
            currentPass.Apply();
            _spriteRenderer.Draw(_game.GraphicsDevice);
        }
        
        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: UiMatrix);
        _game.SpriteBatch.Draw(_game.Resources.UiTexture, new Vector2(0f, 0f), Color.White);
        _game.SpriteBatch.End();
    }

    public void Resize(int width, int height)
    {
        _camera.Resize(width, height);
    }
    
    private void UpdateLocal(Player localPlayer)
    {
        _camera.SetPosition(localPlayer.Position);
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
        _map.Mesh(_game.GraphicsDevice);
    }
}