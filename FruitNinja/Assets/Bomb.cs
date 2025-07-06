using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bomb : MonoBehaviour
{
    Collider _collider;
    void Awake()
    {
        _collider = GetComponent<Collider>();
    }
    
    void OnMouseDown()
    {
        _collider.enabled = false;
        Services.GetGameManager().BombStrucked();
        Destroy(gameObject);
    }
}
