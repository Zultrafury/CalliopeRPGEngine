using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace calliope.Classes;

/// <summary>
/// A static sprite indexed from a tileset image.
/// </summary>
public class Tile : Sprite
{
    public Tile(Texture2D spriteTexture, int tile, int spriteWidth, int spriteHeight) : base(spriteTexture,0,spriteWidth,spriteHeight)
    {
        Playing = false;
        AnimRange = new(tile, tile + 1);
    }
    
    public Tile(Texture2D spriteTexture, int tile, Vector2 dimensions) : this(spriteTexture, tile, (int)dimensions.X, (int)dimensions.Y) {}
}