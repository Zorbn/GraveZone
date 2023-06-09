namespace GraveZone;

public static class ButtonExtensions
{
    public static bool TryPressWithClick(this IButton button, int x, int y, GraveZone game)
    {
        var anchoredRectangle = game.Ui.AnchorRectangle(button.Rectangle, button.UiAnchor);
        var clicked = anchoredRectangle.Contains(x, y);

        if (clicked)
            game.Audio.PlaySoundWithPitch(Sound.Click);

        return clicked;
    }
}