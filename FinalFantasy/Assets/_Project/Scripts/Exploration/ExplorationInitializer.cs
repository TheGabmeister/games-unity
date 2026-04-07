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
        player.UpdateSprite();

        // Set starting Gil if this is a fresh game (no Gil yet)
        var inv = GameManager.Instance?.InventoryManager;
        if (inv != null && inv.Gil == 0)
            inv.AddGil(400);

        // Enable gameplay input
        GameManager.Instance?.InputManager?.EnableGameplay();
        GameManager.Instance?.StateManager?.ChangeState(GameState.Exploration);
    }
}
