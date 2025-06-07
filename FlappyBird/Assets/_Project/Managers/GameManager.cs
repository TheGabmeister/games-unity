using UnityEngine;
using PrimeTween;
using EventBus;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;
    bool _isPreGame = false;

    [SerializeField] GameObject _playerPrefab;
    PlayerController _playerController;
    [SerializeField] Transform _playerSpawnPoint;

    [Header("Obstacles")]
    [SerializeField] GameObject _obstaclePrefab;
    [SerializeField] float _spawnInterval = 2f;
    [SerializeField] float _minY = -1f;
    [SerializeField] float _maxY = 1f;


    void OnEnable()
    {
        Bus.EnemyKilled.Sub(AddScore);
        Bus.GameStart.Sub(StartPreGame);
    }

    void OnDisable()
    {
        Bus.EnemyKilled.Unsub(AddScore);
        Bus.GameStart.Unsub(StartPreGame);
    }

    void Awake()
    {
        // load high score
    }

    void StartPreGame()
    {
        Sequence.Create()
            .ChainCallback(() => Bus.CameraFadeToBlack.Publish(0.5f))
            .ChainDelay(0.5f)
            .ChainCallback(() => Bus.UiToggleGameplay.Publish())
            .ChainCallback(() => Bus.CameraFadeToClear.Publish(0.5f))
            .ChainCallback(() => _playerPrefab = Instantiate(_playerPrefab, _playerSpawnPoint.position, _playerSpawnPoint.rotation))
            .ChainCallback(() => _playerController = _playerPrefab.GetComponent<PlayerController>())
            .ChainCallback(() => _playerController.ToggleControls(false))
            .ChainCallback(() => _isPreGame = true)
        ;
    }

    void Update()
    {
        if (_isPreGame)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isPreGame = false;
                StartGame();
            }
        }
    }

    void StartGame()
    {
        //Bus.GameStart.Publish();
        _playerController.ToggleControls(true);
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
