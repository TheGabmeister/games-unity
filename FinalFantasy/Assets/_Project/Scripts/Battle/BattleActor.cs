using System;
using System.Collections.Generic;
using UnityEngine;

/// Unified abstraction for all battle participants (party members and enemies).
/// Wraps either a PartyMember or an EnemyData instance with runtime HP/status.
public class BattleActor
{
    // Identity
    public string Name { get; private set; }
    public bool IsPartyMember { get; private set; }
    public int SlotIndex { get; private set; } // party slot 0-3, or enemy index in formation

    // Backing data
    public PartyMember PartyMember { get; private set; }
    public EnemyData EnemyData { get; private set; }

    // Runtime battle state
    public int CurrentHP;
    public int MaxHP;
    public StatusEffectFlags StatusEffects;
    public BattleBuffState Buffs = new();

    // Auto-battle: last command this actor used
    public BattleCommand LastCommand;

    // Visual
    public GameObject Visual; // assigned by BattleSceneSetup
    public Vector3 HomePosition; // resting position in battle scene

    public bool IsAlive => CurrentHP > 0
        && (StatusEffects & StatusEffectFlags.KO) == 0
        && (StatusEffects & StatusEffectFlags.Stone) == 0;

    public bool IsEnemy => !IsPartyMember;

    public bool IsBoss => EnemyData != null && EnemyData.IsBoss;
    public bool IsUndead => EnemyData != null && EnemyData.IsUndead;

    // --- Stats (resolved from backing data + buffs) ---

    public int Attack
    {
        get
        {
            int atk = IsPartyMember ? PartyMember.Attack : EnemyData.Attack;
            atk += Buffs.TemperStacks * (BattleConfig.Instance?.TemperAttackBonus ?? 14);
            if (Buffs.HasSaber) atk += BattleConfig.Instance?.SaberAttackBonus ?? 16;
            if ((StatusEffects & StatusEffectFlags.Mini) != 0) atk = Mathf.Max(1, atk / 4);
            return atk;
        }
    }

    public int Defense
    {
        get
        {
            int def = IsPartyMember ? PartyMember.Defense : EnemyData.Defense;
            def += Buffs.ProtectStacks * (BattleConfig.Instance?.ProtectDefenseBonus ?? 8);
            if ((StatusEffects & StatusEffectFlags.Mini) != 0) def = Mathf.Max(0, def / 4);
            return def;
        }
    }

    public int Accuracy => IsPartyMember ? PartyMember.Accuracy : EnemyData.Accuracy;
    public int Evasion => IsPartyMember ? PartyMember.Evasion : EnemyData.Evasion;
    public int MagicDefense => IsPartyMember ? PartyMember.MagicDefense : EnemyData.MagicDefense;
    public int MagicEvasion => IsPartyMember ? PartyMember.MagicEvasion : EnemyData.MagicEvasion;
    public int CritRate => IsPartyMember ? PartyMember.CritRate : EnemyData.CritRate;
    public int Intellect => IsPartyMember ? PartyMember.Intellect : 0;
    public int Agility => IsPartyMember ? PartyMember.Agility : EnemyData.Agility;
    public int Luck => IsPartyMember ? PartyMember.Luck : 0;

    public int HitCount
    {
        get
        {
            int hits = IsPartyMember ? PartyMember.HitCount : EnemyData.HitCount;
            if (Buffs.HasHaste) hits *= 2;
            if (Buffs.HasSlow) hits = Mathf.Max(1, hits / 2);
            return hits;
        }
    }

    // Weapon element (for physical attacks)
    public Element[] WeaponElements
    {
        get
        {
            if (Buffs.HasSaber)
                return new[] { Buffs.SaberElement };
            if (IsPartyMember && PartyMember.Weapon != null && PartyMember.Weapon.ElementalDamage != null
                && PartyMember.Weapon.ElementalDamage.Length > 0)
                return PartyMember.Weapon.ElementalDamage;
            return Array.Empty<Element>();
        }
    }

    // Equipment elemental resistances
    public bool HasElementalResist(Element element)
    {
        if (!IsPartyMember) return false;
        foreach (var slot in new[] { PartyMember.Weapon, PartyMember.Shield, PartyMember.Helmet, PartyMember.Armor })
        {
            if (slot?.ElementalResist != null)
                foreach (var e in slot.ElementalResist)
                    if (e == element) return true;
        }
        // NulElement buffs
        if (element == Element.Fire && Buffs.NulFire) return true;
        if (element == Element.Ice && Buffs.NulIce) return true;
        if (element == Element.Lightning && Buffs.NulLit) return true;
        return false;
    }

