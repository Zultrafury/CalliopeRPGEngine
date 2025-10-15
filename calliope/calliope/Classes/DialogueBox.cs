using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace calliope.Classes;

/// <summary>
/// A box made of rectangles and a <see cref="TextDisplay"/>.
/// It reveals text (wrapped within its width) over time based on its delay, and has a LinkTrigger that can execute a function once closed.
/// </summary>
public class DialogueBox
{
    private string _text;

    public string Text
    {
        get => _text;
        set => _text = EvaluateLength(value);
    }

    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public float Padding { get; set; }
    public float RenderScale {get; set;}
    public int ScrollDelay { get; set; }
    public TextDisplay TextDisplay { get; set; }
    public Texture2D PortraitTexture { get; set; } = null;
    public bool Hidden { get; set; } = true;
    public Player Player { get; set; }
    public bool FreezePlayer { get; set; } = false;
    public Action LinkTrigger { get; set; }
    /// <summary>
    /// Some predefined ints for use with a <see cref="ScrollDelay"/>.
    /// </summary>
    public enum TextSpeeds
    {
        ExtraSlow = 120,
        Slow = 80,
        Normal = 40,
        Fast = 20,
        ExtraFast = 10,
        Instant = 0
    }
    
    private bool finished = false;
    private int progress = 0;
    private int delay = 0;
    
    /// <param name="font">The font for the internal <see cref="TextDisplay"/> to use.</param>
    /// <param name="renderScale">The render scaling amount.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="scrollDelay">The text reveal speed in millisecond delay between characters.</param>
    /// <param name="position">The position of the box.</param>
    /// <param name="size">The size (width and height) of the box.</param>
    /// <param name="padding">The amount of horizontal padding between the box border and the <see cref="TextDisplay"/>.</param>
    public DialogueBox(SpriteFont font, float renderScale, string text, int scrollDelay, Vector2 position, Vector2 size, float padding)
    {
        TextDisplay = new TextDisplay(font,renderScale, "")
        {
            Color = Color.White
        };
        
        Size = size;
        Position = position;
        RenderScale = renderScale;
        ScrollDelay = scrollDelay;
        Padding = padding;
        
        Text = text;
        
        Initiate();
    }

    /// <summary>
    /// This updates the box. Call this in the update loop.
    /// </summary>
    /// <param name="camera">The <see cref="OrthographicCamera"/> of the game.</param>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Draw"/>
    public void Update(OrthographicCamera camera, GameTime gameTime)
    {
        if (Player.InteractPressed)
        {
            Advance();
        }
        
        Position = camera.Center + new Vector2(0,3 * camera.BoundingRectangle.Size.Height/10);
        TextDisplay.Position = Position - new Vector2(Size.X/2-Padding,Size.Y/2);
        delay += gameTime.ElapsedGameTime.Milliseconds;
        if (delay > ScrollDelay && finished == false)
        {
            delay = 0;
            if (progress >= Text.Length) finished = true;
            else
            {
                progress++;
                TextDisplay.Text = Text.Substring(0, progress);
            }
        }
    }
    
    /// <summary>
    /// This draws the box. Call this in the draw loop.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Update"/>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Hidden) return;
        
        spriteBatch.FillRectangle(new Vector2(Position.X-(Size.X/2), Position.Y-(Size.Y/2)), new SizeF(Size.X, Size.Y), Color.Black);
        spriteBatch.DrawRectangle(new Vector2(Position.X-(Size.X/2), Position.Y-(Size.Y/2)), new SizeF(Size.X, Size.Y), Color.White,10*(RenderScale/8));
        
        TextDisplay.Draw(spriteBatch, gameTime);
    }

    /// <summary>
    /// Posts new dialogue to the dialogue box.
    /// </summary>
    /// <param name="text">New text to display. (Optional)</param>
    /// <param name="linkTrigger">New link trigger to activate on box close. Set to "() => {}" for the box to have no trigger. (Optional)</param>
    /// <param name="font">New font for the text to use. (Optional)</param>
    /// <param name="scrollDelay">New scroll delay for the text to use. (Optional)</param>
    /// <param name="freezePlayer"></param>
    public void Initiate(string text = null, Action linkTrigger = null, SpriteFont font = null, int? scrollDelay = null, bool? freezePlayer = null)
    {
        progress = 0;
        finished = false;
        Hidden = false;
        
        if (font != null) TextDisplay.Font = font;
        if (text != null)
        {
            Text = text;
            TextDisplay.Text = "";
        }
        if (scrollDelay != null) ScrollDelay = scrollDelay.Value;
        if (linkTrigger != null) LinkTrigger = linkTrigger;
        if (freezePlayer != null) FreezePlayer = freezePlayer.Value;
        if (FreezePlayer) Player.Frozen = true;
    }

    /// <summary>
    /// An internal function that updates text strings to fit within the box horizontally (wrapping).
    /// </summary>
    /// <param name="text">The text for the box to evaluate.</param>
    /// <returns>The text with LF (\n) characters placed properly to wrap the text in the box.</returns>
    public string EvaluateLength(string text)
    {
        //Console.WriteLine("Evaluating length");
        //"I'm talking! Isn't that great? Yapping is\nseriously my favorite! Like, totes cool and\nstuff..."
        //"I'm talking! Isn't that great? Yapping is seriously my favorite! Like, totes cool and stuff..."
        
        string result = "";
        
        int skimmer = 1;
        int startIndex = 0;
        string lastValid = "";

        while (skimmer <= text.Length)
        {
            if (skimmer == text.Length || text.ToCharArray()[skimmer] == ' ')
            {
                var current = text[startIndex..skimmer];
                float width = TextDisplay.Font.MeasureString(current).X * (RenderScale/8);
                if (width < Size.X - Padding)
                {
                    lastValid = current;
                }
                else
                {
                    //Console.WriteLine("Too long! Adding to result: "+lastValid+" | "+width);
                    startIndex += lastValid.Length + 1;
                    result += lastValid + "\n";
                    skimmer = startIndex;
                }
            }

            skimmer++;
        }
        
        result += text[startIndex..];
        
        //Console.WriteLine(result);
        return result;
    }

    /// <summary>
    /// Advances the text box. If there is text that has yet to be revealed, it reveals it all.
    /// If the text is fully revealed, this closes the text box and triggers the associated <see cref="LinkTrigger"/>.
    /// </summary>
    public void Advance()
    {
        if (!Hidden) Player.InteractPressed = false;
        
        if (finished)
        {
            Hidden = true;
            if (FreezePlayer) Player.Frozen = false;
            LinkTrigger?.Invoke();
        }
        else progress = Text.Length - 1;
    }
}