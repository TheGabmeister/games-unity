using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Header("Identity")]
    public string ItemName;
    public EquipmentSlot Slot;

    [Header("Stats")]
    public int Attack;
    public int Defense;
    public int Evasion;
    public int MagicDefense;
    public int Accuracy;
    public int CritRate;

    [Header("Stat Bonuses")]
    public int StrengthBonus;
    public int AgilityBonus;
    public int VitalityBonus;
    public int IntellectBonus;
    public int LuckBonus;
    public int HPBonus;
    public int MPBonus;

    [Header("Properties")]
    public bool TwoHanded;
    public WeaponType WeaponType; // only meaningful when Slot == Weapon
    public ArmorType ArmorType;   // only meaningful when Slot != Weapon

    [Header("Elemental")]
    public Element[] ElementalResist;
    public Element[] ElementalDamage; // weapon deals this element

    [Header("Special")]
    public SpellData CastableSpell; // use in battle to cast for free
    public StatusEffectFlags StatusResist;

    [Header("Commerce")]
    public int BuyPrice;
    public int SellPrice; // usually BuyPrice / 2
}
