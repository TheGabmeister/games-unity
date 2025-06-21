using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Scriptable Objects/SceneData")]
public class SceneData : ScriptableObject
{
    public GenericDictionary<SceneReference, SceneType> SceneList;
}

public enum SceneType
{
    Menu,
    Gameplay
}