using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHittable
{
    [SerializeField] int _score = 0;

    public void OnHit()
    {
        Destroy(gameObject);

    }
}
