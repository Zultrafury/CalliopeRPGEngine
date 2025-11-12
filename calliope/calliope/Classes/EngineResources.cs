using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace calliope.Classes;

public static class EngineResources
{
    public static List<JsonConverter> Converters
    {
        get
        {
            List<JsonConverter> result =
            [
                //new OrthographicCameraConverter(),
                new SpriteFontResourceConverter(),
                new SoundEffectResourceConverter(),
                new TextureResourceConverter()
            ];
            return result;
        }
    }
    public static ContentManager Content { get; set;}
    public static Dictionary<string, TextureResource> Textures { get; } = new();
    public static Dictionary<string, SpriteFontResource> Fonts { get; } = new();
    public  static Dictionary<string, SoundEffectResource> Sfx { get; } = new();
    
    public static void LoadAsset(string name, bool output = false)
    {
        if (output) Console.WriteLine($"Loading asset {name}...");
        if (name[..14] == "Assets/Images/")
        {
            string alias = name[14..];
            Textures[alias] = new TextureResource(name);
            if (output) Console.WriteLine($"TextureResource {alias} loaded!");
        }
        else if (name[..13] == "Assets/Fonts/")
        {
            string alias = name[13..];
            Fonts[alias] = new SpriteFontResource(name);
            if (output) Console.WriteLine($"SpriteFontResource {alias} loaded!");
        }
        else if (name[..14] == "Assets/Sounds/")
        {
            string alias = name[14..];
            Sfx[alias] = new SoundEffectResource(name);
            if (output) Console.WriteLine($"SoundEffectResource {alias} loaded!");
        }
        else if (output) Console.WriteLine($"Type of {name} not found.");
    }
}

public abstract class EngineResource(string path)
{
    public string Path { get; set; } = path;
}

public class SpriteFontResource(string path) : EngineResource(path)
{
    [JsonIgnore] public SpriteFont Font { get; set; } = EngineResources.Content.Load<SpriteFont>(path);
}

public class SpriteFontResourceConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        SpriteFontResource o = (SpriteFontResource)value;
        serializer.Serialize(writer, o.Path);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        foreach (var f in EngineResources.Fonts.Values)
        {
            if (f.Path != (string)reader.Value) continue;
            
            //Console.WriteLine($"Font loaded from list {f.Path}");
            return f;
        }
        
        //Console.WriteLine($"Font not found at {reader.Value}");
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SpriteFontResource);
    }
}

public class SoundEffectResource(string path) : EngineResource(path)
{
    [JsonIgnore] public SoundEffect SoundEffect { get; set; } = EngineResources.Content.Load<SoundEffect>(path);
}

public class SoundEffectResourceConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        SoundEffectResource o = (SoundEffectResource)value;
        serializer.Serialize(writer, o.Path);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        foreach (var s in EngineResources.Sfx.Values)
        {
            if (s.Path != (string)reader.Value) continue;
            
            //Console.WriteLine($"Sound effect loaded from list {f.Path}");
            return s;
        }
        
        //Console.WriteLine($"Sound effect not found at {reader.Value}");
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SoundEffectResource);
    }
}

public class TextureResource(string path) : EngineResource(path)
{
    [JsonIgnore] public Texture2D Texture { get; set; } = EngineResources.Content.Load<Texture2D>(path);
}

public class TextureResourceConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        TextureResource o = (TextureResource)value;
        serializer.Serialize(writer, o.Path);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        foreach (var t in EngineResources.Textures.Values)
        {
            if (t.Path != (string)reader.Value) continue;
            
            //Console.WriteLine($"Font loaded from list {f.Path}");
            return t;
        }
        
        //Console.WriteLine($"Font not found at {reader.Value}");
        return null;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TextureResource);
    }
}