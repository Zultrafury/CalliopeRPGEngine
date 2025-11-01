using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

public class MenuComponent : IGameObject
{
    public bool Selected { get; set; } = false;
    public enum NavDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    [JsonConverter(typeof(MenuComponentConverter))]
    public Dictionary<NavDirections, uint> Neighbors { get; set; } = new();
    public Vector2 Position { get; set; }
    public Vector2 Offset {  get; set; }
    public Vector2 Size { get; set; }
    public Texture2D Border { get; set; }
    public Texture2D Background { get; set; }
    public TextDisplay Text { get; set; }
    [JsonIgnore]
    public float RenderScale { get; set; }
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    public ICommand LinkedAction { get; set; }
    public Dictionary<NavDirections, ICommand> DirectionalActions { get; set; } = null;

    public MenuComponent(Vector2 offset, Vector2 size, float renderScale, string text, SpriteFont font, Texture2D border = null, Texture2D background = null)
    {
        RenderScale = renderScale;
        Offset = offset*RenderScale;
        Size = size;
        Text = new TextDisplay(font,Position*RenderScale,RenderScale,text)
        {
            Color = Color.White,
            Centered = true
        };
        if (border != null) Border = border;
        if (background != null) Background = background;
    }
    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }

    public void Update(GameTime gameTime)
    {
        if (Text.Centered) Text.Position = (Position+(Offset/2));
        else Text.Position = (Position+(Offset/2))-(new Vector2(Size.X,0)/2)*RenderScale+new Vector2(5f,0)*RenderScale;
    }

    public MenuComponent Navigate(NavDirections direction)
    {
        if (DirectionalActions != null && DirectionalActions.TryGetValue(direction, out ICommand value)) value?.Execute();
        if (!Neighbors.TryGetValue(direction, out var i)) return null;

        var neighbor = (MenuComponent)Scene.Get(i,false);
        Selected = false;
        neighbor.Selected = true;
        return neighbor;
    }
    
    public bool AddRelation(MenuComponent component, NavDirections direction)
    {
        if (!Neighbors.TryAdd(direction, component.Id)) return false;
        component.Neighbors[InvertMoveDirection(direction)] = Id;
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
            spriteBatch.FillRectangle(new RectangleF((Position+(Offset/2))-(Size/2)*RenderScale,Size*RenderScale), Color.Black);
        }
        if (Border == null)
        {
            Color col = Selected ? Color.Red : Color.LightGray;
            spriteBatch.DrawRectangle(new RectangleF((Position+(Offset/2))-(Size/2)*RenderScale,Size*RenderScale), col, 2*RenderScale);
        }
        Text.Draw(spriteBatch, gameTime);
    }
}