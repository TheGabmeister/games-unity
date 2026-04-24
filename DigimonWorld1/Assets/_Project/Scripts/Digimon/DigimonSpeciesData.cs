using UnityEngine;

[CreateAssetMenu(fileName = "NewSpecies", menuName = "DigimonWorld/DigimonSpeciesData")]
public class DigimonSpeciesData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _speciesName;
    [SerializeField] private DigimonStage _stage;
    [SerializeField] private DigimonAttribute _attribute;

    [Header("Base Stats")]
    [SerializeField] private int _baseHP;
    [SerializeField] private int _baseMP;
    [SerializeField] private int _baseOffense;
    [SerializeField] private int _baseDefense;
    [SerializeField] private int _baseSpeed;
    [SerializeField] private int _baseBrains;

    [Header("Lifespan")]
    [SerializeField] private int _lifespanHours;

    [Header("Evolution")]
    [SerializeField] private EvolutionTable _evolutionTable;

    [Header("Techniques")]
    [SerializeField] private TechniqueData[] _learnableTechniques;

    public string SpeciesName => _speciesName;
    public DigimonStage Stage => _stage;
    public DigimonAttribute Attribute => _attribute;
    public int BaseHP => _baseHP;
    public int BaseMP => _baseMP;
    public int BaseOffense => _baseOffense;
    public int BaseDefense => _baseDefense;
    public int BaseSpeed => _baseSpeed;
    public int BaseBrains => _baseBrains;
    public int LifespanHours => _lifespanHours;
    public EvolutionTable EvolutionTable => _evolutionTable;
    public TechniqueData[] LearnableTechniques => _learnableTechniques;
}
