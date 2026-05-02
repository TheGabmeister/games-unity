using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Red Alert/Unit Data")]
public class UnitData : ScriptableObject
{
    public string DisplayName;
    public Sprite Sprite;
    public UnitCategory Category = UnitCategory.Vehicle;
    public Faction Faction = Faction.Allied;
    public int Cost;

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

    [Header("Building")]
    public int FootprintX = 1;
    public int FootprintY = 1;
    public int StorageCapacity;
    public bool RequiresPower;
    public UnitData FreeUnit;

    [Header("Prefab")]
    public GameObject Prefab;
}
