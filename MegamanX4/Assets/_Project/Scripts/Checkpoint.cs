using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] string _checkpointId;
    [SerializeField] Transform _respawnPoint;

    Collider2D _trigger;

    string SceneName => gameObject.scene.name;
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

    void OnEnable()
    {
        RegisterSelf();
    }

    void OnDisable()
    {
        if (string.IsNullOrWhiteSpace(_checkpointId))
            return;

        if (Services.TryGet<ICheckpointService>(out var checkpointService))
            checkpointService.UnregisterCheckpoint(SceneName, _checkpointId);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrWhiteSpace(_checkpointId))
        {
            Debug.LogWarning("Checkpoint trigger entered without a checkpoint id.", this);
            return;
        }

        if (!other.GetComponentInParent<PlayerController>())
            return;

        if (Services.TryGet<ICheckpointService>(out var checkpointService))
            checkpointService.ActivateCheckpoint(SceneName, _checkpointId);
    }

    void RegisterSelf()
    {
        if (string.IsNullOrWhiteSpace(_checkpointId))
            return;

        if (Services.TryGet<ICheckpointService>(out var checkpointService))
            checkpointService.RegisterCheckpoint(SceneName, _checkpointId, RespawnPosition);
    }

    void EnsureTriggerCollider()
    {
        _trigger = GetComponent<Collider2D>();
        if (_trigger)
            _trigger.isTrigger = true;
    }
}
