using System;
using System.Text;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class TextInput
{
    public const int Height = Resources.TileSize * 2;
    private const int TextPadding = Resources.TileSize / 2;

    private static readonly Rectangle LeftTexture = new(5 * Resources.TileSize + 9, 1, Resources.TileSize, Height);
    private static readonly Rectangle MiddleTexture = new(6 * Resources.TileSize + 11, 1, Resources.TileSize, Height);
    private static readonly Rectangle RightTexture = new(7 * Resources.TileSize + 13, 1, Resources.TileSize, Height);

    private readonly StringBuilder _text;
    private string _drawableText;
    private readonly int _widthInTiles;
    private readonly int _maxVisibleChars;
    private readonly Rectangle _rectangle;
    private readonly string _defaultText;
    private readonly UiAnchor _uiAnchor;

    private bool _isFocused;

    public TextInput(int x, int y, int widthInTiles, bool centered, string defaultText, UiAnchor uiAnchor)
    {
        _isFocused = false;

        _text = new StringBuilder();
        _defaultText = defaultText;
        _drawableText = "";
        _widthInTiles = widthInTiles;
        _maxVisibleChars = widthInTiles - 1;
        _uiAnchor = uiAnchor;

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

    public void Draw(BulletHell game)
    {
        var usingDefaultText = _text.Length == 0;
        var color = usingDefaultText ? Color.Gray : Color.White;

        var anchoredRectangle = game.Ui.AnchorRectangle(_rectangle, _uiAnchor);
        var offsetRectangle = new Rectangle(anchoredRectangle.X, anchoredRectangle.Y, Resources.TileSize, Height);
        game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, LeftTexture, Color.White);

        for (var i = 0; i < _widthInTiles - 2; i++)
        {
            offsetRectangle.X += Resources.TileSize;
            game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, MiddleTexture, Color.White);
        }

        offsetRectangle.X += Resources.TileSize;
        game.SpriteBatch.Draw(game.Resources.UiTexture, offsetRectangle, RightTexture, Color.White);

        TextRenderer.Draw(_drawableText, anchoredRectangle.X + TextPadding, anchoredRectangle.Y + TextPadding, game,
            color, false);

        if (_isFocused)
        {
            var textLength = Resources.TileSize * _drawableText.Length;
            TextRenderer.Draw("|", anchoredRectangle.X + TextPadding + textLength, anchoredRectangle.Y + TextPadding,
                game, Color.White, false);
        }
    }

    public void UpdateFocusWithClick(int x, int y, BulletHell game)
    {
        var anchoredRectangle = game.Ui.AnchorRectangle(_rectangle, _uiAnchor);
        _isFocused = anchoredRectangle.Contains(x, y);
    }

    public string GetTextString()
    {
        return _text.Length == 0 ? _defaultText : _text.ToString();
    }
}