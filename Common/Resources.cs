using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Common;

public class Resources
{ 
    public const int TileSize = 8;
    
    public readonly Texture2D MapTexture;
    public readonly Texture2D SpriteTexture;
    public readonly Texture2D UiTexture;
    
    public static readonly Rectangle BlackRectangle = new(1, 19, TileSize, TileSize);
    public static readonly Rectangle WhiteRectangle = new(11, 19, TileSize, TileSize);

    public static readonly Color SkyColor = new(128, 119, 255, 255);

    public Resources(GraphicsDevice graphicsDevice)
    {
        MapTexture = Texture2D.FromFile(graphicsDevice, "Content/tiles.png");
        SpriteTexture = Texture2D.FromFile(graphicsDevice, "Content/sprites.png");
        UiTexture = Texture2D.FromFile(graphicsDevice, "Content/ui.png");
    }
}