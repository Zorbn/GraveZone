using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class ClientPlayer : Player
{
    private const int HealthBarWidth = Inventory.Width * ClientInventory.SlotSize;
    private const int HealthBarHeight = Resources.TileSize / 2;
    private const int HealthBarX = ClientInventory.X;
    private const int HealthBarY = ClientInventory.Y - HealthBarHeight;

    private const int HealthRegenAmount = 5;
    private const float HealthRegenTime = 2f;

    private const float HealthRegenStartTime = 5f;

    public readonly ClientInventory ClientInventory;

    private readonly Attacker _attacker;
    private float _healthRegenTimer;
    private float _healthRegenStartTimer;

    public ClientPlayer(Attacker attacker, Map map, int id, float x, float z, int? health) : base(map, id, x, z, health)
    {
        _attacker = attacker;
        ClientInventory = new ClientInventory(Inventory);
    }

    private void Move(Vector3 movement, ClientMap map, Vector3 cameraForward, Vector3 cameraRight, float deltaTime)
    {
        if (movement.Length() == 0f) return;

        movement = movement.Z * cameraForward + movement.X * cameraRight;
        movement.Normalize();

        var currentSpeed = Speed * deltaTime;
        if (Inventory.EquippedWeaponStats is not null)
        {
            currentSpeed *= Inventory.EquippedWeaponStats.SpeedMultiplier;
        }

        var newPosition = Position;
        newPosition.X += movement.X * currentSpeed;

        if (map.IsCollidingWithBox(newPosition, Size)) newPosition.X = Position.X;

        newPosition.Z += movement.Z * currentSpeed;

        if (map.IsCollidingWithBox(newPosition, Size)) newPosition.Z = Position.Z;

        Teleport(map, newPosition);
    }

    private void UpdateLocal(Input input, ClientMap map, Client client, Camera camera,
        float deltaTime)
    {
        var movement = Vector3.Zero;

        if (input.IsKeyDown(Keys.W)) movement.Z += 1f;

        if (input.IsKeyDown(Keys.S)) movement.Z -= 1f;

        if (input.IsKeyDown(Keys.A)) movement.X -= 1f;

        if (input.IsKeyDown(Keys.D)) movement.X += 1f;

        if (input.WasKeyPressed(Keys.F))
            foreach (var nearbyDroppedWeapon in map.DroppedWeaponsInTiles.GetNearby(Position.X, Position.Z))
            {
                var isColliding =
                    Collision.HasCollision(Position, Size, nearbyDroppedWeapon.Position, Weapon.Size);

                if (!isColliding) continue;

                if (!Inventory.IsFull()) map.RequestPickupWeapon(client, nearbyDroppedWeapon.Id);
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
            _attacker.Attack(Inventory.EquippedWeaponStats, directionToMouse, Position.X, Position.Z, map);

        _healthRegenTimer += deltaTime;

        var wasAbleToRegen = _healthRegenStartTimer >= HealthRegenStartTime;
        
        _healthRegenStartTimer += deltaTime;

        // Health regen is calculated on the client to maintain consistency with
        // the damage calculations that are also done on the client, and prevent
        // a player's health de-syncing across the clients/server.
        if (_healthRegenStartTimer < HealthRegenStartTime) return;

        if (!wasAbleToRegen)
        {
            _healthRegenTimer = 0f;
        }
        
        while (_healthRegenTimer > HealthRegenTime)
        {
            _healthRegenTimer -= HealthRegenTime;
            ClientHeal(HealthRegenAmount, client);
        }
    }

    public override bool TakeDamage(int damage)
    {
        _healthRegenStartTimer = 0f;
        return base.TakeDamage(damage);
    }

    private void ClientHeal(int amount, Client client)
    {
        Heal(amount);

        client.SendToServer(new PlayerHeal
        {
            Amount = amount
        }, DeliveryMethod.ReliableOrdered);
    }

    private bool ShouldUpdateLocally(Client client)
    {
        return client.IsLocal(Id) && !IsDead;
    }

    public void Update(Input input, ClientMap map, Client client, Camera camera,
        float deltaTime)
    {
        if (ShouldUpdateLocally(client))
        {
            UpdateLocal(input, map, client, camera, deltaTime);
            return;
        }

        SpritePosition = Vector3.Lerp(SpritePosition, Position, SpriteInfo.SpriteLerp * deltaTime);
    }

    public void Tick(Client client, float deltaTime)
    {
        if (ShouldUpdateLocally(client))
            client.SendToServer(new PlayerMove { Id = Id, X = Position.X, Z = Position.Z },
                DeliveryMethod.Unreliable);
    }

    public void AddSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(SpritePosition, Sprite.Player);
    }

    public void DrawHud(Resources resources, SpriteBatch spriteBatch)
    {
        var healthBarDestination = new Rectangle(HealthBarX, HealthBarY, HealthBarWidth, HealthBarHeight);
        spriteBatch.Draw(resources.UiTexture, healthBarDestination, Resources.BlackRectangle, Color.White);
        var currentHealthBarWidth = (int)(HealthBarWidth * (Health / (float)MaxHealth));
        healthBarDestination.Width = currentHealthBarWidth;
        spriteBatch.Draw(resources.UiTexture, healthBarDestination, Resources.WhiteRectangle, Color.Red);
        
        ClientInventory.Draw(resources, spriteBatch);
    }
}