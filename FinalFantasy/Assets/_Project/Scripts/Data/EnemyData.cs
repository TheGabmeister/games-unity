using System;
using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string EnemyName;
    public EnemyCategory Category; // Normal, Boss, Undead, Dragon

    [Header("Stats")]
    public int MaxHP = 20;
    public int Attack = 10;
    public int Defense = 0;
    public int Accuracy = 10;
    public int Evasion = 0;
    public int MagicDefense = 10;
    public int MagicEvasion = 10;
    public int CritRate = 0;
    public int HitCount = 1;
    public int Agility = 5; // for turn order

    [Header("Elemental Profile")]
    public ElementalAffinityEntry[] Affinities;
    public StatusEffectFlags StatusImmunities;

    [Header("Rewards")]
    public int EXPReward = 10;
    public int GilReward = 10;
    public ItemDrop[] DropTable;

    [Header("AI")]
    public EnemyAction[] Actions;

    [Header("Appearance")]
    public Color PrimaryColor = Color.red;
    public Color SecondaryColor = Color.white;
    public EnemyShape Shape = EnemyShape.Circle;
    public float SizeScale = 1f; // 0.5 = small, 1 = normal, 2 = boss-sized

    public bool IsUndead => Category == EnemyCategory.Undead;
    public bool IsBoss => Category == EnemyCategory.Boss;

    public ElementalAffinity GetAffinity(Element element)
    {
        if (Affinities != null)
        {
            foreach (var entry in Affinities)
                if (entry.Element == element) return entry.Affinity;
        }
        return ElementalAffinity.Normal;
    }
}

[Serializable]
public class ElementalAffinityEntry
{
    public Element Element;
    public ElementalAffinity Affinity;
}

[Serializable]
public class ItemDrop
{
    public ItemData Item;
    [Range(0f, 1f)] public float DropRate;
}

[Serializable]
public class EnemyAction
{
    public EnemyActionType Type;
    public SpellData Spell; // if Type == CastSpell
    public int Weight = 10; // relative probability
    public AICondition Condition;
    [Tooltip("For scripted patterns: use on every Nth turn (0 = no restriction)")]
    public int EveryNTurns;
}

public enum EnemyActionType { Attack, CastSpell, DoNothing }

public enum AICondition { None, HPBelow75, HPBelow50, HPBelow25, TurnCountAbove3 }

public enum EnemyShape { Circle, Square, Triangle, Diamond, Hexagon }
