using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

public class Menu : IGameObject
{
    [JsonIgnore]
    public float RenderScale { get; set; }
    public float RenderOrder { get; set; } = 2000;
    public float UpdateOrder { get; set; }
    public List<MenuComponent> Components { get; set; }
    [JsonIgnore]
    public OrthographicCamera Camera { get; set; }
    public Vector2 Position { get; set; }
    public bool Active { get; set; }
    public Dictionary<string, SoundEffectResource> Sounds { get; set; } = new()
    {
        { "navigate", null },
        { "select",  null },
        { "back", null }
    };
    
    private MenuComponent currentComponent;
    private KeyboardState keyboard = Keyboard.GetState();
    public Menu(List<MenuComponent> components, OrthographicCamera camera = null)
    {
        Components = components;
        Camera = camera;
        currentComponent = Components[0];
        currentComponent.Selected = true;
    }
    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }

    public void SceneInit(Scene scene)
    {
        Scene = scene;
        Camera = Scene.Camera;
        RenderScale = float.Parse(Scene.Config["renderscale"]);
    }

    public void Update(GameTime gameTime)
    {
        if (!Active)
        {
            keyboard = Keyboard.GetState();
            return;
        }
        
        if (Camera != null)
        {
            Position = Camera.Center;
            foreach (var component in Components) component.Position = Position;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            if (!keyboard.IsKeyDown(Keys.D) && !keyboard.IsKeyDown(Keys.Right)) Navigate(MenuComponent.NavDirections.Right);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            if (!keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.Left)) Navigate(MenuComponent.NavDirections.Left);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            if (!keyboard.IsKeyDown(Keys.S) && !keyboard.IsKeyDown(Keys.Down)) Navigate(MenuComponent.NavDirections.Down);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
        {
            if (!keyboard.IsKeyDown(Keys.W) && !keyboard.IsKeyDown(Keys.Up)) Navigate(MenuComponent.NavDirections.Up);
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.E))
        {
            if (!keyboard.IsKeyDown(Keys.Z) && !keyboard.IsKeyDown(Keys.E)) Select();
        }
        
        keyboard = Keyboard.GetState();
        
        foreach (var component in Components) component.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!Active) return;
        
        foreach (var component in Components.OrderBy(o => o.RenderOrder)) component.Draw(spriteBatch, gameTime);
        //spriteBatch.DrawCircle(Position,5*RenderScale,16,Color.Green,1*RenderScale);
    }

    public void SetSounds(SoundEffectResource navigate, SoundEffectResource select, SoundEffectResource back)
    {
        Sounds["navigate"] = navigate;
        Sounds["select"] = select;
        Sounds["back"] = back;
    }

    void Select()
    {
        Sounds["select"].SoundEffect.Play();
        currentComponent.LinkedAction?.Execute();
    }
    
    void Navigate(MenuComponent.NavDirections direction)
    {
        var nextcomponent = currentComponent.Navigate(direction);

        if (nextcomponent != null)
        {
            Sounds["navigate"].SoundEffect.Play();
            currentComponent = nextcomponent;
        }
        //else Sounds["back"].Play();
    }

    public void Open(int? reselectIndex = null)
    {
        keyboard = Keyboard.GetState();
        Active = true;
        if (reselectIndex != null)
        {
            Engage(reselectIndex.Value);
        }
    }

    public void Close()
    {
        Active = false;
    }

    public void Engage(int index)
    {
        currentComponent.Selected = false;
        currentComponent = Components[index];
        currentComponent.Selected = true;
    }
    
    public void SwapTo(Menu menu, int? reselectIndex = null)
    {
        Close();
        menu.Open(reselectIndex);
    }
}