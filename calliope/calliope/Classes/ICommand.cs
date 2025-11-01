using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public interface ICommand
{
    public void Execute();
}

public class CommandDialogueBoxInitiate(DialogueBox dialogueBox, string text = null, 
    ICommand linkedAction = null, SpriteFont font = null, int? scrollDelay = null, 
    bool? freezePlayer = null) : ICommand
{
    [JsonIgnore] private Scene Scene = dialogueBox.Scene;
    [JsonProperty] uint DialogueBox = dialogueBox.Id;
    [JsonProperty] string Text { get; set; } = text;
    [JsonProperty] ICommand  LinkedAction { get; set; } = linkedAction;
    [JsonIgnore] SpriteFont Font { get; set; } = font;
    [JsonProperty] int? ScrollDelay { get; set; } = scrollDelay;
    [JsonProperty] bool? FreezePlayer { get; set; } = freezePlayer;
    public void Execute() => Scene.Get(DialogueBox).ToDialogueBox().Initiate(Text,LinkedAction,Font,ScrollDelay,FreezePlayer);
}

public class CommandMenuEngage(Menu menu, int index) : ICommand
{
    [JsonIgnore] Menu Menu = menu;
    [JsonProperty] int Index { get; set; } = index;
    public void Execute() => Menu.Engage(Index);
}

public class CommandMenuSwapTo(Menu menu, Menu newMenu, int? reselectIndex = null) : ICommand
{
    [JsonIgnore] private Scene Scene = menu.Scene;
    [JsonProperty] private uint Menu { get; set; } = menu.Id;
    [JsonProperty] uint NewMenu { get; set; } = newMenu.Id;
    [JsonProperty] int? ReselectIndex { get; set; } = reselectIndex;

    public void Execute()
    {
        Menu menu = Scene.Get(Menu,false).ToMenu();
        Menu newMenu = Scene.Get(NewMenu,false).ToMenu();
        menu.SwapTo(newMenu, ReselectIndex);
    }
}

public class CommandMenuOpen(Menu menu, int? index = null) : ICommand
{
    [JsonIgnore] Menu Menu = menu;
    [JsonProperty] int? Index { get; set; } = index;
    public void Execute() => Menu.Open(Index);
}

public class CommandPlayerSwapMenu(Player player, Menu menu, int? reselectIndex = null) : ICommand
{
    [JsonIgnore] Player Player = player;
    [JsonProperty] uint Menu { get; set; } = menu.Id;
    [JsonProperty] int? ReselectIndex { get; set; } = reselectIndex;
    public void Execute() => Player.SwapMenu(player.Scene.Get(Menu,false).ToMenu(),ReselectIndex);
}

public class CommandSceneManagerChangeScene(SceneManager sceneManager, string scene) : ICommand
{
    [JsonIgnore] SceneManager SceneManager = sceneManager;
    [JsonProperty] string Scene { get; set; } = scene;
    public void Execute() => SceneManager.ChangeScene(Scene);
}

public class CommandGameExit(Game game) : ICommand
{
    [JsonIgnore] Game Game = game;
    public void Execute() => Game.Exit();
}

public class CommandSoundEffectMasterVolumeChange(float amount, 
    TextDisplay textDisplay, SoundEffect soundEffect) : ICommand
{
    [JsonIgnore] private Scene Scene = textDisplay.Scene;
    [JsonProperty] float Amount { get; set; } = amount;
    [JsonProperty] uint TextDisplay { get; set; } = textDisplay.Id;
    [JsonIgnore] SoundEffect SoundEffect { get; set; } = soundEffect;

    public void Execute()
    {
        SoundEffect?.Play();
        SoundEffect.MasterVolume = Math.Clamp(SoundEffect.MasterVolume + Amount,0,1);
        Scene.Get(TextDisplay,false).ToTextDisplay().Text = Math.Round(SoundEffect.MasterVolume*100)+"%";
    }
}