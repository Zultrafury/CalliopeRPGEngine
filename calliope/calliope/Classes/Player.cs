﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Newtonsoft.Json;

namespace calliope.Classes;

/// <summary>
/// A sprite that is controlled by the user.
/// </summary>
public class Player : AnimatedSprite, IGameObject
{
    private float _speed = 0.05f;
    public float Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            runSpeed = Speed * 2;
        }
    }
    [JsonIgnore]
    public bool Interacting { get; set; }
    [JsonIgnore]
    public bool InteractPressed { get; set; }
    [JsonIgnore]
    public OrthographicCamera Camera { get; set; }
    [JsonIgnore]
    public Dictionary<string, string> Config { get; set; }
    public bool Frozen { get; set; }
    public enum MoveDirections
    {
        Up,
        Down,
        Left,
        Right
    }
    [JsonIgnore]
    public MoveDirections Facing { get; set; }
    [JsonIgnore]
    public RectangleF CollisionArea { get; set; }
    [JsonIgnore]
    public RectangleF InteractArea { get; set; }
    [JsonIgnore]
    public bool DrawDebugRects { get; set; } = false;
    public List<Follower> Followers { get; set; } = new();
    public Menu StatusMenu { get; set; }
    public Menu CurrentMenu { get; set; }

    private bool statusMenuChange;
    private float runSpeed;
    private float currentSpeed;
    
    public Player(Texture2D spriteTexture, Vector2 position, int spriteWidth, int spriteHeight, int frameRate) : 
        base(spriteTexture, position,spriteWidth,spriteHeight, frameRate)
    {
        runSpeed = Speed * 2;
        UpdateOrder = -100f;
    }
    
    public Player(Texture2D spriteTexture, Vector2 position, Vector2 dimensions, int  frameRate) : 
        this(spriteTexture, position, (int)dimensions.X, (int)dimensions.Y, frameRate) {}

    public new void Update(GameTime gameTime)
    {
        // Status Menu
        if (Keyboard.GetState().IsKeyDown(Keys.Tab) || Keyboard.GetState().IsKeyDown(Keys.C))
        {
            if (Frozen) return;
            
            if (!statusMenuChange)
            {
                if (CurrentMenu == null)
                {
                    StatusMenu.Sounds["select"].Play();
                    AnimIndex = AnimRange.X;
                    SwapMenu(StatusMenu,0);
                }
                else
                {
                    StatusMenu.Sounds["back"].Play();
                    SwapMenu(null);
                }
            }
            statusMenuChange = true;
        }
        else statusMenuChange = false;

        // No menu open - Free control
        if (CurrentMenu == null)
        {

            // Interacting
            if (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.E))
            {
                InteractPressed = (!Interacting);
                Interacting = true;
            }
            else
            {
                InteractPressed = false;
                Interacting = false;
            }

            if (!Frozen) // Movement
            {
                // Run Speed
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    currentSpeed = runSpeed;
                    FrameRate = 100;
                }
                else
                {
                    currentSpeed = Speed;
                    FrameRate = 150;
                }

                // Walking
                if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    Move(MoveDirections.Right, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    Move(MoveDirections.Left, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    Move(MoveDirections.Down, gameTime);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    Move(MoveDirections.Up, gameTime);
                }
                else
                {
                    AnimIndex = 0;
                    Playing = false;
                }
            }
            else Playing = false;
        }
        else Playing = false;

        CalculateCollisionArea();

        // Center Camera
        Camera.Position = Position - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        StatusMenu.Update(gameTime);

        List<AnimatedSprite> orderList =
        [
            this,
            ..Followers
        ];
        orderList.Reverse();
        float order = 0;

        foreach (AnimatedSprite sprite in orderList.OrderBy(o => o.Position.Y))
        {
            sprite.RenderOrder = order;
            order += 0.01f;
        }
    }
    
    public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        
        //StatusMenu.Draw(spriteBatch, gameTime);
        
        if (!DrawDebugRects) return;
        spriteBatch.DrawRectangle(CollisionArea,Color.Red,5);
        spriteBatch.DrawRectangle(InteractArea,Color.Blue,5);
    }

    public void SwapMenu(Menu menu, int? reselectIndex = null)
    {
        CurrentMenu?.Close();
        CurrentMenu = menu;
        CurrentMenu?.Open(reselectIndex);
    }

    public void Move(MoveDirections direction, GameTime gameTime, float modifier = 1f, MoveDirections? faceOverride = null)
    {
        Playing = true;
        
        Face(faceOverride ?? direction);
        
        DragFollowers();

        float moveAmount = currentSpeed * gameTime.ElapsedGameTime.Milliseconds * modifier * RenderScale;
        
        switch (direction)
        {
            case MoveDirections.Up:
                Position -= new Vector2(0,moveAmount);
                break;
            case MoveDirections.Down:
                Position += new Vector2(0,moveAmount);
                break;
            case MoveDirections.Left:
                Position -= new Vector2(moveAmount,0);
                break;
            case MoveDirections.Right:
                Position += new Vector2(moveAmount,0);
                break;
        }
    }

    public void Face(MoveDirections direction)
    {
        Vector2 pos = new();
        SizeF size = new SizeF(SpriteWidth, SpriteHeight) * RenderScale;
        
        switch (direction)
        {
            case MoveDirections.Up:
                AnimRange = AnimSets["walk_up"];
                Facing = MoveDirections.Up;
                pos = new Vector2(Position.X - size.Width/2, Position.Y-size.Height);
                break;
            case MoveDirections.Down:
                AnimRange = AnimSets["walk_down"];
                Facing = MoveDirections.Down;
                pos = new Vector2(Position.X - size.Width/2, Position.Y);
                break;
            case MoveDirections.Left:
                AnimRange = AnimSets["walk_left"];
                Facing = MoveDirections.Left;
                pos = new Vector2(Position.X-size.Width, Position.Y - size.Height/2);
                break;
            case MoveDirections.Right:
                AnimRange = AnimSets["walk_right"];
                Facing = MoveDirections.Right;
                pos = new Vector2(Position.X, Position.Y - size.Height/2);
                break;
        }

        InteractArea = new RectangleF(pos, size);
    }

    public void Block()
    {
        CalculateCollisionArea();
        
        DragFollowers(false);
        
        // Center Camera
        Camera.Position = Position - RenderScale * 
            new Vector2((float.Parse(Config["screenwidth"]) / 2), (float.Parse(Config["screenheight"]) / 2));
        
        //Reset Animation
        AnimIndex = AnimRange.X;
    }

    void CalculateCollisionArea()
    {
        Point pSize = (new Vector2(SpriteDimensions.X,SpriteDimensions.Y) * RenderScale).ToPoint();
        CollisionArea = new Rectangle(Position.ToPoint()-(pSize/new Point(2,2)), pSize);
    }

    void DragFollowers(bool forward = true)
    {
        if (forward)
        {
            foreach (var follower in Followers)
            {
                follower.RecordPosition(Facing,Position,FrameRate);
                follower.Drag();
            }
        }
        else
        {
            foreach (var follower in Followers)
            {
                follower.Ignore();
            }
        }
    }

    public static MoveDirections InvertMoveDirection(MoveDirections direction)
    {
        return direction switch
        {
            MoveDirections.Up => MoveDirections.Down,
            MoveDirections.Down => MoveDirections.Up,
            MoveDirections.Left => MoveDirections.Right,
            MoveDirections.Right => MoveDirections.Left,
            _ => MoveDirections.Up
        };
    }
}