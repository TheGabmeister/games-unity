using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PartyMember
{
    public string Name;
    public ClassDefinition ClassDef;

    [Header("Level & EXP")]
    public int Level = 1;
    public int CurrentEXP;

    [Header("Current Resources")]
    public int CurrentHP;
    public int CurrentMP;

    [Header("Base Stats (accumulated from level-ups, no equipment)")]
    public int BaseMaxHP;
    public int BaseMaxMP;
    public int BaseStr;
    public int BaseAgi;
    public int BaseVit;
    public int BaseInt;
    public int BaseLuck;
    public int BaseAccuracy;
    public int BaseEvasion;
    public int BaseMagicDef;

    [Header("Computed Stats (base + equipment)")]
    public int MaxHP;
    public int MaxMP;
    public int Strength;
    public int Agility;
    public int Vitality;
    public int Intellect;
    public int Luck;
    public int Attack;
    public int Defense;
    public int Evasion;
    public int Accuracy;
    public int MagicDefense;
    public int MagicEvasion;
    public int CritRate;
    public int HitCount;

    [Header("Equipment")]
    public EquipmentData Weapon;
    public EquipmentData Shield;
    public EquipmentData Helmet;
    public EquipmentData Armor;

    [Header("Magic & Status")]
    public List<SpellData> KnownSpells = new();
    public StatusEffectFlags StatusEffects;

    public bool IsAlive => (StatusEffects & (StatusEffectFlags.KO | StatusEffectFlags.Stone)) == 0;

    /// Create a new party member from a class definition
    public static PartyMember Create(ClassDefinition classDef, string name)
    {
        var member = new PartyMember
        {
            Name = name,
            ClassDef = classDef,
            Level = 1,
            CurrentEXP = 0,
            BaseMaxHP = classDef.BaseHP,
            BaseMaxMP = classDef.BaseMP,
            BaseStr = classDef.BaseStrength,
            BaseAgi = classDef.BaseAgility,
            BaseVit = classDef.BaseVitality,
            BaseInt = classDef.BaseIntellect,
            BaseLuck = classDef.BaseLuck,
            BaseAccuracy = classDef.BaseAccuracy,
            BaseEvasion = classDef.BaseEvasion,
            BaseMagicDef = classDef.BaseMagicDefense,
        };
        member.RecalculateStats();
        member.CurrentHP = member.MaxHP;
        member.CurrentMP = member.MaxMP;

        // Equip starter weapon
        if (classDef.StarterWeapon != null)
            member.Weapon = classDef.StarterWeapon;
        member.RecalculateStats();
        member.CurrentHP = member.MaxHP;
        member.CurrentMP = member.MaxMP;

        return member;
    }

    /// Recalculate derived stats from base + all equipment bonuses
    public void RecalculateStats()
    {
        // Start from base
        MaxHP = BaseMaxHP;
        MaxMP = BaseMaxMP;
        Strength = BaseStr;
        Agility = BaseAgi;
        Vitality = BaseVit;
        Intellect = BaseInt;
        Luck = BaseLuck;
        Accuracy = BaseAccuracy;
        Evasion = BaseEvasion;
        MagicDefense = BaseMagicDef;

        Attack = Strength / 2; // base attack from strength
        Defense = 0;
        CritRate = 0;

        // Add equipment bonuses
        AddEquipmentStats(Weapon);
        AddEquipmentStats(Shield);
        AddEquipmentStats(Helmet);
        AddEquipmentStats(Armor);

        // Compute hit count: (Accuracy / 32) + 1
        HitCount = Mathf.Max(1, Accuracy / 32 + 1);

        // Magic evasion from agility
        MagicEvasion = Agility / 4;

        // Clamp HP/MP
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
        CurrentMP = Mathf.Min(CurrentMP, MaxMP);
    }

    void AddEquipmentStats(EquipmentData equip)
    {
        if (equip == null) return;

        Attack += equip.Attack;
        Defense += equip.Defense;
        Evasion += equip.Evasion;
        MagicDefense += equip.MagicDefense;
        Accuracy += equip.Accuracy;
        CritRate += equip.CritRate;

        Strength += equip.StrengthBonus;
        Agility += equip.AgilityBonus;
        Vitality += equip.VitalityBonus;
        Intellect += equip.IntellectBonus;
        Luck += equip.LuckBonus;
        MaxHP += equip.HPBonus;
        MaxMP += equip.MPBonus;
    }

    /// Check if this member can equip a given item
    public bool CanEquip(EquipmentData equip)
    {
        if (equip == null || ClassDef == null) return false;

        if (equip.Slot == EquipmentSlot.Weapon)
            return ClassDef.CanEquipWeapon(equip.WeaponType);
        else
            return ClassDef.CanEquipArmor(equip.ArmorType);
    }

    /// Equip an item, returns the previously equipped item (or null)
    public EquipmentData Equip(EquipmentData equip)
    {
        if (equip == null) return null;

        EquipmentData old = null;
        switch (equip.Slot)
        {
            case EquipmentSlot.Weapon:
                old = Weapon;
                Weapon = equip;
                // Two-handed weapon removes shield
                if (equip.TwoHanded && Shield != null)
                {
                    // Return shield to inventory is handled by caller
                    Shield = null;
                }
                break;
            case EquipmentSlot.Shield:
                // Can't equip shield with two-handed weapon
                if (Weapon != null && Weapon.TwoHanded) return equip;
                old = Shield;
                Shield = equip;
                break;
            case EquipmentSlot.Helmet:
                old = Helmet;
                Helmet = equip;
                break;
            case EquipmentSlot.Armor:
                old = Armor;
                Armor = equip;
                break;
        }
        RecalculateStats();
        return old;
    }

    /// Unequip from a slot, returns removed item
    public EquipmentData Unequip(EquipmentSlot slot)
    {
        EquipmentData old = null;
        switch (slot)
        {
            case EquipmentSlot.Weapon: old = Weapon; Weapon = null; break;
            case EquipmentSlot.Shield: old = Shield; Shield = null; break;
            case EquipmentSlot.Helmet: old = Helmet; Helmet = null; break;
            case EquipmentSlot.Armor:  old = Armor;  Armor = null;  break;
        }
        RecalculateStats();
        return old;
    }

    /// Get the equipment in a specific slot
    public EquipmentData GetEquipment(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => Weapon,
        EquipmentSlot.Shield => Shield,
        EquipmentSlot.Helmet => Helmet,
        EquipmentSlot.Armor => Armor,
        _ => null
    };

    /// Preview stat changes from equipping an item without mutating live state.
    /// Computes hypothetical stats by simulating equipment bonuses directly.
    public StatPreview PreviewEquip(EquipmentData newEquip)
    {
        var preview = new StatPreview
        {
            OldAttack = Attack, OldDefense = Defense, OldEvasion = Evasion,
            OldMagicDef = MagicDefense, OldMaxHP = MaxHP, OldMaxMP = MaxMP,
            OldStr = Strength, OldAgi = Agility, OldVit = Vitality,
            OldInt = Intellect, OldLuck = Luck,
        };

        // Build hypothetical equipment set
        var w = Weapon; var s = Shield; var h = Helmet; var a = Armor;
        switch (newEquip.Slot)
        {
            case EquipmentSlot.Weapon:
                w = newEquip;
                if (newEquip.TwoHanded) s = null; // two-handed clears shield
                break;
            case EquipmentSlot.Shield:
                if (w != null && w.TwoHanded) { preview.CopyNewFromOld(); return preview; } // can't equip shield with 2H
                s = newEquip;
                break;
            case EquipmentSlot.Helmet: h = newEquip; break;
            case EquipmentSlot.Armor:  a = newEquip; break;
        }

        // Compute stats from base + hypothetical equipment
        int str = BaseStr, agi = BaseAgi, vit = BaseVit, intel = BaseInt, luck = BaseLuck;
        int atk = BaseStr / 2, def = 0, eva = BaseEvasion, mdef = BaseMagicDef;
        int maxHP = BaseMaxHP, maxMP = BaseMaxMP;

        void Add(EquipmentData eq)
        {
            if (eq == null) return;
            atk += eq.Attack; def += eq.Defense; eva += eq.Evasion; mdef += eq.MagicDefense;
            str += eq.StrengthBonus; agi += eq.AgilityBonus; vit += eq.VitalityBonus;
            intel += eq.IntellectBonus; luck += eq.LuckBonus;
            maxHP += eq.HPBonus; maxMP += eq.MPBonus;
        }

        Add(w); Add(s); Add(h); Add(a);

        preview.NewAttack = atk; preview.NewDefense = def; preview.NewEvasion = eva;
        preview.NewMagicDef = mdef; preview.NewMaxHP = maxHP; preview.NewMaxMP = maxMP;
        preview.NewStr = str; preview.NewAgi = agi; preview.NewVit = vit;
        preview.NewInt = intel; preview.NewLuck = luck;

        return preview;
    }

    /// Apply a level-up: increment level, roll stat growth with +/-1 variance
    public LevelUpResult ApplyLevelUp()
    {
        var result = new LevelUpResult { OldLevel = Level };
        result.OldHP = MaxHP; result.OldMP = MaxMP;
        result.OldStr = BaseStr; result.OldAgi = BaseAgi;
        result.OldVit = BaseVit; result.OldInt = BaseInt;
        result.OldLuck = BaseLuck;

        Level++;

        int variance1() => UnityEngine.Random.Range(-1, 2); // -1, 0, or 1

        BaseMaxHP += Mathf.Max(1, ClassDef.HPGrowth + UnityEngine.Random.Range(-2, 3));
        BaseMaxMP += Mathf.Max(0, ClassDef.MPGrowth + (ClassDef.MPGrowth > 0 ? variance1() : 0));
        BaseStr += Mathf.Max(0, ClassDef.StrGrowth + variance1());
        BaseAgi += Mathf.Max(0, ClassDef.AgiGrowth + variance1());
        BaseVit += Mathf.Max(0, ClassDef.VitGrowth + variance1());
        BaseInt += Mathf.Max(0, ClassDef.IntGrowth + variance1());
        BaseLuck += Mathf.Max(0, ClassDef.LuckGrowth + variance1());

        RecalculateStats();

        // Restore HP/MP to full on level up
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        result.NewLevel = Level;
        result.NewHP = MaxHP; result.NewMP = MaxMP;
        result.NewStr = BaseStr; result.NewAgi = BaseAgi;
        result.NewVit = BaseVit; result.NewInt = BaseInt;
        result.NewLuck = BaseLuck;

        return result;
    }

    /// How much total EXP needed for next level
    public int EXPForNextLevel()
    {
        if (ClassDef == null) return int.MaxValue;
        return ClassDef.GetTotalEXPForLevel(Level + 1);
    }

    /// Check if member has enough EXP to level up
    public bool CanLevelUp() => CurrentEXP >= EXPForNextLevel() && Level < 99;
}

