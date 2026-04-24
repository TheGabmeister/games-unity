using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "NewZone", menuName = "DigimonWorld/ZoneData")]
public class ZoneData : ScriptableObject
{
    [SerializeField] private SceneReference _scene;
    [SerializeField] private Vector3 _cameraPosition;

    public SceneReference Scene => _scene;
    public Vector3 CameraPosition => _cameraPosition;
}
