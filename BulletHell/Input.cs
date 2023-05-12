using System;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class Input
{
    public KeyboardState PreviousKeyboardState { get; private set; }
    public KeyboardState CurrentKeyboardState { get; private set; }

    public MouseState PreviousMouseState { get; private set; }
    public MouseState CurrentMouseState { get; private set; }
    public int MouseX => CurrentMouseState.X;
    public int MouseY => CurrentMouseState.Y;

    private bool _ignoreLeftButton;

    public Input()
    {
        CurrentKeyboardState = Keyboard.GetState();
        PreviousKeyboardState = CurrentKeyboardState;

        CurrentMouseState = Mouse.GetState();
        PreviousMouseState = CurrentMouseState;
    }

    public void Update(bool isActive)
    {
        PreviousKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = Keyboard.GetState();

        PreviousMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();

        if (CurrentMouseState.LeftButton == ButtonState.Released) _ignoreLeftButton = false;

        if (!isActive) _ignoreLeftButton = true;
    }

    public bool IsKeyDown(Keys key)
    {
        return CurrentKeyboardState.IsKeyDown(key);
    }

    public bool IsMouseButtonDown(MouseButton mouseButton)
    {
        if (_ignoreLeftButton) return false;

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

    public bool WasMouseButtonPressed(MouseButton mouseButton)
    {
        if (_ignoreLeftButton) return false;

        var currentButtonState = mouseButton switch
        {
            MouseButton.Left => CurrentMouseState.LeftButton,
            MouseButton.Middle => CurrentMouseState.MiddleButton,
            MouseButton.Right => CurrentMouseState.RightButton,
            _ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null)
        };

        var previousButtonState = mouseButton switch
        {
            MouseButton.Left => PreviousMouseState.LeftButton,
            MouseButton.Middle => PreviousMouseState.MiddleButton,
            MouseButton.Right => PreviousMouseState.RightButton,
            _ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null)
        };

        return currentButtonState == ButtonState.Pressed && previousButtonState != ButtonState.Pressed;
    }

    public void UiCapturedMouse()
    {
        _ignoreLeftButton = true;
    }
}