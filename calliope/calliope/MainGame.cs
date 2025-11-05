using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using calliope.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Newtonsoft.Json;

namespace calliope;

public class MainGame : Game
{
    private Dictionary<string, string> _config = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OrthographicCamera _camera;
    private float _renderScale = 1.0f;
    private bool _fullscreenPressed;

    private Dictionary<string, Texture2D> _textures = new();
    private Dictionary<string, SpriteFont> _fonts = new();
    private Dictionary<string, SoundEffect> _sfx = new();

    private SceneManager _sceneManager = new();

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // --- CONFIG FILE ---
        var configfile = File.ReadAllLines("Content/config");
        foreach (var line in configfile)
        {
            _config.Add(line.Split('=')[0], line.Split('=')[1]);
        }
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
        Window.AllowUserResizing = true;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Assets
        // TEXTURES
        LoadAsset("Assets/Images/basechar");
        LoadAsset("Assets/Images/basetiles");
        
        // FONTS
        LoadAsset("Assets/Fonts/GamerFont");
        LoadAsset("Assets/Fonts/ArialFont");
        
        // SFX
        LoadAsset("Assets/Sounds/ding");
        LoadAsset("Assets/Sounds/accept");
        LoadAsset("Assets/Sounds/deny");
        
        if (true)
        {
            // -- GAME SCENE --
            BuildGameScene("game");
        
            // -- MAIN MENU SCENE --
            BuildMainMenuScene("mainmenu");
            
            var json = JsonConvert.SerializeObject(_sceneManager.Scenes, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });

