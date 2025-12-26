using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level Data", order = 0)]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public string displayName;
    public AudioClip music;
    public int time;
}