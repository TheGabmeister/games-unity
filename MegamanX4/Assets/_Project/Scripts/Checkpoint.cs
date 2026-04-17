using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform _respawnPoint;

    Collider2D _trigger;

    Vector3 RespawnPosition => _respawnPoint ? _respawnPoint.position : transform.position;

    void Awake()
    {
        EnsureTriggerCollider();
    }

    void Reset()
    {
        EnsureTriggerCollider();
    }

    void OnValidate()
    {
        EnsureTriggerCollider();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponentInParent<PlayerController>())
            return;

        if (Services.TryGet<ICheckpointService>(out var checkpointService))
            checkpointService.ActivateCheckpoint(RespawnPosition);
    }

    void EnsureTriggerCollider()
    {
        _trigger = GetComponent<Collider2D>();
        if (_trigger)
            _trigger.isTrigger = true;
    }
}
