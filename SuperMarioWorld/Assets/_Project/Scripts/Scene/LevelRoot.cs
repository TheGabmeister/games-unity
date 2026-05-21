using UnityEngine;
using UnityEngine.InputSystem;

public sealed class LevelRoot : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private string defaultEntryPoint = "default";
    [SerializeField] private LevelCamera levelCamera;
    [SerializeField] private LevelBounds levelBounds;

    public LevelData LevelData => levelData;
    public LevelCamera Camera => levelCamera;
    public LevelBounds Bounds => levelBounds;
    public GameObject CurrentPlayer { get; private set; }

    private PlayerInputManager _inputManager;
    private bool _begun;

    private void Start()
    {
        if (GameStateMachine.Instance == null)
        {
            Debug.LogError($"[LevelRoot] GameStateMachine not found by Start on {gameObject.name}. Systems scene did not load — check Build Settings.");
            return;
        }

        string entryPoint = defaultEntryPoint;

        if (GameStateMachine.Instance.Current == GameState.Level)
        {
            if (GameStateMachine.Instance.CurrentLevelData != null) levelData = GameStateMachine.Instance.CurrentLevelData;
            if (!string.IsNullOrEmpty(GameStateMachine.Instance.CurrentEntryPoint)) entryPoint = GameStateMachine.Instance.CurrentEntryPoint;
        }
        else
        {
#if UNITY_EDITOR
            if (levelData == null)
            {
                Debug.LogError($"[LevelRoot] No LevelData assigned on {gameObject.name}. Direct-entry aborted.");
                return;
            }
            GameStateMachine.Instance.EnterDirectLevel(levelData, entryPoint);
#else
            var levelId = levelData != null ? levelData.LevelId : "<unassigned>";
            Debug.LogError($"Level {levelId} loaded without GameStateMachine entry. Build config bug.");
            return;
#endif
        }

        Begin(levelData, entryPoint);
    }

    private void OnDestroy()
    {
        End();
    }

    private void Begin(LevelData data, string entryPoint)
    {
        if (_begun) return;
        _begun = true;

        _inputManager = FindAnyObjectByType<PlayerInputManager>();
        if (levelCamera != null && levelBounds != null) levelCamera.SetBounds(levelBounds);

        var spawnPos = ResolveSpawnPosition(data, entryPoint);
        CurrentPlayer = SpawnPlayer(spawnPos);

        if (CurrentPlayer != null && levelCamera != null) levelCamera.SetTarget(CurrentPlayer.transform);

        Debug.Log($"[LevelRoot] Begin(data={(data != null ? data.LevelId : "null")}, entryPoint={entryPoint}). Player spawned at {spawnPos}.");
    }

    private void End()
    {
        if (!_begun) return;
        _begun = false;

        foreach (var p in PlayerInput.all)
        {
            if (p != null) Destroy(p.gameObject);
        }
        CurrentPlayer = null;
    }

    private Vector2 ResolveSpawnPosition(LevelData data, string entryPoint)
    {
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
        if (_inputManager == null || _inputManager.playerPrefab == null)
        {
            Debug.LogError("[LevelRoot] PlayerInputManager or playerPrefab missing — can't spawn player.");
            return null;
        }

        var playerInput = _inputManager.JoinPlayer(playerIndex: 0);
        if (playerInput == null)
        {
            Debug.LogError("[LevelRoot] JoinPlayer returned null.");
            return null;
        }

        var go = playerInput.gameObject;
        go.transform.position = spawnPos;
        if (go.TryGetComponent<PlayerController>(out var pc)) pc.Teleport(spawnPos);
        return go;
    }
}
