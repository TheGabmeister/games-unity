using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnique", menuName = "DigimonWorld/TechniqueData")]
public class TechniqueData : ScriptableObject
{
    [SerializeField] private string _techniqueName;
    [SerializeField] private TechniqueCategory _category;
    [SerializeField] private int _mpCost;
    [SerializeField] private int _power;
    [SerializeField] private float _range;

    public string TechniqueName => _techniqueName;
    public TechniqueCategory Category => _category;
    public int MpCost => _mpCost;
    public int Power => _power;
    public float Range => _range;
}
