using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class ClientPlayer : Player
{
    public readonly ClientInventory ClientInventory;

    private Vector3 _spritePosition;

    private readonly Attacker _attacker;

    public ClientPlayer(Attacker attacker, Map map, int id, float x, float z) : base(map, id, x, z)
    {
        _spritePosition = Position;
        _attacker = attacker;
        ClientInventory = new ClientInventory(Inventory);
    }

    private void Move(Vector3 movement, ClientMap map, Vector3 cameraForward, Vector3 cameraRight, float deltaTime)
    {
        if (movement.Length() == 0f) return;

        movement = movement.Z * cameraForward + movement.X * cameraRight;
        movement.Normalize();

        var newPosition = Position;
        newPosition.X += movement.X * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.X = Position.X;
        }
        
        newPosition.Z += movement.Z * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.Z = Position.Z;
        }

        MoveTo(map, newPosition);

        _spritePosition = Position;
    }

    private void UpdateLocal(Input input, ClientMap map, Client client, Camera camera,
        float deltaTime)
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

        if (input.WasKeyPressed(Keys.F))
        {
            foreach (var nearbyDroppedWeapon in map.DroppedWeaponsInTiles.GetNearby(Position.X, Position.Z))
            {
                var isColliding =
                    Collision.HasCollision(Position, Size, nearbyDroppedWeapon.Position, Weapon.Size);

                if (!isColliding) continue;

                if (!Inventory.IsFull())
                {
                    map.RequestPickupWeapon(client, nearbyDroppedWeapon.Id);
                }
            }
        }

        Move(movement, map, camera.Forward, camera.Right, deltaTime);

        // Compute the player's position on the screen:
        var viewPosition = new Vector4(Position, 1f);
        var worldViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        viewPosition = Vector4.Transform(viewPosition, worldViewProjectionMatrix);
        viewPosition /= viewPosition.W;
        viewPosition.X = (viewPosition.X + 1) * camera.Width * 0.5f;
        viewPosition.Y = (viewPosition.Y + 1) * camera.Height * 0.5f;

        var directionToMouse = new Vector3(input.MouseX - viewPosition.X, 0f, input.MouseY - viewPosition.Y);
        directionToMouse = -directionToMouse.Z * camera.Forward + directionToMouse.X * camera.Right;

        _attacker.Update(deltaTime);

        if (input.IsMouseButtonDown(MouseButton.Left) && Inventory.EquippedWeaponStats is not null)
        {
            _attacker.Attack(Inventory.EquippedWeaponStats, directionToMouse, Position.X, Position.Z, map);
        }
    }

    public void Update(Input input, ClientMap map, Client client, Camera camera,
        float deltaTime)
    {
        if (client.IsLocal(Id))
        {  
            UpdateLocal(input, map, client, camera, deltaTime);
            return;
        }

        _spritePosition = Vector3.Lerp(_spritePosition, Position, SpriteInfo.SpriteLerp * deltaTime);
    }

    public void Tick(Client client, float deltaTime)
    {
        if (client.IsLocal(Id))
        {
            client.SendToServer(new PlayerMove { Id = Id, X = Position.X, Z = Position.Z },
                DeliveryMethod.Unreliable);
        }
    }

    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(_spritePosition.X, _spritePosition.Z, Sprite.Player);
    }
}