using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace calliope.Classes;

public class MenuComponent : IUpdateDraw
{
    public bool Selected { get; set; } = false;
    public enum NavDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    public Dictionary<NavDirections, MenuComponent> Neighbors { get; set; } = new();
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Texture2D Border { get; set; } = null;
    public Texture2D Background { get; set; } = null;
    public TextDisplay Text { get; set; }
    public float RenderScale { get; set; }
    public Action LinkedAction { get; set; }

    public MenuComponent(Vector2 position, Vector2 size, float renderScale, string text, SpriteFont font, Texture2D border = null, Texture2D background = null)
    {
        RenderScale = renderScale;
        Position = position;
        Size = size;
        Text = new TextDisplay(font,Position*RenderScale,RenderScale,text) { Color = Color.White };
        if (border != null) Border = border;
        if (background != null) Background = background;
    }
    
    public void Update(GameTime gameTime)
    {
        Text.Position = Position*RenderScale;
    }

    public MenuComponent Navigate(NavDirections direction)
    {
        if (!Neighbors.TryGetValue(direction, out var neighbor)) return null;
        
        Selected = false;
        neighbor.Selected = true;
        return neighbor;
    }
    
    public bool AddRelation(MenuComponent component, NavDirections direction)
    {
        if (!Neighbors.TryAdd(direction, component)) return false;
        component.Neighbors[InvertMoveDirection(direction)] = this;
        return true;
    }
    
    public static NavDirections InvertMoveDirection(NavDirections direction)
    {
        return direction switch
        {
            NavDirections.Up => NavDirections.Down,
            NavDirections.Down => NavDirections.Up,
            NavDirections.Left => NavDirections.Right,
            NavDirections.Right => NavDirections.Left,
            _ => NavDirections.Up
        };
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Background == null)
        {
            spriteBatch.FillRectangle(new RectangleF(Position*RenderScale,Size*RenderScale), Color.Black);
        }
        if (Border == null)
        {
            Color col = Selected ? Color.Red : Color.LightGray;
            spriteBatch.DrawRectangle(new RectangleF(Position*RenderScale,Size*RenderScale), col, RenderScale*2);
        }
        Text.Draw(spriteBatch, gameTime);
    }
}