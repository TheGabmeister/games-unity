using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private int _ownerPlayerIndex;
    [SerializeField] private UnitData _unitData;

    public int OwnerPlayerIndex => _ownerPlayerIndex;
    public UnitData UnitData => _unitData;
    public string EntityName => _unitData != null ? _unitData.DisplayName : "";
    public Vector2Int Cell { get; private set; }

    void Start()
    {
        if (_unitData != null && _unitData.Sprite != null
            && TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sprite = _unitData.Sprite;
        }

        Cell = MapManager.Instance.WorldToCell(transform.position);
        MapManager.Instance.RegisterEntity(Cell, this);

        var player = PlayerManager.Instance.GetPlayer(_ownerPlayerIndex);
        player.OwnedEntities.Add(this);
    }

    void OnDestroy()
    {
        MapManager.Instance?.UnregisterEntity(Cell);

        var player = PlayerManager.Instance?.GetPlayer(_ownerPlayerIndex);
        player?.OwnedEntities.Remove(this);
    }

    public void SetCell(Vector2Int newCell)
    {
        MapManager.Instance.UnregisterEntity(Cell);
        Cell = newCell;
        MapManager.Instance.RegisterEntity(Cell, this);
    }
}
