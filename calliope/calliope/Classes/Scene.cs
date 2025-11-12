using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace calliope.Classes;

public class Scene
{
    [JsonIgnore] public OrthographicCamera Camera { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> Config { get; set; } = new();

    public uint Player { get; set; }
    [JsonProperty]
    private List<IGameObject> Objects { get; set; } = new();
    [JsonProperty]
    private StaticGameObjectContainer StaticObjects { get; set; } = new();
    public ICommand StartAction { get; set; }
    private uint nextId = 1;

    [JsonConstructor]
    public Scene(List<IGameObject> objects = null, ICommand startAction = null)
    {
        StartAction = startAction;
        var configfile = File.ReadAllLines("Content/config");
        foreach (var line in configfile)
        {
            //Console.WriteLine(line);
            Config.Add(line.Split('=')[0], line.Split('=')[1]);
        }
        
        if (objects == null) return;
        foreach (IGameObject o in objects) Add(o);
    }

    public void InitializeAll()
    {
        foreach (var o in Objects) o.SceneInit(this);
        foreach (var o in StaticObjects.Objects) o.SceneInit(this);
    }

    public void Add(IGameObject obj)
    {
        void Register(IGameObject o)
        {
            //Console.WriteLine(o.GetType().Name+": "+nextId);
            o.SceneInit(this);
            if (o.Id != 0) return;
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
                    Register(o.TextDisplay);
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
                        if (o.TextDisplay.Id == id) return o.TextDisplay;
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
        /*if (false)
        {
            Clear();
            Objects = new (SavedScene.Objects);
            StaticObjects = SavedScene.StaticObjectContainer;
        }*/

        ICommand.Scene = this;
        InitializeAll();
        StartAction?.Execute();
    }
}