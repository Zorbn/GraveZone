using System;
using System.Collections.Generic;
using Common;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class Player
{
    private const float Speed = 2f;
    private static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    private Vector3 _position;
    public Vector3 Position => _position;

    private Weapon _weapon;

    public Player(float x, float z)
    {
        _position = new Vector3(x, 0f, z);
        _weapon = new Weapon(0.2f);
    }

    private void Move(Vector3 movement, Map map, Vector3 cameraForward, Vector3 cameraRight, float deltaTime)
    {
        if (movement.Length() == 0f) return;

        movement = movement.Z * cameraForward + movement.X * cameraRight;
        movement.Normalize();

        var newPosition = _position;
        newPosition.X += movement.X * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.X = _position.X;
        }

        _position.X = newPosition.X;

        newPosition.Z += movement.Z * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.Z = _position.Z;
        }

        _position.Z = newPosition.Z;
    }

    // TODO: Make camera its own class which stores the forward/right vectors, etc.
    public void Update(KeyboardState keyboardState, MouseState mouseState, Map map, List<Projectile> projectiles, Vector3 cameraForward,
        Vector3 cameraRight, AlphaTestEffect cameraEffect, float viewportWidth, float viewportHeight, float deltaTime)
    {
        var movement = Vector3.Zero;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            movement.Z += 1f;
        }

        if (keyboardState.IsKeyDown(Keys.S))
        {
            movement.Z -= 1f;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            movement.X -= 1f;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            movement.X += 1f;
        }

        Move(movement, map, cameraForward, cameraRight, deltaTime);

        // Compute the player's position on the screen:
        var viewPosition = new Vector4(_position, 1f);
        var worldViewProjectionMatrix = cameraEffect.World * cameraEffect.View * cameraEffect.Projection;
        viewPosition = Vector4.Transform(viewPosition, worldViewProjectionMatrix);
        viewPosition /= viewPosition.W;
        viewPosition.X = (viewPosition.X + 1) * viewportWidth * 0.5f;
        viewPosition.Y = (viewPosition.Y + 1) * viewportHeight * 0.5f;
        
        var mouseX = mouseState.X;
        var mouseY = mouseState.Y;

        var directionToMouse = new Vector3(mouseX - viewPosition.X, 0f, mouseY - viewPosition.Y);
        directionToMouse = -directionToMouse.Z * cameraForward + directionToMouse.X * cameraRight;
        
        _weapon.Update(deltaTime);

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            _weapon.Attack(directionToMouse, _position.X, _position.Z, projectiles);
        }
    }

    public void Tick(bool isLocal, int localId, NetPacketProcessor netPacketProcessor, NetDataWriter writer, NetManager client, float deltaTime)
    {
        if (isLocal)
        {
            writer.Reset();
            netPacketProcessor.Write(writer, new PlayerMove { Id = localId, X = _position.X, Z = _position.Z });
            client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public void SetInterpolationTarget(Vector3 target)
    {
        _position = target;
    }

    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(_position.X, _position.Z);
    }
}