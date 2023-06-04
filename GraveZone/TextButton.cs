using System;
using Common;
using Microsoft.Xna.Framework;

namespace GraveZone;

public class TextButton
{
    // TODO: Consolidate similar fields, drawing code between text input and button.
    private const int Height = Resources.TileSize * 2;
    private const int TextPadding = Resources.TileSize / 2;

    private static readonly Rectangle LeftTexture = new(2 * Resources.TileSize + 3, 1, Resources.TileSize, Height);
    private static readonly Rectangle MiddleTexture = new(3 * Resources.TileSize + 5, 1, Resources.TileSize, Height);
    private static readonly Rectangle RightTexture = new(4 * Resources.TileSize + 7, 1, Resources.TileSize, Height);

    private readonly int _widthInTiles;
    private readonly string _text;
    private readonly Rectangle _rectangle;
    private readonly UiAnchor _uiAnchor;

    public TextButton(int x, int y, string text, bool centered, UiAnchor uiAnchor)
    {
        _text = text;
        _uiAnchor = uiAnchor;

        _widthInTiles = text.Length + 1;

        if (_widthInTiles < 3) throw new ArgumentOutOfRangeException(nameof(text));

        if (centered)
        {
            x -= _widthInTiles * Resources.TileSize / 2;
            y -= Height / 2;
        }

        _rectangle = new Rectangle(x, y, _widthInTiles * Resources.TileSize, Height);
    }

    public void Draw(GraveZone game)
    {
        var anchoredRectangle = game.Ui.AnchorRectangle(_rectangle, _uiAnchor);
        var offsetRectangle = new Rectangle(anchoredRectangle.X, anchoredRectangle.Y, Resources.TileSize, Height);
        game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, LeftTexture, Color.White);

        for (var i = 0; i < _widthInTiles - 2; i++)
        {
            offsetRectangle.X += Resources.TileSize;
            game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, MiddleTexture, Color.White);
        }

        offsetRectangle.X += Resources.TileSize;
        game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, RightTexture, Color.White);

        TextRenderer.Draw(_text, anchoredRectangle.X + TextPadding, anchoredRectangle.Y + TextPadding, game,
            Color.White, false);
    }

    public bool Contains(int x, int y, GraveZone game)
    {
        var anchoredRectangle = game.Ui.AnchorRectangle(_rectangle, _uiAnchor);
        return anchoredRectangle.Contains(x, y);
    }
}