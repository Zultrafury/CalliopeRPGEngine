using System;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

public class InteractTriggerArea(Rectangle area, ICommand linkedAction, Player player) : TriggerArea(area, linkedAction, player, true), IGameObject
{
    public new void Update(GameTime gameTime)
    {
        if (Area.Intersects(Player.InteractArea) && Player.InteractPressed && !Player.Frozen)
        {
            LinkedAction?.Execute();
        }
    }
}