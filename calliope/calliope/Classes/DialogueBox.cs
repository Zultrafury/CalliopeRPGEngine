using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace calliope.Classes;

public class DialogueBox
{
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public int ScrollDelay { get; set; }
    public TextDisplay TextDisplay { get; set; }
    public Texture2D PortraitTexture { get; set; } = null;
    
    private bool finished = false;
    private int progress = 0;
    private int delay = 0;

    public DialogueBox(SpriteFont font, string text, int scrollDelay, Vector2 position, Vector2 size)
    {
        Text = text;
        Position = position;
        Size = size;
        ScrollDelay = scrollDelay;

        TextDisplay = new TextDisplay(font, "")
        {
            Color = Color.White
        };
    }

    public void Update(OrthographicCamera camera, GameTime gameTime)
    {
        Position = camera.Center + new Vector2(0,3 * camera.BoundingRectangle.Size.Height/10);
        TextDisplay.Position = Position - new Vector2(Size.X/2 - (Size.X/50),Size.Y/2  - (Size.Y/50));
        delay += gameTime.ElapsedGameTime.Milliseconds;
        if (delay > ScrollDelay && finished == false)
        {
            delay -= ScrollDelay;
            if (progress >= Text.Length) finished = true;
            else
            {
                progress++;
                TextDisplay.Text = Text.Substring(0, progress);
            }
        }
    }
    
    public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
    {
        _spriteBatch.FillRectangle(new Vector2(Position.X-(Size.X/2), Position.Y-(Size.Y/2)), new SizeF(Size.X, Size.Y), Color.Black);
        _spriteBatch.DrawRectangle(new Vector2(Position.X-(Size.X/2), Position.Y-(Size.Y/2)), new SizeF(Size.X, Size.Y), Color.White,10);
        
        TextDisplay.Draw(_spriteBatch, gameTime);
    }
}