using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public interface IGameObject
{
    public uint Id { get; set; }
    public Scene Scene { get; set; }
    public float RenderScale { get; set; }
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    public void SceneInit(Scene scene);
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}

public static class IGameObjectExtensions
{
    private static T Convert<T>(this IGameObject obj) where T : IGameObject
    {
        if (obj is T gameObject) return gameObject;
        return default;
    }
    
    public static Menu ToMenu(this IGameObject obj) => Convert<Menu>(obj);
    public static Player ToPlayer(this IGameObject obj) => Convert<Player>(obj);
    public static Follower ToFollower(this IGameObject obj) => Convert<Follower>(obj);
    public static TextDisplay ToTextDisplay(this IGameObject obj) => Convert<TextDisplay>(obj);
    public static AnimatedSprite ToAnimatedSprite(this IGameObject obj) => Convert<AnimatedSprite>(obj);
    public static DialogueBox ToDialogueBox(this IGameObject obj) => Convert<DialogueBox>(obj);
    public static Sprite ToSprite(this IGameObject obj) => Convert<Sprite>(obj);
    public static MenuComponent ToMenuComponent(this IGameObject obj) => Convert<MenuComponent>(obj);
}

public class IGameObjectConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var obj = value as IGameObject;
        writer.WriteValue(value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return (IGameObject)reader.Value;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IGameObject);
    }
}
