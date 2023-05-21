using Microsoft.Xna.Framework;

namespace Common;

public static class RectangleExtensions
{
    public static bool ReadonlyContains(this Rectangle rectangle, Vector2 value) =>
        (double)rectangle.X <= (double)value.X &&
        (double)value.X <
        (double)(rectangle.X + rectangle.Width) &&
        (double)rectangle.Y <= (double)value.Y &&
        (double)value.Y <
        (double)(rectangle.Y + rectangle.Height);
}