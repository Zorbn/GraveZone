using System;
using BulletHell.Scenes;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

/**
 * TODO (Goals):
 * Improved menus,
 * Save files,
 */
public class BulletHell : Game
{
    public const int UiWidth = 480;
    public const int UiHeight = 270;
    public const int UiCenterX = UiWidth / 2;
    public const int UiCenterY = UiHeight / 2;
    public Matrix UiMatrix { get; private set; }

    public Resources? Resources { get; private set; }

    public SpriteBatch? SpriteBatch { get; private set; }

    private readonly GraphicsDeviceManager _graphics;

    private readonly Input _input = new();

    private IScene _scene;
    private IScene? _nextScene;

    private bool _isFullscreen;
    private int _windowedWidth;
    private int _windowedHeight;

    public BulletHell()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";
        InactiveSleepTime = TimeSpan.Zero;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;

        // Disable VSYNC:
        // _graphics.SynchronizeWithVerticalRetrace = false;

        // Disable FPS cap, separate from VSYNC:
        IsFixedTimeStep = false;

        _scene = new MainMenuScene(this);
    }

    private void OnResize(object? sender, EventArgs? eventArgs)
    {
        var width = GraphicsDevice.Viewport.Width;
        var height = GraphicsDevice.Viewport.Height;
        UpdateUiScale(width, height);
        _scene.Resize(width, height);
    }

    private void UpdateUiScale(int width, int height)
    {
        var uiScale = MathF.Min(width / (float)UiWidth, height / (float)UiHeight);
        var offsetX = (width - UiWidth * uiScale) / 2;
        var offsetY = (height - UiHeight * uiScale) / 2;
        UiMatrix = Matrix.CreateScale(uiScale) * Matrix.CreateTranslation(offsetX, offsetY, 0f);
    }

    public Vector2 GetMouseUiPosition()
    {
        var mousePosition = new Vector3(_input.MouseX, _input.MouseY, 0f);
        mousePosition = Vector3.Transform(mousePosition, Matrix.Invert(UiMatrix));
        return new Vector2(mousePosition.X, mousePosition.Y);
    }

    protected override void Initialize()
    {
        UpdateUiScale(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        Resources = new Resources(Content);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _input.Update(IsActive);
        _scene.Update(_input, deltaTime);

        if (_input.WasKeyPressed(Keys.F11)) ToggleFullscreen();

        if (_nextScene is not null)
        {
            _scene.Exit();
            _scene = _nextScene;
            _nextScene = null;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _scene.Draw();

        base.Draw(gameTime);
    }

    public void SetScene(IScene nextScene)
    {
        if (_nextScene is not null) return;

        _nextScene = nextScene;
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        _scene.Exit();

        base.OnExiting(sender, args);
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            DisableFullscreen();
            return;
        }

        EnableFullscreen();
    }

    private void EnableFullscreen()
    {
        _isFullscreen = true;

        _windowedWidth = Window.ClientBounds.Width;
        _windowedHeight = Window.ClientBounds.Height;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.HardwareModeSwitch = true;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
    }

    private void DisableFullscreen()
    {
        _isFullscreen = false;

        _graphics.PreferredBackBufferWidth = _windowedWidth;
        _graphics.PreferredBackBufferHeight = _windowedHeight;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();
    }
}