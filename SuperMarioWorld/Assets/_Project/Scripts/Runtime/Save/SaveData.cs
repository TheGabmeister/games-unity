using System;
using System.Collections.Generic;

[Serializable]
public sealed class SaveData
{
    public int lives = 5;
    public int score;
    public int totalCoins;
    public string currentOverworldNode;
    public Dictionary<string, LevelCompletionFlags> levelCompletions = new();
    public Dictionary<string, ulong> dragonCoinsByLevel = new();
    public SwitchPalaceStates switchPalaces = new();
    public AudioPreferences audio = new();
}

[Serializable]
public sealed class LevelCompletionFlags
{
    public bool normalExit;
    public bool secretExit;
}

[Serializable]
public sealed class SwitchPalaceStates
{
    public bool yellow;
    public bool green;
    public bool red;
    public bool blue;
}

[Serializable]
public sealed class AudioPreferences
{
    public float master = 1f;
    public float music = 1f;
    public float sfx = 1f;
}
