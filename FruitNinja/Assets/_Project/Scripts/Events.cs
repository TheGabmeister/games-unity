using System;
using UnityEngine;

public class Events
{
    public static Action<int> GameScoreUpdated;
    public static Action GameClassicModeStarted;
    public static Action GameClassicModeEnded;
    public static Action GameZenModeStarted;
    public static Action GameZenModeEnded;
    public static Action<int> GameLivesUpdated;
    public static Action<int> GameTimerUpdated;

    public static Action UiNewGameClicked;
}

public enum UiEvents
{
    NewGameClicked,
    DojoClicked,
}