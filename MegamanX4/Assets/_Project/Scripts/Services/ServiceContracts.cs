using UnityEngine;

public interface IMusicService
{
    void ToggleMusic(bool value);
}

public interface ISfxService
{
    void PlaySound(AudioClip clip);
}

public interface IScreenFaderService
{
    void FadeToColor(Color color, float duration);
}

public interface ICheckpointService
{
    void EnterScene(string sceneName, Vector3 defaultSpawnPosition);
    void MarkPendingRespawn(string sceneName);
    void RegisterCheckpoint(string sceneName, string checkpointId, Vector3 respawnPosition);
    void UnregisterCheckpoint(string sceneName, string checkpointId);
    void ActivateCheckpoint(string sceneName, string checkpointId);
    bool TryGetRespawnPosition(string sceneName, out Vector3 respawnPosition);
}
