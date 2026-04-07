using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    PartyMember[] members = new PartyMember[4];

    [SerializeField] ClassDefinition[] defaultClasses; // assigned by editor or created at runtime

    public PartyMember[] Members => members;
    public bool IsPartyCreated => members[0] != null;

    public event Action OnPartyChanged;

    public PartyMember GetMember(int slot)
    {
        if (slot < 0 || slot >= 4) return null;
        return members[slot];
    }

    public void CreateParty(ClassDefinition[] classes, string[] names)
    {
        for (int i = 0; i < 4; i++)
            members[i] = PartyMember.Create(classes[i], names[i]);
        OnPartyChanged?.Invoke();
    }

    public void SwapMembers(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= 4 || slotB < 0 || slotB >= 4) return;
        (members[slotA], members[slotB]) = (members[slotB], members[slotA]);
        OnPartyChanged?.Invoke();
    }

    /// Force a level-up on a member, returns the result
    public LevelUpResult LevelUp(int slot)
    {
        var member = GetMember(slot);
        if (member == null || member.Level >= 99) return null;
        var result = member.ApplyLevelUp();
        OnPartyChanged?.Invoke();
        return result;
    }

    /// Set a member to a specific level, applying growth for each level
    public void SetLevel(int slot, int targetLevel)
    {
        var member = GetMember(slot);
        if (member == null) return;

        targetLevel = Mathf.Clamp(targetLevel, 1, 99);

        // Reset to level 1 base stats
        member.Level = 1;
        member.BaseMaxHP = member.ClassDef.BaseHP;
        member.BaseMaxMP = member.ClassDef.BaseMP;
        member.BaseStr = member.ClassDef.BaseStrength;
        member.BaseAgi = member.ClassDef.BaseAgility;
        member.BaseVit = member.ClassDef.BaseVitality;
        member.BaseInt = member.ClassDef.BaseIntellect;
        member.BaseLuck = member.ClassDef.BaseLuck;
        member.BaseAccuracy = member.ClassDef.BaseAccuracy;
        member.BaseEvasion = member.ClassDef.BaseEvasion;
        member.BaseMagicDef = member.ClassDef.BaseMagicDefense;

        // Level up one at a time to accumulate growth
        while (member.Level < targetLevel)
            member.ApplyLevelUp();

        member.CurrentEXP = member.ClassDef.GetTotalEXPForLevel(targetLevel);
        OnPartyChanged?.Invoke();
    }

    /// Add EXP to living members, process level-ups
    public List<LevelUpResult> AddEXP(int totalEXP)
    {
        var results = new List<LevelUpResult>();
        int livingCount = 0;
        foreach (var m in members)
            if (m != null && m.IsAlive) livingCount++;
        if (livingCount == 0) return results;

        int perMember = totalEXP / livingCount;

        for (int i = 0; i < 4; i++)
        {
            var m = members[i];
            if (m == null || !m.IsAlive) continue;

            m.CurrentEXP += perMember;
            while (m.CanLevelUp())
            {
                results.Add(m.ApplyLevelUp());
            }
        }

        if (results.Count > 0) OnPartyChanged?.Invoke();
        return results;
    }

    /// Get the party leader (slot 0) for map display
    public PartyMember Leader => members[0];

    /// Check if all party members are KO'd/Stone'd
    public bool IsPartyWiped()
    {
        for (int i = 0; i < 4; i++)
            if (members[i] != null && members[i].IsAlive) return false;
        return true;
    }
}
