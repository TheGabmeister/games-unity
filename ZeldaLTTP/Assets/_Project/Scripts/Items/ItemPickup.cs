using EventBus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] ItemDataSO _item;
    [SerializeField] int _amount = 1;

    [Header("Audio Clip")]
    [SerializeField] protected AudioClip _audioClip;

    [Header("Call to these events...")]
    [SerializeField] protected UnityEvent _unityEvent;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void Awake()
    {
        
    }

    void CollectItem()
    {
        //_onPlaySound?.Raise(_audioClip);

        _unityEvent?.Invoke();
        Bus<E_Inventory_Add>.Raise(new E_Inventory_Add { value = _item, amount = _amount});

        Destroy(gameObject);
    }
}
