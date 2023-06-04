using System;
using Microsoft.Xna.Framework;

namespace GraveZone;

public class Ui
{
    public const int CenterX = Width / 2;
    public const int CenterY = Height / 2;
    private const int Width = 480;
    private const int Height = 270;

    public Matrix Matrix { get; private set; }

    private int _uiTop;
    private int _uiBottom;

    public void UpdateScale(int width, int height)
    {
        var uiScale = MathF.Min(width / (float)Width, height / (float)Height);
        var offsetX = (width - Width * uiScale) / 2;
        var offsetY = (height - Height * uiScale) / 2;
        _uiTop = -(int)(offsetY / uiScale);
        _uiBottom = Height + (int)(offsetY / uiScale);
        Matrix = Matrix.CreateScale(uiScale) * Matrix.CreateTranslation(offsetX, offsetY, 0f);
    }

    public Point AnchorPoint(Point position, UiAnchor uiAnchor)
    {
        return uiAnchor switch
        {
            UiAnchor.None => position,
            UiAnchor.Top => new Point(position.X, position.Y + _uiTop),
            UiAnchor.Bottom => new Point(position.X, position.Y + _uiBottom),
            _ => throw new ArgumentOutOfRangeException(nameof(uiAnchor), uiAnchor, null)
        };
    }

    public Rectangle AnchorRectangle(Rectangle rectangle, UiAnchor uiAnchor)
    {
        var position = AnchorPoint(rectangle.Location, uiAnchor);

        return rectangle with
        {
            X = position.X,
            Y = position.Y
        };
    }
}