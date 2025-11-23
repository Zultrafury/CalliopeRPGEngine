using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace leveleditor.Classes;

public class Button
{
    private RectangleF Bounds { get; set; } =  new(Vector2.Zero, SizeF.Empty);
    public Vector2 Position
    {
        get => Bounds.Position;
        set => Bounds = new RectangleF(value, Bounds.Size);
    }
    public SizeF Size
    {
        get => Bounds.Size;
        set => Bounds = new RectangleF(Bounds.Position, value);
    }

    public Vector2 Offset { get; set; }
    public string Text { get; set; }
    public float TextSize { get; set; }
    public SpriteFont Font { get; set; }
    public Color TextColor { get; set; } = new Color(255, 255, 255, 255);
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 255);
    public Color ClickedColor { get; set; } = new Color(127, 127, 127, 255);
    public bool Clicked { get; set; }
    public bool Enabled { get; set; } = false;
    public bool SnapToCamera { get; set; } = true;
    public float RenderScale { get; set; }
    public Action OnClick { get; set; }

    public Button(Vector2 offset, SpriteFont font, float renderScale, SizeF? size = null)
    {
        Offset = offset;
        Font = font;
        RenderScale = renderScale;
        TextSize = 5;
        Size = size ?? SizeF.Empty;
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled) return;
        var standardsize = TextSize/RenderScale;
        var fontsize = new Vector2(Font.MeasureString(Text).X*standardsize/2,Font.MeasureString(Text).Y*standardsize/2);
        if (Size != SizeF.Empty) spriteBatch.FillRectangle(Position-Size/2, Size, BackgroundColor);
        else spriteBatch.FillRectangle(Position-fontsize*1.25f, fontsize*2.5f, BackgroundColor);
        
        spriteBatch.DrawString(Font,Text,Position-fontsize,TextColor,
            0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
    }

    public void Decorate(string text, float? textSize = null, SpriteFont font = null,
        Color? textColor = null, Color? backgroundColor = null, Color? clickedColor = null)
    {
        Text = text;
        if (textSize != null) TextSize = textSize.Value;
        if (font != null) Font = font;
        if (textColor != null) TextColor = textColor.Value;
        if (backgroundColor != null) BackgroundColor = backgroundColor.Value;
        if (clickedColor != null) ClickedColor = clickedColor.Value;
    }

    public void Click()
    {
        if (Clicked) return;
        Clicked = true;
        OnClick?.Invoke();
    }
}