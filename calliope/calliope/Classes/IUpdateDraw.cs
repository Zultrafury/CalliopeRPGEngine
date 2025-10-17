using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace calliope.Classes;

public interface IUpdateDraw
{
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}