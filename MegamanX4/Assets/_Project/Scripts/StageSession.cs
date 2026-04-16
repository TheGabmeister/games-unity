using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class StageSession : MonoBehaviour
{
    const string DefaultPlayerStartMarker = "PlayerStart";

    [Header("Player")]
    [SerializeField] PlayerController playerPrefab;
    [SerializeField] bool spawnPlayerOnStart = true;
    [SerializeField] float respawnDelay = 1f;

    [Header("Spawn")]
    [SerializeField] Transform playerStartOverride;
    [SerializeField] string playerStartMarker = DefaultPlayerStartMarker;

    Health currentPlayerHealth;
    bool respawnInProgress;

    public PlayerController CurrentPlayer { get; private set; }
    public Transform CurrentSpawnPoint { get; private set; }

    public event Action<PlayerController> PlayerChanged;

    void Start() => InitializeStage();

    void OnDisable() => UnsubscribeCurrentPlayerHealth();

    void InitializeStage()
    {
        CurrentSpawnPoint = ResolvePlayerStart();

        var existingPlayer = FindExistingScenePlayer();
        if (existingPlayer)
        {
            RegisterPlayer(existingPlayer);

            if (spawnPlayerOnStart && playerPrefab)
                Debug.LogWarning("StageSession found an existing player in the scene and will not spawn another one.", this);

            return;
        }

        if (spawnPlayerOnStart)
            SpawnPlayerAt(CurrentSpawnPoint);
    }

    public Transform ResolvePlayerStart()
    {
        if (playerStartOverride)
            return playerStartOverride;

        if (string.IsNullOrWhiteSpace(playerStartMarker))
            playerStartMarker = DefaultPlayerStartMarker;

        foreach (var candidate in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene != gameObject.scene)
                continue;

            if (candidate.name == playerStartMarker || candidate.tag == playerStartMarker)
                return candidate;
        }

        Debug.LogWarning($"StageSession could not find a '{playerStartMarker}' marker. Falling back to the StageSession transform.", this);
        return transform;
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        if (!spawnPoint)
            return;

        CurrentSpawnPoint = spawnPoint;
    }

    public void SpawnPlayerAt(Transform spawnPoint)
    {
        if (!playerPrefab)
        {
            Debug.LogError("StageSession is missing a player prefab reference.", this);
            return;
        }

        var targetSpawn = spawnPoint ? spawnPoint : ResolvePlayerStart();
        CurrentSpawnPoint = targetSpawn;

        var player = Instantiate(playerPrefab, targetSpawn.position, targetSpawn.rotation);
        RegisterPlayer(player);
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (!player || CurrentPlayer == player)
            return;

        UnsubscribeCurrentPlayerHealth();

        CurrentPlayer = player;
        currentPlayerHealth = player.GetComponent<Health>();

        if (currentPlayerHealth)
            currentPlayerHealth.Depleted += OnCurrentPlayerDepleted;

        PlayerChanged?.Invoke(CurrentPlayer);
    }

    public void RespawnPlayer()
    {
        if (respawnInProgress)
            return;

        StartCoroutine(RespawnRoutine());
    }

    PlayerController FindExistingScenePlayer()
    {
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (player.gameObject.scene == gameObject.scene)
                return player;
        }

        return null;
    }

    void OnCurrentPlayerDepleted()
    {
        if (!respawnInProgress)
            RespawnPlayer();
    }

    IEnumerator RespawnRoutine()
    {
        respawnInProgress = true;

        var previousPlayer = CurrentPlayer;
        UnsubscribeCurrentPlayerHealth();
        CurrentPlayer = null;
        PlayerChanged?.Invoke(null);

        if (previousPlayer)
            Destroy(previousPlayer.gameObject);

        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        SpawnPlayerAt(CurrentSpawnPoint ? CurrentSpawnPoint : ResolvePlayerStart());
        respawnInProgress = false;
    }

    void UnsubscribeCurrentPlayerHealth()
    {
        if (currentPlayerHealth)
            currentPlayerHealth.Depleted -= OnCurrentPlayerDepleted;

        currentPlayerHealth = null;
    }
}
