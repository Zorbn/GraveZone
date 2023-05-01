using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class Player
{
    private const float Speed = 2f;
    private static readonly Vector3 Size = new(0.8f, 1.0f, 0.8f);

    private Vector3 _position;
    public Vector3 Position => _position;

    public Player(float x, float z)
    {
        _position = new Vector3(x, 0f, z);
    }

    // TODO: Make camera its own class which stores the forward/right vectors.
    public void Update(KeyboardState keyboardState, Map map, Vector3 cameraForward, Vector3 cameraRight,
        float deltaTime)
    {
        var movement = Vector3.Zero;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            movement.Z += 1f;
        }

        if (keyboardState.IsKeyDown(Keys.S))
        {
            movement.Z -= 1f;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            movement.X -= 1f;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            movement.X += 1f;
        }

        if (movement.Length() == 0f) return;

        movement = movement.Z * cameraForward + movement.X * cameraRight;
        movement.Normalize();

        var newPosition = _position;
        newPosition.X += movement.X * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.X = _position.X;
        }

        _position.X = newPosition.X;

        newPosition.Z += movement.Z * Speed * deltaTime;

        if (map.IsCollidingWithBox(newPosition, Size))
        {
            newPosition.Z = _position.Z;
        }

        _position.Z = newPosition.Z;
    }
    
    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(_position.X, _position.Z);
    }
}