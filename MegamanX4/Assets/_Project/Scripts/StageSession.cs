using UnityEngine;
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
    bool _isReloading;
    ICheckpointService _checkpointService;

    void Start()
    {
        if (!_playerPrefab)
        {
            Debug.LogError("StageSession is missing a player prefab reference.", this);
            return;
        }

        Vector3 defaultSpawnPosition = ResolveDefaultSpawnPosition();

        Services.TryGet(out _checkpointService);
        if (_checkpointService != null)
        {
            _checkpointService.EnterScene(defaultSpawnPosition);

            if (_checkpointService.TryGetRespawnPosition(out var respawnPosition))
            {
                SpawnPlayer(respawnPosition);
                return;
            }
        }

        SpawnPlayer(defaultSpawnPosition);
    }

    void SpawnPlayer(Vector3 position)
    {
        _playerInstance = Instantiate(_playerPrefab, position, Quaternion.identity);
        _playerHealth = _playerInstance.GetComponent<Health>();
        _playerController = _playerInstance.GetComponent<PlayerController>();
        var weapons = _playerInstance.GetComponent<WeaponInventory>();

        if (_playerController)
            _playerController.Died += OnPlayerDepleted;

        if (!_hudPrefab) 
            return;

        var hudGo = Instantiate(_hudPrefab);
        var hud = hudGo.GetComponent<HUD>();
        if (hud) 
            hud.Bind(_playerHealth, weapons);
    }

    void OnDestroy()
    {
        if (_playerController)
            _playerController.Died -= OnPlayerDepleted;
    }

    void OnPlayerDepleted()
    {
        if (_isReloading)
            return;

        _isReloading = true;

        if (_playerController)
            _playerController.Died -= OnPlayerDepleted;

        _checkpointService?.MarkPendingRespawn();

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
}
