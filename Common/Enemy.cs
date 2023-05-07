using Microsoft.Xna.Framework;

namespace Common;

public class Enemy
{
    private Vector3 _position;
    
    public Enemy(float x, float z)
    {
        _position = new Vector3(x, 0f, z);
    }
}