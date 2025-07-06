using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Fruit : MonoBehaviour
{
    [SerializeField] int _score = 100;
    Collider _collider;
    [SerializeField] GameObject _fruitSlicedPrefab;
    void Awake()
    {
        _collider = GetComponent<Collider>();
    }
    
    void OnMouseDown()
    {
        _collider.enabled = false;
        Instantiate(_fruitSlicedPrefab, transform.position, Quaternion.identity);
        Services.GetGameManager().UpdateScore(_score);
        Destroy(gameObject);
    }
}
