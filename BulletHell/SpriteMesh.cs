using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public static class SpriteMesh
{
    private const int TextureWidthTiles = 32;
    private const int TextureHeightTiles = 32;
    private const float UnitX = 1f / TextureWidthTiles;
    private const float UnitY = 1f / TextureHeightTiles;
    public const float InvTextureWidth = 1f / (TextureWidthTiles * Resources.TileSize);
    public const float InvTextureHeight = 1f / (TextureHeightTiles * Resources.TileSize);

    public static readonly VertexPositionColorTexture[] Vertices = {
        new(new Vector3(-0.5f, 0, 0), Color.White, new Vector2(0, UnitY)),
        new(new Vector3(-0.5f, 1, 0), Color.White, new Vector2(0, 0)),
        new(new Vector3(+0.5f, 1, 0), Color.White, new Vector2(UnitX, 0)),
        new(new Vector3(+0.5f, 0, 0), Color.White, new Vector2(UnitX, UnitY))
    };

    public static readonly ushort[] Indices = { 0, 1, 2, 0, 2, 3 };
}
