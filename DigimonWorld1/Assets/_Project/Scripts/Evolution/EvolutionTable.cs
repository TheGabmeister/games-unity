using UnityEngine;

[CreateAssetMenu(fileName = "NewEvolutionTable", menuName = "DigimonWorld/EvolutionTable")]
public class EvolutionTable : ScriptableObject
{
    [SerializeField] private DigimonSpeciesData _fromSpecies;
    [SerializeField] private EvolutionRequirement[] _requirements;

    public DigimonSpeciesData FromSpecies => _fromSpecies;
    public EvolutionRequirement[] Requirements => _requirements;

    public DigimonSpeciesData CheckEvolution(DigimonInstance partner)
    {
        if (_requirements == null) return null;

        foreach (var req in _requirements)
        {
            if (req.IsMet(partner))
                return req.TargetSpecies;
        }

        return null;
    }
}
