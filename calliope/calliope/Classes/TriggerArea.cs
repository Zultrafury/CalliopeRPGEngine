using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

/// <summary>
/// An area that calls a function once the player overlaps it.
/// </summary>
/// <param name="area">The Rectangle denoting the area.</param>
/// <param name="linkedAction">The function to call once entered.</param>
/// <param name="player">The player to check for overlaps.</param>
/// <param name="constantTrigger">Whether to constantly call the function (true) or only call it once when entered (false).</param>
public class TriggerArea(Rectangle area, ICommand linkedAction, Player player, bool constantTrigger) : IGameObject
{
    public RectangleF Area { get; set; } = area;
    public ICommand LinkedAction { get; set; } = linkedAction;
    public bool ConstantTrigger { get; set; } = constantTrigger;
    [JsonIgnore]
    public Player Player { get; set; } = player;
    public float RenderScale { get; set; } = 1f;
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    [JsonIgnore]
    public bool DrawDebugRects { get; set; } = false;
    private bool triggered;

    [JsonIgnore]
    public Scene Scene { get; set; }
    public uint Id { get; set; }

    public void SceneInit(Scene scene)
    {
        Scene = scene;
    }

    /// <summary>
    /// This updates the area. Call this in the update loop.
    /// </summary>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Draw"/>
    public void Update(GameTime gameTime)
    {
        if (!Area.Intersects(Player.CollisionArea))
        {
            triggered = false;
            return;
        }
        
        if (ConstantTrigger) LinkedAction?.Execute();
        else
        {
            if (!triggered)
            {
                triggered = true;
                LinkedAction?.Execute();
            }
        }
    }

    /// <summary>
    /// This draws the area (for debug purposes). Call this in the draw loop.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use for drawing.</param>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Update"/>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(Area,Color.Red,5);
    }
}