            try
            {
                File.WriteAllText("jsontext.json", json);
                Console.WriteLine($"Content successfully written");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
        else
        {
            string path = "jsontext.json";
            _sceneManager.Scenes =
                JsonConvert.DeserializeObject<Dictionary<string, Scene>>(File.ReadAllText(path));
            Console.WriteLine($"Content successfully read from file: {path}");
        }

        _sceneManager.ChangeScene("mainmenu");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.F1))
        {
            if (!_fullscreenPressed)
            {
                _fullscreenPressed = true;
                int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                int scalefactor;

                if (!Window.IsBorderless)
                {
                    Window.IsBorderless = true;
                    scalefactor = screenHeight / int.Parse(_config["screenheight"]);
                    _graphics.PreferredBackBufferHeight = screenHeight;
                    _graphics.PreferredBackBufferWidth = int.Parse(_config["screenwidth"]) * scalefactor;
                    _graphics.ApplyChanges();
                }
                else
                {
                    Window.IsBorderless = false;
                    scalefactor = (screenHeight / 4 * 3) / int.Parse(_config["screenheight"]);
                    _graphics.PreferredBackBufferHeight = int.Parse(_config["screenheight"]) * scalefactor;
                    _graphics.PreferredBackBufferWidth = int.Parse(_config["screenwidth"]) * scalefactor;
                    _graphics.ApplyChanges();
                }

                Window.Position = new Point(
                    screenWidth/2 - _graphics.PreferredBackBufferWidth/2,
                    screenHeight/2 - _graphics.PreferredBackBufferHeight/2);
            }
        }
        else _fullscreenPressed = false;
        
        _sceneManager.Scene.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);
        
        // Worldspace Draw
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
        
        _sceneManager.Scene.Draw(_spriteBatch, gameTime);
        
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

    void BuildGameScene(string sceneName)
    {
        Scene scene = _sceneManager.Scenes[sceneName] = new();
        
        // Player
        Dictionary<string, Point> basecharAnimSets = new()
        {
            { "walk_up", new (4, 8) },
            { "walk_down", new (0, 4) },
            { "walk_left", new (12, 16) },
            { "walk_right", new (8, 12) }
        };
        
        var _player = new Player(_textures["basechar"],
            new Vector2((float.Parse(_config["screenwidth"])/2)-16, (float.Parse(_config["screenheight"])/2)-16)*_renderScale,
            16,16,150)
        {
            RenderScale = _renderScale,
            Camera = _camera,
            Config =  _config,
            AnimSets = basecharAnimSets
        };

        for (int i = 0; i < 2; i++)
        {
            Follower follower = new Follower(_textures["basechar"], _player.Position, new(16, 16), 150, _player, null, 
                1.5f * (float.Parse(_config["framerate"])/60))
            {
                RenderScale = _renderScale,
                AnimSets = basecharAnimSets
            };
            _player.Followers.Add(follower);
        }
        scene.Add(_player);

        
        // YAPPER XD
        
        var _yapperNPC = new AnimatedSprite(_textures["basechar"], new Vector2(),new (16,16), 50)
        {
            Position = new Vector2(0, 16)*_renderScale,
            AnimRange = new (8, 12),
            RenderScale = _renderScale
        };
        
        scene.Add(_yapperNPC);
        
        // Tiles + walls
        var _tiles = new List<Sprite>();
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                _tiles.Add(new Sprite(_textures["basetiles"],new Vector2(i*16, j*16)*_renderScale,
                     new(16, 16), Random.Shared.Next(3))
                {
                    RenderScale = _renderScale,
                    RenderOrder = -101
                });
            }
        }
        _tiles.Add(new Sprite(Sprite.GeneratePlaceholder(GraphicsDevice,32,32), 
            new Vector2(8, -24)*_renderScale, new(32, 32),0)
        {
            RenderScale = _renderScale,
            RenderOrder = -101
        });
        scene.AddRange(_tiles);

        var _walls = new List<Wall>();
        for (int i = -2; i < 0; i++)
        {
            for (int j = 1; j < 31; j++)
            {
                _walls.Add(new Wall(_textures["basetiles"], new Vector2(i * 16, j * 16) * _renderScale, 
                    new(16, 16), _player, 3)
                {
                    RenderOrder = -100
                });
            }
        }
        for (int j = 0; j < 32; j++)
        {
            _walls.Add(new Wall(_textures["basetiles"], new Vector2(-4 * 16, j * 16) * _renderScale, 
                new(16, 16), _player, 3)
            {
                RenderOrder = -100
            });
        }
        _walls.Add(new Wall(_textures["basetiles"], new Vector2(-3 * 16, -1 * 16) * _renderScale, 
            new(16, 16), _player, 3)
        {
            RenderOrder = -100
        });
        _walls.Add(new Wall(null, _yapperNPC.Position, 
            new(16, 16), _player, 0)
        {
            RenderOrder = -100
        });
        scene.AddRange(_walls);
        
        // Dialogue box + text

        var _textDisplay = new TextDisplay(_fonts["GamerFont"], new Vector2(),_renderScale,"Text!\nAlso text...")
        {
            Position = _camera.Center,
            Centered = true,
            Scale = 1,
            Color = Color.White
        };
        scene.Add(_textDisplay);

        var _dialogueBox = new DialogueBox(_fonts["GamerFont"], _sfx["ding"], _renderScale,
            "* I'm talking! Isn't that great? Yapping is seriously my favorite! Like, totes cool and stuff...",
            (int)DialogueBox.TextSpeeds.Normal, _camera.Center,
            new Vector2(4 * _camera.BoundingRectangle.Size.Width / 5, 0.225f * _camera.BoundingRectangle.Size.Height),
            (4 * _camera.BoundingRectangle.Size.Width) / 250)
        {
            CloseSoundEffect = _sfx["accept"],
            Player = _player,
            Camera = _camera,
            FreezePlayer = true
        };
        scene.Add(_dialogueBox);
        _dialogueBox.LinkedAction = new CommandDialogueBoxInitiate(_dialogueBox,"* My dialogue is so freakin' epic.", null,
            _fonts["ArialFont"], (int)DialogueBox.TextSpeeds.Slow, false);
        _dialogueBox.Initiate();

        // Triggers
        
        Point yapSize = (new Vector2(_yapperNPC.SpriteDimensions.X,_yapperNPC.SpriteDimensions.Y) * _renderScale).ToPoint();
        Rectangle yapRect = new(_yapperNPC.Position.ToPoint()-yapSize/new Point(2,2), yapSize);
        CommandDialogueBoxInitiate yap = new(_dialogueBox,"* Letting me yap? Great! Nothing beats even MORE yapping! Y'know, it's my favorite thing to do and whatnot sooo... Yeah!" + 
                                                    "\nGosh, if I yapped any more, my mouth would fall off! For realsies!", null,
            _fonts["GamerFont"], (int)DialogueBox.TextSpeeds.Normal, true);

        var _yapperTriggerArea = new InteractTriggerArea(yapRect, yap, _player);
        scene.Add(_yapperTriggerArea);
        
        // Menus
        // OPTIONS MENU
        List<MenuComponent> components =
        [
            new (new Vector2(-150, -160), new Vector2(100, 25), _renderScale, "SFX Volume", _fonts["GamerFont"]),
            new (new Vector2(-170, 150), new Vector2(80, 25), _renderScale, "Back", _fonts["GamerFont"]),
            new (new Vector2(50, -160), new Vector2(40, 25), _renderScale, Math.Round(SoundEffect.MasterVolume*100)+"%", _fonts["GamerFont"])
        ];

        Menu _optionsMenu = new Menu(components, _renderScale, _camera);
        _optionsMenu.SetSounds(_sfx["ding"], _sfx["accept"], _sfx["deny"]);
        scene.Add(_optionsMenu);
        
        _optionsMenu.Components[0].AddRelation(_optionsMenu.Components[1],MenuComponent.NavDirections.Down);
        
        // STATUS MENU
        components =
        [
            new (new Vector2(-170, -160), new Vector2(80, 25), _renderScale, "Items", _fonts["GamerFont"]),
            new (new Vector2(-170, -100), new Vector2(80, 25), _renderScale, "Save", _fonts["GamerFont"]),
            new (new Vector2(-170, -40), new Vector2(80, 25), _renderScale, "Options", _fonts["GamerFont"])
            {
                LinkedAction = new CommandPlayerSwapMenu(_player,_optionsMenu,0)
            },
            new (new Vector2(-170, 150), new Vector2(80, 25), _renderScale, "Exit", _fonts["GamerFont"])
            {
                LinkedAction = new CommandSceneManagerChangeScene(_sceneManager,"mainmenu")
            }

        ];

        Menu _statusMenu = new Menu(components, _renderScale, _camera);
        _statusMenu.SetSounds(_sfx["ding"], _sfx["accept"], _sfx["deny"]);
        scene.Add(_statusMenu);
        
        _statusMenu.Components[0].AddRelation(_statusMenu.Components[1],MenuComponent.NavDirections.Down);
        _statusMenu.Components[1].AddRelation(_statusMenu.Components[2],MenuComponent.NavDirections.Down);
        _statusMenu.Components[2].AddRelation(_statusMenu.Components[3],MenuComponent.NavDirections.Down);
        
        // OPTIONS MENU ACTIONS
        _optionsMenu.Components[0].LinkedAction = new CommandMenuEngage(_optionsMenu,2);
        _optionsMenu.Components[1].LinkedAction = new CommandPlayerSwapMenu(_player,_statusMenu);
        _optionsMenu.Components[2].LinkedAction = new CommandMenuEngage(_optionsMenu,0);
        _optionsMenu.Components[2].DirectionalActions = new()
        {
            {
                MenuComponent.NavDirections.Up,
                new CommandSoundEffectMasterVolumeChange(0.05f,_optionsMenu.Components[2].Text,_sfx["accept"])
            },
            {
                MenuComponent.NavDirections.Down,
                new CommandSoundEffectMasterVolumeChange(-0.05f,_optionsMenu.Components[2].Text,_sfx["deny"])
            }
        };
        
        // STATUS MENU REGISTRY
        scene.Get(_player.Id).ToPlayer().StatusMenu =  _statusMenu;
        scene.Get(_player.Id).ToPlayer().Interacting = true;
        scene.Get(_player.Id).ToPlayer().InteractPressed = false;
    }

    void BuildMainMenuScene(string sceneName)
    {
        Scene scene = _sceneManager.Scenes[sceneName] = new();
        
        // Menus
        Dictionary<string,Menu> _menus = new();
        // MAIN MENU
        List<MenuComponent> components =
        [
            new (new Vector2(-185, 150), new Vector2(80, 25), _renderScale, "New Game", _fonts["GamerFont"]),
            new (new Vector2(-45, 150), new Vector2(40, 25), _renderScale, "Load", _fonts["GamerFont"]),
            new (new Vector2(85, 150), new Vector2(60, 25), _renderScale, "Options", _fonts["GamerFont"]),
            new (new Vector2(215, 150), new Vector2(40, 25), _renderScale, "Exit", _fonts["GamerFont"]),
            new (new Vector2(0, 150), new Vector2(275, 35), _renderScale, "", _fonts["GamerFont"])
            {
                RenderOrder = -1
            }
        ];

        Menu _mainMenu = new Menu(components, _renderScale, _camera);
        _mainMenu.SetSounds(_sfx["ding"], _sfx["accept"], _sfx["deny"]);
        
        _menus["main"] = _mainMenu;
        scene.Add(_menus["main"]);
        
        _menus["main"].Components[0].AddRelation(_mainMenu.Components[1],MenuComponent.NavDirections.Right);
        _menus["main"].Components[1].AddRelation(_mainMenu.Components[2],MenuComponent.NavDirections.Right);
        _menus["main"].Components[2].AddRelation(_mainMenu.Components[3],MenuComponent.NavDirections.Right);
        
        // OPTIONS MENU
        components =
        [
            new (new Vector2(-150, -160), new Vector2(100, 25), _renderScale, "SFX Volume", _fonts["GamerFont"]),
            new (new Vector2(-170, 150), new Vector2(80, 25), _renderScale, "Back", _fonts["GamerFont"]),
            new (new Vector2(50, -160), new Vector2(40, 25), _renderScale, Math.Round(SoundEffect.MasterVolume*100)+"%", _fonts["GamerFont"])
        ];
        
        Menu _optionsMenu = new Menu(components, _renderScale, _camera);
        _optionsMenu.SetSounds(_sfx["ding"], _sfx["accept"], _sfx["deny"]);
        
        _menus["options"] = _optionsMenu;
        scene.Add(_menus["options"]);
            
        _menus["options"].Components[0].AddRelation(_menus["options"].Components[1],MenuComponent.NavDirections.Down);
        
        // Menu Actions
        // MAIN MENU
        _menus["main"].Components[0].LinkedAction = new CommandSceneManagerChangeScene(_sceneManager,"game");
        _menus["main"].Components[2].LinkedAction = new CommandMenuSwapTo(_menus["main"],_menus["options"],0);
        _menus["main"].Components[3].LinkedAction = new CommandGameExit(this);
        
        // OPTIONS MENU
        _menus["options"].Components[0].LinkedAction = new CommandMenuEngage(_menus["options"],2);
        _menus["options"].Components[1].LinkedAction = new CommandMenuSwapTo(_menus["options"],_menus["main"]);
        _menus["options"].Components[2].LinkedAction = new CommandMenuEngage(_menus["options"],0);
        _menus["options"].Components[2].DirectionalActions = new()
        {
            {
                MenuComponent.NavDirections.Up,
                new CommandSoundEffectMasterVolumeChange(0.05f,_menus["options"].Components[2].Text,_sfx["accept"])
            },
            {
                MenuComponent.NavDirections.Down,
                new CommandSoundEffectMasterVolumeChange(-0.05f,_menus["options"].Components[2].Text,_sfx["deny"])
            }
        };
        
        scene.StartAction = new CommandMenuOpen(_menus["main"],0);
    }
}