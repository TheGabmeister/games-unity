using UnityEngine;
using EventSystem;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] AudioClip _pickupSound;
    [SerializeField] int _score;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SFXManager.Instance.Play(_pickupSound);
            Events.ScoreUpdated.Raise(_score);
        }
    }
}
