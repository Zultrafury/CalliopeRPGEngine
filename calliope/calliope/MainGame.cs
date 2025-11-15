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
using Newtonsoft.Json.Serialization;

namespace calliope;

public class MainGame : Game
{
    private Dictionary<string, string> _config = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OrthographicCamera _camera;
    private float _renderScale = 1.0f;
    private bool _fullscreenPressed;

    private SceneManager _sceneManager = new();

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        EngineResources.Content = Content;
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
        _camera = new OrthographicCamera(viewportAdapter)
        {
            Position = new Vector2(0,0)
        };

        _graphics.ApplyChanges();
        
        SoundEffect.MasterVolume = float.Parse(_config["sfxvolume"]);
        MediaPlayer.Volume = float.Parse(_config["musvolume"]);
        
        Window.Title = "Calliope RPG Engine";
        Window.AllowUserResizing = true;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sceneManager.Camera = _camera;
        ICommand.SceneManager = _sceneManager;
        ICommand.Game = this;
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Assets
        // TEXTURES
        EngineResources.LoadAsset("Assets/Images/basechar");
        EngineResources.LoadAsset("Assets/Images/basetiles");
        
        // FONTS
        EngineResources.LoadAsset("Assets/Fonts/GamerFont");
        EngineResources.LoadAsset("Assets/Fonts/ArialFont");
        
        // SFX
        EngineResources.LoadAsset("Assets/Sounds/ding");
        EngineResources.LoadAsset("Assets/Sounds/accept");
        EngineResources.LoadAsset("Assets/Sounds/deny");
        
        ICommand.SceneManager.Path = _config["scenesfile"];
        
        if (false)
        {
            // -- MAIN MENU SCENE --
            BuildMainMenuScene("mainmenu");
            
            // -- GAME SCENE --
            BuildGameScene("game");

            /*var converters = EngineResources.Converters;
            converters.Add(new IGameObjectConverter());*/
            var json = JsonConvert.SerializeObject(_sceneManager.Scenes, Formatting.Indented,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = EngineResources.Converters
                });

            try
            {
                File.WriteAllText(ICommand.SceneManager.Path, json);
                Console.WriteLine($"Content successfully written to {ICommand.SceneManager.Path}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
            }
        }
        else
        {
            var scenes =
                JsonConvert.DeserializeObject<Dictionary<string, Scene>>(File.ReadAllText(ICommand.SceneManager.Path),
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        Converters = EngineResources.Converters
                    });
            _sceneManager.Scenes = scenes;
            
