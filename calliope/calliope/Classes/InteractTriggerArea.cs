using MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

public class InteractTriggerArea(RectangleF area, ICommand linkedAction, Player player) : TriggerArea(area, linkedAction, player, true), IGameObject
{
    public new void Update(GameTime gameTime)
    {
        var area = new RectangleF(Area.Position * RenderScale,Area.Size * RenderScale);
        if (area.Intersects(Player.InteractArea) && Player.InteractPressed && !Player.Frozen)
        {
            LinkedAction?.Execute();
        }
    }
}