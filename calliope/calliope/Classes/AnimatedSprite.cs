using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace calliope.Classes;

/// <summary>
/// A scene object that will be rendered in a scene. Is a base for many more complicated types of scene components (characters, tiles, etc.)
/// </summary>
public class AnimatedSprite : IGameObject
{
    public Vector2 Position { get; set; }
    public Dictionary<string,Point> AnimSets {get; set;}
    /// <summary>
    /// The indexes between which frames are currently animated. X inclusive, Y exclusive.
    /// </summary>
    public Point AnimRange { get; set; }
    [JsonIgnore]
    public Texture2D SpriteTexture { get; set;}
    [JsonIgnore]
    public int SpriteWidth { get; set; } = 16;
    [JsonIgnore]
    public int SpriteHeight { get; set; } = 16;

    /// <summary>
    /// A property for getting/setting both SpriteWidth and SpriteHeight in a single Vector2.
    /// </summary>
    public Point SpriteDimensions
    {
        get => new(SpriteWidth, SpriteHeight);
        set
        {
            SpriteWidth = (int)value.X;
            SpriteHeight = (int)value.Y;
        }
    }
    [JsonIgnore]
    public int AnimIndex {get; set;}
    [JsonIgnore]
    public int AnimSetWidth { get; set; } = 1;
    public int FrameRate { get; set; } = 100;
    [JsonIgnore]
    public bool Playing { get; set; } = true;
    [JsonIgnore]
    public float RenderScale { get; set; } = 1f;
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }

    protected bool nextFrame;
    protected int passedTime;

    public AnimatedSprite() { }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="spriteWidth">The width of the sprite.</param>
    /// <param name="spriteHeight">The height of the sprite.</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    public AnimatedSprite(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate)
    {
        SpriteTexture = spriteTexture;
        Position = position;
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        if (SpriteTexture == null) return;
        
        // Animation Initialization
        FrameRate = frameRate;
        AnimRange = new (0, (spriteTexture.Height/spriteHeight) * (spriteTexture.Width/spriteWidth));
        AnimIndex = AnimRange.X;
        AnimSetWidth = spriteTexture.Width/spriteWidth;
    }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="dimensions">The dimensions of the sprite. (X + Y reduced to ints)</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    public AnimatedSprite(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int frameRate) :
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate) {}
    

    /// <summary>
    /// This updates the sprite (but does nothing!)
    /// </summary>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Draw"/>
    public void Update(GameTime gameTime) {}

    /// <summary>
    /// This draws the sprite. Call this in the draw loop.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Update"/>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (SpriteTexture == null) return;
            
        if (AnimIndex >= AnimRange.Y) AnimIndex = (int)AnimRange.X;
        if (AnimIndex < AnimRange.X) AnimIndex = (int)AnimRange.X;
        
        Point _animSet = new (AnimIndex%AnimSetWidth, AnimIndex/AnimSetWidth);
        
        spriteBatch.Draw(SpriteTexture, new Rectangle((int)Position.X-(int)(SpriteWidth*RenderScale)/2,(int)Position.Y-(int)(SpriteHeight*RenderScale)/2,
                (int)(SpriteWidth*RenderScale),(int)(SpriteHeight*RenderScale)),
            new Rectangle(_animSet.X*SpriteWidth,_animSet.Y*SpriteHeight,SpriteWidth,SpriteHeight), Color.White);

        if (!Playing) { return; }
        
        passedTime += gameTime.ElapsedGameTime.Milliseconds;

        do
        {
            if (passedTime >= FrameRate)
            {
                passedTime -= FrameRate;
                nextFrame = true;
                AnimIndex++;
            }
            else nextFrame = false;
        } while (nextFrame);
    }
}