using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerState
{
    public int PlayerIndex;
    public Faction Faction;
    public Color Color = Color.white;
    public List<Entity> OwnedEntities = new();

    public int Credits;
    public int StorageCapacity;
}
