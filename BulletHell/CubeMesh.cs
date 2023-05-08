using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public static class CubeMesh
{
    private const int TextureWidthTiles = 32;
    private const int TextureHeightTiles = 32;
    private const float UnitX = 1f / TextureWidthTiles;
    private const float UnitY = 1f / TextureHeightTiles;

    // Used to prevent texture bleeding, where bits of nearby textures
    // are rendered alongside the desired one.
    // TODO: 
    private static readonly float Padding = Math.Min(UnitX, UnitY) * 0.025f;

    public static readonly Dictionary<Direction, VertexPositionColorTexture[]> Vertices = new()
    {
        {
            Direction.Forward,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting5,
                    new Vector2(UnitX - Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting5,
                    new Vector2(UnitX - Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting5,
                    new Vector2(Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting5,
                    new Vector2(Padding, UnitY - Padding))
            }
        },
        {
            Direction.Backward,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting2,
                    new Vector2(Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting2,
                    new Vector2(Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting2,
                    new Vector2(UnitX - Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting2,
                    new Vector2(UnitX - Padding, UnitY - Padding))
            }
        },
        {
            Direction.Right,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting4,
                    new Vector2(UnitX - Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting4,
                    new Vector2(Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting4,
                    new Vector2(Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting4,
                    new Vector2(UnitX - Padding, Padding))
            }
        },
        {
            Direction.Left,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting3,
                    new Vector2(Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting3,
                    new Vector2(UnitX - Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting3,
                    new Vector2(UnitX - Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting3,
                    new Vector2(Padding, Padding))
            }
        },
        {
            Direction.Up,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 1, 0), GameColors.Lighting6,
                    new Vector2(Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), GameColors.Lighting6,
                    new Vector2(Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), GameColors.Lighting6,
                    new Vector2(UnitX - Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), GameColors.Lighting6,
                    new Vector2(UnitX - Padding, UnitY - Padding))
            }
        },
        {
            Direction.Down,
            new[]
            {
                new VertexPositionColorTexture(new Vector3(0, 0, 0), GameColors.Lighting1,
                    new Vector2(Padding, UnitY - Padding)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), GameColors.Lighting1,
                    new Vector2(Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), GameColors.Lighting1,
                    new Vector2(UnitX - Padding, Padding)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), GameColors.Lighting1,
                    new Vector2(UnitX - Padding, UnitY - Padding))
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
        var y = i / TextureHeightTiles;
        return new Vector2(UnitX * x, UnitY * y);
    }
}