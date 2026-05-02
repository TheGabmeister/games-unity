using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Red Alert/Unit Data")]
public class UnitData : ScriptableObject
{
    public string DisplayName;
    public Sprite Sprite;
    public UnitCategory Category = UnitCategory.Vehicle;
    public Faction Faction = Faction.Allied;

    [Header("Movement")]
    public LocomotionType Locomotion;
    public float BaseSpeed = 5f;

    [Header("Combat")]
    public int MaxHP = 100;
    public ArmorType Armor = ArmorType.None;
    public WeaponData PrimaryWeapon;
    public float SightRange = 5f;

    [Header("Flags")]
    public bool IsCrusher;
    public bool NoMovingFire;
    public bool IsCrewedVehicle;
    public bool ExplodesOnDeath;
    public WarheadData DeathWarhead;

    [Header("Death")]
    public AudioClip DeathSound;
    public UnitData BailOutUnit;
}
