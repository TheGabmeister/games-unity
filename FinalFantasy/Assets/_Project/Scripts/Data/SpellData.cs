using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Spell")]
public class SpellData : ScriptableObject
{
    [Header("Identity")]
    public string SpellName;
    public MagicSchool School;
    public int Level; // 1-8

    [Header("Cost")]
    public int MPCost;

    [Header("Effect")]
    public SpellEffectType[] Effects;
    public SpellTarget Targeting;
    public Element Element;
    public int BasePower;
    public int SpellAccuracy;

    [Header("Status")]
    public StatusEffectFlags InflictsStatus;
    public StatusEffectFlags CuresStatus;

    [Header("Usage Context")]
    public bool UsableInField;
    public bool UsableInBattle = true;

    [Header("Commerce")]
    public int BuyPrice;
}
