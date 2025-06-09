using UnityEngine;
using PrimeTween;
using EventBus;
//using SimpleEventSystem;

public class GameManager : MonoBehaviour
{
    int _score = 0;
    int _highScore = 0;
    bool _isPreGame = false;
    bool _isMoving = false;
    float _initialSpeed = 10.0f;
    [SerializeField] GameSettings _gameSettings;
  

    [SerializeField] GameObject _playerPrefab;
    GameObject _playerInstance;
    PlayerController _playerController;
    [SerializeField] GameObject _camera;
    [SerializeField] Transform _playerSpawnPoint;
    [SerializeField] UiManager _uiManager;
  

    [Header("Obstacles")]
    [SerializeField] GameObject _obstaclePrefab;
    [SerializeField] Transform _obstacleSpawnPoint;
    [SerializeField] float _spawnRate = 2f;
    [SerializeField] float _minY = -1f;
    [SerializeField] float _maxY = 1f;


    void OnEnable()
    {
        Bus<EV_GameStart>.Add(StartGame);
        Bus<EV_GameRestart>.Add(RestartGame);
        Bus<EV_PlayerDied>.Add(HandlePlayerDeath);

    }

    void OnDisable()
    {
        Bus<EV_GameStart>.Remove(StartGame);
        Bus<EV_GameRestart>.Remove(RestartGame);
        Bus<EV_PlayerDied>.Remove(HandlePlayerDeath);
    }

    void Awake()
    {
        _initialSpeed = _gameSettings.initialSpeed;
    }

    void StartPreGame()
    {
        Sequence.Create()
            .ChainDelay(0.5f)
            .ChainCallback(() => _uiManager.ToggleGameplayUi())
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
        // _playerController.ToggleControls(true);
        // StartSpawningObstacles();
        // _isMoving = true;
        // _uiManager.DisablePreGameText();
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
            _camera.transform.position += Vector3.right * _initialSpeed * Time.deltaTime;
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
            
            .ChainDelay(0.5f)
            .ChainCallback(() => _uiManager.Init())
            .ChainCallback(() => _camera.transform.position = new Vector3(0, 0, -10))
            .ChainCallback(() => _score = 0)
            
        ;
    }
}
