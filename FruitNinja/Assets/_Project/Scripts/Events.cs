using System;
using UnityEngine;

public class Events
{
    public static Action<int> GameScoreUpdated;
    public static Action GameStarted;
    public static Action GameEnded;
    public static Action<int> GameLivesUpdated;
    
    public static Action<Color, float> ScreenFadeToColor;
}
