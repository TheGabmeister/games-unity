using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] float minY = -1f;
    [SerializeField] float maxY = 1f;

    void Start()
    {
        InvokeRepeating("SpawnObstacle", 0f, spawnInterval);
    }

    void SpawnObstacle()
    {
        float randomY = Random.Range(minY, maxY);
        Vector2 spawnPosition = new Vector2(transform.position.x, randomY);
        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
    }

    public void StopSpawningObstacle()
    {
        CancelInvoke("SpawnObstacle");
    }
}
