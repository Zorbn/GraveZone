using Microsoft.Xna.Framework;

namespace Common;

public static class Collision
{
    public static bool HasCollision(Vector3 aPosition, Vector3 aSize, Vector3 bPosition, Vector3 bSize)
    {
        var aMin = aPosition - aSize * 0.5f;
        var aMax = aPosition + aSize * 0.5f;
        var bMin = bPosition - bSize * 0.5f;
        var bMax = bPosition + bSize * 0.5f;
        return aMin.X < bMax.X && aMax.X > bMin.X && aMin.Z < bMax.Z && aMax.Z > bMin.Z;
    }
}