using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell.Scenes;

public class MainMenuScene : IScene
{
    private BulletHell _game;
    
    public MainMenuScene(BulletHell game)
    {
        _game = game;
    }
    
    public void Update(KeyboardState keyboardState, MouseState mouseState, float deltaTime)
    {
        if (keyboardState.IsKeyDown(Keys.Space))
        {
            _game.SetScene(new GameScene(_game));
        }
    }

    public void Draw()
    {
        _game.GraphicsDevice.Clear(Color.Aqua);
    }

    public void Resize(int width, int height)
    {
    }
}