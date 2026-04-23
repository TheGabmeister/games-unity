using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatusScreen : Singleton<StatusScreen>
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _identityText;
    [SerializeField] private TMP_Text _statsText;
    [SerializeField] private TMP_Text _conditionText;
    [SerializeField] private TMP_Text _instructionsText;

    private bool _isOpen;
    private DigimonInstance _partner;

    public bool IsOpen => _isOpen;

    protected override void Awake()
    {
        base.Awake();
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Update()
    {
        if (BattleSystem.Instance != null && BattleSystem.Instance.InBattle) return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (PauseScreen.Instance != null && PauseScreen.Instance.IsOpen) return;
            if (InventoryScreen.Instance != null && InventoryScreen.Instance.IsOpen) return;
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

            if (_isOpen)
                Close();
            else
                Open();
            return;
        }

        if (!_isOpen) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void Open()
    {
        _isOpen = true;
        _panel.SetActive(true);
        InputManager.Instance.SetPlayerInputEnabled(false);
        RefreshDisplay();
    }

    private void Close()
    {
        _isOpen = false;
        _panel.SetActive(false);
        InputManager.Instance.SetPlayerInputEnabled(true);
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
