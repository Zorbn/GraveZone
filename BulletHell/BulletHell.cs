using System;
using BulletHell.Scenes;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletHell;

public class BulletHell : Game
{
    public const int UiWidth = 480;
    public const int UiHeight = 270;
    public const int UiCenterX = UiWidth / 2;
    public const int UiCenterY = UiHeight / 2;
    public Matrix UiMatrix { get; private set; }

    public Resources Resources { get; private set; }

    private GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }

    private Input _input;

    private IScene _scene;
    private IScene _nextScene;

    public BulletHell()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferMultiSampling = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreparingDeviceSettings += OnPreparingDeviceSettings;
        Content.RootDirectory = "Content";
        InactiveSleepTime = TimeSpan.Zero;
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnResize;

        // Disable VSYNC:
        // _graphics.SynchronizeWithVerticalRetrace = false;

        // Disable FPS cap, separate from VSYNC:
        IsFixedTimeStep = false;
    }

    private static void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
    {
        args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
    }

    private void OnResize(object sender, EventArgs eventArgs)
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

        Resources = new Resources(GraphicsDevice);
        _input = new Input();

        _scene = new MainMenuScene(this);

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
}