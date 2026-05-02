using UnityEngine;

[CreateAssetMenu(fileName = "NewFaction", menuName = "Red Alert/Faction Data")]
public class FactionData : ScriptableObject
{
    public Faction Faction;
    public UnitData[] BuildableStructures;
    public UnitData[] BuildableUnits;
}
