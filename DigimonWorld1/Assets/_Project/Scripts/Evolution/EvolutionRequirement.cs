using System;
using UnityEngine;

[Serializable]
public struct EvolutionRequirement
{
    [SerializeField] private DigimonSpeciesData _targetSpecies;
    [SerializeField] private int _minAge;
    [SerializeField] private int _minHP;
    [SerializeField] private int _minMP;
    [SerializeField] private int _minOffense;
    [SerializeField] private int _minDefense;
    [SerializeField] private int _minSpeed;
    [SerializeField] private int _minBrains;
    [SerializeField] private int _maxCareMistakes;
    [SerializeField] private int _minWeight;
    [SerializeField] private int _maxWeight;
    [SerializeField] private int _minHappiness;
    [SerializeField] private int _minDiscipline;
    [SerializeField] private string _bonusCondition;

    public DigimonSpeciesData TargetSpecies => _targetSpecies;
    public string BonusCondition => _bonusCondition;

    public bool IsMet(DigimonInstance partner)
    {
        if (partner.Age < _minAge) return false;
        if (_minHP > 0 && partner.MaxHP < _minHP) return false;
        if (_minMP > 0 && partner.MaxMP < _minMP) return false;
        if (_minOffense > 0 && partner.Offense < _minOffense) return false;
        if (_minDefense > 0 && partner.Defense < _minDefense) return false;
        if (_minSpeed > 0 && partner.Speed < _minSpeed) return false;
        if (_minBrains > 0 && partner.Brains < _minBrains) return false;
        if (_maxCareMistakes >= 0 && partner.CareMistakes > _maxCareMistakes) return false;
        if (_minWeight > 0 && partner.Weight < _minWeight) return false;
        if (_maxWeight > 0 && partner.Weight > _maxWeight) return false;
        if (_minHappiness > 0 && partner.Happiness < _minHappiness) return false;
        if (_minDiscipline > 0 && partner.Discipline < _minDiscipline) return false;
        return true;
    }
}
