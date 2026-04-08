using System;
using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Encounter Table")]
public class EncounterTable : ScriptableObject
{
    [Header("Encounter Settings")]
    public string TableName;
    [Tooltip("Base steps between encounters")]
    public int BaseStepCount = 30;
    [Tooltip("Random variance added/subtracted from base")]
    public int StepVariance = 10;

    [Header("Formations")]
    public EncounterFormation[] Formations;

    public EncounterFormation GetRandomFormation()
    {
        if (Formations == null || Formations.Length == 0) return null;

        int totalWeight = 0;
        foreach (var f in Formations) totalWeight += f.Weight;
        if (totalWeight <= 0) return Formations[0];

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var f in Formations)
        {
            cumulative += f.Weight;
            if (roll < cumulative) return f;
        }
        return Formations[Formations.Length - 1];
    }

    public int RollStepCount()
    {
        return Mathf.Max(1, BaseStepCount + UnityEngine.Random.Range(-StepVariance, StepVariance + 1));
    }
}

[Serializable]
public class EncounterFormation
{
    public string FormationName;
    public EnemyGroup[] Groups; // multiple groups of same enemy type
    public int Weight = 10;
    public bool IsBoss;
    public bool HasOverrideMusic;
    public MusicTrack OverrideMusic; // only used if HasOverrideMusic is true

    public int TotalEnemyCount
    {
        get
        {
            int count = 0;
            if (Groups != null)
                foreach (var g in Groups) count += g.Count;
            return count;
        }
    }
}

[Serializable]
public class EnemyGroup
{
    public EnemyData Enemy;
    [Range(1, 9)] public int Count = 1;
}
