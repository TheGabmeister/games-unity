using UnityEngine;
using EventBus;
using UnityEngine.Events;

public class EVL_GameStart : MonoBehaviour
{
    [SerializeField] UnityEvent _unityEvent;

    void OnEnable()
    {
        Bus<EV_GameStart>.Add(TriggerEvent);
    }

    void OnDisable()
    {
        Bus<EV_GameStart>.Remove(TriggerEvent);
    }

    void TriggerEvent()
    {
        _unityEvent?.Invoke();
    }
}
