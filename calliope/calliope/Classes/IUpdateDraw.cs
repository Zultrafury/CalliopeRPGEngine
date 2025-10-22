using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace calliope.Classes;

public interface IUpdateDraw
{
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    
    public float RenderScale { get; set; }
}