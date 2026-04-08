using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("Physical Attack")]
    [Tooltip("Base hit chance before evasion")]
    public int BasePhysicalHitChance = 168;
    [Tooltip("Max hit roll value")]
    public int HitRollMax = 200;

    [Header("Magic")]
    [Tooltip("Base magic hit chance before magic evasion")]
    public int BaseMagicHitChance = 148;

    [Header("Elemental")]
    public float WeakMultiplier = 2f;
    public float ResistMultiplier = 0.5f;

    [Header("Buffs")]
    public int TemperAttackBonus = 14;
    public int TemperMaxStacks = 4;
    public int ProtectDefenseBonus = 8;
    public int SaberAttackBonus = 16;

    [Header("Flee")]
    public int FleeBaseChance = 80;

    [Header("Auto-Retarget")]
    public bool AutoRetarget = true;

    [Header("Timing")]
    public float ActionDelay = 0.4f;
    public float DamagePopupDuration = 1f;
    public float AutoBattleSpeedMultiplier = 2f;

    [Header("Poison")]
    public int PoisonBattleDamage = 2; // per turn
    public int PoisonFieldDamage = 1; // per step

    // Singleton-like access from BattleManager
    static BattleConfig _instance;
    public static BattleConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.FindObjectsOfTypeAll<BattleConfig>().Length > 0
                    ? Resources.FindObjectsOfTypeAll<BattleConfig>()[0]
                    : CreateDefault();
            }
            return _instance;
        }
    }

    static BattleConfig CreateDefault()
    {
        var config = CreateInstance<BattleConfig>();
        config.name = "DefaultBattleConfig";
        return config;
    }
}
