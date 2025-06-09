using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] GameObject _obstaclePrefab;
    [SerializeField] Transform _spawnPoint;

    void OnTriggerEnter(Collider other)
    {
        Instantiate(_obstaclePrefab, _spawnPoint.position, _spawnPoint.rotation);
        Destroy(transform.parent.gameObject, 5);
    }
}
