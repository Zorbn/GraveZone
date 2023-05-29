using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class ClientPlayer : Player
{
    private const int HealthBarWidth = Inventory.Width * ClientInventory.SlotSize;
    private const int HealthBarHeight = Resources.TileSize / 2;

    private static readonly Point RelativeHealthBarPosition =
        ClientInventory.RelativePosition - new Point(0, HealthBarHeight);

    private const int HealthRegenAmount = 5;
    private const float HealthRegenTime = 2f;
    private const float HealthRegenStartTime = 5f;

    private const float LerpAnimationThreshold = 0.05f;

    private static readonly Sprite[] Sprites =
        { Sprite.PlayerIdle, Sprite.PlayerStepLeft, Sprite.PlayerIdle, Sprite.PlayerStepRight };

    public bool IsMoving { get; private set; }

    private readonly ClientInventory _clientInventory;
    private readonly Attacker _attacker;
    private float _healthRegenTimer;
    private float _healthRegenStartTimer;

    public ClientPlayer(Attacker attacker, Map map, int id, float x, float z, int health = MaxHealth) : base(map, id, x,
        z, health)
    {
        _attacker = attacker;
        _clientInventory = new ClientInventory(Inventory);
    }

    private void Move(Vector3 movement, ClientMap map, Vector3 cameraForward, Vector3 cameraRight, float deltaTime)
    {
        if (movement.Length() == 0f) return;

        IsMoving = true;

        movement = movement.Z * cameraForward + movement.X * cameraRight;
        movement.Normalize();

        var currentSpeed = Speed * deltaTime;
        if (Inventory.EquippedWeaponStats is not null) currentSpeed *= Inventory.EquippedWeaponStats.SpeedMultiplier;

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

        if (!wasAbleToRegen) _healthRegenTimer = 0f;

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
        IsMoving = false;

        if (ShouldUpdateLocally(client))
        {
            UpdateLocal(input, map, client, camera, deltaTime);
            return;
        }

        if (Vector3.Distance(SpritePosition, Position) > LerpAnimationThreshold) IsMoving = true;
        SpritePosition = Vector3.Lerp(SpritePosition, Position, SpriteInfo.SpriteLerp * deltaTime);
    }

    public bool UpdateInventory(BulletHell game, Client client, Camera camera, Input input, Vector2 mousePosition)
    {
        return _clientInventory.Update(game, client, camera, input, mousePosition);
    }

    public void Tick(Client client)
    {
        if (!ShouldUpdateLocally(client)) return;

        if (IsMoving)
            client.SendToServer(new PlayerMove { Id = Id, X = Position.X, Z = Position.Z },
                DeliveryMethod.Unreliable);
    }

    public void AddSprite(SpriteRenderer spriteRenderer, int animationFrame)
    {
        if (!IsMoving)
        {
            spriteRenderer.Add(SpritePosition, Sprites[0]);
            return;
        }

        var spriteI = animationFrame % Sprites.Length;
        spriteRenderer.Add(SpritePosition, Sprites[spriteI]);
    }

    public void DrawHud(BulletHell game)
    {
        var healthBarPosition = game.Ui.AnchorPoint(RelativeHealthBarPosition, UiAnchor.Bottom);
        var healthBarDestination =
            new Rectangle(healthBarPosition.X, healthBarPosition.Y, HealthBarWidth, HealthBarHeight);
        game.SpriteBatch.Draw(game.Resources.UiTexture, healthBarDestination, Resources.BlackRectangle, Color.White);
        var currentHealthBarWidth = (int)(HealthBarWidth * (Health / (float)MaxHealth));
        healthBarDestination.Width = currentHealthBarWidth;
        game.SpriteBatch.Draw(game.Resources.UiTexture, healthBarDestination, Resources.WhiteRectangle, Color.Red);

        _clientInventory.Draw(game);
    }
}