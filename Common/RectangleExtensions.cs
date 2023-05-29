using Microsoft.Xna.Framework;

namespace Common;

public static class RectangleExtensions
{
    public static bool ReadonlyContains(this Rectangle rectangle, Vector2 value)
    {
        return rectangle.X <= value.X && value.X < rectangle.X + rectangle.Width &&
               rectangle.Y <= value.Y && value.Y < rectangle.Y + rectangle.Height;
    }
}