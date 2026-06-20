namespace DungeonCollectorGUI
{
    public class Enemy
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int NextX { get; private set; }
        public int NextY { get; private set; }
        public bool HasTarget { get; private set; }

        private static Random random = new Random();

        public Enemy(int x, int y)
        {
            X = x;
            Y = y;
            NextX = x;
            NextY = y;
            HasTarget = false;
        }

        public void PrepareMove(GameMap map)
        {
            int[] dx = { 0, 0, 1, -1, 0 };
            int[] dy = { -1, 1, 0, 0, 0 }; 

            int direction = random.Next(5);
            int targetX = X + dx[direction];
            int targetY = Y + dy[direction];

            if (map.IsWalkable(targetX, targetY))
            {
                NextX = targetX;
                NextY = targetY;
            }
            else
            {
                NextX = X;
                NextY = Y;
            }

            HasTarget = true;
        }

        public void ConfirmMove()
        {
            X = NextX;
            Y = NextY;
            HasTarget = false;
        }
    }
}