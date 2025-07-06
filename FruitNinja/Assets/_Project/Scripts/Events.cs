using System;
using UnityEngine;

public class Events
{
    public static Action<int> GameScoreUpdated;
    public static Action ResumeGame;
    public static Action GameOver;
    public static Action Victory;
}
