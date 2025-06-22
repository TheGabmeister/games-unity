using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneDictionary", menuName = "Scriptable Objects/SceneDictionary")]
public class SceneDictionarySO : ScriptableObject
{
    public GenericDictionary<SceneReference, SceneType> scenes;
}

public enum SceneType
{
    Menu,
    Gameplay
}