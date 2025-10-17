using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace calliope.Classes;

public class Wall : Tile, IUpdateDraw
{
    public Player Player { get; set; }
    public RectangleF Area { get; set; }
    public bool DrawDebugRects { get; set; } = false;
    public Wall(Texture2D spriteTexture, int tile, Vector2 position, int spriteWidth, int spriteHeight, Player player, Rectangle? area = null) :
        base(spriteTexture, tile, position, spriteWidth, spriteHeight)
    {
        Player = player;
        RenderScale = player.RenderScale;
        
        if (area != null) Area = area.Value;
        else
        {
            Point size = (SpriteDimensions * RenderScale).ToPoint();
            Area = new Rectangle(Position.ToPoint() - (size / new Point(2, 2)), size);
        }
    }

    public Wall(Texture2D spriteTexture, int tile, Vector2 position, Vector2 dimensions, Player player, Rectangle? area = null) : 
        this(spriteTexture, tile, position, (int)dimensions.X, (int)dimensions.Y, player, area) {}

    public new void Update(GameTime gameTime)
    {
        if (!Area.Intersects(Player.CollisionArea)) return;
        
        var intersection = RectangleF.Intersection(Area, Player.CollisionArea);
        
        if (intersection.Width < intersection.Height)
        {
            if (Player.Position.X < Position.X) Player.Position -= new Vector2(intersection.Width,0);
            else Player.Position += new Vector2(intersection.Width,0);
        }
        else
        {
            if (Player.Position.Y < Position.Y) Player.Position -= new Vector2(0,intersection.Height);
            else Player.Position += new Vector2(0,intersection.Height);
        }

        Player.Block();
    }

    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(Area,Color.Red,5);
    }
}