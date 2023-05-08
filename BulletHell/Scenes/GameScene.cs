﻿using System;
using System.Collections.Generic;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell.Scenes;

public class GameScene : IScene
{
    private const float TickTime = 0.05f;

    public readonly Client Client;

    private readonly BulletHell _game;

    private readonly ClientMap _map;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly Dictionary<int, ClientPlayer> _players;
    private readonly Camera _camera;

    private readonly ImageButton _quitButton;

    private float _tickTimer;

    public GameScene(BulletHell game)
    {
        _game = game;

        _camera = new Camera(game.GraphicsDevice);
        _map = new ClientMap();
        _spriteRenderer = new SpriteRenderer(500, game.GraphicsDevice);
        _players = new Dictionary<int, ClientPlayer>();

        _quitButton = new ImageButton(ClientInventory.X - Resources.TileSize,
            ClientInventory.Y + Resources.TileSize * 3, ImageButton.QuitRectangle);

        Client = new Client();

        Client.NetPacketProcessor.RegisterNestedType<NetVector3>();
        Client.NetPacketProcessor.SubscribeNetSerializable<SetLocalId>(OnSetLocalId);
        Client.NetPacketProcessor.SubscribeNetSerializable<PlayerSpawn>(OnPlayerSpawn);
        Client.NetPacketProcessor.SubscribeNetSerializable<PlayerDespawn>(OnPlayerDespawn);
        Client.NetPacketProcessor.SubscribeNetSerializable<PlayerMove>(OnPlayerMove);
        Client.NetPacketProcessor.SubscribeNetSerializable<ProjectileSpawn>(OnProjectileSpawn);
        Client.NetPacketProcessor.SubscribeNetSerializable<MapGenerate>(OnMapGenerate);
        Client.NetPacketProcessor.SubscribeNetSerializable<DroppedWeaponSpawn>(OnDroppedWeaponSpawn);
        Client.NetPacketProcessor.SubscribeNetSerializable<PickupWeapon>(OnPickupWeapon);
        Client.NetPacketProcessor.SubscribeNetSerializable<GrabSlot>(OnGrabSlot);
        Client.NetPacketProcessor.SubscribeNetSerializable<GrabEquippedSlot>(OnGrabEquippedSlot);
        Client.NetPacketProcessor.SubscribeNetSerializable<DropGrabbed>(OnDropGrabbed);
        Client.NetPacketProcessor.SubscribeReusable<UpdateInventory>(OnUpdateInventory);
        Client.NetPacketProcessor.SubscribeNetSerializable<EnemySpawn>(OnEnemySpawn);
        Client.NetPacketProcessor.SubscribeNetSerializable<EnemyTakeDamage>(OnEnemyTakeDamage);
        Client.NetPacketProcessor.SubscribeNetSerializable<EnemyMove>(OnEnemyMove);

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

        var hasLocalPlayer = _players.TryGetValue(Client.LocalId, out var localPlayer);
        if (hasLocalPlayer)
        {
            var uiCapturedMouse = PreUpdateLocal(input, localPlayer);
            
            // If the mouse was interacting with the ui that needs to be recorded so that mouse
            // clicks don't have side effects anywhere else (ie: player clicks on the inventory
            // and accidentally attacks at the same time).
            if (uiCapturedMouse)
            {
                input.UiCapturedMouse();
            }
        }

        foreach (var (_, player) in _players)
        {
            player.Update(input, _map, Client, _camera, deltaTime);
        }

        if (hasLocalPlayer)
        {
            PostUpdateLocal(localPlayer);
        }
        
        _map.Update(deltaTime);

        _tickTimer += deltaTime;

        while (_tickTimer > TickTime)
        {
            _tickTimer -= TickTime;
            Tick();
        }
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

        // TODO: Move projectiles into map and have a map.DrawSprites method.
        foreach (var projectile in _map.Projectiles)
        {
            projectile.Draw(_spriteRenderer);
        }
        
        foreach (var (_, droppedWeapon) in _map.DroppedWeapons)
        {
            droppedWeapon.Draw(_spriteRenderer);
        }
        
        foreach (var (_, enemy) in _map.Enemies)
        {
            enemy.Draw(_spriteRenderer);
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
        
        _quitButton.Draw(_game.SpriteBatch, _game.Resources);

        if (_players.TryGetValue(Client.LocalId, out var localPlayer))
        {
            DrawLocal(localPlayer);
        }

        _game.SpriteBatch.End();
    }

    public void Resize(int width, int height)
    {
        _camera.Resize(width, height);
    }

    private bool PreUpdateLocal(Input input, ClientPlayer localPlayer)
    {
        var mousePosition = _game.GetMouseUiPosition();
        var mouseX = (int)mousePosition.X;
        var mouseY = (int)mousePosition.Y;
            
        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            if (_quitButton.Contains(mouseX, mouseY))
            {
                _game.SetScene(new MainMenuScene(_game));
            }
        }
        
        var inventoryCapturedMouse = localPlayer.Inventory.Update(Client, input, mousePosition);

        return inventoryCapturedMouse;
    }
    
