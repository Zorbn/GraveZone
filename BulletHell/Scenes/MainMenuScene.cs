using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private readonly BulletHell _game;
    private readonly TextInput _ipInput;
    private readonly TextButton _playButton;

    public MainMenuScene(BulletHell game)
    {
        _game = game;
        const int ipInputWidth = 18;
        _ipInput = new TextInput(BulletHell.UiCenterX, BulletHell.UiCenterY, ipInputWidth, true, "localhost");
        _playButton = new TextButton(BulletHell.UiCenterX, BulletHell.UiCenterY + Resources.TileSize * 3, "play", true);
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
                _game.SetScene(new ConnectingScene(_game, _ipInput.GetTextString()));
        }

        _ipInput.Update(input);
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.UiMatrix);
        _ipInput.Draw(_game.SpriteBatch, _game.Resources);
        _playButton.Draw(_game.SpriteBatch, _game.Resources);
        _game.SpriteBatch.End();
    }
}