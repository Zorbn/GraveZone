using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class ClientInventory
{
    public const int SlotSize = 2 * Resources.TileSize;
    private const UiAnchor Anchor = UiAnchor.Bottom;
    private const int ItemSpriteOffset = Resources.TileSize / 2;
    private const int GrabbedItemScale = 2;

    public static readonly Point RelativePosition =
        new(Ui.CenterX - Inventory.Width * SlotSize / 2, -SlotSize * Inventory.Height);

    private static readonly Point RelativeTooltipPosition =
        RelativePosition + new Point((Inventory.Width - 1) * SlotSize / 2, -SlotSize * 5 / 4);

    private static readonly Point RelativeEquippedPosition =
        RelativePosition + new Point(Inventory.Width * SlotSize, SlotSize);

    private static readonly Rectangle SlotRectangle = new(1, 1, SlotSize, SlotSize);
    private static readonly Rectangle EquippedSlotSource = new(9 * Resources.TileSize + 7, 1, SlotSize, SlotSize);

    private static readonly Rectangle RelativeEquippedSlotDestination =
        new(RelativeEquippedPosition.X, RelativeEquippedPosition.Y, EquippedSlotSource.Width,
            EquippedSlotSource.Height);

    private readonly Inventory _inventory;
    private Vector2 _mousePosition;

    public ClientInventory(Inventory inventory)
    {
        _inventory = inventory;
    }

    private int GetSlotIndexFromPosition(Vector2 position, BulletHell game)
    {
        var inventoryPosition = game.Ui.AnchorPoint(RelativePosition, Anchor);

        // Use floats to prevent bad results when the mouse is right above
        // or to the left of the inventory slots.
        var slotX = (position.X - inventoryPosition.X) / SlotRectangle.Width;
        var slotY = (position.Y - inventoryPosition.Y) / SlotRectangle.Height;

        if (slotX is < 0 or >= Inventory.Width || slotY is < 0 or >= Inventory.Height) return -1;

        return (int)slotX + (int)slotY * Inventory.Width;
    }

    // Returns true if the mouse input was captured by the inventory.
    public bool Update(BulletHell game, Client client, Camera camera, Input input, Vector2 mousePosition)
    {
        _mousePosition = mousePosition;

        if (!input.WasMouseButtonPressed(MouseButton.Left)) return false;

        var i = GetSlotIndexFromPosition(mousePosition, game);

        if (i != -1)
        {
            RequestGrabSlot(client, i);
            return true;
        }

        var equippedSlotDestination = game.Ui.AnchorRectangle(RelativeEquippedSlotDestination, Anchor);

        if (equippedSlotDestination.ReadonlyContains(mousePosition))
        {
            RequestGrabEquippedSlot(client);
            return true;
        }

        if (_inventory.GrabbedWeaponStats is not null)
        {
            RequestDropGrabbed(client, camera);
            return true;
        }

        return false;
    }

    public void Draw(BulletHell game)
    {
        var inventoryPosition = game.Ui.AnchorPoint(RelativePosition, Anchor);
        var equippedSlotPosition = game.Ui.AnchorPoint(RelativeEquippedPosition, Anchor).ToVector2();

        for (var y = 0; y < Inventory.Height; y++)
        for (var x = 0; x < Inventory.Width; x++)
        {
            var slotPosition = new Vector2(inventoryPosition.X + x * SlotRectangle.Width,
                inventoryPosition.Y + y * SlotRectangle.Height);
            game.SpriteBatch.Draw(game.Resources.UiTexture, slotPosition, SlotRectangle, Color.White);

            var weapon = _inventory.Weapons[x + y * Inventory.Width];

            if (weapon is not null)
            {
                var itemPosition = slotPosition;
                itemPosition.X += ItemSpriteOffset;
                itemPosition.Y += ItemSpriteOffset;
                var sourceRectangle = SpriteMesh.GetSourceRectangle(weapon.Sprite);
                game.SpriteBatch.Draw(game.Resources.SpriteTexture, itemPosition, sourceRectangle, Color.White);
            }
        }

        game.SpriteBatch.Draw(game.Resources.UiTexture, equippedSlotPosition, EquippedSlotSource, Color.White);

        if (_inventory.EquippedWeaponStats is not null)
        {
            var equippedItemPosition = equippedSlotPosition;
            equippedItemPosition.X += ItemSpriteOffset;
            equippedItemPosition.Y += ItemSpriteOffset;
            var sourceRectangle = SpriteMesh.GetSourceRectangle(_inventory.EquippedWeaponStats.Sprite);
            game.SpriteBatch.Draw(game.Resources.SpriteTexture, equippedItemPosition, sourceRectangle, Color.White);
        }

        if (_inventory.GrabbedWeaponStats is null)
        {
            DrawTooltip(game);
        }
        else
        {
            var sourceRectangle = SpriteMesh.GetSourceRectangle(_inventory.GrabbedWeaponStats.Sprite);
            game.SpriteBatch.Draw(game.Resources.SpriteTexture, _mousePosition, sourceRectangle, Color.White, 0f,
                Vector2.Zero, GrabbedItemScale, SpriteEffects.None, 0f);
        }
    }

    private void DrawTooltip(BulletHell game)
    {
        var tooltipPosition = game.Ui.AnchorPoint(RelativeTooltipPosition, Anchor);
        var equippedSlotDestination = game.Ui.AnchorRectangle(RelativeEquippedSlotDestination, Anchor);

        var i = GetSlotIndexFromPosition(_mousePosition, game);
        string? hoveredWeaponName = null;

        if (i != -1)
            hoveredWeaponName = _inventory.Weapons[i]?.DisplayName;
        else if (equippedSlotDestination.ReadonlyContains(_mousePosition))
            hoveredWeaponName = _inventory.EquippedWeaponStats?.DisplayName;

        if (hoveredWeaponName is null) return;

        TextRenderer.Draw(hoveredWeaponName, tooltipPosition.X,
            tooltipPosition.Y, game, Color.White, centered: true);
    }

    private void RequestGrabSlot(Client client, int i)
    {
        client.SendToServer(new RequestGrabSlot
        {
            SlotIndex = i
        }, DeliveryMethod.ReliableOrdered);
    }

    private void RequestGrabEquippedSlot(Client client)
    {
        client.SendToServer(new RequestGrabEquippedSlot(), DeliveryMethod.ReliableOrdered);
    }

    private void RequestDropGrabbed(Client client, Camera camera)
    {
        client.SendToServer(new RequestDropGrabbed
        {
            PlayerForward = new NetVector3(camera.Forward)
        }, DeliveryMethod.ReliableOrdered);
    }
}