using Common;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class ImageButton
{
    public static readonly Rectangle QuitRectangle =
        new(1, 3 * Resources.TileSize + 5, Resources.TileSize, Resources.TileSize);

    private readonly Rectangle _destination;
    private readonly Rectangle _source;
    private readonly UiAnchor _uiAnchor;

    public ImageButton(int x, int y, Rectangle source, UiAnchor uiAnchor)
    {
        _source = source;
        _destination = new Rectangle(x, y, source.Width, source.Height);
        _uiAnchor = uiAnchor;
    }

    public void Draw(BulletHell game)
    {
        var anchoredDestination = game.Ui.AnchorRectangle(_destination, _uiAnchor);
        game.SpriteBatch.Draw(game.Resources.UiTexture, anchoredDestination, _source, Color.White);
    }

    public bool Contains(int x, int y, BulletHell game)
    {
        var anchoredRectangle = game.Ui.AnchorRectangle(_destination, _uiAnchor);
        return anchoredRectangle.Contains(x, y);
    }
}