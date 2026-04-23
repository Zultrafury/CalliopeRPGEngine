using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace leveleditor.Classes;

public class Button
{
    public Vector2 Position { get; set;  }
    public SizeF Size { get; set; }
    public RectangleF Bounds => new(Position-(Offset * Size), Size);

    public Vector2 Offset { get; set; }
    public string Text { get; set; }
    public float TextSize { get; set; }
    public SpriteFont Font { get; set; }
    public Color TextColor { get; set; } = new (255, 255, 255, 255);
    public Color BackgroundColor { get; set; } = new (0, 0, 0, 255);
    public Color ClickedColor { get; set; } = new (127, 127, 127, 255);
    public bool Clicked { get; set; }
    public bool Enabled { get; set; } = true;
    public bool SnapToCamera { get; set; }
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
        var fontsize = Font.MeasureString(Text) * standardsize / 2;
        Color color = Clicked ? ClickedColor : BackgroundColor;

        //var relativeOffset = Offset * Size;
        //var offsetBounds = new RectangleF(Position - relativeOffset, Size);
        
        spriteBatch.FillRectangle(Bounds, color);
        
        spriteBatch.DrawString(Font,Text,Bounds.Position+(fontsize/4),TextColor,
            0,Vector2.Zero,new Vector2(standardsize),SpriteEffects.None,0);
        spriteBatch.DrawRectangle(Bounds,Color.Red);
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
        
        if (Size == SizeF.Empty)
        {
            Resize();
        }
    }
    
    public void Resize(float? renderScale = null)
    {
        if (renderScale != null) RenderScale = renderScale.Value;
        
        var standardsize = TextSize / RenderScale;
        Size = Font.MeasureString(Text) * standardsize / 2 * 2.5f;
    }

    public void Click()
    {
        if (Clicked) return;
        Clicked = true;
        OnClick?.Invoke();
    }
}