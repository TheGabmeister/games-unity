using UnityEngine;

[CreateAssetMenu(fileName = "NewWarhead", menuName = "Red Alert/Warhead Data")]
public class WarheadData : ScriptableObject
{
    public string DisplayName;

    [Header("Armor Modifiers")]
    public float ModNone = 1f;
    public float ModWood = 1f;
    public float ModLight = 1f;
    public float ModHeavy = 1f;
    public float ModConcrete = 1f;

    [Header("Splash")]
    public int SpreadFactor = 3;
    public bool WallDestroyer;

    [Header("Sounds")]
    public AudioClip ImpactSound;

    public float GetModifier(ArmorType armor)
    {
        return armor switch
        {
            ArmorType.None => ModNone,
            ArmorType.Wood => ModWood,
            ArmorType.Light => ModLight,
            ArmorType.Heavy => ModHeavy,
            ArmorType.Concrete => ModConcrete,
            _ => 1f
        };
    }
}
