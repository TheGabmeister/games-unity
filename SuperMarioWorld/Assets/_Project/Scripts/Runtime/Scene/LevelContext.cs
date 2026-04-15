using UnityEngine;
using UnityEngine.InputSystem;

namespace SMW
{
    // Per-level services: joins the player, places them at the entry-point spawn
    // marker, and wires the LevelCamera to follow. Sits as a sibling component on
    // the LevelRoot GameObject. See SPEC §4.1 / §4.4.
    //
    // Called from LevelRoot.Awake after the GameStateMachine has entered LevelState.
    public sealed class LevelContext : MonoBehaviour
    {
        [SerializeField] private LevelCamera levelCamera;
        [SerializeField] private LevelBounds levelBounds;
        [SerializeField] private LevelRunState runState;

        public LevelCamera Camera => levelCamera;
        public LevelBounds Bounds => levelBounds;
        public LevelRunState RunState => runState;
        public GameObject CurrentPlayer { get; private set; }

        private bool _begun;

        public void Begin(LevelData data, string entryPoint)
        {
            if (_begun) return;
            _begun = true;

            if (levelCamera != null && levelBounds != null) levelCamera.SetBounds(levelBounds);

            var spawnPos = ResolveSpawnPosition(data, entryPoint);
            CurrentPlayer = SpawnPlayer(spawnPos);

            if (CurrentPlayer != null && levelCamera != null) levelCamera.SetTarget(CurrentPlayer.transform);

            Debug.Log($"[LevelContext] Begin(data={data?.LevelId ?? "null"}, entryPoint={entryPoint}). Player spawned at {spawnPos} → {(CurrentPlayer != null ? CurrentPlayer.name : "NULL")}.");
        }

        public void End()
        {
            if (!_begun) return;
            _begun = false;

            var manager = GameServices.InputManager;
            if (manager != null)
            {
                foreach (var p in PlayerInput.all)
                {
                    if (p != null) Destroy(p.gameObject);
                }
            }
            CurrentPlayer = null;
        }

        private Vector2 ResolveSpawnPosition(LevelData data, string entryPoint)
        {
            // Scene-level SpawnMarker wins over LevelData entry-point coordinates —
            // markers are visible in the scene view and survive prefab edits cleanly.
            var markers = FindObjectsByType<SpawnMarker>(FindObjectsSortMode.None);
            SpawnMarker match = null;
            foreach (var m in markers)
            {
                if (m.PointName == entryPoint) { match = m; break; }
            }
            if (match == null && markers.Length > 0) match = markers[0];
            if (match != null) return match.Position;

            if (data != null && data.TryGetEntryPoint(entryPoint, out var pos)) return pos;
            return Vector2.zero;
        }

        private GameObject SpawnPlayer(Vector2 spawnPos)
        {
            var manager = GameServices.InputManager;
            if (manager == null || manager.playerPrefab == null)
            {
                Debug.LogError("[LevelContext] PlayerInputManager or playerPrefab missing — can't spawn player.");
                return null;
            }

            var playerInput = manager.JoinPlayer(playerIndex: 0);
            if (playerInput == null)
            {
                Debug.LogError("[LevelContext] JoinPlayer returned null.");
                return null;
            }

            var go = playerInput.gameObject;
            go.transform.position = spawnPos;
            if (go.TryGetComponent<PlayerController>(out var pc)) pc.Teleport(spawnPos);
            return go;
        }
    }
}
