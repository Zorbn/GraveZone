using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private readonly BulletHell _game;
    private readonly TextInput _ipInput;
    
    public MainMenuScene(BulletHell game)
    {
        _game = game;
        const int ipInputWidth = 18;
        _ipInput = new TextInput(BulletHell.UiWidth / 2 - ipInputWidth * Resources.TileSize / 2,
            BulletHell.UiHeight / 2, ipInputWidth);
    }
    
    public void Update(Input input, float deltaTime)
    {
        // if (input.IsKeyDown(Keys.Space))
        // {
        //     _game.SetScene(new GameScene(_game));
        // }

        if (input.IsMouseButtonDown(MouseButton.Left))
        {
            var mousePosition = _game.GetMouseUiPosition();
            _ipInput.UpdateFocusWithClick((int)mousePosition.X, (int)mousePosition.Y);
        }
        
        _ipInput.Update(input);
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);
        
        _game.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _game.UiMatrix);
        _ipInput.Draw(_game.SpriteBatch, _game.Resources);
        _game.SpriteBatch.End();
    }

    public void Resize(int width, int height)
    {
    }
}