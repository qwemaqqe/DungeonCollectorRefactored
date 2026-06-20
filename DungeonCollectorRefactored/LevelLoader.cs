public class LevelLoader
{
    private string levelsFolder;

    public LevelLoader(string levelsFolder)
    {
        this.levelsFolder = levelsFolder;
    }

    public GameMap Load(int levelNumber)
    {
        string path = Path.Combine(levelsFolder, $"level{levelNumber}.txt");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Level file not found: {path}");

        string[] lines;

        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to read level file: {ex.Message}");
        }

        if (lines.Length == 0)
            throw new Exception("Level file is empty.");

        int height = lines.Length;
        int width = lines[0].Length;

        GameMap map = new GameMap(width, height);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                char symbol = lines[y][x];
                map[x, y] = CreateElement(symbol);
            }
        }
        return map;

    }

    private MapElement CreateElement(char symbol)
    {
        switch (symbol)
        {
            case '#': return new Wall();
            case '.': return new Floor();
            case '$': return new Treasure();
            case 'X': return new Trap();
            case 'K': return new Key();
            case 'E': return new Exit();
            case 'D': return new Door();
            case 'M': return new EnemySpawn();
            default: return new Floor();
        }
    }
}