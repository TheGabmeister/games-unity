using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private int _ownerPlayerIndex;
    [SerializeField] private string _entityName;

    public int OwnerPlayerIndex => _ownerPlayerIndex;
    public string EntityName => _entityName;
    public Vector2Int Cell { get; private set; }

    void Start()
    {
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
