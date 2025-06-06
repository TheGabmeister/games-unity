using UnityEngine;
using PrimeTween;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;

    [Header("Obstacles")]
    [SerializeField] GameObject _obstaclePrefab;
    [SerializeField] float _spawnInterval = 2f;
    [SerializeField] float _minY = -1f;
    [SerializeField] float _maxY = 1f;
    void Update()
    {

    }

    void OnEnable()
    {
        Bus.EnemyKilled.Sub(AddScore);
        Bus.GameStart.Sub(StartGame);
    }

    void OnDisable()
    {
        Bus.EnemyKilled.Unsub(AddScore);
        Bus.GameStart.Unsub(StartGame);
    }

    void Awake()
    {
        // load high score
    }

    void StartGame()
    {
        Sequence.Create()
            .ChainCallback(() => Bus.CameraFadeToBlack.Publish(0.5f))
            .ChainDelay(0.5f)
            .ChainCallback(() => Bus.UiToggleGameplay.Publish())
            .ChainCallback(() => Bus.CameraFadeToClear.Publish(0.5f))
        ;
    }

    void AddScore(int value)
    {
        _score += value;
        Bus.UiUpdateScore.Publish(_score);
    }

    void SaveHiScore(int value)
    {

    }

    void StartSpawningObstacles()
    {
        InvokeRepeating("SpawnObstacle", 0f, _spawnInterval);
    }

    void StopSpawningObstacles()
    {
        CancelInvoke("SpawnObstacle");
    }

        void SpawnObstacle()
    {
        float randomY = Random.Range(_minY, _maxY);
        Vector2 spawnPosition = new Vector2(transform.position.x, randomY);
        Instantiate(_obstaclePrefab, spawnPosition, Quaternion.identity);
    }

}
