using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private const int IpInputWidth = 18;
    private const float IpInputX = Ui.CenterX - Resources.TileSize * 3.25f;
    private const int SinglePlayerButtonY = Ui.CenterY + Resources.TileSize * 3;
    private const float PlayerButtonX = IpInputX + Resources.TileSize * 13;
    private const int ExitButtonY = Ui.CenterY + Resources.TileSize * 6;

    private readonly BulletHell _game;
    private readonly TextInput _ipInput;
    private readonly TextButton _playButton;
    private readonly TextButton _singlePlayerButton;
    private readonly TextButton _exitButton;

    private static readonly Rectangle ServerIpLabelSource = new(21, 19, 48, 8);

    private static readonly Rectangle ServerIpLabelDestination =
        new((int)IpInputX - IpInputWidth * Resources.TileSize / 2,
            Ui.CenterY - TextInput.Height, 48, 8);

    private static readonly Rectangle TitleSource = new(1, 39, 174, 25);

    private static readonly Rectangle TitleDestination = new(Ui.CenterX - TitleSource.Width / 2,
        Ui.CenterY - Resources.TileSize * 8, TitleSource.Width, TitleSource.Height);

    private static readonly Rectangle BackgroundSource = TileMesh.GetSourceRectangle(Tile.Path);

    public MainMenuScene(BulletHell game)
    {
        _game = game;
        _singlePlayerButton = new TextButton(Ui.CenterX, SinglePlayerButtonY,
            "play single player", true, UiAnchor.None);
        _ipInput = new TextInput((int)IpInputX, Ui.CenterY, IpInputWidth, true, "localhost", UiAnchor.None);
        _playButton = new TextButton((int)PlayerButtonX, Ui.CenterY, "play", true, UiAnchor.None);
        _exitButton = new TextButton(Ui.CenterX, ExitButtonY, "exit", true, UiAnchor.None);
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

            if (_exitButton.Contains(mouseX, mouseY, _game))
                _game.Exit();
        }

        _ipInput.Update(input);
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);

        _game.Ui.Matrix.Decompose(out var uiScale, out _, out _);
        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(uiScale));
        var backgroundTilesX = _game.GraphicsDevice.Viewport.Width / uiScale.X / Resources.TileSize;
        var backgroundTilesY = _game.GraphicsDevice.Viewport.Height / uiScale.Y / Resources.TileSize;
        for (var y = 0; y < backgroundTilesY; y++)
        {
            for (var x = 0; x < backgroundTilesX; x++)
            {
                var position = new Vector2(x, y) * Resources.TileSize;
                _game.SpriteBatch.Draw(_game.Resources.MapTexture, position, BackgroundSource, Color.White);
            }
        }
        _game.SpriteBatch.End();

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.Ui.Matrix);
        _game.SpriteBatch.Draw(_game.Resources.UiTexture, ServerIpLabelDestination, ServerIpLabelSource, Color.White);
        _game.SpriteBatch.Draw(_game.Resources.UiTexture, TitleDestination, TitleSource, Color.White);
        _ipInput.Draw(_game);
        _playButton.Draw(_game);
        _singlePlayerButton.Draw(_game);
        _exitButton.Draw(_game);
        _game.SpriteBatch.End();
    }
}