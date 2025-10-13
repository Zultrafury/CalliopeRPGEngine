using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace calliope.Classes;

public class Player : Sprite
{
    public float Speed { get; set; } = 0.05f;
    public bool Interacting { get; set; }
    public Player(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight) : base(spriteTexture,frameRate,spriteWidth,spriteHeight)
    {
        
    }

    public void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            Playing = true;
            Position += new Vector2(Speed * gameTime.ElapsedGameTime.Milliseconds * Scale,0);
            AnimRange = new Vector2(8, 12);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            Playing = true;
            Position -= new Vector2(Speed * gameTime.ElapsedGameTime.Milliseconds * Scale,0);
            AnimRange = new Vector2(12, 16);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            Playing = true;
            Position += new Vector2(0,Speed * gameTime.ElapsedGameTime.Milliseconds * Scale);
            AnimRange = new Vector2(0, 4);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            Playing = true;
            Position -= new Vector2(0,Speed * gameTime.ElapsedGameTime.Milliseconds * Scale);
            AnimRange = new Vector2(4, 8);
        }
        
        else
        {
            AnimIndex = 0;
            Playing = false;
        }

        Interacting = (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.E));
    }
}