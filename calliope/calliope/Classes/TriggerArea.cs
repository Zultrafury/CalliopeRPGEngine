using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace calliope.Classes;

/// <summary>
/// An area that calls a function once the player overlaps it.
/// </summary>
/// <param name="area">The Rectangle denoting the area.</param>
/// <param name="linkTrigger">The function to call once entered.</param>
/// <param name="player">The player to check for overlaps.</param>
/// <param name="constantTrigger">Whether to constantly call the function (true) or only call it once when entered (false).</param>
public class TriggerArea(Rectangle area, Action linkTrigger, Player player, bool constantTrigger)
{
    public Rectangle Area { get; set; } = area;
    public Action LinkTrigger { get; set; } = linkTrigger;
    public bool ConstantTrigger { get; set; } = constantTrigger;
    public Player Player { get; set; } = player;
    private bool triggered = false;
    protected Rectangle pRect;

    /// <summary>
    /// This updates the area. Call this in the update loop.
    /// </summary>
    /// <param name="gameTime">The <see cref="GameTime"/> of the game.</param>
    /// <seealso cref="Draw"/>
    public void Update(GameTime gameTime)
    {
        Point pSize = (Player.SpriteDimensions * Player.RenderScale).ToPoint();
        pRect = new Rectangle(Player.Position.ToPoint()-(pSize/new Point(2,2)), pSize);
        if (!Area.Intersects(pRect))
        {
            triggered = false;
            return;
        }
        
        if (ConstantTrigger) LinkTrigger?.Invoke();
        else
        {
            if (!triggered)
            {
                triggered = true;
                LinkTrigger?.Invoke();
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
        spriteBatch.DrawRectangle(Area.ToRectangleF(),Color.Red,5);
        spriteBatch.DrawRectangle(pRect.ToRectangleF(),Color.Red,5);
    }
}