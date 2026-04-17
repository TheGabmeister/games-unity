using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointService : MonoBehaviour, ICheckpointService
{
    readonly Dictionary<string, Dictionary<string, Vector3>> _checkpointPositionsByScene = new();

    string _currentSceneName;
    string _activeCheckpointId;
    Vector3 _defaultSpawnPosition;
    bool _hasDefaultSpawnPosition;
    string _pendingRespawnSceneName;

    void OnEnable()
    {
        var services = ResolveServices();
        if (!services)
        {
            Debug.LogError("CheckpointService could not find an active Services root.", this);
            return;
        }

        services.Register<CheckpointService>(this);
        services.Register<ICheckpointService>(this);
    }

    void OnDestroy()
    {
        var services = Services.Instance;
        if (!services)
            return;

        services.Unregister<ICheckpointService>(this);
        services.Unregister<CheckpointService>(this);
    }

    public void EnterScene(string sceneName, Vector3 defaultSpawnPosition)
    {
        bool preserveCheckpoint = _pendingRespawnSceneName == sceneName;

        _currentSceneName = sceneName;
        _defaultSpawnPosition = defaultSpawnPosition;
        _hasDefaultSpawnPosition = true;
        _pendingRespawnSceneName = null;

        if (!preserveCheckpoint)
            _activeCheckpointId = null;
    }

    public void MarkPendingRespawn(string sceneName)
    {
        _pendingRespawnSceneName = sceneName;
    }

    public void RegisterCheckpoint(string sceneName, string checkpointId, Vector3 respawnPosition)
    {
        if (!IsCheckpointKeyValid(sceneName, checkpointId))
            return;

        if (!_checkpointPositionsByScene.TryGetValue(sceneName, out var sceneCheckpoints))
        {
            sceneCheckpoints = new Dictionary<string, Vector3>();
            _checkpointPositionsByScene.Add(sceneName, sceneCheckpoints);
        }

        sceneCheckpoints[checkpointId] = respawnPosition;
    }

    public void UnregisterCheckpoint(string sceneName, string checkpointId)
    {
        if (!IsCheckpointKeyValid(sceneName, checkpointId))
            return;

        if (!_checkpointPositionsByScene.TryGetValue(sceneName, out var sceneCheckpoints))
            return;

        sceneCheckpoints.Remove(checkpointId);
        if (sceneCheckpoints.Count == 0)
            _checkpointPositionsByScene.Remove(sceneName);
    }

    public void ActivateCheckpoint(string sceneName, string checkpointId)
    {
        if (!IsCheckpointKeyValid(sceneName, checkpointId))
            return;

        if (_checkpointPositionsByScene.TryGetValue(sceneName, out var sceneCheckpoints) &&
            sceneCheckpoints.ContainsKey(checkpointId))
        {
            _currentSceneName = sceneName;
            _activeCheckpointId = checkpointId;
            return;
        }

        Debug.LogWarning($"Checkpoint '{checkpointId}' in scene '{sceneName}' was activated before it was registered.", this);
    }

    public bool TryGetRespawnPosition(string sceneName, out Vector3 respawnPosition)
    {
        if (_currentSceneName == sceneName &&
            !string.IsNullOrWhiteSpace(_activeCheckpointId) &&
            _checkpointPositionsByScene.TryGetValue(sceneName, out var sceneCheckpoints) &&
            sceneCheckpoints.TryGetValue(_activeCheckpointId, out respawnPosition))
        {
            return true;
        }

        if (_currentSceneName == sceneName && !string.IsNullOrWhiteSpace(_activeCheckpointId))
            _activeCheckpointId = null;

        if (_currentSceneName == sceneName && _hasDefaultSpawnPosition)
        {
            respawnPosition = _defaultSpawnPosition;
            return true;
        }

        respawnPosition = default;
        return false;
    }

    static Services ResolveServices()
    {
        return Services.Instance ? Services.Instance : Object.FindFirstObjectByType<Services>();
    }

    static bool IsCheckpointKeyValid(string sceneName, string checkpointId)
    {
        return !string.IsNullOrWhiteSpace(sceneName) && !string.IsNullOrWhiteSpace(checkpointId);
    }
}