    private void PostUpdateLocal(ClientPlayer localPlayer)
    {
        _camera.SetPosition(localPlayer.Position);
    }

    private void DrawLocal(ClientPlayer localPlayer)
    {
        localPlayer.Inventory.Draw(_game.Resources, _game.SpriteBatch);
    }

    private void Tick()
    {
        foreach (var (_, player) in _players)
        {
            player.Tick(Client, TickTime);
        }
    }

    private void OnPlayerSpawn(PlayerSpawn playerSpawn)
    {
        Console.WriteLine($"Player spawned with id: {playerSpawn.Id}");
        _players.Add(playerSpawn.Id, new ClientPlayer(playerSpawn.Id, playerSpawn.X, playerSpawn.Z));
    }

    private void OnPlayerDespawn(PlayerDespawn playerDespawn)
    {
        Console.WriteLine($"Player de-spawned with id: {playerDespawn.Id}");
        _players.Remove(playerDespawn.Id);
    }

    private void OnPlayerMove(PlayerMove playerMove)
    {
        if (Client.IsLocal(playerMove.Id)) return;
        if (!_players.TryGetValue(playerMove.Id, out var player)) return;

        var newPosition = player.Position;
        newPosition.X = playerMove.X;
        newPosition.Z = playerMove.Z;
        player.Position = newPosition;
    }

    private void OnSetLocalId(SetLocalId setLocalId)
    {
        Console.WriteLine($"Updating local ID: {setLocalId.Id}");
        Client.LocalId = setLocalId.Id;
    }

    private void OnProjectileSpawn(ProjectileSpawn projectileSpawn)
    {
        var direction = projectileSpawn.Direction.ToVector3();
        _map.Projectiles.Add(new Projectile(direction, projectileSpawn.X, projectileSpawn.Z));
    }

    private void OnMapGenerate(MapGenerate mapGenerate)
    {
        _map.Generate(mapGenerate.Seed);
        _map.Mesh(_game.GraphicsDevice);
    }
    
    private void OnDroppedWeaponSpawn(DroppedWeaponSpawn droppedWeaponSpawn)
    {
        _map.DropWeapon(droppedWeaponSpawn.WeaponType, droppedWeaponSpawn.X, droppedWeaponSpawn.Z, droppedWeaponSpawn.Id);
    }

    private void OnPickupWeapon(PickupWeapon pickupWeapon)
    {
        if (!_players.TryGetValue(pickupWeapon.PlayerId, out var player)) return;
        
        _map.PickupWeapon(pickupWeapon.DroppedWeaponId);
        player.Inventory.AddWeapon(pickupWeapon.WeaponType);
    }

    private void OnGrabSlot(GrabSlot grabSlot)
    {
        if (!_players.TryGetValue(grabSlot.PlayerId, out var player)) return;
        
        player.Inventory.GrabSlot(grabSlot.SlotIndex);
    }
    
    private void OnGrabEquippedSlot(GrabEquippedSlot grabEquippedSlot)
    {
        if (!_players.TryGetValue(grabEquippedSlot.PlayerId, out var player)) return;
        
        player.Inventory.GrabEquippedSlot();
    }
    
    private void OnDropGrabbed(DropGrabbed dropGrabbed)
    {
        if (!_players.TryGetValue(dropGrabbed.PlayerId, out var player)) return;
        
        player.Inventory.DropGrabbed(_map, dropGrabbed.X, dropGrabbed.Z, dropGrabbed.DroppedWeaponId);
    }

    private void OnUpdateInventory(UpdateInventory updateInventory)
    {
        if (!_players.TryGetValue(updateInventory.PlayerId, out var player)) return;

        player.Inventory.UpdateInventory(updateInventory);
    }
    
    private void OnEnemySpawn(EnemySpawn enemySpawn)
    {
        _map.SpawnEnemy(enemySpawn.X, enemySpawn.Z, enemySpawn.Id);
    }
    
    private void OnEnemyTakeDamage(EnemyTakeDamage enemyTakeDamage)
    {
        if (!_map.Enemies.TryGetValue(enemyTakeDamage.Id, out var enemy)) return;

        var enemyDied = enemy.TakeDamage(enemyTakeDamage.Damage);
        
        if (enemyDied)
        {
            _map.DespawnEnemy(enemy.Id);
        }
    }
    
    private void OnEnemyMove(EnemyMove enemyMove)
    {
        if (!_map.Enemies.TryGetValue(enemyMove.Id, out var enemy)) return;

        enemy.MoveTo(_map, enemyMove.X, enemyMove.Z);
    }
}