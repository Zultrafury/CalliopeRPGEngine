using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

public class SceneManager
{
    public string Path { get; set; }
    public OrthographicCamera Camera { get; set; }
    public Dictionary<string, Scene> Scenes { get; set; } = new();
    public string CurrentScene { get; set; }

    public Scene Scene
    {
        get => Scenes.GetValueOrDefault(CurrentScene);
        set => Scenes[CurrentScene] = value;
    }

    public void ChangeScene(string scene, bool reload = true)
    {
        CurrentScene = scene;
        if (reload) Scenes[CurrentScene] = ReadScene(CurrentScene);
        ICommand.Scene = Scenes[CurrentScene];
        Scenes[CurrentScene].Camera = Camera;
        Scenes[CurrentScene].Start(reload);
    }

    public void AddScene(string sceneName, Scene scene)
    {
        scene.Camera = Camera;
        Scenes.Add(sceneName, scene);
    }

    public Scene ReadScene(string sceneName)
    {
        var scenes = JsonConvert.DeserializeObject<Dictionary<string, Scene>>(File.ReadAllText(Path),
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = EngineResources.Converters
                });
        return scenes[sceneName];
    }
}