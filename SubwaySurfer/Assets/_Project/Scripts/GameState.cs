using UnityEngine;

public static class GameState
{
    public static int Score;
    public static int Lives;

    public static void InitGame()
    {
        Score = 0;
        Lives = 4;
    }

    public static void AddScore( int points)
    {
        Score += points;
    }
}
