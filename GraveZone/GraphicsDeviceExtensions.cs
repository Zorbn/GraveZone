﻿using Microsoft.Xna.Framework.Graphics;

namespace GraveZone;

public static class GraphicsDeviceExtensions
{
    public static void ClearState(this GraphicsDevice graphicsDevice)
    {
        // Set graphics state to be suitable for 3D models.
        // Using the sprite batch modifies these to different values.
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    }
}