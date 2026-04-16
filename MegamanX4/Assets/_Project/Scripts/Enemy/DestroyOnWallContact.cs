using UnityEngine;

public class DestroyOnWallContact : MonoBehaviour
{
    int _environmentLayer;

    void Awake() => _environmentLayer = LayerMask.NameToLayer("Environment");

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == _environmentLayer)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == _environmentLayer)
            Destroy(gameObject);
    }
}
