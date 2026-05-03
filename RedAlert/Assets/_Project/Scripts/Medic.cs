using UnityEngine;

public class Medic : MonoBehaviour
{
    private Entity _entity;
    private float _timer;
    private const float HealInterval = 0.5f;
    private const int HealAmount = 2;
    private const float HealRange = 1.83f;

    void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    void Update()
    {
        if (_entity.IsDead) return;

        _timer += Time.deltaTime;
        if (_timer < HealInterval) return;
        _timer = 0f;

        Entity best = null;
        float bestRatio = 1f;

        var player = PlayerManager.Instance.GetPlayer(_entity.OwnerPlayerIndex);
        foreach (var entity in player.OwnedEntities)
        {
            if (entity == null || entity.IsDead) continue;
            if (entity == _entity) continue;
            if (entity.UnitData == null || entity.UnitData.Category != UnitCategory.Infantry) continue;

            float dist = Vector2Int.Distance(_entity.Cell, entity.Cell);
            if (dist > HealRange) continue;

            float ratio = entity.Health.Ratio;
            if (ratio >= 1f) continue;

            if (ratio < bestRatio)
            {
                bestRatio = ratio;
                best = entity;
            }
        }

        if (best != null)
            best.Health.Heal(HealAmount);
    }
}
