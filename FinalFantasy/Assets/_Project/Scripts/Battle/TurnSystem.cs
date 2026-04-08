using System.Collections.Generic;
using UnityEngine;

/// Handles turn ordering: sorts queued commands by effective agility,
/// with random tiebreakers. Flee always goes first, Defend always acts first.
public static class TurnSystem
{
    /// Sort commands into execution order.
    /// Flee goes first. Defend gets max priority. Others sort by agility (desc) with random tiebreak.
    public static List<BattleCommand> SortActions(List<BattleCommand> commands)
    {
        // Separate flee from other commands
        BattleCommand fleeCommand = null;
        var others = new List<BattleCommand>();

        foreach (var cmd in commands)
        {
            if (cmd == null) continue;
            if (cmd.IsFlee)
                fleeCommand = cmd;
            else
                others.Add(cmd);
        }

        // Assign effective agility with random tiebreak
        foreach (var cmd in others)
        {
            if (cmd.IsDefend)
            {
                // Defend always goes first (very high agility)
                cmd.EffectiveAgility = 9999 + Random.Range(0, 100);
            }
            else
            {
                int baseAgi = cmd.Actor?.Agility ?? 0;
                cmd.EffectiveAgility = baseAgi * 100 + Random.Range(0, 100);
            }
        }

        // Sort descending by effective agility (highest acts first)
        others.Sort((a, b) => b.EffectiveAgility.CompareTo(a.EffectiveAgility));

        // Flee always resolves first
        var sorted = new List<BattleCommand>();
        if (fleeCommand != null) sorted.Add(fleeCommand);
        sorted.AddRange(others);

        return sorted;
    }
}
