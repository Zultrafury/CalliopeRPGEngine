using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

/// <summary>
/// A persistent object that will be rendered in a scene. Is a base for many more complicated types of scene components (characters, tiles, etc.)
/// </summary>
public class Sprite
{
    public Vector2 Position { get; set; }
    public Vector2 AnimRange { get; set; }
    public Texture2D SpriteTexture { get; set;}
    public int SpriteWidth { get; set; } = 16;
    public int SpriteHeight { get; set; } = 16;

    public Vector2 SpriteDimensions
    {
        get => new(SpriteWidth, SpriteHeight);
        set
        {
            SpriteWidth = (int)value.X;
            SpriteHeight = (int)value.Y;
        }
    }

    /// <summary>
    /// The indexes between which frames are currently animated. X inclusive, Y exclusive.
    /// </summary>
    public int AnimIndex {get; set;}
    public int AnimSetWidth { get; set; }
    public int FrameRate { get; set; } = 100;
    public bool Playing { get; set; } = true;
    public float Scale { get; set; } = 1f;

    protected bool nextFrame;
    protected int passedTime;

    public Sprite() { }

    public Sprite(Texture2D spriteTexture, int frameRate, int spriteWidth, int spriteHeight)
    {
        SpriteTexture = spriteTexture;
        FrameRate = frameRate;
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        AnimRange = new Vector2(0, (spriteTexture.Height/spriteHeight) * (spriteTexture.Width/spriteWidth));
        AnimIndex = (int)AnimRange.X;
        AnimSetWidth = spriteTexture.Width/spriteWidth;
    }

    public Sprite(Texture2D spriteTexture, int frameRate, Vector2 dimensions) : this(spriteTexture, frameRate, (int)dimensions.X, (int)dimensions.Y) {}

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        if (AnimIndex >= AnimRange.Y) AnimIndex = (int)AnimRange.X;
        if (AnimIndex < AnimRange.X) AnimIndex = (int)AnimRange.X;
        
        Vector2 _animSet = new Vector2(AnimIndex%AnimSetWidth, AnimIndex/AnimSetWidth);
        
        _spriteBatch.Draw(SpriteTexture, new Rectangle((int)Position.X-(int)(SpriteWidth*Scale)/2,(int)Position.Y-(int)(SpriteHeight*Scale)/2,
                (int)(SpriteWidth*Scale),(int)(SpriteHeight*Scale)),
            new Rectangle((int)(_animSet.X*SpriteWidth),(int)(_animSet.Y*SpriteHeight),SpriteWidth,SpriteHeight), Color.White);

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