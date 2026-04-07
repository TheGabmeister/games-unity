using UnityEngine;

public class ExplorationInitializer : MonoBehaviour
{
    [SerializeField] MapBuilder mapBuilder;
    [SerializeField] PlayerController player;

    // Set before loading scene to override default start position (e.g., from save data)
    public static Vector2Int? PendingPlayerPosition;

    void Start()
    {
        mapBuilder.BuildTestMap();

        Vector2Int startPos = PendingPlayerPosition ?? mapBuilder.PlayerStartPosition;
        PendingPlayerPosition = null;

        player.SetPosition(startPos);

        // Enable gameplay input
        GameManager.Instance?.InputManager?.EnableGameplay();
    }
}
