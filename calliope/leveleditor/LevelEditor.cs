using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using calliope.Classes;
using leveleditor.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Newtonsoft.Json;

namespace leveleditor;

public class LevelEditor : Game
{
    private Dictionary<string, string> _settings = new();
    private Dictionary<string, string> _config = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OrthographicCamera _camera;
    private SpriteFont _font;
    private float _graphopacity = 0.5f;
    private float _renderscale = 10;
    private float _zoom = 10;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private Vector2 _storedpos = Vector2.Zero;
    private float _timesincelastrescale = 0;
    private Vector2 _destination = Vector2.Zero;
    private float _sidepanelfactor = 8;
    private (float,string,float) _fadingnotif = (0,"",0);
    private SceneManager _sceneManager = new();
    private List<IGameObject> _sampleObjects = new();
    private int _selectionIndex = 0;

    private IGameObject _selectedGameObject => _sampleObjects[_selectionIndex];
    private Dictionary<PropertyInfo,TextEntryField> _propertyfields = new();
    private List<Button> _buttons = new();
    private Dictionary<string,Button> _selectionbuttons = new();
    private List<TextEntryField> _textfields = new();
    private TextEntryField _currentfield = null;
    private Process runtimeTest = null;

    public LevelEditor()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // --- SETTINGS FILE ---
        var configfile = File.ReadAllLines("Content/settings");
        foreach (var line in configfile)
        {
            _settings.Add(line.Split('=')[0], line.Split('=')[1]);
        }
        
