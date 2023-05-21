using Common;
using LiteNetLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class ClientInventory
{
    public const int X = BulletHell.UiCenterX - Inventory.Width * SlotSize / 2;
    public const int Y = BulletHell.UiHeight - SlotSize * Inventory.Height;
    public const int SlotSize = 2 * Resources.TileSize;

    private const int TooltipX = X + (Inventory.Width - 1) * SlotSize / 2;
    private const int TooltipY = Y - SlotSize * 5 / 4;
    private const int ItemSpriteOffset = Resources.TileSize / 2;
    private const int EquippedX = X + Inventory.Width * SlotSize;
    private const int EquippedY = Y + SlotSize;
    private const int GrabbedItemScale = 2;

    private static readonly Rectangle SlotRectangle = new(1, 1, SlotSize, SlotSize);
    private static readonly Rectangle EquippedSlotSource = new(9 * Resources.TileSize + 7, 1, SlotSize, SlotSize);
    private static readonly Rectangle EquippedSlotDestination = new(EquippedX, EquippedY, EquippedSlotSource.Width, EquippedSlotSource.Height);

    private readonly Inventory _inventory;
    private Vector2 _mousePosition;

    public ClientInventory(Inventory inventory)
    {
        _inventory = inventory;
    }

    private int GetSlotIndexFromPosition(Vector2 position)
    {
        // Use floats to prevent bad results when the mouse is right above
        // or to the left of the inventory slots.
        var slotX = (position.X - X) / SlotRectangle.Width;
        var slotY = (position.Y - Y) / SlotRectangle.Height;

        if (slotX is < 0 or >= Inventory.Width || slotY is < 0 or >= Inventory.Height) return -1;

        return (int)slotX + (int)slotY * Inventory.Width;
    }

    // Returns true if the mouse input was captured by the inventory.
    public bool Update(Client client, Camera camera, Input input, Vector2 mousePosition)
    {
        _mousePosition = mousePosition;

        if (!input.WasMouseButtonPressed(MouseButton.Left)) return false;

        var i = GetSlotIndexFromPosition(mousePosition);

        if (i != -1)
        {
            RequestGrabSlot(client, i);
            return true;
        }

        if (EquippedSlotDestination.ReadonlyContains(mousePosition))
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

    public void Draw(Resources resources, SpriteBatch spriteBatch)
    {
        for (var y = 0; y < Inventory.Height; y++)
        for (var x = 0; x < Inventory.Width; x++)
        {
            var slotPosition = new Vector2(X + x * SlotRectangle.Width, Y + y * SlotRectangle.Height);
            spriteBatch.Draw(resources.UiTexture, slotPosition, SlotRectangle, Color.White);

            var weapon = _inventory.Weapons[x + y * Inventory.Width];

            if (weapon is not null)
            {
                var itemPosition = slotPosition;
                itemPosition.X += ItemSpriteOffset;
                itemPosition.Y += ItemSpriteOffset;
                var sourceRectangle = SpriteMesh.GetSourceRectangle(weapon.Sprite);
                spriteBatch.Draw(resources.SpriteTexture, itemPosition, sourceRectangle, Color.White);
            }
        }

        var equippedSlotPosition = new Vector2(EquippedX, EquippedY);
        spriteBatch.Draw(resources.UiTexture, equippedSlotPosition, EquippedSlotSource, Color.White);

        if (_inventory.EquippedWeaponStats is not null)
        {
            var equippedItemPosition = equippedSlotPosition;
            equippedItemPosition.X += ItemSpriteOffset;
            equippedItemPosition.Y += ItemSpriteOffset;
            var sourceRectangle = SpriteMesh.GetSourceRectangle(_inventory.EquippedWeaponStats.Sprite);
            spriteBatch.Draw(resources.SpriteTexture, equippedItemPosition, sourceRectangle, Color.White);
        }

        if (_inventory.GrabbedWeaponStats is null)
        {
            var i = GetSlotIndexFromPosition(_mousePosition);
            string? hoveredWeaponName = null;

            if (i != -1)
            {
                hoveredWeaponName = _inventory.Weapons[i]?.DisplayName;
            }
            else if (EquippedSlotDestination.ReadonlyContains(_mousePosition))
            {
                hoveredWeaponName = _inventory.EquippedWeaponStats?.DisplayName;
            }

            if (hoveredWeaponName is not null)
            {
                TextRenderer.Draw(hoveredWeaponName, TooltipX,
                    TooltipY, resources, spriteBatch, Color.White, centered: true);
            }
        }
        else
        {
            var sourceRectangle = SpriteMesh.GetSourceRectangle(_inventory.GrabbedWeaponStats.Sprite);
            spriteBatch.Draw(resources.SpriteTexture, _mousePosition, sourceRectangle, Color.White, 0f,
                Vector2.Zero, GrabbedItemScale, SpriteEffects.None, 0f);
        }
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