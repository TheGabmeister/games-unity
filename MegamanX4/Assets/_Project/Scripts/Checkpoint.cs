using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponentInParent<PlayerController>())
            return;

        if (Services.TryGet<ICheckpointService>(out var checkpointService))
            checkpointService.ActivateCheckpoint(transform.position);
    }
}
