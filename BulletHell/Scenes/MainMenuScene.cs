using System.Diagnostics;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private const int IpInputWidth = 18;
    private const float IpInputX = BulletHell.UiCenterX - Resources.TileSize * 3.25f;
    private const int IpInputY = BulletHell.UiCenterY + Resources.TileSize * 4;
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
        _singlePlayerButton = new TextButton(BulletHell.UiCenterX, BulletHell.UiCenterY,
            "play single player", true);
        _ipInput = new TextInput((int)IpInputX, IpInputY, IpInputWidth, true, "localhost");
        _playButton = new TextButton((int)PlayerButtonX, IpInputY, "play", true);
    }

    public void Update(Input input, float deltaTime)
    {
        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;
            _ipInput.UpdateFocusWithClick(mouseX, mouseY);

            if (_playButton.Contains(mouseX, mouseY))
                _game.SetScene(new ConnectingScene(_game, _ipInput.GetTextString(), false));

            if (_singlePlayerButton.Contains(mouseX, mouseY))
                _game.SetScene(new ConnectingScene(_game, "localhost", true));
        }

        _ipInput.Update(input);
    }

    public void Draw()
    {
        Debug.Assert(_game.SpriteBatch is not null && _game.Resources is not null);

        _game.GraphicsDevice.Clear(Color.Aqua);

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.UiMatrix);
        _game.SpriteBatch.Draw(_game.Resources.UiTexture, ServerIpLabelDestination, ServerIpLabelSource, Color.White);
        _ipInput.Draw(_game.SpriteBatch, _game.Resources);
        _playButton.Draw(_game.SpriteBatch, _game.Resources);
        _singlePlayerButton.Draw(_game.SpriteBatch, _game.Resources);
        _game.SpriteBatch.End();
    }
}