using System.Drawing;

public interface IRenderable
{
    Color GetDisplayColor();
}

public abstract class MapElement : IRenderable
{
    public char Symbol { get; protected set; }
    public bool IsWalkable { get; protected set; }

    protected MapElement(char symbol, bool isWalkable)
    {
        Symbol = symbol;
        IsWalkable = isWalkable;
    }

    public virtual void Interact(Player player, GameMap map, int x, int y) { }

    public abstract Color GetDisplayColor();
}

public class Wall : MapElement
{
    public Wall() : base('#', false) { }
    public override Color GetDisplayColor() => Color.Gray;
}

public class Floor : MapElement
{
    public Floor() : base('.', true) { }
    public override Color GetDisplayColor() => Color.FromArgb(30, 30, 30);
}

public class Treasure : MapElement
{
    public Treasure() : base('$', true) { }
    public override Color GetDisplayColor() => Color.Gold;

    public override void Interact(Player player, GameMap map, int x, int y)
    {
        player.Score++;
        map[x, y] = new Floor();
    }
}

public class Trap : MapElement
{
    public Trap() : base('X', true) { }
    public override Color GetDisplayColor() => Color.Red;

    public override void Interact(Player player, GameMap map, int x, int y)
    {
        player.Lives--;
        player.SteppedOnTrap = true;
    }
}

public class Key : MapElement
{
    public Key() : base('K', true) { }
    public override Color GetDisplayColor() => Color.Orange;

    public override void Interact(Player player, GameMap map, int x, int y)
    {
        player.HasKey = true;
        map[x, y] = new Floor();
    }
}

public class Door : MapElement
{
    public Door() : base('D', false) { }
    public override Color GetDisplayColor() => Color.SaddleBrown;

    public void Open(GameMap map, int x, int y)
    {
        map[x, y] = new Floor();
    }
}

public class Exit : MapElement
{
    public Exit() : base('E', true) { }
    public override Color GetDisplayColor() => Color.LimeGreen;

    public override void Interact(Player player, GameMap map, int x, int y)
    {
        player.ReachedExit = true;
    }
}
public class EnemySpawn : MapElement
{
    public EnemySpawn() : base('M', true) { }
    public override Color GetDisplayColor() => Color.FromArgb(30, 30, 30);
}