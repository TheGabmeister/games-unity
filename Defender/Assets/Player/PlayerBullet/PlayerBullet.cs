using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerBullet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var hittable = other.GetComponent<IHittable>();
        if (hittable != null)
        {
            hittable.OnHit();
            Destroy(gameObject);
        }
    }
}
