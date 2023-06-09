using Microsoft.Xna.Framework;

namespace GraveZone;

public interface IButton
{
    Rectangle Rectangle { get; }
    UiAnchor UiAnchor { get; }
}