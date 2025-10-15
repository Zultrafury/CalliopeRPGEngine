using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace calliope.Classes;

/// <summary>
/// A sprite that is controlled by the user.
/// </summary>
public class Player : Sprite
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
    public bool Frozen { get; set; }
    public enum MoveDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    private float runSpeed;
    private float currentSpeed;
    private Vector2 lastPosition;
    
    public Player(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight) : base(spriteTexture,frameRate,spriteWidth,spriteHeight)
    {
        runSpeed = Speed * 2;
    }

    public void Update(GameTime gameTime)
    {
        lastPosition = Position;
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
        
        if (Frozen) return;
        
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

    public void Move(MoveDirections direction, GameTime gameTime)
    {
        Playing = true;
        
        switch (direction)
        {
            case MoveDirections.Up:
                Position -= new Vector2(0,currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale);
                AnimRange = new Vector2(4, 8);
                break;
            case MoveDirections.Down:
                Position += new Vector2(0,currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale);
                AnimRange = new Vector2(0, 4);
                break;
            case MoveDirections.Left:
                Position -= new Vector2(currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale,0);
                AnimRange = new Vector2(12, 16);
                break;
            case MoveDirections.Right:
                Position += new Vector2(currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale,0);
                AnimRange = new Vector2(8, 12);
                break;
        }
    }

    public void Block()
    {
        Position = lastPosition;
    }
}