using UnityEngine;
using PrimeTween;
using SimpleEventSystem;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;
    bool _isPreGame = false;
    bool _isMoving = false;
    float _speed = 10.0f;

    [SerializeField] GameSettings _gameSettings;

    [SerializeField] GameObject _playerPrefab;
    PlayerController _playerController;
    [SerializeField] GameObject _camera;
    [SerializeField] Transform _playerSpawnPoint;
    [SerializeField] UiManager _uiManager;
    [SerializeField] ScreenFader _screenFader;


    [Header("Obstacles")]
    [SerializeField] GameObject _obstaclePrefab;
    [SerializeField] Transform _obstacleSpawnPoint;
    [SerializeField] float _spawnRate = 2f;
    [SerializeField] float _minY = -1f;
    [SerializeField] float _maxY = 1f;


    void OnEnable()
    {
        Events.EnemyKilled.Sub(AddScore);
        Events.GameStart.Sub(StartPreGame);
        Events.PlayerDied.Sub(HandlePlayerDeath);
    }

    void OnDisable()
    {
        Events.EnemyKilled.Unsub(AddScore);
        Events.GameStart.Unsub(StartPreGame);
        Events.PlayerDied.Unsub(HandlePlayerDeath);
    }

    void Awake()
    {
        _spawnRate = _gameSettings.obstacleSpawnRate;
        _speed = _gameSettings.gameSpeed;
    }

    void StartPreGame()
    {
        Sequence.Create()
            .ChainCallback(() => _screenFader.FadeToBlack(0.5f))
            .ChainDelay(0.5f)
            .ChainCallback(() => _uiManager.ToggleGameplayUi())
            .ChainCallback(() => _screenFader.FadeToClear(0.5f))
            .ChainCallback(() => _playerPrefab = Instantiate(_playerPrefab,
                                                    _playerSpawnPoint.position,
                                                    _playerSpawnPoint.rotation,
                                                    _camera.transform))
            .ChainCallback(() => _playerController = _playerPrefab.GetComponent<PlayerController>())
            .ChainCallback(() => _playerController.ToggleControls(false))
            .ChainCallback(() => _isPreGame = true)
        ;
    }

    void StartGame()
    {
        _playerController.ToggleControls(true);
        StartSpawningObstacles();
        _isMoving = true;
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

        if (_isMoving)
        {
            _camera.transform.position += Vector3.right * _speed * Time.deltaTime;
        }
    }

    void AddScore(int value)
    {
        _score += value;
        Events.UiUpdateScore.Publish(_score);
    }

    void SaveHiScore(int value)
    {

    }

    void StartSpawningObstacles()
    {
        InvokeRepeating("SpawnObstacle", 0f, _spawnRate);
    }

    void HandlePlayerDeath()
    {
        CancelInvoke("SpawnObstacle");
        _isMoving = false;
    }

    void SpawnObstacle()
    {
        float randomY = Random.Range(_minY, _maxY);
        Instantiate(_obstaclePrefab, new Vector2(_obstacleSpawnPoint.position.x, randomY), Quaternion.identity);
    }

}
