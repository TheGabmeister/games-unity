using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnique", menuName = "DigimonWorld/TechniqueData")]
public class TechniqueData : ScriptableObject
{
    [SerializeField] private string _techniqueName;
    [SerializeField] private TechniqueCategory _category;
    [SerializeField] private int _mpCost;
    [SerializeField] private int _power;
    [SerializeField] private float _range;
    [SerializeField] private int _accuracy = 100;
    [SerializeField] private StatusEffectType _statusEffect = StatusEffectType.None;
    [SerializeField] private int _statusChance;

    public string TechniqueName => _techniqueName;
    public TechniqueCategory Category => _category;
    public int MpCost => _mpCost;
    public int Power => _power;
    public float Range => _range;
    public int Accuracy => _accuracy;
    public StatusEffectType StatusEffect => _statusEffect;
    public int StatusChance => _statusChance;
}
