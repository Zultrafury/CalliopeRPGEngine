using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace calliope.Classes;

/// <summary>
/// A scene object that renders text.
/// </summary>
/// <param name="font">The SpriteFont that the text will use.</param>
/// <param name="renderScale">The render scaling amount.</param>
/// <param name="text">The text for it to display.</param>
public class TextDisplay(SpriteFont font, float renderScale, string text)
{
    public Vector2 Position { get; set; }
    public Color Color { get; set; } = Color.Black;
    public SpriteFont Font {get; set;} = font;
    public string Text {get; set;} = text;
    public float RenderScale {get; set;} = renderScale;
    public float Scale { get; set; } = 1;
    public bool Centered {get; set;} = false;

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        Vector2 center = (Centered) ? Font.MeasureString(Text)/2 : new (0,0);
        
        _spriteBatch.DrawString(Font, Text, Position, Color,0,center, (RenderScale/8) * Scale, SpriteEffects.None, 0);
    }
}