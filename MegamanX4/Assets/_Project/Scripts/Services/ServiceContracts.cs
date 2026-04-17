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
    void EnterScene(Vector3 defaultSpawnPosition);
    void MarkPendingRespawn();
    void ActivateCheckpoint(Vector3 respawnPosition);
    bool TryGetRespawnPosition(out Vector3 respawnPosition);
}
