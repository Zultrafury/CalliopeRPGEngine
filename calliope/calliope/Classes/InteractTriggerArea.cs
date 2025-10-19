using System;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

public class InteractTriggerArea(Rectangle area, Action linkedAction, Player player) : TriggerArea(area, linkedAction, player, true), IUpdateDraw
{
    public new void Update(GameTime gameTime)
    {
        if (Area.Intersects(Player.InteractArea) && Player.InteractPressed && !Player.Frozen)
        {
            LinkedAction?.Invoke();
        }
    }
}