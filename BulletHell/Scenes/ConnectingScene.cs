using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class ConnectingScene : IScene
{
    private const float TimeoutTime = 30f;
    
    private BulletHell _game;
    private readonly GameScene _gameScene;
    private bool _hasConnected;
    private bool _transitioningToGame;
    private TextButton _backButton;
    private float _timeoutTimer;
    
    public ConnectingScene(BulletHell game, string ip)
    {
        _game = game;
        _gameScene = new GameScene(game);
        _gameScene.Client.ConnectedEvent += () => _hasConnected = true;
        _gameScene.Client.Connect(ip);
        
        _backButton = new TextButton(BulletHell.UiCenterX, BulletHell.UiCenterY + Resources.TileSize * 3, "back", true);
    }

    public void Exit()
    {
        if (_transitioningToGame) return;
        
        _gameScene.Exit();
    }

    public void Update(Input input, float deltaTime)
    {
        _gameScene.Client.PollEvents();

        if (_timeoutTimer > TimeoutTime)
        {
            _game.SetScene(new MainMenuScene(_game));
        }
        
        _timeoutTimer += deltaTime;
        
        if (input.WasMouseButtonPressed(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;

            if (_backButton.Contains(mouseX, mouseY))
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
        
        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.UiMatrix);
        TextRenderer.Draw("Trying to connect...", BulletHell.UiCenterX, BulletHell.UiCenterY, _game.Resources,
            _game.SpriteBatch, Color.White, centered: true);
        _backButton.Draw(_game.SpriteBatch, _game.Resources);
        _game.SpriteBatch.End();
    }
}