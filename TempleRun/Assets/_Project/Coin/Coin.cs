using UnityEngine;
using EventBus;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Bus<EV_CoinCollected>.Raise();
        Destroy(gameObject);
    }
}
