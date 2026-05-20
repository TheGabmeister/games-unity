using UnityEngine;

public sealed class LevelRoot : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private string defaultEntryPoint = "default";
    [SerializeField] private LevelContext levelContext;

    public LevelData LevelData => levelData;
    public LevelContext LevelContext => levelContext;

    private void Awake()
    {
        if (levelContext == null) levelContext = GetComponent<LevelContext>();
    }

    // Runs after every scene's Awake — GameServices is registered by here, regardless
    // of scene-load order between Systems (additive) and the level (primary).
    private void Start()
    {
        if (!GameServices.IsRegistered)
        {
            Debug.LogError($"[LevelRoot] GameServices not registered by Start on {gameObject.name}. Systems scene did not load — check Build Settings.");
            return;
        }

        string entryPoint = defaultEntryPoint;

        if (GameServices.GameState.Current is LevelState existing)
        {
            if (existing.Data != null) levelData = existing.Data;
            if (!string.IsNullOrEmpty(existing.EntryPoint)) entryPoint = existing.EntryPoint;
        }
        else
        {
#if UNITY_EDITOR
            if (levelData == null)
            {
                Debug.LogError($"[LevelRoot] No LevelData assigned on {gameObject.name}. Direct-entry aborted.");
                return;
            }
            GameServices.GameState.EnterDirectLevel(levelData, entryPoint);
#else
            var levelId = levelData != null ? levelData.LevelId : "<unassigned>";
            Debug.LogError($"Level {levelId} loaded without GameStateMachine entry. Build config bug.");
            return;
#endif
        }

        if (levelContext == null)
        {
            Debug.LogError($"[LevelRoot] No LevelContext component found on {gameObject.name} — player won't spawn.");
            return;
        }
        Debug.Log($"[LevelRoot] Handing off to LevelContext.Begin(data={levelData?.LevelId ?? "<null>"}, entry={entryPoint}).");
        levelContext.Begin(levelData, entryPoint);
    }

    private void OnDestroy()
    {
        if (levelContext != null) levelContext.End();
    }
}
