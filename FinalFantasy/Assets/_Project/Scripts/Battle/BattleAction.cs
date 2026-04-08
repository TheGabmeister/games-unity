using System.Collections.Generic;

/// Types of commands a battle actor can perform
public enum BattleCommandType
{
    Attack,
    Magic,
    Item,
    Defend,
    Flee,
    Use, // cast spell from equipment (free)
    EnemyAction, // AI-selected
}

/// A queued battle action ready for execution
public class BattleCommand
{
    public BattleCommandType Type;
    public BattleActor Actor;
    public BattleActor Target; // single target (null for AoE)
    public List<BattleActor> Targets; // resolved target list
    public SpellData Spell; // for Magic/Use commands
    public ItemData Item; // for Item command
    public int EffectiveAgility; // for turn ordering (with random tiebreak)

    // Flee is a special case: processed before all other actions
    public bool IsFlee => Type == BattleCommandType.Flee;
    public bool IsDefend => Type == BattleCommandType.Defend;
}

/// Result of executing a single action against one target
public class ActionResult
{
    public BattleActor Target;
    public bool Hit;
    public bool IsCritical;
    public int TotalDamage; // positive = damage, negative = healing
    public int[] PerHitDamage; // for physical multi-hit
    public bool IsHealing;
    public string Message; // "Miss", "Ineffective", "Silenced!", etc.
    public StatusEffectFlags StatusInflicted;
    public StatusEffectFlags StatusCured;
    public bool TargetKilled;
    public BuffChange BuffApplied;
}

/// Describes a buff/debuff change from a spell
public class BuffChange
{
    public bool Haste;
    public bool Slow;
    public bool Temper;
    public bool Protect;
    public bool Saber;
    public bool NulFire;
    public bool NulIce;
    public bool NulLit;
}
