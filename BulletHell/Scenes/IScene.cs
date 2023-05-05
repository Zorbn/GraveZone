using Microsoft.Xna.Framework.Input;

namespace BulletHell.Scenes;

public interface IScene
{
    public void Update(KeyboardState keyboardState, MouseState mouseState, float deltaTime);
    public void Draw();
    public void Resize(int width, int height);
}