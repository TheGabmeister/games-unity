using UnityEngine;

public class Refinery : MonoBehaviour
{
    private Entity _entity;
    private Harvester _dockedHarvester;

    public bool IsOccupied => _dockedHarvester != null;
    public Vector2Int DockCell => _entity.Cell;

    void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    void OnDestroy()
    {
        if (_dockedHarvester != null)
            _dockedHarvester.OnRefineryDestroyed();
    }

    public bool TryDock(Harvester harvester)
    {
        if (_dockedHarvester != null && _dockedHarvester != harvester) return false;
        _dockedHarvester = harvester;
        return true;
    }

    public void Undock(Harvester harvester)
    {
        if (_dockedHarvester == harvester)
            _dockedHarvester = null;
    }
}
