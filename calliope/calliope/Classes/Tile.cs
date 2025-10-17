using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

/// <summary>
/// A static sprite indexed from a tileset image.
/// </summary>
public class Tile : Sprite, IUpdateDraw
{
    public Tile(Texture2D spriteTexture, int tile, Vector2 position, int spriteWidth, int spriteHeight) : 
        base(spriteTexture, position,spriteWidth,spriteHeight, 0)
    {
        Playing = false;
        AnimRange = new(tile, tile + 1);
    }
    
    public Tile(Texture2D spriteTexture, int tile, Vector2 position, Vector2 dimensions) : 
        this(spriteTexture, tile, position,(int)dimensions.X, (int)dimensions.Y) {}
   
}