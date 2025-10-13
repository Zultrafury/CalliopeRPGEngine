using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using calliope.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace calliope;

public class Game1 : Game
{
    private Dictionary<string,string> _config = new ();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _spriteTexture;
    private SpriteFont _spriteFont;
    private Player _player;
    private Sprite _player2;
    private TextDisplay _textDisplay;
    private DialogueBox _dialogueBox;
    private List<Tile> _tiles = new();
    private OrthographicCamera _camera;
    private float _renderScale = 1.0f;

    public Game1()
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
        
        Window.Title = "Calliope RPG Engine";
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _spriteTexture = Content.Load<Texture2D>("Assets/basechar");
        _spriteFont = Content.Load<SpriteFont>("Assets/Fonts/TextFont");
        
        _player = new Player(_spriteTexture,150,16,16)
        {
            Position = new Vector2((float.Parse(_config["screenwidth"])/2)-16, (float.Parse(_config["screenheight"])/2)-16)*_renderScale,
            Scale = _renderScale
        };
        
        _player2 = new Sprite(_spriteTexture,1,16,16)
        {
            Position = new Vector2(0, 16)*_renderScale,
            AnimRange = new Vector2(8, 12),
            Scale = _renderScale
        };
        
        _spriteTexture = Content.Load<Texture2D>("Assets/basetiles");
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                _tiles.Add(new Tile(_spriteTexture, 0, new(16, 16))
                {
                    Position = new Vector2(i*16, j*16)*_renderScale,
                    Scale = _renderScale
                });
            }
        }
        
        _textDisplay = new TextDisplay(_spriteFont,"Text!\nAlso text...")
        {
            Position = _camera.Center,
            Centered = true,
            Scale = 1,
            Color = Color.White
        };

        _dialogueBox = new DialogueBox(_spriteFont,"I'm talking! Isn't that great? Yapping is\nseriously my favorite! Like, totes cool and\nstuff...", 40, _camera.Center,
            new Vector2(4 * _camera.BoundingRectangle.Size.Width/5,3 * _camera.BoundingRectangle.Size.Height/10));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        _player.Update(gameTime);
        _camera.Position = _player.Position - _renderScale * 
            new Vector2((float.Parse(_config["screenwidth"]) / 2), (float.Parse(_config["screenheight"]) / 2));

        _dialogueBox.Update(_camera,gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        
        // Worldspace Draw
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());

        foreach (var tile in _tiles)
        {
            tile.Draw(_spriteBatch, gameTime);
        }
        
        //_dialogueBox.Draw(_spriteBatch, gameTime);
        _player.Draw(_spriteBatch, gameTime);
        _player2.Draw(_spriteBatch, gameTime);
        
        _textDisplay.Draw(_spriteBatch, gameTime);
        _dialogueBox.Draw(_spriteBatch, gameTime);
        
        _spriteBatch.DrawCircle(new CircleF(_camera.Center,4 * _renderScale),16,Color.Red,5);
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}