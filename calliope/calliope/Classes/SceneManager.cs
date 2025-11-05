using System.Collections.Generic;

namespace calliope.Classes;

public class SceneManager
{
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
        Scenes[CurrentScene].Start(reload);
    }
}