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

    private Menu statusMenu;

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
        _camera = new OrthographicCamera(viewportAdapter)
        {
            /*Position = _renderScale *
                       new Vector2((float.Parse(_config["screenwidth"]) / 2),
                           (float.Parse(_config["screenheight"]) / 2))*/
        };
        
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

        List<MenuComponent> components = new();
        MenuComponent comp;
        comp = new MenuComponent(new Vector2(-200, 0), new Vector2(80, 50), _renderScale, "Box", _fonts["GamerFont"]);
        components.Add(comp);

        comp = new MenuComponent(new Vector2(0, 0), new Vector2(80, 50), _renderScale, "Box", _fonts["GamerFont"]);
        components.Add(comp);
        
        comp = new MenuComponent(new Vector2(200, 0), new Vector2(80, 50), _renderScale, "Box", _fonts["GamerFont"]);
        components.Add(comp);
        
        statusMenu = new Menu(components)
        {
            RenderScale = _renderScale,
            Camera = _camera
        };
        
        statusMenu.SetSounds(_sfx["ding"], _sfx["accept"], _sfx["deny"]);
        
        statusMenu.Components[0].AddRelation(statusMenu.Components[1],MenuComponent.NavDirections.Right);
        statusMenu.Components[1].AddRelation(statusMenu.Components[2],MenuComponent.NavDirections.Right);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        statusMenu.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        
        // Worldspace Draw
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        
        statusMenu.Draw(_spriteBatch, gameTime);
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }

    void LoadAsset(string name, bool output = false)
    {
        if (output) Console.WriteLine($"Loading asset {name}...");
        if (name[..14] == "Assets/Images/")
        {
            string alias = name[14..];
            _textures[alias] = Content.Load<Texture2D>(name);
            if (output) Console.WriteLine($"Texture2D {alias} loaded!");
        }
        else if (name[..13] == "Assets/Fonts/")
        {
            string alias = name[13..];
            _fonts[alias] = Content.Load<SpriteFont>(name);
            if (output) Console.WriteLine($"SpriteFont {alias} loaded!");
        }
        else if (name[..14] == "Assets/Sounds/")
        {
            string alias = name[14..];
            _sfx[alias] = Content.Load<SoundEffect>(name);
            if (output) Console.WriteLine($"SoundEffect {alias} loaded!");
        }
        else if (output) Console.WriteLine($"Type of {name} not found.");
    }
}