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
    private const float SpriteLerp = 10f;
    private static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    private Vector3 _position;
    private Vector3 _spritePosition;

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    public readonly int Id;

    private Weapon _weapon;

    public Player(int id, float x, float z)
    {
        Id = id;

        _position = new Vector3(x, 0f, z);
        _spritePosition = _position;
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

        _spritePosition = _position;
    }

    // TODO: Create class to send packets and store network info.
    private void UpdateLocal(Input input, Map map, List<Projectile> projectiles, Client client, Camera camera, float deltaTime)
    {
        var movement = Vector3.Zero;

        if (input.IsKeyDown(Keys.W))
        {
            movement.Z += 1f;
        }

        if (input.IsKeyDown(Keys.S))
        {
            movement.Z -= 1f;
        }

        if (input.IsKeyDown(Keys.A))
        {
            movement.X -= 1f;
        }

        if (input.IsKeyDown(Keys.D))
        {
            movement.X += 1f;
        }

        Move(movement, map, camera.Forward, camera.Right, deltaTime);

        // Compute the player's position on the screen:
        var viewPosition = new Vector4(_position, 1f);
        var worldViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        viewPosition = Vector4.Transform(viewPosition, worldViewProjectionMatrix);
        viewPosition /= viewPosition.W;
        viewPosition.X = (viewPosition.X + 1) * camera.Width * 0.5f;
        viewPosition.Y = (viewPosition.Y + 1) * camera.Height * 0.5f;

        var directionToMouse = new Vector3(input.MouseX - viewPosition.X, 0f, input.MouseY - viewPosition.Y);
        directionToMouse = -directionToMouse.Z * camera.Forward + directionToMouse.X * camera.Right;

        _weapon.Update(deltaTime);

        if (input.IsMouseButtonDown(MouseButton.Left))
        {
            _weapon.Attack(directionToMouse, _position.X, _position.Z, projectiles, client);
        }
    }

    public void Update(Input input, Map map, List<Projectile> projectiles, Client client, Camera camera, float deltaTime)
    {
        if (client.IsLocal(Id))
        {
            UpdateLocal(input, map, projectiles, client, camera, deltaTime);
            return;
        }

        _spritePosition = Vector3.Lerp(_spritePosition, _position, SpriteLerp * deltaTime);
    }

    public void Tick(Client client, float deltaTime)
    {
        if (client.IsLocal(Id))
        {
            client.SendToServer(new PlayerMove { Id = Id, X = _position.X, Z = _position.Z },
                DeliveryMethod.Unreliable);
        }
    }

    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(_spritePosition.X, _spritePosition.Z);
    }
}