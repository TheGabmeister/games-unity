using System;
using System.Collections.Generic;

public enum SaveType
{
    Manual,
    Auto,
    Quick
}

[Serializable]
public class SaveData
{
    public SaveType Type;
    public int SaveVersion = 1;
    public string Timestamp; // ISO 8601 UTC
    public string CurrentScene;
    public int PlayerGridX;
    public int PlayerGridY;
    public int FacingDirection; // 0=up, 1=right, 2=down, 3=left
    public float PlayTimeSeconds;
    public int Gil;
    public HashSet<string> ProgressionFlags = new HashSet<string>();
    public Dictionary<string, bool> WorldState = new Dictionary<string, bool>();
    public PartyMemberSave[] Party = new PartyMemberSave[4];
    public List<ItemSlotSave> Items = new();
    public List<string> Equipment = new(); // EquipmentData asset names
}

[Serializable]
public class PartyMemberSave
{
    public string Name;
    public string ClassName; // ClassDefinition asset name for lookup
    public int Level;
    public int CurrentEXP;
    public int CurrentHP;
    public int CurrentMP;
    public int BaseMaxHP, BaseMaxMP;
    public int BaseStr, BaseAgi, BaseVit, BaseInt, BaseLuck;
    public int BaseAccuracy, BaseEvasion, BaseMagicDef;
    public string WeaponName;  // EquipmentData asset name, null if empty
    public string ShieldName;
    public string HelmetName;
    public string ArmorName;
    public List<string> KnownSpellNames = new(); // SpellData asset names
    public StatusEffectFlags StatusEffects;
}

[Serializable]
public class ItemSlotSave
{
    public string ItemName; // ItemData asset name
    public int Count;
}

[Serializable]
public class SaveSlotInfo
{
    public bool Exists;
    public SaveType Type;
    public string Timestamp;
    public string LocationName;
    public float PlayTimeSeconds;
    public int Gil;
}
