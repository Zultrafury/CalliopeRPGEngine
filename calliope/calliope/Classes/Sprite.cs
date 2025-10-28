using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

/// <summary>
/// A scene object that will be rendered in a scene. Is a base for many more complicated types of scene components (characters, tiles, etc.)
/// </summary>
public class Sprite : IUpdateDraw
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

    public int AnimSetWidth { get; set; } = 1;
    public int FrameRate { get; set; } = 100;
    public bool Playing { get; set; } = true;
    public float RenderScale { get; set; } = 1f;
    public float RenderOrder { get; set; }

    protected bool nextFrame;
    protected int passedTime;

    public Sprite() { }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="spriteWidth">The width of the sprite.</param>
    /// <param name="spriteHeight">The height of the sprite.</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    public Sprite(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate)
    {
        SpriteTexture = spriteTexture;
        Position = position;
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        if (SpriteTexture == null) return;
        
        // Animation Initialization
        FrameRate = frameRate;
        AnimRange = new Vector2(0, (spriteTexture.Height/spriteHeight) * (spriteTexture.Width/spriteWidth));
        AnimIndex = (int)AnimRange.X;
        AnimSetWidth = spriteTexture.Width/spriteWidth;
    }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="dimensions">The dimensions of the sprite. (X + Y reduced to ints)</param>
    /// <param name="frameRate">The sprite animation speed in millisecond delay between frames.</param>
    public Sprite(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int frameRate) :
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
        
        Vector2 _animSet = new Vector2(AnimIndex%AnimSetWidth, AnimIndex/AnimSetWidth);
        
        spriteBatch.Draw(SpriteTexture, new Rectangle((int)Position.X-(int)(SpriteWidth*RenderScale)/2,(int)Position.Y-(int)(SpriteHeight*RenderScale)/2,
                (int)(SpriteWidth*RenderScale),(int)(SpriteHeight*RenderScale)),
            new Rectangle((int)(_animSet.X*SpriteWidth),(int)(_animSet.Y*SpriteHeight),SpriteWidth,SpriteHeight), Color.White);

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
    
    public static Texture2D GeneratePlaceholder(GraphicsDevice graphicsDevice, int width, int height)
    {
        Color[] colors = new Color[width * height];
        for (int i = 0; i < colors.Length; i++)
        {
            if (i % width < width / 2)
            {
                if (i < colors.Length / 2) colors[i] = Color.Magenta;
                else colors[i] = Color.Black;
            }
            else
            {
                if (i < colors.Length / 2) colors[i] = Color.Black;
                else colors[i] = Color.Magenta;
            }
        }

        var result = new Texture2D(graphicsDevice, width, height);
        result.SetData(colors);
        
        return result;
    }
}