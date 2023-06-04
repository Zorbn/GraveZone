using System;
using Common;
using GraveZone.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GraveZone;

public class GraveZone : Game
{
    public readonly Ui Ui = new();

    // Resources and SpriteBatch are immediately initialized when
    // the game starts and should therefore never be null, if they
    // are null then the game fatally failed to load it's assets.
    public Resources Resources => _resources!;
    public SpriteBatch SpriteBatch => _spriteBatch!;

    private Resources? _resources;
    private SpriteBatch? _spriteBatch;

    private readonly GraphicsDeviceManager _graphics;

    private readonly Input _input = new();

    private IScene _scene;
    private IScene? _nextScene;

    private bool _isFullscreen;
    private int _windowedWidth;
    private int _windowedHeight;

    public GraveZone()
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
        Ui.UpdateScale(width, height);
        _scene.Resize(width, height);
    }

    public Vector2 GetMouseUiPosition()
    {
        var mousePosition = new Vector3(_input.MouseX, _input.MouseY, 0f);
        mousePosition = Vector3.Transform(mousePosition, Matrix.Invert(Ui.Matrix));
        return new Vector2(mousePosition.X, mousePosition.Y);
    }

    protected override void Initialize()
    {
        Ui.UpdateScale(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        _resources = new Resources(Content);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
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