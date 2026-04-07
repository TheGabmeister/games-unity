using System;
using System.Collections.Generic;

public enum SaveType
{
    Manual,
    Auto,
    Quick
}

[Serializable]
public class SaveData
{
    public SaveType Type;
    public int SaveVersion = 1;
    public string Timestamp; // ISO 8601 UTC
    public string CurrentScene;
    public int PlayerGridX;
    public int PlayerGridY;
    public int FacingDirection; // 0=up, 1=right, 2=down, 3=left
    public float PlayTimeSeconds;
    public int Gil;
    public HashSet<string> ProgressionFlags = new HashSet<string>();
    public Dictionary<string, bool> WorldState = new Dictionary<string, bool>();
    // Party, inventory, etc. added in later phases
}

[Serializable]
public class SaveSlotInfo
{
    public bool Exists;
    public SaveType Type;
    public string Timestamp;
    public string LocationName;
    public float PlayTimeSeconds;
    public int Gil;
}
