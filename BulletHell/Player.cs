using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class Player
{
    private const float Speed = 2f;
    
    private Vector3 _position;
    public Vector3 Position => _position;

    public Player(float x, float z)
    {
        _position = new Vector3(x, 0f, z);
    }

    // TODO: Make camera its own class which stores the forward/right vectors.
    public void Update(KeyboardState keyboardState, Map map, Vector3 cameraForward, Vector3 cameraRight, float deltaTime)
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

        if (movement.Length() != 0f)
        {
            movement = movement.Z * cameraForward + movement.X * cameraRight;
            movement.Normalize();

            _position += movement * Speed * deltaTime;
        }
    }

    public void Draw(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.Add(_position.X, _position.Z);
    }
}