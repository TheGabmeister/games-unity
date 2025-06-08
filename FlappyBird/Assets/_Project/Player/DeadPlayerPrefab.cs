using UnityEngine;
using SimpleEventSystem;

public class DeadPlayerPrefab : MonoBehaviour
{
    void OnEnable()
    {
        Events.GameRestart.Sub(DestroySelf);
    }

    void OnDisable()
    {
        Events.GameRestart.Unsub(DestroySelf);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
