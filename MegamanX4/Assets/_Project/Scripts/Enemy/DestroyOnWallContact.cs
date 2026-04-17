using UnityEngine;

public class DestroyOnWallContact : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == Layers.Environment)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == Layers.Environment)
            Destroy(gameObject);
    }
}
