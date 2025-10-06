using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

public class Sprite
{
    public Vector2 Position { get; set; }
    public Vector2 AnimRange { get; set; }
    public Texture2D SpriteTexture { get; set;}
    public int SpriteWidth { get; set; } = 16;
    public int SpriteHeight { get; set; } = 16;
    public int AnimIndex {get; set;}
    public int FrameRate { get; set; } = 100;
    public bool Playing { get; set; } = true;

    protected bool nextFrame;
    protected int passedTime;

    public Sprite()
    {
        
    }

    public Sprite(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight)
    {
        SpriteTexture = spriteTexture;
        FrameRate = frameRate;
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        AnimRange = new Vector2(0, (spriteTexture.Height/spriteHeight) * (spriteTexture.Width/spriteWidth));
        AnimIndex = (int)AnimRange.X;
    }

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime, Matrix? transformMatrix = null)
    {
        if (AnimIndex >= AnimRange.Y) AnimIndex = (int)AnimRange.X;
        if (AnimIndex < AnimRange.X) AnimIndex = (int)AnimRange.X;
        
        if (transformMatrix != null) _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix);
        else _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        _spriteBatch.Draw(SpriteTexture, new Rectangle((int)Position.X,(int)Position.Y,SpriteWidth,SpriteHeight),
            new Rectangle(AnimIndex%4*16,AnimIndex/4*16,SpriteWidth,SpriteHeight), Color.White);
        
        _spriteBatch.End();

        if (!Playing) { return; }
        
        passedTime += gameTime.ElapsedGameTime.Milliseconds;
        if (passedTime >= FrameRate)
        {
            passedTime -= FrameRate;
            nextFrame = true;
        }
        if (nextFrame)
        {
            AnimIndex++;
            nextFrame = false;
        }
    }
    
}