[Serializable]
public class StatPreview
{
    public int OldAttack, NewAttack;
    public int OldDefense, NewDefense;
    public int OldEvasion, NewEvasion;
    public int OldMagicDef, NewMagicDef;
    public int OldMaxHP, NewMaxHP;
    public int OldMaxMP, NewMaxMP;
    public int OldStr, NewStr;
    public int OldAgi, NewAgi;
    public int OldVit, NewVit;
    public int OldInt, NewInt;
    public int OldLuck, NewLuck;

    /// Sets all New values equal to Old (no change preview)
    public void CopyNewFromOld()
    {
        NewAttack = OldAttack; NewDefense = OldDefense; NewEvasion = OldEvasion;
        NewMagicDef = OldMagicDef; NewMaxHP = OldMaxHP; NewMaxMP = OldMaxMP;
        NewStr = OldStr; NewAgi = OldAgi; NewVit = OldVit;
        NewInt = OldInt; NewLuck = OldLuck;
    }
}

[Serializable]
public class LevelUpResult
{
    public int OldLevel, NewLevel;
    public int OldHP, NewHP;
    public int OldMP, NewMP;
    public int OldStr, NewStr;
    public int OldAgi, NewAgi;
    public int OldVit, NewVit;
    public int OldInt, NewInt;
    public int OldLuck, NewLuck;
}
