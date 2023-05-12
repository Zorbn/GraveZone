using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public static class CubeMesh
{
    private const int TextureWidthTiles = 25;
    private const int TextureSize = 256;
    private const float UnitX = Resources.TileSize / (float)TextureSize;

    private const float UnitY = Resources.TileSize / (float)TextureSize;

    // Texture have padding between them to prevent texture bleeding, where bits
    // of nearby textures are rendered alongside the desired one.
    private const float PaddingX = 1f / TextureSize;
    private const float PaddingY = 1f / TextureSize;
    private const float PaddedUnitX = UnitX + PaddingX * 2;
    private const float PaddedUnitY = UnitY + PaddingY * 2;

    public static readonly Dictionary<Direction, VertexPositionColorTexture[]> Vertices = new()
    {
        {
            Direction.Forward,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting5,
                    new Vector2(UnitX, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting5,
                    new Vector2(UnitX, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting5,
                    new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting5,
                    new Vector2(0, UnitY))
            }
        },
        {
            Direction.Backward,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting2,
                    new Vector2(0, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting2,
                    new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting2,
                    new Vector2(UnitX, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting2,
                    new Vector2(UnitX, UnitY))
            }
        },
        {
            Direction.Right,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting4,
                    new Vector2(UnitX, UnitY)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting4,
                    new Vector2(0, UnitY)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting4,
                    new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting4,
                    new Vector2(UnitX, 0))
            }
        },
        {
            Direction.Left,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting3,
                    new Vector2(0, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting3,
                    new Vector2(UnitX, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting3,
                    new Vector2(UnitX, 0)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting3,
                    new Vector2(0, 0))
            }
        },
        {
            Direction.Up,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting6,
                    new Vector2(0, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting6,
                    new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting6,
                    new Vector2(UnitX, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting6,
                    new Vector2(UnitX, UnitY))
            }
        },
        {
            Direction.Down,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting1,
                    new Vector2(0, UnitY)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting1,
                    new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting1,
                    new Vector2(UnitX, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting1,
                    new Vector2(UnitX, UnitY))
            }
        }
    };

    public static readonly Dictionary<Direction, ushort[]> Indices = new()
    {
        {
            Direction.Forward,
            new ushort[] { 0, 2, 1, 0, 3, 2 }
        },
        {
            Direction.Backward,
            new ushort[] { 0, 1, 2, 0, 2, 3 }
        },
        {
            Direction.Right,
            new ushort[] { 0, 1, 2, 0, 2, 3 }
        },
        {
            Direction.Left,
            new ushort[] { 0, 2, 1, 0, 3, 2 }
        },
        {
            Direction.Up,
            new ushort[] { 0, 2, 1, 0, 3, 2 }
        },
        {
            Direction.Down,
            new ushort[] { 0, 1, 2, 0, 2, 3 }
        }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 GetTexCoord(Tile tile)
    {
        var i = (int)tile;
        var x = i % TextureWidthTiles;
        var y = i / TextureWidthTiles;
        return new Vector2(PaddingX + PaddedUnitX * x, PaddingY + PaddedUnitY * y);
    }
}