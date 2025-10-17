using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace calliope.Classes;

public class Follower : Sprite, IUpdateDraw
{
    public int Order { get; set; }
    public Player Player { get; set; }
    public Queue<(Player.MoveDirections,Vector2,int)> Positions { get; set; } = new();
    private bool ignore;
    private Queue<Player.MoveDirections> oldDirs = new();
    private Vector2 oldPos;
    
    public Follower(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate, Player player, int? order = null, float followingDistance = 1) :
        base(spriteTexture, position, spriteWidth, spriteHeight, frameRate)
    {
        Player = player;
        oldPos = Position;
        
        if (order == null)
        {
            Order = Player.Followers.Count;
        }
        else Order = order.Value;

        for (int i = 0; i < 3; i++) oldDirs.Enqueue(Player.Facing);
        for (int i = 0; i < (Order + 1) * spriteWidth * followingDistance; i++) Positions.Enqueue((Player.Facing,Position,Player.FrameRate));
    }
    
    public Follower(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int frameRate, Player player, int? order = null,  float followingDistance = 1) :
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate, player, order, followingDistance) {}
    
    public new void Update(GameTime gameTime)
    {
        if (oldPos == Position)
        {
            Playing = false;
            AnimIndex = (int)AnimRange.X;
        }
        else Playing = true;
        
        oldPos = Position;
    }
    
    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
    }

    public void RecordPosition(Player.MoveDirections direction, Vector2 position, int frameRate)
    {
        if (ignore) return;
        Positions.Enqueue((direction,position,frameRate));
    }

    public void Ignore() => ignore = true;

    public void Drag()
    {

        if (ignore)
        {
            ignore = false;
            return;
        }

        var moveTuple = Positions.Dequeue();
        Position = moveTuple.Item2;
        
        if (oldDirs.Dequeue() == moveTuple.Item1) Face(moveTuple.Item1);
        oldDirs.Enqueue(moveTuple.Item1);

        FrameRate = moveTuple.Item3;
    }

    public void Face(Player.MoveDirections direction)
    {
        switch (direction)
        {
            case Player.MoveDirections.Up:
                AnimRange = new Vector2(4, 8);
                break;
            case Player.MoveDirections.Down:
                AnimRange = new Vector2(0, 4);
                break;
            case Player.MoveDirections.Left:
                AnimRange = new Vector2(12, 16);
                break;
            case Player.MoveDirections.Right:
                AnimRange = new Vector2(8, 12);
                break;
        }
    }
}