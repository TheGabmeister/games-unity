using System.Collections.Generic;
using UnityEngine;

/// Selects enemy actions using weighted random + conditional logic.
/// Supports boss scripted patterns (every-N-turns) alongside random selection.
public static class EnemyAI
{
    public static BattleCommand SelectAction(BattleActor enemy, List<BattleActor> partyActors,
        List<BattleActor> enemyActors, int turnNumber)
    {
        var data = enemy.EnemyData;
        if (data == null || data.Actions == null || data.Actions.Length == 0)
            return MakeAttackCommand(enemy, partyActors);

        // Check for scripted actions first (every-N-turns)
        foreach (var action in data.Actions)
        {
            if (action.EveryNTurns > 0 && turnNumber > 0 && turnNumber % action.EveryNTurns == 0)
            {
                if (EvaluateCondition(action.Condition, enemy))
                    return MakeCommand(enemy, action, partyActors, enemyActors);
            }
        }

        // Weighted random from eligible actions
        var eligible = new List<(EnemyAction action, int weight)>();
        foreach (var action in data.Actions)
        {
            if (action.EveryNTurns > 0) continue; // scripted actions only fire on their turn
            if (!EvaluateCondition(action.Condition, enemy)) continue;
            eligible.Add((action, action.Weight));
        }

        if (eligible.Count == 0)
            return MakeAttackCommand(enemy, partyActors);

        int totalWeight = 0;
        foreach (var e in eligible) totalWeight += e.weight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var e in eligible)
        {
            cumulative += e.weight;
            if (roll < cumulative)
                return MakeCommand(enemy, e.action, partyActors, enemyActors);
        }

        return MakeAttackCommand(enemy, partyActors);
    }

    static bool EvaluateCondition(AICondition condition, BattleActor enemy)
    {
        float hpPercent = enemy.MaxHP > 0 ? (float)enemy.CurrentHP / enemy.MaxHP : 1f;
        return condition switch
        {
            AICondition.None => true,
            AICondition.HPBelow75 => hpPercent < 0.75f,
            AICondition.HPBelow50 => hpPercent < 0.5f,
            AICondition.HPBelow25 => hpPercent < 0.25f,
            AICondition.TurnCountAbove3 => true, // handled by caller via turnNumber
            _ => true,
        };
    }

    static BattleCommand MakeCommand(BattleActor enemy, EnemyAction action,
        List<BattleActor> partyActors, List<BattleActor> enemyActors)
    {
        switch (action.Type)
        {
            case EnemyActionType.CastSpell:
                if (action.Spell != null)
                    return MakeSpellCommand(enemy, action.Spell, partyActors, enemyActors);
                return MakeAttackCommand(enemy, partyActors);

            case EnemyActionType.DoNothing:
                return new BattleCommand
                {
                    Type = BattleCommandType.EnemyAction,
                    Actor = enemy,
                    Targets = new List<BattleActor>(),
                };

            case EnemyActionType.Attack:
            default:
                return MakeAttackCommand(enemy, partyActors);
        }
    }

    static BattleCommand MakeAttackCommand(BattleActor enemy, List<BattleActor> partyActors)
    {
        var living = GetLivingActors(partyActors);
        if (living.Count == 0) return null;

        var target = living[Random.Range(0, living.Count)];
        return new BattleCommand
        {
            Type = BattleCommandType.Attack,
            Actor = enemy,
            Target = target,
            Targets = new List<BattleActor> { target },
        };
    }

    static BattleCommand MakeSpellCommand(BattleActor enemy, SpellData spell,
        List<BattleActor> partyActors, List<BattleActor> enemyActors)
    {
        var targets = ResolveTargets(spell.Targeting, enemy, partyActors, enemyActors);
        if (targets.Count == 0) return MakeAttackCommand(enemy, partyActors);

        return new BattleCommand
        {
            Type = BattleCommandType.EnemyAction,
            Actor = enemy,
            Spell = spell,
            Target = targets.Count == 1 ? targets[0] : null,
            Targets = targets,
        };
    }

    static List<BattleActor> ResolveTargets(SpellTarget targeting, BattleActor caster,
        List<BattleActor> partyActors, List<BattleActor> enemyActors)
    {
        // For enemies: "SingleEnemy" targets a party member, "SingleAlly" targets an enemy ally
        var result = new List<BattleActor>();
        switch (targeting)
        {
            case SpellTarget.SingleEnemy:
                var living = GetLivingActors(partyActors);
                if (living.Count > 0) result.Add(living[Random.Range(0, living.Count)]);
                break;
            case SpellTarget.AllEnemies:
                result.AddRange(GetLivingActors(partyActors));
                break;
            case SpellTarget.SingleAlly:
                var allies = GetLivingActors(enemyActors);
                if (allies.Count > 0) result.Add(allies[Random.Range(0, allies.Count)]);
                break;
            case SpellTarget.AllAllies:
                result.AddRange(GetLivingActors(enemyActors));
                break;
            case SpellTarget.Self:
                result.Add(caster);
                break;
        }
        return result;
    }

    static List<BattleActor> GetLivingActors(List<BattleActor> actors)
    {
        var result = new List<BattleActor>();
        foreach (var a in actors)
            if (a.IsAlive) result.Add(a);
        return result;
    }
}
