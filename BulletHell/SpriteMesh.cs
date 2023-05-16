using System.Runtime.CompilerServices;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public static class SpriteMesh
{
    private const int TextureSize = 256;
    private const int TextureWidthTiles = 25;
    private const float UnitX = Resources.TileSize / (float)TextureSize;
    private const float UnitY = Resources.TileSize / (float)TextureSize;
    private const float PaddingX = 1f / TextureSize;
    private const float PaddingY = 1f / TextureSize;
    private const float PaddedUnitX = UnitX + PaddingX * 2;
    private const float PaddedUnitY = UnitY + PaddingY * 2;
    public const float OutlineWidth = 1f / TextureSize * 0.3f;

    public static readonly VertexPositionColorTexture[] Vertices =
    {
        new(new Vector3(-0.5f, 0, 0), Color.White, new Vector2(-OutlineWidth, UnitY + OutlineWidth)),
        new(new Vector3(-0.5f, 1, 0), Color.White, new Vector2(-OutlineWidth, -OutlineWidth)),
        new(new Vector3(+0.5f, 1, 0), Color.White, new Vector2(UnitX + OutlineWidth, -OutlineWidth)),
        new(new Vector3(+0.5f, 0, 0), Color.White, new Vector2(UnitX + OutlineWidth, UnitY + OutlineWidth))
    };

    public static readonly ushort[] Indices = { 0, 1, 2, 0, 2, 3 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetTexCoord(Sprite sprite)
    {
        var i = (int)sprite;
        var x = i % TextureWidthTiles;
        var y = i / TextureWidthTiles;
        return new Vector2(PaddingX + PaddedUnitX * x, PaddingY + PaddedUnitY * y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle GetSourceRectangle(Sprite sprite)
    {
        var i = (int)sprite;
        var x = i % TextureWidthTiles;
        var y = i / TextureWidthTiles;
        return new Rectangle(1 + (Resources.TileSize + 2) * x, 1 + (Resources.TileSize + 2) * y, Resources.TileSize,
            Resources.TileSize);
    }
}