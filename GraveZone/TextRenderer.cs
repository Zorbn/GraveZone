using Common;
using Microsoft.Xna.Framework;

namespace GraveZone;

public static class TextRenderer
{
    public const int BackgroundOffset = Resources.TileSize / 2;

    private const int TextureXStart = 0;
    private const int TextureYStart = 30;
    private const int TextureCharsPerLine = 32;
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.?!,$:|/*";

    // Characters have one pixel pixel of padding around them in the texture sheet to prevent texture bleeding.
    private static readonly Rectangle CharRectangle = new(TextureXStart * Resources.TileSize + 1,
        TextureYStart * Resources.TileSize - 1, Resources.TileSize - 1, Resources.TileSize);

    private static readonly Rectangle TextBackgroundRectangle = Resources.BlackRectangle;

    private static readonly Color TextBackgroundColor = new(1f, 1f, 1f, 0.2f);

    public static void Draw(string text, int x, int y, GraveZone game, Color color, bool withBackground = true,
        float scale = 1f, bool centered = false, UiAnchor uiAnchor = UiAnchor.None)
    {
        (x, y) = game.Ui.AnchorPoint(new Point(x, y), uiAnchor);

        var sizeX = (text.Length + 1) * scale;
        var sizeY = scale * 2;
        var textOffsetX = centered ? (sizeX / 2 - scale) * -Resources.TileSize : 0;

        text = text.ToUpper();

        if (withBackground)
        {
            var bgStartX = centered ? (int)((sizeX / 2 - scale) * -Resources.TileSize) : 0;
            var destination = new Rectangle((int)(x + bgStartX - BackgroundOffset * scale),
                (int)(y - BackgroundOffset * scale),
                (int)(Resources.TileSize * sizeX), (int)(Resources.TileSize * sizeY));
            game.SpriteBatch.Draw(game.Resources.UiTexture, destination, TextBackgroundRectangle, TextBackgroundColor);
        }

        var i = 0;
        foreach (var c in text)
        {
            if (c != ' ')
            {
                var index = Characters.IndexOf(c);
                var texX = index % TextureCharsPerLine;
                var texY = index / TextureCharsPerLine;
                var destination = new Rectangle((int)(x + textOffsetX + Resources.TileSize * scale * i + 1), y,
                    (int)((Resources.TileSize - 1) * scale),
                    (int)(Resources.TileSize * scale));
                var source = CharRectangle;
                source.X += texX * Resources.TileSize;
                source.Y += texY * (Resources.TileSize + 1);

                game.SpriteBatch.Draw(game.Resources.UiTexture, destination, source, color);
            }

            i++;
        }
    }
}