using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GraveZone.Scenes;

public class ConnectingScene : IScene
{
    private const float TimeoutTime = 30f;

    private readonly GraveZone _game;
    private readonly GameScene _gameScene;
    private bool _hasConnected;
    private bool _transitioningToGame;
    private readonly TextButton _backButton;
    private float _timeoutTimer;
    private readonly string _loadingText;

    public ConnectingScene(GraveZone game, string ip, bool startInternalServer)
    {
        _loadingText = startInternalServer ? "Loading..." : "Trying to connect...";

        _game = game;
        _gameScene = new GameScene(game, startInternalServer);
        _gameScene.Client.ConnectedEvent += () => _hasConnected = true;
        _gameScene.Client.Connect(ip);

        _backButton = new TextButton(Ui.CenterX, Ui.CenterY + Resources.TileSize * 3, "back", true,
            UiAnchor.None);
    }

    public void Exit()
    {
        if (_transitioningToGame) return;

        _gameScene.Exit();
    }

    public void Update(Input input, float deltaTime)
    {
        _gameScene.Client.PollEvents();

        if (_timeoutTimer > TimeoutTime) _game.SetScene(new MainMenuScene(_game));

        _timeoutTimer += deltaTime;

        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;

            if (_backButton.TryPressWithClick(mouseX, mouseY, _game))
            {
                _game.SetScene(new MainMenuScene(_game));
                return;
            }
        }

        if (!_hasConnected) return;

        _transitioningToGame = true;
        _game.SetScene(_gameScene);
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);

        BackgroundRenderer.Draw(_game);

        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.Ui.Matrix);
        TextRenderer.Draw(_loadingText, Ui.CenterX, Ui.CenterY, _game, Color.White, centered: true);
        _backButton.Draw(_game);
        _game.SpriteBatch.End();
    }
}