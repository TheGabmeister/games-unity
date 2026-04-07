using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Class Definition")]
public class ClassDefinition : ScriptableObject
{
    [Header("Identity")]
    public string ClassName;
    public string Abbreviation; // W, Th, Mk, RM, WM, BM
    public Color ClassColor = Color.white;

    [Header("Base Stats (Level 1)")]
    public int BaseHP = 30;
    public int BaseMP = 0;
    public int BaseStrength = 5;
    public int BaseAgility = 5;
    public int BaseVitality = 5;
    public int BaseIntellect = 5;
    public int BaseLuck = 5;
    public int BaseAccuracy = 10;
    public int BaseEvasion = 5;
    public int BaseMagicDefense = 15;

    [Header("Growth Per Level (base increment, +/-1 variance applied)")]
    public int HPGrowth = 20;
    public int MPGrowth = 0;
    public int StrGrowth = 1;
    public int AgiGrowth = 1;
    public int VitGrowth = 1;
    public int IntGrowth = 1;
    public int LuckGrowth = 1;

    [Header("Equipment Access")]
    public WeaponType[] AllowedWeapons;
    public ArmorType[] AllowedArmor;

    [Header("Magic Access")]
    public MagicSchool[] LearnableSchools; // empty = no magic
    public int MaxSpellLevel = 0; // 0 = no magic, 1-8

    [Header("Starter Equipment")]
    public EquipmentData StarterWeapon; // null for Monk

    [Header("EXP Table")]
    public int BaseEXPPerLevel = 40; // EXP for level 2 = BaseEXPPerLevel, scales up
    public float EXPGrowthRate = 1.15f; // multiplier per level

    public int GetEXPForLevel(int level)
    {
        if (level <= 1) return 0;
        // Simple geometric growth: base * growthRate^(level-2)
        return Mathf.RoundToInt(BaseEXPPerLevel * Mathf.Pow(EXPGrowthRate, level - 2));
    }

    public int GetTotalEXPForLevel(int level)
    {
        int total = 0;
        for (int i = 2; i <= level; i++)
            total += GetEXPForLevel(i);
        return total;
    }

    public bool CanEquipWeapon(WeaponType type)
    {
        if (AllowedWeapons == null) return false;
        return System.Array.IndexOf(AllowedWeapons, type) >= 0;
    }

    public bool CanEquipArmor(ArmorType type)
    {
        if (AllowedArmor == null) return false;
        return System.Array.IndexOf(AllowedArmor, type) >= 0;
    }
}
