using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

/// <summary>
/// A scene object that will be rendered in a scene. Is a base for many more complicated types of scene components (characters, tiles, etc.)
/// </summary>
public class Sprite
{
    public Vector2 Position { get; set; }
    public Vector2 AnimRange { get; set; }
    public Texture2D SpriteTexture { get; set;}
    public int SpriteWidth { get; set; } = 16;
    public int SpriteHeight { get; set; } = 16;

    /// <summary>
    /// A property for getting/setting both SpriteWidth and SpriteHeight in a single Vector2.
    /// </summary>
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
    public float RenderScale { get; set; } = 1f;

    protected bool nextFrame;
    protected int passedTime;

    public Sprite() { }
    
    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    /// <param name="spriteWidth">The width of the sprite.</param>
    /// <param name="spriteHeight">The height of the sprite.</param>
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
    
    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    /// <param name="dimensions">The dimensions of the sprite. (X + Y reduced to ints)</param>
    public Sprite(Texture2D spriteTexture, int frameRate, Vector2 dimensions) :
        this(spriteTexture, frameRate, (int)dimensions.X, (int)dimensions.Y) {}

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        if (AnimIndex >= AnimRange.Y) AnimIndex = (int)AnimRange.X;
        if (AnimIndex < AnimRange.X) AnimIndex = (int)AnimRange.X;
        
        Vector2 _animSet = new Vector2(AnimIndex%AnimSetWidth, AnimIndex/AnimSetWidth);
        
        _spriteBatch.Draw(SpriteTexture, new Rectangle((int)Position.X-(int)(SpriteWidth*RenderScale)/2,(int)Position.Y-(int)(SpriteHeight*RenderScale)/2,
                (int)(SpriteWidth*RenderScale),(int)(SpriteHeight*RenderScale)),
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