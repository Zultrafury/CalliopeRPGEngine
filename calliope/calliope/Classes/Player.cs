using System.Collections.Generic;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace calliope.Classes;

/// <summary>
/// A sprite that is controlled by the user.
/// </summary>
public class Player : Sprite, IUpdateDraw
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
    public OrthographicCamera Camera { get; set; }
    public Dictionary<string, string> Config { get; set; }
    public bool Frozen { get; set; }
    public enum MoveDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    public MoveDirections Facing { get; set; }
    public RectangleF CollisionArea { get; set; }
    public RectangleF InteractArea { get; set; }
    public bool DrawDebugRects { get; set; } = false;
    public List<Follower> Followers { get; set; } = new();
    public Menu StatusMenu { get; set; }

    private bool statusMenuChange = false;
    private float runSpeed;
    private float currentSpeed;
    
    public Player(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate) : 
        base(spriteTexture, position,spriteWidth,spriteHeight, frameRate)
    {
        runSpeed = Speed * 2;
        Face(MoveDirections.Down);
    }
    
    public Player(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int  frameRate) : 
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate) {}

    public new void Update(GameTime gameTime)
    {
        // Status Menu
        if (Keyboard.GetState().IsKeyDown(Keys.Tab) || Keyboard.GetState().IsKeyDown(Keys.C))
        {
            if (Frozen) return;
            
            if (!statusMenuChange)
            {
                StatusMenu.Active = !StatusMenu.Active;
                
                if (StatusMenu.Active) StatusMenu.Sounds["select"].Play();
                else StatusMenu.Sounds["back"].Play();
            }
            statusMenuChange = true;
        }
        else statusMenuChange = false;

        if (!StatusMenu.Active)
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

        CalculateCollisionArea();

        // Center Camera
        Camera.Position = Position - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        StatusMenu.Update(gameTime);
    }
    
    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        
        StatusMenu.Draw(spriteBatch, gameTime);
        
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(CollisionArea,Color.Red,5);
        spriteBatch.DrawRectangle(InteractArea,Color.Blue,5);
    }

    public void Move(MoveDirections direction, GameTime gameTime, float modifier = 1f, MoveDirections? faceOverride = null)
    {
        Playing = true;
        
        Face(faceOverride ?? direction);
        
        DragFollowers();

        float moveAmount = currentSpeed * gameTime.ElapsedGameTime.Milliseconds * modifier * RenderScale;
        
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
                AnimRange = new Vector2(4, 8);
                Facing = MoveDirections.Up;
                pos = new Vector2(Position.X - size.Width/2, Position.Y-size.Height);
                break;
            case MoveDirections.Down:
                AnimRange = new Vector2(0, 4);
                Facing = MoveDirections.Down;
                pos = new Vector2(Position.X - size.Width/2, Position.Y);
                break;
            case MoveDirections.Left:
                AnimRange = new Vector2(12, 16);
                Facing = MoveDirections.Left;
                pos = new Vector2(Position.X-size.Width, Position.Y - size.Height/2);
                break;
            case MoveDirections.Right:
                AnimRange = new Vector2(8, 12);
                Facing = MoveDirections.Right;
                pos = new Vector2(Position.X, Position.Y - size.Height/2);
                break;
        }

        InteractArea = new RectangleF(pos, size);
    }

    public void Block()
    {
        CalculateCollisionArea();
        
        DragFollowers(false);
        
        // Center Camera
        Camera.Position = Position - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        //Reset Animation
        AnimIndex = (int)AnimRange.X;
    }

    void CalculateCollisionArea()
    {
        Point pSize = (SpriteDimensions * RenderScale).ToPoint();
        CollisionArea = new Rectangle(Position.ToPoint()-(pSize/new Point(2,2)), pSize);
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