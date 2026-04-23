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
    }
}
