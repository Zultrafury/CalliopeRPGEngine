using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public interface ICommand
{
    public static Scene Scene { get; set; }
    public static SceneManager SceneManager { get; set; }
    public static Game Game { get; set; }
    
    public void Execute();
}

public class CommandDialogueBoxInitiate(uint dialogueBoxId, string text = null, 
    ICommand linkedAction = null, SpriteFontResource font = null, int? scrollDelay = null, 
    bool? freezePlayer = null) : ICommand
{
    [JsonProperty] uint DialogueBoxId = dialogueBoxId;
    [JsonProperty] string Text { get; set; } = text;
    [JsonProperty] ICommand  LinkedAction { get; set; } = linkedAction;
    [JsonProperty] SpriteFontResource Font { get; set; } = font;
    [JsonProperty] int? ScrollDelay { get; set; } = scrollDelay;
    [JsonProperty] bool? FreezePlayer { get; set; } = freezePlayer;

    public void Execute()
    {
        ICommand.Scene.Get(DialogueBoxId).ToDialogueBox().Initiate(Text, LinkedAction, Font, ScrollDelay, FreezePlayer);
    }
}

public class CommandMenuEngage(uint menuId, int index) : ICommand
{
    [JsonProperty] uint MenuId = menuId;
    [JsonProperty] int Index { get; set; } = index;
    
    public void Execute() => ICommand.Scene.Get(MenuId).ToMenu().Engage(Index);
}

public class CommandMenuSwapTo(uint menuId, uint newMenuId, int? reselectIndex = null) : ICommand
{
    [JsonProperty] uint MenuId = menuId;
    [JsonProperty] uint NewMenuId = newMenuId;
    [JsonProperty] int? ReselectIndex { get; set; } = reselectIndex;

    public void Execute()
    {
        Menu menu = ICommand.Scene.Get(MenuId,false).ToMenu();
        Menu newMenu = ICommand.Scene.Get(NewMenuId,false).ToMenu();
        menu.SwapTo(newMenu, ReselectIndex);
    }
}

public class CommandMenuOpen(uint menuId, int? index = null) : ICommand
{
    [JsonProperty] uint MenuId = menuId;
    [JsonProperty] int? Index { get; set; } = index;
    
    public void Execute() => ICommand.Scene.Get(MenuId,false).ToMenu().Open(Index);
}

public class CommandPlayerSwapMenu(uint playerId, uint menuId, int? reselectIndex = null) : ICommand
{
    [JsonProperty] uint PlayerId = playerId;
    [JsonProperty] uint MenuId { get; set; } = menuId;
    [JsonProperty] int? ReselectIndex { get; set; } = reselectIndex;

    public void Execute()
    {
        Player player = ICommand.Scene.Get(PlayerId,false).ToPlayer();
        player.SwapMenu(MenuId, ReselectIndex);
    }
}

public class CommandSceneManagerChangeScene(string scene, bool reload = true) : ICommand
{
    [JsonProperty] string SceneName { get; set; } = scene;
    [JsonProperty] private bool Reload { get; set; } = reload;
    
    public void Execute() => ICommand.SceneManager.ChangeScene(SceneName, Reload);
}

public class CommandGameExit : ICommand
{
    public void Execute() => ICommand.Game.Exit();
}

public class CommandSoundEffectMasterVolumeChange(float amount, 
    uint textDisplayId, SoundEffectResource soundEffect) : ICommand
{
    [JsonProperty] float Amount { get; set; } = amount;
    [JsonProperty] uint TextDisplayId { get; set; } = textDisplayId;
    [JsonProperty] SoundEffectResource Sfx { get; set; } = soundEffect;

    public void Execute()
    {
        Sfx?.SoundEffect.Play();
        SoundEffect.MasterVolume = Math.Clamp(SoundEffect.MasterVolume + Amount,0,1);
        ICommand.Scene.Get(TextDisplayId,false).ToTextDisplay().Text = Math.Round(SoundEffect.MasterVolume*100)+"%";
    }
}