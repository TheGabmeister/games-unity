using System.Collections.Generic;
using UnityEngine;

public static class EnemyBattleAI
{
    public static BattleAction ChooseAction(WildDigimonInstance enemy)
    {
        if (enemy.CurrentHP < enemy.MaxHP * 0.2f && Random.value < 0.5f)
            return new BattleAction { Type = BattleActionType.Flee };

        List<TechniqueData> affordable = new List<TechniqueData>();
        foreach (var tech in enemy.Techniques)
        {
            if (tech.MpCost <= enemy.CurrentMP)
                affordable.Add(tech);
        }

        if (affordable.Count > 0)
        {
            int totalWeight = 0;
            foreach (var tech in affordable)
                totalWeight += tech.Power;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var tech in affordable)
            {
                cumulative += tech.Power;
                if (roll < cumulative)
                    return new BattleAction { Type = BattleActionType.Technique, Technique = tech };
            }
        }

        return new BattleAction { Type = BattleActionType.Attack };
    }
}
