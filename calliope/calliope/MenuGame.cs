using System;
using System.Collections.Generic;
using System.IO;
using calliope.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace calliope;

public class MenuGame : Game
{
    private Dictionary<string,string> _config = new ();
    private GraphicsDeviceManager _graphics;
    private OrthographicCamera _camera;
    private SpriteBatch _spriteBatch;
    private float _renderScale = 1.0f;
    private Dictionary<string,Texture2D> _textures = new();
    private Dictionary<string,SpriteFont> _fonts = new();
    private Dictionary<string,SoundEffect> _sfx = new();

    private Menu _mainMenu;

    public MenuGame()
    {
        // --- CONFIG FILE ---
        var configfile = File.ReadAllLines("../../../config");
        foreach (var line in configfile)
        {
            _config.Add(line.Split('=')[0], line.Split('=')[1]);
        }

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / float.Parse(_config["framerate"]));
        
        int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        int scalefactor = (screenHeight/4 * 3) / int.Parse(_config["screenheight"]);
        _renderScale = float.Parse(_config["renderscale"]);
        
        _graphics.PreferredBackBufferHeight = int.Parse(_config["screenheight"]) * scalefactor;
        _graphics.PreferredBackBufferWidth = int.Parse(_config["screenwidth"]) * scalefactor;
        
        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 
            (int)(int.Parse(_config["screenwidth"])*_renderScale), (int)(int.Parse(_config["screenheight"])*_renderScale));
        _camera = new OrthographicCamera(viewportAdapter);
        _camera.Position = new Vector2(0,0);
        
        _graphics.ApplyChanges();
        
        SoundEffect.MasterVolume = float.Parse(_config["sfxvolume"]);
        MediaPlayer.Volume = float.Parse(_config["musvolume"]);
        
        Window.Title = "Calliope RPG Engine";
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        LoadAsset("Assets/Images/basechar");
        LoadAsset("Assets/Fonts/GamerFont");
        LoadAsset("Assets/Fonts/ArialFont");
        LoadAsset("Assets/Sounds/ding");
        LoadAsset("Assets/Sounds/accept");
        LoadAsset("Assets/Sounds/deny");

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        
        // Worldspace Draw
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    void LoadAsset(string name, bool output = true)
    {
        if (output) Console.WriteLine($"Loading asset {name}...");
        if (name[..14] == "Assets/Images/")
        {
            _textures[name[14..]] = Content.Load<Texture2D>(name);
            if (output) Console.WriteLine($"Texture2D {name[14..]} loaded!");
        }
        else if (name[..13] == "Assets/Fonts/")
        {
            _fonts[name[13..]] = Content.Load<SpriteFont>(name);
            if (output) Console.WriteLine($"SpriteFont {name[13..]} loaded!");
        }
        else if (name[..14] == "Assets/Sounds/")
        {
            _sfx[name[14..]] = Content.Load<SoundEffect>(name);
            if (output) Console.WriteLine($"SoundEffect {name[14..]} loaded!");
        }
        else if (output) Console.WriteLine($"Type of {name} not found.");
    }
}