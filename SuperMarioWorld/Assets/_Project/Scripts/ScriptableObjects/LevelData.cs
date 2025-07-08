using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public SceneReference levelName;
    public LevelType levelType;
    public AudioClip music;
    public int timeLimit;
}

public enum LevelType
{
    Gameplay,
    MainMenu,
    Overworld
}