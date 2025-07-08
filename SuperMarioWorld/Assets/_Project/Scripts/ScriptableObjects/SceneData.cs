using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Scriptable Objects/SceneData")]
public class SceneData : ScriptableObject
{
    public SceneReference sceneName;
    public SceneType sceneType;
    public AudioClip music;
}

public enum SceneType
{
    Gameplay,
    MainMenu,
    Overworld
}