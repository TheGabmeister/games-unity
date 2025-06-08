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
    GameObject _playerInstance;
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
        Events.CheckpointPassed.Sub(AddScore);
        Events.GameStart.Sub(StartPreGame);
        Events.GameRestart.Sub(RestartGame);
        Events.PlayerDied.Sub(HandlePlayerDeath);
    }

    void OnDisable()
    {
        Events.CheckpointPassed.Unsub(AddScore);
        Events.GameStart.Unsub(StartPreGame);
        Events.GameRestart.Unsub(RestartGame);
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
            .ChainCallback(() => _playerInstance = Instantiate(_playerPrefab,
                                                    _playerSpawnPoint.position,
                                                    _playerSpawnPoint.rotation,
                                                    _camera.transform))
            .ChainCallback(() => _playerController = _playerInstance.GetComponent<PlayerController>())
            .ChainCallback(() => _playerController.ToggleControls(false))
            .ChainCallback(() => _isPreGame = true)
        ;
    }

    void StartGame()
    {
        _playerController.ToggleControls(true);
        StartSpawningObstacles();
        _isMoving = true;
        _uiManager.DisablePreGameText();
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

    void AddScore()
    {
        _score += 1;
        _uiManager.UpdateScore(_score);
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
        _uiManager.StartGameOverUiSequence();
    }

    void SpawnObstacle()
    {
        float randomY = Random.Range(_minY, _maxY);
        Instantiate(_obstaclePrefab, new Vector2(_obstacleSpawnPoint.position.x, randomY), Quaternion.identity);
    }

    void RestartGame()
    {
        Sequence.Create()
            .ChainCallback(() => _screenFader.FadeToBlack(0.5f))
            .ChainDelay(0.5f)
            .ChainCallback(() => _uiManager.Init())
            .ChainCallback(() => _camera.transform.position = new Vector3(0, 0, -10))
            .ChainCallback(() => _score = 0)
            .ChainCallback(() => _screenFader.FadeToClear(0.5f))
        ;
    }
}
