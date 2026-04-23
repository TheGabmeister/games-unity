using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounter", menuName = "DigimonWorld/EncounterData")]
public class EncounterData : ScriptableObject
{
    [SerializeField] private DigimonSpeciesData _species;
    [SerializeField] private float _statScale = 1f;
    [SerializeField] private int _bitReward = 100;

    public DigimonSpeciesData Species => _species;
    public float StatScale => _statScale;
    public int BitReward => _bitReward;
}
