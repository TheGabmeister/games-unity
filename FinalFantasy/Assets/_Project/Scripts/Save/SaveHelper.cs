using System.Collections.Generic;
using UnityEngine;

/// Centralized save/load logic: builds SaveData from live state and applies SaveData back.
public static class SaveHelper
{
    public static SaveData CreateSaveData(SaveType type)
    {
        var gm = GameManager.Instance;
        var player = Object.FindFirstObjectByType<PlayerController>();

        var data = new SaveData
        {
            Type = type,
            CurrentScene = gm?.SceneLoader?.CurrentSceneName ?? "Exploration",
            PlayerGridX = player?.GridPosition.x ?? 0,
            PlayerGridY = player?.GridPosition.y ?? 0,
            FacingDirection = player?.FacingDirection ?? 2,
            PlayTimeSeconds = Time.time,
        };

        // Gil
        var inv = gm?.InventoryManager;
        data.Gil = inv?.Gil ?? 0;

        // Progression flags
        var prog = gm?.ProgressionManager;
        if (prog != null)
            data.ProgressionFlags = prog.GetAllFlags();

        // Party
        var pm = gm?.PartyManager;
        if (pm != null)
        {
            data.Party = new PartyMemberSave[4];
            for (int i = 0; i < 4; i++)
            {
                var m = pm.GetMember(i);
                if (m == null) continue;

                data.Party[i] = new PartyMemberSave
                {
                    Name = m.Name,
                    ClassName = m.ClassDef != null ? m.ClassDef.ClassName : "",
                    Level = m.Level,
                    CurrentEXP = m.CurrentEXP,
                    CurrentHP = m.CurrentHP,
                    CurrentMP = m.CurrentMP,
                    BaseMaxHP = m.BaseMaxHP,
                    BaseMaxMP = m.BaseMaxMP,
                    BaseStr = m.BaseStr,
                    BaseAgi = m.BaseAgi,
                    BaseVit = m.BaseVit,
                    BaseInt = m.BaseInt,
                    BaseLuck = m.BaseLuck,
                    BaseAccuracy = m.BaseAccuracy,
                    BaseEvasion = m.BaseEvasion,
                    BaseMagicDef = m.BaseMagicDef,
                    WeaponName = m.Weapon != null ? m.Weapon.name : null,
                    ShieldName = m.Shield != null ? m.Shield.name : null,
                    HelmetName = m.Helmet != null ? m.Helmet.name : null,
                    ArmorName = m.Armor != null ? m.Armor.name : null,
                    StatusEffects = m.StatusEffects,
                };

                foreach (var spell in m.KnownSpells)
                    if (spell != null) data.Party[i].KnownSpellNames.Add(spell.name);
            }
        }

        // Items
        if (inv != null)
        {
            data.Items = new List<ItemSlotSave>();
            foreach (var slot in inv.GetAllItems())
            {
                if (slot.Item != null)
                    data.Items.Add(new ItemSlotSave { ItemName = slot.Item.name, Count = slot.Count });
            }

            data.Equipment = new List<string>();
            foreach (var equip in inv.GetAllEquipment())
            {
                if (equip != null)
                    data.Equipment.Add(equip.name);
            }
        }

        return data;
    }

    public static void ApplySaveData(SaveData data)
    {
        if (data == null) return;

        // Position
        var player = Object.FindFirstObjectByType<PlayerController>();
        player?.SetPosition(new Vector2Int(data.PlayerGridX, data.PlayerGridY));

        var gm = GameManager.Instance;

        // Gil
        gm?.InventoryManager?.SetGil(data.Gil);

        // Flags
        if (data.ProgressionFlags != null)
            gm?.ProgressionManager?.RestoreFlags(data.ProgressionFlags);

        // Party restore is complex (needs ClassDefinition lookups from DataRepository).
        // For now, restore base stats directly if party already exists.
        var pm = gm?.PartyManager;
        if (pm != null && data.Party != null)
        {
            for (int i = 0; i < 4 && i < data.Party.Length; i++)
            {
                var saved = data.Party[i];
                var member = pm.GetMember(i);
                if (saved == null || member == null) continue;

                member.Name = saved.Name;
                member.Level = saved.Level;
                member.CurrentEXP = saved.CurrentEXP;
                member.BaseMaxHP = saved.BaseMaxHP;
                member.BaseMaxMP = saved.BaseMaxMP;
                member.BaseStr = saved.BaseStr;
                member.BaseAgi = saved.BaseAgi;
                member.BaseVit = saved.BaseVit;
                member.BaseInt = saved.BaseInt;
                member.BaseLuck = saved.BaseLuck;
                member.BaseAccuracy = saved.BaseAccuracy;
                member.BaseEvasion = saved.BaseEvasion;
                member.BaseMagicDef = saved.BaseMagicDef;
                member.StatusEffects = saved.StatusEffects;
                member.RecalculateStats();
                member.CurrentHP = saved.CurrentHP;
                member.CurrentMP = saved.CurrentMP;
            }
        }
    }
}
