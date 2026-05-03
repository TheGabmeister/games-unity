using UnityEngine;

public class Spy : MonoBehaviour
{
    private Entity _entity;
    private Mover _mover;
    private Entity _targetBuilding;
    private bool _disguised = true;
    private const float DockDistance = 1.5f;

    public bool IsDisguised => _disguised;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _mover = GetComponent<Mover>();
    }

    public void Infiltrate(Entity building)
    {
        if (building == null || !building.IsBuilding) return;
        _targetBuilding = building;

        var adjacent = building.FindAdjacentFreeCell();
        _mover.MoveTo(adjacent);
    }

    public void RevealDisguise()
    {
        _disguised = false;
    }

    void Update()
    {
        if (_entity.IsDead || _targetBuilding == null) return;

        if (_targetBuilding.IsDead)
        {
            _targetBuilding = null;
            return;
        }

        if (_mover.IsMoving) return;

        float dist = Vector2Int.Distance(_entity.Cell, _targetBuilding.Cell);
        if (dist > DockDistance + _targetBuilding.UnitData.FootprintX)
        {
            var adjacent = _targetBuilding.FindAdjacentFreeCell();
            _mover.MoveTo(adjacent);
            return;
        }

        ApplyInfiltrationEffect();
        _targetBuilding = null;
        _entity.Die();
    }

    void ApplyInfiltrationEffect()
    {
        int targetPlayer = _targetBuilding.OwnerPlayerIndex;
        string buildingName = _targetBuilding.EntityName;

        switch (buildingName)
        {
            case "Ore Refinery":
            case "Ore Silo":
                int stolen = EconomyManager.Instance.GetCredits(targetPlayer) / 2;
                if (stolen > 0)
                {
                    EconomyManager.Instance.SpendCredits(targetPlayer, stolen);
                    EconomyManager.Instance.AddCredits(_entity.OwnerPlayerIndex, stolen);
                }
                break;

            case "Power Plant":
            case "Advanced Power Plant":
                // Reveals enemy power status — in singleplayer the human already sees the AI's state
                break;

            case "Radar Dome":
                RevealAllMap(targetPlayer);
                break;

            case "Sub Pen":
                RevealAllMap(targetPlayer);
                break;

            case "Construction Yard":
                // Reveals structure under construction — sidebar already visible in singleplayer
                break;
        }
    }

    void RevealAllMap(int targetPlayer)
    {
        if (FogManager.Instance == null) return;
        var map = MapManager.Instance;
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
                FogManager.Instance.RevealCell(new Vector2Int(x, y));
        }
    }
}
