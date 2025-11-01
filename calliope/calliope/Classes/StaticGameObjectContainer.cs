using System.Collections.Generic;
using Newtonsoft.Json;

namespace calliope.Classes;

public class StaticGameObjectContainer
{
    public List<AnimatedSprite> AnimatedSprites { get; set; } = new();
    public List<Sprite> Sprites { get; set; } = new();
    public List<Wall> Walls { get; set; } = new();

    [JsonIgnore]
    public List<IGameObject> Objects
    {
        get
        {
            List<IGameObject> objects = new();
            objects.AddRange(AnimatedSprites);
            objects.AddRange(Sprites);
            objects.AddRange(Walls);
            return objects;
        }
    }

    static bool CanAdd(IGameObject obj)
    {
        return obj.GetType().Name == nameof(AnimatedSprite) ||
               obj.GetType().Name == nameof(Sprite) ||
               obj.GetType().Name == nameof(Wall);
    }
    
    public bool Add(IGameObject obj)
    {
        if (!CanAdd(obj)) return false;
        
        switch (obj.GetType().Name)
        {
            case nameof(AnimatedSprite):
            {
                AnimatedSprites.Add(obj as AnimatedSprite);
                break;
            }
            case nameof(Wall):
            {
                Walls.Add(obj as Wall);
                break;
            }
            case nameof(Sprite):
            {
                Sprites.Add(obj as Sprite);
                break;
            }
        }

        return true;
    }

    public IGameObject Get(uint id)
    {
        foreach (var animatedSprite in AnimatedSprites) if (animatedSprite.Id == id) return animatedSprite;
        foreach (var sprite in Sprites) if (sprite.Id == id) return sprite;
        foreach (var wall in Walls) if (wall.Id == id) return wall;
        return null;
    }

    public void Clear()
    {
        AnimatedSprites.Clear();
        Sprites.Clear();
        Walls.Clear();
    }
}