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

    private static readonly Rectangle SlotRectangle = new(1, 1, SlotSize, SlotSize);
    private static readonly Rectangle EquippedSlotRectangle = new(9 * Resources.TileSize + 7, 1, SlotSize, SlotSize);

    private Weapon[] _weapons = new Weapon[Width * Height];
    private Weapon _equippedWeapon;
    
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
                    spriteBatch.Draw(resources.UiTexture, itemPosition, weapon.SourceRectangle, Color.White);
                }
            }
        }
        
        var equippedSlotPosition = new Vector2(EquippedX, EquippedY);
        spriteBatch.Draw(resources.UiTexture, equippedSlotPosition, EquippedSlotRectangle, Color.White);

        if (_equippedWeapon is not null)
        {
            var equippedItemPosition = equippedSlotPosition;
            equippedItemPosition.X += ItemSpriteOffset;
            equippedItemPosition.Y += ItemSpriteOffset;
            spriteBatch.Draw(resources.UiTexture, equippedItemPosition, _equippedWeapon.SourceRectangle, Color.White);
        }
    }
}