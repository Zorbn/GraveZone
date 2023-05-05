using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private readonly BulletHell _game;
    private readonly TextInput _ipInput;
    private readonly Button _playButton;
    
    public MainMenuScene(BulletHell game)
    {
        _game = game;
        const int ipInputWidth = 18;
        const int centerX = BulletHell.UiWidth / 2;
        const int centerY = BulletHell.UiHeight / 2;
        _ipInput = new TextInput(centerX, centerY, ipInputWidth, true);
        _playButton = new Button(centerX, centerY + Resources.TileSize * 3, "play", true);
    }

    public void Update(Input input, float deltaTime)
    {
        if (input.IsMouseButtonDown(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            var mouseX = (int)mousePosition.X;
            var mouseY = (int)mousePosition.Y;
            _ipInput.UpdateFocusWithClick(mouseX, mouseY);

            if (_playButton.Contains(mouseX, mouseY))
            {
                _game.SetScene(new GameScene(_game, _ipInput.GetTextString()));
            }
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