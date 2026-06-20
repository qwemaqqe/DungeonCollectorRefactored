public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class Game
{
    private Player player;
    private GameMap map;
    private LevelLoader levelLoader;
    private SettingsManager settingsManager;

    private Difficulty difficulty;
    private int currentLevel;
    private int maxLevel;
    private int startX;
    private int startY;
    private bool isRunning;
    private GameMap originalMap;

    public Game()
    {
        levelLoader = new LevelLoader("levels");
        settingsManager = new SettingsManager("settings.txt");
        difficulty = settingsManager.LoadDifficulty();
        maxLevel = 3;
        startX = 1;
        startY = 1;
        isRunning = true;
    }

    public void Run()
    {
        ShowMenu();

        while (isRunning)
        {
            Console.Clear();
            DrawHUD();
            map.Draw(player);
            ConsoleKey key = Console.ReadKey(true).Key;
            HandleInput(key);
        }
    }

    //menu

    private void ShowMenu()
    {
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("=== Dungeon Collector ===");
            Console.WriteLine("1 - Start Game");
            Console.WriteLine("2 - Rules");
            Console.WriteLine($"3 - Difficulty (current: {difficulty})");
            Console.WriteLine("4 - Exit");
            Console.Write("Choose an option: ");

            choice = Console.ReadLine();

            if (choice == "2")
                ShowRules();
            else if (choice == "3")
                ShowDifficultyMenu();
            else if (choice != "1" && choice != "4")
            {
                Console.WriteLine("Invalid choice. Press any key...");
                Console.ReadKey(true);
            }
        } while (choice != "1" && choice != "4");

        if (choice == "4")
            isRunning = false;
        else if (choice == "1")
            StartNewGame();
    }

    private void ShowRules()
    {
        Console.Clear();
        Console.WriteLine("=== RULES ===");
        Console.WriteLine("Use W A S D to move.");
        Console.WriteLine("Collect all treasures ($) then reach the exit (E).");
        Console.WriteLine("Pick up the key (K) to open the door (D).");
        Console.WriteLine("Avoid traps (X) - they remove a life.");
        Console.WriteLine("R - restart level, Esc - quit.");
        Console.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }

    private void ShowDifficultyMenu()
    {
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("=== Difficulty ===");
            Console.WriteLine("1 - Easy (5 lives)");
            Console.WriteLine("2 - Normal (3 lives)");
            Console.WriteLine("3 - Hard (1 life)");
            Console.Write("Choose difficulty: ");

            choice = Console.ReadLine();

            switch (choice)
            {
                case "1": difficulty = Difficulty.Easy; break;
                case "2": difficulty = Difficulty.Normal; break;
                case "3": difficulty = Difficulty.Hard; break;
                default:
                    Console.WriteLine("Invalid choice. Press any key...");
                    Console.ReadKey(true);
                    break;
            }
        } while (choice != "1" && choice != "2" && choice != "3");

        settingsManager.SaveDifficulty(difficulty);
        Console.WriteLine("Saved! Press any key...");
        Console.ReadKey(true);
    }

    private void ShowGameOverScreen()
    {
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("=== GAME OVER ===");
            Console.WriteLine($"Score: {player.Score}");
            Console.WriteLine($"Moves: {player.Moves}");
            Console.WriteLine($"Level reached: {currentLevel}");
            Console.WriteLine("\n1 - Play again");
            Console.WriteLine("2 - Main menu");
            Console.WriteLine("3 - Exit");
            Console.Write("Choose an option: ");

            choice = Console.ReadLine();

            if (choice == "1") StartNewGame();
            else if (choice == "2") ShowMenu();
            else if (choice == "3") isRunning = false;
            else
            {
                Console.WriteLine("Invalid choice. Press any key...");
                Console.ReadKey(true);
            }
        } while (choice != "1" && choice != "2" && choice != "3");
    }

    private void ShowWinScreen()
    {
        string choice;
        do
        {
            Console.Clear();
            Console.WriteLine("=== YOU WIN ===");
            Console.WriteLine("All levels completed!");
            Console.WriteLine($"Score: {player.Score}");
            Console.WriteLine($"Moves: {player.Moves}");
            Console.WriteLine($"Difficulty: {difficulty}");
            Console.WriteLine("\n1 - Play again");
            Console.WriteLine("2 - Main menu");
            Console.WriteLine("3 - Exit");
            Console.Write("Choose an option: ");

            choice = Console.ReadLine();

            if (choice == "1") StartNewGame();
            else if (choice == "2") ShowMenu();
            else if (choice == "3") isRunning = false;
            else
            {
                Console.WriteLine("Invalid choice. Press any key...");
                Console.ReadKey(true);
            }
        } while (choice != "1" && choice != "2" && choice != "3");
    }

    //hud

    private void DrawHUD()
    {
        Console.WriteLine("=== Dungeon Collector ===");
        Console.WriteLine($"Level: {currentLevel}/{maxLevel}  |  Difficulty: {difficulty}");
        Console.WriteLine($"Lives: {player.Lives}  |  Score: {player.Score}  |  Moves: {player.Moves}");
        if (currentLevel == 3) Console.WriteLine($"Key: {(player.HasKey ? "Yes" : "No")}");
        Console.WriteLine("Controls: W A S D  |  R - restart  |  Esc - quit");
        Console.WriteLine();
    }

    //input

    private void HandleInput(ConsoleKey key)
    {
        int newX = player.X;
        int newY = player.Y;

        switch (key)
        {
            case ConsoleKey.W: newY--; break;
            case ConsoleKey.S: newY++; break;
            case ConsoleKey.A: newX--; break;
            case ConsoleKey.D: newX++; break;
            case ConsoleKey.R: ResetCurrentLevel(); return;
            case ConsoleKey.Escape: isRunning = false; return;
            default: return;
        }

        
        if (map[newX, newY] is Door door)
        {
            if (!player.HasKey)
            {
                Console.Clear();
                DrawHUD();
                map.Draw(player);
                Console.WriteLine("\nYou need a key first!");
                Console.ReadKey(true);
                return;
            }
            else
            {
                door.Open(map, newX, newY);
                player.HasKey = false;
            }
        }

        if (!map.IsWalkable(newX, newY)) return;

        player.X = newX;
        player.Y = newY;
        player.Moves++;

        map[newX, newY].Interact(player, map, newX, newY);

        CheckFlags();

        player.ResetFlags();
    }

    private void CheckFlags()
    {
        if (player.SteppedOnTrap)
        {
            Console.Clear();
            DrawHUD();
            map.Draw(player);

            if (player.Lives <= 0)
            {
                ShowGameOverScreen();
                return;
            }

            Console.WriteLine($"\nYou stepped on a trap! Lives left: {player.Lives}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            player.ResetPosition(startX, startY);
        }

        if (player.ReachedExit)
        {
            if (map.HasTreasures())
            {
                Console.Clear();
                DrawHUD();
                map.Draw(player);
                Console.WriteLine("\nCollect all treasures first!");
                Console.ReadKey(true);
                return;
            }

            if (currentLevel < maxLevel)
            {
                currentLevel++;
                LoadCurrentLevel();
            }
            else
            {
                ShowWinScreen();
            }
        }
    }

    //levels

    private void StartNewGame()
    {
        currentLevel = 1;
        player = new Player(startX, startY, GetLivesByDifficulty());
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        try
        {
            map = levelLoader.Load(currentLevel);
            originalMap = levelLoader.Load(currentLevel);
            player.HasKey = false;
            player.ResetPosition(startX, startY);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading level: {ex.Message}");
            Console.ReadKey(true);
            isRunning = false;
        }
    }

    private void ResetCurrentLevel()
    {
        map = levelLoader.Load(currentLevel);
        player.HasKey = false;
        player.ResetPosition(startX, startY);
    }

    private int GetLivesByDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 5;
            case Difficulty.Normal: return 3;
            case Difficulty.Hard: return 1;
            default: return 3;
        }
    }
}