using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

public class Scene
{
    private string FilePath { get; set; }
    [JsonIgnore]
    public OrthographicCamera Camera { get; set; }
    [JsonIgnore]
    public Dictionary<string, string> Config { get; set; } 
    public uint Player { get; set; }
    [JsonProperty]
    private List<IGameObject> Objects { get; set; } = new();
    [JsonProperty]
    private StaticGameObjectContainer StaticObjects { get; set; } = new();
    [JsonIgnore]
    private (StaticGameObjectContainer StaticObjectContainer, List<IGameObject> Objects) SavedScene { get; set; } = (null,null);
    public ICommand StartAction { get; set; }
    private uint nextId = 1;

    public Scene(List<IGameObject> objects = null, ICommand startAction = null)
    {
        StartAction = startAction;
        
        if (objects == null) return;
        foreach (IGameObject o in objects) Add(o);
        
        SavedScene = (null,null);
    }

    public void Add(IGameObject obj)
    {
        void Register(IGameObject o)
        {
            //Console.WriteLine(o.GetType().Name+": "+nextId);
            o.SceneInit(this);
            o.Id = nextId;
            nextId++;
        }
        Register(obj);

        // If static, only add to static objects
        if (StaticObjects.Add(obj)) return;

        // Game object must be non-static
        Objects.Add(obj);

        // Handle GameObjects that have other GameObjects
        switch (obj)
        {
            // Handle Menu
            case Menu menu:
            {
                foreach (MenuComponent o in menu.Components)
                {
                    Register(o);
                    Register(o.Text);
                }
                break;
            }
            
            // Handle DialogueBox
            case DialogueBox box:
            {
                Register(box.TextDisplay);
                break;
            }

            // Handle Player
            case Player player:
            {
                Player = player.Id;
                foreach (Follower f in player.Followers) Register(f);
                break;
            }
        }
    }

    public void AddRange(IEnumerable<IGameObject> objects)
    {
        foreach (IGameObject o in objects) Add(o);
    }

    public IGameObject Get(uint id, bool searchStatic = true)
    {
        // Start with statics if not specified otherwise
        if (searchStatic)
        {
            IGameObject result = StaticObjects.Get(id);
            if (result != null) return result;
        }
        
        foreach (IGameObject obj in Objects)
        {
            if (obj.Id == id) return obj;

            // Handle GameObjects that have other GameObjects
            switch (obj)
            {
                // Handle Menu
                case Menu menu:
                {
                    foreach (MenuComponent o in menu.Components)
                    {
                        if (o.Id == id) return o;
                        if (o.Text.Id == id) return o.Text;
                    }
                    break;
                }
                
                // Handle DialogueBox
                case DialogueBox box:
                {
                    if (box.TextDisplay.Id == id) return box.TextDisplay;
                    break;
                }
                
                // Handle Player
                case Player player:
                {
                    foreach (Follower f in player.Followers)
                    {
                        if (f.Id == id) return f;
                    }
                    break;
                }
            }
        }
        return null;
    }

    public void Clear()
    {
        StaticObjects.Clear();
        Objects.Clear();
    }

    public void Update(GameTime gameTime)
    {
        List<IGameObject> objects = new(Objects);
        objects.AddRange(StaticObjects.Objects);
        foreach (var gameObject in objects.OrderBy(o => o.UpdateOrder)) gameObject.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        List<IGameObject> objects = new(Objects);
        objects.AddRange(StaticObjects.Objects);
        foreach (var gameObject in objects.OrderBy(o => o.RenderOrder)) gameObject.Draw(spriteBatch,gameTime);
    }

    public void Start(bool reload)
    {
        //if (SavedScene == (null,null)) SavedScene = (StaticObjects.Clone, [..Objects]);
        if (false)
        {
            Clear();
            Objects = new (SavedScene.Objects);
            StaticObjects = SavedScene.StaticObjectContainer;
        }
        
        StartAction?.Execute();
    }
}