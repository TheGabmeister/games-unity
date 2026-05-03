using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D _selectCursor;
    [SerializeField] private Texture2D _moveCursor;
    [SerializeField] private Texture2D _attackCursor;
    [SerializeField] private Texture2D _harvestCursor;
    [SerializeField] private Texture2D _noGoCursor;
    [SerializeField] private Texture2D _sellCursor;
    [SerializeField] private Texture2D _repairCursor;

    private CursorType _currentCursor = CursorType.None;
    private Camera _cam;

    private static readonly Vector2 HotspotTopLeft = Vector2.zero;
    private static readonly Vector2 HotspotCenter = new(15, 15);

    public static CursorManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        _cam = Camera.main;
    }

    void Update()
    {
        var desired = DetermineCursor();
        if (desired != _currentCursor)
        {
            _currentCursor = desired;
            ApplyCursor(desired);
        }
    }

    CursorType DetermineCursor()
    {
        if (SellRepairManager.Instance != null && SellRepairManager.Instance.SellMode)
            return CursorType.Sell;
        if (SellRepairManager.Instance != null && SellRepairManager.Instance.RepairMode)
            return CursorType.Repair;
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
            return CursorType.Select;

        if (InputManager.Instance == null || _cam == null)
            return CursorType.Select;

        Vector2 mouseScreen = InputManager.Instance.MousePosition;
        float viewportRight = Screen.width * (1f - RTSCamera.Instance.SidebarWidthFraction);
        if (mouseScreen.x > viewportRight)
            return CursorType.Select;

        var selected = SelectionManager.Instance?.Selected;
        if (selected == null || selected.Count == 0)
            return CursorType.Select;

        Vector3 world = _cam.ScreenToWorldPoint(mouseScreen);
        Vector2Int cell = MapManager.Instance.WorldToCell(world);

        if (!MapManager.Instance.IsInBounds(cell))
            return CursorType.NoGo;

        Entity target = MapManager.Instance.GetEntityAt(cell);
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;

        if (InputManager.Instance.IsCtrlHeld)
            return CursorType.Attack;

        if (InputManager.Instance.IsAltHeld)
            return CursorType.Move;

        if (target != null && !target.IsDead)
        {
            if (PlayerManager.Instance.AreEnemies(localPlayer, target.OwnerPlayerIndex))
                return HasAttacker(selected) ? CursorType.Attack : CursorType.Select;

            if (target.GetComponent<Refinery>() != null && HasHarvester(selected))
                return CursorType.Harvest;
        }

        if (MapManager.Instance.HasOre(cell) && HasHarvester(selected))
            return CursorType.Harvest;

        if (HasMover(selected))
            return CursorType.Move;

        return CursorType.Select;
    }

    void ApplyCursor(CursorType type)
    {
        var (texture, hotspot) = type switch
        {
            CursorType.Move => (_moveCursor, HotspotCenter),
            CursorType.Attack => (_attackCursor, HotspotCenter),
            CursorType.Harvest => (_harvestCursor, HotspotCenter),
            CursorType.NoGo => (_noGoCursor, HotspotCenter),
            CursorType.Sell => (_sellCursor, HotspotCenter),
            CursorType.Repair => (_repairCursor, HotspotCenter),
            _ => (_selectCursor, HotspotTopLeft)
        };

        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
    }

    static bool HasAttacker(System.Collections.Generic.IReadOnlyList<Selectable> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i] != null && selected[i].GetComponent<Attacker>() != null)
                return true;
        }
        return false;
    }

    static bool HasMover(System.Collections.Generic.IReadOnlyList<Selectable> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i] != null && selected[i].GetComponent<Mover>() != null)
                return true;
        }
        return false;
    }

    static bool HasHarvester(System.Collections.Generic.IReadOnlyList<Selectable> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i] != null && selected[i].GetComponent<Harvester>() != null)
                return true;
        }
        return false;
    }

    enum CursorType
    {
        None,
        Select,
        Move,
        Attack,
        Harvest,
        NoGo,
        Sell,
        Repair
    }
}
