using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

/// <summary>
/// A sprite that is controlled by the user.
/// </summary>
public class Player : AnimatedSprite, IGameObject
{
    private float _speed = 0.05f;
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            runSpeed = Speed * 2;
        }
    }
    public bool Interacting { get; set; }
    public bool InteractPressed { get; set; }
    [JsonIgnore]
    public OrthographicCamera Camera { get; set; }
    [JsonIgnore]
    public Dictionary<string, string> Config { get; set; }
    public bool Frozen { get; set; }
    public enum MoveDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    [JsonIgnore]
    public MoveDirections Facing { get; set; }
    [JsonIgnore]
    public RectangleF CollisionArea { get; set; }
    [JsonIgnore]
    public RectangleF InteractArea { get; set; }
    [JsonIgnore]
    public bool DrawDebugRects { get; set; } = false;

    public List<Follower> Followers { get; set; } = new();
    public uint StatusMenu { get; set; }
    public uint? CurrentMenu { get; set; }

    private bool statusMenuChange;
    private float runSpeed;
    private float currentSpeed;
    [JsonConstructor]
    public Player(TextureResource spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate) : 
        base(spriteTexture, position,spriteWidth,spriteHeight, frameRate)
    {
        runSpeed = Speed * 2;
        UpdateOrder = -100f;
    }
    
    public Player(TextureResource spriteTexture, Vector2 position, Point dimensions, int  frameRate) : 
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate) {}

    public new void SceneInit(Scene scene)
    {
        base.SceneInit(scene);
        Config = Scene.Config;
        Camera = Scene.Camera;
        foreach (Follower follower in Followers) follower.RenderScale = RenderScale;
    }

    public new void Update(GameTime gameTime)
    {
        // Status Menu
        if (Keyboard.GetState().IsKeyDown(Keys.Tab) || Keyboard.GetState().IsKeyDown(Keys.C))
        {
            if (Frozen) return;
            
            if (!statusMenuChange)
            {
                if (CurrentMenu == null)
                {
                    Scene.Get(StatusMenu,false).ToMenu().Sounds["select"].SoundEffect.Play();
                    AnimIndex = AnimRange.X;
                    SwapMenu(StatusMenu,0);
                }
                else
                {
                    Scene.Get(StatusMenu,false).ToMenu().Sounds["back"].SoundEffect.Play();
                    SwapMenu(null);
                }
            }
            statusMenuChange = true;
        }
        else statusMenuChange = false;

        // No menu open - Free control
        if (CurrentMenu == null)
        {

            // Interacting
            if (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.E))
            {
                InteractPressed = (!Interacting);
                Interacting = true;
            }
            else
            {
                InteractPressed = false;
                Interacting = false;
            }

            if (!Frozen) // Movement
            {
                // Run Speed
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    currentSpeed = runSpeed;
                    FrameRate = 100;
                }
                else
                {
                    currentSpeed = Speed;
                    FrameRate = 150;
                }

                // Walking
                if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    Move(MoveDirections.Right, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    Move(MoveDirections.Left, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    Move(MoveDirections.Down, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    Move(MoveDirections.Up, gameTime);
                }
                else
                {
                    AnimIndex = 0;
                    Playing = false;
                }
            }
            else Playing = false;
        }
        else Playing = false;

        CalculateCollisionArea();

        // Center Camera
        Camera.Position = (Position * RenderScale) - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        Scene.Get(StatusMenu,false).ToMenu().Update(gameTime);

        List<AnimatedSprite> orderList =
        [
            this,
            ..Followers
        ];
        orderList.Reverse();
        float order = 0;

        foreach (AnimatedSprite sprite in orderList.OrderBy(o => o.Position.Y))
        {
            sprite.RenderOrder = order;
            order += 0.01f;
        }

        foreach (var f in Followers) f.Update(gameTime);
    }
    
    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        foreach (var f in Followers) f.Draw(spriteBatch, gameTime);
        
        //StatusMenu.Draw(spriteBatch, gameTime);
        
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(CollisionArea,Color.Red,5);
        spriteBatch.DrawRectangle(InteractArea,Color.Blue,5);
    }

    public void SwapMenu(uint? menuId, int? reselectIndex = null)
    {
        if (CurrentMenu != null) Scene.Get(CurrentMenu.Value,false).ToMenu().Close();
        CurrentMenu = menuId;
        if (CurrentMenu != null) Scene.Get(CurrentMenu.Value,false).ToMenu().Open(reselectIndex);
    }

    public void Move(MoveDirections direction, GameTime gameTime, float modifier = 1f, MoveDirections? faceOverride = null)
    {
        Playing = true;
        
        Face(faceOverride ?? direction);
        
        DragFollowers();

        float moveAmount = currentSpeed * gameTime.ElapsedGameTime.Milliseconds * modifier;
        
        switch (direction)
        {
            case MoveDirections.Up:
                Position -= new Vector2(0,moveAmount);
                break;
            case MoveDirections.Down:
                Position += new Vector2(0,moveAmount);
                break;
            case MoveDirections.Left:
                Position -= new Vector2(moveAmount,0);
                break;
            case MoveDirections.Right:
                Position += new Vector2(moveAmount,0);
                break;
        }
    }

    public void Face(MoveDirections direction)
    {
        Vector2 pos = new();
        SizeF size = new SizeF(SpriteWidth, SpriteHeight) * RenderScale;
        
        switch (direction)
        {
            case MoveDirections.Up:
                AnimRange = AnimSets["walk_up"];
                Facing = MoveDirections.Up;
                pos = new Vector2(Position.X*RenderScale - size.Width/2, Position.Y*RenderScale-size.Height);
                break;
            case MoveDirections.Down:
                AnimRange = AnimSets["walk_down"];
                Facing = MoveDirections.Down;
                pos = new Vector2(Position.X*RenderScale - size.Width/2, Position.Y*RenderScale);
                break;
            case MoveDirections.Left:
                AnimRange = AnimSets["walk_left"];
                Facing = MoveDirections.Left;
                pos = new Vector2(Position.X*RenderScale-size.Width, Position.Y*RenderScale - size.Height/2);
                break;
            case MoveDirections.Right:
                AnimRange = AnimSets["walk_right"];
                Facing = MoveDirections.Right;
                pos = new Vector2(Position.X*RenderScale, Position.Y*RenderScale - size.Height/2);
                break;
        }

        InteractArea = new RectangleF(pos, size);
    }

    public void Block()
    {
        CalculateCollisionArea();
        
        DragFollowers(false);
        
        // Center Camera
        Camera.Position = (Position * RenderScale) - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        //Reset Animation
        AnimIndex = AnimRange.X;
    }

    void CalculateCollisionArea()
    {
        Vector2 pSize = new Vector2(SpriteDimensions.X,SpriteDimensions.Y) * RenderScale;
        CollisionArea = new RectangleF((Position*RenderScale)-(pSize/new Vector2(2,2)), pSize);
    }

    void DragFollowers(bool forward = true)
    {
        if (forward)
        {
            foreach (var follower in Followers)
            {
                follower.RecordPosition(Facing,Position,FrameRate);
                follower.Drag();
            }
        }
        else
        {
            foreach (var follower in Followers)
            {
                follower.Ignore();
            }
        }
    }

    public static MoveDirections InvertMoveDirection(MoveDirections direction)
    {
        return direction switch
        {
            MoveDirections.Up => MoveDirections.Down,
            MoveDirections.Down => MoveDirections.Up,
            MoveDirections.Left => MoveDirections.Right,
            MoveDirections.Right => MoveDirections.Left,
            _ => MoveDirections.Up
        };
    }
}