using UnityEngine;

public enum FeedbackId
{
    None = 0,
    BrickShards, CoinSparkle, StompPuff, ScorePopup, DustKick, Splash, FireballExplode
}

public sealed class FeedbackService : MonoBehaviour
{
    // Phase 0 stub: records the last spawn for tests. Full prefab catalog lands later.
    public FeedbackId LastSpawned { get; private set; }
    public Vector3 LastSpawnPos { get; private set; }

    public void Spawn(FeedbackId id, Vector3 pos)
    {
        LastSpawned = id;
        LastSpawnPos = pos;
    }
}
