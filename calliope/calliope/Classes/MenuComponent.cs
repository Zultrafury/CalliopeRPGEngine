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
    //[JsonConverter(typeof(MenuComponentConverter))]
    public Dictionary<NavDirections, uint> Neighbors { get; set; } = new();
    public Vector2 Position { get; set; }
    public Vector2 Offset {  get; set; }
    public Vector2 Size { get; set; }
    public Texture2D Border { get; set; }
    public Texture2D Background { get; set; }
    public TextDisplay TextDisplay { get; set; }
    [JsonIgnore]
    public float RenderScale { get; set; }
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    public ICommand LinkedAction { get; set; }
    public Dictionary<NavDirections, ICommand> DirectionalActions { get; set; } = null;

    public MenuComponent(Vector2 offset, Vector2 size, string text, SpriteFontResource font, Texture2D border = null, Texture2D background = null)
    {
        Offset = offset;
        Size = size;
        TextDisplay = new TextDisplay(font,Position,text)
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

    public void SceneInit(Scene scene)
    {
        Scene = scene;
        RenderScale = float.Parse(Scene.Config["renderscale"]);
    }

    public void Update(GameTime gameTime)
    {
        if (TextDisplay.Centered) TextDisplay.Position = (Position+(Offset*RenderScale/2));
        else TextDisplay.Position = (Position+(Offset/2))-(new Vector2(Size.X,0)/2)*RenderScale+new Vector2(5f,0)*RenderScale;
    }

    public MenuComponent Navigate(NavDirections direction)
    {
        if (DirectionalActions != null && DirectionalActions.TryGetValue(direction, out ICommand value)) value?.Execute();
        if (!Neighbors.TryGetValue(direction, out var i)) return null;

        var neighbor = Scene.Get(i,false).ToMenuComponent();
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
        var position  = Position;
        var rect = new RectangleF((position + (Offset*RenderScale / 2)) - ((Size / 2) * RenderScale), Size * RenderScale);
        if (Background == null)
        {
            spriteBatch.FillRectangle(rect, Color.Black);
        }
        if (Border == null)
        {
            Color col = Selected ? Color.Red : Color.LightGray;
            spriteBatch.DrawRectangle(rect, col, 2*RenderScale);
        }
        TextDisplay.Draw(spriteBatch, gameTime);
    }
}

/*[Obsolete("MenuComponents do not need a custom converter now",true)]
public class MenuComponentConverter : JsonConverter
{
    private readonly HashSet<MenuComponent.NavDirections> _keysToInclude =
    [
        MenuComponent.NavDirections.Down,
        MenuComponent.NavDirections.Right
    ];
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is not IDictionary<MenuComponent.NavDirections, MenuComponent> dictionary)
        {
            serializer.Serialize(writer, value);
            return;
        }

        writer.WriteStartObject();
        foreach (var kvp in dictionary)
        {
            if (!_keysToInclude.Contains(kvp.Key)) continue;
            
            writer.WritePropertyName(nameof(kvp.Key));
            serializer.Serialize(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return serializer.Deserialize<IDictionary<MenuComponent.NavDirections, MenuComponent>>(reader);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType &&
               objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
               objectType.GetGenericArguments() == (Type[])[typeof(MenuComponent.NavDirections),typeof(MenuComponent)];
    }
}*/