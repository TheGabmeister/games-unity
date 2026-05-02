using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Red Alert/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string DisplayName;
    public int Damage = 10;
    public float Range = 4f;
    public float ROF = 1f;
    public int Burst = 1;
    public ProjectileData Projectile;
    public WarheadData Warhead;
    public AudioClip FireSound;
}
