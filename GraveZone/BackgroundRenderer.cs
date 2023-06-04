using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraveZone;

public static class BackgroundRenderer
{
    private static readonly Rectangle BackgroundSource = TileMesh.GetSourceRectangle(Tile.Path);

    public static void Draw(GraveZone game)
    {
        game.Ui.Matrix.Decompose(out var uiScale, out _, out _);
        game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(uiScale));
        var backgroundTilesX = game.GraphicsDevice.Viewport.Width / uiScale.X / Resources.TileSize;
        var backgroundTilesY = game.GraphicsDevice.Viewport.Height / uiScale.Y / Resources.TileSize;
        for (var y = 0; y < backgroundTilesY; y++)
        {
            for (var x = 0; x < backgroundTilesX; x++)
            {
                var position = new Vector2(x, y) * Resources.TileSize;
                game.SpriteBatch.Draw(game.Resources.MapTexture, position, BackgroundSource, Color.White);
            }
        }
        game.SpriteBatch.End();
    }
}