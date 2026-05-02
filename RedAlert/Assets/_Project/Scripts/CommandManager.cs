using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public static CommandManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        if (InputManager.Instance.Command.WasPressedThisFrame())
            HandleMoveCommand();

        if (InputManager.Instance.Stop.WasPressedThisFrame())
            HandleStop();
    }

    void HandleMoveCommand()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        Vector3 world = Camera.main.ScreenToWorldPoint(InputManager.Instance.MousePosition);
        Vector2Int targetCell = MapManager.Instance.WorldToCell(world);

        Debug.Log($"Move command: {selected.Count} unit(s) to {targetCell}");

        foreach (var selectable in selected)
        {
            var mover = selectable.GetComponent<Mover>();
            if (mover != null)
                mover.MoveTo(targetCell);
        }
    }

    void HandleStop()
    {
        var selected = SelectionManager.Instance.Selected;
        if (selected.Count == 0) return;

        foreach (var selectable in selected)
        {
            var mover = selectable.GetComponent<Mover>();
            if (mover != null)
                mover.Stop();
        }
    }
}
