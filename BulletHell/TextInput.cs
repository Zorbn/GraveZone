using System;
using System.Text;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class TextInput
{
    private const int Height = Resources.TileSize * 2;
    private const int TextPadding = Resources.TileSize / 2;

    private static readonly Rectangle LeftTexture = new(5 * Resources.TileSize + 9, 1, Resources.TileSize, Height);
    private static readonly Rectangle MiddleTexture = new(6 * Resources.TileSize + 11, 1, Resources.TileSize, Height);
    private static readonly Rectangle RightTexture = new(7 * Resources.TileSize + 13, 1, Resources.TileSize, Height);

    private readonly StringBuilder _text;
    private string _drawableText;
    private readonly int _widthInTiles;
    private readonly int _maxVisibleChars;
    private Rectangle _rectangle;
    private readonly string _defaultText;

    private bool _isFocused;

    public TextInput(int x, int y, int widthInTiles, bool centered, string defaultText)
    {
        _isFocused = false;

        _text = new StringBuilder();
        _defaultText = defaultText;
        _drawableText = "";
        _widthInTiles = widthInTiles;
        _maxVisibleChars = widthInTiles - 1;

        if (_widthInTiles < 3) throw new ArgumentOutOfRangeException(nameof(widthInTiles));

        if (centered)
        {
            x -= _widthInTiles * Resources.TileSize / 2;
            y -= Height / 2;
        }

        _rectangle = new Rectangle(x, y, widthInTiles * Resources.TileSize, Height);

        UpdateDrawableText();
    }

    private void UpdateDrawableText()
    {
        _drawableText = _text.Length == 0 ? _defaultText : _text.ToString();

        if (_drawableText.Length > _maxVisibleChars) _drawableText = _drawableText[^_maxVisibleChars..];
    }

    public void Update(Input input)
    {
        if (!_isFocused) return;
        var pressedKeys = input.CurrentKeyboardState.GetPressedKeys();
        if (pressedKeys.Length == 0) return;

        if (_text.Length > 0 && input.WasKeyPressed(Keys.Back))
        {
            if (input.IsKeyDown(Keys.LeftControl))
                _text.Clear();
            else
                _text.Remove(_text.Length - 1, 1);
        }

        foreach (var heldKey in pressedKeys)
        {
            if (!input.WasKeyPressed(heldKey)) continue;

            var keyIndex = (int)heldKey;
            var keyIsLetter = keyIndex is >= (int)Keys.A and <= (int)Keys.Z;
            var keyIsNumber = keyIndex is >= (int)Keys.D0 and <= (int)Keys.D9;
            var keyIsPeriod = heldKey == Keys.OemPeriod;
            var keyIsValid = keyIsLetter || keyIsNumber || keyIsPeriod;
            if (!keyIsValid) continue;

            var keyString = heldKey.ToString().ToLower();
            var keyChar = keyIsPeriod ? '.' : keyString[^1];
            _text.Append(keyChar);
        }

        UpdateDrawableText();
    }

    public void Draw(SpriteBatch spriteBatch, Resources resources)
    {
        var usingDefaultText = _text.Length == 0;
        var color = usingDefaultText ? Color.Gray : Color.White;

        var offsetRectangle = new Rectangle(_rectangle.X, _rectangle.Y, Resources.TileSize, Height);
        spriteBatch.Draw(resources.UiTexture, offsetRectangle, LeftTexture, Color.White);

        for (var i = 0; i < _widthInTiles - 2; i++)
        {
            offsetRectangle.X += Resources.TileSize;
            spriteBatch.Draw(resources.UiTexture, offsetRectangle, MiddleTexture, Color.White);
        }

        offsetRectangle.X += Resources.TileSize;
        spriteBatch.Draw(resources.UiTexture, offsetRectangle, RightTexture, Color.White);

        TextRenderer.Draw(_drawableText, _rectangle.X + TextPadding, _rectangle.Y + TextPadding, resources, spriteBatch,
            color, false);

        if (_isFocused)
        {
            var textLength = Resources.TileSize * _drawableText.Length;
            TextRenderer.Draw("|", _rectangle.X + TextPadding + textLength, _rectangle.Y + TextPadding, resources,
                spriteBatch,
                Color.White, false);
        }
    }

    public void UpdateFocusWithClick(int x, int y)
    {
        _isFocused = _rectangle.Contains(x, y);
    }

    public string GetTextString()
    {
        return _text.Length == 0 ? _defaultText : _text.ToString();
    }
}