using System.Collections.Generic;
using System.Linq;

namespace DungeonCollectorGUI
{
    public class GameEvent<T>
    {
        public T Data { get; }
        public GameEvent(T data) { Data = data; }
    }

    public class GameEngine
    {
        private Player player;
        private GameMap map;
        private LevelLoader levelLoader;
        private SettingsManager settingsManager;
        private List<Enemy> enemies;

        private int currentLevel;
        private int maxLevel;
        private int startX;
        private int startY;

        public Difficulty Difficulty { get; private set; }
        public Player Player => player;
        public GameMap Map => map;
        public int CurrentLevel => currentLevel;
        public List<Enemy> Enemies => enemies;

        public event Action<GameEvent<string>> OnMessage;
        public event Action OnStateChanged;
        public event Action OnGameOver;
        public event Action OnWin;

        public GameEngine()
        {
            levelLoader = new LevelLoader("levels");
            settingsManager = new SettingsManager("settings.txt");
            Difficulty = settingsManager.LoadDifficulty();
            maxLevel = 5;
            startX = 1;
            startY = 1;
            enemies = new List<Enemy>();
        }

        public void StartNewGame()
        {
            currentLevel = 1;
            player = new Player(startX, startY, GetLivesByDifficulty());
            LoadCurrentLevel();
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
            settingsManager.SaveDifficulty(difficulty);
        }

        public void MovePlayer(int dx, int dy)
        {
            int newX = player.X + dx;
            int newY = player.Y + dy;

            if (map[newX, newY] is Door door)
            {
                if (!player.HasKey)
                {
                    OnMessage?.Invoke(new GameEvent<string>("You need a key first!"));
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

           
            if (IsEnemyAt(player.X, player.Y))
            {
                player.Lives--;
                OnMessage?.Invoke(new GameEvent<string>($"Caught by enemy! Lives left: {player.Lives}"));
                player.ResetPosition(startX, startY);

                if (player.Lives <= 0)
                {
                    OnGameOver?.Invoke();
                    return;
                }
            }

            OnStateChanged?.Invoke();
        }

        public void PrepareEnemyMoves()
        {
            if (map == null) return;

            foreach (Enemy enemy in enemies)
                enemy.PrepareMove(map);

            OnStateChanged?.Invoke();
        }

        public void ExecuteEnemyMoves()
        {
            if (map == null || player == null) return;

            foreach (Enemy enemy in enemies)
                enemy.ConfirmMove();

            if (IsEnemyAt(player.X, player.Y))
            {
                player.Lives--;
                OnMessage?.Invoke(new GameEvent<string>($"Caught by enemy! Lives left: {player.Lives}"));
                player.ResetPosition(startX, startY);

                if (player.Lives <= 0)
                {
                    OnGameOver?.Invoke();
                    return;
                }
            }

            OnStateChanged?.Invoke();
        }

        public bool IsEnemyAt(int x, int y)
        {
            return enemies.Any(e => e.X == x && e.Y == y);
        }

        private void CheckFlags()
        {
            if (player.SteppedOnTrap)
            {
                if (player.Lives <= 0)
                {
                    OnGameOver?.Invoke();
                    return;
                }
                OnMessage?.Invoke(new GameEvent<string>($"Trap! Lives left: {player.Lives}"));
                player.ResetPosition(startX, startY);
            }

            if (player.ReachedExit)
            {
                if (map.HasTreasures())
                {
                    OnMessage?.Invoke(new GameEvent<string>("Collect all treasures first!"));
                    return;
                }

                if (currentLevel < maxLevel)
                {
                    currentLevel++;
                    LoadCurrentLevel();
                    OnMessage?.Invoke(new GameEvent<string>($"Level {currentLevel}! Keep going!"));
                }
                else
                {
                    OnWin?.Invoke();
                }
            }
        }

        private void LoadCurrentLevel()
        {
            map = levelLoader.Load(currentLevel);
            enemies = new List<Enemy>();
            FindEnemySpawns();
            player.HasKey = false;
            player.ResetPosition(startX, startY);
        }

        private void FindEnemySpawns()
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    if (map[x, y].Symbol == 'M')
                    {
                        enemies.Add(new Enemy(x, y));
                        map[x, y] = new Floor();
                    }
                }
            }
        }

        public void ResetCurrentLevel()
        {
            LoadCurrentLevel();
            OnStateChanged?.Invoke();
        }

        private int GetLivesByDifficulty()
        {
            switch (Difficulty)
            {
                case Difficulty.Easy: return 5;
                case Difficulty.Normal: return 3;
                case Difficulty.Hard: return 1;
                default: return 3;
            }
        }
    }
}