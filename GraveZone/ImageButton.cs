using Common;
using Microsoft.Xna.Framework;

namespace GraveZone;

public class ImageButton : IButton
{
    public static readonly Rectangle QuitRectangle =
        new(1, 3 * Resources.TileSize + 5, Resources.TileSize, Resources.TileSize);

    public Rectangle Rectangle => _destination;
    public UiAnchor UiAnchor { get; }

    private readonly Rectangle _destination;
    private readonly Rectangle _source;

    public ImageButton(int x, int y, Rectangle source, UiAnchor uiAnchor)
    {
        _source = source;
        _destination = new Rectangle(x, y, source.Width, source.Height);
        UiAnchor = uiAnchor;
    }

    public void Draw(GraveZone game)
    {
        var anchoredDestination = game.Ui.AnchorRectangle(_destination, UiAnchor);
        game.SpriteBatch.Draw(game.Resources.UiTexture, anchoredDestination, _source, Color.White);
    }
}