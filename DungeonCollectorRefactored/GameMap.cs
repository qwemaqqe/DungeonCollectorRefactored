public class GameMap
{
    private MapElement[,] grid;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new MapElement[height, width];
    }

    public MapElement this[int x, int y]
    {
        get { return grid[y, x]; }
        set { grid[y, x] = value; }
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsWalkable(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        return grid[y, x].IsWalkable;
    }

    public bool HasTreasures()
    {
        foreach (MapElement element in grid)
        {
            if (element is Treasure) return true;
        }
        return false;
    }

    public void Draw(Player player)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (x == player.X && y == player.Y)
                    Console.Write('@');
                else
                    Console.Write(grid[y, x].Symbol);
            }
            Console.WriteLine();
        }
    }
}