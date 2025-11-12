using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

public class Wall : Sprite, IGameObject
{
    [JsonIgnore]
    public Player Player { get; set; }
    [JsonIgnore]
    public RectangleF Area { get; set; }
    [JsonIgnore]
    public bool DrawDebugRects { get; set; } = false;
    [JsonConstructor]
    public Wall(TextureResource spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int costume, RectangleF? area = null) :
        base(spriteTexture, position, spriteWidth, spriteHeight, costume)
    {
        if (area != null) Area = area.Value;
        else RecalculateArea();
    }

    public Wall(TextureResource spriteTexture, Vector2 position, Vector2 dimensions, int costume, RectangleF? area = null) : 
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, costume, area) {}

    public new void SceneInit(Scene scene)
    {
        base.SceneInit(scene);
        Player = Scene.Get(Scene.Player).ToPlayer();
        RecalculateArea();
    }

    public new void Update(GameTime gameTime)
    {
        if (!Area.Intersects(Player.CollisionArea)) return;
        
        var intersection = RectangleF.Intersection(Area, Player.CollisionArea);
        intersection.Size /= RenderScale;
        
        if (intersection.Width < intersection.Height)
        {
            if (Player.Position.X < Position.X) Player.Position -= new Vector2(intersection.Width,0);
            else Player.Position += new Vector2(intersection.Width,0);
        }
        else
        {
            if (Player.Position.Y < Position.Y) Player.Position -= new Vector2(0,intersection.Height);
            else Player.Position += new Vector2(0,intersection.Height);
        }

        Player.Block();
    }

    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(Area,Color.Red,5);
    }

    public void RecalculateArea()
    {
        Vector2 size = new Vector2(SpriteDimensions.X,SpriteDimensions.Y) * RenderScale;
        Area = new RectangleF(Position * RenderScale - (size / new Vector2(2, 2)), size);
    }
}

/*public class WallConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, (Sprite)value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        existingValue ??= Activator.CreateInstance(objectType);
        serializer.Populate(reader, existingValue);
        return existingValue;
        
        var obj = serializer.Deserialize<Wall>(reader);
        //obj.Player = obj.Scene.Get(obj.Scene.Player).ToPlayer();
        return obj;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Wall);
    }
}*/