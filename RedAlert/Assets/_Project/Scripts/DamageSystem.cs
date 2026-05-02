using UnityEngine;

public static class DamageSystem
{
    private const float SplashRadius = 1.5f;

    public static void ApplyDamage(Entity target, int baseDamage, WarheadData warhead)
    {
        if (target == null || target.IsDead) return;

        float modifier = warhead.GetModifier(target.Armor);
        int effective = Mathf.RoundToInt(baseDamage * modifier);
        if (effective < 1) effective = 1;

        target.TakeDamage(effective);

        if (warhead.ImpactSound != null && SfxManager.Instance != null)
            SfxManager.Instance.PlaySound(warhead.ImpactSound);
    }

    public static void ApplySplash(Vector3 worldPos, int baseDamage, WarheadData warhead, Entity shooter)
    {
        if (warhead.SpreadFactor <= 0) return;

        Vector2Int center = MapManager.Instance.WorldToCell(worldPos);

        int scanRadius = Mathf.CeilToInt(SplashRadius);
        for (int dx = -scanRadius; dx <= scanRadius; dx++)
        {
            for (int dy = -scanRadius; dy <= scanRadius; dy++)
            {
                Vector2Int cell = new Vector2Int(center.x + dx, center.y + dy);
                Entity entity = MapManager.Instance.GetEntityAt(cell);
                if (entity == null || entity == shooter || entity.IsDead) continue;

                float dist = Vector2.Distance(
                    new Vector2(center.x + 0.5f, center.y + 0.5f),
                    new Vector2(cell.x + 0.5f, cell.y + 0.5f));

                if (dist > SplashRadius) continue;

                float falloff = Mathf.Max(0f, 1f - dist / (warhead.SpreadFactor * 0.5f));
                float modifier = warhead.GetModifier(entity.Armor);
                int effective = Mathf.RoundToInt(baseDamage * modifier * falloff);
                if (effective < 1) continue;

                entity.TakeDamage(effective);
            }
        }

        if (warhead.ImpactSound != null && SfxManager.Instance != null)
            SfxManager.Instance.PlaySound(warhead.ImpactSound);
    }

    public static void ApplyDamageAtPoint(Vector3 worldPos, int baseDamage, WarheadData warhead, Entity shooter, Entity directTarget)
    {
        if (warhead.SpreadFactor > 0)
        {
            ApplySplash(worldPos, baseDamage, warhead, shooter);
        }
        else if (directTarget != null && !directTarget.IsDead)
        {
            ApplyDamage(directTarget, baseDamage, warhead);
        }
    }
}
