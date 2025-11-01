using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace calliope.Classes;

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
}