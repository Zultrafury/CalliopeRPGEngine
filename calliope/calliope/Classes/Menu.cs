using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace calliope.Classes;

public class Menu : IUpdateDraw
{
    public float RenderScale { get; set; }
    public List<MenuComponent> Components { get; set; }
    public Dictionary<string, string> Config { get; set; }
    public OrthographicCamera Camera { get; set; }
    public Vector2 Position { get; set; }

    public Dictionary<string, SoundEffect> Sounds { get; set; } = new()
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

    public void Update(GameTime gameTime)
    {
        if (Camera != null)
        {
            Position = Camera.Center;
            //foreach (var component in Components) component.Position = Position;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            if (!keyboard.IsKeyDown(Keys.D) && !keyboard.IsKeyDown(Keys.Right)) Navigate(MenuComponent.NavDirections.Right);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            if (!keyboard.IsKeyDown(Keys.A) && !keyboard.IsKeyDown(Keys.Left)) Navigate(MenuComponent.NavDirections.Left);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
        {
            if (!keyboard.IsKeyDown(Keys.S) && !keyboard.IsKeyDown(Keys.Down)) Navigate(MenuComponent.NavDirections.Down);
        }
        else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
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
        foreach (var component in Components) component.Draw(spriteBatch, gameTime);
        spriteBatch.DrawCircle(Position,5*RenderScale,16,Color.Green,1*RenderScale);
    }

    public void SetSounds(SoundEffect navigate, SoundEffect select, SoundEffect back)
    {
        Sounds["navigate"] = navigate;
        Sounds["select"] = select;
        Sounds["back"] = back;
    }

    void Select()
    {
        Sounds["select"].Play();
        currentComponent.LinkedAction?.Invoke();
    }
    
    void Navigate(MenuComponent.NavDirections direction)
    {
        var nextcomponent = currentComponent.Navigate(direction);

        if (nextcomponent != null)
        {
            Sounds["navigate"].Play();
            currentComponent = nextcomponent;
        }
        else Sounds["back"].Play();
    }
}