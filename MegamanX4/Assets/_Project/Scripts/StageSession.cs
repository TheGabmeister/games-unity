using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class StageSession : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _hudPrefab;

    void Start()
    {
        if (!_playerPrefab)
        {
            Debug.LogError("StageSession is missing a player prefab reference.", this);
            return;
        }

        var playerStart = FindPlayerStart();
        if (playerStart)
        {
            SpawnPlayer(playerStart.transform.position);
            return;
        }

        var spawnPosition = ResolveFallbackSpawnPosition();
        SpawnPlayer(spawnPosition);
    }

    void SpawnPlayer(Vector3 position)
    {
        var player = Instantiate(_playerPrefab, position, Quaternion.identity);
        var health = player.GetComponent<Health>();
        var weapons = player.GetComponent<WeaponInventory>();

        if (!_hudPrefab) return;

        var hudGo = Instantiate(_hudPrefab);
        var hud = hudGo.GetComponent<HUD>();
        if (hud) hud.Bind(health, weapons);
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
            var distanceToPlane = Mathf.Abs(forward.z) > 0.0001f
                ? -sceneCamera.transform.position.z / forward.z
                : 10f;

            if (distanceToPlane < 0f)
                distanceToPlane = 10f;

            return sceneCamera.transform.position + forward * distanceToPlane;
        }
#endif

        Debug.LogWarning("StageSession could not find a PlayerStart tag or an active Scene view camera. Spawning at the StageSession position.", this);
        return transform.position;
    }
}
