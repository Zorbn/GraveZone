using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class TextRenderer
{
    public const int TextureXStart = 0;
    public const int TextureYStart = 30;
    public const int TextureCharsPerLine = 32;
    public const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.?!,$:|";

    // Characters have one pixel pixel of padding around them in the texture sheet to prevent texture bleeding.
    private static readonly Rectangle CharRectangle = new(TextureXStart * Resources.TileSize + 1,
        TextureYStart * Resources.TileSize - 1, Resources.TileSize - 1, Resources.TileSize);

    private static readonly Rectangle TextBackgroundRectangle = Resources.BlackRectangle;

    public static void Draw(string text, int x, int y, Resources resources, SpriteBatch spriteBatch, Color color,
        bool withBackground = true, int scale = 1, bool centered = false)
    {
        var sizeX = (text.Length + 1) * scale;
        var sizeY = scale * 2;
        var textOffsetX = centered ? (sizeX / 2 - scale) * -Resources.TileSize : 0;

        text = text.ToUpper();

        if (withBackground)
        {
            const int bgOffset = -Resources.TileSize / 2;
            var bgStartX = centered ? (sizeX / 2 - scale) * -Resources.TileSize : 0;
            var destination = new Rectangle(x + bgStartX + bgOffset, y + bgOffset, Resources.TileSize * sizeX,
                Resources.TileSize * sizeY);
            spriteBatch.Draw(resources.UiTexture, destination, TextBackgroundRectangle, Color.White);
        }

        var i = 0;
        foreach (var c in text)
        {
            if (c != ' ')
            {
                var index = Characters.IndexOf(c);
                var texX = index % TextureCharsPerLine;
                var texY = index / TextureCharsPerLine;
                var destination = new Rectangle(x + textOffsetX + Resources.TileSize * scale * i + 1, y,
                    Resources.TileSize - 1,
                    Resources.TileSize);
                var source = CharRectangle;
                source.X += texX * Resources.TileSize;
                source.Y += texY * (Resources.TileSize + 1);

                spriteBatch.Draw(resources.UiTexture, destination, source, color);
            }

            i++;
        }
    }
}