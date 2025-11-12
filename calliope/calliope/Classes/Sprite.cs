using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public class Sprite : IGameObject
{
    public Vector2 Position { get; set; }
    /// <summary>
    /// The indexes between which frames are currently animated. X inclusive, Y exclusive.
    /// </summary>
    public int Costume { get; set; }
    [JsonIgnore]
    public Point CurrentCostume { get; set; }
    public TextureResource SpriteTexture { get; set;}
    public int SpriteWidth { get; set; } = 16;
    public int SpriteHeight { get; set; } = 16;

    /// <summary>
    /// A property for getting/setting both SpriteWidth and SpriteHeight in a single Vector2.
    /// </summary>
    [JsonIgnore]
    public Point SpriteDimensions
    {
        get => new(SpriteWidth, SpriteHeight);
        set
        {
            SpriteWidth = value.X;
            SpriteHeight = value.Y;
        }
    }
    [JsonIgnore]
    public float RenderScale { get; set; } = 1f;
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }

    public Sprite() { }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="spriteWidth">The width of the sprite.</param>
    /// <param name="spriteHeight">The height of the sprite.</param>
    /// <param name="costume"></param>
    [JsonConstructor]
    public Sprite(TextureResource spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int costume = 0)
    {
        SpriteTexture = spriteTexture;
        Position = position;
        SpriteWidth = spriteWidth;
        SpriteHeight = spriteHeight;
        if (SpriteTexture == null) return;
        
        Costume = costume;
        int costumeSetWidth = spriteTexture.Texture.Width/spriteWidth;
        CurrentCostume = new ((Costume%costumeSetWidth)*SpriteWidth, (Costume/costumeSetWidth)*SpriteHeight);
    }

    /// <param name="spriteTexture">The Texture2D that the sprite uses.</param>
    /// <param name="position"></param>
    /// <param name="dimensions">The dimensions of the sprite. (X + Y reduced to ints)</param>
    /// <param name="costume"></param>
    public Sprite(TextureResource spriteTexture, Vector2 position, Point dimensions, int costume = 0) :
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, costume) {}

    public void SceneInit(Scene scene)
    {
        Scene = scene;
        RenderScale = float.Parse(Scene.Config["renderscale"]);
    }

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
        
        var position = Position * RenderScale;
        spriteBatch.Draw(SpriteTexture.Texture, new Rectangle((int)position.X-(int)(SpriteWidth*RenderScale)/2,(int)position.Y-(int)(SpriteHeight*RenderScale)/2,
                (int)(SpriteWidth*RenderScale),(int)(SpriteHeight*RenderScale)),
            new Rectangle(CurrentCostume,SpriteDimensions), Color.White);
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