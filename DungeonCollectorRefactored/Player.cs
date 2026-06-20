public class Player
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Score { get; set; }
    public int Lives { get; set; }
    public int Moves { get; set; }
    public bool HasKey { get; set; }
    public bool SteppedOnTrap { get; set; }
    public bool ReachedExit { get; set; }

    public Player(int startX, int startY, int startLives)
    {
        X = startX;
        Y = startY;
        Lives = startLives;
        Score = 0;
        Moves = 0;
        HasKey = false;
        SteppedOnTrap = false;
        ReachedExit = false;
    }
    public void ResetFlags()
    {
        SteppedOnTrap = false;
        ReachedExit = false;
    }

    public void ResetPosition(int startX, int startY)
    {
        X = startX;
        Y = startY;
    }
}
