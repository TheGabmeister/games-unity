using UnityEngine;

public class CareSystem : Singleton<CareSystem>
{
    [SerializeField] private int _hungerIncreasePerHour = 4;
    [SerializeField] private int _tirednessIncreasePerHour = 3;
    [SerializeField] private int _hungerCareThreshold = 80;
    [SerializeField] private int _tirednessCareThreshold = 80;
    [SerializeField] private int _sleepHour = 21;
    [SerializeField] private int _wakeHour = 6;
    [SerializeField] private int _tirednessRecoveryPerHourSleeping = 10;

    private DigimonInstance _partner;
    private bool _hungerWarningActive;
    private bool _tirednessWarningActive;

    private void Start()
    {
        TimeSystem.Instance.OnHourChanged += OnHourChanged;
    }

    protected override void OnDestroy()
    {
        var timeSystem = FindFirstObjectByType<TimeSystem>();
        if (timeSystem != null)
            timeSystem.OnHourChanged -= OnHourChanged;
        base.OnDestroy();
    }

    private void OnHourChanged()
    {
        DigimonInstance partner = GetPartner();
        if (partner == null) return;

        partner.IncrementAge();

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

        if (TimeSystem.Instance.Hour == _wakeHour)
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
        if (TimeSystem.Instance.Hour == _sleepHour)
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

    private DigimonInstance GetPartner()
    {
        if (_partner == null)
            _partner = FindFirstObjectByType<DigimonInstance>();
        return _partner;
    }
}
