namespace BulletHell.Scenes;

public interface IScene
{
    public void Update(Input input, float deltaTime);
    public void Draw();

    public void Resize(int width, int height)
    {
    }

    public void Exit()
    {
    }
}