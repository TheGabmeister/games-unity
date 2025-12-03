using UnityEngine;
using UnityEngine.Events;

public class PlayerDetector2D : MonoBehaviour
{
    [SerializeField] string _colliderTag = "Player";

    [Header("Player in range...")]
    [SerializeField] UnityEvent<GameObject> _playerInRangeEvents;

    [Header("Player out of range...")]
    [SerializeField] UnityEvent<GameObject> _playerOutOfRangeEvents;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(_colliderTag) && collision.gameObject.CompareTag(_colliderTag))
        {
            _playerInRangeEvents?.Invoke(collision.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!string.IsNullOrEmpty(_colliderTag) && collision.gameObject.CompareTag(_colliderTag))
        {
            _playerOutOfRangeEvents?.Invoke(null);
        }
    }
}
