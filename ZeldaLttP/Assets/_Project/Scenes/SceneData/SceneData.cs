using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Scriptable Objects/SceneData")]
public class SceneData : ScriptableObject
{
    public string sceneName;
    
}

public enum RegionType
{
    Overworld,
    Dungeon
}