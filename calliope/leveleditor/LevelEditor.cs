using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace leveleditor;

public class LevelEditor : Game
{
    private Dictionary<string, string> _config = new();
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OrthographicCamera _camera;
    private SpriteFont _font;
    private float _renderscale = 10;
    private float _zoom = 10;
    private MouseState _previousMouseState;
    private KeyboardState _previousKeyboardState;
    private Vector2 _storedpos = Vector2.Zero;
    private Vector2 _destination = Vector2.Zero;
    private float _sidepanelfactor = 8;
    private (float,string) _fadingnotif = (0,"");

    public LevelEditor()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // --- CONFIG FILE ---
        var configfile = File.ReadAllLines("Content/settings");
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
        
        _renderscale = float.Parse(_config["renderscale"]);
        _sidepanelfactor = float.Parse(_config["sidepanelfactor"]);
        
        var viewportAdapter = new WindowViewportAdapter(Window, GraphicsDevice);
        _camera = new OrthographicCamera(viewportAdapter)
        {
            Position = new Vector2(0,0)
        };
        
        _graphics.ApplyChanges();
        
        Window.Title = "Calliope RPG Engine";
        Window.AllowUserResizing = true;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _font = Content.Load<SpriteFont>("Fonts/GamerFont");
    }

    protected override void Update(GameTime gameTime)
    {
        void Zoom(float factor, Vector2 pos)
        {
            _zoom *= factor;
            _zoom = float.Clamp(_zoom, 0.5f, 99.9f);
            pos = _camera.BoundingRectangle.Center + new Vector2(_zoom * _renderscale)*pos;
            pos /= (_zoom * _renderscale);
            //Console.WriteLine(pos);
            _destination = (pos*_zoom*_renderscale)-_camera.BoundingRectangle.Center;
            _camera.LookAt(_destination);
        }
        
        _camera.LookAt((Vector2.Lerp(_camera.BoundingRectangle.Center, _destination, 0.25f)));
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        int scrolldelta = Mouse.GetState().ScrollWheelValue - _previousMouseState.ScrollWheelValue;

        if (scrolldelta > 0) Zoom(1.1f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
        else if (scrolldelta < 0) Zoom(0.9f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));

        if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
        {
            Vector2 panDelta = Mouse.GetState().Position.ToVector2() - _previousMouseState.Position.ToVector2();
            _destination -= panDelta;
            //_destination = _camera.BoundingRectangle.Center;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.K)) Zoom(0.975f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
        if (Keyboard.GetState().IsKeyDown(Keys.I)) Zoom(1.025f, _camera.BoundingRectangle.Center/(_zoom * _renderscale));
        
        if (Keyboard.GetState().IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space))
        {
            _destination = new Vector2(0, 0);
            _zoom = 10;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.F) && !_previousKeyboardState.IsKeyDown(Keys.F)) _destination = _camera.ScreenToWorld(Mouse.GetState().Position.ToVector2());

        if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && !_previousKeyboardState.IsKeyDown(Keys.OemPlus) &&
            _renderscale < 15)
        {
            _renderscale+=0.5f;
            _fadingnotif = (3,"Scale: "+_renderscale);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && !_previousKeyboardState.IsKeyDown(Keys.OemMinus) &&
            _renderscale > 8f)
        {
            _renderscale-=0.5f;
            _fadingnotif = (3,"Scale: "+_renderscale);
        }
        
        _fadingnotif.Item1 = float.Max(0,_fadingnotif.Item1 - gameTime.ElapsedGameTime.Milliseconds/100f);
        //if (_fadingnotif.Item1 > 0.009) Console.WriteLine(_fadingnotif.Item1.ToString("F2"));
        
        _previousMouseState = Mouse.GetState();
        _previousKeyboardState = Keyboard.GetState();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.LightGray);
        
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), blendState:  BlendState.AlphaBlend);

        Vector2 topleft = new Vector2((_camera.BoundingRectangle.Left-_camera.Position.X%(_zoom*_renderscale))/_renderscale,
            (_camera.BoundingRectangle.Top-_camera.Position.Y%(_zoom*_renderscale))/_renderscale);
        Vector2 bottomright = new Vector2((_camera.BoundingRectangle.Right)/_renderscale,
            (_camera.BoundingRectangle.Bottom)/_renderscale);
        for (float x = topleft.X; x < bottomright.X; x+=_zoom) _spriteBatch.DrawLine(new Vector2(x*_renderscale,_camera.Position.Y), float.MaxValue,float.DegreesToRadians(90), Color.White,_renderscale/4f);
        for (float y = topleft.Y; y < bottomright.Y; y+=_zoom) _spriteBatch.DrawLine(new Vector2(_camera.Position.X, y*_renderscale), float.MaxValue,0,Color.White,_renderscale/4f);

        /*for (int i = 0; i < 10; i++)
        {
            var x = i*_zoom*_renderscale;
            var d = 0*_zoom*_renderscale;
            _spriteBatch.DrawCircle(new Vector2(d+x,d),_renderscale,16,Color.Blue,_renderscale/2);
            _spriteBatch.DrawCircle(new Vector2(d-x,d),_renderscale,16,Color.Blue,_renderscale/2);
            _spriteBatch.DrawCircle(new Vector2(d,d+x),_renderscale,16,Color.Blue,_renderscale/2);
            _spriteBatch.DrawCircle(new Vector2(d,d-x),_renderscale,16,Color.Blue,_renderscale/2);
        }*/
        
        // Side panel
        var leftside = _camera.BoundingRectangle.Left + (_camera.BoundingRectangle.Right-_camera.BoundingRectangle.Left)/_sidepanelfactor;
        
        _spriteBatch.FillRectangle(
            _camera.Position,new Vector2(_camera.BoundingRectangle.Width/_sidepanelfactor*2, _camera.BoundingRectangle.Height),
            new Color(0.25f,0.25f,0.25f,0.75f));

        var standardsize = 5/_renderscale;
        string text = "Zoom: "+(_zoom / 10f).ToString("P0");
        var fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,0);//_font.MeasureString(text).Y*33f/60);
        _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Top)-fontsize,Color.Black,
            0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
        text = "Coords: "+(_camera.BoundingRectangle.Center.X/(_zoom*_renderscale)).ToString("F1")+","
               +(_camera.BoundingRectangle.Center.Y/(_zoom*_renderscale)).ToString("F1");
        fontsize = new Vector2(_font.MeasureString(text).X*standardsize/2,_font.MeasureString(text).Y*standardsize);
        _spriteBatch.DrawString(_font,text,new Vector2(leftside,_camera.BoundingRectangle.Bottom)-fontsize,Color.Black,
            0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
        
        // Fading notification
        if (_fadingnotif.Item1 > 0.009)
        {
            standardsize = 12 / _renderscale;
            fontsize = new Vector2(_font.MeasureString(_fadingnotif.Item2).X * standardsize / 2,
                _font.MeasureString(_fadingnotif.Item2).Y * (standardsize * 33f / 60f));
            var pos = new Vector2(_camera.BoundingRectangle.Center.X,_camera.BoundingRectangle.Top+ 
                (fontsize.Y+(50*standardsize)));
            
            _spriteBatch.FillRectangle(
                pos-fontsize*1.25f, fontsize*2.5f,
                new Color(0, 0, 0, _fadingnotif.Item1));
            
            var col = new Color(_fadingnotif.Item1, _fadingnotif.Item1, _fadingnotif.Item1, _fadingnotif.Item1);
            _spriteBatch.DrawString(_font, _fadingnotif.Item2, pos - fontsize,
                col, 0, Vector2.Zero, new Vector2(standardsize), SpriteEffects.None, 0);
        }

        _spriteBatch.DrawCircle(_destination,_renderscale, 16,Color.Red,_renderscale/2);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}