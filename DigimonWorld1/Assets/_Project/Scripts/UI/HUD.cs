using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _dayText;
    [SerializeField] private TMP_Text _statsText;
    [SerializeField] private TimeSystem _timeSystem;

    private DigimonInstance _partner;
    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    public void SetVisible(bool visible)
    {
        if (_canvas != null)
            _canvas.enabled = visible;
    }

    private void Update()
    {
        _timeText.text = _timeSystem.TimeString;
        _dayText.text = $"Day {_timeSystem.Day}";

        DigimonInstance partner = GetPartner();
        if (partner == null) return;

        _statsText.text = partner.IsSleeping
            ? $"{partner.Species.SpeciesName}  [Sleeping]\nHP {partner.CurrentHP}  MP {partner.CurrentMP}"
            : $"{partner.Species.SpeciesName}\nHP {partner.CurrentHP}  MP {partner.CurrentMP}\n" +
              $"Hunger {partner.Hunger}  Tired {partner.Tiredness}\n" +
              $"Happy {partner.Happiness}  Disc {partner.Discipline}";
    }

    private DigimonInstance GetPartner()
    {
        if (_partner == null)
            _partner = FindFirstObjectByType<DigimonInstance>();
        return _partner;
    }
}
