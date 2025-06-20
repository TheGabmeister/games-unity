using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemDataSO : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public ItemType type;
}

public enum ItemType
{
    Rupee,
    Arrow,
    Bomb
}