        // --- CONFIG FILE ---
        configfile = File.ReadAllLines("Content/config");
        foreach (var line in configfile)
        {
            _config.Add(line.Split('=')[0], line.Split('=')[1]);
        }
    }

    protected override void Initialize()
    {
        float screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * (3f/4);
        float screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * (3f/4);

        _graphics.PreferredBackBufferHeight = (int)screenHeight;
        _graphics.PreferredBackBufferWidth = (int)screenWidth;
        
        _renderscale = float.Parse(_settings["renderscale"]);
        _sidepanelfactor = float.Parse(_settings["sidepanelfactor"]);
        
        var viewportAdapter = new WindowViewportAdapter(Window, GraphicsDevice);
        _camera = new OrthographicCamera(viewportAdapter)
        {
            Position = new Vector2(0,0)
        };
        
        _graphics.ApplyChanges();
        
        Window.Title = "Calliope RPG Engine";
        Window.AllowUserResizing = true;

        EngineResources.Content = Content;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sceneManager.Camera = _camera;
        ICommand.SceneManager = _sceneManager;
        ICommand.Game = this;
        ICommand.SceneManager.Path = _settings["scenesfile"];
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Assets
        // TEXTURES
        EngineResources.LoadAsset("Assets/Images/basechar");
        EngineResources.LoadAsset("Assets/Images/basetiles");
        
        // FONTS
        EngineResources.LoadAsset("Assets/Fonts/GamerFont");
        EngineResources.LoadAsset("Assets/Fonts/ArialFont");
        _font = EngineResources.Fonts["GamerFont"].Font;
        
        // SFX
        EngineResources.LoadAsset("Assets/Sounds/ding");
        EngineResources.LoadAsset("Assets/Sounds/accept");
        EngineResources.LoadAsset("Assets/Sounds/deny");

        /*string text = "My textfield!";
        var textfield = new TextEntryField(-(_font.MeasureString(text)/2)*(5/_renderscale), _font, _renderscale, true);
        textfield.Decorate(text, () => { Console.WriteLine(textfield.Text); });
        _textfields.Add(textfield);*/
        
        // Load on startup
        if (bool.Parse(_settings["loadfile"]))
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
            _fadingnotif = (20,$"Content successfully read from\n{ICommand.SceneManager.Path}",4);

            _sceneManager.ChangeScene(_sceneManager.Scenes.Last().Key);
        }
        else
        {
            _sceneManager.AddScene("mainmenu",new Scene());
            _sceneManager.CurrentScene = "mainmenu";
        }
        
        // Sample Objects
        var sprite = new Sprite(EngineResources.Textures["basetiles"], Vector2.Zero, new Point(16), 1)
        {
            RenderOrder = -100
        };
        _sampleObjects.Add(sprite);

        var wall = new Wall(EngineResources.Textures["basetiles"], Vector2.Zero, new(16, 16), 3)
        {
            RenderOrder = -100
        };
        _sampleObjects.Add(wall);
        
        var animatedsprite = new AnimatedSprite(EngineResources.Textures["basechar"], Vector2.Zero, new (16,16), 150)
        {
            AnimRange = new (0, 4)
        };
        _sampleObjects.Add(animatedsprite);
        
        Dictionary<string, Point> basecharAnimSets = new()
        {
            { "walk_up", new (4, 8) },
            { "walk_down", new (0, 4) },
            { "walk_left", new (12, 16) },
            { "walk_right", new (8, 12) }
        };
        var player = new Player(EngineResources.Textures["basechar"], Vector2.Zero,
            16,16,150)
        {
            Camera = _sceneManager.Scene.Camera,
            Config =  _config,
            AnimSets = basecharAnimSets
        };
        _sampleObjects.Add(player);
        
        foreach (var o in _sampleObjects) o.RenderScale = _renderscale;
        _selectionIndex = 0;
        
        // Selection Buttons
        var button = new Button(new (0, 0), _font, _renderscale);
        button.Decorate(" < ",3);
        button.OnClick = () =>
        {
            _selectionIndex--;
            if (_selectionIndex < 0) _selectionIndex = _sampleObjects.Count - 1;
            PopulateSidePanel();
            ResizeAll();
        };
        _buttons.Add(button);
        _selectionbuttons["left"] = button;
        
        button = new Button(new (0, 0), _font, _renderscale);
        button.Decorate(" > ",3);
        button.OnClick = () =>
        {
            _selectionIndex++;
            if (_selectionIndex >= _sampleObjects.Count) _selectionIndex = 0;
            PopulateSidePanel();
            ResizeAll();
        };
        _buttons.Add(button);
        _selectionbuttons["right"] = button;
        
        button = new Button(new Vector2(1,  0),   _font, _renderscale);
        button.Decorate("Launch Runtime",3);
        button.OnClick = () =>
        {
            if (runtimeTest == null || runtimeTest.HasExited)
            {
                runtimeTest = Process.Start("calliope.exe");
            }
            else
            {
                _fadingnotif = (5,"Runtime already launched!",6);
            }
        };
        _buttons.Add(button);
        _selectionbuttons["launch"] = button;

        PopulateSidePanel();
        ResizeAll();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) return;
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        void CameraControlsUpdate() {
            void Zoom(float factor, Vector2 pos)
            {
                _zoom *= factor;
                _zoom = float.Clamp(_zoom, 0.1f, 99.9f);
                pos = _camera.BoundingRectangle.Center + new Vector2(_zoom * _renderscale)*pos;
                pos /= (_zoom * _renderscale);
                //Console.WriteLine(pos);
                _destination = (pos*_zoom*_renderscale)-_camera.BoundingRectangle.Center;
                _camera.LookAt(_destination);
                ResizeAll();
            }
            
            _camera.LookAt((Vector2.Lerp(_camera.BoundingRectangle.Center, _destination, 0.25f)));

            int scrolldelta = Mouse.GetState().ScrollWheelValue - _previousMouseState.ScrollWheelValue;

            if (scrolldelta > 0) Zoom(1.1f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
            else if (scrolldelta < 0) Zoom(0.9f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));

            if (Mouse.GetState().MiddleButton == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.G))
            {
                Vector2 panDelta = Mouse.GetState().Position.ToVector2() - _previousMouseState.Position.ToVector2();
                _destination -= panDelta;
                //_destination = _camera.BoundingRectangle.Center;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.K)) Zoom(0.975f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
            if (Keyboard.GetState().IsKeyDown(Keys.I)) Zoom(1.025f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
            
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
            {
                if (_destination == Vector2.Zero)
                {
                    _zoom = (1000/(_renderscale*_renderscale));
                    ResizeAll();
                }
                _destination = new Vector2(0, 0);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F))
            {
                _destination = _camera.ScreenToWorld(Mouse.GetState().Position.ToVector2());
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && _timesincelastrescale == 0 && _renderscale < 50)
            {
                _timesincelastrescale = 0.1f;
                _renderscale+=0.5f;
                _zoom = (1000 / (_renderscale * _renderscale));
                _fadingnotif = (3,"Scale: "+_renderscale,12);
                ResizeAll();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && _timesincelastrescale == 0 && _renderscale > 1)
            {
                _timesincelastrescale = 0.1f;
                _renderscale-=0.5f;
                _zoom = (1000 / (_renderscale * _renderscale));
                _fadingnotif = (3,"Scale: "+_renderscale,12);
                ResizeAll();
            }

            foreach (Button button in _buttons) button.Clicked = false;
        }
                    
        void ProcessMouseClick() {
            if (!MouseInBounds()) return;

            if (Keyboard.GetState().IsKeyDown(Keys.S) && !_previousKeyboardState.IsKeyDown(Keys.S) &&
                Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
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
                    _fadingnotif = (20,$"Content successfully written to\n{ICommand.SceneManager.Path}",4);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error writing to file: {ex.Message}");
                }
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                PlaceSprite(_selectedGameObject);
            }
            else if (Mouse.GetState().LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                foreach (TextEntryField textfield in _textfields)
                {
                    if (!textfield.Bounds.Contains(_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2()))) continue;
                    
                    _currentfield = textfield;
                    _currentfield.Clicked = true;
                    return;
                }

                foreach (Button button in _buttons)
                {
                    if (!button.Bounds.Contains(_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2()))) continue;
                    
                    button.Click();
                    return;
                }

                PlaceSprite(_selectedGameObject);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                RemoveSprite();
            }
            else if (Mouse.GetState().RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released)
            {
                RemoveSprite();
            }
        }
        
        // Text editing mode
        if (_currentfield == null)
        {
            // Camera controls
            CameraControlsUpdate();
            
            // Mouse registration
            ProcessMouseClick();
        }
        else
        {
            // Text edit mode logic
            void TextEditMode()
            {
                void SwapBack()
                {
                    _currentfield.Clicked = false;
                    _currentfield.Submit();
                    _currentfield.Resize();
                    _currentfield = null;
                }

                if (Mouse.GetState().LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_currentfield.Bounds.Contains(_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2())))
                    {
                        return;
                    }
                    SwapBack();
                    foreach (TextEntryField textfield in _textfields)
                    {
                        if (textfield.Bounds.Contains(_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2())))
                        {
                            _currentfield = textfield;
                            _currentfield.Clicked = true;
                            return;
                        }
                    }
                }

                var keyboardState = Keyboard.GetState();
                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (key is Keys.LeftShift or Keys.RightShift) continue;
                    if (keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key))
                    {
                        if (key is Keys.Enter)
                        {
                            SwapBack();
                            break;
                        }

                        if (key is Keys.Back)
                        {
                            _currentfield.Text = _currentfield.Text.Remove(_currentfield.Text.Length - 1);
                            if (_currentfield.Text.Length < 1) _currentfield.Text = " ";
                            _currentfield.Resize();
                            break;
                        }

                        bool shiftdown = keyboardState.IsKeyDown(Keys.LeftShift) ||
                                         keyboardState.IsKeyDown(Keys.RightShift);
                        if (_currentfield.Text == " ") _currentfield.Text = "";
                        var c = CharFromKey(key, shiftdown).ToString();
                        if (c != "\0") _currentfield.Text += c;
                        _currentfield.Resize();
                    }
                }
            }
            TextEditMode();
        }
        
        _timesincelastrescale = float.Max(0,_timesincelastrescale - gameTime.ElapsedGameTime.Milliseconds / 1000f);
        //if (_timesincelastrescale > 0.009) Console.WriteLine(_timesincelastrescale.ToString("F2"));
        
        _fadingnotif.Item1 = float.Max(0,_fadingnotif.Item1 - gameTime.ElapsedGameTime.Milliseconds/100f);
        //if (_fadingnotif.Item1 > 0.009) Console.WriteLine(_fadingnotif.Item1.ToString("F2"));
        
        _previousMouseState = Mouse.GetState();
        _previousKeyboardState = Keyboard.GetState();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!IsActive) return;

        GraphicsDevice.Clear(Color.LightGray);
        
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), blendState:  BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

        _sceneManager.Scene.Draw(_spriteBatch, gameTime);

        // Graph
        void DrawGraph()
        {
            var linecolor = new Color(_graphopacity, _graphopacity, _graphopacity, _graphopacity);
            Vector2 topleft = new Vector2(
                (_camera.BoundingRectangle.Left - _camera.Position.X % (_zoom * _renderscale)) / _renderscale,
                (_camera.BoundingRectangle.Top - _camera.Position.Y % (_zoom * _renderscale)) / _renderscale);
            Vector2 bottomright = new Vector2(
                (_camera.BoundingRectangle.Right) / _renderscale,
                (_camera.BoundingRectangle.Bottom) / _renderscale);
            float halfway = (_zoom) / 2;
            for (float x = topleft.X-halfway; x < bottomright.X; x += _zoom)
                _spriteBatch.DrawLine(new Vector2(x * _renderscale, _camera.Position.Y), float.MaxValue,
                    float.DegreesToRadians(90), linecolor, 25/_renderscale);
            for (float y = topleft.Y-halfway; y < bottomright.Y; y += _zoom)
                _spriteBatch.DrawLine(new Vector2(_camera.Position.X, y * _renderscale), float.MaxValue, 
                    0, linecolor, 25/_renderscale);
        }
        DrawGraph();

        // Side panel
        void DrawSidePanel() {
            var standardsize = 5/_renderscale;

            var leftside = _camera.BoundingRectangle.Left + (_camera.BoundingRectangle.Right-_camera.BoundingRectangle.Left)/_sidepanelfactor;
            
            // Side panel fill
            _spriteBatch.FillRectangle(
                _camera.Position,new Vector2(_camera.BoundingRectangle.Width/_sidepanelfactor*2, _camera.BoundingRectangle.Height),
                new Color(0.25f,0.25f,0.25f,0.75f));

            // Zoom text
            string text = "Zoom: "+(_zoom / (1000/(_renderscale*_renderscale))).ToString("P0");
            var fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,0);//_font.MeasureString(text).Y*33f/60);
            _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Top)-fontsize,Color.Black,
                0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
            
            // Type text
            text = "Type: "+_selectedGameObject.GetType().Name;
            float starting = _font.MeasureString(text).Y * -standardsize * 1.5f;
            standardsize = 3/_renderscale;
            fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,starting);
            _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Top)-fontsize,Color.Black,
                0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
            
            // Selection buttons
            starting += _font.MeasureString(text).Y * -standardsize * 1.5f;
            float soffset = _font.MeasureString(" < ").X*standardsize;
            _selectionbuttons["left"].Position = new(leftside-soffset*1.75f,_camera.BoundingRectangle.Top-starting);
            _selectionbuttons["right"].Position = new(leftside+soffset/2,_camera.BoundingRectangle.Top-starting);
            _selectionbuttons["launch"].Position = new(_camera.BoundingRectangle.Right,_camera.BoundingRectangle.Top);
            
            // Properties
            starting += _font.MeasureString(text).Y * -standardsize * 1.5f;
            int i = 0;
            void DrawProperty()
            {
                fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,(_font.MeasureString(text).Y*-(standardsize*i*1.25f))+starting);
                _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Top)-fontsize,Color.Black,
                    0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
                i++;
            }
            foreach (var property in _propertyfields)
            {
                text = property.Key.Name + ":";
                DrawProperty();

                text = property.Value.Text;
                fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,(_font.MeasureString(text).Y*-(standardsize*i*1.25f))+starting);
                property.Value.Position = new Vector2(leftside, _camera.BoundingRectangle.Top) - fontsize;
                i++;
            }
            
            // Coords text
            standardsize = 5/_renderscale;
            text = "Coords: "+(_camera.BoundingRectangle.Center.X/(_zoom*_renderscale)).ToString("F1")+","
                   +(_camera.BoundingRectangle.Center.Y/(_zoom*_renderscale)).ToString("F1");
            fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,_font.MeasureString(text).Y*standardsize);
            _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Bottom)-fontsize,Color.Black,
                0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
            
            // Selected object
            void DisplaySelectedGameObject()
            {
                _selectedGameObject.RenderScale = 80/_renderscale;
                switch (_selectedGameObject)
                {
                    case Sprite sprite:
                    {
                        Vector2 center = new (_camera.BoundingRectangle.Center.X,_camera.BoundingRectangle.Bottom-fontsize.Y
                            -(sprite.SpriteHeight*(40/_renderscale)));
                        sprite.Position = (center-new Vector2((_camera.BoundingRectangle.Width/_sidepanelfactor) 
                                            *((_sidepanelfactor/2)-1),0)) 
                                            /(80/_renderscale);
                        /*sprite.Position = ((_camera.BoundingRectangle.Center-new Vector2((_camera.BoundingRectangle.Width/_sidepanelfactor)
                                               *((_sidepanelfactor/2)-1),0))
                                           *sprite.SpriteWidth*(80/_renderscale))
                                          /(80/_renderscale*_zoom);*/
                        //Console.WriteLine(sprite.Position.ToNumerics()+" : "+_camera.BoundingRectangle.Center.ToNumerics());
                        break;
                    }
                    case AnimatedSprite animatedSprite:
                    {
                        Vector2 center = new (_camera.BoundingRectangle.Center.X,_camera.BoundingRectangle.Bottom-fontsize.Y
                            -(animatedSprite.SpriteHeight*(40/_renderscale)));
                        animatedSprite.Position = (center-new Vector2((_camera.BoundingRectangle.Width/_sidepanelfactor) 
                                                              *((_sidepanelfactor/2)-1),0)) 
                                          /(80/_renderscale);
                        break;
                    }
                }
                _selectedGameObject.Draw(_spriteBatch, gameTime);
            }
            DisplaySelectedGameObject();
        }
        DrawSidePanel();

        // Buttons + Text entry fields
        foreach (var b in _buttons)
        {
            if (b.SnapToCamera) b.Position = _camera.BoundingRectangle.Center + b.Offset;
            b.Draw(_spriteBatch);
        }
        foreach (var t in _textfields)
        {
            if (t.SnapToCamera) t.Position = _camera.BoundingRectangle.Center + t.Offset;
            t.Draw(_spriteBatch);
        }
        
        // Fading notification
        void DrawFadingNotification()
        {
            if (_fadingnotif.Item1 > 0.009)
            {
                var standardsize = _fadingnotif.Item3 / _renderscale;
                var fontsize = new Vector2(_font.MeasureString(_fadingnotif.Item2).X * standardsize / 2,
                    _font.MeasureString(_fadingnotif.Item2).Y * (standardsize * 33f / 60f));
                var pos = new Vector2(_camera.BoundingRectangle.Center.X, _camera.BoundingRectangle.Top +
                                                                          (fontsize.Y + (50 * standardsize)));

                _spriteBatch.FillRectangle(
                    pos - fontsize * 1.25f, fontsize * 2.5f,
                    new Color(0, 0, 0, _fadingnotif.Item1));

                var col = new Color(_fadingnotif.Item1, _fadingnotif.Item1, _fadingnotif.Item1, _fadingnotif.Item1);
                _spriteBatch.DrawString(_font, _fadingnotif.Item2, pos - fontsize,
                    col, 0, Vector2.Zero, new Vector2(standardsize), SpriteEffects.None, 0);
            }
        }
        DrawFadingNotification();

        _spriteBatch.DrawCircle(_destination,100/_renderscale, 16,Color.Red,40/_renderscale);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    void PopulateSidePanel()
    {
        foreach (var property in _propertyfields) _textfields.Remove(property.Value);
        _propertyfields.Clear();
        
        var properties = _selectedGameObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            string[] gameobjectproperties = ["Position","Id","UpdateOrder","CurrentMenu","StatusMenu","AnimSets","Followers"];
            if (gameobjectproperties.Contains(property.Name)) continue;
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
            
            var textfield = new TextEntryField(new(0,0), _font, _renderscale);
            string text;
            
            if (property.PropertyType.IsSubclassOf(typeof(EngineResource)) && property.GetValue(_selectedGameObject) is EngineResource res)
            {
                text = res.Path;
            }
            else if (property.PropertyType == typeof(Point) && property.GetValue(_selectedGameObject) is Point pnt)
            {
                text = pnt.X+","+pnt.Y;
            }
            else
            {
                text = property.GetValue(_selectedGameObject)!.ToString();
            }

            void RegisterChanges()
            {
                try
                {
                    if (property.PropertyType.IsSubclassOf(typeof(EngineResource)))
                    {
                        int inter = textfield.Text.LastIndexOf('/')+1;
                        string field = textfield.Text[inter..];

                        //Console.WriteLine(EngineResources.Textures[field].Path);

                        EngineResource resource = property.PropertyType.Name switch
                        {
                            nameof(SpriteFontResource) => EngineResources.Fonts[field],
                            nameof(TextureResource) => EngineResources.Textures[field],
                            nameof(SoundEffectResource) => EngineResources.Sfx[field],
                            _ => null
                        };

                        property.SetValue(_selectedGameObject,resource);
                        textfield.OldText = textfield.Text;
                    }
                    else if (property.PropertyType == typeof(Point))
                    {
                        var point = new Point();
                        int inter = textfield.Text.IndexOf(',');
                        point.X = int.Parse(textfield.Text[..inter]);
                        point.Y = int.Parse(textfield.Text[(inter+1)..]);
                        
                        property.SetValue(_selectedGameObject,point);
                        textfield.OldText = textfield.Text;
                    }
                    
                    var t = Convert.ChangeType(textfield.Text,property.PropertyType);
                    property.SetValue(_selectedGameObject, t);
                    textfield.OldText = textfield.Text;
                }
                catch (Exception e)
                {
                    textfield.Text = textfield.OldText;
                }
                //Console.WriteLine(property.Name+": "+property.GetValue(_selectedGameObject));
                //Console.WriteLine(property.Name+": "+textfield.OldText);
            }
            
            textfield.Decorate(text,RegisterChanges,3);
            
            _propertyfields.Add(property,textfield);
            _textfields.Add(textfield);
        }
    }

    void ResizeAll()
    {
        ResizeGameObject(_selectedGameObject);
        
        foreach (var o in _sceneManager.Scene.Objects) ResizeGameObject(o);
        
        foreach (var o in _sceneManager.Scene.StaticObjects.Objects) ResizeGameObject(o);

        foreach (var b in _buttons) b.Resize(_renderscale);
        
        foreach (var t in _textfields) t.Resize(_renderscale);
    }

    void ResizeGameObject(IGameObject obj)
    {
        switch (obj)
        {
            case Sprite sprite:
            {
                sprite.RenderScale = _renderscale * _zoom / sprite.SpriteWidth;
                break;
            }
            case AnimatedSprite animatedSprite:
            {
                animatedSprite.RenderScale = _renderscale * _zoom / animatedSprite.SpriteWidth;
                if (animatedSprite.GetType().Name == nameof(Player))
                {
                    var p = (Player)animatedSprite;
                    foreach (var f in p.Followers)
                    {
                        ResizeGameObject(f);
                    }
                }
                break;
            }
            case DialogueBox dialogueBox:
            {
                dialogueBox.Hidden = true;
                break;
            }
        }
    }
    
    void PlaceSprite(IGameObject obj)
    {
        Vector2 placementpos = Vector2.Round(
            (_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2()))
            / (_renderscale * _zoom));

        foreach (var o in _sceneManager.Scene.StaticObjects.Objects)
        {
            if (o is Sprite s)
            {
                if (Vector2.Distance(s.Position/s.SpriteWidth, placementpos) > 0.5f) continue;
                if (Math.Abs(s.RenderOrder - _selectedGameObject.RenderOrder) > 1) continue;
                _fadingnotif = (20,$"Overlap at {placementpos}",4);
                return;
            }
            
            if (o is AnimatedSprite a)
            {
                if (Vector2.Distance(a.Position/a.SpriteWidth, placementpos) > 0.5f) continue;
                if (Math.Abs(a.RenderOrder - _selectedGameObject.RenderOrder) > 1) continue;
                _fadingnotif = (20,$"Overlap at {placementpos}",4);
                return;
            }
        }

        var clone = obj.Clone();
        switch (clone)
        {
            case Sprite sprite:
                sprite.Position = placementpos * sprite.SpriteWidth;
                ResizeGameObject(sprite);
                _sceneManager.Scene.StaticObjects.Add(sprite);
                break;
            case AnimatedSprite animatedSprite:
                animatedSprite.Position = placementpos * animatedSprite.SpriteWidth;
                ResizeGameObject(animatedSprite);
                _sceneManager.Scene.StaticObjects.Add(animatedSprite);
                break;
        }
    }

    void RemoveSprite()
    {
        var placementpos = Vector2.Round(
            (_camera.ScreenToWorld(Mouse.GetState().Position.ToVector2()))
            / (_renderscale * _zoom));

        foreach (var o in _sceneManager.Scene.Objects)
        {
            if (o is Player p)
            {
                if (Vector2.Distance(p.Position / p.SpriteWidth, placementpos) < 0.5f)
                {
                    _sceneManager.Scene.Objects.Remove(p);
                    return;
                }
            }
        }
        
        foreach (var o in _sceneManager.Scene.StaticObjects.Objects)//.OrderBy(x => x.RenderOrder).Reverse())
        {
            if (o.GetType().Name == "Sprite" && o is Sprite s)
            {
                if (Vector2.Distance(s.Position / s.SpriteWidth, placementpos) < 0.5f)
                {
                    //Console.WriteLine(s.Position+", "+placementpos);
                    _sceneManager.Scene.StaticObjects.Sprites.Remove(s);
                    return;
                }
            }

            else if (o.GetType().Name == "AnimatedSprite" && o is AnimatedSprite a)
            {
                if (Vector2.Distance(a.Position / a.SpriteWidth, placementpos) < 0.5f)
                {
                    _sceneManager.Scene.StaticObjects.AnimatedSprites.Remove(a);
                    return;
                }
            }
            
            else if (o.GetType().Name == "Wall" && o is Wall w)
            {
                if (Vector2.Distance(w.Position / w.SpriteWidth, placementpos) < 0.5f)
                {
                    _sceneManager.Scene.StaticObjects.Walls.Remove(w);
                    return;
                }
            }
        }
    }

    private char CharFromKey(Keys Key, bool Shift = false)
    {
        if (Key == Keys.Space) return ' ';
        
        string s = Key.ToString();

        if (s.Length == 1)
        {
            char c = char.Parse(s);
            byte b = Convert.ToByte(c);

            if (b is >= 65 and <= 90 or >= 97 and <= 122)
            {
                return (!Shift ? c.ToString().ToLower() : c.ToString())[0];
            }
        }

        return Key switch
        {
            Keys.D0 => Shift ? ')' : '0',
            Keys.D1 => Shift ? '!' : '1',
            Keys.D2 => Shift ? '@' : '2',
            Keys.D3 => Shift ? '#' : '3',
            Keys.D4 => Shift ? '$' : '4',
            Keys.D5 => Shift ? '%' : '5',
            Keys.D6 => Shift ? '^' : '6',
            Keys.D7 => Shift ? '&' : '7',
            Keys.D8 => Shift ? '*' : '8',
            Keys.D9 => Shift ? '(' : '9',
            Keys.OemTilde => Shift ? '~' : '`',
            Keys.OemSemicolon => Shift ? ':' : ';',
            Keys.OemQuotes => Shift ? '"' : '\'',
            Keys.OemQuestion => Shift ? '?' : '/',
            Keys.OemPlus => Shift ? '+' : '=',
            Keys.OemPipe => Shift ? '|' : '\\',
            Keys.OemPeriod => Shift ? '>' : '.',
            Keys.OemOpenBrackets => Shift ? '{' : '[',
            Keys.OemCloseBrackets => Shift ? '}' : ']',
            Keys.OemMinus => Shift ? '_' : '-',
            Keys.OemComma => Shift ? '<' : ',',
            Keys.NumPad0 => '0',
            Keys.NumPad1 => '1',
            Keys.NumPad2 => '2',
            Keys.NumPad3 => '3',
            Keys.NumPad4 => '4',
            Keys.NumPad5 => '5',
            Keys.NumPad6 => '6',
            Keys.NumPad7 => '7',
            Keys.NumPad8 => '8',
            Keys.NumPad9 => '9',
            _ => '\0'
        };
    }

    bool MouseInBounds() => GraphicsDevice.Viewport.Bounds.Contains(Mouse.GetState().Position);
}