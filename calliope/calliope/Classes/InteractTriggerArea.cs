using System;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

public class InteractTriggerArea(Rectangle area, Action linkTrigger, Player player) : TriggerArea(area, linkTrigger, player, true)
{
    public new void Update(GameTime gameTime)
    {
        Point pSize = (Player.SpriteDimensions * Player.RenderScale).ToPoint();
        pRect = new Rectangle(Player.Position.ToPoint()-(pSize/new Point(2,2)), pSize);
        
        if (Area.Intersects(pRect) && Player.InteractPressed && !Player.Frozen)
        {
            LinkTrigger?.Invoke();
        }
    }
}