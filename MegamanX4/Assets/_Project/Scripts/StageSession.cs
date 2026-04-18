using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class StageSession : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _hudPrefab;
    [SerializeField, Min(0f)] float _deathReloadDelay = 1f;
    

    GameObject _playerInstance;
    Health _playerHealth;
    PlayerController _playerController;
    PlayerInput _playerInput;
    InputAction _pauseAction;
    HUD _hud;
    bool _isReloading;
    bool _isPaused;

    public bool IsPaused => _isPaused;

    void Start()
    {
        if (!_playerPrefab)
        {
            Debug.LogError("StageSession is missing a player prefab reference.", this);
            return;
        }

        Vector3 defaultSpawnPosition = ResolveDefaultSpawnPosition();

        // _checkpointService = CheckpointService.Instance;
        // if (_checkpointService != null)
        // {
        //     _checkpointService.EnterScene(defaultSpawnPosition);

        //     if (_checkpointService.TryGetRespawnPosition(out var respawnPosition))
        //     {
        //         SpawnPlayer(respawnPosition);
        //         return;
        //     }
        // }

        SpawnPlayer(defaultSpawnPosition);
    }

    void SpawnPlayer(Vector3 position)
    {
        _playerInstance = Instantiate(_playerPrefab, position, Quaternion.identity);
        _playerHealth = _playerInstance.GetComponent<Health>();
        _playerController = _playerInstance.GetComponent<PlayerController>();
        _playerInput = _playerInstance.GetComponent<PlayerInput>();
        var weapons = _playerInstance.GetComponent<WeaponInventory>();

        if (_playerController)
            _playerController.Died += OnPlayerDepleted;

        BindPauseAction();

        if (!_hudPrefab)
            return;

        var hudGo = Instantiate(_hudPrefab);
        _hud = hudGo.GetComponent<HUD>();
        if (_hud)
        {
            _hud.Bind(_playerHealth, weapons);
            _hud.SetPaused(_isPaused);
        }
    }

    void OnDestroy()
    {
        UnbindPauseAction();

        if (_playerController)
            _playerController.Died -= OnPlayerDepleted;

        if (_isPaused)
            SetPaused(false);
    }

    void OnPlayerDepleted()
    {
        if (_isReloading)
            return;

        _isReloading = true;

        if (_isPaused)
            SetPaused(false);

        if (_playerController)
            _playerController.Died -= OnPlayerDepleted;

        UnbindPauseAction();

        //_checkpointService?.MarkPendingRespawn();

        StartCoroutine(ReloadSceneAfterDelay());
    }

    System.Collections.IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(_deathReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    Vector3 ResolveDefaultSpawnPosition()
    {
        var playerStart = FindPlayerStart();
        if (playerStart)
            return playerStart.transform.position;

        return ResolveFallbackSpawnPosition();
    }

    GameObject FindPlayerStart()
    {
        try
        {
            return GameObject.FindGameObjectWithTag("PlayerStart");
        }
        catch (UnityException)
        {
            return null;
        }
    }

    Vector3 ResolveFallbackSpawnPosition()
    {
#if UNITY_EDITOR
        var sceneView = SceneView.lastActiveSceneView;
        var sceneCamera = sceneView ? sceneView.camera : null;
        if (sceneCamera)
        {
            var forward = sceneCamera.transform.forward;
            float distanceToPlane;
            if (Mathf.Abs(forward.z) > 0.0001f)
                distanceToPlane = -sceneCamera.transform.position.z / forward.z;
            else
                distanceToPlane = 10f;

            if (distanceToPlane < 0f)
                distanceToPlane = 10f;

            return sceneCamera.transform.position + forward * distanceToPlane;
        }
#endif

        Debug.LogWarning("StageSession could not find a PlayerStart tag or an active Scene view camera. Spawning at the StageSession position.", this);
        return transform.position;
    }

    public void TogglePause()
    {
        if (_isReloading)
            return;

        SetPaused(!_isPaused);
    }

    void BindPauseAction()
    {
        UnbindPauseAction();

        if (_playerInput == null)
            return;

        _pauseAction = _playerInput.actions["Pause"];
        if (_pauseAction == null)
        {
            Debug.LogWarning("StageSession could not find a Pause action on the player input actions asset.", this);
            return;
        }

        _pauseAction.performed += OnPausePerformed;
    }

    void UnbindPauseAction()
    {
        if (_pauseAction == null)
            return;

        _pauseAction.performed -= OnPausePerformed;
        _pauseAction = null;
    }

    void OnPausePerformed(InputAction.CallbackContext _)
    {
        if (_playerInstance == null)
            return;

        TogglePause();
    }

    void SetPaused(bool paused)
    {
        _isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (_hud)
            _hud.SetPaused(paused);
    }
}