    // Equipment status immunities
    public bool HasStatusImmunity(StatusEffectFlags status)
    {
        if (IsEnemy)
            return (EnemyData.StatusImmunities & status) != 0;

        foreach (var slot in new[] { PartyMember.Weapon, PartyMember.Shield, PartyMember.Helmet, PartyMember.Armor })
        {
            if (slot != null && (slot.StatusResist & status) != 0) return true;
        }
        return false;
    }

    // Equipment castable spells
    public List<(EquipmentData equip, SpellData spell)> GetCastableSpells()
    {
        var result = new List<(EquipmentData, SpellData)>();
        if (!IsPartyMember) return result;
        foreach (var slot in new[] { PartyMember.Weapon, PartyMember.Shield, PartyMember.Helmet, PartyMember.Armor })
        {
            if (slot?.CastableSpell != null)
                result.Add((slot, slot.CastableSpell));
        }
        return result;
    }

    public ElementalAffinity GetElementalAffinity(Element element)
    {
        if (IsEnemy)
            return EnemyData.GetAffinity(element);

        // Party members: check equipment resist
        if (HasElementalResist(element))
            return ElementalAffinity.Resist;
        return ElementalAffinity.Normal;
    }

    // --- Factories ---

    public static BattleActor FromPartyMember(PartyMember member, int slot)
    {
        return new BattleActor
        {
            Name = member.Name,
            IsPartyMember = true,
            SlotIndex = slot,
            PartyMember = member,
            CurrentHP = member.CurrentHP,
            MaxHP = member.MaxHP,
            StatusEffects = member.StatusEffects,
        };
    }

    public static BattleActor FromEnemy(EnemyData data, int index)
    {
        return new BattleActor
        {
            Name = data.EnemyName,
            IsPartyMember = false,
            SlotIndex = index,
            EnemyData = data,
            CurrentHP = data.MaxHP,
            MaxHP = data.MaxHP,
            StatusEffects = StatusEffectFlags.None,
        };
    }

    // --- HP Management ---

    public void TakeDamage(int amount)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            StatusEffects |= StatusEffectFlags.KO;
        }
        SyncToPartyMember();
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        SyncToPartyMember();
    }

    public void Revive(int hpPercent)
    {
        StatusEffects &= ~StatusEffectFlags.KO;
        CurrentHP = Mathf.Max(1, MaxHP * hpPercent / 100);
        SyncToPartyMember();
    }

    public void ApplyStatus(StatusEffectFlags status)
    {
        if (HasStatusImmunity(status)) return;
        StatusEffects |= status;
        if ((status & StatusEffectFlags.KO) != 0) CurrentHP = 0;
        SyncToPartyMember();
    }

    public void CureStatus(StatusEffectFlags status)
    {
        StatusEffects &= ~status;
        SyncToPartyMember();
    }

    /// Syncs battle state back to the underlying PartyMember
    public void SyncToPartyMember()
    {
        if (!IsPartyMember || PartyMember == null) return;
        PartyMember.CurrentHP = CurrentHP;
        PartyMember.StatusEffects = StatusEffects;
    }

    // --- Ability checks ---

    public bool CanAct()
    {
        if (!IsAlive) return false;
        if ((StatusEffects & StatusEffectFlags.Sleep) != 0) return false;
        if ((StatusEffects & StatusEffectFlags.Paralysis) != 0) return false;
        if ((StatusEffects & StatusEffectFlags.Stone) != 0) return false;
        return true;
    }

    public bool IsConfused => (StatusEffects & StatusEffectFlags.Confuse) != 0;
    public bool IsSilenced => (StatusEffects & StatusEffectFlags.Silence) != 0;
    public bool IsBlind => (StatusEffects & StatusEffectFlags.Blind) != 0;
    public bool IsPoisoned => (StatusEffects & StatusEffectFlags.Poison) != 0;
    public bool IsDefending { get; set; }
}

/// Tracks battle-only buff/debuff state for a single actor. Cleared at battle end.
[Serializable]
public class BattleBuffState
{
    public bool HasHaste;
    public bool HasSlow;
    public int TemperStacks; // 0-4
    public int ProtectStacks;
    public bool HasSaber;
    public Element SaberElement = Element.Fire;
    public bool NulFire;
    public bool NulIce;
    public bool NulLit;
}
