using UnityEngine;

namespace SMW
{
    // Named position marker in a Level scene. LevelContext resolves the entry point
    // (e.g. "default", "midway", "pipe_from_subarea") to one of these by searching
    // the scene for the first SpawnMarker whose name matches.
    public sealed class SpawnMarker : MonoBehaviour
    {
        [SerializeField] private string pointName = "default";

        public string PointName => pointName;
        public Vector2 Position => transform.position;
    }
}
