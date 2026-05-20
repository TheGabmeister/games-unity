using UnityEngine;

public sealed class GameSession : MonoBehaviour
{
    public LevelRunState CurrentRun { get; private set; }
    public object PendingOverworldPayload;

    public void BeginRun(LevelRunState run) => CurrentRun = run;
    public void EndRun() => CurrentRun = null;
}
