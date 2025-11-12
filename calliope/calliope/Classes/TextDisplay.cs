using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

/// <summary>
/// A scene object that renders text.
/// </summary>
/// <param name="font">The SpriteFont that the text will use.</param>
/// <param name="renderScale">The render scaling amount.</param>
/// <param name="text">The text for it to display.</param>
public class TextDisplay(SpriteFontResource font, Vector2 position, string text) : IGameObject
{
    public Vector2 Position { get; set; } = position;
    public Color Color { get; set; } = Color.Black;
    public SpriteFontResource Font { get; set; } = font;
    public string Text {get; set;} = text;
    [JsonIgnore]
    public float RenderScale {get; set;}
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    public float Scale { get; set; } = 1;
    public bool Centered {get; set;}
    public bool CenteredY {get; set;}
    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }
    public void SceneInit(Scene scene)
    {
        Scene = scene;
        RenderScale = float.Parse(Scene.Config["renderscale"]);
    }

    public void Update(GameTime gameTime) {}

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        Vector2 center;
        if (Centered) center = Font.Font.MeasureString(Text)/2;
        else center = (CenteredY) ? new (0,Font.Font.MeasureString(Text).Y / 2) : new (0,0);
        
        _spriteBatch.DrawString(Font.Font, Text, Position, Color,0,center, (RenderScale/8) * Scale, SpriteEffects.None, 0);
    }
}