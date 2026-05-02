using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Red Alert/Unit Data")]
public class UnitData : ScriptableObject
{
    public string DisplayName;
    public Sprite Sprite;
    public LocomotionType Locomotion;
    public float BaseSpeed = 5f;
}
