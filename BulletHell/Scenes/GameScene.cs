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

    public readonly Client Client;

    private readonly BulletHell _game;

    private readonly Map _map;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly Dictionary<int, Player> _players;
    private readonly List<Projectile> _projectiles;
    private readonly Camera _camera;

    private readonly ImageButton _quitButton;

    private float _tickTimer;

    public GameScene(BulletHell game)
    {
        _game = game;

        _camera = new Camera(game.GraphicsDevice);
        _map = new Map(Common.Map.Size);
        _spriteRenderer = new SpriteRenderer(500, game.GraphicsDevice);
        _players = new Dictionary<int, Player>();
        _projectiles = new List<Projectile>();

        _quitButton = new ImageButton(Inventory.X - Resources.TileSize,
            Inventory.Y + Resources.TileSize * 3, ImageButton.QuitRectangle);

        Client = new Client();

        Client.NetPacketProcessor.RegisterNestedType<NetVector3>();
        Client.NetPacketProcessor.SubscribeReusable<SetLocalId, NetPeer>(OnSetLocalId);
        Client.NetPacketProcessor.SubscribeReusable<PlayerSpawn, NetPeer>(OnPlayerSpawn);
        Client.NetPacketProcessor.SubscribeReusable<PlayerDespawn, NetPeer>(OnPlayerDespawn);
        Client.NetPacketProcessor.SubscribeReusable<PlayerMove, NetPeer>(OnPlayerMove);
        Client.NetPacketProcessor.SubscribeReusable<ProjectileSpawn, NetPeer>(OnProjectileSpawn);
        Client.NetPacketProcessor.SubscribeReusable<MapGenerate>(OnMapGenerate);

        Console.WriteLine("Starting client...");
    }

    public void Exit()
    {
        Console.WriteLine("Disconnecting...");
        Client.Stop();
    }

    public void Update(Input input, float deltaTime)
    {
        Client.PollEvents();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            input.IsKeyDown(Keys.Escape))
            _game.Exit();

        var cameraAngleMovement = 0f;

        if (input.IsKeyDown(Keys.Q))
        {
            cameraAngleMovement += 1f;
        }

        if (input.IsKeyDown(Keys.E))
        {
            cameraAngleMovement -= 1f;
        }

        _camera.Rotate(cameraAngleMovement * deltaTime);

        if (input.IsKeyDown(Keys.Z))
        {
            _camera.ResetAngle();
        }

        foreach (var (_, player) in _players)
        {
            player.Update(input, _map, _projectiles, Client, _camera, deltaTime);
        }

        if (_players.TryGetValue(Client.LocalId, out var localPlayer))
        {
            UpdateLocal(input, localPlayer);
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

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.UiMatrix);

        if (_players.TryGetValue(Client.LocalId, out var localPlayer))
        {
            DrawLocal(localPlayer);
        }

        _quitButton.Draw(_game.SpriteBatch, _game.Resources);
        _game.SpriteBatch.End();
    }

    public void Resize(int width, int height)
    {
        _camera.Resize(width, height);
    }

    private void UpdateLocal(Input input, Player localPlayer)
    {
        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;
            if (_quitButton.Contains(mouseX, mouseY))
            {
                _game.SetScene(new MainMenuScene(_game));
            }
        }

        _camera.SetPosition(localPlayer.Position);
    }

    private void DrawLocal(Player localPlayer)
    {
        localPlayer.Inventory.Draw(_game.Resources, _game.SpriteBatch);
    }

    private void Tick(float deltaTime)
    {
        _tickTimer -= deltaTime;

        if (_tickTimer > 0f) return;

        _tickTimer = TickTime;

        foreach (var (_, player) in _players)
        {
            player.Tick(Client, TickTime);
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
        if (Client.IsLocal(playerMove.Id)) return;
        if (!_players.TryGetValue(playerMove.Id, out var player)) return;

        var newPosition = player.Position;
        newPosition.X = playerMove.X;
        newPosition.Z = playerMove.Z;
        player.Position = newPosition;
    }

    private void OnSetLocalId(SetLocalId setLocalId, NetPeer peer)
    {
        Console.WriteLine($"Updating local ID: {setLocalId.Id}");
        Client.LocalId = setLocalId.Id;
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