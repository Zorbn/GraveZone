using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class ImageButton
{
    public static readonly Rectangle QuitRectangle =
        new(1, 3 * Resources.TileSize + 5, Resources.TileSize, Resources.TileSize);
    
    private Rectangle _destination;
    private Rectangle _source;
    
    public ImageButton(int x, int y, Rectangle source)
    {
        _source = source;
        _destination = new Rectangle(x, y, source.Width, source.Height);
    }
    
    public void Draw(SpriteBatch spriteBatch, Resources resources)
    {
        spriteBatch.Draw(resources.UiTexture, _destination, _source, Color.White);
    }

    public bool Contains(int x, int y)
    {
        return _destination.Contains(x, y);
    }
}