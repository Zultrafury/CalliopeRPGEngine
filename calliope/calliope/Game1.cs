using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using calliope.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace calliope;

public class Game1 : Game
{
    private Dictionary<string,string> _config = new ();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _spriteTexture;
    private SpriteFont _gamerFont;
    private SpriteFont _arialFont;
    private Player _player;
    private Sprite _yapperNPC;
    private TextDisplay _textDisplay;
    private DialogueBox _dialogueBox;
    private List<Tile> _tiles = new();
    private List<Wall> _walls = new();
    private OrthographicCamera _camera;
    private float _renderScale = 1.0f;
    private InteractTriggerArea _yapperTriggerArea;
    private Dictionary<string,SoundEffect> _sfx = new();

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
        _spriteTexture = Content.Load<Texture2D>("Assets/Images/basechar");
        _gamerFont = Content.Load<SpriteFont>("Assets/Fonts/GamerFont");
        _arialFont = Content.Load<SpriteFont>("Assets/Fonts/ArialFont");
        _sfx["ding"] = Content.Load<SoundEffect>("Assets/Sounds/ding");
        _sfx["accept"] = Content.Load<SoundEffect>("Assets/Sounds/accept");
        _sfx["deny"] = Content.Load<SoundEffect>("Assets/Sounds/deny");
        
        // Players + NPCs
        
        _player = new Player(_spriteTexture,
            new Vector2((float.Parse(_config["screenwidth"])/2)-16, (float.Parse(_config["screenheight"])/2)-16)*_renderScale,
            16,16,150)
        {
            RenderScale = _renderScale,
            Camera = _camera,
            Config =  _config,
            //DrawDebugRects = true
        };

        for (int i = 0; i < 2; i++)
        {
            Follower follower = new Follower(_spriteTexture, _player.Position, new(16, 16), 150, _player, null, 
                1.5f * (float.Parse(_config["framerate"])/60))
            {
                RenderScale = _renderScale
            };
            _player.Followers.Add(follower);
        }
        
        _yapperNPC = new Sprite(_spriteTexture, new Vector2(),new (16,16), 50)
        {
            Position = new Vector2(0, 16)*_renderScale,
            AnimRange = new Vector2(8, 12),
            RenderScale = _renderScale
        };
        
        // Tiles + walls
        
        _spriteTexture = Content.Load<Texture2D>("Assets/Images/basetiles");
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                _tiles.Add(new Tile(_spriteTexture, Random.Shared.Next(3), 
                    new Vector2(i*16, j*16)*_renderScale, new(16, 16))
                {
                    RenderScale = _renderScale
                });
            }
        }
        _tiles.Add(new Tile(Sprite.GeneratePlaceholder(GraphicsDevice,32,32), 0, 
            new Vector2(8, -24)*_renderScale, new(32, 32))
        {
            RenderScale = _renderScale
        });

        for (int i = -2; i < 0; i++)
        {
            for (int j = 1; j < 31; j++)
            {
                _walls.Add(new Wall(_spriteTexture, 3,
                    new Vector2(i * 16, j * 16) * _renderScale, new(16, 16), _player));
            }
        }
        for (int j = 0; j < 32; j++)
        {
            _walls.Add(new Wall(_spriteTexture, 3,
                new Vector2(-4 * 16, j * 16) * _renderScale, new(16, 16), _player));
        }
        _walls.Add(new Wall(_spriteTexture, 3,
            new Vector2(-3 * 16, -1 * 16) * _renderScale, new(16, 16), _player));
        _walls.Add(new Wall(null, 0,
            _yapperNPC.Position, new(16, 16), _player)
        {
            //DrawDebugRects = true
        });
        
        // Dialogue box + text

        _textDisplay = new TextDisplay(_gamerFont, new Vector2(),_renderScale,"Text!\nAlso text...")
        {
            Position = _camera.Center,
            Centered = true,
            Scale = 1,
            Color = Color.White
        };

        _dialogueBox = new DialogueBox(_gamerFont, _sfx["ding"], _renderScale,
            "* I'm talking! Isn't that great? Yapping is seriously my favorite! Like, totes cool and stuff...",
            (int)DialogueBox.TextSpeeds.Normal, _camera.Center,
            new Vector2(4 * _camera.BoundingRectangle.Size.Width / 5, 0.225f * _camera.BoundingRectangle.Size.Height),
            (4 * _camera.BoundingRectangle.Size.Width) / 250)
        {
            LinkedAction = () => _dialogueBox.Initiate("* My dialogue is so freakin' epic.", () => {}, 
                _arialFont,(int)DialogueBox.TextSpeeds.Slow, false),
            CloseSoundEffect = _sfx["accept"],
            Player = _player,
            Camera = _camera,
            FreezePlayer = true
        };
        _dialogueBox.Initiate();

        // Triggers
        
        Point yapSize = (_yapperNPC.SpriteDimensions * _renderScale).ToPoint();
        Rectangle yapRect = new(_yapperNPC.Position.ToPoint()-yapSize/new Point(2,2), yapSize);
        void yapDialogue() => _dialogueBox.Initiate("* Letting me yap? Great! Nothing beats even MORE yapping! Y'know, it's my favorite thing to do and whatnot sooo... Yeah!" + 
                                                    "\nGosh, if I yapped any more, my mouth would fall off! For realsies!", () => { },
                                                    _gamerFont, (int)DialogueBox.TextSpeeds.Normal, true);

        _yapperTriggerArea = new InteractTriggerArea(yapRect, yapDialogue, _player);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        foreach (var follower in _player.Followers) follower.Update(gameTime);
        _player.Update(gameTime);
        
        foreach (var wall in _walls) wall.Update(gameTime);
        
        _dialogueBox.Update(gameTime);
        
        _yapperTriggerArea.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        
        // Worldspace Draw
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());

        foreach (var tile in _tiles) tile.Draw(_spriteBatch, gameTime);
        foreach (var wall in _walls) wall.Draw(_spriteBatch, gameTime);
        
        foreach (var follower in _player.Followers) follower.Draw(_spriteBatch, gameTime);
        _player.Draw(_spriteBatch, gameTime);
        _yapperNPC.Draw(_spriteBatch, gameTime);
        
        _textDisplay.Draw(_spriteBatch, gameTime);
        _dialogueBox.Draw(_spriteBatch, gameTime);
        
        //_spriteBatch.DrawCircle(new CircleF(_camera.Center,4 * _renderScale),16,Color.Red,5);
        _yapperTriggerArea.Draw(_spriteBatch, gameTime);
        
        _spriteBatch.End();
        
        base.Draw(gameTime);
    }
}