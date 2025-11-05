using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public class Follower : AnimatedSprite, IGameObject
{
    [JsonIgnore]
    public int Order { get; set; }
    private Queue<(Player.MoveDirections,Vector2,int)> Positions { get; set; } = new();
    private Queue<Player.MoveDirections> oldDirs = new();
    private bool ignore;
    private Vector2 oldPos;
    
    public Follower(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate, Player player, int? order = null, float followingDistance = 1) :
        base(spriteTexture, position, spriteWidth, spriteHeight, frameRate)
    {
        oldPos = Position;
        
        if (order == null)
        {
            Order = player.Followers.Count;
        }
        else Order = order.Value;

        for (int i = 0; i < 3; i++) oldDirs.Enqueue(player.Facing);
        for (int i = 0; i < (Order + 1) * spriteWidth * followingDistance; i++) Positions.Enqueue((player.Facing,Position,player.FrameRate));
    }
    
    public Follower(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int frameRate, Player player, int? order = null,  float followingDistance = 1) :
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate, player, order, followingDistance) {}
    
    public new void Update(GameTime gameTime)
    {
        if (oldPos == Position)
        {
            Playing = false;
            AnimIndex = AnimRange.X;
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
                AnimRange = AnimSets["walk_up"];
                break;
            case Player.MoveDirections.Down:
                AnimRange = AnimSets["walk_down"];
                break;
            case Player.MoveDirections.Left:
                AnimRange = AnimSets["walk_left"];
                break;
            case Player.MoveDirections.Right:
                AnimRange = AnimSets["walk_right"];
                break;
        }
    }
}