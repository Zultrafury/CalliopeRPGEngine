using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace calliope.Classes;

public class TextDisplay(SpriteFont font, string text)
{
    public Vector2 Position { get; set; }
    public Color Color { get; set; } = Color.Black;
    public SpriteFont Font {get; set;} = font;
    public string Text {get; set;} = text;
    public float Scale {get; set;} = 1f;
    public bool Centered {get; set;} = false;

    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        Vector2 center = (Centered) ? Font.MeasureString(Text)/2 : new (0,0);
        
        _spriteBatch.DrawString(Font, Text, Position, Color,0,center, Scale, SpriteEffects.None, 0);
    }
}