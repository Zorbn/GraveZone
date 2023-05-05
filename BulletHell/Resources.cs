using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class Resources
{ 
    public const int TileSize = 8;
    
    public readonly Texture2D MapTexture;
    public readonly Texture2D SpriteTexture;
    public readonly Texture2D UiTexture;

    public Resources(GraphicsDevice graphicsDevice)
    {
        MapTexture = Texture2D.FromFile(graphicsDevice, "Content/tiles.png");
        SpriteTexture = Texture2D.FromFile(graphicsDevice, "Content/sprites.png");
        UiTexture = Texture2D.FromFile(graphicsDevice, "Content/ui.png");
    }
}