            Console.WriteLine($"Content successfully read from {ICommand.SceneManager.Path}");
        }

        _sceneManager.ChangeScene(_config["startupscene"]);
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
        //Console.WriteLine("Menu camera pos: " + _camera.Position);
        
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

    void BuildGameScene(string sceneName)
    {
        Scene scene = _sceneManager.Scenes[sceneName] = new() {
            Camera = _camera,
        };
        
        // Player
        Dictionary<string, Point> basecharAnimSets = new()
        {
            { "walk_up", new (4, 8) },
            { "walk_down", new (0, 4) },
            { "walk_left", new (12, 16) },
            { "walk_right", new (8, 12) }
        };
        
        var _player = new Player(EngineResources.Textures["basechar"],
            new Vector2((float.Parse(_config["screenwidth"])/2)-16, (float.Parse(_config["screenheight"])/2)-16),
            16,16,150)
        {
            Camera = scene.Camera,
            Config =  _config,
            AnimSets = basecharAnimSets
        };

        for (int i = 0; i < 2; i++)
        {
            Follower follower = new Follower(EngineResources.Textures["basechar"], _player.Position, new(16, 16), 150, null, 
                1.5f * (float.Parse(_config["framerate"])/60))
            {
                AnimSets = basecharAnimSets
            };
            _player.Followers.Add(follower);
        }
        scene.Add(_player);

        
        // YAPPER XD
        
        var _yapperNPC = new AnimatedSprite(EngineResources.Textures["basechar"], new Vector2(),new (16,16), 50)
        {
            Position = new Vector2(0, 16),
            AnimRange = new (8, 12),
        };
        
        scene.Add(_yapperNPC);
        
        // Tiles + walls
        for (int i = 0; i < 32; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                scene.Add(new Sprite(EngineResources.Textures["basetiles"],new Vector2(i*16, j*16),
                     new(16, 16), Random.Shared.Next(3))
                {
                    RenderOrder = -101
                });
            }
        }
        /*scene.Add(new Sprite(Sprite.GeneratePlaceholder(GraphicsDevice,32,32), 
            new Vector2(8, -24)*_renderScale, new(32, 32),0)
        {
            RenderScale = _renderScale,
            RenderOrder = -101
        });*/

        for (int i = -2; i < 0; i++)
        {
            for (int j = 1; j < 31; j++)
            {
                scene.Add(new Wall(EngineResources.Textures["basetiles"], new Vector2(i * 16, j * 16), 
                    new(16, 16), 3)
                {
                    RenderOrder = -100
                });
            }
        }
        for (int j = 0; j < 32; j++)
        {
            scene.Add(new Wall(EngineResources.Textures["basetiles"], new Vector2(-4 * 16, j * 16), 
                new(16, 16), 3)
            {
                RenderOrder = -100
            });
        }
        scene.Add(new Wall(EngineResources.Textures["basetiles"], new Vector2(-3 * 16, -1 * 16), 
            new(16, 16), 3)
        {
            RenderOrder = -100
        });
        scene.Add(new Wall(null, _yapperNPC.Position, 
            new(16, 16), 0)
        {
            RenderOrder = -100
        });
        
        // Dialogue box + text
        var _textDisplay = new TextDisplay(EngineResources.Fonts["GamerFont"], new Vector2(),"Text!\nAlso text...")
        {
            Position = scene.Camera.Center,
            Centered = true,
            Scale = 1,
            Color = Color.White
        };
        scene.Add(_textDisplay);

        var _dialogueBox = new DialogueBox(EngineResources.Fonts["GamerFont"], EngineResources.Sfx["ding"],
            "* I'm talking! Isn't that great? Yapping is seriously my favorite! Like, totes cool and stuff...",
            (int)DialogueBox.TextSpeeds.Normal, new Vector2(0,0),
            new Vector2(4.0f / 5.0f, 0.225f),
            4f / 250f)
        {
            CloseSoundEffect = EngineResources.Sfx["accept"],
            Player = _player,
            Camera = scene.Camera,
            FreezePlayer = true
        };
        scene.Add(_dialogueBox);
        _dialogueBox.LinkedAction = new CommandDialogueBoxInitiate(_dialogueBox.Id,"* My dialogue is so freakin' epic.", null,
            EngineResources.Fonts["ArialFont"], (int)DialogueBox.TextSpeeds.Slow, false);

        // Triggers
        
        Point yapSize = (new Vector2(_yapperNPC.SpriteDimensions.X,_yapperNPC.SpriteDimensions.Y)).ToPoint();
        Rectangle yapRect = new(_yapperNPC.Position.ToPoint()-yapSize/new Point(2,2), yapSize);
        CommandDialogueBoxInitiate yap = new(_dialogueBox.Id,"* Letting me yap? Great! Nothing beats even MORE yapping! Y'know, it's my favorite thing to do and whatnot sooo... Yeah!" + 
                                                    "\nGosh, if I yapped any more, my mouth would fall off! For realsies!", null,
            EngineResources.Fonts["GamerFont"], (int)DialogueBox.TextSpeeds.Normal, true);

        var _yapperTriggerArea = new InteractTriggerArea(yapRect, yap, _player);
        scene.Add(_yapperTriggerArea);
        
        // Menus
        // OPTIONS MENU
        List<MenuComponent> components =
        [
            new (new Vector2(-150, -160), new Vector2(100, 25), "SFX Volume", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(-170, 150), new Vector2(80, 25), "Back", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(50, -160), new Vector2(40, 25), Math.Round(SoundEffect.MasterVolume*100)+"%", EngineResources.Fonts["GamerFont"])
        ];

        Menu _optionsMenu = new Menu(components, scene.Camera);
        _optionsMenu.SetSounds(EngineResources.Sfx["ding"], EngineResources.Sfx["accept"], EngineResources.Sfx["deny"]);
        scene.Add(_optionsMenu);
        
        _optionsMenu.Components[0].AddRelation(_optionsMenu.Components[1],MenuComponent.NavDirections.Down);
        
        // STATUS MENU
        components =
        [
            new (new Vector2(-170, -160), new Vector2(80, 25), "Items", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(-170, -100), new Vector2(80, 25), "Save", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(-170, -40), new Vector2(80, 25), "Options", EngineResources.Fonts["GamerFont"])
            {
                LinkedAction = new CommandPlayerSwapMenu(_player.Id,_optionsMenu.Id,0)
            },
            new (new Vector2(-170, 150), new Vector2(80, 25), "Exit", EngineResources.Fonts["GamerFont"])
            {
                LinkedAction = new CommandSceneManagerChangeScene("mainmenu")
            }

        ];

        Menu _statusMenu = new Menu(components, scene.Camera);
        _statusMenu.SetSounds(EngineResources.Sfx["ding"], EngineResources.Sfx["accept"], EngineResources.Sfx["deny"]);
        scene.Add(_statusMenu);
        
        _statusMenu.Components[0].AddRelation(_statusMenu.Components[1],MenuComponent.NavDirections.Down);
        _statusMenu.Components[1].AddRelation(_statusMenu.Components[2],MenuComponent.NavDirections.Down);
        _statusMenu.Components[2].AddRelation(_statusMenu.Components[3],MenuComponent.NavDirections.Down);
        
        // OPTIONS MENU ACTIONS
        _optionsMenu.Components[0].LinkedAction = new CommandMenuEngage(_optionsMenu.Id,2);
        _optionsMenu.Components[1].LinkedAction = new CommandPlayerSwapMenu(_player.Id,_statusMenu.Id);
        _optionsMenu.Components[2].LinkedAction = new CommandMenuEngage(_optionsMenu.Id,0);
        _optionsMenu.Components[2].DirectionalActions = new()
        {
            {
                MenuComponent.NavDirections.Up,
                new CommandSoundEffectMasterVolumeChange(0.05f,_optionsMenu.Components[2].TextDisplay.Id,EngineResources.Sfx["accept"])
            },
            {
                MenuComponent.NavDirections.Down,
                new CommandSoundEffectMasterVolumeChange(-0.05f,_optionsMenu.Components[2].TextDisplay.Id,EngineResources.Sfx["deny"])
            }
        };
        
        // STATUS MENU REGISTRY
        scene.Get(_player.Id).ToPlayer().StatusMenu =  _statusMenu.Id;
        scene.Get(_player.Id).ToPlayer().Interacting = true;
        scene.Get(_player.Id).ToPlayer().InteractPressed = false;
        
        scene.StartAction = new CommandDialogueBoxInitiate(_dialogueBox.Id);
    }

    void BuildMainMenuScene(string sceneName)
    {
        Scene scene = _sceneManager.Scenes[sceneName] = new() { 
            Camera = _camera,
        };
        
        // Menus
        Dictionary<string,Menu> _menus = new();
        // MAIN MENU
        List<MenuComponent> components =
        [
            new (new Vector2(-185, 150), new Vector2(80, 25), "New Game", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(-45, 150), new Vector2(40, 25), "Load", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(85, 150), new Vector2(60, 25), "Options", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(215, 150), new Vector2(40, 25), "Exit", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(0, 150), new Vector2(275, 35), "", EngineResources.Fonts["GamerFont"])
            {
                RenderOrder = -1
            }
        ];

        Menu _mainMenu = new Menu(components, scene.Camera);
        _mainMenu.SetSounds(EngineResources.Sfx["ding"], EngineResources.Sfx["accept"], EngineResources.Sfx["deny"]);
        
        _menus["main"] = _mainMenu;
        scene.Add(_menus["main"]);
        
        _menus["main"].Components[0].AddRelation(_mainMenu.Components[1],MenuComponent.NavDirections.Right);
        _menus["main"].Components[1].AddRelation(_mainMenu.Components[2],MenuComponent.NavDirections.Right);
        _menus["main"].Components[2].AddRelation(_mainMenu.Components[3],MenuComponent.NavDirections.Right);
        
        // OPTIONS MENU
        components =
        [
            new (new Vector2(-150, -160), new Vector2(100, 25), "SFX Volume", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(-170, 150), new Vector2(80, 25), "Back", EngineResources.Fonts["GamerFont"]),
            new (new Vector2(50, -160), new Vector2(40, 25), Math.Round(SoundEffect.MasterVolume*100)+"%", EngineResources.Fonts["GamerFont"])
        ];
        
        Menu _optionsMenu = new Menu(components, scene.Camera);
        _optionsMenu.SetSounds(EngineResources.Sfx["ding"], EngineResources.Sfx["accept"], EngineResources.Sfx["deny"]);
        
        _menus["options"] = _optionsMenu;
        scene.Add(_menus["options"]);
            
        _menus["options"].Components[0].AddRelation(_menus["options"].Components[1],MenuComponent.NavDirections.Down);
        
        // Menu Actions
        // MAIN MENU
        _menus["main"].Components[0].LinkedAction = new CommandSceneManagerChangeScene("game");
        _menus["main"].Components[2].LinkedAction = new CommandMenuSwapTo(_menus["main"].Id,_menus["options"].Id,0);
        _menus["main"].Components[3].LinkedAction = new CommandGameExit();
        
        // OPTIONS MENU
        _menus["options"].Components[0].LinkedAction = new CommandMenuEngage(_menus["options"].Id,2);
        _menus["options"].Components[1].LinkedAction = new CommandMenuSwapTo(_menus["options"].Id,_menus["main"].Id);
        _menus["options"].Components[2].LinkedAction = new CommandMenuEngage(_menus["options"].Id,0);
        _menus["options"].Components[2].DirectionalActions = new()
        {
            {
                MenuComponent.NavDirections.Up,
                new CommandSoundEffectMasterVolumeChange(0.05f,_menus["options"].Components[2].TextDisplay.Id,EngineResources.Sfx["accept"])
            },
            {
                MenuComponent.NavDirections.Down,
                new CommandSoundEffectMasterVolumeChange(-0.05f,_menus["options"].Components[2].TextDisplay.Id,EngineResources.Sfx["deny"])
            }
        };
        
        scene.StartAction = new CommandMenuOpen(_menus["main"].Id,0);
    }
}