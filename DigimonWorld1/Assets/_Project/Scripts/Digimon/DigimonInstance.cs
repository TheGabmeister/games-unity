using System;
using System.Collections.Generic;
using UnityEngine;

public class DigimonInstance : MonoBehaviour
{
    [SerializeField] private DigimonSpeciesData _species;

    private int _currentHP;
    private int _currentMP;
    private int _bonusOffense;
    private int _bonusDefense;
    private int _bonusSpeed;
    private int _bonusBrains;
    private int _age;
    private int _weight;
    private int _hunger;
    private int _tiredness;
    private int _happiness;
    private int _discipline;
    private int _careMistakes;
    private int _virusGauge;
    private bool _isSleeping;
    private List<TechniqueData> _knownTechniques = new List<TechniqueData>();
    private int _totalLives;
    private int _inheritedBonusOffense;
    private int _inheritedBonusDefense;
    private int _inheritedBonusSpeed;
    private int _inheritedBonusBrains;

    public DigimonSpeciesData Species => _species;
    public int TotalLives => _totalLives;
    public IReadOnlyList<TechniqueData> KnownTechniques => _knownTechniques;
    public bool IsAlive => _currentHP > 0;
    public int CurrentHP => _currentHP;
    public int CurrentMP => _currentMP;
    public int MaxHP => _species != null ? _species.BaseHP + _bonusOffense : _currentHP;
    public int MaxMP => _species != null ? _species.BaseMP + _bonusBrains : _currentMP;
    public int Offense => (_species != null ? _species.BaseOffense : 0) + _bonusOffense;
    public int Defense => (_species != null ? _species.BaseDefense : 0) + _bonusDefense;
    public int Speed => (_species != null ? _species.BaseSpeed : 0) + _bonusSpeed;
    public int Brains => (_species != null ? _species.BaseBrains : 0) + _bonusBrains;
    public int BonusOffense => _bonusOffense;
    public int BonusDefense => _bonusDefense;
    public int BonusSpeed => _bonusSpeed;
    public int BonusBrains => _bonusBrains;
    public int Age => _age;
    public int Weight => _weight;
    public int Hunger => _hunger;
    public int Tiredness => _tiredness;
    public int Happiness => _happiness;
    public int Discipline => _discipline;
    public int CareMistakes => _careMistakes;
    public int VirusGauge => _virusGauge;
    public bool IsSleeping => _isSleeping;

    public event Action OnCareMistake;

    private void Awake()
    {
        if (_species != null)
            InitializeFromSpecies(_species);
    }

    public void InitializeFromSpecies(DigimonSpeciesData species)
    {
        _species = species;
        _currentHP = species.BaseHP;
        _currentMP = species.BaseMP;
        _bonusOffense = 0;
        _bonusDefense = 0;
        _bonusSpeed = 0;
        _bonusBrains = 0;
        _age = 0;
        _weight = 15;
        _hunger = 0;
        _tiredness = 0;
        _happiness = 50;
        _discipline = 50;
        _careMistakes = 0;
        _virusGauge = 0;
        _isSleeping = false;

        _knownTechniques.Clear();
        if (species.LearnableTechniques != null)
        {
            int count = Mathf.Min(3, species.LearnableTechniques.Length);
            for (int i = 0; i < count; i++)
            {
                if (species.LearnableTechniques[i] != null)
                    _knownTechniques.Add(species.LearnableTechniques[i]);
            }
        }
    }

    public void ModifyHunger(int amount)
    {
        _hunger = Mathf.Clamp(_hunger + amount, 0, 100);
    }

    public void ModifyTiredness(int amount)
    {
        _tiredness = Mathf.Clamp(_tiredness + amount, 0, 100);
    }

    public void ModifyHappiness(int amount)
    {
        _happiness = Mathf.Clamp(_happiness + amount, 0, 100);
    }

    public void ModifyDiscipline(int amount)
    {
        _discipline = Mathf.Clamp(_discipline + amount, 0, 100);
    }

    public void ModifyWeight(int amount)
    {
        _weight = Mathf.Max(1, _weight + amount);
    }

    public void IncrementCareMistakes()
    {
        _careMistakes++;
        OnCareMistake?.Invoke();
    }

    public void IncrementVirusGauge()
    {
        _virusGauge = Mathf.Min(_virusGauge + 1, 16);
    }

    public void IncrementAge()
    {
        _age++;
    }

    public void SetSleeping(bool sleeping)
    {
        _isSleeping = sleeping;
    }

    public void TrainStat(TrainableStat stat, int amount)
    {
        switch (stat)
        {
            case TrainableStat.HP:
                _currentHP = Mathf.Max(0, _currentHP + amount);
                break;
            case TrainableStat.MP:
                _currentMP = Mathf.Max(0, _currentMP + amount);
                break;
            case TrainableStat.Offense:
                _bonusOffense += amount;
                break;
            case TrainableStat.Defense:
                _bonusDefense += amount;
                break;
            case TrainableStat.Speed:
                _bonusSpeed += amount;
                break;
            case TrainableStat.Brains:
                _bonusBrains += amount;
                break;
        }
    }

    public void Heal(int amount)
    {
        if (_species == null) return;
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, MaxHP);
    }

    public void RestoreMP(int amount)
    {
        if (_species == null) return;
        _currentMP = Mathf.Clamp(_currentMP + amount, 0, MaxMP);
    }

    public void TakeDamage(int amount)
    {
        _currentHP = Mathf.Max(0, _currentHP - amount);
    }

    public bool SpendMP(int amount)
    {
        if (_currentMP < amount) return false;
        _currentMP -= amount;
        return true;
    }

    public void LearnTechnique(TechniqueData technique)
    {
        if (technique == null || _knownTechniques.Contains(technique)) return;
        if (_knownTechniques.Count >= 4) return;
        _knownTechniques.Add(technique);
    }

    public void Evolve(DigimonSpeciesData newSpecies)
    {
        _species = newSpecies;
        _currentHP = newSpecies.BaseHP + _bonusOffense;
        _currentMP = newSpecies.BaseMP + _bonusBrains;

        _knownTechniques.Clear();
        if (newSpecies.LearnableTechniques != null)
        {
            int count = Mathf.Min(3, newSpecies.LearnableTechniques.Length);
            for (int i = 0; i < count; i++)
            {
                if (newSpecies.LearnableTechniques[i] != null)
                    _knownTechniques.Add(newSpecies.LearnableTechniques[i]);
            }
        }
    }

    public DigimonInheritance Die()
    {
        return new DigimonInheritance
        {
            BonusOffense = _bonusOffense,
            BonusDefense = _bonusDefense,
            BonusSpeed = _bonusSpeed,
            BonusBrains = _bonusBrains,
            TotalLives = _totalLives + 1
        };
    }

    public void Reincarnate(DigimonSpeciesData freshSpecies, DigimonInheritance inheritance)
    {
        InitializeFromSpecies(freshSpecies);
        _totalLives = inheritance.TotalLives;
        _inheritedBonusOffense = inheritance.BonusOffense;
        _inheritedBonusDefense = inheritance.BonusDefense;
        _inheritedBonusSpeed = inheritance.BonusSpeed;
        _inheritedBonusBrains = inheritance.BonusBrains;
        _bonusOffense = _inheritedBonusOffense / 4;
        _bonusDefense = _inheritedBonusDefense / 4;
        _bonusSpeed = _inheritedBonusSpeed / 4;
        _bonusBrains = _inheritedBonusBrains / 4;
    }
}
