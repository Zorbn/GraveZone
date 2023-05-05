using System;
using BulletHell.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BulletHell;

public class BulletHell : Game
{
    public Resources Resources { get; private set; }
    
    private GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch { get; private set; }

    private IScene _scene;
    
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
    }

    private static void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
    {
        args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
    }

    private void OnResize(object sender, EventArgs eventArgs)
    {
        _scene.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
    }
    
    protected override void Initialize()
    {
        Resources = new Resources(GraphicsDevice);

        _scene = new MainMenuScene(this);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }
    
    protected override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var keyboardState = Keyboard.GetState();
        // Don't allow mouse inputs when the window isn't focused.
        var mouseState = IsActive ? Mouse.GetState() : new MouseState();
        
        _scene.Update(keyboardState, mouseState, deltaTime);
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        _scene.Draw();
        
        base.Draw(gameTime);
    }

    public void SetScene(IScene scene)
    {
        _scene = scene;
    }
}