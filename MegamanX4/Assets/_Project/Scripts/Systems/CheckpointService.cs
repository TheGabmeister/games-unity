using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointService : PersistentSingleton<CheckpointService>
{
    Vector3 _activeCheckpointPosition;
    bool _hasActiveCheckpoint;
    Vector3 _defaultSpawnPosition;
    bool _hasDefaultSpawnPosition;
    bool _pendingRespawn;

    public void EnterScene(Vector3 defaultSpawnPosition)
    {
        _defaultSpawnPosition = defaultSpawnPosition;
        _hasDefaultSpawnPosition = true;
        bool preserveCheckpoint = _pendingRespawn;
        _pendingRespawn = false;

        if (!preserveCheckpoint)
            _hasActiveCheckpoint = false;
    }

    public void MarkPendingRespawn()
    {
        _pendingRespawn = true;
    }

    public void ActivateCheckpoint(Vector3 respawnPosition)
    {
        _activeCheckpointPosition = respawnPosition;
        _hasActiveCheckpoint = true;
    }

    public bool TryGetRespawnPosition(out Vector3 respawnPosition)
    {
        if (_hasActiveCheckpoint)
        {
            respawnPosition = _activeCheckpointPosition;
            return true;
        }

        if (_hasDefaultSpawnPosition)
        {
            respawnPosition = _defaultSpawnPosition;
            return true;
        }

        respawnPosition = default;
        return false;
    }
}
