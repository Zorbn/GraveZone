using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class Button
{
    // TODO: Consolidate similar fields, drawing code between text input and button.
    private const int Height = Resources.TileSize * 2;
    private const int TextPadding = Resources.TileSize / 2;
    
    private static readonly Rectangle LeftTexture = new(2 * Resources.TileSize, 0, Resources.TileSize, Height);
    private static readonly Rectangle MiddleTexture = new(3 * Resources.TileSize, 0, Resources.TileSize, Height);
    private static readonly Rectangle RightTexture = new(4 * Resources.TileSize, 0, Resources.TileSize, Height);
    
    private readonly int _widthInTiles;
    private readonly string _text;
    private Rectangle _rectangle;
    
    public Button(int x, int y, string text, bool centered)
    {
        _text = text;
        
        _widthInTiles = text.Length + 1;
        
        if (_widthInTiles < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(text));
        }

        if (centered)
        {
            x -= _widthInTiles * Resources.TileSize / 2;
            y -= Height / 2;
        }
        
        _rectangle = new Rectangle(x, y, _widthInTiles * Resources.TileSize, Height);
    }
    
    public void Draw(SpriteBatch spriteBatch, Resources resources)
    {
        var offsetRectangle = new Rectangle(_rectangle.X, _rectangle.Y, Resources.TileSize, Height);
        spriteBatch.Draw(resources.UiTexture, offsetRectangle, LeftTexture, Color.White);
        
        for (var i = 0; i < _widthInTiles - 2; i++)
        {
            offsetRectangle.X += Resources.TileSize;
            spriteBatch.Draw(resources.UiTexture, offsetRectangle, MiddleTexture, Color.White);
        }
        
        offsetRectangle.X += Resources.TileSize;
        spriteBatch.Draw(resources.UiTexture, offsetRectangle, RightTexture, Color.White);

        TextRenderer.Draw(_text, _rectangle.X + TextPadding, _rectangle.Y + TextPadding, resources, spriteBatch,
            Color.White, false);
    }

    public bool Contains(int x, int y)
    {
        return _rectangle.Contains(x, y);
    }
}