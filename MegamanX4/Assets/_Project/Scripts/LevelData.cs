using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "MegamanX4/Level Data")]
public class LevelData : ScriptableObject
{
    [SerializeField] string _displayName;
    [SerializeField] SceneReference _scene;
    [SerializeField] AudioClip _music;

    public string DisplayName => _displayName;
    public SceneReference Scene => _scene;
    public AudioClip Music => _music;
}
