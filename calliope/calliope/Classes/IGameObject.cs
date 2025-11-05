using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace calliope.Classes;

public interface IGameObject
{
    public uint Id { get; set; }
    public Scene Scene { get; set; }
    public float RenderScale { get; set; }
    public float RenderOrder { get; set; }
    public float UpdateOrder { get; set; }
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    
}

public static class IGameObjectExtensions
{
    private static T Convert<T>(this IGameObject obj) where T : IGameObject
    {
        if (obj is T gameObject) return gameObject;
        return default;
    }
    
    public static Menu ToMenu(this IGameObject obj) => Convert<Menu>(obj);
    public static Player ToPlayer(this IGameObject obj) => Convert<Player>(obj);
    public static Follower ToFollower(this IGameObject obj) => Convert<Follower>(obj);
    public static TextDisplay ToTextDisplay(this IGameObject obj) => Convert<TextDisplay>(obj);
    public static AnimatedSprite ToAnimatedSprite(this IGameObject obj) => Convert<AnimatedSprite>(obj);
    public static DialogueBox ToDialogueBox(this IGameObject obj) => Convert<DialogueBox>(obj);
}