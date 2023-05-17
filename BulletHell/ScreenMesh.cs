using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public static class ScreenMesh
{
    public static readonly VertexPositionTexture[] Vertices =
    {
        new(new Vector3(-1, -1, 0), new Vector2(0, 1)),
        new(new Vector3(-1, +1, 0), new Vector2(0, 0)),
        new(new Vector3(+1, +1, 0), new Vector2(1, 0)),
        new(new Vector3(+1, -1, 0), new Vector2(1, 1))
    };

    public static readonly ushort[] Indices = { 0, 1, 2, 0, 2, 3 };
}