using System;
using UnityEngine;

public class CareSystem : MonoBehaviour
{
    [SerializeField] private int _hungerIncreasePerHour = 4;
    [SerializeField] private int _tirednessIncreasePerHour = 3;
    [SerializeField] private int _hungerCareThreshold = 80;
    [SerializeField] private int _tirednessCareThreshold = 80;
    [SerializeField] private int _sleepHour = 21;
    [SerializeField] private int _wakeHour = 6;
    [SerializeField] private int _tirednessRecoveryPerHourSleeping = 10;
    [SerializeField] private TimeSystem _timeSystem;

    private DigimonInstance _partner;
    private bool _hungerWarningActive;
    private bool _tirednessWarningActive;
    private bool _evolutionPending;
    private bool _deathPending;

    public event Action<DigimonSpeciesData> OnEvolutionReady;
    public event Action OnPartnerDied;

    private void Start()
    {
        _timeSystem.OnHourChanged += OnHourChanged;
    }

    private void OnDestroy()
    {
        if (_timeSystem != null)
            _timeSystem.OnHourChanged -= OnHourChanged;
    }

    private void OnHourChanged()
    {
        DigimonInstance partner = GetPartner();
        if (partner == null) return;

        partner.IncrementAge();

        if (CheckLifespan(partner)) return;
        CheckEvolution(partner);

        if (partner.IsSleeping)
        {
            HandleSleeping(partner);
            return;
        }

        partner.ModifyHunger(_hungerIncreasePerHour);
        partner.ModifyTiredness(_tirednessIncreasePerHour);

        CheckHunger(partner);
        CheckTiredness(partner);
        CheckSleepTime(partner);
    }

    private void HandleSleeping(DigimonInstance partner)
    {
        partner.ModifyTiredness(-_tirednessRecoveryPerHourSleeping);

        if (_timeSystem.Hour == _wakeHour)
        {
            partner.SetSleeping(false);
            partner.ModifyHappiness(5);
        }
    }

    private void CheckHunger(DigimonInstance partner)
    {
        if (partner.Hunger >= _hungerCareThreshold)
        {
            if (!_hungerWarningActive)
            {
                _hungerWarningActive = true;
                Debug.Log($"[CareSystem] {partner.Species.SpeciesName} is hungry!");
            }

            if (partner.Hunger >= 100)
            {
                partner.IncrementCareMistakes();
                partner.ModifyHappiness(-5);
                _hungerWarningActive = false;
                Debug.Log($"[CareSystem] {partner.Species.SpeciesName} care mistake: starving!");
            }
        }
        else
        {
            _hungerWarningActive = false;
        }
    }

    private void CheckTiredness(DigimonInstance partner)
    {
        if (partner.Tiredness >= _tirednessCareThreshold)
        {
            if (!_tirednessWarningActive)
            {
                _tirednessWarningActive = true;
                Debug.Log($"[CareSystem] {partner.Species.SpeciesName} is tired!");
            }

            if (partner.Tiredness >= 100)
            {
                partner.IncrementCareMistakes();
                partner.ModifyHappiness(-5);
                _tirednessWarningActive = false;
                Debug.Log($"[CareSystem] {partner.Species.SpeciesName} care mistake: exhausted!");
            }
        }
        else
        {
            _tirednessWarningActive = false;
        }
    }

    private void CheckSleepTime(DigimonInstance partner)
    {
        if (_timeSystem.Hour == _sleepHour)
        {
            partner.SetSleeping(true);
            Debug.Log($"[CareSystem] {partner.Species.SpeciesName} fell asleep.");
        }
    }

    public void Feed(int hungerReduction, int weightGain)
    {
        DigimonInstance partner = GetPartner();
        if (partner == null || partner.IsSleeping) return;

        partner.ModifyHunger(-hungerReduction);
        partner.ModifyWeight(weightGain);
        partner.ModifyHappiness(2);
        _hungerWarningActive = false;
    }

    public void Praise()
    {
        DigimonInstance partner = GetPartner();
        if (partner == null || partner.IsSleeping) return;

        partner.ModifyHappiness(5);
        partner.ModifyDiscipline(3);
    }

    public void Scold()
    {
        DigimonInstance partner = GetPartner();
        if (partner == null || partner.IsSleeping) return;

        partner.ModifyHappiness(-3);
        partner.ModifyDiscipline(5);
    }

    private void CheckEvolution(DigimonInstance partner)
    {
        if (_evolutionPending) return;

        EvolutionTable table = partner.Species.EvolutionTable;
        if (table == null) return;

        DigimonSpeciesData target = table.CheckEvolution(partner);
        if (target != null)
        {
            _evolutionPending = true;
            OnEvolutionReady?.Invoke(target);
        }
    }

    public void ClearEvolutionPending()
    {
        _evolutionPending = false;
    }

    private bool CheckLifespan(DigimonInstance partner)
    {
        if (_deathPending) return true;
        if (partner.Species.LifespanHours > 0 && partner.Age >= partner.Species.LifespanHours)
        {
            _deathPending = true;
            OnPartnerDied?.Invoke();
            return true;
        }
        return false;
    }

    public void ClearDeathPending()
    {
        _deathPending = false;
    }

    private DigimonInstance GetPartner()
    {
        if (_partner == null)
            _partner = FindFirstObjectByType<DigimonInstance>();
        return _partner;
    }
}
