using System;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class Input
{
    public KeyboardState PreviousKeyboardState { get; private set; }
    public KeyboardState CurrentKeyboardState { get; private set; }
    
    public MouseState CurrentMouseState { get; private set; }
    public int MouseX => CurrentMouseState.X;
    public int MouseY => CurrentMouseState.Y;

    public Input()
    {
        CurrentKeyboardState = Keyboard.GetState();
        PreviousKeyboardState = CurrentKeyboardState;
    }

    public void Update(bool isActive)
    {
        PreviousKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = Keyboard.GetState();
        
        CurrentMouseState = isActive ? Mouse.GetState() : new MouseState();
        CurrentMouseState = Mouse.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
        return CurrentKeyboardState.IsKeyDown(key);
    }
    
    public bool IsMouseButtonDown(MouseButton mouseButton)
    {
        var buttonState = mouseButton switch
        {
            MouseButton.Left => CurrentMouseState.LeftButton,
            MouseButton.Middle => CurrentMouseState.MiddleButton,
            MouseButton.Right => CurrentMouseState.RightButton,
            _ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null)
        };

        return buttonState == ButtonState.Pressed;
    }

    public bool WasKeyPressed(Keys key)
    {
        return CurrentKeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
    }
}
