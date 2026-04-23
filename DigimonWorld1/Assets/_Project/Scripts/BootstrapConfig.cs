using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "BootstrapConfig", menuName = "DigimonWorld/BootstrapConfig")]
public class BootstrapConfig : ScriptableObject
{
    [SerializeField] private SceneReference _bootstrapScene;
    [SerializeField] private SceneReference _gameplayScene;

    public string BootstrapScenePath => _bootstrapScene.Path;
    public string GameplayScenePath => _gameplayScene.Path;
}
