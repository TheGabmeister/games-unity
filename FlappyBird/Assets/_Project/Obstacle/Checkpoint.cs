using UnityEngine;
using SimpleEventSystem;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Events.CheckpointPassed.Publish();
        Destroy(gameObject);
    }
}
