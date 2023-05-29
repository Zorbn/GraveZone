using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private const int IpInputWidth = 18;
    private const float IpInputX = Ui.CenterX - Resources.TileSize * 3.25f;
    private const int IpInputY = Ui.CenterY + Resources.TileSize * 4;
    private const float PlayerButtonX = IpInputX + Resources.TileSize * 13;

    private readonly BulletHell _game;
    private readonly TextInput _ipInput;
    private readonly TextButton _playButton;
    private readonly TextButton _singlePlayerButton;

    private static readonly Rectangle ServerIpLabelSource = new(21, 19, 48, 8);

    private static readonly Rectangle ServerIpLabelDestination =
        new((int)IpInputX - IpInputWidth * Resources.TileSize / 2,
            IpInputY - TextInput.Height, 48, 8);

    public MainMenuScene(BulletHell game)
    {
        _game = game;
        _singlePlayerButton = new TextButton(Ui.CenterX, Ui.CenterY,
            "play single player", true, UiAnchor.None);
        _ipInput = new TextInput((int)IpInputX, IpInputY, IpInputWidth, true, "localhost", UiAnchor.None);
        _playButton = new TextButton((int)PlayerButtonX, IpInputY, "play", true, UiAnchor.None);
    }

    public void Update(Input input, float deltaTime)
    {
        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;
            _ipInput.UpdateFocusWithClick(mouseX, mouseY, _game);

            if (_playButton.Contains(mouseX, mouseY, _game))
                _game.SetScene(new ConnectingScene(_game, _ipInput.GetTextString(), false));

            if (_singlePlayerButton.Contains(mouseX, mouseY, _game))
                _game.SetScene(new ConnectingScene(_game, "localhost", true));
        }

        _ipInput.Update(input);
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.Ui.Matrix);
        _game.SpriteBatch.Draw(_game.Resources.UiTexture, ServerIpLabelDestination, ServerIpLabelSource, Color.White);
        _ipInput.Draw(_game);
        _playButton.Draw(_game);
        _singlePlayerButton.Draw(_game);
        _game.SpriteBatch.End();
    }
}