using UnityEngine;
using EventSystem;

public class Pickup : MonoBehaviour
{
    [SerializeField] AudioClip _pickupSound;
    [SerializeField] int _score = 100;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SfxManager.Instance.Play(_pickupSound);
            GameInstance.Instance.UpdateScore(_score);
            TriggerEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    protected virtual void TriggerEffect(GameObject other)
    {

    }
}
