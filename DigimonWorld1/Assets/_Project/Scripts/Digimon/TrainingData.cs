using UnityEngine;

[CreateAssetMenu(fileName = "NewTraining", menuName = "DigimonWorld/TrainingData")]
public class TrainingData : ScriptableObject
{
    [SerializeField] private string _facilityName;
    [SerializeField] private TrainableStat _stat;
    [SerializeField] private int _statGainMin;
    [SerializeField] private int _statGainMax;
    [SerializeField] private int _tirednessCost;
    [SerializeField] private int _happinessCost;

    public string FacilityName => _facilityName;
    public TrainableStat Stat => _stat;
    public int StatGainMin => _statGainMin;
    public int StatGainMax => _statGainMax;
    public int TirednessCost => _tirednessCost;
    public int HappinessCost => _happinessCost;
}
