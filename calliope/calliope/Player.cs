using System.Data;
using calliope.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace calliope;

public class Player : Sprite
{
    public Player(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight) : base(spriteTexture,frameRate,spriteWidth,spriteHeight)
    {
        
    }

    public void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            Playing = true;
            Position += new Vector2(0.05f * gameTime.ElapsedGameTime.Milliseconds,0);
            AnimRange = new Vector2(8, 12);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            Playing = true;
            Position -= new Vector2(0.05f * gameTime.ElapsedGameTime.Milliseconds,0);
            AnimRange = new Vector2(12, 16);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            Playing = true;
            Position += new Vector2(0,0.05f * gameTime.ElapsedGameTime.Milliseconds);
            AnimRange = new Vector2(0, 4);
        }

        else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            Playing = true;
            Position -= new Vector2(0,0.05f * gameTime.ElapsedGameTime.Milliseconds);
            AnimRange = new Vector2(4, 8);
        }

        else
        {
            AnimIndex = 0;
            Playing = false;
        }
    }
}