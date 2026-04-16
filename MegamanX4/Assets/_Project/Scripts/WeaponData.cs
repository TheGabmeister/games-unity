using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "MegamanX4/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string displayName;
    public Color tint = Color.white;
    public GameObject smallPrefab;
    public GameObject semiPrefab;
    public GameObject fullPrefab;
}
