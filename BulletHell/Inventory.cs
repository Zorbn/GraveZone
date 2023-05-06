using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class Inventory
{
    public const int X = BulletHell.UiCenterX - Width * SlotSize / 2;
    public const int Y = BulletHell.UiHeight - SlotSize * Height;

    private const int Width = 10;
    private const int Height = 2;
    private const int SlotSize = 2 * Resources.TileSize;
    private const int ItemSpriteOffset = Resources.TileSize / 2;
    private const int EquippedX = X + Width * SlotSize;
    private const int EquippedY = Y + SlotSize;
    private const int GrabbedItemScale = 2;

    private static readonly Rectangle SlotRectangle = new(1, 1, SlotSize, SlotSize);
    private static readonly Rectangle EquippedSlotRectangle = new(9 * Resources.TileSize + 7, 1, SlotSize, SlotSize);
    
    public Weapon EquippedWeapon { get; private set; }

    private Weapon[] _weapons = new Weapon[Width * Height];
    private Weapon _grabbedWeapon;
    private Vector2 _mousePosition;

    public Inventory()
    {
    }

    // Returns true if the weapon was successfully added, false otherwise (ie: the inventory is full).
    public bool AddWeapon(Weapon weapon)
    {
        for (var i = 0; i < _weapons.Length; i++)
        {
            if (_weapons[i] is not null) continue;

            _weapons[i] = weapon;

            return true;
        }

        return false;
    }

    private int GetSlotIndexFromPosition(Vector2 position)
    {
        // Use floats to prevent bad results when the mouse is right above
        // or to the left of the inventory slots.
        var slotX = (position.X - X) / SlotRectangle.Width;
        var slotY = (position.Y - Y) / SlotRectangle.Height;

        if (slotX is < 0 or >= Width || slotY is < 0 or >= Height) return -1;

        return (int)slotX + (int)slotY * Width;
    }

    public Weapon RemoveWeapon(int i)
    {
        var weapon = _weapons[i];
        _weapons[i] = null;

        return weapon;
    }

    private void GrabSlot(int i)
    {
        if (_grabbedWeapon is null)
        {
            var removedWeapon = RemoveWeapon(i);
            _grabbedWeapon = removedWeapon;
            return;
        }

        (_weapons[i], _grabbedWeapon) = (_grabbedWeapon, _weapons[i]);
    }
    
    private void GrabEquippedSlot()
    {
        if (_grabbedWeapon is null)
        {
            _grabbedWeapon = EquippedWeapon;
            EquippedWeapon = null;
            return;
        }
        
        (EquippedWeapon, _grabbedWeapon) = (_grabbedWeapon, EquippedWeapon);
    }

    public bool Update(Input input, Vector2 mousePosition)
    {
        _mousePosition = mousePosition;
        
        if (!input.WasMouseButtonPressed(MouseButton.Left)) return false;

        var i = GetSlotIndexFromPosition(mousePosition);
        var equippedSlotDestination = new Rectangle(EquippedX, EquippedY, EquippedSlotRectangle.Width, EquippedSlotRectangle.Height);

        if (i != -1)
        {
            GrabSlot(i);
            return true;
        }
        
        if (equippedSlotDestination.Contains(mousePosition))
        {
            GrabEquippedSlot();
            return true;
        }

        if (_grabbedWeapon is not null)
        {
            // TODO: Allow dropping items by clicking outside the inventory when holding one.
            return true;
        }

        return false;
    }

    public void Draw(Resources resources, SpriteBatch spriteBatch)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var slotPosition = new Vector2(X + x * SlotRectangle.Width, Y + y * SlotRectangle.Height);
                spriteBatch.Draw(resources.UiTexture, slotPosition, SlotRectangle, Color.White);

                var weapon = _weapons[x + y * Width];

                if (weapon is not null)
                {
                    var itemPosition = slotPosition;
                    itemPosition.X += ItemSpriteOffset;
                    itemPosition.Y += ItemSpriteOffset;
                    spriteBatch.Draw(resources.SpriteTexture, itemPosition, weapon.SourceRectangle, Color.White);
                }
            }
        }

        var equippedSlotPosition = new Vector2(EquippedX, EquippedY);
        spriteBatch.Draw(resources.UiTexture, equippedSlotPosition, EquippedSlotRectangle, Color.White);

        if (EquippedWeapon is not null)
        {
            var equippedItemPosition = equippedSlotPosition;
            equippedItemPosition.X += ItemSpriteOffset;
            equippedItemPosition.Y += ItemSpriteOffset;
            spriteBatch.Draw(resources.SpriteTexture, equippedItemPosition, EquippedWeapon.SourceRectangle, Color.White);
        }

        if (_grabbedWeapon is not null)
        {
            spriteBatch.Draw(resources.SpriteTexture, _mousePosition, _grabbedWeapon.SourceRectangle, Color.White, 0f,
                Vector2.Zero, GrabbedItemScale, SpriteEffects.None, 0f);
        }
    }
}