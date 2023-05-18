﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Common;

public class Resources
{
    public const int TileSize = 8;
    public const float ShadowAlpha = 0.2f;

    public readonly Texture2D MapTexture;
    public readonly Texture2D SpriteTexture;
    public readonly Texture2D UiTexture;

    public static readonly Rectangle BlackRectangle = new(1, 19, TileSize, TileSize);
    public static readonly Rectangle WhiteRectangle = new(11, 19, TileSize, TileSize);
    public static readonly Rectangle ShadowLargeRectangle = new(1, 11, TileSize * 2, TileSize * 2);
    public static readonly Rectangle ShadowMediumRectangle = new(21, 11, TileSize, TileSize);
    public static readonly Rectangle ShadowSmallRectangle = new(31, 11, TileSize, TileSize);

    public static readonly Color SkyColor = new(128, 119, 255, 255);

    public Resources(ContentManager contentManager)
    {
        MapTexture = contentManager.Load<Texture2D>("tiles");
        SpriteTexture = contentManager.Load<Texture2D>("sprites");
        UiTexture = contentManager.Load<Texture2D>("ui");
    }
}