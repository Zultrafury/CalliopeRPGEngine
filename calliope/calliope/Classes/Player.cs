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
    private float runSpeed;
    public Player(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight) : base(spriteTexture,frameRate,spriteWidth,spriteHeight)
    {
        runSpeed = Speed * 2;
    }

    public void Update(GameTime gameTime)
    {
        // Run Speed
        float currentSpeed;
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
            Playing = true;
            Position += new Vector2(currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale,0);
            AnimRange = new Vector2(8, 12);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            Playing = true;
            Position -= new Vector2(currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale,0);
            AnimRange = new Vector2(12, 16);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            Playing = true;
            Position += new Vector2(0,currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale);
            AnimRange = new Vector2(0, 4);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            Playing = true;
            Position -= new Vector2(0,currentSpeed * gameTime.ElapsedGameTime.Milliseconds * RenderScale);
            AnimRange = new Vector2(4, 8);
        }
        else
        {
            AnimIndex = 0;
            Playing = false;
        }

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
    }
}