using UnityEngine;
using EventBus;

[RequireComponent(typeof(BoxCollider2D))]
public abstract class Enemy : MonoBehaviour, IHittable
{
    [SerializeField] int _score = 0;

    public void OnHit()
    {
        Bus.EnemyKilled.Publish(_score);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var hittable = other.gameObject.GetComponent<IHittable>();
        if (hittable != null)
        {
            hittable.OnHit();
            Destroy(gameObject);
        }
    }
}
