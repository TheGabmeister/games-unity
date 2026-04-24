using TMPro;
using UnityEngine;

public class StatusScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _identityText;
    [SerializeField] private TMP_Text _statsText;
    [SerializeField] private TMP_Text _conditionText;
    [SerializeField] private TMP_Text _instructionsText;

    private bool _isOpen;
    private DigimonInstance _partner;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    public void Open()
    {
        _isOpen = true;
        _panel.SetActive(true);
        RefreshDisplay();
    }

    public void Close()
    {
        _isOpen = false;
        _panel.SetActive(false);
    }

    private void RefreshDisplay()
    {
        DigimonInstance partner = GetPartner();
        if (partner == null || partner.Species == null)
        {
            _identityText.text = "No partner";
            _statsText.text = "";
            _conditionText.text = "";
            _instructionsText.text = "C / ESC: Close";
            return;
        }

        DigimonSpeciesData species = partner.Species;

        _identityText.text = $"{species.SpeciesName}\n" +
            $"Stage: {species.Stage}    Attribute: {species.Attribute}";

        _statsText.text =
            $"HP    {partner.CurrentHP} / {partner.MaxHP}\n" +
            $"MP    {partner.CurrentMP} / {partner.MaxMP}\n" +
            $"OFF   {partner.Offense}" + (partner.BonusOffense > 0 ? $" (+{partner.BonusOffense})" : "") + "\n" +
            $"DEF   {partner.Defense}" + (partner.BonusDefense > 0 ? $" (+{partner.BonusDefense})" : "") + "\n" +
            $"SPD   {partner.Speed}" + (partner.BonusSpeed > 0 ? $" (+{partner.BonusSpeed})" : "") + "\n" +
            $"BRN   {partner.Brains}" + (partner.BonusBrains > 0 ? $" (+{partner.BonusBrains})" : "");

        _conditionText.text =
            $"Age: {partner.Age} hrs    Weight: {partner.Weight} g\n" +
            $"Hunger: {partner.Hunger}    Tiredness: {partner.Tiredness}\n" +
            $"Happiness: {partner.Happiness}    Discipline: {partner.Discipline}\n" +
            $"Care Mistakes: {partner.CareMistakes}    Virus: {partner.VirusGauge}\n" +
            $"Lifespan: {species.LifespanHours} hrs" +
            (partner.IsSleeping ? "\n[Sleeping]" : "");

        _instructionsText.text = "C / ESC: Close";
    }

    private DigimonInstance GetPartner()
    {
        if (_partner == null)
            _partner = FindFirstObjectByType<DigimonInstance>();
        return _partner;
    }
}
