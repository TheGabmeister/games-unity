using UnityEngine;
using EventBus;

public abstract class Enemy : MonoBehaviour, IHittable
{
    [SerializeField] int _score = 0;

    public void OnHit()
    {
        Bus.EnemyKilled.Publish(_score);
        Destroy(gameObject);
    }
}
