using System;
using UnityEngine;

public class DigimonInstance : MonoBehaviour
{
    [SerializeField] private DigimonSpeciesData _species;

    private int _currentHP;
    private int _currentMP;
    private int _age;
    private int _weight;
    private int _hunger;
    private int _tiredness;
    private int _happiness;
    private int _discipline;
    private int _careMistakes;
    private int _virusGauge;
    private bool _isSleeping;

    public DigimonSpeciesData Species => _species;
    public int CurrentHP => _currentHP;
    public int CurrentMP => _currentMP;
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
        _age = 0;
        _weight = 15;
        _hunger = 0;
        _tiredness = 0;
        _happiness = 50;
        _discipline = 50;
        _careMistakes = 0;
        _virusGauge = 0;
        _isSleeping = false;
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

    public void Heal(int amount)
    {
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, _species.BaseHP);
    }

    public void RestoreMP(int amount)
    {
        _currentMP = Mathf.Clamp(_currentMP + amount, 0, _species.BaseMP);
    }
}
