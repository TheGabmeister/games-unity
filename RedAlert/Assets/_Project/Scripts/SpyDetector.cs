using UnityEngine;

public class SpyDetector : MonoBehaviour
{
    private Entity _entity;
    private Attacker _attacker;
    private float _timer;
    private const float ScanInterval = 0.5f;
    private const float DetectionRange = 7f;

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _attacker = GetComponent<Attacker>();
    }

    void Update()
    {
        if (_entity.IsDead) return;
        if (_attacker == null) return;

        _timer += Time.deltaTime;
        if (_timer < ScanInterval) return;
        _timer = 0f;

        for (int p = 0; p < PlayerManager.Instance.PlayerCount; p++)
        {
            if (!PlayerManager.Instance.AreEnemies(_entity.OwnerPlayerIndex, p)) continue;
            var player = PlayerManager.Instance.GetPlayer(p);

            for (int i = player.OwnedEntities.Count - 1; i >= 0; i--)
            {
                var entity = player.OwnedEntities[i];
                if (entity == null || entity.IsDead) continue;

                var spy = entity.GetComponent<Spy>();
                if (spy == null) continue;

                float dist = Vector2Int.Distance(_entity.Cell, entity.Cell);
                if (dist > DetectionRange) continue;

                spy.RevealDisguise();
                _attacker.AttackTarget(entity);
                return;
            }
        }
    }
}
