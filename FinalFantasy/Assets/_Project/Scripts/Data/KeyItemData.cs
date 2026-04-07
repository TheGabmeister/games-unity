using UnityEngine;

[CreateAssetMenu(menuName = "FF1/Key Item")]
public class KeyItemData : ScriptableObject
{
    public string ItemName;
    public string Description;
    public string GrantsFlag;
    public bool ConsumedOnUse;
}
