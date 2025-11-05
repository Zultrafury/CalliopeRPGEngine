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
    public Wall(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, Player player, int costume, Rectangle? area = null) :
        base(spriteTexture, position, spriteWidth, spriteHeight, costume)
    {
        Player = player;
        RenderScale = player.RenderScale;
        
        if (area != null) Area = area.Value;
        else
        {
            Point size = (new Vector2(SpriteDimensions.X,SpriteDimensions.Y) * RenderScale).ToPoint();
            Area = new Rectangle(Position.ToPoint() - (size / new Point(2, 2)), size);
        }
    }

    public Wall(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, Player player, int costume, Rectangle? area = null) : 
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, player, costume, area) {}

    public new void Update(GameTime gameTime)
    {
        if (!Area.Intersects(Player.CollisionArea)) return;
        
        var intersection = RectangleF.Intersection(Area, Player.CollisionArea);
        
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
}

public class WallConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
        return;
        if (value is not Wall wall)
        {
            serializer.Serialize(writer, value);
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("a");
        writer.WriteEndObject();
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return serializer.Deserialize<IDictionary<MenuComponent.NavDirections, MenuComponent>>(reader);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Wall);
    }
}