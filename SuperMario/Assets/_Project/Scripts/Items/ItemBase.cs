using ScriptableObjectArchitecture;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ItemBase : MonoBehaviour
{
    [Header("Audio Clip")]
    [SerializeField] protected AudioClip _audioClip;
    [SerializeField] protected AudioClipGameEvent _onPlaySound;

    [Header("Score")]
    [SerializeField] protected int score = 0;
    [SerializeField] protected IntGameEvent _onUpdateScore;

    [Header("Call to these events...")]
    [SerializeField] protected List<GameEvent> _gameEvents;
    [SerializeField] protected UnityEvent _unityEvent;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AffectPlayer(collision.gameObject);
            CollectItem();
        }
    }

    void CollectItem()
    {
        _onPlaySound?.Raise(_audioClip);
        _onUpdateScore?.Raise(score);

        if (_gameEvents != null && _gameEvents.Count > 0)
        {
            foreach (GameEvent gameEvent in _gameEvents)
                gameEvent.Raise();
        }

        _unityEvent?.Invoke();

        Destroy(gameObject);
    }

    protected virtual void AffectPlayer(GameObject target)
    {

    }
}