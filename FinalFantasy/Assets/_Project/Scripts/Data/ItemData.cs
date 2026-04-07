using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string ItemName;
    public string Description;

    [Header("Effect")]
    public ItemEffectType EffectType;
    public int Power; // heal amount, damage amount, etc.
    public StatusEffectFlags CuresStatus; // which statuses this cures
    public StatusEffectFlags InflictsStatus; // which statuses this inflicts
    public SpellTarget Targeting; // SingleAlly, AllAllies, SingleEnemy

    [Header("Usage")]
    public bool UsableInField;
    public bool UsableInBattle;

    [Header("Commerce")]
    public int BuyPrice;
    public int SellPrice;
}

public enum ItemEffectType
{
    HealHP,
    HealMP,
    CureStatus,
    Revive, // revive KO'd at Power% HP
    Damage,
    InflictStatus,
    None
}
