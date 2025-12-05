using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace leveleditor.Classes;

public class TextEntryField
{
    public Vector2 Position { get; set;  }
    public SizeF Size { get; set; }
    public RectangleF Bounds => new(Position, Size);
    public Vector2 Offset { get; set; }
    public string Text { get; set; }
    public float TextSize { get; set; }
    public SpriteFont Font { get; set; }
    public Color TextColor { get; set; } = new (0, 0, 0, 255);
    public Color BackgroundColor { get; set; } = new (255, 255, 255, 255);
    public bool Clicked { get; set; }
    public bool Enabled { get; set; } = true;
    public bool SnapToCamera { get; set; }
    public float RenderScale { get; set; }
    public Action OnSubmit { get; set; }
    public string OldText { get; set; }
    
    public TextEntryField(Vector2 offset, SpriteFont font, float renderScale, bool snapToCamera = false)
    {
        Offset = offset;
        Font = font;
        RenderScale = renderScale;
        TextSize = 5;
        SnapToCamera = snapToCamera;
    }
    
    public void Decorate(string text, Action onSubmit = null, float? textSize = null,
        SpriteFont font = null, Color? textColor = null, Color? backgroundColor = null)
    {
        Text = text;
        OldText = Text;
        OnSubmit = onSubmit;
        if (textSize != null) TextSize = textSize.Value;
        if (font != null) Font = font;
        if (textColor != null) TextColor = textColor.Value;
        if (backgroundColor != null) BackgroundColor = backgroundColor.Value;
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled) return;
        if (Clicked) spriteBatch.FillRectangle(Position,Size, BackgroundColor);
        
        spriteBatch.DrawString(Font,Text,Position,TextColor,
            0,Vector2.Zero,new Vector2(TextSize/RenderScale),SpriteEffects.None,0);
        spriteBatch.DrawRectangle(Position,Size, Color.Red);
    }

    public void Resize(float? renderScale = null)
    {
        if (renderScale != null) RenderScale = renderScale.Value;
        
        var standardsize = TextSize/RenderScale;
        SizeF fontsize = Font.MeasureString(Text) * standardsize;
        Size = fontsize;
    }

    public void Submit()
    {
        OnSubmit?.Invoke();
    }
}